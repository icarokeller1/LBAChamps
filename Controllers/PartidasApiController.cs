using LBAChamps.Data;
using LBAChamps.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/partidas")]               // GET api/partidas
public class PartidasApiController : ControllerBase
{
    private readonly LigaContext _db;
    public PartidasApiController(LigaContext db) => _db = db;

    /// <summary>
    /// Proximas partidas nos proximos N dias (default 7).
    /// </summary>
    [HttpGet("proximas")]
    public async Task<IEnumerable<object>> Proximas([FromQuery] int dias = 7)
    {
        var ate = DateTime.Now.AddDays(dias);

        return await _db.Partidas
            .Include(p => p.TimeCasa).Include(p => p.TimeFora).Include(p => p.Liga)
            .Where(p => p.DataHora >= DateTime.Now && p.DataHora <= ate)
            .OrderBy(p => p.DataHora)
            .Select(p => new
            {
                p.IdPartida,
                p.DataHora,
                Mandante = p.TimeCasa.Nome,
                Visitante = p.TimeFora.Nome,
                p.Liga.Nome
            })
            .AsNoTracking()
            .ToListAsync();
    }

    // GET api/partidas/by-liga/3
    [HttpGet("by-liga/{ligaId:int}")]
    public async Task<IActionResult> PorLiga(int ligaId)
    {
        var lista = await _db.Partidas
            .Where(p => p.IdLiga == ligaId)
            .OrderBy(p => p.DataHora)
            .Select(p => new {
                p.IdPartida,
                Descricao = p.DataHora.ToString("dd/MM") + " - "
                          + p.TimeCasa.Nome + " x " + p.TimeFora.Nome
            })
            .AsNoTracking()
            .ToListAsync();

        return Ok(lista);
    }

    // GET api/partidas/by-liga-full/3
    [HttpGet("by-liga-full/{ligaId:int}")]
    public async Task<IActionResult> PorLigaCompleto(int ligaId)
    {
        var lista = await _db.Partidas
            .Where(p => p.IdLiga == ligaId)
            .Include(p => p.TimeCasa)
            .Include(p => p.TimeFora)
            .OrderBy(p => p.DataHora)
            .Select(p => new
            {
                p.IdPartida,
                p.DataHora,                 // YYYY-MM-DDTHH:mm:ssZ
                p.Local,

                p.IdTimeCasa,
                NomeTimeCasa = p.TimeCasa.Nome,
                p.PlacarCasa,

                p.IdTimeFora,
                NomeTimeFora = p.TimeFora.Nome,
                p.PlacarFora
            })
            .AsNoTracking()
            .ToListAsync();

        return Ok(lista);
    }

    [HttpPost("autosave")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AutoSave([FromBody] PartidaScoutViewModel vm)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        Partida partida;
        if (vm.IdPartida.HasValue)
        {
            partida = await _db.Partidas
                .Include(p => p.Estatisticas)
                .FirstOrDefaultAsync(p => p.IdPartida == vm.IdPartida.Value);

            if (partida == null)
                return NotFound();

            // Atualiza campos básicos
            partida.DataHora = vm.DataHora!.Value;
            partida.Local = vm.Local;
            // Remove estatísticas antigas
            _db.EstatisticasPartidas.RemoveRange(partida.Estatisticas);
        }
        else
        {
            partida = new Partida
            {
                IdLiga = vm.IdLiga!.Value,
                IdTimeCasa = vm.IdTimeCasa!.Value,
                IdTimeFora = vm.IdTimeFora!.Value,
                DataHora = vm.DataHora!.Value,
                Local = vm.Local
            };
            _db.Partidas.Add(partida);
        }

        // Adiciona estatísticas
        var estatList = vm.Players
            .Where(p => p.Pontos > 0 || p.Rebotes > 0 || p.Assistencias > 0 || p.RoubosBola > 0 || p.Tocos > 0 || p.Faltas > 0)
            .Select(p => new EstatisticasPartida
            {
                IdPartida = partida.IdPartida,
                IdJogador = p.IdJogador,
                Pontos = p.Pontos,
                Rebotes = p.Rebotes,
                Assistencias = p.Assistencias,
                RoubosBola = p.RoubosBola,
                Tocos = p.Tocos,
                Faltas = p.Faltas
            });
        _db.EstatisticasPartidas.AddRange(estatList);

        await _db.SaveChangesAsync();

        return Ok(new { idPartida = partida.IdPartida });
    }
}
