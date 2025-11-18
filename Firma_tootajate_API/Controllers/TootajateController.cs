using Firma_tootajate_API.Data;
using Firma_tootajate_API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Firma_tootajate_API.Controllers
{
    [Route("api/tootaja/[controller]")]
    [ApiController]
    [Authorize(Roles = "Töötaja")]
    public class TootajateController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TootajateController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Tootajate/{nimi} - saada töötaja andmed
        [HttpGet("{nimi}")]
        public async Task<IActionResult> GetTootaja(string nimi)
        {
            var tootaja = await _context.Tootajates
                .FirstOrDefaultAsync(e => e.Nimi.ToLower() == nimi.ToLower());

            if (tootaja == null)
            {
                return NotFound("Töötajat ei leitud");
            }

            return Ok(new
            {
                tootaja.Nimi,
                tootaja.Isikukood,
                tootaja.Amet,
                tootaja.Tunnitasu,
                tootaja.Email
            });
        }

        // PUT: api/Tootajate/{nimi} - töötaja muudab oma andmeid
        [HttpPut("{nimi}")]
        public async Task<IActionResult> PutTootaja(string nimi, [FromBody] TootajaUpdateKasutajale dto)
        {
            var tootaja = await _context.Tootajates
                .FirstOrDefaultAsync(e => e.Nimi.ToLower() == nimi.ToLower());

            if (tootaja == null)
            {
                return NotFound("Töötajat ei leitud");
            }

            tootaja.Nimi = dto.Nimi;
            tootaja.Email = dto.Email;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "Andmed uuendatud",
                tootaja.Nimi,
                tootaja.Email
            });
        }
    }
}
//