using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Team_3_HMS.Models;

namespace Team_3_HMS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly ProjectContext _context;
        private readonly IConfiguration _config;

        public UserController(ProjectContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        // 1. POST: Register public user
        [HttpPost("register")]
        public IActionResult Register([FromBody] user newUser)
        {
            // Prevent public registration for Admin accounts
            if (newUser.role != null && newUser.role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("Admin accounts cannot be registered publicly.");
            }

            if (_context.Users.Any(u => u.email == newUser.email))
            {
                return BadRequest("Email is already registered.");
            }

            _context.Users.Add(newUser);
            _context.SaveChanges();

            return Ok(new { message = "User registered successfully!", userId = newUser.userID });
        }

        [HttpPost("login")]
        public IActionResult Login(string email, string password)
        {
            var user = _context.Users.FirstOrDefault(u => u.email == email && u.PasswordHash == password);
            if (user == null)
            {
                return Unauthorized("Invalid email or password.");
            }

            var token = GenerateJwtToken(user);

            // Append JWT token into an HTTP-Only Cookie via C#
            Response.Cookies.Append("jwt_token", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddHours(2)
            });

            return Ok(new
            {
                message = "Login successful!",
                token = token,
                role = user.role,
                userId = user.userID,
                fullname = user.Fullname
            });
        }

       
        [Authorize]
        [HttpPut("update/{id}")]
        public IActionResult UpdateUser(int id, [FromBody] user updatedData)
        {
            var user = _context.Users.Find(id);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            user.Fullname = updatedData.Fullname;
            user.Phone = updatedData.Phone;

            _context.SaveChanges();
            return Ok(new { message = "User details updated successfully." });
        }

       
        // 4. PUT: Change user password
       
        [Authorize]
        [HttpPut("change-password/{id}")]
        public IActionResult ChangePassword(int id, string oldPassword, string newPassword)
        {
            var user = _context.Users.Find(id);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            if (user.PasswordHash != oldPassword)
            {
                return BadRequest("Incorrect current password.");
            }

            user.PasswordHash = newPassword;
            _context.SaveChanges();

            return Ok(new { message = "Password updated successfully." });
        }

        
        // 5. DELETE: Remove user account (Admin only)
        
        [Authorize(Roles = "Admin")]
        [HttpDelete("delete/{id}")]
        public IActionResult DeleteUser(int id)
        {
            var user = _context.Users.Find(id);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            _context.Users.Remove(user);
            _context.SaveChanges();

            return Ok(new { message = "User account deleted successfully." });
        }

        
        // 6. GET: Retrieve all users (Admin only)
        
        [Authorize(Roles = "Admin")]
        [HttpGet("all")]
        public IActionResult GetAllUsers()
        {
            var users = _context.Users.ToList();
            return Ok(users);
        }

        
        // 7. GET: Find user by Id
       
        [Authorize]
        [HttpGet("find/{id}")]
        public IActionResult GetUserById(int id)
        {
            var user = _context.Users.FirstOrDefault(u => u.userID == id);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            return Ok(user);
        }

        
        // 8. GET: Count total users by role (Admin only)
       
        [Authorize(Roles = "Admin")]
        [HttpGet("count-by-role/{role}")]
        public IActionResult GetUserCountByRole(string role)
        {
            // Added null-check u.role != null to prevent NullReferenceException
            var count = _context.Users.Count(u => u.role != null && u.role.ToLower() == role.ToLower());
            return Ok(new { role = role, totalUsers = count });
        }

        
        // Helper Method: JWT Token Generator
        
        private string GenerateJwtToken(user user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.userID.ToString()),
                new Claim(ClaimTypes.Email, user.email ?? ""),
                new Claim(ClaimTypes.Role, user.role ?? "Patient")
            };

            // Added ?? fallback guard to prevent System.ArgumentNullException: 'Value cannot be null. (Parameter 's')'
            string secretKey = _config["Jwt:Key"] ?? "ThisIsAVerySecretKeyForTeam3HospitalManagementSystem2026!";

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddHours(2),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}