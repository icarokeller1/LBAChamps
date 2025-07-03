using System.ComponentModel.DataAnnotations;

namespace LBAChamps.Models;

public class EstatisticasPartida
{
    [Key]
    public int IdEstatistica { get; set; }


    [Required, Range(1, int.MaxValue)]
    public int IdPartida { get; set; }
    [Required, Range(1, int.MaxValue)]
    public int IdJogador { get; set; }


    public Partida? Partida { get; set; }
    public Jogador? Jogador { get; set; }


    public int Pontos { get; set; }
    public int Rebotes { get; set; }
    public int Assistencias { get; set; }
    public int RoubosBola { get; set; }
    public int Tocos { get; set; }
    public int Faltas { get; set; }
}
