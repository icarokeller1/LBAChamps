using LBAChamps.Models;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

[Route("Error")]
public class ErrorController : Controller
{
    [Route("")]
    public IActionResult Index(int? codigo = null)
    {
        var exFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();

        var vm = new ErrorViewModel
        {
            StatusCode = codigo ?? HttpContext.Response.StatusCode,
            Path = exFeature?.Path,
            Message = exFeature?.Error?.Message
        };

        return View("Error", vm);
    }
}
