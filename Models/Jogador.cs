using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LBAChamps.Models;

public class Jogador
{
    [Key]
    public int IdJogador { get; set; }

    [Required, StringLength(120)]
    public string Nome { get; set; } = default!;

    [DataType(DataType.Date)]
    public DateOnly? DataNascimento { get; set; }

    [Required, StringLength(30)]
    public string Posicao { get; set; } = default!;

    [Range(0, 99, ErrorMessage = "Número de camisa deve ser 0-99")]
    public int NumeroCamisa { get; set; }

    public int IdTime { get; set; }

    public Time? Time { get; set; }

    public ICollection<EstatisticasPartida> Estatisticas { get; set; } = [];
}
