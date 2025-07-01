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
    /// Próximas partidas nos próximos N dias (default 7), sem filtro por liga.
    /// </summary>
    [HttpGet("proximas")]
    public async Task<IActionResult> Proximas([FromQuery] int dias = 7)
    {
        var agora = DateTime.Now.AddHours(-3);
        var ate = agora.AddDays(dias);

        var lista = await _db.Partidas
            .Include(p => p.TimeCasa)
            .Include(p => p.TimeFora)
            .Include(p => p.Liga)
            .Where(p => p.DataHora >= agora && p.DataHora <= ate)
            .OrderBy(p => p.DataHora)
            .Select(p => new {
                p.IdPartida,
                Descricao = p.DataHora.ToString("dd/MM HH/mm")
                          + " - "
                          + p.TimeCasa!.Nome
                          + " x "
                          + p.TimeFora!.Nome,
                Liga      = p.Liga!.Nome
            })
            .AsNoTracking()
            .ToListAsync();

        return Ok(lista);
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
                p.DataHora,
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

        // Calcular e atribuir placar antes de salvar estatísticas
        var casaTotal = vm.Players
            .Where(p => p.IdTime == partida.IdTimeCasa)
            .Sum(p => p.Pontos);
        var foraTotal = vm.Players
               .Where(p => p.IdTime == partida.IdTimeFora)
               .Sum(p => p.Pontos);
        partida.PlacarCasa = casaTotal;
        partida.PlacarFora = foraTotal;

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

    /// <summary>
    /// Retorna a partida que está em andamento (até 90min após o início), sem filtro por liga, e suas estatísticas.
    /// </summary>
    [HttpGet("atual")]
    public async Task<IActionResult> PartidaAtual()
    {
        var agora = DateTime.Now.AddHours(-3);
        var limite = agora.AddMinutes(-90);

        var partida = await _db.Partidas
            .Where(p => p.DataHora <= agora && p.DataHora >= limite)
            .Include(p => p.TimeCasa)
            .Include(p => p.TimeFora)
            .Include(p => p.Liga)
            .Include(p => p.Estatisticas)
                .ThenInclude(e => e.Jogador)
                    .ThenInclude(j => j.Time)
            .OrderBy(p => p.DataHora) // pega a mais antiga que ainda está rolando
            .AsNoTracking()
            .FirstOrDefaultAsync();

        if (partida == null)
            return NoContent();  // 204 quando não há jogo em andamento

        var result = new
        {
            partida = new
            {
                partida.IdPartida,
                partida.DataHora,
                partida.Local,
                Mandante = partida.TimeCasa!.Nome,
                Visitante = partida.TimeFora!.Nome,
                Liga = partida.Liga!.Nome
            },
            estatisticas = partida.Estatisticas.Select(e => new {
                e.IdEstatistica,
                e.IdPartida,
                NumeroCamisa = e.Jogador!.NumeroCamisa,
                Jogador = e.Jogador!.Nome,
                Time = e.Jogador.Time!.Nome,
                e.Pontos,
                e.Rebotes,
                e.Assistencias,
                e.RoubosBola,
                e.Tocos,
                e.Faltas
            })
        };

        return Ok(result);
    }
}
