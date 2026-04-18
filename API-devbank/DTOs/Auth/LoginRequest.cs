using System.ComponentModel.DataAnnotations;

namespace API_devbank.DTOs.Auth
{
    public class LoginRequest
    {
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Senha { get; set; }
    }
}
