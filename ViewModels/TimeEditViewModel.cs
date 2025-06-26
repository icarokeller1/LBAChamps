using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.ComponentModel.DataAnnotations;

namespace LBAChamps.ViewModels
{
    public class PlayerEditViewModel
    {
        public int IdJogador { get; set; }

        [Required, StringLength(120)]
        public string Nome { get; set; } = "";

        [DataType(DataType.Date)]
        public DateOnly DataNascimento { get; set; }

        [Required, StringLength(30)]
        public string Posicao { get; set; } = "";

        [Required, Range(0, 99)]
        public int NumeroCamisa { get; set; }
    }

    public class TimeEditViewModel
    {
        public int? IdTime { get; set; }

        [Required, StringLength(120)]
        public string Nome { get; set; } = "";

        [Required, StringLength(80)]
        public string Cidade { get; set; } = "";

        [Required, StringLength(2)]
        public string Estado { get; set; } = "";

        [Required]
        public int IdLiga { get; set; }

        // Para upload de novo logo
        public IFormFile? LogoFile { get; set; }

        // Para exibir o logo já existente, no GET Edit
        public string? LogoExistingPath { get; set; }

        // Dropdown de ligas
        public List<SelectListItem> Ligas { get; set; } = new();

        // Lista de jogadores para editar/remover/adicionar
        public List<PlayerEditViewModel> Players { get; set; } = new();
    }
}
