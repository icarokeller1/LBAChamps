using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LBAChamps.Models;

public class Partida
{
    [Key]
    public int IdPartida { get; set; }

    [Required]
    [DataType(DataType.DateTime)]
    public DateTime DataHora { get; set; }

    [StringLength(160)]
    public string Local { get; set; } = default!;

    /* ──────────────── FKs ──────────────── */
    public int IdTimeCasa { get; set; }
    public int IdTimeFora { get; set; }
    public int IdLiga { get; set; }

    public Time? TimeCasa { get; set; }
    public Time? TimeFora { get; set; }
    public Liga? Liga { get; set; }

    /* Placar */
    public int? PlacarCasa { get; set; }
    public int? PlacarFora { get; set; }

    /* Estatísticas */
    public ICollection<EstatisticasPartida> Estatisticas { get; set; } = [];
}
