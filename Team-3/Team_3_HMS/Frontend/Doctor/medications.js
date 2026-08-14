/* ═══════════════════════════════════════════════════════
   HMS Clinical — Medications Formulary Client (medications.js)
   ═══════════════════════════════════════════════════════ */

let medicationsList = [];
let deleteTargetMedId = null;

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

async function loadMedications() {
  const tbody = document.getElementById('medTableBody');
  if (tbody) {
    tbody.innerHTML = '<tr><td colspan="6" class="text-center p-4 text-muted"><i class="bi bi-hourglass-split me-1"></i>Loading medications formulary from database...</td></tr>';
  }

  try {
    const data = await apiFetch('/Medication');
    medicationsList = Array.isArray(data) ? data : [];
  } catch (err) {
    console.error('Error fetching medications:', err);
    medicationsList = [];
  }

  renderMedications(medicationsList);
  updateStats(medicationsList);
}

function renderMedications(list) {
  const tbody = document.getElementById('medTableBody');
  if (!tbody) return;
  tbody.innerHTML = '';

  if (!list || list.length === 0) {
    tbody.innerHTML = '<tr><td colspan="6" class="text-center p-4 text-muted">No medications found in the hospital formulary.</td></tr>';
    updateStats([]);
    return;
  }

  list.forEach(m => {
    const id = m.medicationId || m.MedicationId || m.id;
    const name = m.name || m.Name || 'N/A';
    const generic = m.genericName || m.GenericName || '-';
    const dosage = m.dosageForm || m.DosageForm || 'Tablet';
    const price = parseFloat(m.unitPrice || m.UnitPrice || 0).toFixed(2);

    const tr = document.createElement('tr');
    tr.innerHTML = `
      <td><strong>#MED-${String(id).padStart(4, '0')}</strong></td>
      <td>
        <strong style="color:var(--text-primary);">${name}</strong>
      </td>
      <td><span class="generic-name">${generic}</span></td>
      <td><span class="badge-dosage">${dosage}</span></td>
      <td><span class="price-tag">OMR ${price}</span></td>
      <td>
        <div class="d-flex gap-2">
          <button class="btn-ghost" onclick="openEditMedModal(${id})"><i class="bi bi-pencil me-1"></i>Edit</button>
          <button class="btn-ghost text-danger" onclick="promptDeleteMed(${id}, '${name.replace(/'/g, "\\'")}')"><i class="bi bi-trash me-1"></i>Delete</button>
        </div>
      </td>
    `;
    tbody.appendChild(tr);
  });

  updateStats(list);
}

function updateStats(list) {
  const statTotal = document.getElementById('statTotalMeds');
  if (statTotal) statTotal.textContent = list.length;

  const statAvg = document.getElementById('statAvgPrice');
  if (statAvg) {
    const sum = list.reduce((acc, m) => acc + parseFloat(m.unitPrice || m.UnitPrice || 0), 0);
    const avg = list.length > 0 ? (sum / list.length).toFixed(2) : '0.00';
    statAvg.textContent = `OMR ${avg}`;
  }

  const paginationInfo = document.getElementById('paginationInfo');
  if (paginationInfo) paginationInfo.textContent = `Showing ${list.length} formulary medications`;
}

function filterMedications() {
  const q = (document.getElementById('searchMedInput')?.value || '').toLowerCase().trim();
  const dosageVal = document.getElementById('dosageFilter')?.value || 'all';

  const filtered = medicationsList.filter(m => {
    const name = (m.name || m.Name || '').toLowerCase();
    const generic = (m.genericName || m.GenericName || '').toLowerCase();
    const dosage = (m.dosageForm || m.DosageForm || '').toLowerCase();
    const id = (m.medicationId || m.MedicationId || m.id || '').toString();

    const matchesQuery = !q || name.includes(q) || generic.includes(q) || id.includes(q);
    const matchesDosage = dosageVal === 'all' || dosage.includes(dosageVal.toLowerCase());

    return matchesQuery && matchesDosage;
  });

  renderMedications(filtered);
}

function sortMedicationsByPrice(dir) {
  const sorted = [...medicationsList].sort((a, b) => {
    const pa = parseFloat(a.unitPrice || a.UnitPrice || 0);
    const pb = parseFloat(b.unitPrice || b.UnitPrice || 0);
    return dir === 'asc' ? pa - pb : pb - pa;
  });
  renderMedications(sorted);
}

function openAddMedModal() {
  document.getElementById('newMedForm')?.reset();
  new bootstrap.Modal(document.getElementById('addMedModal')).show();
}

async function handleCreateMedication(e) {
  e.preventDefault();
  const name = document.getElementById('newMedName').value.trim();
  const generic = document.getElementById('newGenericName').value.trim();
  const dosage = document.getElementById('newDosageForm').value.trim();
  const price = parseFloat(document.getElementById('newUnitPrice').value);

  if (!name || isNaN(price)) {
    showAlert('Please provide a valid medication name and unit price.', true);
    return;
  }

  try {
    await apiFetch('/Medication', {
      method: 'POST',
      body: JSON.stringify({
        Name: name,
        GenericName: generic,
        DosageForm: dosage,
        UnitPrice: price
      })
    });

    showAlert(`Medication "${name}" added to formulary successfully.`);
    bootstrap.Modal.getInstance(document.getElementById('addMedModal')).hide();
    await loadMedications();
  } catch (err) {
    showAlert('Create error: ' + err.message, true);
  }
}

function openEditMedModal(id) {
  const m = medicationsList.find(x => (x.medicationId || x.MedicationId || x.id) === id);
  if (!m) return;

  document.getElementById('editMedId').value = id;
  document.getElementById('editMedName').value = m.name || m.Name || '';
  document.getElementById('editGenericName').value = m.genericName || m.GenericName || '';
  document.getElementById('editDosageForm').value = m.dosageForm || m.DosageForm || 'Tablet';
  document.getElementById('editUnitPrice').value = parseFloat(m.unitPrice || m.UnitPrice || 0).toFixed(2);

  new bootstrap.Modal(document.getElementById('editMedModal')).show();
}

async function handleUpdateMedication(e) {
  e.preventDefault();
  const id = parseInt(document.getElementById('editMedId').value);
  const name = document.getElementById('editMedName').value.trim();
  const generic = document.getElementById('editGenericName').value.trim();
  const dosage = document.getElementById('editDosageForm').value.trim();
  const price = parseFloat(document.getElementById('editUnitPrice').value);

  try {
    await apiFetch(`/Medication/${id}`, {
      method: 'PUT',
      body: JSON.stringify({
        MedicationId: id,
        Name: name,
        GenericName: generic,
        DosageForm: dosage,
        UnitPrice: price
      })
    });

    showAlert(`Medication #${id} updated successfully.`);
    bootstrap.Modal.getInstance(document.getElementById('editMedModal')).hide();
    await loadMedications();
  } catch (err) {
    showAlert('Update error: ' + err.message, true);
  }
}

function promptDeleteMed(id, name) {
  deleteTargetMedId = id;
  document.getElementById('deleteMedPrompt').textContent = `Are you sure you want to remove "${name}" (#MED-${String(id).padStart(4, '0')}) from the hospital medications formulary?`;
  new bootstrap.Modal(document.getElementById('deleteMedModal')).show();
}

async function confirmDeleteMedication() {
  if (!deleteTargetMedId) return;

  try {
    await apiFetch(`/Medication/${deleteTargetMedId}`, { method: 'DELETE' });
    showAlert('Medication deleted successfully from database.');
    bootstrap.Modal.getInstance(document.getElementById('deleteMedModal')).hide();
    deleteTargetMedId = null;
    await loadMedications();
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
  loadMedications();
});
