using LBAChamps.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBAChamps.Controllers;

[ApiController]
[Route("api/estatisticaspartidas")]                 // /api/estatisticaspartidas
public class EstatisticasPartidasApiController : ControllerBase
{
    private readonly LigaContext _db;
    public EstatisticasPartidasApiController(LigaContext db) => _db = db;

    // GET /api/estatisticaspartidas?partidaId=3&minPontos=20
    [HttpGet]
    public async Task<IActionResult> Get(
        [FromQuery] int? partidaId,
        [FromQuery] int? jogadorId,
        [FromQuery] int? minPontos)
    {
        var q = _db.EstatisticasPartidas
            .Include(e => e.Jogador).ThenInclude(j => j.Time)
            .Include(e => e.Partida)
            .AsQueryable();

        if (partidaId != null) q = q.Where(e => e.IdPartida == partidaId);
        if (jogadorId != null) q = q.Where(e => e.IdJogador == jogadorId);
        if (minPontos != null) q = q.Where(e => e.Pontos >= minPontos);

        var data = await q.Select(e => new
        {
            e.IdEstatistica,
            e.Partida.IdPartida,
            Jogador = e.Jogador.Nome,
            Time = e.Jogador.Time.Nome,
            e.Pontos,
            e.Rebotes,
            e.Assistencias,
            e.RoubosBola,
            e.Tocos,
            e.Faltas
        })
        .AsNoTracking()
        .ToListAsync();

        return Ok(data);
    }

    // GET /api/estatisticaspartidas/10
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var est = await _db.EstatisticasPartidas
            .Include(j => j.Jogador)
            .Include(p => p.Partida)
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.IdEstatistica == id);

        return est is null ? NotFound() : Ok(est);
    }

    // GET api/estatisticaspartidas/by-liga-sum/3
    [HttpGet("by-liga-sum/{ligaId:int}")]
    public async Task<IActionResult> SumPorLiga(int ligaId)
    {
        var stats = await _db.EstatisticasPartidas
            .Where(e => e.Partida!.IdLiga == ligaId)
            .Include(e => e.Jogador!).ThenInclude(j => j.Time!)
            .GroupBy(e => new { e.IdJogador, e.Jogador!.Nome, Time = e.Jogador!.Time!.Nome })
            .Select(g => new {
                g.Key.IdJogador,
                Jogador = g.Key.Nome,
                Time = g.Key.Time,

                GP = g.Count(),
                PTS = g.Sum(x => x.Pontos),
                REB = g.Sum(x => x.Rebotes),
                AST = g.Sum(x => x.Assistencias),
                STL = g.Sum(x => x.RoubosBola),
                BLK = g.Sum(x => x.Tocos),
                PF = g.Sum(x => x.Faltas),

                AvgPts = Math.Round(g.Average(x => x.Pontos), 2),
                AvgReb = Math.Round(g.Average(x => x.Rebotes), 2),
                AvgAst = Math.Round(g.Average(x => x.Assistencias), 2),

                EFF = g.Sum(x => x.Pontos + x.Rebotes + x.Assistencias +
                                 x.RoubosBola + x.Tocos - x.Faltas),

                DD = g.Count(x =>
                       ((x.Pontos >= 10 ? 1 : 0) +
                        (x.Rebotes >= 10 ? 1 : 0) +
                        (x.Assistencias >= 10 ? 1 : 0) +
                        (x.RoubosBola >= 10 ? 1 : 0) +
                        (x.Tocos >= 10 ? 1 : 0)) >= 2),

                TD = g.Count(x =>
                       ((x.Pontos >= 10 ? 1 : 0) +
                        (x.Rebotes >= 10 ? 1 : 0) +
                        (x.Assistencias >= 10 ? 1 : 0) +
                        (x.RoubosBola >= 10 ? 1 : 0) +
                        (x.Tocos >= 10 ? 1 : 0)) >= 3)
            })
            .OrderByDescending(x => x.AvgPts)   // ou .EFF
            .ToListAsync();

        return Ok(stats);
    }

    // GET api/estatisticaspartidas/by-jogador-hist/12
    [HttpGet("by-jogador-hist/{jogadorId:int}")]
    public async Task<IActionResult> HistoricoJogador(int jogadorId)
    {
        var hist = await _db.EstatisticasPartidas
            .Where(e => e.IdJogador == jogadorId)
            .Include(e => e.Partida!)
            .OrderBy(e => e.Partida!.DataHora)
            .Select(e => new {
                e.Partida!.IdPartida,
                Data = e.Partida.DataHora,
                Adversario = e.Partida.TimeCasa!.IdTime == e.IdJogador
                             ? e.Partida.TimeFora!.Nome
                             : e.Partida.TimeCasa!.Nome,

                e.Pontos,
                e.Rebotes,
                e.Assistencias,
                e.RoubosBola,
                e.Tocos,
                e.Faltas
            })
            .AsNoTracking()
            .ToListAsync();

        return Ok(hist);
    }
}
