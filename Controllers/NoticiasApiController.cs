using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LBAChamps.Data;

[ApiController]
[Route("api/noticias")]
public class NoticiasApiController : ControllerBase
{
    private readonly LigaContext _db;
    public NoticiasApiController(LigaContext db) => _db = db;


    [HttpGet("{id:int}/imagem")]
    public async Task<IActionResult> Imagem(int id)
    {
        var n = await _db.Noticias
                         .AsNoTracking()
                         .Select(x => new { x.IdNoticia, x.Imagem, x.ImagemMimeType })
                         .FirstOrDefaultAsync(x => x.IdNoticia == id);

        return n?.Imagem is null
            ? NotFound()
            : File(n.Imagem, n.ImagemMimeType ?? "image/png");
    }


    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] int? ligaId, [FromQuery] int limite = 30)
    {
        var q = _db.Noticias
                   .Include(n => n.Liga)
                   .OrderByDescending(n => n.DataPublicacao)
                   .AsQueryable();

        if (ligaId is not null)
            q = q.Where(n => n.IdLiga == ligaId || n.IdLiga == null);


        q = q.Where(n => string.IsNullOrEmpty(n.LinkInstagram));

        var lista = await q.Take(limite)
            .Select(n => new {
                n.IdNoticia,
                n.Titulo,
                n.Subtitulo,
                n.Autor,
                n.DataPublicacao,
                Liga = n.Liga == null ? null : n.Liga.Nome,
                temImagem = n.Imagem != null
            })
            .AsNoTracking()
            .ToListAsync();

        return Ok(lista);
    }


    [HttpGet("{id:int}")]
    public async Task<IActionResult> Detalhe(int id)
    {
        var n = await _db.Noticias
            .Include(l => l.Liga)
            .AsNoTracking()
            .Where(x => x.IdNoticia == id)
            .Select(x => new {
                x.IdNoticia,
                x.Titulo,
                x.Subtitulo,
                x.Conteudo,
                x.Autor,
                x.DataPublicacao,
                Liga = x.Liga == null ? null : x.Liga.Nome,
                x.LinkInstagram,
                temImagem = x.Imagem != null
            })
            .FirstOrDefaultAsync();

        return n is null ? NotFound() : Ok(n);
    }

    [HttpGet("instagram")]
    public async Task<IActionResult> ListarComInstagram(
    [FromQuery] int? ligaId,
    [FromQuery] int limite = 30)
    {
        var q = _db.Noticias
                   .Include(n => n.Liga)
                   .Where(n => n.LinkInstagram != null && n.LinkInstagram != "")
                   .OrderByDescending(n => n.DataPublicacao)
                   .AsQueryable();

        if (ligaId is not null)
            q = q.Where(n => n.IdLiga == ligaId || n.IdLiga == null);

        q = q.Where(n => n.LinkInstagram != null &&
                 n.LinkInstagram.ToLower().Contains("instagram"));

        var lista = await q.Take(limite)
            .Select(n => new {
                n.IdNoticia,
                n.LinkInstagram,
                n.Titulo,
                Liga = n.Liga == null ? null : n.Liga.Nome
            })
            .AsNoTracking()
            .ToListAsync();

        return Ok(lista);
    }
}
