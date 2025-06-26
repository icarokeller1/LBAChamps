using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LBAChamps.Data;
using LBAChamps.Models;

namespace LBAChamps.Controllers
{
    public class NoticiasController : Controller
    {
        private readonly LigaContext _db;

        public NoticiasController(LigaContext context)
        {
            _db = context;
        }

        // GET: Noticias
        public async Task<IActionResult> Index(int? ligaId)
        {
            ViewBag.Ligas = new SelectList(_db.Ligas.OrderBy(l => l.Nome),
                                           "IdLiga", "Nome", ligaId);

            var q = _db.Noticias.Include(n => n.Liga)
                                .OrderByDescending(n => n.DataPublicacao)
                                .AsQueryable();

            if (ligaId is not null)
                q = q.Where(n => n.IdLiga == ligaId);

            return View(await q.AsNoTracking().ToListAsync());
        }

        // GET: Noticias/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var noticia = await _db.Noticias
                .FirstOrDefaultAsync(m => m.IdNoticia == id);
            if (noticia == null)
            {
                return NotFound();
            }

            return View(noticia);
        }

        // GET: Noticias/Create
        public IActionResult Create()
        {
            CarregarLigas();
            return View();
        }

        // POST: Noticias/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Noticia noticia, IFormFile? imagemFile)
        {
            // imagem opcional
            if (imagemFile is { Length: > 0 })
            {
                using var ms = new MemoryStream();
                await imagemFile.CopyToAsync(ms);
                noticia.Imagem = ms.ToArray();
                noticia.ImagemMimeType = imagemFile.ContentType;
            }

            noticia.DataPublicacao = DateTime.UtcNow;   // sempre agora

            if (!ModelState.IsValid)
            {
                CarregarLigas(noticia.IdLiga);
                return View(noticia);
            }

            _db.Add(noticia);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Noticias/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id is null) return NotFound();

            var noticia = await _db.Noticias.FindAsync(id);
            if (noticia is null) return NotFound();

            CarregarLigas(noticia.IdLiga);
            return View(noticia);
        }

        // POST: Noticias/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Noticia noticia, IFormFile? imagemFile)
        {
            if (id != noticia.IdNoticia) return NotFound();

            if (imagemFile is { Length: > 0 })
            {
                using var ms = new MemoryStream();
                await imagemFile.CopyToAsync(ms);
                noticia.Imagem = ms.ToArray();
                noticia.ImagemMimeType = imagemFile.ContentType;
            }

            if (!ModelState.IsValid)
            {
                CarregarLigas(noticia.IdLiga);
                return View(noticia);
            }

            try
            {
                _db.Update(noticia);
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_db.Noticias.Any(e => e.IdNoticia == id))
                    return NotFound();
                throw;
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Noticias/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var noticia = await _db.Noticias
                .FirstOrDefaultAsync(m => m.IdNoticia == id);
            if (noticia == null)
            {
                return NotFound();
            }

            return View(noticia);
        }

        // POST: Noticias/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var noticia = await _db.Noticias.FindAsync(id);
            if (noticia != null)
            {
                _db.Noticias.Remove(noticia);
            }

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool NoticiaExists(int id)
        {
            return _db.Noticias.Any(e => e.IdNoticia == id);
        }


        // helper -----------------------------------------------------------------
        private void CarregarLigas(int? selecionada = null)
        {
            ViewBag.Ligas = new SelectList(
                _db.Ligas.OrderBy(l => l.Nome).ToList(),
                "IdLiga", "Nome", selecionada);
        }
    }
}
