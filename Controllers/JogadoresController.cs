using LBAChamps.Data;
using LBAChamps.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace LBAChamps.Controllers;

public class JogadoresController : Controller
{
    private readonly LigaContext _db;
    public JogadoresController(LigaContext context) => _db = context;

    public async Task<IActionResult> Index(int? ligaId, int? timeId)
    {
        var q = _db.Jogadores
                   .Include(j => j.Time).ThenInclude(t => t.Liga)
                   .AsQueryable();

        if (ligaId != null) q = q.Where(j => j.Time.IdLiga == ligaId);
        if (timeId != null) q = q.Where(j => j.IdTime == timeId);

        ViewData["Ligas"] = new SelectList(_db.Ligas.OrderBy(l => l.Nome),
                                           "IdLiga", "Nome", ligaId);

        var timesQ = _db.Times.AsQueryable();
        if (ligaId != null) timesQ = timesQ.Where(t => t.IdLiga == ligaId);

        ViewData["Times"] = new SelectList(timesQ.OrderBy(t => t.Nome),
                                           "IdTime", "Nome", timeId);

        return View(await q.AsNoTracking().ToListAsync());
    }

    public IActionResult Create()
    {
        CarregarDropDown();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int ligaId,
        [Bind("Nome,DataNascimento,Posicao,NumeroCamisa,IdTime")]
    Jogador jogador)
    {
        bool repetido = await _db.Jogadores
            .AnyAsync(j => j.IdTime == jogador.IdTime &&
                           j.NumeroCamisa == jogador.NumeroCamisa);

        if (repetido)
            ModelState.AddModelError("NumeroCamisa",
                "Já existe jogador com esse número neste time.");

        if (!ModelState.IsValid)
        {
            CarregarDropDown(ligaId, jogador.IdTime);
            return View(jogador);
        }

        _db.Add(jogador);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var jogador = await _db.Jogadores
            .Include(j => j.Time)
            .FirstOrDefaultAsync(j => j.IdJogador == id);

        if (jogador == null) 
            return NotFound();

        var ligaId = jogador.Time!.IdLiga;

        ViewBag.Ligas = new SelectList(
            _db.Ligas.OrderBy(l => l.Nome),
            "IdLiga", "Nome", ligaId);


        ViewBag.Times = new SelectList(
            _db.Times.Where(t => t.IdLiga == ligaId)
                          .OrderBy(t => t.Nome),
            "IdTime", "Nome", jogador.IdTime);

        return View(jogador);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Jogador jogador)
    {
        if (id != jogador.IdJogador) return NotFound();

        if (_db.Jogadores.Any(j =>
            j.IdJogador != jogador.IdJogador &&
            j.IdTime == jogador.IdTime &&
            j.NumeroCamisa == jogador.NumeroCamisa))
        {
            ModelState.AddModelError("NumeroCamisa",
                "Já existe jogador com esse número nesse time.");
        }

        if (!ModelState.IsValid)
        {
            var ligaId = _db.Times
                         .Where(t => t.IdTime == jogador.IdTime)
                         .Select(t => t.IdLiga)
                         .FirstOrDefault();

            ViewBag.Ligas = new SelectList(
                _db.Ligas.OrderBy(l => l.Nome),
                "IdLiga", "Nome", ligaId);

            ViewBag.Times = new SelectList(
                _db.Times.Where(t => t.IdLiga == ligaId)
                              .OrderBy(t => t.Nome),
                "IdTime", "Nome", jogador.IdTime);

            return View(jogador);
        }

        try
        {
            _db.Update(jogador);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_db.Jogadores.Any(e => e.IdJogador == id))
                return NotFound();
            else throw;
        }
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null) return NotFound();

        var jogador = await _db.Jogadores
                                    .Include(j => j.Time)
                                    .FirstOrDefaultAsync(j => j.IdJogador == id);
        return jogador is null ? NotFound() : View(jogador);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var jogador = await _db.Jogadores.FindAsync(id);
        if (jogador is not null) _db.Jogadores.Remove(jogador);

        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private void CarregarDropDown(int? ligaId = null, int? timeId = null)
    {
        ViewData["Ligas"] = new SelectList(
            _db.Ligas.OrderBy(l => l.Nome), "IdLiga", "Nome", ligaId);

        var timesQ = _db.Times.AsQueryable();
        if (ligaId != null) timesQ = timesQ.Where(t => t.IdLiga == ligaId);

        ViewData["Times"] = new SelectList(
            timesQ.OrderBy(t => t.Nome), "IdTime", "Nome", timeId);
    }

    private void CarregarTimes(int? selecionado = null) =>
        ViewData["IdTime"] = new SelectList(
            _db.Times.OrderBy(t => t.Nome),
            "IdTime",
            "Nome",
            selecionado);

    private bool JogadorExists(int id) =>
        _db.Jogadores.Any(e => e.IdJogador == id);
}
