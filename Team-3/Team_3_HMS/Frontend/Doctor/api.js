const API_BASE_URL = "https://localhost:7286/api";

const MEDICAL_RECORD_API = `${API_BASE_URL}/MedicalRecord`;
const LAB_TEST_API = `${API_BASE_URL}/LabTest`;
const APPOINTMENT_API = `${API_BASE_URL}/Appointment`;
const ROOM_API = `${API_BASE_URL}/Room`;
const PATIENT_PROFILE_API = `${API_BASE_URL}/PatientProfile`;

async function handleResponse(response) {
    if (!response.ok) {
        const message = await response.text();
        throw new Error(message || "Something went wrong.");
    }


    if (response.status === 204) {
        return null;
    }

    return await response.json();
}



async function getMedicalRecords() {
    const response = await fetch(MEDICAL_RECORD_API, {
        credentials: "include"
    });

    return await handleResponse(response);
}

async function getMedicalRecordById(id) {
    const response = await fetch(
        `${MEDICAL_RECORD_API}/${id}`,
        {
            credentials: "include"
        }
    );

    return await handleResponse(response);
}

async function filterMedicalRecords(diagnosis) {
    const response = await fetch(
        `${MEDICAL_RECORD_API}/filter?diagnosis=${encodeURIComponent(diagnosis)}`,
        {
            credentials: "include"
        }
    );

    return await handleResponse(response);
}

async function getMedicalRecordSummary() {
    const response = await fetch(
        `${MEDICAL_RECORD_API}/summary`,
        {
            credentials: "include"
        }
    );

    return await handleResponse(response);
}

async function createMedicalRecord(record) {
    const response = await fetch(MEDICAL_RECORD_API, {
        method: "POST",
        headers: {
            "Content-Type": "application/json"
        },
        credentials: "include",
        body: JSON.stringify(record)
    });

    return await handleResponse(response);
}

async function updateMedicalRecord(id, record) {
    const response = await fetch(
        `${MEDICAL_RECORD_API}/${id}`,
        {
            method: "PUT",
            headers: {
                "Content-Type": "application/json"
            },
            credentials: "include",
            body: JSON.stringify(record)
        }
    );

    return await handleResponse(response);
}

async function updateDiagnosis(id, diagnosis, treatmentPlan) {
    const response = await fetch(
        `${MEDICAL_RECORD_API}/${id}/diagnosis`,
        {
            method: "PATCH",
            headers: {
                "Content-Type": "application/json"
            },
            credentials: "include",
            body: JSON.stringify({
                diagnosis,
                treatmentPlan
            })
        }
    );

    return await handleResponse(response);
}

async function deleteMedicalRecord(id) {
    const response = await fetch(
        `${MEDICAL_RECORD_API}/${id}`,
        {
            method: "DELETE",
            credentials: "include"
        }
    );

    return await handleResponse(response);
}



async function getLabTests() {
    const response = await fetch(LAB_TEST_API, {
        credentials: "include"
    });

    return await handleResponse(response);
}

async function getLabTestById(id) {
    const response = await fetch(
        `${LAB_TEST_API}/${id}`,
        {
            credentials: "include"
        }
    );

    return await handleResponse(response);
}

async function filterLabTests(category) {
    const response = await fetch(
        `${LAB_TEST_API}/filter?category=${encodeURIComponent(category)}`,
        {
            credentials: "include"
        }
    );

    return await handleResponse(response);
}

async function getLabTestSummary() {
    const response = await fetch(
        `${LAB_TEST_API}/summary`,
        {
            credentials: "include"
        }
    );

    return await handleResponse(response);
}

async function createLabTest(labTest) {
    const response = await fetch(LAB_TEST_API, {
        method: "POST",
        headers: {
            "Content-Type": "application/json"
        },
        credentials: "include",
        body: JSON.stringify(labTest)
    });

    return await handleResponse(response);
}

async function updateLabTest(id, labTest) {
    const response = await fetch(
        `${LAB_TEST_API}/${id}`,
        {
            method: "PUT",
            headers: {
                "Content-Type": "application/json"
            },
            credentials: "include",
            body: JSON.stringify(labTest)
        }
    );

    return await handleResponse(response);
}

async function updateLabTestResult(id, result) {
    const response = await fetch(
        `${LAB_TEST_API}/${id}/result`,
        {
            method: "PATCH",
            headers: {
                "Content-Type": "application/json"
            },
            credentials: "include",
            body: JSON.stringify({
                result
            })
        }
    );

    return await handleResponse(response);
}

async function deleteLabTest(id) {
    const response = await fetch(
        `${LAB_TEST_API}/${id}`,
        {
            method: "DELETE",
            credentials: "include"
        }
    );

    return await handleResponse(response);
}



// ── Appointments ─────────────────────────────────────────────
// A confirmation/reminder email is sent automatically by the server on create.

async function getAppointments() {
    const response = await fetch(APPOINTMENT_API, {
        credentials: "include"
    });

    return await handleResponse(response);
}

async function getAppointmentById(id) {
    const response = await fetch(
        `${APPOINTMENT_API}/${id}`,
        {
            credentials: "include"
        }
    );

    return await handleResponse(response);
}

async function getAppointmentsByStatus(status) {
    const response = await fetch(
        `${APPOINTMENT_API}/by-status/${encodeURIComponent(status)}`,
        {
            credentials: "include"
        }
    );

    return await handleResponse(response);
}

async function createAppointment(appointment) {
    const response = await fetch(APPOINTMENT_API, {
        method: "POST",
        headers: {
            "Content-Type": "application/json"
        },
        credentials: "include",
        body: JSON.stringify(appointment)
    });

    return await handleResponse(response);
}

async function updateAppointment(id, appointment) {
    const response = await fetch(
        `${APPOINTMENT_API}/${id}`,
        {
            method: "PUT",
            headers: {
                "Content-Type": "application/json"
            },
            credentials: "include",
            body: JSON.stringify(appointment)
        }
    );

    return await handleResponse(response);
}

async function updateAppointmentStatus(id, status) {
    const response = await fetch(
        `${APPOINTMENT_API}/${id}/status`,
        {
            method: "PATCH",
            headers: {
                "Content-Type": "application/json"
            },
            credentials: "include",
            body: JSON.stringify(status)
        }
    );

    return await handleResponse(response);
}

async function deleteAppointment(id) {
    const response = await fetch(
        `${APPOINTMENT_API}/${id}`,
        {
            method: "DELETE",
            credentials: "include"
        }
    );

    return await handleResponse(response);
}



// ── Rooms ────────────────────────────────────────────────────

async function getRooms() {
    const response = await fetch(ROOM_API, {
        credentials: "include"
    });

    return await handleResponse(response);
}

async function getAvailableRooms() {
    const response = await fetch(
        `${ROOM_API}/available`,
        {
            credentials: "include"
        }
    );

    return await handleResponse(response);
}



// ── Patient profiles  ──

async function getPatientProfileById(id) {
    const response = await fetch(
        `${PATIENT_PROFILE_API}/find/${id}`,
        {
            credentials: "include"
        }
    );

    return await handleResponse(response);
}