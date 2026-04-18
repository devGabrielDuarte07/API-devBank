using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;

namespace API_devbank.Models;

public partial class DevbankContext : DbContext
{
    public DevbankContext()
    {
    }

    public DevbankContext(DbContextOptions<DevbankContext> options)
        : base(options)
    {
    }

    public virtual DbSet<TabelaConta> TabelaContas { get; set; }

    public virtual DbSet<TabelaTransaco> TabelaTransacoes { get; set; }

    public virtual DbSet<TabelaUsuario> TabelaUsuarios { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseMySql("server=localhost;database=devbank;user=root;password=senai", Microsoft.EntityFrameworkCore.ServerVersion.Parse("12.0.2-mariadb"));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_uca1400_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<TabelaConta>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("tabela_contas");

            entity.HasIndex(e => e.IdUsuario, "FK__tabela_usuarios");

            entity.Property(e => e.Id)
                .HasColumnType("int(11)")
                .HasColumnName("id");
            entity.Property(e => e.CriadoEm)
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("timestamp")
                .HasColumnName("criado_em");
            entity.Property(e => e.IdUsuario)
                .HasColumnType("int(11)")
                .HasColumnName("id_usuario");
            entity.Property(e => e.Saldo)
                .HasPrecision(10, 2)
                .HasColumnName("saldo");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.TabelaConta)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__tabela_usuarios");
        });

        modelBuilder.Entity<TabelaTransaco>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("tabela_transacoes");

            entity.HasIndex(e => e.ContaOrigemId, "FK_tabela_transacoes_tabela_contas");

            entity.HasIndex(e => e.ContaDestinoId, "FK_tabela_transacoes_tabela_contas_2");

            entity.Property(e => e.Id)
                .HasColumnType("int(11)")
                .HasColumnName("id");
            entity.Property(e => e.ContaDestinoId)
                .HasColumnType("int(11)")
                .HasColumnName("conta_destino_id");
            entity.Property(e => e.ContaOrigemId)
                .HasColumnType("int(11)")
                .HasColumnName("conta_origem_id");
            entity.Property(e => e.CriadoEm)
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("timestamp")
                .HasColumnName("criado_em");
            entity.Property(e => e.Tipo)
                .HasColumnType("enum('D','S','T')")
                .HasColumnName("tipo");
            entity.Property(e => e.Valor)
                .HasPrecision(10, 2)
                .HasColumnName("valor");

            entity.HasOne(d => d.ContaDestino).WithMany(p => p.TabelaTransacoContaDestinos)
                .HasForeignKey(d => d.ContaDestinoId)
                .HasConstraintName("FK_tabela_transacoes_tabela_contas_2");

            entity.HasOne(d => d.ContaOrigem).WithMany(p => p.TabelaTransacoContaOrigems)
                .HasForeignKey(d => d.ContaOrigemId)
                .HasConstraintName("FK_tabela_transacoes_tabela_contas");
        });

        modelBuilder.Entity<TabelaUsuario>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("tabela_usuarios");

            entity.HasIndex(e => e.Cpf, "CPF").IsUnique();

            entity.HasIndex(e => e.Email, "email").IsUnique();

            entity.Property(e => e.Id)
                .HasColumnType("int(11)")
                .HasColumnName("id");
            entity.Property(e => e.Cpf)
                .HasMaxLength(11)
                .HasColumnName("CPF");
            entity.Property(e => e.CriadoEm)
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("timestamp")
                .HasColumnName("criado_em");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.IsAtivo)
                .HasDefaultValueSql("b'1'")
                .HasColumnType("bit(1)")
                .HasColumnName("isAtivo");
            entity.Property(e => e.Nome)
                .HasMaxLength(100)
                .HasColumnName("nome");
            entity.Property(e => e.Perfil)
                .HasDefaultValueSql("'C'")
                .HasColumnType("enum('C','A')")
                .HasColumnName("perfil");
            entity.Property(e => e.Senha)
                .HasMaxLength(255)
                .HasDefaultValueSql("''")
                .HasColumnName("senha");
            entity.Property(e => e.Telefone)
                .HasMaxLength(20)
                .HasColumnName("telefone");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
