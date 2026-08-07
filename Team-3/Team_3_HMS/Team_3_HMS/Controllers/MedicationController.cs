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

        [HttpPost]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> CreateMedication([FromBody] Medication medication)
        {
            _context.Medications.Add(medication);
            await _context.SaveChangesAsync();

            return Ok(medication);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> UpdateMedication(int id, [FromBody] Medication updated)
        {
            var medication = await _context.Medications.FindAsync(id);

            if (medication == null)
                return NotFound();

            medication.Name = updated.Name;
            medication.GenericName = updated.GenericName;
            medication.DosageForm = updated.DosageForm;
            medication.UnitPrice = updated.UnitPrice;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPatch("{id}/price")]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> UpdatePrice(int id, [FromBody] double price)
        {
            var medication = await _context.Medications.FindAsync(id);

            if (medication == null)
                return NotFound();

            medication.UnitPrice = price;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> DeleteMedication(int id)
        {
            var medication = await _context.Medications.FindAsync(id);

            if (medication == null)
                return NotFound();

            _context.Medications.Remove(medication);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetMedication(int id)
        {
            var medication = await _context.Medications.FindAsync(id);

            if (medication == null)
                return NotFound();

            return Ok(medication);
        }
        [HttpGet("search/{name}")]
        public async Task<IActionResult> SearchMedication(string name)
        {
            var medications = await _context.Medications
                .Where(x => x.Name.Contains(name))
                .ToListAsync();

            return Ok(medications);
        }
        [HttpGet("sort")]
        public async Task<IActionResult> SortByPrice()
        {
            var medications = await _context.Medications
                .OrderBy(x => x.UnitPrice)
                .ToListAsync();

            return Ok(medications);
        }

        [HttpGet("count")]
        public async Task<IActionResult> CountMedication()
        {
            var count = await _context.Medications.CountAsync();

            return Ok(count);
        }
         

    }
}





