using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Team_3_HMS.Models;

namespace Team_3_HMS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MedicationController : ControllerBase
    {
        private readonly ProjectContext _context;

        public MedicationController(ProjectContext context)
        {
            _context = context;
        }
    }
}