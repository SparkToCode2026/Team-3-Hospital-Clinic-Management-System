using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        // PATCH Update one field (Description)
        [HttpPatch("UpdateSpecializationDescription")]
        public IActionResult UpdateSpecializationDescription(int id, string newDescription)
        {
            // To Find Specialization by ID
            Specialization specialization = context.Specializations
            .FirstOrDefault(s => s.SpecializationId == id);
            if (specialization == null)
            {
                return NotFound("Specialization not found");
            }
            specialization.Description = newDescription;
            context.SaveChanges();
            return Ok(new
            {
                message = "Specialization description updated successfully",
                updatedSpecialization = specialization
            });

        }
        // DELETE to Remove Specialization
        [HttpDelete("RemoveSpecialization")]
        public IActionResult RemoveSpecialization(int id)
        {
            Specialization specialization = context.Specializations
            .FirstOrDefault(s => s.SpecializationId == id);
            if (specialization == null)
            {
                return NotFound("Specialization not found");
            }
            var deletedSpecialization = specialization;
            context.Specializations.Remove(specialization);
            context.SaveChanges();
            return Ok(new
            {
                message = "Specialization removed successfully",
                deletedSpecialization = deletedSpecialization
            });
        }
        // GET  all Specializations (data)
        [HttpGet("GetAllSpecializations")]
        public IActionResult GetAllSpecializations()
        {
            List<Specialization> specializations = context.Specializations
            .Include(s => s.doctors)
            .ToList();
            return Ok(specializations);
        }
        // GET to Find Specialization by Id
        [HttpGet("GetSpecialization")]
        public IActionResult GetSpecialization(int id)
        {
            Specialization specialization = context.Specializations
            .FirstOrDefault(s => s.SpecializationId == id);
            if (specialization == null)
            {
                return NotFound("Specialization not found");
            }
            return Ok(specialization);
        }
        // GET Filter Specializations by name
        [HttpGet("GetSpecializationsByName")]
        public IActionResult GetSpecializationsByName(string name)
        {
            List<Specialization> specializations = context.Specializations
            .Where(s => s.Name.Contains(name))
            .ToList();
           
            return Ok(specializations);
        }
        // GET to Sort Specializations by name
        [HttpGet("GetSpecializationsSorted")]
        public IActionResult GetSpecializationsSorted()
        {
            List<Specialization> specializations = context.Specializations
            .OrderBy(s => s.Name)
            .ToList();
            return Ok(specializations);
        }
    }
}
