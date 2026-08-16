using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Team_3_HMS.Models;

namespace Team_3_HMS.Controllers
{
    [ApiController]
    [Route("Appointment")]
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

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out int uid) ? uid : null;
        }

        private IQueryable<Appointment> GetUserScopedAppointmentsQuery()
        {
            var query = _context.Appointments
                .Include(a => a.room)
                .Include(a => a.PatientProfile)
                    .ThenInclude(p => p!.user)
                .Include(a => a.DoctorProfile)
                    .ThenInclude(d => d!.userid)
                .AsQueryable();

            if (User.IsInRole("Admin"))
            {
                return query;
            }

            int? currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return query.Where(a => false);
            }

            if (User.IsInRole("Doctor"))
            {
                return query.Where(a => a.DoctorProfile != null && a.DoctorProfile.userID == currentUserId.Value);
            }

            if (User.IsInRole("Patient"))
            {
                return query.Where(a => a.PatientProfile != null && a.PatientProfile.userID == currentUserId.Value);
            }

            return query.Where(a => false);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAppointment([FromBody] Appointment appointment)
        {
            if (appointment == null)
            {
                return BadRequest("Appointment data is required.");
            }

            if (!DateTime.TryParse(appointment.AppointmentDateTime, out _))
            {
                return BadRequest("AppointmentDateTime must be a valid date/time string, e.g. 2026-08-15T14:30:00");
            }

            int? currentUserId = GetCurrentUserId();

            // 1. Resolve & Validate PatientProfileID
            if (User.IsInRole("Patient"))
            {
                if (!currentUserId.HasValue)
                {
                    return Unauthorized("User ID not found in token.");
                }

                var userPatientProfile = await _context.PatientProfiles.FirstOrDefaultAsync(p => p.userID == currentUserId.Value);
                if (userPatientProfile == null)
                {
                    var user = await _context.Users.FindAsync(currentUserId.Value);
                    userPatientProfile = new PatientProfile
                    {
                        userID = currentUserId.Value,
                        DateOfBirth = "2000-01-01",
                        gender = "Not Specified",
                        BloodGroup = "O+",
                        Address = "Hospital Clinic",
                        emergencyContact = user?.Phone ?? "99999999"
                    };
                    _context.PatientProfiles.Add(userPatientProfile);
                    await _context.SaveChangesAsync();
                }
                appointment.PatientProfileID = userPatientProfile.PatientProfileID;
            }
            else if (appointment.PatientProfileID <= 0 || !_context.PatientProfiles.Any(p => p.PatientProfileID == appointment.PatientProfileID))
            {
                if (currentUserId.HasValue)
                {
                    var userPatientProfile = await _context.PatientProfiles.FirstOrDefaultAsync(p => p.userID == currentUserId.Value);
                    if (userPatientProfile != null)
                    {
                        appointment.PatientProfileID = userPatientProfile.PatientProfileID;
                    }
                }

                if (appointment.PatientProfileID <= 0 || !_context.PatientProfiles.Any(p => p.PatientProfileID == appointment.PatientProfileID))
                {
                    var firstPatient = await _context.PatientProfiles.FirstOrDefaultAsync();
                    if (firstPatient != null)
                    {
                        appointment.PatientProfileID = firstPatient.PatientProfileID;
                    }
                }
            }

            // 2. Resolve & Validate DoctorProfileId
            if (User.IsInRole("Doctor") && currentUserId.HasValue)
            {
                var docProfile = await _context.DoctorProfiles.FirstOrDefaultAsync(d => d.userID == currentUserId.Value);
                if (docProfile != null)
                {
                    appointment.DoctorProfileId = docProfile.DoctorProfileId;
                }
            }
            else if (appointment.DoctorProfileId <= 0 || !_context.DoctorProfiles.Any(d => d.DoctorProfileId == appointment.DoctorProfileId))
            {
                if (currentUserId.HasValue)
                {
                    var docProfile = await _context.DoctorProfiles.FirstOrDefaultAsync(d => d.userID == currentUserId.Value);
                    if (docProfile != null)
                    {
                        appointment.DoctorProfileId = docProfile.DoctorProfileId;
                    }
                }

                if (appointment.DoctorProfileId <= 0 || !_context.DoctorProfiles.Any(d => d.DoctorProfileId == appointment.DoctorProfileId))
                {
                    var firstDoc = await _context.DoctorProfiles.FirstOrDefaultAsync();
                    if (firstDoc != null)
                    {
                        appointment.DoctorProfileId = firstDoc.DoctorProfileId;
                    }
                }
            }

            // 3. Resolve & Validate RoomId
            if (appointment.RoomId <= 0 || !_context.Rooms.Any(r => r.RoomId == appointment.RoomId))
            {
                var firstRoom = await _context.Rooms.FirstOrDefaultAsync();
                if (firstRoom != null)
                {
                    appointment.RoomId = firstRoom.RoomId;
                }
            }

            if (string.IsNullOrWhiteSpace(appointment.Status))
            {
                appointment.Status = "Confirmed";
            }

            if (string.IsNullOrWhiteSpace(appointment.ReasonForVisit))
            {
                appointment.ReasonForVisit = "General Clinical Consultation";
            }

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            // Automatically generate pending invoice for the appointment
            try
            {
                var doc = await _context.DoctorProfiles.FindAsync(appointment.DoctorProfileId);
                double fee = (doc != null && doc.ConsultationFee > 0) ? doc.ConsultationFee : 15.00;

                var initialInvoice = new Invoice
                {
                    AppointmentID = appointment.AppointmentId,
                    TotalAmount = fee,
                    Paymentmethod = "Card",
                    PaymentStatus = "Pending",
                    IssuedDate = DateTime.Now.ToString("yyyy-MM-dd")
                };
                _context.Invoices.Add(initialInvoice);
                await _context.SaveChangesAsync();
            }
            catch (Exception exInv)
            {
                Console.WriteLine($"[AppointmentController Invoice Note]: {exInv.Message}");
            }

            try
            {
                var patient = await _context.PatientProfiles
                    .Include(p => p.user)
                    .FirstOrDefaultAsync(p => p.PatientProfileID == appointment.PatientProfileID);
                var patientUser = patient?.user ?? (patient != null ? await _context.Users.FindAsync(patient.userID) : null);

                var doctor = await _context.DoctorProfiles
                    .Include(d => d.userid)
                    .FirstOrDefaultAsync(d => d.DoctorProfileId == appointment.DoctorProfileId);
                var doctorUser = doctor?.userid ?? (doctor != null ? await _context.Users.FindAsync(doctor.userID) : null);

                var room = await _context.Rooms.FindAsync(appointment.RoomId);
                string roomName = room != null ? $"Room {room.RoomNumber} ({room.Type})" : $"Room #{appointment.RoomId}";
                string patientName = patientUser?.Fullname ?? "Patient";
                string doctorName = doctorUser?.Fullname ?? "Doctor";

                string emailHtml = $@"
<div style=""font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; max-width: 600px; margin: 0 auto; background: #ffffff; border: 1px solid #e2e8f0; border-radius: 12px; overflow: hidden; box-shadow: 0 4px 6px -1px rgba(0,0,0,0.1);"">
  <div style=""background: #2563eb; color: #ffffff; padding: 24px; text-align: center;"">
    <h2 style=""margin: 0; font-size: 22px;"">🏥 MedCore HMS — Appointment Confirmed</h2>
    <p style=""margin: 6px 0 0; opacity: 0.9; font-size: 14px;"">Ref ID: #APT-{appointment.AppointmentId:D4}</p>
  </div>
  <div style=""padding: 24px; color: #334155; line-height: 1.6;"">
    <p style=""font-size: 16px;"">Hello <strong>{patientName}</strong>,</p>
    <p>Your clinical appointment has been scheduled and confirmed in our hospital system. Here are your booking details:</p>
    
    <div style=""background: #f8fafc; border: 1px solid #cbd5e1; border-radius: 8px; padding: 16px; margin: 20px 0;"">
      <table style=""width: 100%; border-collapse: collapse; font-size: 14px;"">
        <tr>
          <td style=""padding: 6px 0; color: #64748b; font-weight: 600;"">📅 Date & Time:</td>
          <td style=""padding: 6px 0; font-weight: 700; color: #0f172a;"">{appointment.AppointmentDateTime}</td>
        </tr>
        <tr>
          <td style=""padding: 6px 0; color: #64748b; font-weight: 600;"">👨‍⚕️ Physician:</td>
          <td style=""padding: 6px 0; font-weight: 600; color: #0f172a;"">{doctorName}</td>
        </tr>
        <tr>
          <td style=""padding: 6px 0; color: #64748b; font-weight: 600;"">🚪 Location:</td>
          <td style=""padding: 6px 0; font-weight: 600; color: #0f172a;"">{roomName}</td>
        </tr>
        <tr>
          <td style=""padding: 6px 0; color: #64748b; font-weight: 600;"">📋 Reason:</td>
          <td style=""padding: 6px 0; color: #0f172a;"">{appointment.ReasonForVisit}</td>
        </tr>
        <tr>
          <td style=""padding: 6px 0; color: #64748b; font-weight: 600;"">⚡ Status:</td>
          <td style=""padding: 6px 0; font-weight: 700; color: #16a34a;"">{appointment.Status}</td>
        </tr>
      </table>
    </div>

    <p style=""font-size: 13px; color: #64748b;"">Please arrive 10 minutes prior to your consultation. If you need to reschedule or cancel, please access the MedCore Patient Portal.</p>
  </div>
  <div style=""background: #f1f5f9; padding: 14px; text-align: center; font-size: 12px; color: #64748b; border-top: 1px solid #e2e8f0;"">
    MedCore Hospital & Clinic Management System &bull; Automated Notification
  </div>
</div>";

                if (!string.IsNullOrWhiteSpace(patientUser?.email))
                {
                    await _emailService.SendEmailAsync(
                        patientUser.email,
                        $"Appointment Confirmation (#APT-{appointment.AppointmentId:D4})",
                        emailHtml
                    );
                }

                if (!string.IsNullOrWhiteSpace(doctorUser?.email) && doctorUser.email != patientUser?.email)
                {
                    string docEmailHtml = emailHtml.Replace($"Hello <strong>{patientName}</strong>", $"Hello <strong>Dr. {doctorName}</strong>");
                    await _emailService.SendEmailAsync(
                        doctorUser.email,
                        $"New Scheduled Appointment (#APT-{appointment.AppointmentId:D4})",
                        docEmailHtml
                    );
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AppointmentController Warning] Simulated email note: {ex.Message}");
            }

            return CreatedAtAction(nameof(GetAppointment), new { id = appointment.AppointmentId }, appointment);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAppointment(int id, [FromBody] Appointment updated)
        {
            if (!DateTime.TryParse(updated.AppointmentDateTime, out _))
            {
                return BadRequest("AppointmentDateTime must be a valid date/time string, e.g. 2026-08-15T14:30:00");
            }

            var appointment = await _context.Appointments
                .Include(a => a.PatientProfile)
                .Include(a => a.DoctorProfile)
                .FirstOrDefaultAsync(a => a.AppointmentId == id);

            if (appointment == null) return NotFound("Appointment not found.");

            if (!User.IsInRole("Admin"))
            {
                int? currentUserId = GetCurrentUserId();
                if (!currentUserId.HasValue) return Unauthorized();

                if (User.IsInRole("Doctor") && appointment.DoctorProfile?.userID != currentUserId.Value)
                {
                    return Forbid();
                }
                if (User.IsInRole("Patient") && appointment.PatientProfile?.userID != currentUserId.Value)
                {
                    return Forbid();
                }
            }

            appointment.AppointmentDateTime = updated.AppointmentDateTime;
            appointment.ReasonForVisit = updated.ReasonForVisit;
            appointment.Status = updated.Status;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] string status)
        {
            var appointment = await _context.Appointments
                .Include(a => a.PatientProfile)
                .Include(a => a.DoctorProfile)
                .FirstOrDefaultAsync(a => a.AppointmentId == id);

            if (appointment == null) return NotFound("Appointment not found.");

            if (!User.IsInRole("Admin"))
            {
                int? currentUserId = GetCurrentUserId();
                if (!currentUserId.HasValue) return Unauthorized();

                if (User.IsInRole("Doctor") && appointment.DoctorProfile?.userID != currentUserId.Value)
                {
                    return Forbid();
                }
                if (User.IsInRole("Patient") && appointment.PatientProfile?.userID != currentUserId.Value)
                {
                    return Forbid();
                }
            }

            appointment.Status = status;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteAppointment(int id)
        {
            var appointment = await _context.Appointments
                .Include(a => a.PatientProfile)
                .Include(a => a.DoctorProfile)
                .FirstOrDefaultAsync(a => a.AppointmentId == id);

            if (appointment == null) return NotFound("Appointment not found.");

            if (!User.IsInRole("Admin"))
            {
                int? currentUserId = GetCurrentUserId();
                if (!currentUserId.HasValue) return Unauthorized();

                if (User.IsInRole("Doctor") && appointment.DoctorProfile?.userID != currentUserId.Value)
                {
                    return Forbid();
                }
                if (User.IsInRole("Patient") && appointment.PatientProfile?.userID != currentUserId.Value)
                {
                    return Forbid();
                }
            }

            // 1. Delete associated invoices
            var invoices = _context.Invoices.Where(i => i.AppointmentID == id).ToList();
            _context.Invoices.RemoveRange(invoices);

            // 2. Delete associated medical records and their child records
            var medicalRecords = _context.MedicalRecords.Where(m => m.AppointmentId == id).ToList();
            if (medicalRecords.Any())
            {
                var medRecordIds = medicalRecords.Select(m => m.MedicalRecordID).ToList();

                var labTests = _context.LabTests.Where(l => medRecordIds.Contains(l.MedicalRecordId)).ToList();
                _context.LabTests.RemoveRange(labTests);

                var prescriptions = _context.Prescriptions
                    .Include(p => p.Medications)
                    .Where(p => medRecordIds.Contains(p.MedicalRecordId))
                    .ToList();
                foreach (var prescription in prescriptions)
                {
                    if (prescription.Medications != null)
                    {
                        prescription.Medications.Clear();
                    }
                }
                _context.Prescriptions.RemoveRange(prescriptions);

                _context.MedicalRecords.RemoveRange(medicalRecords);
            }

            _context.Appointments.Remove(appointment);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet]
        public async Task<IActionResult> GetAppointments()
        {
            var appointments = await GetUserScopedAppointmentsQuery().ToListAsync();
            return Ok(appointments);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAppointment(int id)
        {
            var appointment = await GetUserScopedAppointmentsQuery().FirstOrDefaultAsync(a => a.AppointmentId == id);
            if (appointment == null) return NotFound("Appointment not found or access denied.");
            return Ok(appointment);
        }

        [HttpGet("by-status/{status}")]
        public async Task<IActionResult> GetByStatus(string status)
        {
            var results = await GetUserScopedAppointmentsQuery()
                .Where(a => a.Status == status)
                .ToListAsync();
            return Ok(results);
        }

        [HttpGet("sorted")]
        public async Task<IActionResult> GetSortedByDate()
        {
            var appointments = await GetUserScopedAppointmentsQuery().ToListAsync();

            var sorted = appointments
                .OrderByDescending(a => DateTime.TryParse(a.AppointmentDateTime, out var dt) ? dt : DateTime.MinValue)
                .ToList();

            return Ok(sorted);
        }

        [HttpGet("count")]
        public async Task<IActionResult> GetAppointmentCount()
        {
            var count = await GetUserScopedAppointmentsQuery().CountAsync();
            return Ok(count);
        }
    }

    public class AppointmentReminderService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public AppointmentReminderService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<ProjectContext>();
                    var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

                    var appointments = await context.Appointments
                        .Include(a => a.PatientProfile)
                            .ThenInclude(p => p.user)
                        .ToListAsync();

                    foreach (var appt in appointments)
                    {
                        if (DateTime.TryParse(appt.AppointmentDateTime, out var appointmentDate))
                        {
                            var reminderTime = appointmentDate.AddHours(-24);

                            if (DateTime.Now >= reminderTime && DateTime.Now < reminderTime.AddMinutes(30)
                                && appt.PatientProfile?.user?.email != null)
                            {
                                await emailService.SendEmailAsync(
                                    appt.PatientProfile.user.email,
                                    "Appointment Reminder",
                                    $"Reminder: your appointment is tomorrow at {appointmentDate}."
                                );
                            }
                        }
                    }
                }

                await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
            }
        }
    }
}