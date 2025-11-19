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
        // GET: api/admin/Tootajate?nimi=&amet=&sortAsc=true
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? nimi = null,
            [FromQuery] string? amet = null,
            [FromQuery] bool? sortAsc = null)
        {
            var paring = _context.Tootajates.AsQueryable();

            // Фильтрация по имени
            if (!string.IsNullOrEmpty(nimi))
                paring = paring.Where(t => t.Nimi.ToLower().Contains(nimi.ToLower()));

            // Фильтрация по должности
            if (!string.IsNullOrEmpty(amet))
                paring = paring.Where(t => t.Amet.ToLower().Contains(amet.ToLower()));

            // Сортировка по часовке
            if (sortAsc.HasValue)
            {
                paring = sortAsc.Value
                    ? paring.OrderBy(t => t.Tunnitasu)
                    : paring.OrderByDescending(t => t.Tunnitasu);
            }
            else
            {
                paring = paring.OrderBy(t => t.Nimi); // по умолчанию сортировка по имени
            }

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
        public async Task<IActionResult> Palgaaruanne([FromQuery] int aasta, [FromQuery] int kuu)
        {
            if (aasta <= 0 || kuu <= 0 || kuu > 12)
                return BadRequest("õiged parameetrid aasta ja kuu");

            var worktimes = await _context.Worktimes
                .Include(w => w.Tootajate)
                .Where(w => w.Kuupaev.Year == aasta && w.Kuupaev.Month == kuu)
                .ToListAsync();

            var aruanne = worktimes
                .GroupBy(w => w.Tootajate.Nimi)
                .Select(g => new
                {
                    Nimi = g.Key,
                    KokkuTunnid = Math.Round(g.Sum(w => w.Valjapaas.HasValue ? (w.Valjapaas.Value - w.Sissepaas).TotalHours : 0), 2),
                    Palk = Math.Round(g.Sum(w => w.Valjapaas.HasValue ? (decimal)(w.Valjapaas.Value - w.Sissepaas).TotalHours * w.Tootajate.Tunnitasu : 0), 2)
                })
                .ToList();

            var kuuNimed = new string[]
            {
                "Jaanuar", "Veebruar", "Märts", "Aprill", "Mai", "Juuni",
                "Juuli", "August", "September", "Oktoober", "November", "Detsember"
            };

            var kuuNimi = kuuNimed[kuu - 1];

            return Ok(new
            {
                Kuu = kuuNimi,
                Aasta = aasta,
                Andmed = aruanne
            });
        }



        // ARUANNE: Teatud päeval töötanud töötajad
        // GET: api/admin/Tootajate/aruanne/{kuupaev}
        [HttpGet("aruanne/{kuupaev}")]
        public async Task<IActionResult> Paevaaruanne(DateOnly kuupaev)
        {
            var tootajad = await _context.Tootajates.ToListAsync();
            var worktimes = await _context.Worktimes
                .Where(w => w.Kuupaev == kuupaev)
                .ToListAsync();

            var aruanne = new List<object>();

            foreach (var t in tootajad)
            {
                var w = worktimes.FirstOrDefault(x => x.TootajateId == t.Id);

                if (w == null)
                {
                    aruanne.Add(new
                    {
                        t.Nimi,
                        Sissepaas = "-",
                        Valjapaas = "-",
                        Tunnid = 0,
                        Staatus = "Puudub"
                    });
                    continue;
                }

                var end = w.Valjapaas ?? TimeOnly.FromDateTime(DateTime.Now);
                var hours = (decimal)(end - w.Sissepaas).TotalHours;

                aruanne.Add(new
                {
                    t.Nimi,
                    w.Sissepaas,
                    Valjapaas = w.Valjapaas?.ToString() ?? "Pole lahkunud",
                    Tunnid = Math.Round(hours, 2),
                    Staatus = w.Valjapaas == null ? "Tööl" : "Lõpetanud"
                });
            }

            return Ok(aruanne);
        }

    }
}
