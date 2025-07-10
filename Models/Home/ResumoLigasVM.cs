namespace LBAChamps.Models;

public class DashboardVM
{
    /* Status */
    public int NaoIniciadas { get; set; }
    public int EmAndamento { get; set; }
    public int Concluidas { get; set; }
    public int Canceladas { get; set; }

    /* Entidades */
    public int TotalTimes { get; set; }
    public int TotalJogadores { get; set; }
    public int TotalPartidas { get; set; }

    /* Estatísticas acumuladas */
    public int TotalPontos { get; set; }
    public int TotalRebotes { get; set; }
    public int TotalAssistencias { get; set; }
    public int TotalRoubosBola { get; set; }
    public int TotalTocos { get; set; }
    public int TotalFaltas { get; set; }
}
