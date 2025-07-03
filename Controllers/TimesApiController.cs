using LBAChamps.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBAChamps.Controllers;

[ApiController]
[Route("api/times")]
public class TimesApiController : ControllerBase
{
    private readonly LigaContext _db;
    public TimesApiController(LigaContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> Get(
        [FromQuery] int? ligaId,
        [FromQuery] string? estado,
        [FromQuery] string? cidade)
    {
        var q = _db.Times.Include(l => l.Liga).AsQueryable();

        if (ligaId != null) q = q.Where(t => t.IdLiga == ligaId);
        if (!string.IsNullOrWhiteSpace(estado)) q = q.Where(t => t.Estado == estado);
        if (!string.IsNullOrWhiteSpace(cidade)) q = q.Where(t => t.Cidade.Contains(cidade));

        var lista = await q
            .OrderBy(t => t.Nome)
            .Select(t => new
            {
                t.IdTime,
                t.Nome,
                t.Cidade,
                t.Estado,
                Liga = t.Liga.Nome
            })
            .AsNoTracking()
            .ToListAsync();

        return Ok(lista);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var time = await _db.Times
            .Include(l => l.Liga)
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.IdTime == id);

        return time is null ? NotFound() : Ok(time);
    }

    [HttpGet("by-liga/{ligaId:int}")]
    public async Task<IActionResult> PorLiga(int ligaId)
    {
        var lista = await _db.Times
            .Where(t => t.IdLiga == ligaId)
            .OrderBy(t => t.Nome)
            .Select(t => new { t.IdTime, t.Nome })
            .AsNoTracking()
            .ToListAsync();

        return Ok(lista);
    }

    [HttpGet("{id:int}/logo")]
    public async Task<IActionResult> GetLogo(int id)
    {
        var t = await _db.Times
            .AsNoTracking()
            .Select(x => new { x.IdTime, x.Logo, x.LogoMimeType })
            .FirstOrDefaultAsync(x => x.IdTime == id);

        if (t?.Logo is null) return NotFound();
        return File(t.Logo, t.LogoMimeType ?? "image/png");
    }
}
