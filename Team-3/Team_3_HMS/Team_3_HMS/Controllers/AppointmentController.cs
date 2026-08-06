using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Team_3_HMS.Models;

namespace Team_3_HMS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AppointmentController : ControllerBase
    {
        private readonly ProjectContext _context;
        private readonly IEmailService _emailService;

        public AppointmentController(ProjectContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateAppointment([FromBody] Appointment appointment)
        {
            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            var patient = await _context.PatientProfiles
                .Include(p => p.user)
                .FirstOrDefaultAsync(p => p.PatientProfileID == appointment.PatientProfileID);

            if (patient?.user?.email != null)
            {
                await _emailService.SendEmailAsync(
                    patient.user.email,
                    "Appointment Confirmation",
                    $"Your appointment is confirmed for {appointment.AppointmentDateTime} in Room {appointment.RoomId}."
                );
            }

            return CreatedAtAction(nameof(GetAppointment), new { id = appointment.AppointmentId }, appointment);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAppointment(int id, [FromBody] Appointment updated)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null) return NotFound();

            appointment.AppointmentDateTime = updated.AppointmentDateTime;
            appointment.ReasonForVisit = updated.ReasonForVisit;
            appointment.Status = updated.Status;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] string status)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null) return NotFound();

            appointment.Status = status;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAppointment(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null) return NotFound();

            _context.Appointments.Remove(appointment);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet]
        public async Task<IActionResult> GetAppointments()
        {
            var appointments = await _context.Appointments
                .Include(a => a.room)
                .ToListAsync();
            return Ok(appointments);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAppointment(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null) return NotFound();
            return Ok(appointment);
        }

        [HttpGet("by-status/{status}")]
        public async Task<IActionResult> GetByStatus(string status)
        {
            var results = await _context.Appointments
                .Where(a => a.Status == status)
                .ToListAsync();
            return Ok(results);
        }

        [HttpGet("sorted")]
        public async Task<IActionResult> GetSortedByDate()
        {
            var results = await _context.Appointments
                .OrderByDescending(a => a.AppointmentDateTime)
                .ToListAsync();
            return Ok(results);
        }

        [HttpGet("count")]
        public async Task<IActionResult> GetAppointmentCount()
        {
            var count = await _context.Appointments.CountAsync();
            return Ok(count);
        }
    }
}