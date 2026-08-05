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
    }
}