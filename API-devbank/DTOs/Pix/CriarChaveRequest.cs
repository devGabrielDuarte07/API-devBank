using API_devbank.Enums;

namespace API_devbank.DTOs.Pix
{
    public class CriarChaveRequest
    {
        public TipoChave Tipo { get; set; }
        public string Chave { get; set; } = string.Empty;

    }
}
