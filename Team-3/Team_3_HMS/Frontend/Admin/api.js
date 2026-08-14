const API_BASE_URL = "https://localhost:7286/api";

const ROOM_API = `${API_BASE_URL}/Room`;

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



// ── Rooms ────────────────────────────────────────────────────

async function getRooms() {
    const response = await fetch(ROOM_API, {
        credentials: "include"
    });

    return await handleResponse(response);
}

async function getRoomsSorted() {
    const response = await fetch(
        `${ROOM_API}/sorted`,
        {
            credentials: "include"
        }
    );

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

async function getRoomById(id) {
    const response = await fetch(
        `${ROOM_API}/${id}`,
        {
            credentials: "include"
        }
    );

    return await handleResponse(response);
}

async function createRoom(room) {
    const response = await fetch(ROOM_API, {
        method: "POST",
        headers: {
            "Content-Type": "application/json"
        },
        credentials: "include",
        body: JSON.stringify(room)
    });

    return await handleResponse(response);
}

async function updateRoom(id, room) {
    const response = await fetch(
        `${ROOM_API}/${id}`,
        {
            method: "PUT",
            headers: {
                "Content-Type": "application/json"
            },
            credentials: "include",
            body: JSON.stringify(room)
        }
    );

    return await handleResponse(response);
}

async function updateRoomAvailability(id, isAvailable) {
    const response = await fetch(
        `${ROOM_API}/${id}/availability`,
        {
            method: "PATCH",
            headers: {
                "Content-Type": "application/json"
            },
            credentials: "include",
            body: JSON.stringify(isAvailable)
        }
    );

    return await handleResponse(response);
}

async function deleteRoom(id) {
    const response = await fetch(
        `${ROOM_API}/${id}`,
        {
            method: "DELETE",
            credentials: "include"
        }
    );

    return await handleResponse(response);
}
