using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Team_3_HMS;
using Team_3_HMS.Models;

namespace Team_3_HMS.Controllers
{
    [Route("MedicalRecord")]
    [Route("api/[controller]")]
    [ApiController]
    public class MedicalRecordController : ControllerBase
    {
        private readonly ProjectContext _context;

        public MedicalRecordController(ProjectContext context)
        {
            _context = context;
        }
        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out int uid) ? uid : null;
        }

        private IQueryable<MedicalRecord> GetUserScopedMedicalRecordsQuery()
        {
            var query = _context.MedicalRecords
                .Include(m => m.Appointment)
                    .ThenInclude(a => a!.PatientProfile)
                        .ThenInclude(p => p!.user)
                .Include(m => m.Appointment)
                    .ThenInclude(a => a!.DoctorProfile)
                        .ThenInclude(d => d!.userid)
                .AsQueryable();

            if (User.IsInRole("Admin"))
            {
                return query;
            }

            int? currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return query.Where(m => false);
            }

            if (User.IsInRole("Doctor"))
            {
                return query.Where(m => m.Appointment != null && m.Appointment.DoctorProfile != null && m.Appointment.DoctorProfile.userID == currentUserId.Value);
            }

            if (User.IsInRole("Patient"))
            {
                return query.Where(m => m.Appointment != null && m.Appointment.PatientProfile != null && m.Appointment.PatientProfile.userID == currentUserId.Value);
            }

            return query.Where(m => false);
        }

        // Case 1: 
        [HttpPost]
        [Authorize(Roles = "Doctor,Admin")]
        public async Task<ActionResult<MedicalRecord>> CreateMedicalRecord(
            MedicalRecord medicalRecord)
        {
            var appointment = await _context.Appointments
                .Include(a => a.DoctorProfile)
                .FirstOrDefaultAsync(a => a.AppointmentId == medicalRecord.AppointmentId);

            if (appointment == null)
            {
                return BadRequest("Appointment not found.");
            }

            if (User.IsInRole("Doctor"))
            {
                int? currentUserId = GetCurrentUserId();
                if (!currentUserId.HasValue || appointment.DoctorProfile?.userID != currentUserId.Value)
                {
                    return Forbid();
                }
            }

            _context.MedicalRecords.Add(medicalRecord);
            await _context.SaveChangesAsync();

            return Created($"/api/MedicalRecord/{medicalRecord.MedicalRecordID}", medicalRecord);
        }

        // Case 2: 
        [HttpPut("{id}")]
        [Authorize(Roles = "Doctor,Admin")]
        public async Task<IActionResult> UpdateMedicalRecord(
            int id,
            MedicalRecord updatedMedicalRecord)
        {
            if (id != updatedMedicalRecord.MedicalRecordID)
            {
                return BadRequest("Medical record ID does not match.");
            }

            var existingRecord = await _context.MedicalRecords
                .Include(m => m.Appointment)
                    .ThenInclude(a => a!.DoctorProfile)
                .FirstOrDefaultAsync(m => m.MedicalRecordID == id);

            if (existingRecord == null)
            {
                return NotFound("Medical record not found.");
            }

            if (User.IsInRole("Doctor"))
            {
                int? currentUserId = GetCurrentUserId();
                if (!currentUserId.HasValue || existingRecord.Appointment?.DoctorProfile?.userID != currentUserId.Value)
                {
                    return Forbid();
                }
            }

            var appointmentExists = await _context.Appointments
                .AnyAsync(a => a.AppointmentId == updatedMedicalRecord.AppointmentId);

            if (!appointmentExists)
            {
                return BadRequest("Appointment not found.");
            }

            existingRecord.Diagnosis = updatedMedicalRecord.Diagnosis;
            existingRecord.TreatmentPlan = updatedMedicalRecord.TreatmentPlan;
            existingRecord.Symptom = updatedMedicalRecord.Symptom;
            existingRecord.RecordDate = updatedMedicalRecord.RecordDate;
            existingRecord.AppointmentId = updatedMedicalRecord.AppointmentId;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // Case 3:
        [HttpPatch("{id}/diagnosis")]
        [Authorize(Roles = "Doctor,Admin")]
        public async Task<IActionResult> UpdateDiagnosis(
            int id,
            UpdateDiagnosisRequest request)
        {
            var existingRecord = await _context.MedicalRecords
                .Include(m => m.Appointment)
                    .ThenInclude(a => a!.DoctorProfile)
                .FirstOrDefaultAsync(m => m.MedicalRecordID == id);

            if (existingRecord == null)
            {
                return NotFound("Medical record not found.");
            }

            if (User.IsInRole("Doctor"))
            {
                int? currentUserId = GetCurrentUserId();
                if (!currentUserId.HasValue || existingRecord.Appointment?.DoctorProfile?.userID != currentUserId.Value)
                {
                    return Forbid();
                }
            }

            if (string.IsNullOrWhiteSpace(request.Diagnosis) ||
                string.IsNullOrWhiteSpace(request.TreatmentPlan))
            {
                return BadRequest(
                    "Diagnosis and treatment plan are required.");
            }

            existingRecord.Diagnosis = request.Diagnosis;
            existingRecord.TreatmentPlan = request.TreatmentPlan;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // Case 4: 
        [HttpDelete("{id}")]
        [HttpDelete("delete/{id}")]
        [Authorize(Roles = "Doctor,Admin")]
        public async Task<IActionResult> DeleteMedicalRecord(int id)
        {
            var medicalRecord = await _context.MedicalRecords
                .Include(m => m.Appointment)
                    .ThenInclude(a => a!.DoctorProfile)
                .FirstOrDefaultAsync(m => m.MedicalRecordID == id);

            if (medicalRecord == null)
            {
                return NotFound("Medical record not found.");
            }

            if (User.IsInRole("Doctor"))
            {
                int? currentUserId = GetCurrentUserId();
                if (!currentUserId.HasValue || medicalRecord.Appointment?.DoctorProfile?.userID != currentUserId.Value)
                {
                    return Forbid();
                }
            }

            // 1. Delete associated lab tests
            var labTests = _context.LabTests.Where(l => l.MedicalRecordId == id).ToList();
            _context.LabTests.RemoveRange(labTests);

            // 2. Delete associated prescriptions and their medication links
            var prescriptions = _context.Prescriptions
                .Include(p => p.Medications)
                .Where(p => p.MedicalRecordId == id)
                .ToList();
            foreach (var prescription in prescriptions)
            {
                if (prescription.Medications != null)
                {
                    prescription.Medications.Clear();
                }
            }
            _context.Prescriptions.RemoveRange(prescriptions);

            _context.MedicalRecords.Remove(medicalRecord);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // Case 5: 
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetMedicalRecords()
        {
            var records = await GetUserScopedMedicalRecordsQuery()
                .Select(m => new
                {
                    m.MedicalRecordID,
                    m.Diagnosis,
                    m.TreatmentPlan,
                    m.Symptom,
                    m.RecordDate,
                    m.AppointmentId,

                    Appointment = m.Appointment == null
                        ? null
                        : new
                        {
                            m.Appointment.AppointmentDateTime,
                            m.Appointment.ReasonForVisit,
                            m.Appointment.PatientProfileID,
                            PatientName = m.Appointment.PatientProfile != null && m.Appointment.PatientProfile.user != null ? m.Appointment.PatientProfile.user.Fullname : null,
                            DoctorName = m.Appointment.DoctorProfile != null && m.Appointment.DoctorProfile.userid != null ? m.Appointment.DoctorProfile.userid.Fullname : null
                        }
                })
                .ToListAsync();

            return Ok(records);
        }

        // Case 6: 
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetMedicalRecordById(int id)
        {
            var medicalRecord = await GetUserScopedMedicalRecordsQuery()
                .Where(m => m.MedicalRecordID == id)
                .Select(m => new
                {
                    m.MedicalRecordID,
                    m.Diagnosis,
                    m.TreatmentPlan,
                    m.Symptom,
                    m.RecordDate,
                    m.AppointmentId,

                    Appointment = m.Appointment == null
                        ? null
                        : new
                        {
                            m.Appointment.AppointmentDateTime,
                            m.Appointment.ReasonForVisit,
                            m.Appointment.PatientProfileID,
                            PatientName = m.Appointment.PatientProfile != null && m.Appointment.PatientProfile.user != null ? m.Appointment.PatientProfile.user.Fullname : null,
                            DoctorName = m.Appointment.DoctorProfile != null && m.Appointment.DoctorProfile.userid != null ? m.Appointment.DoctorProfile.userid.Fullname : null
                        }
                })
                .FirstOrDefaultAsync();

            if (medicalRecord == null)
            {
                return NotFound(
                    "Medical record not found or access is not allowed.");
            }

            return Ok(medicalRecord);
        }

        // Case 7: 
        [HttpGet("filter")]
        [Authorize]
        public async Task<IActionResult> FilterMedicalRecords(string diagnosis)
        {
            if (string.IsNullOrWhiteSpace(diagnosis))
            {
                return BadRequest("Diagnosis is required.");
            }

            var records = await GetUserScopedMedicalRecordsQuery()
                .Where(m => m.Diagnosis.Contains(diagnosis))
                .Select(m => new
                {
                    m.MedicalRecordID,
                    m.Diagnosis,
                    m.TreatmentPlan,
                    m.Symptom,
                    m.RecordDate,
                    m.AppointmentId,

                    Appointment = m.Appointment == null
                        ? null
                        : new
                        {
                            m.Appointment.AppointmentDateTime,
                            m.Appointment.ReasonForVisit,
                            m.Appointment.PatientProfileID,
                            PatientName = m.Appointment.PatientProfile != null && m.Appointment.PatientProfile.user != null ? m.Appointment.PatientProfile.user.Fullname : null,
                            DoctorName = m.Appointment.DoctorProfile != null && m.Appointment.DoctorProfile.userid != null ? m.Appointment.DoctorProfile.userid.Fullname : null
                        }
                })
                .ToListAsync();

            if (records.Count == 0)
            {
                return NotFound("No matching medical records were found.");
            }

            return Ok(records);
        }

        // Case 8: 
        [HttpGet("summary")]
        [Authorize]
        public async Task<IActionResult> GetMedicalRecordsSummary()
        {
            var query = GetUserScopedMedicalRecordsQuery();

            var totalRecords = await query.CountAsync();

            var sortedRecords = await query
                .OrderByDescending(m => m.RecordDate)
                .Select(m => new
                {
                    m.MedicalRecordID,
                    m.Diagnosis,
                    m.TreatmentPlan,
                    m.Symptom,
                    m.RecordDate,
                    m.AppointmentId
                })
                .ToListAsync();

            return Ok(new
            {
                TotalRecords = totalRecords,
                Records = sortedRecords
            });
        }
        public class UpdateDiagnosisRequest
        {
            public string Diagnosis { get; set; } = string.Empty;

            public string TreatmentPlan { get; set; } = string.Empty;
        }
    }
}
