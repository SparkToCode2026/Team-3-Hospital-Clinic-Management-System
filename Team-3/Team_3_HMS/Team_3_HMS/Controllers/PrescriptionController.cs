using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Team_3_HMS.Models;

namespace Team_3_HMS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PrescriptionController : ControllerBase
    {
        private readonly ProjectContext _context;
        private readonly IEmailService _emailService;

        public PrescriptionController(ProjectContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        [HttpPost]
        [Authorize(Roles = "Doctor,Admin")]
        public async Task<IActionResult> CreatePrescription([FromBody] Prescription prescription)
        {
            _context.Prescriptions.Add(prescription);
            await _context.SaveChangesAsync();

            var record = await _context.MedicalRecords
                .Include(m => m.Appointment)
                .ThenInclude(a => a.PatientProfile)
                .FirstOrDefaultAsync(m => m.MedicalRecordID == prescription.MedicalRecordId);

            if (record?.Appointment?.PatientProfile != null)
            {
                var patient = await _context.Users.FirstOrDefaultAsync(u =>
                    u.userID == record.Appointment.PatientProfile.userID);

                if (patient != null)
                {
                    await _emailService.SendEmailAsync(
                        patient.email,
                        "Prescription Created",
                        $"Your prescription has been created.\n\nInstructions: {prescription.Instructions}"
                    );
                }
            }

            return Ok(prescription);
        }
        [HttpPut("{id}")]
        [Authorize(Roles = "Doctor,Admin")]
        public async Task<IActionResult> UpdatePrescription(int id, [FromBody] Prescription updated)
        {
            var prescription = await _context.Prescriptions.FindAsync(id);

            if (prescription == null)
                return NotFound();

            prescription.IssuedDate = updated.IssuedDate;
            prescription.Instructions = updated.Instructions;
            prescription.Notes = updated.Notes;

            await _context.SaveChangesAsync();

            return NoContent();
        }
        [HttpPatch("{id}/notes")]
        [Authorize(Roles = "Doctor,Admin")]
        public async Task<IActionResult> UpdateNotes(int id, [FromBody] string notes)
        {
            var prescription = await _context.Prescriptions.FindAsync(id);

            if (prescription == null)
                return NotFound();

            prescription.Notes = notes;

            await _context.SaveChangesAsync();

            return NoContent();
        }
        [HttpDelete("{id}")]
        [Authorize(Roles = "Doctor,Admin")]
        public async Task<IActionResult> DeletePrescription(int id)
        {
            var prescription = await _context.Prescriptions.FindAsync(id);

            if (prescription == null)
                return NotFound();

            _context.Prescriptions.Remove(prescription);

            await _context.SaveChangesAsync();

            return NoContent();
        }
        [HttpGet]
        public async Task<IActionResult> GetAllPrescriptions()
        {
            var prescriptions = await _context.Prescriptions
                .Include(p => p.Medical)
                .Include(p => p.Medications)
                .ToListAsync();

            return Ok(prescriptions);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPrescription(int id)
        {
            var prescription = await _context.Prescriptions.FindAsync(id);

            if (prescription == null)
                return NotFound();

            return Ok(prescription);
        }
        [HttpGet("search/{date}")]
        public async Task<IActionResult> SearchByDate(string date)
        {
            var prescriptions = await _context.Prescriptions
                .Where(p => p.IssuedDate.Contains(date))
                .ToListAsync();

            return Ok(prescriptions);
        }

        [HttpGet("sort")]
        public async Task<IActionResult> SortByDate()
        {
            var prescriptions = await _context.Prescriptions
                .OrderBy(p => p.IssuedDate)
                .ToListAsync();

            return Ok(prescriptions);
        }
        [HttpGet("count")]
        public async Task<IActionResult> CountPrescriptions()
        {
            var count = await _context.Prescriptions.CountAsync();

            return Ok(count);
        }
    }
}