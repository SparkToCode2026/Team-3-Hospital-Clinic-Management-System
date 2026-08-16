using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Team_3_HMS.Models;

namespace Team_3_HMS.Controllers
{
    [Route("Invoice")]
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class InvoiceController : ControllerBase
    {
        private readonly ProjectContext _context;

        public InvoiceController(ProjectContext context)
        {
            _context = context;
        }

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out int uid) ? uid : null;
        }

        private IQueryable<Invoice> GetUserScopedInvoicesQuery()
        {
            var query = _context.Invoices
                .Include(i => i.Appointment)
                    .ThenInclude(a => a!.PatientProfile)
                        .ThenInclude(p => p!.user)
                .Include(i => i.Appointment)
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
                return query.Where(i => false);
            }

            if (User.IsInRole("Doctor"))
            {
                return query.Where(i => i.Appointment != null && i.Appointment.DoctorProfile != null && i.Appointment.DoctorProfile.userID == currentUserId.Value);
            }

            if (User.IsInRole("Patient"))
            {
                return query.Where(i => i.Appointment != null && i.Appointment.PatientProfile != null && i.Appointment.PatientProfile.userID == currentUserId.Value);
            }

            return query.Where(i => false);
        }

        // 0. GET INVOICES (ROLE-AWARE DEFAULT ROUTE)
        // GET: api/Invoice
        [HttpGet]
        public IActionResult GetInvoices()
        {
            if (User.IsInRole("Admin"))
            {
                var allInvoices = GetUserScopedInvoicesQuery().ToList();
                return Ok(allInvoices);
            }

            int? currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return Unauthorized("User ID not found in token.");
            }

            if (User.IsInRole("Patient"))
            {
                var patientProfile = _context.PatientProfiles.FirstOrDefault(p => p.userID == currentUserId.Value);
                if (patientProfile != null)
                {
                    // Ensure any appointments without an invoice get an invoice created automatically
                    var apptsWithoutInvoice = _context.Appointments
                        .Where(a => a.PatientProfileID == patientProfile.PatientProfileID && !_context.Invoices.Any(i => i.AppointmentID == a.AppointmentId))
                        .ToList();

                    foreach (var appt in apptsWithoutInvoice)
                    {
                        var doc = _context.DoctorProfiles.Find(appt.DoctorProfileId);
                        _context.Invoices.Add(new Invoice
                        {
                            AppointmentID = appt.AppointmentId,
                            TotalAmount = doc?.ConsultationFee > 0 ? doc.ConsultationFee : 15.00,
                            Paymentmethod = "Card",
                            PaymentStatus = "Pending",
                            IssuedDate = DateTime.Now.ToString("yyyy-MM-dd")
                        });
                    }

                    if (apptsWithoutInvoice.Any())
                    {
                        _context.SaveChanges();
                    }
                }
            }

            var invoices = GetUserScopedInvoicesQuery().ToList();
            return Ok(invoices);
        }

        // 1. GET ALL INVOICES
        // GET: api/Invoice/all
        [Authorize(Roles = "Admin")]
        [HttpGet("all")]
        public IActionResult GetAll()
        {
            var invoices = _context.Invoices
                .Include(i => i.Appointment)
                .ToList();
            return Ok(invoices);
        }

        // 2. FIND INVOICE BY ID
        // GET: api/Invoice/find/{id}
        [HttpGet("find/{id}")]
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var invoice = GetUserScopedInvoicesQuery().FirstOrDefault(i => i.InvoiceID == id);
            if (invoice == null)
            {
                return NotFound("Invoice not found or access denied.");
            }
            return Ok(invoice);
        }

        // 3. FIND INVOICES BY USER ID
        // GET: api/Invoice/user/{userId}
        [Authorize(Roles = "Admin")]
        [HttpGet("user/{userId}")]
        public IActionResult GetByUserId(int userId)
        {
            var invoices = _context.Invoices
                .Where(i => i.Appointment != null &&
                            i.Appointment.PatientProfile != null &&
                            i.Appointment.PatientProfile.userID == userId)
                .ToList();

            return Ok(invoices);
        }

        // 4. CREATE INVOICE
        // POST: api/Invoice/create
        [Authorize(Roles = "Admin,Doctor")]
        [HttpPost("create")]
        public IActionResult Create([FromBody] Invoice invoice)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!_context.Appointments.Any(a => a.AppointmentId == invoice.AppointmentID))
            {
                return BadRequest("Appointment ID does not exist.");
            }

            if (User.IsInRole("Doctor"))
            {
                int? currentUserId = GetCurrentUserId();
                var appt = _context.Appointments
                    .Include(a => a.DoctorProfile)
                    .FirstOrDefault(a => a.AppointmentId == invoice.AppointmentID);

                if (appt == null || appt.DoctorProfile?.userID != currentUserId)
                {
                    return Forbid();
                }
            }

            _context.Invoices.Add(invoice);
            _context.SaveChanges();

            return Ok(invoice);
        }

        // 5. UPDATE OR PAY INVOICE BY ID
        // PUT: api/Invoice/update/{id}, api/Invoice/{id}, api/Invoice/pay/{id}
        [Authorize]
        [HttpPut("update/{id}")]
        [HttpPut("{id}")]
        [HttpPost("pay/{id}")]
        [HttpPut("pay/{id}")]
        [HttpPatch("pay/{id}")]
        public IActionResult Update(int id, [FromBody] Invoice? updatedData)
        {
            var existing = _context.Invoices
                .Include(i => i.Appointment)
                    .ThenInclude(a => a!.PatientProfile)
                .Include(i => i.Appointment)
                    .ThenInclude(a => a!.DoctorProfile)
                .FirstOrDefault(i => i.InvoiceID == id);

            if (existing == null)
            {
                return NotFound("Invoice not found.");
            }

            if (!User.IsInRole("Admin"))
            {
                int? currentUserId = GetCurrentUserId();
                if (!currentUserId.HasValue) return Unauthorized();

                if (User.IsInRole("Patient"))
                {
                    int patientUserId = existing.Appointment?.PatientProfile?.userID ?? 0;
                    if (patientUserId == 0 && existing.AppointmentID > 0)
                    {
                        var appt = _context.Appointments
                            .Include(a => a.PatientProfile)
                            .FirstOrDefault(a => a.AppointmentId == existing.AppointmentID);
                        patientUserId = appt?.PatientProfile?.userID ?? 0;
                    }

                    if (patientUserId != currentUserId.Value)
                    {
                        return Forbid();
                    }

                    // Patients can only pay their invoice
                    existing.PaymentStatus = "Paid";
                    if (updatedData != null && !string.IsNullOrWhiteSpace(updatedData.Paymentmethod))
                    {
                        existing.Paymentmethod = updatedData.Paymentmethod;
                    }
                    else
                    {
                        existing.Paymentmethod = "Card";
                    }

                    _context.SaveChanges();
                    return Ok(existing);
                }

                if (User.IsInRole("Doctor"))
                {
                    int doctorUserId = existing.Appointment?.DoctorProfile?.userID ?? 0;
                    if (doctorUserId == 0 && existing.AppointmentID > 0)
                    {
                        var appt = _context.Appointments
                            .Include(a => a.DoctorProfile)
                            .FirstOrDefault(a => a.AppointmentId == existing.AppointmentID);
                        doctorUserId = appt?.DoctorProfile?.userID ?? 0;
                    }

                    if (doctorUserId != currentUserId.Value)
                    {
                        return Forbid();
                    }
                }
            }

            if (updatedData != null)
            {
                if (updatedData.TotalAmount >= 0) existing.TotalAmount = updatedData.TotalAmount;
                if (!string.IsNullOrWhiteSpace(updatedData.Paymentmethod)) existing.Paymentmethod = updatedData.Paymentmethod;
                if (!string.IsNullOrWhiteSpace(updatedData.PaymentStatus)) existing.PaymentStatus = updatedData.PaymentStatus;
                if (!string.IsNullOrWhiteSpace(updatedData.IssuedDate)) existing.IssuedDate = updatedData.IssuedDate;
                if (updatedData.AppointmentID > 0 && _context.Appointments.Any(a => a.AppointmentId == updatedData.AppointmentID))
                {
                    existing.AppointmentID = updatedData.AppointmentID;
                }
            }
            else
            {
                existing.PaymentStatus = "Paid";
                existing.Paymentmethod = "Card";
            }

            _context.SaveChanges();
            return Ok(existing);
        }

        // 6. GET MY INVOICES
        // GET: api/Invoice/my-invoices
        [HttpGet("my-invoices")]
        public IActionResult GetMyInvoices()
        {
            return GetInvoices();
        }

        // 7. DELETE INVOICE
        // DELETE: api/Invoice/delete/{id}
        [Authorize(Roles = "Admin")]
        [HttpDelete("delete/{id}")]
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var invoice = _context.Invoices.Find(id);
            if (invoice == null)
            {
                return NotFound("Invoice not found.");
            }

            _context.Invoices.Remove(invoice);
            _context.SaveChanges();
            return Ok("Invoice deleted successfully.");
        }

        // 8. SEARCH INVOICES BY PAYMENT STATUS
        // GET: api/Invoice/search?status=Pending
        [Authorize(Roles = "Admin")]
        [HttpGet("search")]
        public IActionResult Search(string? status)
        {
            var query = _context.Invoices.AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(i => i.PaymentStatus == status);
            }

            var results = query.ToList();
            return Ok(results);
        }
    }
}