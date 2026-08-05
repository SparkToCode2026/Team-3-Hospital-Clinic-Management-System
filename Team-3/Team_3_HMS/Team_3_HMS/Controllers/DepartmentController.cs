using Microsoft.AspNetCore.Mvc;

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

    }
}