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
    }
}
