using LBAChamps.Data;
using LBAChamps.Models;
using LBAChamps.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NuGet.ProjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LBAChamps.Controllers
{
    public class TimesController : Controller
    {
        private readonly LigaContext _db;

        public TimesController(LigaContext context)
        {
            _db = context;
        }

        public async Task<IActionResult> Index(int? ligaId)
        {
            ViewBag.Ligas = new SelectList(
                _db.Ligas.OrderBy(l => l.Nome), "IdLiga", "Nome", ligaId);

            var q = _db.Times
                       .Include(t => t.Liga)
                       .AsQueryable();

            if (ligaId is not null)
                q = q.Where(t => t.IdLiga == ligaId);

            return View(await q.AsNoTracking().ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var time = await _db.Times
                .Include(t => t.Liga)
                .FirstOrDefaultAsync(m => m.IdTime == id);
            if (time == null)
            {
                return NotFound();
            }

            return View(time);
        }

        public IActionResult Create()
        {
            var vm = new TimeCreateViewModel
            {
                Ligas = _db.Ligas
                           .OrderBy(l => l.Nome)
                           .Select(l => new SelectListItem(l.Nome, l.IdLiga.ToString()))
                           .ToList()
            };
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TimeCreateViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                vm.Ligas = _db.Ligas
                    .OrderBy(l => l.Nome)
                    .Select(l => new SelectListItem(l.Nome, l.IdLiga.ToString()))
                    .ToList();
                return View(vm);
            }

            var time = new Time
            {
                Nome = vm.Nome,
                Cidade = vm.Cidade,
                Estado = vm.Estado,
                IdLiga = vm.IdLiga
            };

            if (vm.LogoFile is not null && vm.LogoFile.Length > 0)
            {
                using var ms = new MemoryStream();
                await vm.LogoFile.CopyToAsync(ms);
                time.Logo = ms.ToArray();
                time.LogoMimeType = vm.LogoFile.ContentType;
            }

            _db.Times.Add(time);
            await _db.SaveChangesAsync();

            foreach (var p in vm.Players
                                .Where(p => !string.IsNullOrWhiteSpace(p.Nome)))
            {
                var jog = new Jogador
                {
                    Nome = p.Nome!,
                    DataNascimento = p.DataNascimento,
                    Posicao = p.Posicao!,
                    NumeroCamisa = p.NumeroCamisa!.Value,
                    IdTime = time.IdTime
                };
            _db.Jogadores.Add(jog);
            }
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var time = await _db.Times
                .Include(t => t.Jogadores)
                .FirstOrDefaultAsync(t => t.IdTime == id);
            if (time == null) return NotFound();

            var vm = new TimeEditViewModel
            {
                IdTime = time.IdTime,
                Nome = time.Nome,
                Cidade = time.Cidade,
                Estado = time.Estado,
                IdLiga = time.IdLiga,
                LogoExistingPath = time.Logo != null
                    ? $"data:{time.LogoMimeType};base64,{Convert.ToBase64String(time.Logo)}"
                    : null,
                Ligas = await _db.Ligas
                    .OrderBy(l => l.Nome)
                    .Select(l => new SelectListItem(l.Nome, l.IdLiga.ToString()))
                    .ToListAsync(),
                Players = time.Jogadores
                    .Select(j => new PlayerEditViewModel
                    {
                        IdJogador = j.IdJogador,
                        Nome = j.Nome,
                        DataNascimento = j.DataNascimento,
                        Posicao = j.Posicao,
                        NumeroCamisa = j.NumeroCamisa
                    })
                    .ToList()
            };

            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(TimeEditViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                vm.Ligas = await _db.Ligas
                    .OrderBy(l => l.Nome)
                    .Select(l => new SelectListItem(l.Nome, l.IdLiga.ToString()))
                    .ToListAsync();
                return View(vm);
            }

            var time = await _db.Times.FindAsync(vm.IdTime);
            if (time == null) return NotFound();

            time.Nome = vm.Nome;
            time.Cidade = vm.Cidade;
            time.Estado = vm.Estado;
            time.IdLiga = vm.IdLiga;

            if (vm.LogoFile is not null && vm.LogoFile.Length > 0)
            {
                using var ms = new MemoryStream();
                await vm.LogoFile.CopyToAsync(ms);
                time.Logo = ms.ToArray();
                time.LogoMimeType = vm.LogoFile.ContentType;
            }

            var existentes = await _db.Jogadores
                .Where(j => j.IdTime == vm.IdTime)
                .ToListAsync();

            var enviadosIds = vm.Players
                .Where(p => p.IdJogador > 0)
                .Select(p => p.IdJogador)
                .ToHashSet();

            var paraRemover = existentes
                .Where(j => !enviadosIds.Contains(j.IdJogador));
            _db.Jogadores.RemoveRange(paraRemover);

            foreach (var p in vm.Players)
            {
                if (p.IdJogador > 0)
                {
                    var j = existentes.First(j0 => j0.IdJogador == p.IdJogador);
                    j.Nome = p.Nome;
                    j.DataNascimento = p.DataNascimento;
                    j.Posicao = p.Posicao;
                    j.NumeroCamisa = p.NumeroCamisa;
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(p.Nome))
                    {
                        _db.Jogadores.Add(new Jogador
                        {
                            Nome = p.Nome,
                            DataNascimento = p.DataNascimento,
                            Posicao = p.Posicao,
                            NumeroCamisa = p.NumeroCamisa,
                            IdTime = vm.IdTime!.Value
                        });
                    }
                }
            }

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var time = await _db.Times
                .Include(t => t.Liga)
                .FirstOrDefaultAsync(m => m.IdTime == id);
            if (time == null)
            {
                return NotFound();
            }

            return View(time);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var time = await _db.Times.FindAsync(id);
            if (time != null)
            {
                _db.Times.Remove(time);
            }

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TimeExists(int id)
        {
            return _db.Times.Any(e => e.IdTime == id);
        }

        private void CarregarDropDown(int? ligaId = null)
        {
            ViewBag.Ligas = new SelectList(
                _db.Ligas.OrderBy(l => l.Nome), "IdLiga", "Nome", ligaId);
        }
    }
}
