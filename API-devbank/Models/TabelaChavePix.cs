using API_devbank.Enums;
using static System.Net.Mime.MediaTypeNames;

namespace API_devbank.Models
{
    public class TabelaChavePix
    {
        public int Id { get; set; }

        public TipoChave Tipo { get; set; }

        public string Chave { get; set; }

        public int ContaId { get; set; }

        public bool IsAtivo { get; set; }
        public TabelaConta Conta { get; set; }
    }
}
