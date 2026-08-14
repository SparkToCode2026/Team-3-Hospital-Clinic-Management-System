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

        // 0. GET INVOICES (ROLE-AWARE DEFAULT ROUTE)
        // GET: api/Invoice
        [HttpGet]
        public IActionResult GetInvoices()
        {
            var isUserAdmin = User.IsInRole("Admin");
            if (isUserAdmin)
            {
                var allInvoices = _context.Invoices
                    .Include(i => i.Appointment)
                    .ToList();
                return Ok(allInvoices);
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim != null && int.TryParse(userIdClaim, out int currentUserId))
            {
                var patientProfile = _context.PatientProfiles.FirstOrDefault(p => p.userID == currentUserId);
                if (patientProfile != null)
                {
                    var myInvoices = _context.Invoices
                        .Include(i => i.Appointment)
                        .Where(i => i.Appointment != null && i.Appointment.PatientProfileID == patientProfile.PatientProfileID)
                        .ToList();
                    
                    if (myInvoices.Any())
                    {
                        return Ok(myInvoices);
                    }
                }

                // Fallback: return all invoices if no specific appointment mapping is restricting
                return Ok(_context.Invoices.Include(i => i.Appointment).ToList());
            }

            return Ok(_context.Invoices.Include(i => i.Appointment).ToList());
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
        public IActionResult GetById(int id)
        {
            var invoice = _context.Invoices.Find(id);
            if (invoice == null)
            {
                return NotFound("Invoice not found.");
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
        [Authorize(Roles = "Admin")]
        [HttpPost("create")]
        public IActionResult Create([FromBody] Invoice invoice)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Invoices.Add(invoice);
            _context.SaveChanges();

            return Ok(invoice);
        }

        // 5. UPDATE INVOICE BY ID
        // PUT: api/Invoice/update/{id}
        [Authorize(Roles = "Admin")]
        [HttpPut("update/{id}")]
        public IActionResult Update(int id, [FromBody] Invoice updatedData)
        {
            var existing = _context.Invoices.Find(id);
            if (existing == null)
            {
                return NotFound("Invoice not found.");
            }

            existing.TotalAmount = updatedData.TotalAmount;
            existing.Paymentmethod = updatedData.Paymentmethod;
            existing.PaymentStatus = updatedData.PaymentStatus;
            existing.IssuedDate = updatedData.IssuedDate;
            existing.AppointmentID = updatedData.AppointmentID;

            _context.SaveChanges();
            return Ok(existing);
        }

        // 6. GET MY INVOICES
        // GET: api/Invoice/my-invoices
        [HttpGet("my-invoices")]
        public IActionResult GetMyInvoices()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
            {
                return Unauthorized("User ID not found in token.");
            }

            int currentUserId = int.Parse(userIdClaim);

            var myInvoices = _context.Invoices
                .Where(i => i.Appointment != null &&
                            i.Appointment.PatientProfile != null &&
                            i.Appointment.PatientProfile.userID == currentUserId)
                .ToList();

            return Ok(myInvoices);
        }

        // 7. DELETE INVOICE
        // DELETE: api/Invoice/delete/{id}
        [Authorize(Roles = "Admin")]
        [HttpDelete("delete/{id}")]
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