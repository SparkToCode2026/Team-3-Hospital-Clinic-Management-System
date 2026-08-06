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
        // Case 2: 
        [HttpPut("{id}")]
        [Authorize(Roles = "Doctor,Admin")]
        public async Task<IActionResult> UpdateLabTest(
            int id,
            LabTest updatedLabTest)
        {
            if (id != updatedLabTest.LabTestId)
            {
                return BadRequest("Lab test ID does not match.");
            }

            var existingLabTest = await _context.LabTests.FindAsync(id);

            if (existingLabTest == null)
            {
                return NotFound("Lab test not found.");
            }

            var medicalRecordExists = await _context.MedicalRecords
                .AnyAsync(m =>
                    m.MedicalRecordID == updatedLabTest.MedicalRecordId);

            if (!medicalRecordExists)
            {
                return BadRequest("Medical record not found.");
            }

            if (updatedLabTest.Cost < 0)
            {
                return BadRequest("Lab test cost cannot be negative.");
            }

            existingLabTest.TestName = updatedLabTest.TestName;
            existingLabTest.Category = updatedLabTest.Category;
            existingLabTest.TestDate = updatedLabTest.TestDate;
            existingLabTest.Cost = updatedLabTest.Cost;
            existingLabTest.Result = updatedLabTest.Result;
            existingLabTest.MedicalRecordId =
                updatedLabTest.MedicalRecordId;

            await _context.SaveChangesAsync();

            return NoContent();
        }
        // Case 3:
        [HttpPatch("{id}/result")]
        [Authorize(Roles = "Doctor,Admin")]
        public async Task<IActionResult> UpdateLabTestResult(
            int id,
            UpdateLabTestResultRequest request)
        {
            var existingLabTest = await _context.LabTests.FindAsync(id);

            if (existingLabTest == null)
            {
                return NotFound("Lab test not found.");
            }

            if (string.IsNullOrWhiteSpace(request.Result))
            {
                return BadRequest("Lab test result is required.");
            }

            existingLabTest.Result = request.Result;

            await _context.SaveChangesAsync();

            return NoContent();
        }
        public class UpdateLabTestResultRequest
        {
            public string Result { get; set; } = string.Empty;
        }
    }
}
