using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LBAChamps.Models;

public class Time
{
    [Key]
    public int IdTime { get; set; }

    [Required, StringLength(120)]
    public string Nome { get; set; } = default!;

    [Required, StringLength(80)]
    public string Cidade { get; set; } = default!;

    [Required, StringLength(2)]
    public string Estado { get; set; } = default!;

    public byte[]? Logo { get; set; }
    public string? LogoMimeType { get; set; }


    public int IdLiga { get; set; }
    public Liga? Liga { get; set; }


    public ICollection<Jogador> Jogadores { get; set; } = [];
    public ICollection<Partida> PartidasCasa { get; set; } = [];
    public ICollection<Partida> PartidasFora { get; set; } = [];
}
