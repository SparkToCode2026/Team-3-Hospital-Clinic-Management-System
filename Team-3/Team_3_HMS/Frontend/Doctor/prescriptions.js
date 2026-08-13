/* ═══════════════════════════════════════════════════════
   MedCore HMS — Doctor Portal  |  prescriptions.js
   Simple Prescription Management Functions
   ═══════════════════════════════════════════════════════ */

// 1. Available Medications Dictionary for Checkboxes
const availableMeds = [
  { id: 501, name: "Glucophage 500mg" },
  { id: 502, name: "Synthroid 75mcg" },
  { id: 503, name: "Lantus SoloStar" },
  { id: 504, name: "Lipitor 20mg" },
  { id: 505, name: "Amoxil 500mg" },
  { id: 506, name: "Ventolin HFA" }
];

// 2. Prescriptions Dataset
let prescriptions = [
  { id: 701, medicalRecordId: 201, patientName: "Michael Wilson", date: "2026-08-12", instructions: "Take Glucophage 500mg twice daily with meals.", notes: "Monitor blood glucose weekly.", meds: [501, 504] },
  { id: 702, medicalRecordId: 202, patientName: "Lisa Andersen", date: "2026-08-12", instructions: "Take Synthroid 75mcg once daily on empty stomach.", notes: "Re-check TSH in 6 weeks.", meds: [502] },
  { id: 703, medicalRecordId: 203, patientName: "Emily Carter", date: "2026-08-11", instructions: "Inject Lantus SoloStar 12 units at 10:00 PM.", notes: "Log blood sugar daily.", meds: [503] },
  { id: 704, medicalRecordId: 204, patientName: "James Wilson", date: "2026-08-10", instructions: "Take Amoxil 500mg every 8 hours for 7 days.", notes: "Complete full antibiotic course.", meds: [505] },
  { id: 705, medicalRecordId: 205, patientName: "Sarah Jenkins", date: "2026-08-08", instructions: "Inhale Ventolin HFA 2 puffs as needed.", notes: "Keep rescue inhaler accessible.", meds: [506] }
];

let selectedRxId = null;

// Populate Medication Checkboxes in Modal
function populateCheckboxes() {
  const container = document.getElementById('medCheckboxes');
  if (!container) return;
  container.innerHTML = '';
  availableMeds.forEach(m => {
    container.innerHTML += `
      <div class="form-check mb-1">
        <input class="form-check-input rx-med-check" type="checkbox" value="${m.id}" id="chk_${m.id}">
        <label class="form-check-label" for="chk_${m.id}" style="font-size:0.85rem;">${m.name}</label>
      </div>
    `;
  });
}

// Render Table Rows
function renderTable() {
  const tbody = document.getElementById('tableBody');
  if (!tbody) return;
  tbody.innerHTML = '';

  prescriptions.forEach(p => {
    const medBadges = p.meds.map(mId => {
      const med = availableMeds.find(x => x.id === mId);
      return med ? `<span class="badge badge-blue me-1">${med.name}</span>` : '';
    }).join('');

    const tr = document.createElement('tr');
    tr.innerHTML = `
      <td><strong>#RX-${p.id}</strong></td>
      <td><strong>${p.patientName}</strong> <small class="text-muted">(#REC-${p.medicalRecordId})</small></td>
      <td>${p.date}</td>
      <td>${p.instructions}</td>
      <td>${medBadges || '<span class="text-muted">None</span>'}</td>
      <td>
        <button class="btn-ghost me-1" onclick="viewDetails(${p.id})">View</button>
        <button class="btn-ghost me-1" onclick="openNotesModal(${p.id})">Notes</button>
        <button class="btn-ghost me-1" onclick="openEditModal(${p.id})">Edit</button>
        <button class="btn-danger-ghost" onclick="deletePrescription(${p.id})">Delete</button>
      </td>
    `;
    tbody.appendChild(tr);
  });

  updateCounts();
}

function updateCounts() {
  const totalEl = document.getElementById('countTotal');
  if (totalEl) totalEl.textContent = prescriptions.length;
}

// Filter Table by Search or Date
function filterTable() {
  const query = document.getElementById('searchInput').value.toLowerCase();
  const dateVal = document.getElementById('dateFilter').value;

  const rows = document.querySelectorAll('#tableBody tr');
  let count = 0;

  rows.forEach((row, index) => {
    const p = prescriptions[index];
    if (!p) return;

    const matchesSearch = p.patientName.toLowerCase().includes(query) || p.instructions.toLowerCase().includes(query);
    const matchesDate = !dateVal || p.date === dateVal;

    if (matchesSearch && matchesDate) {
      row.style.display = '';
      count++;
    } else {
      row.style.display = 'none';
    }
  });

  const infoEl = document.getElementById('paginationInfo');
  if (infoEl) infoEl.textContent = `Showing ${count} of ${prescriptions.length} prescriptions`;
}

// Sort Table by Issued Date
function sortTable() {
  const sortVal = document.getElementById('dateSort').value;
  if (sortVal === 'asc') {
    prescriptions.sort((a, b) => new Date(a.date) - new Date(b.date));
  } else if (sortVal === 'desc') {
    prescriptions.sort((a, b) => new Date(b.date) - new Date(a.date));
  }
  renderTable();
}

// View Details Offcanvas
function viewDetails(id) {
  const p = prescriptions.find(x => x.id === id);
  if (!p) return;
  selectedRxId = id;

  document.getElementById('detRxId').textContent = `#RX-${p.id}`;
  document.getElementById('detPatient').textContent = `${p.patientName} (#REC-${p.medicalRecordId})`;
  document.getElementById('detDate').textContent = p.date;
  document.getElementById('detInstructions').textContent = p.instructions;
  document.getElementById('detNotes').textContent = p.notes || 'None';

  new bootstrap.Offcanvas(document.getElementById('detailsOffcanvas')).show();
}

// Update Notes Modal (PATCH)
function openNotesModal(id) {
  const p = prescriptions.find(x => x.id === id);
  if (!p) return;
  selectedRxId = id;

  document.getElementById('notesRxId').value = id;
  document.getElementById('notesPatient').value = `#RX-${p.id} - ${p.patientName}`;
  document.getElementById('newNotes').value = p.notes;

  new bootstrap.Modal(document.getElementById('notesModal')).show();
}

function handleSaveNotes(e) {
  e.preventDefault();
  const id = parseInt(document.getElementById('notesRxId').value);
  const newNotes = document.getElementById('newNotes').value;

  const p = prescriptions.find(x => x.id === id);
  if (p) {
    p.notes = newNotes;
    renderTable();
    showAlert(`Updated clinical notes for ${p.patientName}.`);
  }
  bootstrap.Modal.getInstance(document.getElementById('notesModal')).hide();
}

// Edit Prescription Modal (PUT)
function openEditModal(id) {
  const p = prescriptions.find(x => x.id === id);
  if (!p) return;
  selectedRxId = id;

  document.getElementById('editRxId').value = id;
  document.getElementById('editDate').value = p.date;
  document.getElementById('editInstructions').value = p.instructions;
  document.getElementById('editNotes').value = p.notes;

  new bootstrap.Modal(document.getElementById('editModal')).show();
}

function handleSaveEdit(e) {
  e.preventDefault();
  const id = parseInt(document.getElementById('editRxId').value);
  const p = prescriptions.find(x => x.id === id);

  if (p) {
    p.date = document.getElementById('editDate').value;
    p.instructions = document.getElementById('editInstructions').value;
    p.notes = document.getElementById('editNotes').value;

    renderTable();
    showAlert(`Updated prescription #${id}.`);
  }
  bootstrap.Modal.getInstance(document.getElementById('editModal')).hide();
}

// Add New Prescription
function handleAddPrescription(e) {
  e.preventDefault();
  const recSelect = document.getElementById('newRecord');
  const medRecId = parseInt(recSelect.value);
  const patientName = recSelect.options[recSelect.selectedIndex].getAttribute('data-patient');

  const date = document.getElementById('newDate').value;
  const instructions = document.getElementById('newInstructions').value;
  const notes = document.getElementById('newNotesAdd').value;

  const selectedMeds = [];
  document.querySelectorAll('.rx-med-check:checked').forEach(cb => {
    selectedMeds.push(parseInt(cb.value));
  });

  const newId = 700 + prescriptions.length + 1;
  prescriptions.unshift({
    id: newId,
    medicalRecordId: medRecId,
    patientName: patientName,
    date: date,
    instructions: instructions,
    notes: notes,
    meds: selectedMeds
  });

  renderTable();
  showAlert(`Issued prescription #RX-${newId} for ${patientName}. Email sent.`);
  bootstrap.Modal.getInstance(document.getElementById('addModal')).hide();
}

// Delete Prescription
function deletePrescription(id) {
  const p = prescriptions.find(x => x.id === id);
  if (p && confirm(`Are you sure you want to delete prescription #RX-${id}?`)) {
    prescriptions = prescriptions.filter(x => x.id !== id);
    renderTable();
    showAlert(`Deleted prescription #RX-${id}.`);
  }
}

// Helper: Show Alert Toast
function showAlert(msg) {
  const alertBox = document.getElementById('actionAlert');
  if (!alertBox) return;
  document.getElementById('alertMsg').textContent = msg;
  alertBox.classList.remove('d-none');
  setTimeout(() => alertBox.classList.add('d-none'), 3500);
}

// Initial Load
document.addEventListener('DOMContentLoaded', () => {
  const dtInput = document.getElementById('newDate');
  if (dtInput) dtInput.value = new Date().toISOString().split('T')[0];
  populateCheckboxes();
  renderTable();
});
