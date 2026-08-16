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
        // Case 1: 
        [HttpPost]
        [Authorize(Roles = "Doctor,Admin")]
        public async Task<ActionResult<MedicalRecord>> CreateMedicalRecord(
            MedicalRecord medicalRecord)
        {
            var appointmentExists = await _context.Appointments
                .AnyAsync(a => a.AppointmentId == medicalRecord.AppointmentId);

            if (!appointmentExists)
            {
                return BadRequest("Appointment not found.");
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

            var existingRecord = await _context.MedicalRecords.FindAsync(id);

            if (existingRecord == null)
            {
                return NotFound("Medical record not found.");
            }

            var appointmentExists = await _context.Appointments
                .AnyAsync(a =>
                    a.AppointmentId == updatedMedicalRecord.AppointmentId);

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
            var existingRecord = await _context.MedicalRecords.FindAsync(id);

            if (existingRecord == null)
            {
                return NotFound("Medical record not found.");
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
            var medicalRecord = await _context.MedicalRecords.FindAsync(id);

            if (medicalRecord == null)
            {
                return NotFound("Medical record not found.");
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
            var query = _context.MedicalRecords
                .Include(m => m.Appointment)
                .ThenInclude(a => a!.PatientProfile)
                .AsQueryable();

            // Patients can only view their own medical records
            if (User.IsInRole("Patient"))
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized("User ID was not found in the token.");
                }

                query = query.Where(m =>
                    m.Appointment != null &&
                    m.Appointment.PatientProfile != null &&
                    m.Appointment.PatientProfile.userID == userId);
            }
            else if (!User.IsInRole("Doctor") && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            var records = await query
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
                            m.Appointment.PatientProfileID
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
            var query = _context.MedicalRecords
                .Include(m => m.Appointment)
                .ThenInclude(a => a!.PatientProfile)
                .Where(m => m.MedicalRecordID == id);

            // Patient can only view their own medical record
            if (User.IsInRole("Patient"))
            {
                var userIdClaim =
                    User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(
                        "User ID was not found in the token.");
                }

                query = query.Where(m =>
                    m.Appointment != null &&
                    m.Appointment.PatientProfile != null &&
                    m.Appointment.PatientProfile.userID == userId);
            }
            else if (!User.IsInRole("Doctor") &&
                     !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            var medicalRecord = await query
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
                            m.Appointment.PatientProfileID
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

            var query = _context.MedicalRecords
                .Include(m => m.Appointment)
                .ThenInclude(a => a!.PatientProfile)
                .AsQueryable();

            // Patient can only filter their own medical records
            if (User.IsInRole("Patient"))
            {
                var userIdClaim =
                    User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(
                        "User ID was not found in the token.");
                }

                query = query.Where(m =>
                    m.Appointment != null &&
                    m.Appointment.PatientProfile != null &&
                    m.Appointment.PatientProfile.userID == userId);
            }
            else if (!User.IsInRole("Doctor") &&
                     !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            var records = await query
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
                            m.Appointment.PatientProfileID
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
            var query = _context.MedicalRecords
                .Include(m => m.Appointment)
                .ThenInclude(a => a!.PatientProfile)
                .AsQueryable();

            // Patient can only view the summary of their own records
            if (User.IsInRole("Patient"))
            {
                var userIdClaim =
                    User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(
                        "User ID was not found in the token.");
                }

                query = query.Where(m =>
                    m.Appointment != null &&
                    m.Appointment.PatientProfile != null &&
                    m.Appointment.PatientProfile.userID == userId);
            }
            else if (!User.IsInRole("Doctor") &&
                     !User.IsInRole("Admin"))
            {
                return Forbid();
            }

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
