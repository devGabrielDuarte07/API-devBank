namespace API_devbank.DTOs.Conta
{
    public class TransferenciaRequest
    {
        public string CpfContaDestino { get; set; }
        public decimal Valor { get; set;  }
    }
}
