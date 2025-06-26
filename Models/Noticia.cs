using LBAChamps.Models;
using System.ComponentModel.DataAnnotations;

namespace LBAChamps.Models;

public class Noticia
{
    [Key] public int IdNoticia { get; set; }

    [StringLength(160)]
    public string Titulo { get; set; } = default!;

    [StringLength(200)]
    public string? Subtitulo { get; set; }

    [StringLength(6000)]                 // HTML ou Markdown livre
    public string Conteudo { get; set; } = default!;

    [StringLength(120)]
    public string Autor { get; set; } = default!;

    /* liga opcional: notícia geral ou específica */
    public int? IdLiga { get; set; }
    public Liga? Liga { get; set; }

    /* imagem principal (opcional) */
    public byte[]? Imagem { get; set; }
    public string? ImagemMimeType { get; set; }

    [StringLength(8000)]
    public string? LinkInstagram { get; set; }

    [DataType(DataType.DateTime)]
    public DateTime DataPublicacao { get; set; } = DateTime.UtcNow; // sempre “agora”
}