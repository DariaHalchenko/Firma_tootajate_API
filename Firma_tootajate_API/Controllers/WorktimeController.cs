using Firma_tootajate_API.Data;
using Firma_tootajate_API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Firma_tootajate_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WorktimeController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public WorktimeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Worktime/tootaja/{nimi}
        [HttpGet("tootaja/{nimi}")]
        public async Task<IActionResult> GetWorktimeWithSalary(string nimi)
        {
            var tootaja = await _context.Tootajates
                .FirstOrDefaultAsync(t => t.Nimi.ToLower() == nimi.ToLower());

            if (tootaja == null)
                return NotFound("Töötajat ei leitud");

            var worktimes = await _context.Worktimes
                .Where(w => w.TootajateId == tootaja.Id)
                .ToListAsync();

            var result = worktimes.Select(w => new
            {
                w.Kuupaev,
                w.Sissepaas,
                w.Valjapaas,
                Palk = ((decimal)(w.Valjapaas - w.Sissepaas).TotalHours) * tootaja.Tunnitasu
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
        public async Task<IActionResult> AddWorktime(string nimi, [FromBody] Worktime worktime)
        {
            var tootaja = await _context.Tootajates
                .FirstOrDefaultAsync(t => t.Nimi.ToLower() == nimi.ToLower());

            if (tootaja == null)
                return NotFound("Töötajat ei leitud");

            worktime.Id = 0;
            worktime.TootajateId = tootaja.Id;
            worktime.Tootajate = null;

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
    }
}
