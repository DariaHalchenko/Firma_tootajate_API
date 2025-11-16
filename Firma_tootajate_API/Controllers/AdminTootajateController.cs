using Firma_tootajate_API.Data;
using Firma_tootajate_API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Firma_tootajate_API.Controllers
{
    [Route("api/admin/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminTootajateController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AdminTootajateController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Filtreerimine nime ja ameti järgi
        // Sorteerimine tunnitasu ja nime järgi
        // GET: api/admin/Tootajate?nimi=&amet=&tunnitasu=
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? nimi = null,[FromQuery] string? amet = null,
            [FromQuery(Name = "tunnitasu")] string? sortByTunnitasu = null)
        {
            var paring = _context.Tootajates.AsQueryable();

            // Filtreerimine nime järgi
            if (!string.IsNullOrEmpty(nimi))
                paring = paring.Where(t => t.Nimi.ToLower().Contains(nimi.ToLower()));
            // Filtreerimine ameti järgi
            if (!string.IsNullOrEmpty(amet))
                paring = paring.Where(t => t.Amet.ToLower().Contains(amet.ToLower()));

            // Sorteerimine tunnitasu ja nime järgi
            if (!string.IsNullOrEmpty(sortByTunnitasu))
                paring = paring.OrderBy(t => t.Tunnitasu); // tunnitasu sorteerimine
            else
                paring = paring.OrderBy(t => t.Nimi); // sorteerimine nime järgi

            var tootajad = await paring.ToListAsync();

            return Ok(tootajad.Select(t => new
            {
                t.Nimi,
                t.Isikukood,
                t.Amet,
                t.Tunnitasu,
                t.Email
            }));
        }

        // GET: api/admin/Tootajate/{nimi} - saada konkreetse töötaja andmed nimepidi
        [HttpGet("{nimi}")]
        public async Task<IActionResult> Get(string nimi)
        {
            var tootaja = await _context.Tootajates
                .FirstOrDefaultAsync(t => t.Nimi.ToLower() == nimi.ToLower());

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

        // POST: api/admin/Tootajate - lisada uus töötaja
        [HttpPost]
        public async Task<IActionResult> PostTootajate([FromBody] TootajaAdmin dto)
        {
            var tootaja = await _context.Tootajates.AnyAsync(t => t.Email.ToLower() == dto.Email.ToLower());
            if (tootaja) return BadRequest("Sellise e-posti aadressiga töötaja on juba olemas.");
            
            var nimiExists = await _context.Tootajates.AnyAsync(t => t.Nimi.ToLower() == dto.Nimi.ToLower());
            if (nimiExists)
                return BadRequest("Sellise nimega töötaja on juba olemas.");
            
            var uus_tootaja = new Tootajate
            {
                Nimi = dto.Nimi,
                Isikukood = dto.Isikukood,
                Amet = dto.Amet,
                Tunnitasu = dto.Tunnitasu,
                Email = dto.Email,
                Parool = BCrypt.Net.BCrypt.HashPassword(dto.Parool) 
            };

            _context.Tootajates.Add(uus_tootaja);
            await _context.SaveChangesAsync();

            return Ok(uus_tootaja);
        }

        // PUT: api/admin/Tootajate/{nimi} - muuta töötaja andmeid nimega
        [HttpPut("{nimi}")]
        public async Task<IActionResult> PutTootajate(string nimi, [FromBody] TootajaUpdateAdmin dto)
        {
            var tootaja = await _context.Tootajates
                .FirstOrDefaultAsync(t => t.Nimi.ToLower() == nimi.ToLower());

            if (tootaja == null)
            {
                return NotFound("Töötajat ei leitud");
            }   

            tootaja.Amet = dto.Amet;
            tootaja.Tunnitasu = dto.Tunnitasu;

            await _context.SaveChangesAsync();
            return Ok(tootaja);
        }

        // DELETE: api/admin/Tootajate/{nimi} - kustuta töötaja nimega
        [HttpDelete("{nimi}")]
        public async Task<IActionResult> Delete(string nimi)
        {
            var tootaja = await _context.Tootajates
                .FirstOrDefaultAsync(t => t.Nimi.ToLower() == nimi.ToLower());

            if (tootaja == null)
            {
                return NotFound("Töötajat ei leitud");
            }

            _context.Tootajates.Remove(tootaja);
            await _context.SaveChangesAsync();

            return Ok(new 
            { 
                Message = "Töötaja eemaldatud" 
            });
        }

        // ARUANNE: Kõigi töötajate palk
        // GET: api/admin/Tootajate/aruanne/palk
        [HttpGet("aruanne/palk")]
        public async Task<IActionResult> SalaryReport()
        {
            var worktimes = await _context.Worktimes
                .Include(w => w.Tootajate)
                .ToListAsync(); // laadime kõik andmed mällu

            var aruanne = worktimes
                .GroupBy(w => w.Tootajate.Nimi)
                .Select(g => new
                {
                    Nimi = g.Key,
                    Tunnitasu = g.First().Tootajate.Tunnitasu,
                    Palk = g.Sum(w => ((decimal)(w.Valjapaas - w.Sissepaas).TotalHours) * w.Tootajate.Tunnitasu)
                })
                .ToList();

            return Ok(aruanne);
        }


        // ARUANNE: Teatud päeval töötanud töötajad
        // GET: api/admin/Tootajate/aruanne/{kuupaev}
        [HttpGet("aruanne/{kuupaev}")]
        public async Task<IActionResult> DailyReport(DateOnly kuupaev)
        {
            var aruanne = await _context.Worktimes
                .Include(w => w.Tootajate)
                .Where(w => w.Kuupaev == kuupaev)
                .Select(w => new
                {
                    w.Tootajate.Nimi,
                    w.Kuupaev,
                    w.Sissepaas,
                    w.Valjapaas,
                    Palk = ((decimal)(w.Valjapaas - w.Sissepaas).TotalHours) * w.Tootajate.Tunnitasu
                })
                .ToListAsync();

            return Ok(aruanne);
        }
    }
}
