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


        // 2. FIND PROFILE BY PATIENT ID
        // GET: api/PatientProfile/find/1
        [Authorize]
        [HttpGet("find/{id}")]
        public IActionResult GetById(int id)
        {
            var profile = _context.PatientProfiles.Find(id);

            if (profile == null)
            {
                return NotFound("Patient profile not found.");
            }

            return Ok(profile);
        }

        // 3. FIND PROFILE BY USER ID (NEW CASE)
        // GET: api/PatientProfile/user/1
        [Authorize]
        [HttpGet("user/{userId}")]
        public IActionResult GetByUserId(int userId)
        {
            var profile = _context.PatientProfiles.FirstOrDefault(p => p.userID == userId);

            if (profile == null)
            {
                return NotFound("Patient profile not found for this user.");
            }

            return Ok(profile);
        }


    }
}