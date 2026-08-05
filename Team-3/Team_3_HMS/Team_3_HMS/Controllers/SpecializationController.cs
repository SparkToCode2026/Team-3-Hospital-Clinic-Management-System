using Microsoft.AspNetCore.Mvc;

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
    }
}
