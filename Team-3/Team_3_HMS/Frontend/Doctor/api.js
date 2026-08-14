/* ═══════════════════════════════════════════════════════
   Doctor Portal API Client Helper (Frontend/Doctor/api.js)
   ═══════════════════════════════════════════════════════ */

let detectedBaseUrl = "http://localhost:5251/api";

function getAuthToken() {
    return localStorage.getItem('token') || sessionStorage.getItem('token') || '';
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

async function requestApi(endpoint, options = {}) {
    const token = getAuthToken();
    const headers = {
        'Content-Type': 'application/json',
        ...(token ? { 'Authorization': `Bearer ${token}` } : {}),
        ...(options.headers || {})
    };

    const cleanEndpoint = endpoint.startsWith('/') ? endpoint : `/${endpoint}`;
    const urlsToTry = [
        detectedBaseUrl + cleanEndpoint,
        `http://localhost:5251/api${cleanEndpoint}`,
        `http://localhost:5000/api${cleanEndpoint}`,
        `https://localhost:7286/api${cleanEndpoint}`
    ];
    const uniqueUrls = [...new Set(urlsToTry)];

    let lastErr = null;
    for (const url of uniqueUrls) {
        try {
            const response = await fetch(url, {
                ...options,
                headers,
                credentials: 'include'
            });

            if (response.status === 204) {
                return null;
            }

            if (!response.ok) {
                const message = await response.text();
                throw new Error(message || `HTTP ${response.status}`);
            }

            if (url.includes('/api')) {
                detectedBaseUrl = url.substring(0, url.indexOf('/api') + 4);
            }

            const contentType = response.headers.get('content-type');
            if (contentType && contentType.includes('application/json')) {
                return await response.json();
            }
            return null;
        } catch (err) {
            lastErr = err;
            if (err.message && (err.message.startsWith('HTTP 4') || err.message.startsWith('HTTP 5') || err.message.includes('not found') || err.message.includes('Forbidden'))) {
                throw err;
            }
        }
    }

    throw lastErr || new Error('Unable to connect to backend server.');
}

/* ── Medical Record APIs ── */
async function getMedicalRecords() {
    return await requestApi('/MedicalRecord');
}

async function getMedicalRecordById(id) {
    return await requestApi(`/MedicalRecord/${id}`);
}

async function filterMedicalRecords(diagnosis) {
    return await requestApi(`/MedicalRecord/filter?diagnosis=${encodeURIComponent(diagnosis)}`);
}

async function getMedicalRecordSummary() {
    return await requestApi('/MedicalRecord/summary');
}

async function createMedicalRecord(record) {
    return await requestApi('/MedicalRecord', {
        method: 'POST',
        body: JSON.stringify(record)
    });
}

async function updateMedicalRecord(id, record) {
    return await requestApi(`/MedicalRecord/${id}`, {
        method: 'PUT',
        body: JSON.stringify(record)
    });
}

async function updateDiagnosis(id, diagnosis, treatmentPlan) {
    return await requestApi(`/MedicalRecord/${id}/diagnosis`, {
        method: 'PATCH',
        body: JSON.stringify({ diagnosis, treatmentPlan })
    });
}

async function deleteMedicalRecord(id) {
    return await requestApi(`/MedicalRecord/${id}`, {
        method: 'DELETE'
    });
}

/* ── Lab Test APIs ── */
async function getLabTests() {
    return await requestApi('/LabTest');
}

async function getLabTestById(id) {
    return await requestApi(`/LabTest/${id}`);
}

async function filterLabTests(category) {
    return await requestApi(`/LabTest/filter?category=${encodeURIComponent(category)}`);
}

async function getLabTestSummary() {
    return await requestApi('/LabTest/summary');
}

async function createLabTest(labTest) {
    return await requestApi('/LabTest', {
        method: 'POST',
        body: JSON.stringify(labTest)
    });
}

async function updateLabTest(id, labTest) {
    return await requestApi(`/LabTest/${id}`, {
        method: 'PUT',
        body: JSON.stringify(labTest)
    });
}

async function updateLabTestResult(id, result) {
    return await requestApi(`/LabTest/${id}/result`, {
        method: 'PATCH',
        body: JSON.stringify({ result })
    });
}

async function deleteLabTest(id) {
    return await requestApi(`/LabTest/${id}`, {
        method: 'DELETE'
    });
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