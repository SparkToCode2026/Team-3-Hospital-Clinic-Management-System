const ROOM_PAGE_SIZE = 6;

let allRooms = [];
let filteredRooms = [];
let currentPage = 1;

document.addEventListener("DOMContentLoaded", () => {
  loadRooms();
});

// ---------------------------------------------------------------------
// Load & render
// ---------------------------------------------------------------------

async function loadRooms() {
  const tbody = document.getElementById("roomTableBody");
  tbody.innerHTML = `<tr><td colspan="5" style="padding:1.5rem 1rem;text-align:center;">Loading rooms…</td></tr>`;

  try {
    allRooms = await getRoomsSorted();
    applyRoomFilter();
    updateStats();
  } catch (err) {
    console.error("Failed to load rooms:", err);
    tbody.innerHTML = `<tr><td colspan="5" style="padding:1.5rem 1rem;text-align:center;color:#dc2626;">Couldn't load rooms: ${escapeHtml(err.message)}</td></tr>`;
  }
}

function applyRoomFilter() {
  const query = (document.getElementById("roomSearchInput").value || "").toLowerCase().trim();
  const typeVal = document.getElementById("roomTypeFilter").value;

  filteredRooms = allRooms.filter((r) => {
    const number = String(field(r, "RoomNumber") || "").toLowerCase();
    const type = field(r, "Type") || "";
    const matchesQuery = !query || number.includes(query) || type.toLowerCase().includes(query);
    const matchesType = typeVal === "All Types" || !typeVal || type === typeVal;
    return matchesQuery && matchesType;
  });

  currentPage = 1;
  renderRoomTable();
}

function renderRoomTable() {
  const tbody = document.getElementById("roomTableBody");

  if (!filteredRooms.length) {
    tbody.innerHTML = `<tr><td colspan="5" style="padding:1.5rem 1rem;text-align:center;">No rooms match your search.</td></tr>`;
    renderPagination();
    return;
  }

  const start = (currentPage - 1) * ROOM_PAGE_SIZE;
  const pageRooms = filteredRooms.slice(start, start + ROOM_PAGE_SIZE);

  tbody.innerHTML = pageRooms
    .map((r) => {
      const id = field(r, "RoomId");
      const number = escapeHtml(field(r, "RoomNumber"));
      const type = escapeHtml(field(r, "Type"));
      const isAvailable = !!field(r, "IsAvailable");

      return `
        <tr data-room-id="${id}">
          <td class="usr-id">${number}</td>
          <td>${type}</td>
          <td>
            <label class="switch-toggle">
              <input type="checkbox" ${isAvailable ? "checked" : ""} onchange="toggleRoomAvailability(${id}, this.checked)" />
              <span class="slider-toggle"></span>
            </label>
          </td>
          <td>${
            isAvailable
              ? `<span class="role-badge doctor">Available</span>`
              : `<span class="role-badge" style="background: #fef2f2; color: #dc2626;">Occupied</span>`
          }</td>
          <td>
            <button class="btn-tbl-edit" onclick="openEditRoomModal(${id})">Edit</button>
            <button class="btn-tbl-delete" onclick="openDeleteRoomModal(${id})">Delete</button>
          </td>
        </tr>`;
    })
    .join("");

  renderPagination();
}

function renderPagination() {
  const info = document.getElementById("roomPaginationInfo");
  const controls = document.getElementById("roomPaginationControls");
  const total = filteredRooms.length;
  const totalPages = Math.max(1, Math.ceil(total / ROOM_PAGE_SIZE));

  if (currentPage > totalPages) currentPage = totalPages;

  const start = total === 0 ? 0 : (currentPage - 1) * ROOM_PAGE_SIZE + 1;
  const end = Math.min(currentPage * ROOM_PAGE_SIZE, total);
  info.textContent = `Showing ${start} to ${end} of ${total} Hospital Rooms`;

  let buttons = `<button class="btn-filter" ${currentPage === 1 ? "disabled" : ""} onclick="goToRoomPage(${currentPage - 1})">Previous</button>`;
  for (let p = 1; p <= totalPages; p++) {
    buttons += `<button class="${p === currentPage ? "btn-add-user" : "btn-filter"}" style="padding: 0.35rem 0.75rem;" onclick="goToRoomPage(${p})">${p}</button>`;
  }
  buttons += `<button class="btn-filter" ${currentPage === totalPages ? "disabled" : ""} onclick="goToRoomPage(${currentPage + 1})">Next</button>`;
  controls.innerHTML = buttons;
}

function goToRoomPage(page) {
  currentPage = page;
  renderRoomTable();
}

function updateStats() {
  const totals = {};
  const occupied = {};

  allRooms.forEach((r) => {
    const type = field(r, "Type") || "Other";
    totals[type] = (totals[type] || 0) + 1;
    if (!field(r, "IsAvailable")) occupied[type] = (occupied[type] || 0) + 1;
  });

  setStat("statIcuVal", totals["ICU"] || 0, occupied["ICU"] || 0);
  setStat("statGeneralVal", totals["General Ward"] || 0, occupied["General Ward"] || 0);
  setStat("statPrivateVal", totals["Private Suite"] || 0, occupied["Private Suite"] || 0);
  setStat("statTheatreVal", totals["Operating Theatre"] || 0, occupied["Operating Theatre"] || 0);
}

function setStat(elId, total, occupiedCount) {
  const el = document.getElementById(elId);
  if (!el) return;
  el.textContent = `${occupiedCount} / ${total}`;
  const sub = el.nextElementSibling;
  if (sub) sub.textContent = total ? `${Math.round((occupiedCount / total) * 100)}% Occupied` : "No rooms yet";
}

// ---------------------------------------------------------------------
// Toggle availability
// ---------------------------------------------------------------------

async function toggleRoomAvailability(id, isAvailable) {
  try {
    await updateRoomAvailability(id, isAvailable);
    const room = allRooms.find((r) => field(r, "RoomId") === id);
    if (room) room.isAvailable = isAvailable;
    renderRoomTable();
    updateStats();
    triggerRoomAlert("Room availability updated successfully in real-time inventory.");
  } catch (err) {
    console.error("Failed to update availability:", err);
    triggerRoomAlert(`Couldn't update availability: ${err.message}`, true);
    loadRooms(); // resync the toggle with server state
  }
}

// ---------------------------------------------------------------------
// Add room
// ---------------------------------------------------------------------

function openAddRoomModal() {
  document.getElementById("newRoomNum").value = "";
  document.getElementById("newRoomType").selectedIndex = 0;
  document.getElementById("newRoomAvailable").checked = true;
  document.getElementById("addRoomModal").style.display = "flex";
}

function closeAddRoomModal() {
  document.getElementById("addRoomModal").style.display = "none";
}

async function saveNewRoom() {
  const roomNumber = document.getElementById("newRoomNum").value.trim();
  const type = document.getElementById("newRoomType").value;
  const isAvailable = document.getElementById("newRoomAvailable").checked;

  if (!roomNumber) {
    triggerRoomAlert("Please enter a room number.", true);
    return;
  }

  try {
    await createRoom({ RoomNumber: roomNumber, Type: type, IsAvailable: isAvailable });
    closeAddRoomModal();
    triggerRoomAlert("Room added successfully.");
    loadRooms();
  } catch (err) {
    console.error("Failed to add room:", err);
    triggerRoomAlert(`Couldn't add the room: ${err.message}`, true);
  }
}

// ---------------------------------------------------------------------
// Edit room
// ---------------------------------------------------------------------

function openEditRoomModal(id) {
  const room = allRooms.find((r) => field(r, "RoomId") === id);
  if (!room) return;

  document.getElementById("editRoomId").value = id;
  document.getElementById("editRoomNum").value = field(room, "RoomNumber");
  document.getElementById("editRoomType").value = field(room, "Type");
  document.getElementById("editRoomModal").style.display = "flex";
}

function closeEditRoomModal() {
  document.getElementById("editRoomModal").style.display = "none";
}

async function saveEditRoom() {
  const id = document.getElementById("editRoomId").value;
  const roomNumber = document.getElementById("editRoomNum").value.trim();
  const type = document.getElementById("editRoomType").value;

  if (!roomNumber) {
    triggerRoomAlert("Please enter a room number.", true);
    return;
  }

  try {
    await updateRoom(id, { RoomNumber: roomNumber, Type: type });
    closeEditRoomModal();
    triggerRoomAlert("Room updated successfully.");
    loadRooms();
  } catch (err) {
    console.error("Failed to update room:", err);
    triggerRoomAlert(`Couldn't update the room: ${err.message}`, true);
  }
}

// ---------------------------------------------------------------------
// Delete room
// ---------------------------------------------------------------------

async function openDeleteRoomModal(id) {
  const room = allRooms.find((r) => field(r, "RoomId") === id);
  const label = room ? field(room, "RoomNumber") : id;

  if (!confirm(`Are you sure you want to delete room ${label}?`)) return;

  try {
    await deleteRoom(id);
    triggerRoomAlert("Room deleted successfully.");
    loadRooms();
  } catch (err) {
    console.error("Failed to delete room:", err);
    triggerRoomAlert(`Couldn't delete the room: ${err.message}`, true);
  }
}

// ---------------------------------------------------------------------
// Small helpers
// ---------------------------------------------------------------------
function field(obj, pascalName) {
  if (!obj) return undefined;
  const camel = pascalName.charAt(0).toLowerCase() + pascalName.slice(1);
  return obj[camel] !== undefined ? obj[camel] : obj[pascalName];
}

function triggerRoomAlert(message, isError = false) {
  const alertBox = document.getElementById("room-alert");
  alertBox.querySelector("span").textContent = message || "Room status updated successfully in real-time inventory.";
  alertBox.style.background = isError ? "#fef2f2" : "";
  alertBox.style.color = isError ? "#dc2626" : "";
  alertBox.style.display = "flex";
  clearTimeout(alertBox._hideTimer);
  alertBox._hideTimer = setTimeout(() => {
    alertBox.style.display = "none";
    alertBox.style.background = "";
    alertBox.style.color = "";
  }, 4000);
}

function escapeHtml(str) {
  return String(str).replace(/[&<>"']/g, (c) => ({ "&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;", "'": "&#39;" }[c]));
}
