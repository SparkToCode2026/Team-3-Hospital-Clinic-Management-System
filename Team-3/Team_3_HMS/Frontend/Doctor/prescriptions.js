/* ═══════════════════════════════════════════════════════
   HMS Clinical — Prescriptions Client (prescriptions.js)
   ═══════════════════════════════════════════════════════ */

let prescriptionsList = [];
let medicalRecordsList = [];
let deleteTargetRxId = null;

function initDoctorProfile() {
  const user = getAuthUser();
  if (!user || !user.userId) {
    window.location.href = '../Assets/login.html';
    return;
  }
  const name = user.fullname || 'Doctor';
  document.querySelectorAll('.sidebar-user-name, .topbar-user-label').forEach(el => el.textContent = name);
  const roleEl = document.querySelector('.sidebar-user-role');
  if (roleEl) roleEl.textContent = `Doctor · ID ${user.userId}`;
}

async function loadPrescriptions() {
  const tbody = document.getElementById('rxTableBody');
  if (tbody) {
    tbody.innerHTML = '<tr><td colspan="6" class="text-center p-4 text-muted"><i class="bi bi-hourglass-split me-1"></i>Loading patient prescriptions from database...</td></tr>';
  }

  try {
    const data = await apiFetch('/Prescription');
    prescriptionsList = Array.isArray(data) ? data : [];
  } catch (err) {
    console.error('Error fetching prescriptions:', err);
    prescriptionsList = [];
  }

  renderPrescriptions(prescriptionsList);
  updateStats(prescriptionsList);
}

async function loadMedicalRecordsOptions() {
  try {
    const data = await apiFetch('/MedicalRecord');
    medicalRecordsList = Array.isArray(data) ? data : [];
    populateRecordSelect(medicalRecordsList);
  } catch (err) {
    console.log('Note on medical records list:', err.message);
  }
}

function populateRecordSelect(list) {
  const select = document.getElementById('newRecordSelect');
  if (!select) return;
  select.innerHTML = '<option value="">Select a medical record...</option>';

  list.forEach(r => {
    const id = r.medicalRecordID || r.MedicalRecordID || r.id;
    const diag = r.diagnosis || r.Diagnosis || 'General Clinical Record';
    const dt = r.recordDate ? r.recordDate.split('T')[0] : '';
    const opt = document.createElement('option');
    opt.value = id;
    opt.textContent = `#REC-${String(id).padStart(4, '0')} · ${diag} (${dt})`;
    select.appendChild(opt);
  });
}

function onRecordSelectChange(select) {
  if (select.value) {
    document.getElementById('newRecordId').value = select.value;
  }
}

function renderPrescriptions(list) {
  const tbody = document.getElementById('rxTableBody');
  if (!tbody) return;
  tbody.innerHTML = '';

  if (!list || list.length === 0) {
    tbody.innerHTML = '<tr><td colspan="6" class="text-center p-4 text-muted">No prescription records found in database.</td></tr>';
    updateStats([]);
    return;
  }

  list.forEach(rx => {
    const id = rx.prescriptionId || rx.PrescriptionId || rx.id;
    const recId = rx.medicalRecordId || rx.MedicalRecordId || rx.medicalRecordID || 'N/A';
    const rawDate = rx.issuedDate || rx.IssuedDate || '';
    const dt = rawDate ? (rawDate.includes('T') ? rawDate.split('T')[0] : rawDate) : 'N/A';
    const instructions = rx.instructions || rx.Instructions || 'Take as directed.';
    const notes = rx.notes || rx.Notes || '-';

    const tr = document.createElement('tr');
    tr.innerHTML = `
      <td><strong class="rx-tag">#RX-${String(id).padStart(4, '0')}</strong></td>
      <td><strong>#REC-${String(recId).padStart(4, '0')}</strong></td>
      <td><i class="bi bi-calendar3 me-1 text-muted"></i> ${dt}</td>
      <td><div class="rx-instructions">${instructions}</div></td>
      <td><span class="text-muted" style="font-size:0.83rem;">${notes}</span></td>
      <td>
        <div class="d-flex gap-2">
          <button class="btn-ghost" onclick="openEditRxModal(${id})"><i class="bi bi-pencil me-1"></i>Edit</button>
          <button class="btn-ghost text-danger" onclick="promptDeleteRx(${id})"><i class="bi bi-trash me-1"></i>Delete</button>
        </div>
      </td>
    `;
    tbody.appendChild(tr);
  });

  updateStats(list);
}

function updateStats(list) {
  const statTotal = document.getElementById('statTotalRx');
  if (statTotal) statTotal.textContent = list.length;

  const statLinked = document.getElementById('statLinkedRecords');
  if (statLinked) {
    const uniqueRecords = new Set(list.map(r => r.medicalRecordId || r.MedicalRecordId)).size;
    statLinked.textContent = uniqueRecords;
  }

  const paginationInfo = document.getElementById('paginationInfo');
  if (paginationInfo) paginationInfo.textContent = `Showing ${list.length} prescription orders`;
}

function filterPrescriptions() {
  const q = (document.getElementById('searchRxInput')?.value || '').toLowerCase().trim();

  const filtered = prescriptionsList.filter(rx => {
    const id = (rx.prescriptionId || rx.PrescriptionId || rx.id || '').toString();
    const recId = (rx.medicalRecordId || rx.MedicalRecordId || '').toString();
    const instructions = (rx.instructions || rx.Instructions || '').toLowerCase();
    const notes = (rx.notes || rx.Notes || '').toLowerCase();
    const date = (rx.issuedDate || rx.IssuedDate || '').toLowerCase();

    return !q || id.includes(q) || `rx-${id}`.includes(q) || recId.includes(q) || instructions.includes(q) || notes.includes(q) || date.includes(q);
  });

  renderPrescriptions(filtered);
}

function openAddRxModal() {
  document.getElementById('newRxForm')?.reset();
  const dateInput = document.getElementById('newIssuedDate');
  if (dateInput) dateInput.value = new Date().toISOString().split('T')[0];
  new bootstrap.Modal(document.getElementById('addRxModal')).show();
}

async function handleCreatePrescription(e) {
  e.preventDefault();
  const recId = parseInt(document.getElementById('newRecordId').value);
  const issuedDate = document.getElementById('newIssuedDate').value;
  const instructions = document.getElementById('newInstructions').value.trim();
  const notes = document.getElementById('newNotes').value.trim();

  if (isNaN(recId) || !instructions) {
    showAlert('Please select a valid Medical Record and enter prescription instructions.', true);
    return;
  }

  try {
    await apiFetch('/Prescription', {
      method: 'POST',
      body: JSON.stringify({
        MedicalRecordId: recId,
        IssuedDate: issuedDate,
        Instructions: instructions,
        Notes: notes
      })
    });

    showAlert('Prescription created successfully and linked to patient file.');
    bootstrap.Modal.getInstance(document.getElementById('addRxModal')).hide();
    await loadPrescriptions();
  } catch (err) {
    showAlert('Create error: ' + err.message, true);
  }
}

function openEditRxModal(id) {
  const rx = prescriptionsList.find(x => (x.prescriptionId || x.PrescriptionId || x.id) === id);
  if (!rx) return;

  document.getElementById('editRxId').value = id;
  document.getElementById('editRecordId').value = rx.medicalRecordId || rx.MedicalRecordId || '';
  const rawDate = rx.issuedDate || rx.IssuedDate || '';
  document.getElementById('editIssuedDate').value = rawDate.includes('T') ? rawDate.split('T')[0] : rawDate;
  document.getElementById('editInstructions').value = rx.instructions || rx.Instructions || '';
  document.getElementById('editNotes').value = rx.notes || rx.Notes || '';

  new bootstrap.Modal(document.getElementById('editRxModal')).show();
}

async function handleUpdatePrescription(e) {
  e.preventDefault();
  const id = parseInt(document.getElementById('editRxId').value);
  const issuedDate = document.getElementById('editIssuedDate').value;
  const instructions = document.getElementById('editInstructions').value.trim();
  const notes = document.getElementById('editNotes').value.trim();

  try {
    await apiFetch(`/Prescription/${id}`, {
      method: 'PUT',
      body: JSON.stringify({
        PrescriptionId: id,
        IssuedDate: issuedDate,
        Instructions: instructions,
        Notes: notes
      })
    });

    showAlert(`Prescription #RX-${String(id).padStart(4, '0')} updated successfully.`);
    bootstrap.Modal.getInstance(document.getElementById('editRxModal')).hide();
    await loadPrescriptions();
  } catch (err) {
    showAlert('Update error: ' + err.message, true);
  }
}

function promptDeleteRx(id) {
  deleteTargetRxId = id;
  document.getElementById('deleteRxPrompt').textContent = `Are you sure you want to permanently delete prescription #RX-${String(id).padStart(4, '0')}?`;
  new bootstrap.Modal(document.getElementById('deleteRxModal')).show();
}

async function confirmDeletePrescription() {
  if (!deleteTargetRxId) return;

  try {
    await apiFetch(`/Prescription/${deleteTargetRxId}`, { method: 'DELETE' });
    showAlert('Prescription deleted successfully from database.');
    bootstrap.Modal.getInstance(document.getElementById('deleteRxModal')).hide();
    deleteTargetRxId = null;
    await loadPrescriptions();
  } catch (err) {
    showAlert('Delete error: ' + err.message, true);
  }
}

function showAlert(msg, isError = false) {
  const alertBox = document.getElementById('actionAlert');
  if (!alertBox) return;
  const text = document.getElementById('alertMsg');
  if (text) text.textContent = msg;

  if (isError) {
    alertBox.className = 'alert alert-danger';
    alertBox.querySelector('i')?.setAttribute('class', 'bi bi-x-circle-fill me-2');
  } else {
    alertBox.className = 'alert alert-success';
    alertBox.querySelector('i')?.setAttribute('class', 'bi bi-check-circle-fill me-2');
  }

  alertBox.classList.remove('d-none');
  window.scrollTo({ top: 0, behavior: 'smooth' });
  setTimeout(() => alertBox.classList.add('d-none'), 4000);
}

document.addEventListener('DOMContentLoaded', () => {
  initDoctorProfile();
  loadPrescriptions();
  loadMedicalRecordsOptions();
});
