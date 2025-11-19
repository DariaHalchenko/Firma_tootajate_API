using Firma_tootajate_API.Data;
using Firma_tootajate_API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Firma_tootajate_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Töötaja")]
    public class WorktimeController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public WorktimeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Worktime/tootaja/{nimi}
        [HttpGet("tootaja/{nimi}")]
        public async Task<IActionResult> GetWorktime(string nimi)
        {
            var tootaja = await _context.Tootajates
                .FirstOrDefaultAsync(t => t.Nimi.ToLower() == nimi.ToLower());

            if (tootaja == null)
                return NotFound("Töötajat ei leitud");

            var worktimes = await _context.Worktimes
                .Where(w => w.TootajateId == tootaja.Id)
                .ToListAsync();

            var result = worktimes.Select(w =>
            {
                var end = w.Valjapaas ?? TimeOnly.FromDateTime(DateTime.Now); 
                var hours = (decimal)(end - w.Sissepaas).TotalHours;

                return new
                {
                    w.Kuupaev,
                    w.Sissepaas,
                    Valjapaas = w.Valjapaas?.ToString() ?? "Pole lahkunud",
                    Palk = Math.Round(hours * tootaja.Tunnitasu, 2),
                    Tunnid = Math.Round(hours, 2)
                };
            });


            return Ok(new
            {
                tootaja.Nimi,
                tootaja.Email,
                tootaja.Amet,
                Tunnitasu = tootaja.Tunnitasu,
                Worktimes = result
            });
        }

        // POST: api/Worktime/tootaja/lisada/{nimi}
        [HttpPost("tootaja/lisada/{nimi}")]
        public async Task<IActionResult> PostWorktime(string nimi, [FromBody] CreateWorktime dto)
        {
            var tootaja = await _context.Tootajates
                .FirstOrDefaultAsync(t => t.Nimi.ToLower() == nimi.ToLower());

            if (tootaja == null)
                return NotFound("Töötajat ei leitud");

            var worktime = new Worktime
            {
                Kuupaev = dto.Kuupaev,
                Sissepaas = dto.Sissepaas,
                Valjapaas = dto.Valjapaas, // может быть null
                TootajateId = tootaja.Id
            };

            _context.Worktimes.Add(worktime);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "Tööaeg lisatud",
                tootaja = tootaja.Nimi,
                worktime.Kuupaev,
                worktime.Sissepaas,
                worktime.Valjapaas
            });
        }

        // PUT: api/Worktime/valjapaas/{id}
        // Обновление времени выхода для конкретного рабочего дня
        [HttpPut("valjapaas/{nimi}/{kuupaev}")]
        public async Task<IActionResult> UpdateValjapaasByName(string nimi, DateOnly kuupaev, [FromBody] UpdateValjapaas dto)
        {
            var worktime = await _context.Worktimes
                .Include(w => w.Tootajate)
                .FirstOrDefaultAsync(w => w.Kuupaev == kuupaev && w.Tootajate.Nimi.ToLower() == nimi.ToLower());

            if (worktime == null)
                return NotFound("Tööaega ei leitud");

            // конвертируем строку в TimeOnly
            if (!TimeOnly.TryParse(dto.Valjapaas, out var valjapaas))
                return BadRequest("Vale aeg formaat");

            worktime.Valjapaas = valjapaas;
            await _context.SaveChangesAsync();

            var hours = (decimal)(worktime.Valjapaas.Value - worktime.Sissepaas).TotalHours;

            return Ok(new
            {
                Message = "Väljalogimise aeg uuendatud",
                worktime.Kuupaev,
                worktime.Sissepaas,
                worktime.Valjapaas,
                Tunnid = Math.Round(hours, 2),
                Palk = Math.Round(hours * worktime.Tootajate.Tunnitasu, 2)
            });
        }
    }
}
