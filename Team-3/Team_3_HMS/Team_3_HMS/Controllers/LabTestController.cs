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
        // Case 4: 
        [HttpDelete("{id}")]
        [Authorize(Roles = "Doctor,Admin")]
        public async Task<IActionResult> DeleteLabTest(int id)
        {
            var labTest = await _context.LabTests.FindAsync(id);

            if (labTest == null)
            {
                return NotFound("Lab test not found.");
            }

            _context.LabTests.Remove(labTest);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        // Case 5: 
        [HttpGet]
        [Authorize(Roles = "Doctor,Admin")]
        public async Task<IActionResult> GetLabTests()
        {
            var labTests = await _context.LabTests
                .Include(l => l.record)
                .Select(l => new
                {
                    l.LabTestId,
                    l.TestName,
                    l.Category,
                    l.TestDate,
                    l.Cost,
                    l.Result,
                    l.MedicalRecordId,

                    MedicalRecord = l.record == null
                        ? null
                        : new
                        {
                            l.record.MedicalRecordID,
                            l.record.Diagnosis,
                            l.record.TreatmentPlan,
                            l.record.RecordDate
                        }
                })
                .ToListAsync();

            return Ok(labTests);
        }
        // Case 6: 
        [HttpGet("{id}")]
        [Authorize(Roles = "Doctor,Admin")]
        public async Task<IActionResult> GetLabTestById(int id)
        {
            var labTest = await _context.LabTests
                .Include(l => l.record)
                .Where(l => l.LabTestId == id)
                .Select(l => new
                {
                    l.LabTestId,
                    l.TestName,
                    l.Category,
                    l.TestDate,
                    l.Cost,
                    l.Result,
                    l.MedicalRecordId,

                    MedicalRecord = l.record == null
                        ? null
                        : new
                        {
                            l.record.MedicalRecordID,
                            l.record.Diagnosis,
                            l.record.TreatmentPlan,
                            l.record.Symptom,
                            l.record.RecordDate
                        }
                })
                .FirstOrDefaultAsync();

            if (labTest == null)
            {
                return NotFound("Lab test not found.");
            }

            return Ok(labTest);
        }
        // Case 7: 
        [HttpGet("filter")]
        [Authorize(Roles = "Doctor,Admin")]
        public async Task<IActionResult> FilterLabTests(string category)
        {
            if (string.IsNullOrWhiteSpace(category))
            {
                return BadRequest("Lab test category is required.");
            }

            var labTests = await _context.LabTests
                .Include(l => l.record)
                .Where(l => l.Category.Contains(category))
                .Select(l => new
                {
                    l.LabTestId,
                    l.TestName,
                    l.Category,
                    l.TestDate,
                    l.Cost,
                    l.Result,
                    l.MedicalRecordId,

                    MedicalRecord = l.record == null
                        ? null
                        : new
                        {
                            l.record.MedicalRecordID,
                            l.record.Diagnosis,
                            l.record.RecordDate
                        }
                })
                .ToListAsync();

            if (labTests.Count == 0)
            {
                return NotFound("No matching lab tests were found.");
            }

            return Ok(labTests);
        }
        // Case 8: 
        [HttpGet("summary")]
        [Authorize(Roles = "Doctor,Admin")]
        public async Task<IActionResult> GetLabTestSummary()
        {
            var totalLabTests = await _context.LabTests.CountAsync();

            var totalCost = await _context.LabTests
                .SumAsync(l => (decimal?)l.Cost) ?? 0;

            var labTests = await _context.LabTests
                .OrderByDescending(l => l.TestDate)
                .Select(l => new
                {
                    l.LabTestId,
                    l.TestName,
                    l.Category,
                    l.TestDate,
                    l.Cost,
                    l.Result,
                    l.MedicalRecordId
                })
                .ToListAsync();

            return Ok(new
            {
                TotalLabTests = totalLabTests,
                TotalCost = totalCost,
                LabTests = labTests
            });
        }
        public class UpdateLabTestResultRequest
        {
            public string Result { get; set; } = string.Empty;
        }
    }
}
