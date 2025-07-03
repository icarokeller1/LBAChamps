using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

public class PlayerStatsViewModel
{
    public int IdJogador { get; set; }
    public int IdTime { get; set; }
    public string Nome { get; set; } = "";
    public int Pontos { get; set; }
    public int Rebotes { get; set; }
    public int Assistencias { get; set; }
    public int RoubosBola { get; set; }
    public int Tocos { get; set; }
    public int Faltas { get; set; }
}

public class PartidaScoutViewModel
{
    public int? IdPartida { get; set; }
    public int? IdLiga { get; set; }
    public List<SelectListItem> Ligas { get; set; } = new();

    public int? IdTimeCasa { get; set; }
    public int? IdTimeFora { get; set; }
    public List<SelectListItem> Times { get; set; } = new();

    [Required, DataType(DataType.DateTime)]
    public DateTime? DataHora { get; set; }

    [Required, StringLength(160)]
    public string Local { get; set; } = "";

    public List<PlayerStatsViewModel> Players { get; set; } = new();
}
