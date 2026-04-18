using API_devbank.Enums;
using System.ComponentModel.DataAnnotations;

namespace API_devbank.DTOs.Usuario
{
    public class CriarUsuarioRequest
    {
        [Required, MinLength(3), MaxLength(100)]
        public string Nome { get; set;  }

        [Required, StringLength(11, MinimumLength = 11, ErrorMessage = "CPF deve ter 11 dígitos")]
        public string CPF { get; set; }

        [Required, EmailAddress, MaxLength(100)]
        public string Email { get; set; }

        [Required, MaxLength(20)]
        public string Telefone { get; set; }

        [Required, MinLength(8, ErrorMessage = "Senha deve ter no mínimo 8 caracteres")]
        public string Senha { get; set; }

    }
}
