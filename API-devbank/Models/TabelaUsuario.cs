using System;
using System.Collections.Generic;

namespace API_devbank.Models;

public partial class TabelaUsuario
{
    public int Id { get; set; }

    public string Nome { get; set; } = null!;

    public string Cpf { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Telefone { get; set; } = null!;

    public string Perfil { get; set; } = null!;

    public string Senha { get; set; } = null!;

    public bool IsAtivo { get; set; }

    public DateTime CriadoEm { get; set; }

    public virtual ICollection<TabelaConta> TabelaConta { get; set; } = new List<TabelaConta>();
}
