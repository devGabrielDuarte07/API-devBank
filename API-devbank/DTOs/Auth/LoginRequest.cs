using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace API_devbank.DTOs.Auth
{
    public class LoginRequest
    {
        [DefaultValue("teste3@gmail.com")]
        [EmailAddress]
        public string Email { get; set; }

        [DefaultValue("Aa123456")]
        [Required]
        public string Senha { get; set; }
    }
}
