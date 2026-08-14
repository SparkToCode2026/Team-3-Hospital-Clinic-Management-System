
// Medical Records Page


let selectedMedicalRecordId = null;


// Page Load


document.addEventListener("DOMContentLoaded", async function () {

    await loadMedicalRecords();
    await loadSummaries();

    setupSearch();
    setupCreateForm();
    setupEditForm();
    setupDiagnosisForm();
    setupDeleteButton();
    setupLabResultButton();
    setupTableActions();
    setupEditDiagnosisButton();

});


// GET: Load All Medical Records


async function loadMedicalRecords() {

    const tableBody =
        document.getElementById("medicalRecordsTableBody");

    const recordsCount =
        document.getElementById("recordsCount");

    try {

        tableBody.innerHTML = `
        <tr>
            <td colspan="7" class="text-center py-4">
                Loading medical records...
            </td>
        </tr>
    `;

        const records = await getMedicalRecords();

        renderMedicalRecords(records);

    }
    catch (error) {

        console.error(
            "Error loading medical records:",
            error
        );

        tableBody.innerHTML = `
        <tr>
            <td colspan="7" class="text-center py-4">
                Unable to load medical records.
            </td>
        </tr>
    `;

        recordsCount.textContent =
            "Unable to load records.";
    }

}

// Render Medical Records Table


function renderMedicalRecords(records) {

    const tableBody =
        document.getElementById("medicalRecordsTableBody");

    const recordsCount =
        document.getElementById("recordsCount");

    tableBody.innerHTML = "";

    if (!records || records.length === 0) {

        tableBody.innerHTML = `
        <tr>
            <td colspan="7" class="text-center py-4">
                No medical records found.
            </td>
        </tr>
    `;

        recordsCount.textContent = "0 records";

        return;
    }


    records.forEach(function (record) {

        const recordId = getMedicalRecordId(record);

        const appointment =
            record.appointment ??
            record.Appointment ??
            null;

        const patientId =
            appointment?.patientProfileID ??
            appointment?.patientProfileId ??
            appointment?.PatientProfileID ??
            "-";

        const appointmentId =
            record.appointmentId ??
            record.AppointmentId ??
            "-";

        const diagnosis =
            record.diagnosis ??
            record.Diagnosis ??
            "-";

        const symptom =
            record.symptom ??
            record.Symptom ??
            "-";

        const recordDate =
            record.recordDate ??
            record.RecordDate ??
            "-";


        const row = document.createElement("tr");

        row.innerHTML = `
        <td>#MR-${escapeHtml(recordId)}</td>

        <td>
            ${patientId === "-"
                ? "-"
                : `Patient #${escapeHtml(patientId)}`
            }
        </td>

        <td>${escapeHtml(diagnosis)}</td>

        <td>${escapeHtml(symptom)}</td>

        <td>${escapeHtml(recordDate)}</td>

        <td>
            ${appointmentId === "-"
                ? "-"
                : `#APT-${escapeHtml(appointmentId)}`
            }
        </td>

        <td>
            <div class="d-flex gap-1">

                <button
                    type="button"
                    class="btn-ghost view-record-btn"
                    data-record-id="${recordId}">
                    View
                </button>

                <button
                    type="button"
                    class="btn-ghost edit-record-btn"
                    data-record-id="${recordId}">
                    Edit
                </button>

                <button
                    type="button"
                    class="btn-ghost delete-record-btn"
                    data-record-id="${recordId}">
                    Delete
                </button>

            </div>
        </td>
    `;

        tableBody.appendChild(row);
    });


    recordsCount.textContent =
        `${records.length} record${records.length === 1 ? "" : "s"}`;

}


// GET: Summary Cards


async function loadSummaries() {

    // Medical Record Summary
    try {

        const summary =
            await getMedicalRecordSummary();

        const totalRecords =
            summary.totalRecords ??
            summary.TotalRecords ??
            0;

        document
            .getElementById("totalMedicalRecords")
            .textContent = totalRecords;

    }
    catch (error) {

        console.error(
            "Error loading medical record summary:",
            error
        );

        document
            .getElementById("totalMedicalRecords")
            .textContent = "0";
    }


    // Lab Test Summary
    try {

        const summary =
            await getLabTestSummary();

        const totalCost =
            summary.totalCost ??
            summary.TotalCost ??
            0;

        document
            .getElementById("totalLabTestCosts")
            .textContent =
            `OMR ${Number(totalCost).toFixed(3)}`;

    }
    catch (error) {

        console.error(
            "Error loading lab test summary:",
            error
        );

        document
            .getElementById("totalLabTestCosts")
            .textContent =
            "OMR 0.000";
    }

}


// Search Medical Records By Diagnosis


function setupSearch() {

    const searchInput =
        document.getElementById("diagnosisSearch");

    let searchTimer;


    searchInput.addEventListener(
        "input",
        function () {

            clearTimeout(searchTimer);

            searchTimer = setTimeout(
                async function () {

                    const diagnosis =
                        searchInput.value.trim();

                    if (diagnosis === "") {

                        await loadMedicalRecords();

                        return;
                    }

                    await searchMedicalRecords(
                        diagnosis
                    );

                },
                400
            );
        }
    );

}

async function searchMedicalRecords(diagnosis) {

    const tableBody =
        document.getElementById("medicalRecordsTableBody");

    const recordsCount =
        document.getElementById("recordsCount");

    try {

        const records =
            await filterMedicalRecords(
                diagnosis
            );

        renderMedicalRecords(records);

    }
    catch (error) {

        console.error(
            "Search error:",
            error
        );

        tableBody.innerHTML = `
        <tr>
            <td colspan="7" class="text-center py-4">
                No matching medical records found.
            </td>
        </tr>
    `;

        recordsCount.textContent =
            "0 records";
    }

}

// POST: Create Medical Record


function setupCreateForm() {

    const form =
        document.getElementById(
            "createMedicalRecordForm"
        );


    form.addEventListener(
        "submit",
        async function (event) {

            event.preventDefault();


            const medicalRecord = {

                medicalRecordID: 0,

                appointmentId:
                    Number(
                        document.getElementById(
                            "createAppointmentId"
                        ).value
                    ),

                recordDate:
                    document.getElementById(
                        "createRecordDate"
                    ).value,

                diagnosis:
                    document.getElementById(
                        "createDiagnosis"
                    ).value.trim(),

                symptom:
                    document.getElementById(
                        "createSymptoms"
                    ).value.trim(),

                treatmentPlan:
                    document.getElementById(
                        "createTreatmentPlan"
                    ).value.trim()
            };


            try {

                await createMedicalRecord(
                    medicalRecord
                );


                const modalElement =
                    document.getElementById(
                        "newRecordModal"
                    );

                bootstrap.Modal
                    .getOrCreateInstance(
                        modalElement
                    )
                    .hide();


                form.reset();


                await loadMedicalRecords();
                await loadSummaries();


                alert(
                    "Medical record created successfully."
                );

            }
            catch (error) {

                console.error(
                    "Create medical record error:",
                    error
                );

                alert(
                    error.message ||
                    "Unable to create medical record."
                );
            }
        }
    );

}


// Table Buttons


function setupTableActions() {

    const tableBody =
        document.getElementById(
            "medicalRecordsTableBody"
        );


    tableBody.addEventListener(
        "click",
        async function (event) {

            const viewButton =
                event.target.closest(
                    ".view-record-btn"
                );

            const editButton =
                event.target.closest(
                    ".edit-record-btn"
                );

            const deleteButton =
                event.target.closest(
                    ".delete-record-btn"
                );


            // View
            if (viewButton) {

                const id =
                    Number(
                        viewButton.dataset.recordId
                    );

                await showRecordDetails(id);

                return;
            }


            // Edit
            if (editButton) {

                const id =
                    Number(
                        editButton.dataset.recordId
                    );

                await openEditRecordModal(id);

                return;
            }


            // Delete
            if (deleteButton) {

                const id =
                    Number(
                        deleteButton.dataset.recordId
                    );

                openDeleteModal(id);
            }
        }
    );

}


// GET BY ID: Medical Record Details


async function showRecordDetails(id) {

    try {

        const record =
            await getMedicalRecordById(id);

        selectedMedicalRecordId = id;


        const recordId =
            getMedicalRecordId(record);

        const appointment =
            record.appointment ??
            record.Appointment ??
            null;

        const patientId =
            appointment?.patientProfileID ??
            appointment?.patientProfileId ??
            appointment?.PatientProfileID ??
            "-";

        const appointmentDate =
            appointment?.appointmentDateTime ??
            appointment?.AppointmentDateTime ??
            "-";

        const reasonForVisit =
            appointment?.reasonForVisit ??
            appointment?.ReasonForVisit ??
            "-";

        const diagnosis =
            record.diagnosis ??
            record.Diagnosis ??
            "-";

        const symptoms =
            record.symptom ??
            record.Symptom ??
            "-";

        const treatmentPlan =
            record.treatmentPlan ??
            record.TreatmentPlan ??
            "-";

        const recordDate =
            record.recordDate ??
            record.RecordDate ??
            "-";

        const appointmentId =
            record.appointmentId ??
            record.AppointmentId ??
            "-";


        document
            .getElementById(
                "detailsRecordId"
            )
            .textContent =
            `#MR-${recordId}`;


        document
            .getElementById(
                "detailsPatient"
            )
            .textContent =
            patientId === "-"
                ? "-"
                : `Patient #${patientId}`;


        document
            .getElementById(
                "detailsDiagnosis"
            )
            .textContent =
            diagnosis;


        document
            .getElementById(
                "detailsSymptoms"
            )
            .textContent =
            symptoms;


        document
            .getElementById(
                "detailsTreatmentPlan"
            )
            .textContent =
            treatmentPlan;


        document
            .getElementById(
                "detailsRecordDate"
            )
            .textContent =
            recordDate;


        document
            .getElementById(
                "detailsAppointmentId"
            )
            .textContent =
            appointmentId === "-"
                ? "-"
                : `#APT-${appointmentId}`;


        document
            .getElementById(
                "detailsAppointmentDate"
            )
            .textContent =
            appointmentDate;


        document
            .getElementById(
                "detailsReasonForVisit"
            )
            .textContent =
            reasonForVisit;


        // Prepare Diagnosis Modal
        document
            .getElementById(
                "diagnosisRecordId"
            )
            .value = recordId;


        document
            .getElementById(
                "editDiagnosisOnly"
            )
            .value =
            diagnosis === "-"
                ? ""
                : diagnosis;


        document
            .getElementById(
                "editDiagnosisTreatmentPlan"
            )
            .value =
            treatmentPlan === "-"
                ? ""
                : treatmentPlan;


        document
            .getElementById(
                "editDiagnosisRecordLabel"
            )
            .textContent =
            `Medical Record #MR-${recordId}`;


        await loadRecordLabTests(
            recordId
        );


        const offcanvasElement =
            document.getElementById(
                "recordDetails"
            );

        bootstrap.Offcanvas
            .getOrCreateInstance(
                offcanvasElement
            )
            .show();

    }
    catch (error) {

        console.error(
            "Load medical record details error:",
            error
        );

        alert(
            error.message ||
            "Unable to load medical record details."
        );
    }

}


// Lab Tests Inside Medical Record Details


async function loadRecordLabTests(
    medicalRecordId
) {

    const container =
        document.getElementById(
            "recordLabTests"
        );


    container.innerHTML = `
    <p class="mt-2 text-muted">
        Loading lab tests...
    </p>
`;


    try {

        const labTests =
            await getLabTests();


        const recordLabTests =
            (labTests || []).filter(
                function (labTest) {

                    const id =
                        labTest.medicalRecordId ??
                        labTest.MedicalRecordId;

                    return Number(id) ===
                        Number(medicalRecordId);
                }
            );


        if (recordLabTests.length === 0) {

            container.innerHTML = `
            <p class="mt-2 text-muted">
                No lab tests found for this record.
            </p>
        `;

            return;
        }


        container.innerHTML = "";


        recordLabTests.forEach(
            function (labTest) {

                const labTestId =
                    labTest.labTestId ??
                    labTest.LabTestId;

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
                    "Not available";


                const card =
                    document.createElement(
                        "div"
                    );


                card.style.cssText = `
                background:var(--bg-page);
                border:1px solid var(--border);
                border-radius:8px;
                padding:.85rem;
                margin-top:.5rem;
                display:flex;
                align-items:center;
                justify-content:space-between;
                gap:1rem;
            `;


                card.innerHTML = `
                <div>

                    <strong
                        style="font-size:.85rem;">
                        ${escapeHtml(testName)}
                    </strong>

                    <div
                        style="font-size:.72rem;color:var(--text-secondary);">
                        Category:
                        ${escapeHtml(category)}
                    </div>

                    <div
                        style="font-size:.72rem;color:var(--text-secondary);">
                        Date:
                        ${escapeHtml(testDate)}
                    </div>

                    <div
                        style="font-size:.72rem;color:var(--text-secondary);">
                        Cost:
                        OMR ${Number(cost).toFixed(3)}
                    </div>

                    <div
                        style="font-size:.72rem;color:var(--green);font-weight:600;">
                        Result:
                        ${escapeHtml(result)}
                    </div>

                </div>

                <button
                    type="button"
                    class="btn-ghost update-lab-result-btn"
                    data-lab-id="${labTestId}"
                    data-test-name="${escapeHtml(testName)}"
                    data-result="${escapeHtml(result)}">
                    Update Result
                </button>
            `;


                container.appendChild(card);
            }
        );


        setupLabTestCardButtons();

    }
    catch (error) {

        console.error(
            "Load lab tests error:",
            error
        );

        container.innerHTML = `
        <p class="mt-2 text-muted">
            Unable to load lab tests.
        </p>
    `;
    }

}

// Open Update Lab Result Modal


function setupLabTestCardButtons() {

    const buttons =
        document.querySelectorAll(
            ".update-lab-result-btn"
        );


    buttons.forEach(
        function (button) {

            button.addEventListener(
                "click",
                function () {

                    document
                        .getElementById(
                            "updateLabTestId"
                        )
                        .value =
                        button.dataset.labId;


                    document
                        .getElementById(
                            "updateLabTestName"
                        )
                        .value =
                        button.dataset.testName;


                    document
                        .getElementById(
                            "updateLabTestResult"
                        )
                        .value =
                        button.dataset.result ===
                            "Not available"
                            ? ""
                            : button.dataset.result;


                    document
                        .getElementById(
                            "updateLabTestLabel"
                        )
                        .textContent =
                        `Lab Test #${button.dataset.labId}`;


                    const offcanvasElement =
                        document.getElementById(
                            "recordDetails"
                        );


                    bootstrap.Offcanvas
                        .getOrCreateInstance(
                            offcanvasElement
                        )
                        .hide();


                    const modalElement =
                        document.getElementById(
                            "updateResultModal"
                        );


                    bootstrap.Modal
                        .getOrCreateInstance(
                            modalElement
                        )
                        .show();
                }
            );
        }
    );

}


// PATCH: Update Lab Result


function setupLabResultButton() {

    const button =
        document.getElementById(
            "updateLabResultBtn"
        );


    button.addEventListener(
        "click",
        async function () {

            const labTestId =
                Number(
                    document.getElementById(
                        "updateLabTestId"
                    ).value
                );

            const result =
                document.getElementById(
                    "updateLabTestResult"
                ).value.trim();


            if (!result) {

                alert(
                    "Please enter the lab test result."
                );

                return;
            }


            try {

                await updateLabTestResult(
                    labTestId,
                    result
                );


                const modalElement =
                    document.getElementById(
                        "updateResultModal"
                    );


                bootstrap.Modal
                    .getOrCreateInstance(
                        modalElement
                    )
                    .hide();


                await loadSummaries();


                alert(
                    "Lab test result updated successfully."
                );


                if (selectedMedicalRecordId) {

                    await showRecordDetails(
                        selectedMedicalRecordId
                    );
                }

            }
            catch (error) {

                console.error(
                    "Update lab result error:",
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


// Open Edit Medical Record Modal


async function openEditRecordModal(id) {

    try {

        const record =
            await getMedicalRecordById(id);


        const recordId =
            getMedicalRecordId(record);


        document
            .getElementById(
                "editRecordId"
            )
            .value =
            recordId;


        document
            .getElementById(
                "editRecordLabel"
            )
            .textContent =
            `Medical Record #MR-${recordId}`;


        document
            .getElementById(
                "editAppointmentId"
            )
            .value =
            record.appointmentId ??
            record.AppointmentId ??
            "";


        document
            .getElementById(
                "editRecordDate"
            )
            .value =
            toDateInputValue(
                record.recordDate ??
                record.RecordDate ??
                ""
            );


        document
            .getElementById(
                "editDiagnosis"
            )
            .value =
            record.diagnosis ??
            record.Diagnosis ??
            "";


        document
            .getElementById(
                "editSymptoms"
            )
            .value =
            record.symptom ??
            record.Symptom ??
            "";


        document
            .getElementById(
                "editTreatmentPlan"
            )
            .value =
            record.treatmentPlan ??
            record.TreatmentPlan ??
            "";


        const modalElement =
            document.getElementById(
                "editRecordModal"
            );


        bootstrap.Modal
            .getOrCreateInstance(
                modalElement
            )
            .show();

    }
    catch (error) {

        console.error(
            "Open edit record error:",
            error
        );

        alert(
            error.message ||
            "Unable to load medical record."
        );
    }

}


// PUT: Update Medical Record


function setupEditForm() {

    const form =
        document.getElementById(
            "editMedicalRecordForm"
        );


    form.addEventListener(
        "submit",
        async function (event) {

            event.preventDefault();


            const id =
                Number(
                    document.getElementById(
                        "editRecordId"
                    ).value
                );


            const medicalRecord = {

                medicalRecordID: id,

                appointmentId:
                    Number(
                        document.getElementById(
                            "editAppointmentId"
                        ).value
                    ),

                recordDate:
                    document.getElementById(
                        "editRecordDate"
                    ).value,

                diagnosis:
                    document.getElementById(
                        "editDiagnosis"
                    ).value.trim(),

                symptom:
                    document.getElementById(
                        "editSymptoms"
                    ).value.trim(),

                treatmentPlan:
                    document.getElementById(
                        "editTreatmentPlan"
                    ).value.trim()
            };


            try {

                await updateMedicalRecord(
                    id,
                    medicalRecord
                );


                const modalElement =
                    document.getElementById(
                        "editRecordModal"
                    );


                bootstrap.Modal
                    .getOrCreateInstance(
                        modalElement
                    )
                    .hide();


                await loadMedicalRecords();
                await loadSummaries();


                alert(
                    "Medical record updated successfully."
                );

            }
            catch (error) {

                console.error(
                    "Update medical record error:",
                    error
                );

                alert(
                    error.message ||
                    "Unable to update medical record."
                );
            }
        }
    );

}


// Prepare Edit Diagnosis


function setupEditDiagnosisButton() {

    const button =
        document.getElementById(
            "openEditDiagnosisBtn"
        );


    button.addEventListener(
        "click",
        function () {

            if (!selectedMedicalRecordId) {

                return;
            }


            const diagnosis =
                document.getElementById(
                    "detailsDiagnosis"
                ).textContent;


            const treatmentPlan =
                document.getElementById(
                    "detailsTreatmentPlan"
                ).textContent;


            document
                .getElementById(
                    "diagnosisRecordId"
                )
                .value =
                selectedMedicalRecordId;


            document
                .getElementById(
                    "editDiagnosisRecordLabel"
                )
                .textContent =
                `Medical Record #MR-${selectedMedicalRecordId}`;


            document
                .getElementById(
                    "editDiagnosisOnly"
                )
                .value =
                diagnosis === "-"
                    ? ""
                    : diagnosis;


            document
                .getElementById(
                    "editDiagnosisTreatmentPlan"
                )
                .value =
                treatmentPlan === "-"
                    ? ""
                    : treatmentPlan;
        }
    );

}


// PATCH: Diagnosis + Treatment Plan


function setupDiagnosisForm() {

    const form =
        document.getElementById(
            "editDiagnosisForm"
        );


    form.addEventListener(
        "submit",
        async function (event) {

            event.preventDefault();


            const id =
                Number(
                    document.getElementById(
                        "diagnosisRecordId"
                    ).value
                );


            const diagnosis =
                document.getElementById(
                    "editDiagnosisOnly"
                ).value.trim();


            const treatmentPlan =
                document.getElementById(
                    "editDiagnosisTreatmentPlan"
                ).value.trim();


            try {

                await updateDiagnosis(
                    id,
                    diagnosis,
                    treatmentPlan
                );


                const modalElement =
                    document.getElementById(
                        "editDiagnosisModal"
                    );


                bootstrap.Modal
                    .getOrCreateInstance(
                        modalElement
                    )
                    .hide();


                await loadMedicalRecords();
                await loadSummaries();


                alert(
                    "Diagnosis updated successfully."
                );


                await showRecordDetails(id);

            }
            catch (error) {

                console.error(
                    "Update diagnosis error:",
                    error
                );

                alert(
                    error.message ||
                    "Unable to update diagnosis."
                );
            }
        }
    );

}


// Open Delete Modal


function openDeleteModal(id) {

    document
        .getElementById(
            "deleteRecordId"
        )
        .value = id;


    document
        .getElementById(
            "deleteRecordIdLabel"
        )
        .textContent =
        `#MR-${id}`;


    const modalElement =
        document.getElementById(
            "deleteRecordModal"
        );


    bootstrap.Modal
        .getOrCreateInstance(
            modalElement
        )
        .show();

}


// DELETE: Medical Record


function setupDeleteButton() {

    const button =
        document.getElementById(
            "confirmDeleteRecordBtn"
        );


    button.addEventListener(
        "click",
        async function () {

            const id =
                Number(
                    document.getElementById(
                        "deleteRecordId"
                    ).value
                );


            try {

                await deleteMedicalRecord(id);


                const modalElement =
                    document.getElementById(
                        "deleteRecordModal"
                    );


                bootstrap.Modal
                    .getOrCreateInstance(
                        modalElement
                    )
                    .hide();


                selectedMedicalRecordId = null;


                await loadMedicalRecords();
                await loadSummaries();


                alert(
                    "Medical record deleted successfully."
                );

            }
            catch (error) {

                console.error(
                    "Delete medical record error:",
                    error
                );

                alert(
                    error.message ||
                    "Unable to delete medical record."
                );
            }
        }
    );

}

// 
// Helpers
// 

function getMedicalRecordId(record) {

    return (
        record.medicalRecordID ??
        record.medicalRecordId ??
        record.MedicalRecordID ??
        "-"
    );

}

function toDateInputValue(value) {

    if (!value) {
        return "";
    }

    return String(value).substring(0, 10);

}

function escapeHtml(value) {

    return String(value)
        .replaceAll("&", "&amp;")
        .replaceAll("<", "&lt;")
        .replaceAll(">", "&gt;")
        .replaceAll('"', "&quot;")
        .replaceAll("'", "&#039;");

}