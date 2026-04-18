using System;
using System.Collections.Generic;

namespace API_devbank.Models;

public partial class TabelaTransaco
{
    public int Id { get; set; }

    public string Tipo { get; set; } = null!;

    public decimal Valor { get; set; }

    public int? ContaOrigemId { get; set; }

    public int? ContaDestinoId { get; set; }

    public DateTime CriadoEm { get; set; }

    public virtual TabelaConta? ContaDestino { get; set; }

    public virtual TabelaConta? ContaOrigem { get; set; }
}
