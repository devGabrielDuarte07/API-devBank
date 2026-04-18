namespace API_devbank.DTOs.Conta
{
    public class TransferenciaRequest
    {
        public int ContaDestinoId { get; set; }
        public decimal Valor { get; set;  }
    }
}
