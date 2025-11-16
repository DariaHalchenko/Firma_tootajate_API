using Firma_tootajate_API.Data;
using Firma_tootajate_API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

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
            if (tootaja == null) return Unauthorized("Kasutajat ei leitud");

            bool validParool = BCrypt.Net.BCrypt.Verify(login.Parool, tootaja.Parool);
            if (!validParool) return Unauthorized("Vale parool");

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, tootaja.Nimi),
                new Claim(ClaimTypes.Email, tootaja.Email),
                new Claim(ClaimTypes.Role, tootaja.IsAdmin ? "Admin" : "Töötaja")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("YourSuperSecretKey1234567890123456!"));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddHours(5),
                signingCredentials: creds
            );

            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                role = tootaja.IsAdmin ? "Admin" : "Töötaja",
                name = tootaja.Nimi
            });
        }
    }
}