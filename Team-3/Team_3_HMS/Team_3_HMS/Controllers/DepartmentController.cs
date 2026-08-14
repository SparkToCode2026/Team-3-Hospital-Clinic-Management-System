using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Team_3_HMS.Models;

namespace Team_3_HMS.Controllers
{
    [ApiController]
    [Route("Department")]
    [Route("api/[controller]")]
    public class DepartmentController : ControllerBase
    {
        private ProjectContext context;

        public DepartmentController(ProjectContext _context)
        {
            context = _context;
        }
        // Method: POST To Create new Department
        [Authorize(Roles = "Admin")]
        [HttpPost("AddDepartment")]
        public IActionResult AddDepartment(Department department)
        {
            context.Departments.Add(department);
            context.SaveChanges();
            return Ok(new
            {
                message = "Department added successfully",
                department = department
            });
        }
        // PUT to Update all Department details
        [Authorize(Roles = "Admin")]
        [HttpPut("UpdateDepartment")]
        [HttpPut("UpdateDepartment/{id}")]
        public IActionResult UpdateDepartment([FromQuery] int? id, [FromRoute] int? routeId, [FromBody] Department updatedDepartment)
        {
            int targetId = id ?? routeId ?? updatedDepartment.DepartmentId;
            Department? department = context.Departments
                .FirstOrDefault(d => d.DepartmentId == targetId);

            if (department == null)
            {
                return NotFound("Department not found");
            }

            department.Name = updatedDepartment.Name;
            department.Description = updatedDepartment.Description;
            department.BuildingLocation = updatedDepartment.BuildingLocation;
            if (updatedDepartment.DoctorProfileId > 0)
            {
                department.DoctorProfileId = updatedDepartment.DoctorProfileId;
            }

            context.SaveChanges();
            return Ok(new
            {
                message = "Department updated successfully",
                updatedDepartment = department
            });
        }

        // PATCH Update one field (Building Location)
        [Authorize(Roles = "Admin")]
        [HttpPatch("UpdateBuildingLocation")]
        [HttpPatch("UpdateBuildingLocation/{id}")]
        public IActionResult UpdateBuildingLocation([FromQuery] int? id, [FromRoute] int? routeId, [FromQuery] string? newLocation, [FromBody] Department? body)
        {
            int targetId = id ?? routeId ?? body?.DepartmentId ?? 0;
            string location = newLocation ?? body?.BuildingLocation ?? "";

            Department? department = context.Departments
                .FirstOrDefault(d => d.DepartmentId == targetId);

            if (department == null)
            {
                return NotFound("Department not found");
            }

            department.BuildingLocation = location;
            context.SaveChanges();
            return Ok(new
            {
                message = "Building location updated successfully",
                updatedDepartment = department
            });
        }

        // DELETE to Remove Department
        [Authorize(Roles = "Admin")]
        [HttpDelete("RemoveDepartment")]
        [HttpDelete("RemoveDepartment/{id}")]
        public IActionResult RemoveDepartment([FromQuery] int? id, [FromRoute] int? routeId)
        {
            int targetId = id ?? routeId ?? 0;
            Department? department = context.Departments
                .FirstOrDefault(d => d.DepartmentId == targetId);

            if (department == null)
            {
                return NotFound("Department not found");
            }

            var deletedDepartment = department;
            context.Departments.Remove(department);
            context.SaveChanges();
            return Ok(new
            {
                message = "Department removed successfully",
                deletedDepartment = deletedDepartment
            });
        }

        // GET All Departments include(related Doctor and User)
        [HttpGet("GetAllDepartments")]
        public IActionResult GetAllDepartments()
        {
            List<Department> departments = context.Departments
                .Include(d => d.Profile)
                    .ThenInclude(p => p.userid)
                .ToList();
            return Ok(departments);
        }

        // GET Department by Id
        [HttpGet("GetDepartmentByID")]
        [HttpGet("GetDepartmentByID/{id}")]
        [HttpGet("{id}")]
        public IActionResult GetDepartmentByID([FromQuery] int? id, [FromRoute] int? routeId)
        {
            int targetId = id ?? routeId ?? 0;
            Department? department = context.Departments
                .Include(d => d.Profile)
                    .ThenInclude(p => p.userid)
                .FirstOrDefault(d => d.DepartmentId == targetId);

            if (department == null)
            {
                return NotFound("Department not found");
            }

            return Ok(department);
        }
        // GET Filter Departments by name
        [HttpGet("GetDepartmentsByName")]
        public IActionResult GetDepartmentsByName(string name)
        {
            List<Department> departments = context.Departments
            .Where(d => d.Name.Contains(name))
            .ToList();
            return Ok(departments);
        }
        // GET Sort Departments by name
        [HttpGet("GetDepartmentsSorted")]
        public IActionResult GetDepartmentsSorted()
        {
            List<Department> departments = context.Departments
            .OrderBy(d => d.Name)
            .ToList();
            return Ok(departments);
        }
    }
}