using System.Diagnostics;
using LBAChamps.Data;
using LBAChamps.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBAChamps.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly LigaContext _context;

        public HomeController(ILogger<HomeController> logger,
                              LigaContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var hoje = DateOnly.FromDateTime(DateTime.Today);

            // agrupa por string (ATIVA, PLANEJADA, FINALIZADA)
            var resumo = await _context.Ligas
                .GroupBy(l =>
                    l.Status == "ATIVA" ? "Ativas" :
                    l.DataInicio > hoje ? "Futuras" :
                    l.Status == "FINALIZADA" ? "Finalizadas" :
                                                 "Outras")
                .Select(g => new { g.Key, Qtde = g.Count() })
                .ToDictionaryAsync(k => k.Key, v => v.Qtde);

            var vmResumo = new ResumoLigasVM(
                resumo.GetValueOrDefault("Ativas"),
                resumo.GetValueOrDefault("Futuras"),
                resumo.GetValueOrDefault("Finalizadas"));

            ViewBag.ResumoLigas = vmResumo;

            return View();
        }

        public IActionResult Privacy() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() =>
            View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
