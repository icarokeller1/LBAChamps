using System.ComponentModel.DataAnnotations;

namespace LBAChamps.Models;

public class Usuario
{
    [Key]
    public int IdUsuario { get; set; }

    [Required, StringLength(120)]
    public string Nome { get; set; } = default!;

    [Required, EmailAddress, StringLength(160)]
    public string Email { get; set; } = default!;

    [Required, StringLength(255)]
    [DataType(DataType.Password)]
    public string Senha { get; set; } = default!;   // guarde aqui o hash

    [Required, StringLength(30)]
    public string Tipo { get; set; } = default!;    // ADMIN, ORGANIZADOR, ATLETA…
}
