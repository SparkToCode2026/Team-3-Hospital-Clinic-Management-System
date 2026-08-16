using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Team_3_HMS.Models;

namespace Team_3_HMS.Controllers
{
    [Route("PatientProfile")]
    [Route("api/[controller]")]
    [ApiController]
    public class PatientProfileController : ControllerBase
    {
        private readonly ProjectContext _context;

        public PatientProfileController(ProjectContext context)
        {
            _context = context;
        }

        // 1. GET ALL PROFILES
        // GET: api/PatientProfile/all
        [Authorize(Roles = "Admin,Doctor")]
        [HttpGet("all")]
        public IActionResult GetAll()
        {
            var profiles = _context.PatientProfiles
                .Include(p => p.user)
                .ToList();
            return Ok(profiles);
        }


        // 2. FIND PROFILE BY PATIENT ID
        // GET: api/PatientProfile/find/1
        [Authorize]
        [HttpGet("find/{id}")]
        public IActionResult GetById(int id)
        {
            var profile = _context.PatientProfiles.Find(id);

            if (profile == null)
            {
                return NotFound("Patient profile not found.");
            }

            return Ok(profile);
        }

        // 3. FIND PROFILE BY USER ID
        // GET: api/PatientProfile/user/1
        [Authorize]
        [HttpGet("user/{userId}")]
        public IActionResult GetByUserId(int userId)
        {
            var profile = _context.PatientProfiles.FirstOrDefault(p => p.userID == userId);

            if (profile == null)
            {
                return NotFound("Patient profile not found for this user.");
            }

            return Ok(profile);
        }

        // 4. CREATE PATIENT PROFILE
        // POST: api/PatientProfile/create
        [Authorize]
        [HttpPost("create")]
        public IActionResult Create([FromBody] PatientProfile profile)
        {
            _context.PatientProfiles.Add(profile);
            _context.SaveChanges();

            return Ok("Patient profile created successfully.");
        }


        // 5. UPDATE PATIENT PROFILE BY ID
        // PUT: api/PatientProfile/update/1
        [Authorize]
        [HttpPut("update/{id}")]
        public IActionResult Update(int id, [FromBody] PatientProfile updatedData)
        {
            var existing = _context.PatientProfiles.Find(id);

            if (existing == null)
            {
                return NotFound("Patient profile not found.");
            }

            existing.DateOfBirth = updatedData.DateOfBirth;
            existing.gender = updatedData.gender;
            existing.Address = updatedData.Address;
            existing.emergencyContact = updatedData.emergencyContact;
            existing.BloodGroup = updatedData.BloodGroup;

            _context.SaveChanges();

            return Ok("Patient profile updated successfully.");
        }

        // 6. UPDATE CURRENT LOGGED-IN USER'S PROFILE
        // PUT: api/PatientProfile/update-my-profile
        [Authorize]
        [HttpPut("update-my-profile")]
        public IActionResult UpdateMyProfile([FromBody] PatientProfile updatedData)
        {
            // Extract the user ID directly from the logged-in user's JWT token
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized("Unable to read user ID from token.");
            }

            int currentUserId = int.Parse(userIdClaim);
            var existing = _context.PatientProfiles.FirstOrDefault(p => p.userID == currentUserId);

            if (existing == null)
            {
                return NotFound("Your patient profile was not found.");
            }

            existing.DateOfBirth = updatedData.DateOfBirth;
            existing.gender = updatedData.gender;
            existing.Address = updatedData.Address;
            existing.emergencyContact = updatedData.emergencyContact;
            existing.BloodGroup = updatedData.BloodGroup;

            _context.SaveChanges();

            return Ok("Your patient profile was updated successfully.");
        }

        // 7. DELETE PATIENT PROFILE
        // DELETE: api/PatientProfile/delete/1
        [Authorize(Roles = "Admin")]
        [HttpDelete("delete/{id}")]
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var profile = _context.PatientProfiles.Find(id);

            if (profile == null)
            {
                return NotFound("Patient profile not found.");
            }

            // Cascade delete appointments of this patient
            var patientAppointments = _context.Appointments.Where(a => a.PatientProfileID == id).ToList();
            if (patientAppointments.Any())
            {
                var apptIds = patientAppointments.Select(a => a.AppointmentId).ToList();

                var invoices = _context.Invoices.Where(i => apptIds.Contains(i.AppointmentID)).ToList();
                _context.Invoices.RemoveRange(invoices);

                var medicalRecords = _context.MedicalRecords.Where(m => apptIds.Contains(m.AppointmentId)).ToList();
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

                _context.Appointments.RemoveRange(patientAppointments);
            }

            _context.PatientProfiles.Remove(profile);
            _context.SaveChanges();

            return Ok(new { message = "Patient profile deleted successfully." });
        }

        // 8. SEARCH PROFILES BY BLOOD GROUP OR GENDER
        // GET: api/PatientProfile/search?bloodGroup=A+&gender=Male
        [Authorize(Roles = "Doctor")]
        [HttpGet("search")]
        public IActionResult Search(string? bloodGroup, string? gender)
        {
            var query = _context.PatientProfiles.AsQueryable();

            if (!string.IsNullOrEmpty(bloodGroup))
            {
                query = query.Where(p => p.BloodGroup == bloodGroup);
            }

            if (!string.IsNullOrEmpty(gender))
            {
                query = query.Where(p => p.gender == gender);
            }

            var results = query.ToList();
            return Ok(results);
        }
    }
}