using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Team_3_HMS.Models;

namespace Team_3_HMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PatientProfileController : ControllerBase
    {
        private readonly ProjectContext _context;

        public PatientProfileController(ProjectContext context)
        {
            _context = context;
        }

        // 1. GET ALL PROFILES
        // GET: api/PatientProfile/all
        [Authorize(Roles = "Admin")]
        [HttpGet("all")]
        public IActionResult GetAll()
        {
            var profiles = _context.PatientProfiles.ToList();
            return Ok(profiles);
        }

    }
}