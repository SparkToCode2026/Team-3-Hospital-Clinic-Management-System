using Microsoft.AspNetCore.Mvc;
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
        [HttpPut("UpdateDepartment")]
        public IActionResult UpdateDepartment(int id, Department updatedDepartment)
        {
            Department department = context.Departments
            .FirstOrDefault(d => d.DepartmentId == id); //update by id 
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
    }
}