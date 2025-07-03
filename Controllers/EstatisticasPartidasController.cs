using LBAChamps.Data;
using LBAChamps.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace LBAChamps.Controllers;

public class EstatisticasPartidasController : Controller
{
    private readonly LigaContext _db;
    public EstatisticasPartidasController(LigaContext db) => _db = db;


    public async Task<IActionResult> Index(int? ligaId, int? timeId, int? jogadorId)
    {
        var q = _db.EstatisticasPartidas
            .Include(e => e.Partida)
                .ThenInclude(p => p.TimeCasa)
            .Include(e => e.Partida)
                .ThenInclude(p => p.TimeFora)
            .Include(e => e.Partida)
                .ThenInclude(p => p.Liga)
            .Include(e => e.Jogador)
                .ThenInclude(j => j.Time)
            .AsQueryable();

        if (ligaId is not null)
            q = q.Where(e => e.Partida.IdLiga == ligaId);

        if (timeId is not null)
            q = q.Where(e => e.Jogador.IdTime == timeId);

        if (jogadorId is not null)
            q = q.Where(e => e.IdJogador == jogadorId);

        ViewData["Ligas"] = new SelectList(
            _db.Ligas.OrderBy(l => l.Nome),
            "IdLiga", "Nome", ligaId);

        var timesQuery = _db.Times.AsQueryable();
        if (ligaId is not null) timesQuery = timesQuery.Where(t => t.IdLiga == ligaId);

        ViewData["Times"] = new SelectList(
            timesQuery.OrderBy(t => t.Nome),
            "IdTime", "Nome", timeId);

        var jogadoresQuery = _db.Jogadores.AsQueryable();
        if (timeId is not null) jogadoresQuery = jogadoresQuery.Where(j => j.IdTime == timeId);

        ViewData["Jogadores"] = new SelectList(
            jogadoresQuery.OrderBy(j => j.Nome),
            "IdJogador", "Nome", jogadorId);

        var lista = await q.AsNoTracking()
                           .OrderBy(e => e.Partida.DataHora)
                           .ToListAsync();

        return View(lista);
    }


    public async Task<IActionResult> Details(int id)
    {
        var est = await _db.EstatisticasPartidas
            .Include(e => e.Partida)
                .ThenInclude(p => p.TimeCasa)
            .Include(e => e.Partida)
                .ThenInclude(p => p.TimeFora)
            .Include(e => e.Jogador)
                .ThenInclude(j => j.Time)
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.IdEstatistica == id);

        return est is null ? NotFound() : View(est);
    }


    public IActionResult Create()
    {
        CarregarDropDown();
        return View();
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        int LigaId,
        [Bind("IdPartida,IdJogador,Pontos,Rebotes,Assistencias,RoubosBola,Tocos,Faltas")]
    EstatisticasPartida est)
    {

        if (!ModelState.IsValid)
        {
            CarregarDropDown(LigaId, est.IdPartida, est.IdJogador);
            return View(est);
        }

        var jaExiste = await _db.EstatisticasPartidas
            .AnyAsync(e => e.IdPartida == est.IdPartida &&
                   e.IdJogador == est.IdJogador);

        if (jaExiste)
        {
            ModelState.AddModelError(string.Empty,
                "Estatística para esse jogador nesta partida já existe.");
            CarregarDropDown(LigaId, est.IdPartida, est.IdJogador);
            return View(est);
        }

        _db.Add(est);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var est = await _db.EstatisticasPartidas
                           .Include(p => p.Partida)
                           .FirstOrDefaultAsync(e => e.IdEstatistica == id);
        if (est is null) return NotFound();

        CarregarDropDown(est.Partida.IdLiga, est.IdPartida, est.IdJogador);
        return View(est);
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, int LigaId,
        [Bind("IdEstatistica,IdPartida,IdJogador,Pontos,Rebotes,Assistencias,RoubosBola,Tocos,Faltas")]
    EstatisticasPartida est)
    {
        if (id != est.IdEstatistica) return NotFound();

        if (!ModelState.IsValid)
        {
            CarregarDropDown(LigaId, est.IdPartida, est.IdJogador);
            return View(est);
        }

        _db.Update(est);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }


    public async Task<IActionResult> Delete(int id)
    {
        var est = await _db.EstatisticasPartidas
            .Include(e => e.Partida)
            .Include(e => e.Jogador)
            .FirstOrDefaultAsync(e => e.IdEstatistica == id);

        return est is null ? NotFound() : View(est);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var est = await _db.EstatisticasPartidas.FindAsync(id);
        if (est is not null) _db.Remove(est);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }


    private void CarregarDropDown(int? ligaId = null,
                              int? partidaId = null,
                              int? jogadorId = null)
    {
        ViewData["Ligas"] = new SelectList(
            _db.Ligas.OrderBy(l => l.Nome),
            "IdLiga", "Nome", ligaId);


        var partidasQuery = Enumerable.Empty<object>();
        if (ligaId is not null)
        {
            partidasQuery = _db.Partidas
                .Where(p => p.IdLiga == ligaId)
                .OrderBy(p => p.DataHora)
                .Select(p => new
                {
                    p.IdPartida,
                    Descricao = p.DataHora.ToString("dd/MM") + " - " +
                                p.TimeCasa.Nome + " x " + p.TimeFora.Nome
                });
        }
        ViewData["Partidas"] = new SelectList(partidasQuery, "IdPartida", "Descricao", partidaId);

        var jogadoresQuery = Enumerable.Empty<object>();
        if (partidaId is not null)
        {
            jogadoresQuery = _db.Jogadores
                .Where(j => j.Time.PartidasCasa.Any(p => p.IdPartida == partidaId) ||
                            j.Time.PartidasFora.Any(p => p.IdPartida == partidaId))
                .OrderBy(j => j.Nome)
                .Select(j => new
                {
                    j.IdJogador,
                    Nome = j.Nome + " (" + j.Time.Nome + ")"
                });
        }
        ViewData["Jogadores"] = new SelectList(jogadoresQuery, "IdJogador", "Nome", jogadorId);
    }
}
