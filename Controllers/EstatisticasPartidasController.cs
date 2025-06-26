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

    // ------------ LISTA ----------------------------------------------------
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

        // --- combos -----------------
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

    // ------------ DETAILS --------------------------------------------------
    public async Task<IActionResult> Details(int id)
    {
        var est = await _db.EstatisticasPartidas
            .Include(e => e.Partida)
                .ThenInclude(p => p.TimeCasa)      // inclui mandante
            .Include(e => e.Partida)
                .ThenInclude(p => p.TimeFora)      // inclui visitante
            .Include(e => e.Jogador)
                .ThenInclude(j => j.Time)          // opcional, mostra time do jogador
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.IdEstatistica == id);

        return est is null ? NotFound() : View(est);
    }

    // ---- CREATE (GET) --------------------------------------------------------
    public IActionResult Create()
    {
        CarregarDropDown();               // sem pré-selec.
        return View();
    }

    // ---- CREATE (POST) -------------------------------------------------------
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        int LigaId,                      // ← vem do combo, fora do modelo
        [Bind("IdPartida,IdJogador,Pontos,Rebotes,Assistencias,RoubosBola,Tocos,Faltas")]
    EstatisticasPartida est)
    {
        // Apenas IdPartida/IdJogador precisam ser > 0
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

    // ---- EDIT (GET) ----------------------------------------------------------
    public async Task<IActionResult> Edit(int id)
    {
        var est = await _db.EstatisticasPartidas
                           .Include(p => p.Partida)
                           .FirstOrDefaultAsync(e => e.IdEstatistica == id);
        if (est is null) return NotFound();

        CarregarDropDown(est.Partida.IdLiga, est.IdPartida, est.IdJogador);
        return View(est);
    }

    // ---- EDIT (POST) ---------------------------------------------------------
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

    // ------------ DELETE ---------------------------------------------------
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

    // ---- HELPER --------------------------------------------------------------
    private void CarregarDropDown(int? ligaId = null,
                              int? partidaId = null,
                              int? jogadorId = null)
    {
        // Ligas sempre disponíveis
        ViewData["Ligas"] = new SelectList(
            _db.Ligas.OrderBy(l => l.Nome),
            "IdLiga", "Nome", ligaId);

        // Partidas SOMENTE se liga selecionada
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

        // Jogadores SOMENTE se partida selecionada
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
