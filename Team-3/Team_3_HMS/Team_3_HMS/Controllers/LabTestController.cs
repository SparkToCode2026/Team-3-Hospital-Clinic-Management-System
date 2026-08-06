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
    }
}
