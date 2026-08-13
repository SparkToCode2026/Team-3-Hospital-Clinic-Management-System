/* ═══════════════════════════════════════════════════════
   MedCore HMS — Doctor Portal  |  medications.js
   Simple Medication Management Functions
   ═══════════════════════════════════════════════════════ */

// Medication Catalog Data
let medications = [
  { id: 501, name: "Glucophage 500mg", genericName: "Metformin Hydrochloride", dosageForm: "Tablet", unitPrice: 12.50 },
  { id: 502, name: "Synthroid 75mcg", genericName: "Levothyroxine Sodium", dosageForm: "Tablet", unitPrice: 18.20 },
  { id: 503, name: "Lantus SoloStar", genericName: "Insulin Glargine", dosageForm: "Injection", unitPrice: 65.00 },
  { id: 504, name: "Lipitor 20mg", genericName: "Atorvastatin Calcium", dosageForm: "Tablet", unitPrice: 22.00 },
  { id: 505, name: "Amoxil 500mg", genericName: "Amoxicillin Trihydrate", dosageForm: "Capsule", unitPrice: 8.50 },
  { id: 506, name: "Ventolin HFA", genericName: "Albuterol Sulfate", dosageForm: "Inhaler", unitPrice: 22.30 }
];

let selectedMedId = null;

// 1. Render Table Rows
function renderTable() {
  const tbody = document.getElementById('tableBody');
  if (!tbody) return;
  tbody.innerHTML = '';

  medications.forEach(m => {
    let badgeClass = 'badge-blue';
    if (m.dosageForm === 'Injection') badgeClass = 'badge-danger';
    else if (m.dosageForm === 'Capsule') badgeClass = 'badge-warn';
    else if (m.dosageForm === 'Inhaler' || m.dosageForm === 'Syrup') badgeClass = 'badge-success';

    const tr = document.createElement('tr');
    tr.innerHTML = `
      <td><strong>#MED-${m.id}</strong></td>
      <td><strong>${m.name}</strong></td>
      <td>${m.genericName}</td>
      <td><span class="badge ${badgeClass}">${m.dosageForm}</span></td>
      <td><strong>$${m.unitPrice.toFixed(2)}</strong></td>
      <td>
        <button class="btn-ghost me-1" onclick="viewDetails(${m.id})">View</button>
        <button class="btn-ghost me-1" onclick="openPriceModal(${m.id})">Price</button>
        <button class="btn-ghost me-1" onclick="openEditModal(${m.id})">Edit</button>
        <button class="btn-danger-ghost" onclick="deleteMedication(${m.id})">Delete</button>
      </td>
    `;
    tbody.appendChild(tr);
  });

  updateCounts();
}

// 2. Update Stats & Counts
function updateCounts() {
  const totalEl = document.getElementById('countTotal');
  if (totalEl) totalEl.textContent = medications.length;

  if (medications.length > 0) {
    const avg = medications.reduce((sum, item) => sum + item.unitPrice, 0) / medications.length;
    const avgEl = document.getElementById('avgPrice');
    if (avgEl) avgEl.textContent = `$${avg.toFixed(2)}`;
  }
}

// 3. Search and Filter Table
function filterTable() {
  const query = document.getElementById('searchInput').value.toLowerCase();
  const formVal = document.getElementById('formFilter').value;

  const rows = document.querySelectorAll('#tableBody tr');
  let count = 0;

  rows.forEach((row, index) => {
    const m = medications[index];
    if (!m) return;

    const matchesSearch = m.name.toLowerCase().includes(query) || m.genericName.toLowerCase().includes(query);
    const matchesForm = formVal === 'all' || m.dosageForm === formVal;

    if (matchesSearch && matchesForm) {
      row.style.display = '';
      count++;
    } else {
      row.style.display = 'none';
    }
  });

  const infoEl = document.getElementById('paginationInfo');
  if (infoEl) infoEl.textContent = `Showing ${count} of ${medications.length} medications`;
}

// 4. Sort Table by Price
function sortTable() {
  const sortVal = document.getElementById('priceSort').value;
  if (sortVal === 'asc') {
    medications.sort((a, b) => a.unitPrice - b.unitPrice);
  } else if (sortVal === 'desc') {
    medications.sort((a, b) => b.unitPrice - a.unitPrice);
  }
  renderTable();
}

// 5. View Details Offcanvas
function viewDetails(id) {
  const m = medications.find(x => x.id === id);
  if (!m) return;
  selectedMedId = id;

  document.getElementById('detMedId').textContent = `#MED-${m.id}`;
  document.getElementById('detName').textContent = m.name;
  document.getElementById('detGeneric').textContent = m.genericName;
  document.getElementById('detForm').textContent = m.dosageForm;
  document.getElementById('detPrice').textContent = `$${m.unitPrice.toFixed(2)}`;

  new bootstrap.Offcanvas(document.getElementById('detailsOffcanvas')).show();
}

// 6. Quick Price Update Modal
function openPriceModal(id) {
  const m = medications.find(x => x.id === id);
  if (!m) return;
  selectedMedId = id;

  document.getElementById('priceMedId').value = id;
  document.getElementById('priceMedName').value = m.name;
  document.getElementById('newPrice').value = m.unitPrice.toFixed(2);

  new bootstrap.Modal(document.getElementById('priceModal')).show();
}

function handleSavePrice(e) {
  e.preventDefault();
  const id = parseInt(document.getElementById('priceMedId').value);
  const newPrice = parseFloat(document.getElementById('newPrice').value);

  const m = medications.find(x => x.id === id);
  if (m) {
    m.unitPrice = newPrice;
    renderTable();
    showAlert(`Updated price to $${newPrice.toFixed(2)} for ${m.name}.`);
  }
  bootstrap.Modal.getInstance(document.getElementById('priceModal')).hide();
}

// 7. Edit Medication Modal
function openEditModal(id) {
  const m = medications.find(x => x.id === id);
  if (!m) return;
  selectedMedId = id;

  document.getElementById('editMedId').value = id;
  document.getElementById('editName').value = m.name;
  document.getElementById('editGeneric').value = m.genericName;
  document.getElementById('editForm').value = m.dosageForm;
  document.getElementById('editPrice').value = m.unitPrice.toFixed(2);

  new bootstrap.Modal(document.getElementById('editModal')).show();
}

function handleSaveEdit(e) {
  e.preventDefault();
  const id = parseInt(document.getElementById('editMedId').value);
  const m = medications.find(x => x.id === id);
  if (m) {
    m.name = document.getElementById('editName').value;
    m.genericName = document.getElementById('editGeneric').value;
    m.dosageForm = document.getElementById('editForm').value;
    m.unitPrice = parseFloat(document.getElementById('editPrice').value);

    renderTable();
    showAlert(`Updated medication details for ${m.name}.`);
  }
  bootstrap.Modal.getInstance(document.getElementById('editModal')).hide();
}

// 8. Add New Medication
function handleAddMedication(e) {
  e.preventDefault();
  const name = document.getElementById('newName').value;
  const genericName = document.getElementById('newGeneric').value;
  const dosageForm = document.getElementById('newForm').value;
  const unitPrice = parseFloat(document.getElementById('newPriceAdd').value);

  const newId = 500 + medications.length + 1;
  medications.unshift({ id: newId, name, genericName, dosageForm, unitPrice });

  renderTable();
  showAlert(`Added new medication #${newId} (${name}).`);
  bootstrap.Modal.getInstance(document.getElementById('addModal')).hide();
}

// 9. Delete Medication
function deleteMedication(id) {
  const m = medications.find(x => x.id === id);
  if (m && confirm(`Are you sure you want to delete ${m.name}?`)) {
    medications = medications.filter(x => x.id !== id);
    renderTable();
    showAlert(`Deleted medication #${id}.`);
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

// Initial Table Load
renderTable();
