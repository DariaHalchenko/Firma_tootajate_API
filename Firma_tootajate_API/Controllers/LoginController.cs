using Firma_tootajate_API.Data;
using Firma_tootajate_API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Firma_tootajate_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public LoginController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] Login login)
        {
            if (login == null || string.IsNullOrEmpty(login.Email) || string.IsNullOrEmpty(login.Parool))
                return BadRequest("E-posti aadress ja parool on kohustuslikud");

            var tootaja = _context.Tootajates.FirstOrDefault(u => u.Email == login.Email);
            if (tootaja == null)
                return Unauthorized("Kasutajat ei leitud");

            bool validPassword = BCrypt.Net.BCrypt.Verify(login.Parool, tootaja.Parool);
            if (!validPassword)
                return Unauthorized("Vale parool");

            return Ok(new
            {
                tootaja.Id,
                tootaja.Nimi,
                tootaja.Email,
                tootaja.IsAdmin,
                tootaja.Amet,
                tootaja.Tunnitasu
            });
        }
    }
}