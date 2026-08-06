using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Team_3_HMS.Models;

namespace Team_3_HMS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PrescriptionController : ControllerBase
    {
        private readonly ProjectContext _context;
        private readonly IEmailService _emailService;

        public PrescriptionController(ProjectContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }
    }
}