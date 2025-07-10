using LBAChamps.Data;
using LBAChamps.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBAChamps.Controllers;

public class LigasController : Controller
{
    private readonly LigaContext _context;
    public LigasController(LigaContext context) => _context = context;


    public async Task<IActionResult> Index() =>
        View(await _context.Ligas.OrderBy(l => l.Nome).ToListAsync());


    public IActionResult Create() => View();


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("Nome,Descricao,DataInicio,DataFim,Status")] Liga liga)
    {
        ValidarLiga(liga);

        if (!ModelState.IsValid) return View(liga);

        _context.Add(liga);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }


    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null) return NotFound();

        var liga = await _context.Ligas.FindAsync(id);
        return liga is null ? NotFound() : View(liga);
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id,
        [Bind("IdLiga,Nome,Descricao,DataInicio,DataFim,Status")] Liga liga)
    {
        if (id != liga.IdLiga) return NotFound();

        ValidarLiga(liga, ignoreId: id);

        if (!ModelState.IsValid) return View(liga);

        try
        {
            _context.Update(liga);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException) when (!LigaExists(id))
        {
            return NotFound();
        }

        return RedirectToAction(nameof(Index));
    }


    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null) return NotFound();

        var liga = await _context.Ligas.FirstOrDefaultAsync(l => l.IdLiga == id);
        return liga is null ? NotFound() : View(liga);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var liga = await _context.Ligas.FindAsync(id);
        if (liga is null) return RedirectToAction(nameof(Index));

        try
        {
            _context.Ligas.Remove(liga);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            TempData["Error"] = "Não é possível excluir a liga: existem times associados.";
        }

        return RedirectToAction(nameof(Index));
    }


    private void ValidarLiga(Liga liga, int? ignoreId = null)
    {
        if (liga.DataFim is not null && liga.DataFim < liga.DataInicio)
            ModelState.AddModelError("DataFim", "Data de término deve ser posterior à de início.");

        bool existe = _context.Ligas.Any(l =>
            l.Nome == liga.Nome &&
            (ignoreId == null || l.IdLiga != ignoreId));

        if (existe)
            ModelState.AddModelError("Nome", "Já existe uma liga com esse Nome.");
    }

    private bool LigaExists(int id) =>
        _context.Ligas.Any(e => e.IdLiga == id);
}
