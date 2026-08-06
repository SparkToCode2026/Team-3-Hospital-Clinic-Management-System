using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Team_3_HMS.Models;

namespace Team_3_HMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoomController : ControllerBase
    {
        private readonly ProjectContext _context;

        public RoomController(ProjectContext context)
        {
            _context = context;
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> CreateRoom(Room room)
        {
            _context.Rooms.Add(room);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetRoom), new { id = room.RoomId }, room);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> UpdateRoom(int id, Room updated)
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room == null) return NotFound();

            room.RoomNumber = updated.RoomNumber;
            room.Type = updated.Type;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPatch("{id}/availability")]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> UpdateAvailability(int id, [FromBody] bool isAvailable)
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room == null) return NotFound();

            room.IsAvailable = isAvailable;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> DeleteRoom(int id)
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room == null) return NotFound();

            _context.Rooms.Remove(room);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet]
        public async Task<IActionResult> GetRooms()
        {
            return Ok(await _context.Rooms.ToListAsync());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetRoom(int id)
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room == null) return NotFound();
            return Ok(room);
        }

        [HttpGet("available")]
        public async Task<IActionResult> GetAvailableRooms()
        {
            var rooms = await _context.Rooms
                .Where(r => r.IsAvailable)
                .ToListAsync();
            return Ok(rooms);
        }

        [HttpGet("sorted")]
        public async Task<IActionResult> GetRoomsSorted()
        {
            return Ok(await _context.Rooms.OrderBy(r => r.RoomNumber).ToListAsync());
        }

        [HttpGet("count-by-type")]
        public async Task<IActionResult> CountByType()
        {
            var counts = await _context.Rooms
                .GroupBy(r => r.Type)
                .Select(g => new { Type = g.Key, Count = g.Count() })
                .ToListAsync();
            return Ok(counts);
        }
    }
}