using Firma_tootajate_API.Data;
using Firma_tootajate_API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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

        // GET: api/Worktime 
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var worktimes = await _context.Worktimes
                .Include(w => w.Tootajate) 
                .ToListAsync();

            var result = worktimes.Select(w => new
            {
                w.Id,
                w.TootajateId,
                Nimi = w.Tootajate?.Nimi,
                w.Kuupaev,
                w.Sissepaas,
                w.Valjapaas
            });

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] Worktime worktime)
        {
            var employeeExists = await _context.Tootajates.AnyAsync(e => e.Id == worktime.TootajateId);
            if (!employeeExists)
                return BadRequest("Töötajat ei leitud");

            _context.Worktimes.Add(worktime);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "Tööaja registreerimine lisatud",
                worktime.Id,
                worktime.TootajateId,
                worktime.Kuupaev,
                worktime.Sissepaas,
                worktime.Valjapaas
            });
        }
    }
}