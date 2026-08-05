using Microsoft.AspNetCore.Mvc;
using Team_3_HMS.Models;

namespace Team_3_HMS.Controllers
{
    [ApiController]
    [Route("Specialization")]
    public class SpecializationController : ControllerBase
    {
        private ProjectContext context;

        public SpecializationController(ProjectContext _context)
        {
            context = _context;
        }
        // (POST) Create new Specialization
        [HttpPost("AddSpecialization")]
        public IActionResult AddSpecialization(Specialization specialization)
        {
            context.Specializations.Add(specialization);
            context.SaveChanges();
            return Ok(new
            {
                message = "Specialization added successfully",
                specialization = specialization
            });
        }
        // PUT to Update all Specialization details
        public IActionResult UpdateSpecialization(int id, Specialization updatedSpecialization)
        {
            Specialization specialization = context.Specializations
            .FirstOrDefault(s => s.SpecializationId == id);
            if (specialization == null)
            {
                return NotFound("Specialization not found");
            }
            specialization.Name = updatedSpecialization.Name;
            specialization.Description = updatedSpecialization.Description;
            specialization.DoctorProfileId = updatedSpecialization.DoctorProfileId;
            context.SaveChanges();
            return Ok(new
            {
                message = "Specialization updated successfully",
                updatedSpecialization = specialization
            });
        }
    }
}
