using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LBAChamps.ViewModels
{
    public class PlayerCreateViewModel
    {
        [StringLength(120)]
        public string Nome { get; set; } = "";

        [DataType(DataType.Date)]
        public DateOnly? DataNascimento { get; set; }

        [StringLength(30)]
        public string Posicao { get; set; } = "";

        [Range(0, 99)]
        public int? NumeroCamisa { get; set; }
    }

    public class TimeCreateViewModel
    {
        // Time
        public int? IdTime { get; set; }           // para edição, mas no Create fica nulo
        [Required, StringLength(120)]
        public string Nome { get; set; } = "";
        [Required, StringLength(80)]
        public string Cidade { get; set; } = "";
        [Required, StringLength(2)]
        public string Estado { get; set; } = "";
        [Required]
        public int IdLiga { get; set; }
        public IFormFile? LogoFile { get; set; }

        public List<SelectListItem> Ligas { get; set; } = new();
        public List<PlayerCreateViewModel> Players { get; set; } = new();
    }
}
