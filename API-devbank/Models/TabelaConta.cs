using System;
using System.Collections.Generic;

namespace API_devbank.Models;

public partial class TabelaConta
{
    public int Id { get; set; }

    public int IdUsuario { get; set; }

    public decimal Saldo { get; set; }

    public DateTime CriadoEm { get; set; }

    public virtual TabelaUsuario IdUsuarioNavigation { get; set; } = null!;

    public virtual ICollection<TabelaTransaco> TabelaTransacoContaDestinos { get; set; } = new List<TabelaTransaco>();

    public virtual ICollection<TabelaTransaco> TabelaTransacoContaOrigems { get; set; } = new List<TabelaTransaco>();
}
