using Microsoft.AspNetCore.Mvc;
namespace Team_3_HMS.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Team_3_HMS.Models;


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

    [HttpPost("register")]
    public IActionResult Register([FromBody] user newUser)
    {
        if (newUser.role != null && newUser.role.ToLower() == "admin")
        {
            return BadRequest("Admin accounts cannot be registered publicly.");
        }

        user existingUser = _context.Users.FirstOrDefault(u => u.email == newUser.email);
        if (existingUser != null)
        {
            return BadRequest("Email is already registered.");
        }

        _context.Users.Add(newUser);
        _context.SaveChanges();

        return Ok("User registered successfully!");
    }


    [HttpPost("login")]
    public IActionResult Login(string email, string password)
    {
        user foundUser = _context.Users.FirstOrDefault(u => u.email == email && u.PasswordHash == password);
        if (foundUser == null)
        {
            return Unauthorized("Invalid email or password.");
        }

        string token = GenerateJwtToken(foundUser);

        return Ok(token);
    }

    [Authorize]
    [HttpPut("update/{id}")]
    public IActionResult UpdateUser(int id, [FromBody] user updatedData)
    {
        user existingUser = _context.Users.FirstOrDefault(u => u.userID == id);
        if (existingUser == null)
        {
            return NotFound("User not found.");
        }

        existingUser.Fullname = updatedData.Fullname;
        existingUser.Phone = updatedData.Phone;

        _context.SaveChanges();
        return Ok("User details updated successfully.");
    }

    [Authorize]
    [HttpPut("change-password/{id}")]
    public IActionResult ChangePassword(int id, string oldPassword, string newPassword)
    {
        user existingUser = _context.Users.FirstOrDefault(u => u.userID == id);
        if (existingUser == null)
        {
            return NotFound("User not found.");
        }

        if (existingUser.PasswordHash != oldPassword)
        {
            return BadRequest("Incorrect current password.");
        }

        existingUser.PasswordHash = newPassword;
        _context.SaveChanges();

        return Ok("Password updated successfully.");
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("delete/{id}")]
    public IActionResult DeleteUser(int id)
    {
        user existingUser = _context.Users.FirstOrDefault(u => u.userID == id);
        if (existingUser == null)
        {
            return NotFound("User not found.");
        }

        _context.Users.Remove(existingUser);
        _context.SaveChanges();

        return Ok("User account deleted successfully.");
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("all")]
    public IActionResult GetAllUsers()
    {
        List<user> users = _context.Users.ToList();
        return Ok(users);
    }

    [Authorize]
    [HttpGet("find/{id}")]
    public IActionResult GetUserById(int id)
    {
        user foundUser = _context.Users.FirstOrDefault(u => u.userID == id);
        if (foundUser == null)
        {
            return NotFound("User not found.");
        }

        return Ok(foundUser);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("count-by-role/{role}")]
    public IActionResult GetUserCountByRole(string role)
    {
        int totalCount = _context.Users.Count(u => u.role != null && u.role.ToLower() == role.ToLower());
        return Ok(totalCount);
    }


    private string GenerateJwtToken(user userToSign)
    {
        var claims = new[]
        {
                new Claim(ClaimTypes.NameIdentifier, userToSign.userID.ToString()),
                new Claim(ClaimTypes.Email, userToSign.email ?? ""),
                new Claim(ClaimTypes.Role, userToSign.role ?? "Patient")
            };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.Now.AddHours(2),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

}
