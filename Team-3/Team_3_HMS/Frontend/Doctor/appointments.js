let appointments = [];
let roomsById = {};
let selectedApptId = null;

document.addEventListener("DOMContentLoaded", () => {
  loadRoomOptions();
  loadAppointments();
});

// ---------------------------------------------------------------------
// Reference data: rooms
// ---------------------------------------------------------------------

async function loadRoomOptions() {
  const select = document.getElementById("newRoom");
  try {
    const [all, available] = await Promise.all([getRooms(), getAvailableRooms()]);
    roomsById = {};
    all.forEach((r) => (roomsById[field(r, "RoomId")] = r));

    select.innerHTML = available
      .map((r) => `<option value="${field(r, "RoomId")}">${escapeHtml(field(r, "RoomNumber"))} — ${escapeHtml(field(r, "Type"))}</option>`)
      .join("");
    if (!available.length) select.innerHTML = `<option value="">No rooms currently available</option>`;
  } catch (err) {
    console.error("Failed to load rooms:", err);
    select.innerHTML = `<option value="">Couldn't load rooms</option>`;
  }
}

function roomLabelFor(roomId) {
  const room = roomsById[roomId];
  return room ? field(room, "RoomNumber") : roomId ? `Room ${roomId}` : "—";
}

// ---------------------------------------------------------------------
// Patient lookup preview 
// ---------------------------------------------------------------------

let patientLookupTimer = null;

function onPatientIdInput() {
  clearTimeout(patientLookupTimer);
  const preview = document.getElementById("patientLookupPreview");
  const id = document.getElementById("newPatientId").value.trim();

  if (!id) {
    preview.textContent = "";
    return;
  }

  patientLookupTimer = setTimeout(async () => {
    preview.textContent = "Looking up patient…";
    try {
      const profile = await getPatientProfileById(id);
      preview.textContent = `✓ Profile #${field(profile, "PatientProfileID") ?? id} — Blood group ${field(profile, "BloodGroup") || "n/a"}, DOB ${field(profile, "DateOfBirth") || "n/a"}`;
      preview.style.color = "var(--green, #16a34a)";
    } catch (err) {
      preview.textContent = "No patient profile found with that ID.";
      preview.style.color = "#dc2626";
    }
  }, 400);
}

// ---------------------------------------------------------------------
// Load & render appointments
// ---------------------------------------------------------------------

async function loadAppointments() {
  const tbody = document.getElementById("tableBody");
  tbody.innerHTML = `<tr><td colspan="7" style="padding:1.5rem 1rem;text-align:center;">Loading appointments…</td></tr>`;

  try {
    appointments = await getAppointments();
    renderTable();
  } catch (err) {
    console.error("Failed to load appointments:", err);
    tbody.innerHTML = `<tr><td colspan="7" style="padding:1.5rem 1rem;text-align:center;color:#dc2626;">Couldn't load appointments: ${escapeHtml(err.message)}</td></tr>`;
  }
}

function renderTable() {
  const tbody = document.getElementById("tableBody");
  tbody.innerHTML = "";

  if (!appointments.length) {
    tbody.innerHTML = `<tr><td colspan="7" style="padding:1.5rem 1rem;text-align:center;">No appointments found.</td></tr>`;
    updateCounts();
    return;
  }

  appointments.forEach((appt) => {
    const id = field(appt, "AppointmentId");
    const status = field(appt, "Status") || "Pending";
    const patientId = field(appt, "PatientProfileID");
    const roomId = field(appt, "RoomId");

    let badgeClass = "badge-neutral";
    if (status === "Confirmed") badgeClass = "badge-success";
    else if (status === "Pending") badgeClass = "badge-warn";
    else if (status === "Cancelled") badgeClass = "badge-danger";

    const tr = document.createElement("tr");
    tr.innerHTML = `
      <td><strong>#APT-${id}</strong></td>
      <td><strong>Patient #${patientId}</strong></td>
      <td>${formatDateTime(field(appt, "AppointmentDateTime"))}</td>
      <td><span class="badge-dept">${escapeHtml(roomLabelFor(roomId))}</span></td>
      <td>${escapeHtml(field(appt, "ReasonForVisit") || "—")}</td>
      <td><span class="badge ${badgeClass}">${escapeHtml(status)}</span></td>
      <td>
        <button class="btn-ghost me-1" onclick="viewDetails(${id})">View</button>
        <button class="btn-ghost me-1" onclick="openStatusModal(${id})">Status</button>
        ${status !== "Cancelled" ? `<button class="btn-danger-ghost" onclick="cancelAppointment(${id})">Cancel</button>` : ""}
      </td>
    `;
    tbody.appendChild(tr);
  });

  updateCounts();
}

// ---------------------------------------------------------------------
// Search / filter 
// ---------------------------------------------------------------------

function filterTable() {
  const query = document.getElementById("searchInput").value.toLowerCase();
  const statusVal = document.getElementById("statusFilter").value;

  const rows = document.querySelectorAll("#tableBody tr");
  let visibleCount = 0;

  rows.forEach((row, index) => {
    const appt = appointments[index];
    if (!appt) return;

    const reason = (field(appt, "ReasonForVisit") || "").toLowerCase();
    const patientId = String(field(appt, "PatientProfileID"));
    const status = field(appt, "Status") || "";

    const matchesSearch = !query || reason.includes(query) || patientId.includes(query);
    const matchesStatus = statusVal === "all" || status === statusVal;

    if (matchesSearch && matchesStatus) {
      row.style.display = "";
      visibleCount++;
    } else {
      row.style.display = "none";
    }
  });

  document.getElementById("paginationInfo").textContent = `Showing ${visibleCount} of ${appointments.length} appointments`;
}

function filterByStatus(st) {
  document.getElementById("statusFilter").value = st;
  filterTable();
}

function updateCounts() {
  document.getElementById("countTotal").textContent = appointments.length;
  document.getElementById("countConfirmed").textContent = appointments.filter((a) => field(a, "Status") === "Confirmed").length;
  document.getElementById("countPending").textContent = appointments.filter((a) => field(a, "Status") === "Pending").length;
  document.getElementById("countCompleted").textContent = appointments.filter((a) => field(a, "Status") === "Completed").length;
}

// ---------------------------------------------------------------------
// View details 
// ---------------------------------------------------------------------

async function viewDetails(id) {
  const appt = appointments.find((a) => field(a, "AppointmentId") === id);
  if (!appt) return;

  selectedApptId = id;
  const status = field(appt, "Status") || "Pending";

  document.getElementById("detApptId").textContent = `#APT-${id}`;
  document.getElementById("detPatient").textContent = `Patient #${field(appt, "PatientProfileID")}`;
  document.getElementById("detDateTime").textContent = formatDateTime(field(appt, "AppointmentDateTime"));
  document.getElementById("detRoom").textContent = roomLabelFor(field(appt, "RoomId"));
  document.getElementById("detReason").textContent = field(appt, "ReasonForVisit") || "—";

  let badgeClass = "badge-neutral";
  if (status === "Confirmed") badgeClass = "badge-success";
  else if (status === "Pending") badgeClass = "badge-warn";
  else if (status === "Cancelled") badgeClass = "badge-danger";
  document.getElementById("detStatus").innerHTML = `<span class="badge ${badgeClass}">${escapeHtml(status)}</span>`;

  new bootstrap.Offcanvas(document.getElementById("detailsOffcanvas")).show();

  try {
    const profile = await getPatientProfileById(field(appt, "PatientProfileID"));
    document.getElementById("detPatient").textContent = `Patient #${field(appt, "PatientProfileID")} — Blood group ${field(profile, "BloodGroup") || "n/a"}`;
  } catch (_) {
    /* leave the plain "Patient #id" label */
  }
}

// ---------------------------------------------------------------------
// Update status
// ---------------------------------------------------------------------

function openStatusModal(id) {
  const appt = appointments.find((a) => field(a, "AppointmentId") === id);
  if (!appt) return;

  selectedApptId = id;
  document.getElementById("statusApptId").value = id;
  document.getElementById("statusPatientName").value = `#APT-${id} — Patient #${field(appt, "PatientProfileID")}`;
  document.getElementById("statusSelect").value = field(appt, "Status") || "Pending";

  new bootstrap.Modal(document.getElementById("updateStatusModal")).show();
}

function openStatusModalFromDetails() {
  if (selectedApptId) openStatusModal(selectedApptId);
}

async function handleSaveStatus(e) {
  e.preventDefault();
  const id = parseInt(document.getElementById("statusApptId").value, 10);
  const newStatus = document.getElementById("statusSelect").value;

  try {
    await updateAppointmentStatus(id, newStatus);
    await loadAppointments();
    showAlert(`Updated appointment status to "${newStatus}".`);
  } catch (err) {
    console.error("Failed to update status:", err);
    showAlert(`Couldn't update status: ${err.message}`);
  }

  bootstrap.Modal.getInstance(document.getElementById("updateStatusModal")).hide();
}

// ---------------------------------------------------------------------
// Schedule new appointment
// ---------------------------------------------------------------------

async function handleScheduleAppt(e) {
  e.preventDefault();
  const patientId = document.getElementById("newPatientId").value.trim();
  const dtVal = document.getElementById("newDateTime").value;
  const roomId = document.getElementById("newRoom").value;
  const reason = document.getElementById("newReason").value.trim();
  const status = document.getElementById("newStatus").value;

  if (!patientId || !dtVal || !roomId || !reason) {
    showAlert("Please fill in patient ID, date/time, room, and reason.");
    return;
  }

  const payload = {
    PatientProfileID: Number(patientId),
    RoomId: Number(roomId),
    AppointmentDateTime: dtVal,
    ReasonForVisit: reason,
    Status: status,
  };

  try {
    await createAppointment(payload);
    await loadAppointments();
    showAlert(`Scheduled a new appointment for Patient #${patientId}. A confirmation email has been sent.`);
    bootstrap.Modal.getInstance(document.getElementById("createApptModal")).hide();
    e.target.reset();
    document.getElementById("patientLookupPreview").textContent = "";
  } catch (err) {
    console.error("Failed to schedule appointment:", err);
    showAlert(`Couldn't schedule the appointment: ${err.message}`);
  }
}

// ---------------------------------------------------------------------
// Cancel appointment
// ---------------------------------------------------------------------

async function cancelAppointment(id) {
  const appt = appointments.find((a) => field(a, "AppointmentId") === id);
  if (!appt) return;

  if (!confirm(`Are you sure you want to cancel appointment #APT-${id} (Patient #${field(appt, "PatientProfileID")})?`)) return;

  try {
    await updateAppointmentStatus(id, "Cancelled");
    await loadAppointments();
    showAlert(`Cancelled appointment #APT-${id}.`);
  } catch (err) {
    console.error("Failed to cancel appointment:", err);
    showAlert(`Couldn't cancel the appointment: ${err.message}`);
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

function showAlert(msg) {
  const alertBox = document.getElementById("actionAlert");
  document.getElementById("alertMsg").textContent = msg;
  alertBox.classList.remove("d-none");
  clearTimeout(alertBox._hideTimer);
  alertBox._hideTimer = setTimeout(() => alertBox.classList.add("d-none"), 3500);
}

function formatDateTime(raw) {
  if (!raw) return "—";
  const d = new Date(raw);
  if (isNaN(d.getTime())) return escapeHtml(raw);
  return d.toLocaleString(undefined, { year: "numeric", month: "short", day: "numeric", hour: "2-digit", minute: "2-digit" });
}

function escapeHtml(str) {
  return String(str).replace(/[&<>"']/g, (c) => ({ "&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;", "'": "&#39;" }[c]));
}
