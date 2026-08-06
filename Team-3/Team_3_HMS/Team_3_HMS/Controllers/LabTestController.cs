using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Team_3_HMS.Models;

namespace Team_3_HMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LabTestController : ControllerBase
    {
        private readonly ProjectContext _context;

        public LabTestController(ProjectContext context)
        {
            _context = context;
        }
        // Case 1: 
        [HttpPost]
        [Authorize(Roles = "Doctor,Admin")]
        public async Task<ActionResult<LabTest>> CreateLabTest(LabTest labTest)
        {
            var medicalRecordExists = await _context.MedicalRecords
                .AnyAsync(m =>
                    m.MedicalRecordID == labTest.MedicalRecordId);

            if (!medicalRecordExists)
            {
                return BadRequest("Medical record not found.");
            }

            if (labTest.Cost < 0)
            {
                return BadRequest("Lab test cost cannot be negative.");
            }

            _context.LabTests.Add(labTest);
            await _context.SaveChangesAsync();

            return Created(
                $"/api/LabTest/{labTest.LabTestId}",
                labTest);
        }
    }
}
