using LBAChamps.Data;
using LBAChamps.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBAChamps.Controllers;

[ApiController]
[Route("api/ligas")]
public class LigasApiController : ControllerBase
{
    private readonly LigaContext _db;
    public LigasApiController(LigaContext db) => _db = db;


    [HttpGet]
    public async Task<IEnumerable<Liga>> Get(
        [FromQuery] string? status)
    {
        IQueryable<Liga> q = _db.Ligas;

        if (!string.IsNullOrEmpty(status))
            q = q.Where(l => l.Status == status);

        return await q
            .OrderBy(l => l.Nome)
            .AsNoTracking()
            .ToListAsync();
    }


    [HttpGet("{id:int}")]
    public async Task<ActionResult<Liga>> Get(int id)
    {
        var liga = await _db.Ligas
                            .AsNoTracking()
                            .FirstOrDefaultAsync(l => l.IdLiga == id);

        return liga is null ? NotFound() : liga;
    }
}
