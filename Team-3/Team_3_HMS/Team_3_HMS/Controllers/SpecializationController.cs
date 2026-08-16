using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Team_3_HMS.Models;

namespace Team_3_HMS.Controllers
{
    [ApiController]
    [Route("Specialization")]
    [Route("api/[controller]")]
    public class SpecializationController : ControllerBase
    {
        private ProjectContext context;

        public SpecializationController(ProjectContext _context)
        {
            context = _context;
        }
        // (POST) Create new Specialization
        [Authorize(Roles = "Admin")]
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
        [Authorize(Roles = "Admin")]
        [HttpPut("UpdateSpecialization")]
        [HttpPut("UpdateSpecialization/{id}")]
        public IActionResult UpdateSpecialization([FromQuery] int? id, [FromRoute] int? routeId, [FromBody] Specialization updatedSpecialization)
        {
            int targetId = id ?? routeId ?? updatedSpecialization.SpecializationId;
            Specialization? specialization = context.Specializations
                .FirstOrDefault(s => s.SpecializationId == targetId);

            if (specialization == null)
            {
                return NotFound("Specialization not found");
            }

            specialization.Name = updatedSpecialization.Name;
            specialization.Description = updatedSpecialization.Description;
            if (updatedSpecialization.DoctorProfileId > 0)
            {
                specialization.DoctorProfileId = updatedSpecialization.DoctorProfileId;
            }

            context.SaveChanges();
            return Ok(new
            {
                message = "Specialization updated successfully",
                updatedSpecialization = specialization
            });
        }

        // PATCH Update one field (Description)
        [Authorize(Roles = "Admin")]
        [HttpPatch("UpdateSpecializationDescription")]
        [HttpPatch("UpdateSpecializationDescription/{id}")]
        public IActionResult UpdateSpecializationDescription([FromQuery] int? id, [FromRoute] int? routeId, [FromQuery] string? newDescription, [FromBody] Specialization? body)
        {
            int targetId = id ?? routeId ?? body?.SpecializationId ?? 0;
            string desc = newDescription ?? body?.Description ?? "";

            Specialization? specialization = context.Specializations
                .FirstOrDefault(s => s.SpecializationId == targetId);

            if (specialization == null)
            {
                return NotFound("Specialization not found");
            }

            specialization.Description = desc;
            context.SaveChanges();
            return Ok(new
            {
                message = "Specialization description updated successfully",
                updatedSpecialization = specialization
            });
        }

        // DELETE to Remove Specialization
        [Authorize(Roles = "Admin")]
        [HttpDelete("RemoveSpecialization")]
        [HttpDelete("RemoveSpecialization/{id}")]
        [HttpDelete("delete/{id}")]
        [HttpDelete("{id}")]
        public IActionResult RemoveSpecialization([FromQuery] int? id, [FromRoute(Name = "id")] int? routeId)
        {
            int targetId = (id.HasValue && id.Value > 0) ? id.Value : (routeId ?? 0);
            Specialization? specialization = context.Specializations
                .Include(s => s.doctors)
                .FirstOrDefault(s => s.SpecializationId == targetId);

            if (specialization == null)
            {
                return NotFound("Specialization not found");
            }

            var deletedSpecialization = specialization;

            // Clear doctor associations
            if (specialization.doctors != null)
            {
                specialization.doctors.Clear();
            }

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
                    .ThenInclude(d => d.userid)
                .ToList();
            return Ok(specializations);
        }

        // GET to Find Specialization by Id
        [HttpGet("GetSpecialization")]
        [HttpGet("GetSpecialization/{id}")]
        [HttpGet("{id}")]
        public IActionResult GetSpecialization([FromQuery] int? id, [FromRoute] int? routeId)
        {
            int targetId = id ?? routeId ?? 0;
            Specialization? specialization = context.Specializations
                .Include(s => s.doctors)
                    .ThenInclude(d => d.userid)
                .FirstOrDefault(s => s.SpecializationId == targetId);

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
