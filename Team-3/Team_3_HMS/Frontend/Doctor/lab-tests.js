
let selectedLabTestId = null;
let allLabTests = [];


function initDoctorProfile() {
    try {
        const raw = localStorage.getItem('user') || sessionStorage.getItem('user');
        let user = raw ? JSON.parse(raw) : {};
        if (!user.fullname) {
            const token = localStorage.getItem('token') || sessionStorage.getItem('token') || '';
            if (token && token.includes('.')) {
                const payload = JSON.parse(atob(token.split('.')[1]));
                user.fullname = payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'] || payload.unique_name || payload.name || '';
                user.userId = payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'] || payload.nameid || '';
                user.role = payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || payload.role || 'Doctor';
            }
        }
        if (user && user.fullname) {
            document.querySelectorAll('.sidebar-user-name, .topbar-user-label').forEach(el => el.textContent = user.fullname);
        }
        if (user && user.userId) {
            const roleEl = document.querySelector('.sidebar-user-role');
            if (roleEl) roleEl.textContent = `Doctor · ID ${user.userId}`;
        }
    } catch (e) {}
}

document.addEventListener("DOMContentLoaded", async function () {

    initDoctorProfile();
    setupSearch();
    setupCategoryFilter();
    setupCreateForm();
    setupEditForm();
    setupTableActions();
    setupUpdateResultButton();
    setupDetailsUpdateResultButton();
    setupDeleteButton();

    await loadLabTests();
    await loadLabTestSummary();

});


// GET: Load All Lab Tests


async function loadLabTests() {

    const tableBody =
        document.getElementById("labTestsTableBody");

    const countText =
        document.getElementById("labTestsCount");

    try {

        tableBody.innerHTML = `
        <tr>
            <td colspan="8" class="text-center py-4">
                Loading lab tests...
            </td>
        </tr>
    `;

        const labTests = await getLabTests();

        allLabTests = Array.isArray(labTests)
            ? labTests
            : [];

        renderLabTests(allLabTests);

    }
    catch (error) {

        console.error(
            "Error loading lab tests:",
            error
        );

        tableBody.innerHTML = `
        <tr>
            <td colspan="8" class="text-center py-4">
                Unable to load lab tests.
            </td>
        </tr>
    `;

        countText.textContent =
            "Unable to load lab tests.";
    }

}


// Render Lab Tests Table


function renderLabTests(labTests) {

    const tableBody =
        document.getElementById("labTestsTableBody");

    const countText =
        document.getElementById("labTestsCount");

    tableBody.innerHTML = "";


    if (!labTests || labTests.length === 0) {

        tableBody.innerHTML = `
        <tr>
            <td colspan="8" class="text-center py-4">
                No lab tests found.
            </td>
        </tr>
    `;

        countText.textContent =
            "0 lab tests";

        return;
    }


    labTests.forEach(function (labTest) {

        const labTestId =
            getLabTestId(labTest);

        const testName =
            labTest.testName ??
            labTest.TestName ??
            "-";

        const category =
            labTest.category ??
            labTest.Category ??
            "-";

        const testDate =
            labTest.testDate ??
            labTest.TestDate ??
            "-";

        const cost =
            labTest.cost ??
            labTest.Cost ??
            0;

        const result =
            labTest.result ??
            labTest.Result ??
            "-";

        const medicalRecordId =
            labTest.medicalRecordId ??
            labTest.MedicalRecordId ??
            "-";


        const row =
            document.createElement("tr");


        row.innerHTML = `
        <td>
            <strong>
                #LT-${escapeHtml(labTestId)}
            </strong>
        </td>

        <td>
            ${escapeHtml(testName)}
        </td>

        <td>
            <span class="badge badge-blue">
                ${escapeHtml(category)}
            </span>
        </td>

        <td>
            ${escapeHtml(testDate)}
        </td>

        <td>
            OMR ${formatCost(cost)}
        </td>

        <td>
            ${escapeHtml(
            result || "Pending"
        )}
        </td>

        <td>
            ${medicalRecordId === "-"
                ? "-"
                : `#MR-${escapeHtml(medicalRecordId)}`
            }
        </td>

        <td>
            <div class="d-flex gap-1">

                <button
                    type="button"
                    class="btn-ghost view-lab-btn"
                    data-lab-id="${labTestId}">
                    View
                </button>

                <button
                    type="button"
                    class="btn-ghost edit-lab-btn"
                    data-lab-id="${labTestId}">
                    Edit
                </button>

                <button
                    type="button"
                    class="btn-ghost result-lab-btn"
                    data-lab-id="${labTestId}">
                    Result
                </button>

                <button
                    type="button"
                    class="btn-ghost delete-lab-btn"
                    data-lab-id="${labTestId}">
                    Delete
                </button>

            </div>
        </td>
    `;


        tableBody.appendChild(row);
    });


    countText.textContent =
        `${labTests.length} lab test${labTests.length === 1 ? "" : "s"
        }`;

}

// GET: Summary


async function loadLabTestSummary() {

    try {

        const summary =
            await getLabTestSummary();


        const totalLabTests =
            summary.totalLabTests ??
            summary.TotalLabTests ??
            0;


        const totalCost =
            summary.totalCost ??
            summary.TotalCost ??
            0;


        document
            .getElementById("totalLabTests")
            .textContent =
            totalLabTests;


        document
            .getElementById("totalLabTestCost")
            .textContent =
            `OMR ${formatCost(totalCost)}`;

    }
    catch (error) {

        console.error(
            "Error loading lab test summary:",
            error
        );


        document
            .getElementById("totalLabTests")
            .textContent =
            "0";


        document
            .getElementById("totalLabTestCost")
            .textContent =
            "OMR 0.000";
    }

}


// Search By Test Name


function setupSearch() {

    const searchInput =
        document.getElementById(
            "labTestSearch"
        );


    searchInput.addEventListener(
        "input",
        function () {

            applyFilters();

        }
    );

}


// Filter By Category


function setupCategoryFilter() {

    const categoryFilter =
        document.getElementById(
            "categoryFilter"
        );


    categoryFilter.addEventListener(
        "change",
        async function () {

            const category =
                categoryFilter.value.trim();


            if (category === "") {

                await loadLabTests();

                applyFilters();

                return;
            }


            try {

                const labTests =
                    await filterLabTests(
                        category
                    );


                allLabTests =
                    Array.isArray(labTests)
                        ? labTests
                        : [];


                applyFilters();

            }
            catch (error) {

                console.error(
                    "Category filter error:",
                    error
                );


                allLabTests = [];

                renderLabTests([]);
            }
        }
    );

}


// Apply Search


function applyFilters() {

    const searchValue =
        document
            .getElementById("labTestSearch")
            .value
            .trim()
            .toLowerCase();


    let filteredTests =
        [...allLabTests];


    if (searchValue !== "") {

        filteredTests =
            filteredTests.filter(
                function (labTest) {

                    const testName =
                        labTest.testName ??
                        labTest.TestName ??
                        "";

                    return String(testName)
                        .toLowerCase()
                        .includes(searchValue);
                }
            );
    }


    renderLabTests(filteredTests);

}


// POST: Create Lab Test


function setupCreateForm() {

    const form =
        document.getElementById(
            "createLabTestForm"
        );


    form.addEventListener(
        "submit",
        async function (event) {

            event.preventDefault();


            const labTest = {

                labTestId: 0,

                testName:
                    document
                        .getElementById(
                            "createLabTestName"
                        )
                        .value
                        .trim(),

                category:
                    document
                        .getElementById(
                            "createLabCategory"
                        )
                        .value,

                testDate:
                    document
                        .getElementById(
                            "createLabTestDate"
                        )
                        .value,

                cost:
                    Number(
                        document
                            .getElementById(
                                "createLabCost"
                            )
                            .value
                    ),

                result:
                    document
                        .getElementById(
                            "createLabResult"
                        )
                        .value
                        .trim(),

                medicalRecordId:
                    Number(
                        document
                            .getElementById(
                                "createLabMedicalRecordId"
                            )
                            .value
                    )
            };


            try {

                await createLabTest(
                    labTest
                );


                bootstrap.Modal
                    .getOrCreateInstance(
                        document.getElementById(
                            "orderLabModal"
                        )
                    )
                    .hide();


                form.reset();


                await refreshLabTests();


                alert(
                    "Lab test created successfully."
                );

            }
            catch (error) {

                console.error(
                    "Create lab test error:",
                    error
                );


                alert(
                    error.message ||
                    "Unable to create lab test."
                );
            }
        }
    );

}


// Table Buttons


function setupTableActions() {

    const tableBody =
        document.getElementById(
            "labTestsTableBody"
        );


    tableBody.addEventListener(
        "click",
        async function (event) {

            const viewButton =
                event.target.closest(
                    ".view-lab-btn"
                );

            const editButton =
                event.target.closest(
                    ".edit-lab-btn"
                );

            const resultButton =
                event.target.closest(
                    ".result-lab-btn"
                );

            const deleteButton =
                event.target.closest(
                    ".delete-lab-btn"
                );


            if (viewButton) {

                const id =
                    Number(
                        viewButton.dataset.labId
                    );

                await showLabTestDetails(id);

                return;
            }


            if (editButton) {

                const id =
                    Number(
                        editButton.dataset.labId
                    );

                await openEditLabTestModal(id);

                return;
            }


            if (resultButton) {

                const id =
                    Number(
                        resultButton.dataset.labId
                    );

                await openUpdateResultModal(id);

                return;
            }


            if (deleteButton) {

                const id =
                    Number(
                        deleteButton.dataset.labId
                    );

                openDeleteLabTestModal(id);
            }
        }
    );

}


// GET BY ID: Lab Test Details


async function showLabTestDetails(id) {

    try {

        const labTest =
            await getLabTestById(id);


        selectedLabTestId =
            getLabTestId(labTest);


        const testName =
            labTest.testName ??
            labTest.TestName ??
            "-";

        const category =
            labTest.category ??
            labTest.Category ??
            "-";

        const testDate =
            labTest.testDate ??
            labTest.TestDate ??
            "-";

        const cost =
            labTest.cost ??
            labTest.Cost ??
            0;

        const result =
            labTest.result ??
            labTest.Result ??
            "-";

        const medicalRecordId =
            labTest.medicalRecordId ??
            labTest.MedicalRecordId ??
            "-";


        document
            .getElementById(
                "detailsLabTestTitle"
            )
            .textContent =
            `#LT-${selectedLabTestId} — ${testName}`;


        document
            .getElementById(
                "detailsTestName"
            )
            .textContent =
            testName;


        document
            .getElementById(
                "detailsCategory"
            )
            .textContent =
            category;


        document
            .getElementById(
                "detailsTestDate"
            )
            .textContent =
            testDate;


        document
            .getElementById(
                "detailsCost"
            )
            .textContent =
            `OMR ${formatCost(cost)}`;


        document
            .getElementById(
                "detailsMedicalRecordId"
            )
            .textContent =
            medicalRecordId === "-"
                ? "-"
                : `#MR-${medicalRecordId}`;


        document
            .getElementById(
                "detailsResult"
            )
            .textContent =
            result || "Pending";


        const offcanvas =
            document.getElementById(
                "labTestDetails"
            );


        bootstrap.Offcanvas
            .getOrCreateInstance(
                offcanvas
            )
            .show();

    }
    catch (error) {

        console.error(
            "Load lab test details error:",
            error
        );


        alert(
            error.message ||
            "Unable to load lab test details."
        );
    }

}


// Details → Update Result


function setupDetailsUpdateResultButton() {

    const button =
        document.getElementById(
            "detailsUpdateResultBtn"
        );


    button.addEventListener(
        "click",
        async function () {

            if (!selectedLabTestId) {
                return;
            }


            const offcanvas =
                document.getElementById(
                    "labTestDetails"
                );


            bootstrap.Offcanvas
                .getOrCreateInstance(
                    offcanvas
                )
                .hide();


            await openUpdateResultModal(
                selectedLabTestId
            );
        }
    );

}

// Open Edit Lab Test Modal


async function openEditLabTestModal(id) {

    try {

        const labTest =
            await getLabTestById(id);


        const labTestId =
            getLabTestId(labTest);


        document
            .getElementById(
                "editLabTestId"
            )
            .value =
            labTestId;


        document
            .getElementById(
                "editLabTestLabel"
            )
            .textContent =
            `Lab Test #LT-${labTestId}`;


        document
            .getElementById(
                "editLabMedicalRecordId"
            )
            .value =
            labTest.medicalRecordId ??
            labTest.MedicalRecordId ??
            "";


        document
            .getElementById(
                "editLabTestName"
            )
            .value =
            labTest.testName ??
            labTest.TestName ??
            "";


        document
            .getElementById(
                "editLabCategory"
            )
            .value =
            labTest.category ??
            labTest.Category ??
            "Blood Test";


        document
            .getElementById(
                "editLabTestDate"
            )
            .value =
            toDateInputValue(
                labTest.testDate ??
                labTest.TestDate ??
                ""
            );


        document
            .getElementById(
                "editLabCost"
            )
            .value =
            labTest.cost ??
            labTest.Cost ??
            0;


        document
            .getElementById(
                "editLabResult"
            )
            .value =
            labTest.result ??
            labTest.Result ??
            "";


        bootstrap.Modal
            .getOrCreateInstance(
                document.getElementById(
                    "editLabTestModal"
                )
            )
            .show();

    }
    catch (error) {

        console.error(
            "Open edit lab test error:",
            error
        );


        alert(
            error.message ||
            "Unable to load lab test."
        );
    }

}


// PUT: Update Lab Test


function setupEditForm() {

    const form =
        document.getElementById(
            "editLabTestForm"
        );


    form.addEventListener(
        "submit",
        async function (event) {

            event.preventDefault();


            const id =
                Number(
                    document
                        .getElementById(
                            "editLabTestId"
                        )
                        .value
                );


            const labTest = {

                labTestId: id,

                medicalRecordId:
                    Number(
                        document
                            .getElementById(
                                "editLabMedicalRecordId"
                            )
                            .value
                    ),

                testName:
                    document
                        .getElementById(
                            "editLabTestName"
                        )
                        .value
                        .trim(),

                category:
                    document
                        .getElementById(
                            "editLabCategory"
                        )
                        .value,

                testDate:
                    document
                        .getElementById(
                            "editLabTestDate"
                        )
                        .value,

                cost:
                    Number(
                        document
                            .getElementById(
                                "editLabCost"
                            )
                            .value
                    ),

                result:
                    document
                        .getElementById(
                            "editLabResult"
                        )
                        .value
                        .trim()
            };


            try {

                await updateLabTest(
                    id,
                    labTest
                );


                bootstrap.Modal
                    .getOrCreateInstance(
                        document.getElementById(
                            "editLabTestModal"
                        )
                    )
                    .hide();


                await refreshLabTests();


                alert(
                    "Lab test updated successfully."
                );

            }
            catch (error) {

                console.error(
                    "Update lab test error:",
                    error
                );


                alert(
                    error.message ||
                    "Unable to update lab test."
                );
            }
        }
    );

}


// Open Result Modal


async function openUpdateResultModal(id) {

    try {

        const labTest =
            await getLabTestById(id);


        const labTestId =
            getLabTestId(labTest);


        document
            .getElementById(
                "updateLabTestId"
            )
            .value =
            labTestId;


        document
            .getElementById(
                "updateLabTestLabel"
            )
            .textContent =
            `Lab Test #LT-${labTestId}`;


        document
            .getElementById(
                "updateLabTestName"
            )
            .value =
            labTest.testName ??
            labTest.TestName ??
            "";


        document
            .getElementById(
                "updateLabTestResult"
            )
            .value =
            labTest.result ??
            labTest.Result ??
            "";


        bootstrap.Modal
            .getOrCreateInstance(
                document.getElementById(
                    "updateResultModal"
                )
            )
            .show();

    }
    catch (error) {

        console.error(
            "Open result modal error:",
            error
        );


        alert(
            error.message ||
            "Unable to load lab test."
        );
    }

}


// PATCH: Update Result


function setupUpdateResultButton() {

    const button =
        document.getElementById(
            "updateLabResultBtn"
        );


    button.addEventListener(
        "click",
        async function () {

            const id =
                Number(
                    document
                        .getElementById(
                            "updateLabTestId"
                        )
                        .value
                );


            const result =
                document
                    .getElementById(
                        "updateLabTestResult"
                    )
                    .value
                    .trim();


            if (result === "") {

                alert(
                    "Please enter the lab test result."
                );

                return;
            }


            try {

                await updateLabTestResult(
                    id,
                    result
                );


                bootstrap.Modal
                    .getOrCreateInstance(
                        document.getElementById(
                            "updateResultModal"
                        )
                    )
                    .hide();


                await refreshLabTests();


                alert(
                    "Lab test result updated successfully."
                );


                if (
                    selectedLabTestId &&
                    Number(selectedLabTestId) === Number(id)
                ) {

                    await showLabTestDetails(id);
                }

            }
            catch (error) {

                console.error(
                    "Update result error:",
                    error
                );


                alert(
                    error.message ||
                    "Unable to update lab test result."
                );
            }
        }
    );

}


// Open Delete Modal


function openDeleteLabTestModal(id) {

    document
        .getElementById(
            "deleteLabTestId"
        )
        .value =
        id;


    document
        .getElementById(
            "deleteLabTestIdLabel"
        )
        .textContent =
        `#LT-${id}`;


    bootstrap.Modal
        .getOrCreateInstance(
            document.getElementById(
                "deleteLabTestModal"
            )
        )
        .show();

}


// DELETE: Lab Test


function setupDeleteButton() {

    const button =
        document.getElementById(
            "confirmDeleteLabTestBtn"
        );


    button.addEventListener(
        "click",
        async function () {

            const id =
                Number(
                    document
                        .getElementById(
                            "deleteLabTestId"
                        )
                        .value
                );


            try {

                await deleteLabTest(id);


                bootstrap.Modal
                    .getOrCreateInstance(
                        document.getElementById(
                            "deleteLabTestModal"
                        )
                    )
                    .hide();


                if (
                    Number(selectedLabTestId) ===
                    Number(id)
                ) {

                    selectedLabTestId = null;
                }


                await refreshLabTests();


                alert(
                    "Lab test deleted successfully."
                );

            }
            catch (error) {

                console.error(
                    "Delete lab test error:",
                    error
                );


                alert(
                    error.message ||
                    "Unable to delete lab test."
                );
            }
        }
    );

}


// Refresh Page Data


async function refreshLabTests() {

    document
        .getElementById(
            "categoryFilter"
        )
        .value =
        "";


    document
        .getElementById(
            "labTestSearch"
        )
        .value =
        "";


    await loadLabTests();
    await loadLabTestSummary();

}


// Helpers


function getLabTestId(labTest) {

    return (
        labTest.labTestId ??
        labTest.LabTestId ??
        "-"
    );

}

function formatCost(value) {

    const number =
        Number(value);

    if (Number.isNaN(number)) {
        return "0.000";
    }

    return number.toFixed(3);

}

function toDateInputValue(value) {

    if (!value) {
        return "";
    }

    return String(value)
        .substring(0, 10);

}

function escapeHtml(value) {

    return String(value)
        .replaceAll("&", "&amp;")
        .replaceAll("<", "&lt;")
        .replaceAll(">", "&gt;")
        .replaceAll('"', "&quot;")
        .replaceAll("'", "&#039;");

}