using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LBAChamps.Models;


public class Liga
{
    [Key]
    public int IdLiga { get; set; }

    [Required, StringLength(120)]
    public string Nome { get; set; } = default!;

    public string? Descricao { get; set; }

    [DataType(DataType.Date)]
    public DateOnly DataInicio { get; set; }

    [DataType(DataType.Date)]
    public DateOnly? DataFim { get; set; }

    [Required, StringLength(30)]
    public string Status { get; set; } = default!;

    public ICollection<Time> Times { get; set; } = [];
    public ICollection<Partida> Partidas { get; set; } = [];

    public ICollection<Noticia> Noticias { get; set; } = [];
}