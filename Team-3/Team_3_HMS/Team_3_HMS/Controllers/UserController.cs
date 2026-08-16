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
    [Route("User")]
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
            // Prevent public registration for Admin accounts unless caller is authenticated Admin
            bool isAdminCaller = User.IsInRole("Admin");
            if (!isAdminCaller && newUser.role != null && newUser.role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("Admin accounts cannot be registered publicly.");
            }

            if (_context.Users.Any(u => u.email == newUser.email))
            {
                return BadRequest("Email is already registered.");
            }

            // Hash the password using BCrypt
            if (!string.IsNullOrEmpty(newUser.PasswordHash))
            {
                newUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newUser.PasswordHash);
            }

            _context.Users.Add(newUser);
            _context.SaveChanges();

            return Ok(new { message = "User registered successfully!", userId = newUser.userID });
        }

        [HttpPost("login")]
        public IActionResult Login(string email, string password)
        {
            var user = _context.Users.FirstOrDefault(u => u.email == email);
            
            if (user == null)
            {
                return Unauthorized("Invalid email or password.");
            }

            bool isValid = false;

            // Verify password using BCrypt
            if (!string.IsNullOrEmpty(user.PasswordHash) && user.PasswordHash.StartsWith("$2"))
            {
                isValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
            }
            // Fallback & automatic upgrade for existing unhashed plain-text passwords
            else if (user.PasswordHash == password)
            {
                isValid = true;
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
                _context.SaveChanges();
            }

            if (!isValid)
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

       
        public class UpdateUserDto
        {
            public string? Fullname { get; set; }
            public string? Phone { get; set; }
            public string? Email { get; set; }
        }

        [Authorize]
        [HttpPut("update/{id}")]
        public IActionResult UpdateUser(int id, [FromBody] UpdateUserDto updatedData)
        {
            var user = _context.Users.Find(id);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            if (!string.IsNullOrWhiteSpace(updatedData.Fullname))
                user.Fullname = updatedData.Fullname;
            if (!string.IsNullOrWhiteSpace(updatedData.Phone))
                user.Phone = updatedData.Phone;
            if (!string.IsNullOrWhiteSpace(updatedData.Email))
                user.email = updatedData.Email;

            _context.SaveChanges();
            return Ok(new { message = "User details updated successfully." });
        }

       
        public class ChangePasswordDto
        {
            public string? OldPassword { get; set; }
            public string? NewPassword { get; set; }
        }

        // 4. PUT: Change user password
        [Authorize]
        [HttpPut("change-password/{id}")]
        public IActionResult ChangePassword(int id, [FromQuery] string? oldPassword = null, [FromQuery] string? newPassword = null)
        {
            string oldPwd = oldPassword ?? string.Empty;
            string newPwd = newPassword ?? string.Empty;

            if (string.IsNullOrWhiteSpace(oldPwd) || string.IsNullOrWhiteSpace(newPwd))
            {
                return BadRequest("Current password and new password are required.");
            }

            var user = _context.Users.Find(id);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            bool isOldPasswordValid = false;
            if (!string.IsNullOrEmpty(user.PasswordHash) && user.PasswordHash.StartsWith("$2"))
            {
                isOldPasswordValid = BCrypt.Net.BCrypt.Verify(oldPwd, user.PasswordHash);
            }
            else
            {
                isOldPasswordValid = (user.PasswordHash == oldPwd);
            }

            if (!isOldPasswordValid)
            {
                return BadRequest("Incorrect current password.");
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPwd);
            _context.SaveChanges();

            return Ok(new { message = "Password updated successfully." });
        }

        
        // 5. DELETE: Remove user account (Admin only)
        [Authorize(Roles = "Admin")]
        [HttpDelete("delete/{id}")]
        [HttpDelete("{id}")]
        public IActionResult DeleteUser(int id)
        {
            var user = _context.Users.Find(id);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            // 1. Cascade delete if user is a Patient
            var patientProfiles = _context.PatientProfiles.Where(p => p.userID == id).ToList();
            foreach (var patient in patientProfiles)
            {
                var patientAppointments = _context.Appointments.Where(a => a.PatientProfileID == patient.PatientProfileID).ToList();
                DeleteAppointmentsCascade(patientAppointments);
                _context.PatientProfiles.Remove(patient);
            }

            // 2. Cascade delete if user is a Doctor
            var doctorProfiles = _context.DoctorProfiles
                .Include(d => d.specializations)
                .Where(d => d.userID == id)
                .ToList();

            foreach (var doctor in doctorProfiles)
            {
                var doctorAppointments = _context.Appointments.Where(a => a.DoctorProfileId == doctor.DoctorProfileId).ToList();
                DeleteAppointmentsCascade(doctorAppointments);

                // Handle departments where doctor is assigned
                var departments = _context.Departments.Where(d => d.DoctorProfileId == doctor.DoctorProfileId).ToList();
                _context.Departments.RemoveRange(departments);

                // Clear many-to-many specializations
                if (doctor.specializations != null)
                {
                    doctor.specializations.Clear();
                }

                _context.DoctorProfiles.Remove(doctor);
            }

            _context.Users.Remove(user);
            _context.SaveChanges();

            return Ok(new { message = "User account deleted successfully." });
        }

        private void DeleteAppointmentsCascade(List<Appointment> appointments)
        {
            if (appointments == null || !appointments.Any()) return;

            var apptIds = appointments.Select(a => a.AppointmentId).ToList();

            // Delete Invoices for these appointments
            var invoices = _context.Invoices.Where(i => apptIds.Contains(i.AppointmentID)).ToList();
            _context.Invoices.RemoveRange(invoices);

            // Delete MedicalRecords (and their related Prescriptions & LabTests)
            var medicalRecords = _context.MedicalRecords.Where(m => apptIds.Contains(m.AppointmentId)).ToList();
            if (medicalRecords.Any())
            {
                var medRecordIds = medicalRecords.Select(m => m.MedicalRecordID).ToList();

                // Delete LabTests
                var labTests = _context.LabTests.Where(l => medRecordIds.Contains(l.MedicalRecordId)).ToList();
                _context.LabTests.RemoveRange(labTests);

                // Delete Prescriptions
                var prescriptions = _context.Prescriptions
                    .Include(p => p.Medications)
                    .Where(p => medRecordIds.Contains(p.MedicalRecordId))
                    .ToList();
                foreach (var prescription in prescriptions)
                {
                    if (prescription.Medications != null)
                    {
                        prescription.Medications.Clear();
                    }
                }
                _context.Prescriptions.RemoveRange(prescriptions);

                _context.MedicalRecords.RemoveRange(medicalRecords);
            }

            _context.Appointments.RemoveRange(appointments);
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