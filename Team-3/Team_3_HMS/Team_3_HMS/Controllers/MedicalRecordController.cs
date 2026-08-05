using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Team_3_HMS.Models;
using Team_3_HMS;

namespace Team_3_HMS.Controllers
{
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

            return Created(
                $"/api/MedicalRecord/{medicalRecord.MedicalRecordID}",
                medicalRecord);
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
        // Case 3: Update diagnosis and treatment plan only
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
        public class UpdateDiagnosisRequest
        {
            public string Diagnosis { get; set; } = string.Empty;

            public string TreatmentPlan { get; set; } = string.Empty;
        }
    }
}
