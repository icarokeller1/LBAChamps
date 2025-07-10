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

        public IActionResult Index()
        {
            var vm = new DashboardVM
            {
                /* Status das ligas ---------------------------------------- */
                NaoIniciadas = _context.Ligas.Count(l => l.Status == "Não Iniciada"),
                EmAndamento = _context.Ligas.Count(l => l.Status == "Em andamento"),
                Concluidas = _context.Ligas.Count(l => l.Status == "Concluída"),
                Canceladas = _context.Ligas.Count(l => l.Status == "Cancelada"),

                /* Totais de entidades ------------------------------------- */
                TotalTimes = _context.Times.Count(),
                TotalJogadores = _context.Jogadores.Count(),
                TotalPartidas = _context.Partidas.Count(),

                /* Somatório de estatísticas ------------------------------- */
                TotalPontos = _context.EstatisticasPartidas.Sum(e => e.Pontos),
                TotalRebotes = _context.EstatisticasPartidas.Sum(e => e.Rebotes),
                TotalAssistencias = _context.EstatisticasPartidas.Sum(e => e.Assistencias),
                TotalRoubosBola = _context.EstatisticasPartidas.Sum(e => e.RoubosBola),
                TotalTocos = _context.EstatisticasPartidas.Sum(e => e.Tocos),
                TotalFaltas = _context.EstatisticasPartidas.Sum(e => e.Faltas)
            };

            return View(vm);          // envia como model fortemente tipado
        }


        public IActionResult Privacy() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() =>
            View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
