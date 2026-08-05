using Microsoft.AspNetCore.Mvc;
using Team_3_HMS.Models;

namespace Team_3_HMS.Controllers
{
    [ApiController]
    [Route("DoctorProfile")]
    public class DoctorProfileController : ControllerBase
    {
        private ProjectContext context;

        public DoctorProfileController(ProjectContext context)
        {
            context = context;
        }
        // Method: POST create new doctor Profile
        [HttpPost("AddDoctorProfile")]
        public IActionResult AddDoctorProfile(DoctorProfile doctor)
        {
            context.DoctorProfiles.Add(doctor);

            context.SaveChanges();
            
            return Ok(doctor.DoctorProfileId);
        }
        // Method: PUT update all doctor profile details
        [HttpPut("UpdateDoctorProfile")]
        public IActionResult UpdateDoctorProfile(int id, DoctorProfile updatedDoctor)
        {
           DoctorProfile doctor = context.DoctorProfiles
           .FirstOrDefault(d => d.DoctorProfileId == id);
            if (doctor == null)
            {
                return NotFound();
            }
            doctor.LicenseNumber = updatedDoctor.LicenseNumber;
            doctor.YearsOfExperience = updatedDoctor.YearsOfExperience;
            doctor.ConsultationFee = updatedDoctor.ConsultationFee;
            doctor.userID = updatedDoctor.userID;
            doctor.SpecializationId = updatedDoctor.SpecializationId;
         
            context.SaveChanges();
            return Ok("Doctor profile updated successfully");
        }

        // Method: PATCH Update one field (Consultation Fee)

        [HttpPatch("UpdateConsultationFee")]
        public IActionResult UpdateConsultationFee(int id, double newFee)
        {
            // To Find Doctor by ID
            DoctorProfile doctor = context.DoctorProfiles
           .FirstOrDefault(d => d.DoctorProfileId == id);

            if (doctor == null)
            {
                return NotFound("Doctor profile not found");
            }
            // Change only the consultation fee
            doctor.ConsultationFee = newFee;

            context.SaveChanges();
            return Ok("Consultation fee updated successfully");
        }
    }
}
