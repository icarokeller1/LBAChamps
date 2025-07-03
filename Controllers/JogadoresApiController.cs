using LBAChamps.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBAChamps.Controllers;

[ApiController]
[Route("api/jogadores")]
public class JogadoresApiController : ControllerBase
{
    private readonly LigaContext _db;
    public JogadoresApiController(LigaContext db) => _db = db;


    [HttpGet]
    public async Task<IActionResult> Get(
        [FromQuery] int? timeId,
        [FromQuery] string? nome)
    {
        var q = _db.Jogadores.Include(t => t.Time).AsQueryable();

        if (timeId != null) q = q.Where(j => j.IdTime == timeId);
        if (!string.IsNullOrWhiteSpace(nome))
            q = q.Where(j => j.Nome.Contains(nome));

        var data = await q
            .OrderBy(j => j.Nome)
            .Select(j => new
            {
                j.IdJogador,
                j.Nome,
                j.Posicao,
                j.NumeroCamisa,
                Time = j.Time.Nome
            })
            .AsNoTracking()
            .ToListAsync();

        return Ok(data);
    }


    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var jog = await _db.Jogadores
            .Include(t => t.Time)
            .AsNoTracking()
            .FirstOrDefaultAsync(j => j.IdJogador == id);

        return jog is null ? NotFound() : Ok(jog);
    }


    [HttpGet("by-partida/{partidaId:int}")]
    public async Task<IActionResult> PorPartida(int partidaId)
    {
        var partida = await _db.Partidas
            .Include(p => p.TimeCasa!).ThenInclude(t => t.Jogadores)
            .Include(p => p.TimeFora!).ThenInclude(t => t.Jogadores)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.IdPartida == partidaId);

        if (partida is null) return NotFound();

        var jogadores = partida.TimeCasa.Jogadores
            .Concat(partida.TimeFora.Jogadores)
            .OrderBy(j => j.Nome)
            .Select(j => new {
                j.IdJogador,
                Nome = $"{j.Nome} ({j.Time.Nome})"
            });

        return Ok(jogadores);
    }


    [HttpGet("by-time/{timeId:int}")]
    public async Task<IActionResult> PorTime(int timeId)
    {
        var list = await _db.Jogadores
            .Include(j => j.Time)
            .Where(j => j.IdTime == timeId)
            .OrderBy(j => j.Nome)
            .Select(j => new {
                idJogador = j.IdJogador,
                idTime = j.IdTime,
                nome = j.Nome,
                timeNome = j.Time.Nome
            })
            .ToListAsync();

        return Ok(list);
    }
}
