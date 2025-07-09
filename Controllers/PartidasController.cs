using LBAChamps.Data;
using LBAChamps.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Threading;

namespace LBAChamps.Controllers;

public class PartidasController : Controller
{
    private readonly LigaContext _db;
    public PartidasController(LigaContext context) => _db = context;

    public async Task<IActionResult> Index(int? ligaId, int? timeId)
    {
        var q = _db.Partidas
            .Include(p => p.TimeCasa)
            .Include(p => p.TimeFora)
            .Include(p => p.Liga)
            .AsQueryable();

        if (ligaId is not null)
            q = q.Where(p => p.IdLiga == ligaId);
        if (timeId is not null)
            q = q.Where(p => p.IdTimeCasa == timeId || p.IdTimeFora == timeId);

        ViewData["Ligas"] = new SelectList(_db.Ligas.OrderBy(l => l.Nome),
                                           "IdLiga", "Nome", ligaId);
        var timesQuery = _db.Times.AsQueryable();
        if (ligaId is not null) timesQuery = timesQuery.Where(t => t.IdLiga == ligaId);
        ViewData["Times"] = new SelectList(timesQuery.OrderBy(t => t.Nome),
                                           "IdTime", "Nome", timeId);

        var lista = await q.AsNoTracking().ToListAsync();

        var hoje = DateTime.Today;
        lista = lista
            .OrderByDescending(p => p.DataHora.Date == hoje)
            .ThenBy(p => p.DataHora)
            .ToList();

        return View(lista);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id is null) return NotFound();

        var partida = await _db.Partidas
            .Include(p => p.TimeCasa)
            .Include(p => p.TimeFora)
            .Include(p => p.Liga)
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.IdPartida == id);

        return partida is null ? NotFound() : View(partida);
    }


    public IActionResult Create()
    {
        ViewData["Ligas"] = new SelectList(_db.Ligas.OrderBy(l => l.Nome),
                                           "IdLiga", "Nome");

        ViewData["Times"] = new SelectList(Enumerable.Empty<object>(), "IdTime", "Nome");
        return View();
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("DataHora,Local,IdTimeCasa,IdTimeFora,IdLiga,PlacarCasa,PlacarFora")]
        Partida partida)
    {
        ValidarTimes(partida);

        if (!ModelState.IsValid)
        {
            CarregarDropdowns(partida.IdLiga);
            return View(partida);
        }

        var liga = await _db.Ligas
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.IdLiga == partida.IdLiga);

        if (liga is null)
        {
            ModelState.AddModelError("IdLiga", "Liga não encontrada.");
        }
        else
        {
            var dentroDoIntervalo = partida.DataHora.Date >= liga.DataInicio.ToDateTime(new())
                                 && (liga.DataFim == null || 
                                    partida.DataHora.Date <= liga.DataFim.Value.ToDateTime(new()));

            if (!dentroDoIntervalo)
                ModelState.AddModelError("DataHora",
                    $"Data deve estar entre {liga.DataInicio:dd/MM/yyyy}" +
                    $"{(liga.DataFim != null ? $" e {liga.DataFim:dd/MM/yyyy}" : "")}.");
        }

        _db.Add(partida);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }


    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null) return NotFound();

        var partida = await _db.Partidas.FindAsync(id);
        if (partida is null) return NotFound();

        CarregarDropdowns(partida.IdLiga);
        return View(partida);
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id,
        [Bind("IdPartida,DataHora,Local,IdLiga,PlacarCasa,PlacarFora")]
            Partida partidaPost)
    {
        if (id != partidaPost.IdPartida) return NotFound();

        var original = await _db.Partidas
                                .AsNoTracking()
                                .FirstOrDefaultAsync(p => p.IdPartida == id);
        if (original is null) return NotFound();

        partidaPost.IdTimeCasa = original.IdTimeCasa;
        partidaPost.IdTimeFora = original.IdTimeFora;

        if (partidaPost.IdTimeCasa == partidaPost.IdTimeFora)
            ModelState.AddModelError("IdTimeFora",
                "O time visitante deve ser diferente do mandante.");

        if (!ModelState.IsValid)
        {
            CarregarDropdowns(partidaPost.IdLiga);
            return View(partidaPost);
        }

        var liga = await _db.Ligas
                            .AsNoTracking()
                            .FirstOrDefaultAsync(l => l.IdLiga == partidaPost.IdLiga);
        if (liga is null)
        {
            ModelState.AddModelError("IdLiga", "Liga não encontrada.");
            CarregarDropdowns(partidaPost.IdLiga);
            return View(partidaPost);
        }

        var dentroDoIntervalo = partidaPost.DataHora.Date >= liga.DataInicio.ToDateTime(new())
                             && (liga.DataFim == null ||
                                 partidaPost.DataHora.Date <= liga.DataFim.Value.ToDateTime(new()));
        if (!dentroDoIntervalo)
        {
            ModelState.AddModelError("DataHora",
                $"Data deve estar entre {liga.DataInicio:dd/MM/yyyy}"
              + $"{(liga.DataFim != null ? $" e {liga.DataFim:dd/MM/yyyy}" : "")}.");
            CarregarDropdowns(partidaPost.IdLiga);
            return View(partidaPost);
        }

        _db.Entry(partidaPost).Property(p => p.DataHora).IsModified = true;
        _db.Entry(partidaPost).Property(p => p.Local).IsModified = true;
        _db.Entry(partidaPost).Property(p => p.IdLiga).IsModified = true;
        _db.Entry(partidaPost).Property(p => p.PlacarCasa).IsModified = true;
        _db.Entry(partidaPost).Property(p => p.PlacarFora).IsModified = true;

        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null) return NotFound();

        var partida = await _db.Partidas
                                    .Include(p => p.TimeCasa)
                                    .Include(p => p.TimeFora)
                                    .FirstOrDefaultAsync(p => p.IdPartida == id);

        return partida is null ? NotFound() : View(partida);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var partida = await _db.Partidas.FindAsync(id);
        if (partida is not null) _db.Partidas.Remove(partida);

        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult CreateScout(int? id)
    {
        var vm = new PartidaScoutViewModel
        {
            Ligas = _db.Ligas
                       .OrderBy(l => l.Nome)
                       .Select(l => new SelectListItem(l.Nome, l.IdLiga.ToString()))
                       .ToList()
        };

        if (id.HasValue)
        {
            var partida = _db.Partidas
                             .Include(p => p.TimeCasa)
                             .Include(p => p.TimeFora)
                             .AsNoTracking()
                             .FirstOrDefault(p => p.IdPartida == id.Value);

            if (partida != null)
            {
                vm.IdPartida = partida.IdPartida;
                vm.IdLiga = partida.IdLiga;
                vm.IdTimeCasa = partida.IdTimeCasa;
                vm.IdTimeFora = partida.IdTimeFora;
                vm.DataHora = partida.DataHora;
                vm.Local = partida.Local;

                vm.Times = _db.Times
                              .Where(t => t.IdLiga == partida.IdLiga)
                              .OrderBy(t => t.Nome)
                              .Select(t => new SelectListItem(t.Nome, t.IdTime.ToString()))
                              .ToList();

                var estatList = _db.EstatisticasPartidas
                          .Where(e => e.IdPartida == partida.IdPartida)
                          .ToList();
                
                vm.Players = _db.Jogadores
                           .Include(j => j.Time)
                           .Where(j => j.IdTime == partida.IdTimeCasa
                                    || j.IdTime == partida.IdTimeFora)
                           .OrderBy(j => j.Nome)
                           .AsEnumerable()
                           .Select((j, i) => {
                    var est = estatList.FirstOrDefault(e => e.IdJogador == j.IdJogador);
                    return new PlayerStatsViewModel
                    {
                        IdJogador = j.IdJogador,
                        IdTime = j.IdTime,
                        Nome = $"{j.Nome} ({j.Time!.Nome})",
                        Pontos = est?.Pontos ?? 0,
                        Rebotes = est?.Rebotes ?? 0,
                        Assistencias = est?.Assistencias ?? 0,
                        RoubosBola = est?.RoubosBola ?? 0,
                        Tocos = est?.Tocos ?? 0,
                        Faltas = est?.Faltas ?? 0
                    };
                }).ToList();
            }
        }

        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateScout(PartidaScoutViewModel vm)
    {
        if (vm.IdTimeCasa == vm.IdTimeFora)
            ModelState.AddModelError(nameof(vm.IdTimeFora),
               "O visitante deve ser diferente do mandante.");

        if (!ModelState.IsValid)
        {
            vm.Ligas = _db.Ligas.OrderBy(l => l.Nome)
                                 .Select(l => new SelectListItem(l.Nome, l.IdLiga.ToString()))
                                 .ToList();
            vm.Times = _db.Times.Where(t => t.IdLiga == vm.IdLiga)
                                .OrderBy(t => t.Nome)
                                .Select(t => new SelectListItem(t.Nome, t.IdTime.ToString()))
                                .ToList();
            return View(vm);
        }

        var casaTotal = vm.Players.Where(p => p.IdTime == vm.IdTimeCasa).Sum(p => p.Pontos);
        var foraTotal = vm.Players.Where(p => p.IdTime == vm.IdTimeFora).Sum(p => p.Pontos);

        Partida partida;
        if (vm.IdPartida.GetValueOrDefault() > 0)
        {
            partida = await _db.Partidas
                       .Include(p => p.Estatisticas)
                       .FirstOrDefaultAsync(p => p.IdPartida == vm.IdPartida.Value);
            if (partida == null) return NotFound();
            
            partida.DataHora = vm.DataHora!.Value;
            partida.Local = vm.Local;
            partida.PlacarCasa = casaTotal;
            partida.PlacarFora = foraTotal;
            
            _db.EstatisticasPartidas.RemoveRange(partida.Estatisticas);
            _db.Partidas.Update(partida);
        }
        else
        {
            partida = new Partida
            {
                IdLiga = vm.IdLiga!.Value,
                IdTimeCasa = vm.IdTimeCasa!.Value,
                IdTimeFora = vm.IdTimeFora!.Value,
                DataHora = vm.DataHora!.Value,
                Local = vm.Local,
                PlacarCasa = casaTotal,
                PlacarFora = foraTotal
            };
            _db.Partidas.Add(partida);
            await _db.SaveChangesAsync();
        }

        var estatList = vm.Players
            .Where(p => p.Pontos > 0
                     || p.Rebotes > 0
                     || p.Assistencias > 0
                     || p.RoubosBola > 0
                     || p.Tocos > 0
                     || p.Faltas > 0)
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
            })
            .ToList();

        _db.EstatisticasPartidas.AddRange(estatList);
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    private void CarregarDropdowns(int? ligaSel = null)
    {
        ViewData["Ligas"] = new SelectList(
            _db.Ligas.OrderBy(l => l.Nome),
            "IdLiga", "Nome", ligaSel);


        var timesQuery = _db.Times.AsQueryable();
        if (ligaSel.HasValue)
            timesQuery = timesQuery.Where(t => t.IdLiga == ligaSel.Value);

        ViewData["Times"] = new SelectList(
            timesQuery.OrderBy(t => t.Nome),
            "IdTime", "Nome");
    }

    private void ValidarTimes(Partida p)
    {
        if (p.IdTimeCasa == p.IdTimeFora)
            ModelState.AddModelError("IdTimeFora", "O time visitante deve ser diferente do mandante.");
    }
}
