const STATUS_VALUES = ["Pending", "Confirmed", "Cancelled"];

let currentFilter = "all";
let currentAppointments = [];
let roomsById = {};
let currentPatientProfileId = null;

document.addEventListener("DOMContentLoaded", async () => {
  currentPatientProfileId = await resolveCurrentPatientProfileId();
  if (!currentPatientProfileId) {
    console.warn(
      "Couldn't resolve the logged-in patient's PatientProfileID. " +
        "Make sure localStorage has a 'userId' (or 'patientProfileId' directly) set after login."
    );
  }

  loadAppointments();
  loadRoomOptions();
});



async function resolveCurrentPatientProfileId() {
  const cached = localStorage.getItem("patientProfileId");
  if (cached) return Number(cached);

  const userId = localStorage.getItem("userId");
  if (!userId) return null;

  try {
    const profile = await getPatientProfileByUserId(userId);
    const id = field(profile, "PatientProfileID");
    if (id) localStorage.setItem("patientProfileId", id);
    return id ?? null;
  } catch (err) {
    console.warn("Couldn't resolve patient profile from userId:", err);
    return null;
  }
}



async function loadAppointments() {
  const tbody = document.getElementById("apptTableBody");
  tbody.innerHTML = `<tr><td colspan="6" style="padding:1.5rem 1rem;text-align:center;color:var(--text-secondary);">Loading appointments…</td></tr>`;

  try {
    const data = currentFilter === "all" ? await getAppointments() : await getAppointmentsByStatus(currentFilter);

    currentAppointments = currentPatientProfileId
      ? data.filter((a) => String(field(a, "PatientProfileID")) === String(currentPatientProfileId))
      : data;

    renderAppointments(currentAppointments);
    await refreshPillCounts();
  } catch (err) {
    console.error("Failed to load appointments:", err);
    tbody.innerHTML = `<tr><td colspan="6" style="padding:1.5rem 1rem;text-align:center;color:#dc2626;">Couldn't load appointments: ${escapeHtml(err.message)}</td></tr>`;
  }
}

function renderAppointments(appointments) {
  const tbody = document.getElementById("apptTableBody");

  if (!appointments.length) {
    tbody.innerHTML = `<tr><td colspan="6" style="padding:1.5rem 1rem;text-align:center;color:var(--text-secondary);">No appointments found.</td></tr>`;
    return;
  }

  tbody.innerHTML = appointments
    .map((a) => {
      const id = field(a, "AppointmentId");
      const status = field(a, "Status") || "Pending";
      const dateTime = formatDateTime(field(a, "AppointmentDateTime"));
      const reason = escapeHtml(field(a, "ReasonForVisit") || "—");
      const roomLabel = escapeHtml(roomLabelFor(field(a, "RoomId")));

      return `
        <tr style="border-bottom: 1px solid var(--border);" data-appt-id="${id}">
          <td style="padding: 0.9rem 1rem; font-weight: 700;">APT-${id}</td>
          <td style="padding: 0.9rem 1rem;">${dateTime}</td>
          <td style="padding: 0.9rem 1rem;">${reason}</td>
          <td style="padding: 0.9rem 1rem;">${roomLabel}</td>
          <td style="padding: 0.9rem 1rem;">${statusBadge(status)}</td>
          <td style="padding: 0.9rem 1rem;">
            <button class="btn btn-secondary" style="padding: 0.3rem 0.6rem; font-size: 0.78rem;" onclick="viewAppointment(${id})">View</button>
            ${
              status !== "Cancelled"
                ? `<button class="btn btn-secondary" style="padding: 0.3rem 0.6rem; font-size: 0.78rem;" onclick="cancelAppointment(${id})">Cancel</button>`
                : ""
            }
          </td>
        </tr>`;
    })
    .join("");
}

function statusBadge(status) {
  if (status === "Confirmed") return `<span class="badge badge-success">Confirmed</span>`;
  if (status === "Pending") return `<span class="badge badge-warn">Pending</span>`;
  return `<span class="badge" style="background:#fef2f2;color:#dc2626;">${escapeHtml(status)}</span>`;
}

async function refreshPillCounts() {
  try {
    const all = await getAppointments();
    const mine = currentPatientProfileId
      ? all.filter((a) => String(field(a, "PatientProfileID")) === String(currentPatientProfileId))
      : all;

    const counts = { all: mine.length, pending: 0, confirmed: 0, cancelled: 0 };
    mine.forEach((a) => {
      const s = (field(a, "Status") || "").toLowerCase();
      if (counts[s] !== undefined) counts[s]++;
    });

    document.querySelectorAll(".pill-btn").forEach((btn) => {
      const key = btn.dataset.filter;
      const badge = btn.querySelector(".pill-count-badge");
      if (key && badge && counts[key] !== undefined) badge.textContent = counts[key];
    });
  } catch (err) {
    console.warn("Couldn't refresh pill counts:", err);
  }
}

// ---------------------------------------------------------------------
// Filtering
// ---------------------------------------------------------------------

function filterAppointments(status, evt) {
  currentFilter = status;

  document.querySelectorAll(".pill-btn").forEach((p) => p.classList.remove("active"));
  const target = evt ? evt.currentTarget : document.querySelector(`.pill-btn[data-filter="${status}"]`);
  if (target) target.classList.add("active");

  loadAppointments();
}

// ---------------------------------------------------------------------
// Rooms (for the booking modal picker)
// ---------------------------------------------------------------------

async function loadRoomOptions() {
  const select = document.getElementById("apptRoomSelect");
  try {
    const rooms = await getAvailableRooms();
    roomsById = {};
    rooms.forEach((r) => (roomsById[field(r, "RoomId")] = r));

    select.innerHTML = rooms
      .map((r) => `<option value="${field(r, "RoomId")}">${escapeHtml(field(r, "RoomNumber"))} — ${escapeHtml(field(r, "Type"))}</option>`)
      .join("");

    if (!rooms.length) select.innerHTML = `<option value="">No rooms currently available</option>`;
  } catch (err) {
    console.error("Failed to load rooms:", err);
    select.innerHTML = `<option value="">Couldn't load rooms</option>`;
  }
}

function roomLabelFor(roomId) {
  const room = roomsById[roomId];
  if (!room) return roomId ? `Room ${roomId}` : "—";
  return `${field(room, "RoomNumber")} (${field(room, "Type")})`;
}

// ---------------------------------------------------------------------
// Book appointment
// ---------------------------------------------------------------------

function openBookModal() {
  document.getElementById("apptReasonInput").value = "";
  document.getElementById("apptDateTimeInput").value = "";
  document.getElementById("bookModal").style.display = "flex";
}

function closeBookModal() {
  document.getElementById("bookModal").style.display = "none";
}

async function confirmBook() {
  if (!currentPatientProfileId) {
    showAlert("Can't book: no logged-in patient profile found. See console for details.", true);
    return;
  }

  const reason = document.getElementById("apptReasonInput").value.trim();
  const dateTimeLocal = document.getElementById("apptDateTimeInput").value; 
  const roomId = document.getElementById("apptRoomSelect").value;

  if (!reason || !dateTimeLocal || !roomId) {
    showAlert("Please fill in the reason, date/time, and room before booking.", true);
    return;
  }

  const payload = {
    PatientProfileID: currentPatientProfileId,
    RoomId: Number(roomId),
    AppointmentDateTime: dateTimeLocal, 
    ReasonForVisit: reason,
    Status: "Pending",
  };

  const confirmBtn = document.querySelector('#bookModal button[onclick="confirmBook()"]');
  if (confirmBtn) confirmBtn.disabled = true;

  try {
    await createAppointment(payload);
    closeBookModal();
    showAlert("Appointment booked — a confirmation email is on its way.");
    loadAppointments();
  } catch (err) {
    console.error("Failed to book appointment:", err);
    showAlert(`Couldn't book the appointment: ${err.message}`, true);
  } finally {
    if (confirmBtn) confirmBtn.disabled = false;
  }
}

// ---------------------------------------------------------------------
// View appointment
// ---------------------------------------------------------------------

async function viewAppointment(id) {
  const modal = document.getElementById("viewApptModal");
  const body = document.getElementById("viewApptBody");
  body.innerHTML = `<p style="color:var(--text-secondary);">Loading…</p>`;
  modal.style.display = "flex";

  try {
    const a = await getAppointmentById(id);
    body.innerHTML = `
      <div style="display:flex;flex-direction:column;gap:0.6rem;">
        <div><strong>Appointment ID:</strong> APT-${field(a, "AppointmentId")}</div>
        <div><strong>Date &amp; Time:</strong> ${formatDateTime(field(a, "AppointmentDateTime"))}</div>
        <div><strong>Reason:</strong> ${escapeHtml(field(a, "ReasonForVisit") || "—")}</div>
        <div><strong>Room:</strong> ${escapeHtml(roomLabelFor(field(a, "RoomId")))}</div>
        <div><strong>Status:</strong> ${statusBadge(field(a, "Status") || "Pending")}</div>
      </div>`;
  } catch (err) {
    body.innerHTML = `<p style="color:#dc2626;">Couldn't load appointment details: ${escapeHtml(err.message)}</p>`;
  }
}

function closeViewApptModal() {
  document.getElementById("viewApptModal").style.display = "none";
}

// ---------------------------------------------------------------------
// Cancel appointment
// ---------------------------------------------------------------------

async function cancelAppointment(id) {
  if (!confirm("Cancel this appointment?")) return;

  try {
    await updateAppointmentStatus(id, "Cancelled");
    showAlert("Appointment cancelled.");
    loadAppointments();
  } catch (err) {
    console.error("Failed to cancel appointment:", err);
    showAlert(`Couldn't cancel the appointment: ${err.message}`, true);
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

function showAlert(message, isError = false) {
  const alertBox = document.getElementById("appt-alert");
  alertBox.querySelector("span").textContent = message;
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

function formatDateTime(raw) {
  if (!raw) return "—";
  const d = new Date(raw);
  if (isNaN(d.getTime())) return escapeHtml(raw);
  return d.toLocaleString(undefined, {
    year: "numeric",
    month: "short",
    day: "numeric",
    hour: "2-digit",
    minute: "2-digit",
  });
}

function escapeHtml(str) {
  return String(str).replace(/[&<>"']/g, (c) => ({ "&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;", "'": "&#39;" }[c]));
}
