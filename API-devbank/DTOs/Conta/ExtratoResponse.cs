using API_devbank.Enums;

namespace API_devbank.DTOs.Conta
{
    public class ExtratoResponse
    {
        public string Tipo { get; set; }
        public decimal Valor { get; set; }
        public DateTime Data { get; set; }
        public string Direcao { get; set; }
        public string? NomeDestino { get; set; }
        public string? NomeOrigem { get; set; }
    }
}
