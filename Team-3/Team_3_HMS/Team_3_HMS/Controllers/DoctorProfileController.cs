using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Team_3_HMS.Models;

namespace Team_3_HMS.Controllers
{
    [ApiController]
    [Route("DoctorProfile")]
    [Route("api/[controller]")]
    public class DoctorProfileController : ControllerBase
    {
        private ProjectContext context;

        public DoctorProfileController(ProjectContext _context)
        {
            context = _context;
        }
        // Method: POST create new doctor Profile
        [Authorize(Roles = "Admin,Doctor")]
        [HttpPost("AddDoctorProfile")]
        public IActionResult AddDoctorProfile(DoctorProfile doctor)
        {
            context.DoctorProfiles.Add(doctor);

            context.SaveChanges();

            return Ok(new
            {
                message = "Doctor profile added successfully",
                doctor = doctor
            });
        }
        // Method: PUT update all doctor profile details
        [Authorize(Roles = "Admin,Doctor")]
        [HttpPut("UpdateDoctorProfile")]
        [HttpPut("UpdateDoctorProfile/{id}")]
        public IActionResult UpdateDoctorProfile([FromQuery] int? id, [FromBody] DoctorProfile updatedDoctor)
        {
            int targetId = id.HasValue && id.Value > 0 ? id.Value : updatedDoctor.DoctorProfileId;
            DoctorProfile? doctor = null;
            if (targetId > 0)
            {
                doctor = context.DoctorProfiles.FirstOrDefault(d => d.DoctorProfileId == targetId);
            }
            if (doctor == null && updatedDoctor.userID > 0)
            {
                doctor = context.DoctorProfiles.FirstOrDefault(d => d.userID == updatedDoctor.userID);
            }
            if (doctor == null)
            {
                return NotFound("Doctor profile not found");
            }
            doctor.LicenseNumber = updatedDoctor.LicenseNumber;
            doctor.YearsOfExperience = updatedDoctor.YearsOfExperience;
            doctor.ConsultationFee = updatedDoctor.ConsultationFee;
            if (updatedDoctor.userID > 0) doctor.userID = updatedDoctor.userID;
            if (updatedDoctor.SpecializationId > 0) doctor.SpecializationId = updatedDoctor.SpecializationId;

            context.SaveChanges();
            return Ok(new
            {
                message = "Doctor profile updated successfully",
                updatedDoctor = doctor
            });
        }
        // Method: PATCH Update one field (Consultation Fee)
        [Authorize(Roles = "Admin,Doctor")]
        [HttpPatch("UpdateConsultationFee")]
        [HttpPatch("UpdateConsultationFee/{id}")]
        public IActionResult UpdateConsultationFee([FromQuery] int? id, [FromQuery] double? newFee, [FromRoute] int? routeId)
        {
            int targetId = id ?? routeId ?? 0;
            double fee = newFee ?? 0;
            DoctorProfile? doctor = context.DoctorProfiles
               .FirstOrDefault(d => d.DoctorProfileId == targetId);

            if (doctor == null)
            {
                return NotFound("Doctor profile not found");
            }
            // Change only the consultation fee
            doctor.ConsultationFee = fee;

            context.SaveChanges();
            return Ok(new
            {
                message = "Consultation fee updated successfully",
                updatedDoctor = doctor
            });
        }
        // Method: DELETE to remove doctor profile by ID
        [Authorize(Roles = "Admin")]
        [HttpDelete("RemoveDoctorProfile")]
        [HttpDelete("RemoveDoctorProfile/{id}")]
        [HttpDelete("delete/{id}")]
        [HttpDelete("{id}")]
        public IActionResult RemoveDoctorProfile([FromQuery] int? id, [FromRoute(Name = "id")] int? routeId)
        {
            int targetId = (id.HasValue && id.Value > 0) ? id.Value : (routeId ?? 0);
            DoctorProfile? doctor = context.DoctorProfiles
                .Include(d => d.specializations)
                .FirstOrDefault(d => d.DoctorProfileId == targetId);

            if (doctor == null)
            {
                return NotFound("Doctor profile not found");
            }

            var deletedDoctor = doctor;

            // 1. Delete all appointments for this doctor and their cascade dependencies
            var doctorAppointments = context.Appointments.Where(a => a.DoctorProfileId == targetId).ToList();
            if (doctorAppointments.Any())
            {
                var apptIds = doctorAppointments.Select(a => a.AppointmentId).ToList();

                var invoices = context.Invoices.Where(i => apptIds.Contains(i.AppointmentID)).ToList();
                context.Invoices.RemoveRange(invoices);

                var medicalRecords = context.MedicalRecords.Where(m => apptIds.Contains(m.AppointmentId)).ToList();
                if (medicalRecords.Any())
                {
                    var medRecordIds = medicalRecords.Select(m => m.MedicalRecordID).ToList();

                    var labTests = context.LabTests.Where(l => medRecordIds.Contains(l.MedicalRecordId)).ToList();
                    context.LabTests.RemoveRange(labTests);

                    var prescriptions = context.Prescriptions
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
                    context.Prescriptions.RemoveRange(prescriptions);

                    context.MedicalRecords.RemoveRange(medicalRecords);
                }

                context.Appointments.RemoveRange(doctorAppointments);
            }

            // 2. Unassign or remove from departments
            var departments = context.Departments.Where(d => d.DoctorProfileId == targetId).ToList();
            context.Departments.RemoveRange(departments);

            // 3. Clear specialization links
            if (doctor.specializations != null)
            {
                doctor.specializations.Clear();
            }

            // 4. Remove doctor profile from database
            context.DoctorProfiles.Remove(doctor);
            context.SaveChanges();

            return Ok(new
            {
                message = "Doctor profile deleted successfully",
                deletedDoctor = deletedDoctor
            });
        }

        // Method: GET all doctors
        [HttpGet("GetAllDoctorProfiles")]
        [HttpGet("GetAllDoctors")]
        public IActionResult GetAllDoctorProfiles()
        {
            List<DoctorProfile> doctors = context.DoctorProfiles
                    .Include(d => d.userid)
                    .ToList();
            return Ok(doctors);
        }
        // Method: GET To Find doctor by Id
        [HttpGet("GetDoctorProfile")]
        [HttpGet("GetDoctorProfile/{id}")]
        public IActionResult GetDoctorProfile(int id)
        {
            DoctorProfile? doctor = context.DoctorProfiles
            .Include(d => d.userid)
            .FirstOrDefault(d => d.DoctorProfileId == id);
            
            if (doctor == null)
            {
                return NotFound("Doctor profile not found");
            }
            return Ok(doctor);
        }

        // Method: GET To Find doctor by User Id
        [HttpGet("user/{userId}")]
        [HttpGet("GetDoctorProfileByUserId/{userId}")]
        public IActionResult GetDoctorProfileByUserId(int userId)
        {
            var doctor = context.DoctorProfiles
                .Include(d => d.userid)
                .FirstOrDefault(d => d.userID == userId);
            if (doctor == null)
            {
                return NotFound("Doctor profile not found for this user.");
            }
            return Ok(doctor);
        }
        // Method: GET Search, Filter, and Sort doctor profiles
        [HttpGet("SearchDoctors")]
        [HttpGet("search")]
        public IActionResult SearchDoctors([FromQuery] string? query, [FromQuery] int? minExperience, [FromQuery] string? sortFee)
        {
            var q = context.DoctorProfiles
                .Include(d => d.userid)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(query))
            {
                string term = query.Trim().ToLower();
                string normId = term
                    .Replace("#doc-", "")
                    .Replace("doc-", "")
                    .Replace("#", "");

                bool isNum = int.TryParse(normId, out int idVal);

                q = q.Where(d =>
                    (isNum && d.DoctorProfileId == idVal) ||
                    (d.LicenseNumber != null && d.LicenseNumber.ToLower().Contains(term)) ||
                    (d.userid != null && d.userid.Fullname != null && d.userid.Fullname.ToLower().Contains(term)) ||
                    (d.userid != null && d.userid.email != null && d.userid.email.ToLower().Contains(term)) ||
                    (d.userid != null && d.userid.Phone != null && d.userid.Phone.ToLower().Contains(term))
                );
            }

            if (minExperience.HasValue && minExperience.Value > 0)
            {
                q = q.Where(d => d.YearsOfExperience >= minExperience.Value);
            }

            if (!string.IsNullOrWhiteSpace(sortFee))
            {
                if (sortFee.Equals("asc", StringComparison.OrdinalIgnoreCase))
                {
                    q = q.OrderBy(d => d.ConsultationFee);
                }
                else if (sortFee.Equals("desc", StringComparison.OrdinalIgnoreCase))
                {
                    q = q.OrderByDescending(d => d.ConsultationFee);
                }
            }

            var result = q.ToList();
            return Ok(result);
        }

        // Method: GET Filter doctors Example: doctors with experience >= years
        [HttpGet("GetDoctorsByExperience")]
        public IActionResult GetDoctorsByExperience(int years)
        {
            List<DoctorProfile> doctors = context.DoctorProfiles
                .Include(d => d.userid)
                .Where(d => d.YearsOfExperience >= years)
                .ToList();
            return Ok(doctors);
        }

        // Method: GET Sort doctors by consultation fee
        [HttpGet("GetDoctorsSortedByFee")]
        public IActionResult GetDoctorsSortedByFee([FromQuery] string? order)
        {
            var q = context.DoctorProfiles.Include(d => d.userid);
            if (order != null && order.Equals("desc", StringComparison.OrdinalIgnoreCase))
            {
                return Ok(q.OrderByDescending(d => d.ConsultationFee).ToList());
            }
            return Ok(q.OrderBy(d => d.ConsultationFee).ToList());
        }
    }
}