using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using YeniSalon.Data;

namespace YeniSalon.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DenemeController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DenemeController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("/api/deneme/GetAntrenors")]
        public IActionResult GetAntrenors()
        {
            var antrenors = _context.Antrenorler.ToList();
            if(antrenors != null)
                return Ok(antrenors);
            else
                return NotFound(antrenors);
        }
    }
}
