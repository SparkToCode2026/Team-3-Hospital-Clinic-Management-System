const API_BASE_URL = "https://localhost:7286/api";

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



// ── Appointments ─────────────────────────────────────────────
// A confirmation email is sent automatically by the server when an appointment is created 

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



// ── Rooms ────────────────────────────────────────────────────

async function getAvailableRooms() {
    const response = await fetch(
        `${ROOM_API}/available`,
        {
            credentials: "include"
        }
    );

    return await handleResponse(response);
}

async function getRoomById(id) {
    const response = await fetch(
        `${ROOM_API}/${id}`,
        {
            credentials: "include"
        }
    );

    return await handleResponse(response);
}



// ── Patient profile ──────────────────

async function getPatientProfileByUserId(userId) {
    const response = await fetch(
        `${PATIENT_PROFILE_API}/user/${userId}`,
        {
            credentials: "include"
        }
    );

    return await handleResponse(response);
}

async function getPatientProfileById(id) {
    const response = await fetch(
        `${PATIENT_PROFILE_API}/find/${id}`,
        {
            credentials: "include"
        }
    );

    return await handleResponse(response);
}
