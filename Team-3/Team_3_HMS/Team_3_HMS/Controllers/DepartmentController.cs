using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Team_3_HMS.Models;

namespace Team_3_HMS.Controllers
{
    [ApiController]
    [Route("Department")]
    public class DepartmentController : ControllerBase
    {
        private ProjectContext context;

        public DepartmentController(ProjectContext _context)
        {
            context = _context;
        }
        // Method: POST To Create new Department
        //[Authorize(Roles = "Admin")]
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
        //[Authorize(Roles = "Admin")]
        [HttpPut("UpdateDepartment")]
        public IActionResult UpdateDepartment(int id, Department updatedDepartment)
        {
            Department department = context.Departments
            .FirstOrDefault(d => d.DepartmentId == id);
            if (department == null)
            {
                return NotFound("Department not found");
            }
            department.Name = updatedDepartment.Name;
            department.Description = updatedDepartment.Description;
            department.BuildingLocation = updatedDepartment.BuildingLocation;
            department.DoctorProfileId = updatedDepartment.DoctorProfileId;
            context.SaveChanges();
            return Ok(new
            {
                message = "Department updated successfully",
                updatedDepartment = department
            });
        }
        // PATCH Update one field (Building Location)
        //[Authorize(Roles = "Admin")]
        [HttpPatch("UpdateBuildingLocation")]
        public IActionResult UpdateBuildingLocation(int id, string newLocation)
        {
            // To Find Department by ID
            Department department = context.Departments
            .FirstOrDefault(d => d.DepartmentId == id);
            if (department == null)
            {
                return NotFound("Department not found");
            }
            department.BuildingLocation = newLocation;
            context.SaveChanges();
            return Ok(new
            {
                message = "Building location updated successfully",
                updatedDepartment = department
            });
        }
        // DELETE to Remove Department
        //[Authorize(Roles = "Admin")]
        [HttpDelete("RemoveDepartment")]
        public IActionResult RemoveDepartment(int id)
        {
            Department department = context.Departments
            .FirstOrDefault(d => d.DepartmentId == id);
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
        // GET All Departments include(related Doctor)
        [HttpGet("GetAllDepartments")]
        public IActionResult GetAllDepartments()
        {
        List<Department> departments = context.Departments
        .Include(d => d.Profile)
        .ToList();
            return Ok(departments);
        }
        // GET Department by Id
        [HttpGet("GetDepartmentByID")]
        public IActionResult GetDepartmentByID(int id)
        {
            Department department = context.Departments
            .FirstOrDefault(d => d.DepartmentId == id);
            if(department == null)
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