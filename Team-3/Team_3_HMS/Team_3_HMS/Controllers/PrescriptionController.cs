using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Team_3_HMS.Models;

namespace Team_3_HMS.Controllers
{
    [ApiController]
    [Route("Prescription")]
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

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out int uid) ? uid : null;
        }

        private IQueryable<Prescription> GetUserScopedPrescriptionsQuery()
        {
            var query = _context.Prescriptions
                .Include(p => p.Medical)
                    .ThenInclude(m => m!.Appointment)
                        .ThenInclude(a => a!.PatientProfile)
                            .ThenInclude(pt => pt!.user)
                .Include(p => p.Medical)
                    .ThenInclude(m => m!.Appointment)
                        .ThenInclude(a => a!.DoctorProfile)
                            .ThenInclude(d => d!.userid)
                .Include(p => p.Medications)
                .AsQueryable();

            if (User.IsInRole("Admin"))
            {
                return query;
            }

            int? currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return query.Where(p => false);
            }

            if (User.IsInRole("Doctor"))
            {
                return query.Where(p => p.Medical != null && p.Medical.Appointment != null && p.Medical.Appointment.DoctorProfile != null && p.Medical.Appointment.DoctorProfile.userID == currentUserId.Value);
            }

            if (User.IsInRole("Patient"))
            {
                return query.Where(p => p.Medical != null && p.Medical.Appointment != null && p.Medical.Appointment.PatientProfile != null && p.Medical.Appointment.PatientProfile.userID == currentUserId.Value);
            }

            return query.Where(p => false);
        }

        [HttpPost]
        [Authorize(Roles = "Doctor,Admin")]
        public async Task<IActionResult> CreatePrescription([FromBody] Prescription prescription)
        {
            var record = await _context.MedicalRecords
                .Include(m => m.Appointment)
                    .ThenInclude(a => a!.DoctorProfile)
                .Include(m => m.Appointment)
                    .ThenInclude(a => a!.PatientProfile)
                .FirstOrDefaultAsync(m => m.MedicalRecordID == prescription.MedicalRecordId);

            if (record == null)
            {
                return BadRequest("Medical record not found.");
            }

            if (User.IsInRole("Doctor"))
            {
                int? currentUserId = GetCurrentUserId();
                if (!currentUserId.HasValue || record.Appointment?.DoctorProfile?.userID != currentUserId.Value)
                {
                    return Forbid();
                }
            }

            _context.Prescriptions.Add(prescription);
            await _context.SaveChangesAsync();

            if (record.Appointment?.PatientProfile != null)
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
            var prescription = await _context.Prescriptions
                .Include(p => p.Medical)
                    .ThenInclude(m => m!.Appointment)
                        .ThenInclude(a => a!.DoctorProfile)
                .FirstOrDefaultAsync(p => p.PrescriptionID == id);

            if (prescription == null)
                return NotFound("Prescription not found.");

            if (User.IsInRole("Doctor"))
            {
                int? currentUserId = GetCurrentUserId();
                if (!currentUserId.HasValue || prescription.Medical?.Appointment?.DoctorProfile?.userID != currentUserId.Value)
                {
                    return Forbid();
                }
            }

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
            var prescription = await _context.Prescriptions
                .Include(p => p.Medical)
                    .ThenInclude(m => m!.Appointment)
                        .ThenInclude(a => a!.DoctorProfile)
                .FirstOrDefaultAsync(p => p.PrescriptionID == id);

            if (prescription == null)
                return NotFound("Prescription not found.");

            if (User.IsInRole("Doctor"))
            {
                int? currentUserId = GetCurrentUserId();
                if (!currentUserId.HasValue || prescription.Medical?.Appointment?.DoctorProfile?.userID != currentUserId.Value)
                {
                    return Forbid();
                }
            }

            prescription.Notes = notes;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Doctor,Admin")]
        public async Task<IActionResult> DeletePrescription(int id)
        {
            var prescription = await _context.Prescriptions
                .Include(p => p.Medical)
                    .ThenInclude(m => m!.Appointment)
                        .ThenInclude(a => a!.DoctorProfile)
                .FirstOrDefaultAsync(p => p.PrescriptionID == id);

            if (prescription == null)
                return NotFound("Prescription not found.");

            if (User.IsInRole("Doctor"))
            {
                int? currentUserId = GetCurrentUserId();
                if (!currentUserId.HasValue || prescription.Medical?.Appointment?.DoctorProfile?.userID != currentUserId.Value)
                {
                    return Forbid();
                }
            }

            _context.Prescriptions.Remove(prescription);

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet]
        public async Task<IActionResult> GetAllPrescriptions()
        {
            var prescriptions = await GetUserScopedPrescriptionsQuery().ToListAsync();
            return Ok(prescriptions);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPrescription(int id)
        {
            var prescription = await GetUserScopedPrescriptionsQuery().FirstOrDefaultAsync(p => p.PrescriptionID == id);

            if (prescription == null)
                return NotFound("Prescription not found or access denied.");

            return Ok(prescription);
        }

        [HttpGet("search/{date}")]
        public async Task<IActionResult> SearchByDate(string date)
        {
            var prescriptions = await GetUserScopedPrescriptionsQuery()
                .Where(p => p.IssuedDate.Contains(date))
                .ToListAsync();

            return Ok(prescriptions);
        }

        [HttpGet("sort")]
        public async Task<IActionResult> SortByDate()
        {
            var prescriptions = await GetUserScopedPrescriptionsQuery()
                .OrderBy(p => p.IssuedDate)
                .ToListAsync();

            return Ok(prescriptions);
        }

        [HttpGet("count")]
        public async Task<IActionResult> CountPrescriptions()
        {
            var count = await GetUserScopedPrescriptionsQuery().CountAsync();

            return Ok(count);
        }

    }
}



