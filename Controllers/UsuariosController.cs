using LBAChamps.Data;
using LBAChamps.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace LBAChamps.Controllers;

public class UsuariosController : Controller
{
    private readonly LigaContext _context;
    public UsuariosController(LigaContext context) => _context = context;

    /*────────────────────────────────────────────────────────────*/
    /* LISTAGEM                                                   */
    /*────────────────────────────────────────────────────────────*/
    public async Task<IActionResult> Index() =>
        View(await _context.Usuarios.OrderBy(u => u.Nome).ToListAsync());

    /*────────────────────────────────────────────────────────────*/
    /* DETAILS                                                    */
    /*────────────────────────────────────────────────────────────*/
    public async Task<IActionResult> Details(int? id)
    {
        if (id is null) return NotFound();

        var user = await _context.Usuarios.FirstOrDefaultAsync(u => u.IdUsuario == id);
        return user is null ? NotFound() : View(user);
    }

    /*────────────────────────────────────────────────────────────*/
    /* CREATE – GET                                               */
    /*────────────────────────────────────────────────────────────*/
    public IActionResult Create()
    {
        CarregarTipos();
        return View();
    }

    /*────────────────────────────────────────────────────────────*/
    /* CREATE – POST                                              */
    /*────────────────────────────────────────────────────────────*/
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("Nome,Email,Senha,Tipo")] Usuario usuario)
    {
        ValidarEmail(usuario.Email);

        if (!ModelState.IsValid)
        {
            CarregarTipos(usuario.Tipo);
            return View(usuario);
        }

        // TODO: aplicar hash na Senha antes de salvar
        _context.Add(usuario);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    /*────────────────────────────────────────────────────────────*/
    /* EDIT – GET                                                 */
    /*────────────────────────────────────────────────────────────*/
    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null) return NotFound();

        var usuario = await _context.Usuarios.FindAsync(id);
        if (usuario is null) return NotFound();

        CarregarTipos(usuario.Tipo);
        return View(usuario);
    }

    /*────────────────────────────────────────────────────────────*/
    /* EDIT – POST                                                */
    /*────────────────────────────────────────────────────────────*/
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id,
        [Bind("IdUsuario,Nome,Email,Senha,Tipo")] Usuario usuario)
    {
        if (id != usuario.IdUsuario) return NotFound();

        ValidarEmail(usuario.Email, ignoreId: id);

        if (!ModelState.IsValid)
        {
            CarregarTipos(usuario.Tipo);
            return View(usuario);
        }

        try
        {
            // TODO: aplicar hash se Senha foi alterada
            _context.Update(usuario);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException) when (!UsuarioExists(id))
        {
            return NotFound();
        }

        return RedirectToAction(nameof(Index));
    }

    /*────────────────────────────────────────────────────────────*/
    /* DELETE                                                     */
    /*────────────────────────────────────────────────────────────*/
    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null) return NotFound();

        var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.IdUsuario == id);
        return usuario is null ? NotFound() : View(usuario);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var usuario = await _context.Usuarios.FindAsync(id);
        if (usuario is not null) _context.Usuarios.Remove(usuario);

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    /*────────────────────────────────────────────────────────────*/
    /* AUXILIARES                                                 */
    /*────────────────────────────────────────────────────────────*/
    private void CarregarTipos(string? selecionado = null)
    {
        var tipos = new[] { "ADMIN", "ORGANIZADOR", "ATLETA" };
        ViewData["Tipo"] = new SelectList(tipos, selecionado);
    }

    private void ValidarEmail(string email, int? ignoreId = null)
    {
        bool existe = _context.Usuarios.Any(u =>
            u.Email == email && (ignoreId == null || u.IdUsuario != ignoreId));

        if (existe)
            ModelState.AddModelError("Email", "Este e-mail já está cadastrado.");
    }

    private bool UsuarioExists(int id) =>
        _context.Usuarios.Any(e => e.IdUsuario == id);
}
