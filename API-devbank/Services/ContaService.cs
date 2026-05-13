using API_devbank.Common;
using API_devbank.DTOs.Conta;
using API_devbank.DTOs.Usuario;
using API_devbank.Enums;
using API_devbank.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace API_devbank.Services
{
    public class ContaService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly DevbankContext db;
        public ContaService(IHttpContextAccessor httpContextAccessor, DevbankContext context)
        {
            _httpContextAccessor = httpContextAccessor;
            db = context;
        }

        public ResultadoPadrao<DadosUsuarioResponse> DadosUsuarioLogado()
        {
            var dados = ObterDadosUsuarioLogado();

            if (dados == null)
                return ResultadoPadrao<DadosUsuarioResponse>.Falha("Conta não encontrada", 404);


            return ResultadoPadrao<DadosUsuarioResponse>.Ok(dados);
        }

        public async Task<ResultadoPadrao<object>> Depositar(ValorRequest dto) 
        {
            var conta = ObterContaUsuarioLogado();
            if (conta == null)
                return ResultadoPadrao<object>.Falha("Conta não encontrada", 404);
            

            if (dto.Valor <= 0)
                return ResultadoPadrao<object>.Falha("Valor tem que ser positivo", 400);

            using var transaction = await db.Database.BeginTransactionAsync();
            try
            {


                var ContaAtualizada = AtualizarSaldo(conta, dto.Valor);
                if (ContaAtualizada == null)
                {
                    await transaction.RollbackAsync();
                    return ResultadoPadrao<object>.Falha("Erro ao atualizar saldo", 400);
                }

                var deposito = new TabelaTransaco
                {
                    Tipo = TipoTransacao.D.ToString(),
                    Valor = dto.Valor,
                    ContaDestinoId = conta.Id
                };
                db.TabelaTransacoes.Add(deposito);
                await db.SaveChangesAsync();
                await transaction.CommitAsync();

                return ResultadoPadrao<object>.Ok(null, mensagem: $"Depósito feito com sucesso, saldo atual: {conta.Saldo}");
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return ResultadoPadrao<object>.Falha("Erro ao fazer deposito", 500);
            }

        }

        public async Task<ResultadoPadrao<object>> Sacar(ValorRequest dto)
        {
            var conta = ObterContaUsuarioLogado();
            if (conta == null)
                return ResultadoPadrao<object>.Falha("Conta não encotrada", 404);

            if (dto.Valor <= 0)
                return ResultadoPadrao<object>.Falha("Valor tem que ser positivo", 400);

            using var transaction = await db.Database.BeginTransactionAsync();
            try
            {
                var valor = -dto.Valor;

                var ContaAtualizada = AtualizarSaldo(conta, valor);

                if (ContaAtualizada == null)
                {
                    await transaction.RollbackAsync();
                    return ResultadoPadrao<object>.Falha("Saldo insuficiente", 400);
                }

                var saque = new TabelaTransaco
                {
                    Tipo = TipoTransacao.S.ToString(),
                    Valor = dto.Valor,
                    ContaOrigemId = conta.Id
                };
                db.TabelaTransacoes.Add(saque);
                await db.SaveChangesAsync();
                await transaction.CommitAsync();
                return ResultadoPadrao<object>.Ok(null, mensagem: $"Saque feito com sucesso, saldo atual: {conta.Saldo}");
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return ResultadoPadrao<object>.Falha("Erro ao fazer saque", 500);
            }
        }

        public async Task<ResultadoPadrao<object>> Transferir(TransferenciaRequest dto)
        {
            if (dto.Valor <= 0)
            {
                return ResultadoPadrao<object>.Falha("Tem que ser um valor positivo", 400);
            }
            var usuarioDestino = db.TabelaUsuarios.FirstOrDefault(u => u.Cpf == dto.CpfContaDestino); 

            if (usuarioDestino == null) 
                return ResultadoPadrao<object>.Falha("CPF não encontrado", 404); 

            var contaDestino = db.TabelaContas.FirstOrDefault(c => c.IdUsuario == usuarioDestino.Id);

            if (contaDestino == null)
            {
                return ResultadoPadrao<object>.Falha("Conta destino não encontrada", 404);
            }

            var contaOrigem = ObterContaUsuarioLogado();
            if (contaOrigem == null)
            {
                return ResultadoPadrao<object>.Falha("Conta não encontrada", 404);
            }

            if (contaOrigem.Id == contaDestino.Id)
            {
                return ResultadoPadrao<object>.Falha("não pode transferir para própia conta", 400);
            }

            using var transaction = await db.Database.BeginTransactionAsync();

            try
            {
                var transferir = AtualizarSaldo(contaOrigem, -dto.Valor);
                if (transferir == null)
                {
                    await transaction.RollbackAsync();
                    return ResultadoPadrao<object>.Falha("Saldo insuficiente", 400);
                }
                var receber = AtualizarSaldo(contaDestino, dto.Valor);
                if (receber == null)
                {
                    await transaction.RollbackAsync();
                    return ResultadoPadrao<object>.Falha("Erro ao processar crédito na conta destino", 400);
                }

                var transferencia = new TabelaTransaco
                {
                    Tipo = TipoTransacao.T.ToString(),
                    Valor = dto.Valor,
                    ContaOrigemId = contaOrigem.Id,
                    ContaDestinoId = contaDestino.Id,
                };
                db.TabelaTransacoes.Add(transferencia);
                await db.SaveChangesAsync();
                await transaction.CommitAsync();
                return ResultadoPadrao<object>.Ok(null, mensagem: $"Transferencia feita com sucesso, saldo atual: {contaOrigem.Saldo}");
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return ResultadoPadrao<object>.Falha("Erro ao fazer transferencia", 500);
            }
        }

        public async Task<ResultadoPadrao<List<ExtratoResponse>>> Extrato()
        {
            var conta = ObterContaUsuarioLogado();
            if (conta == null)
                return ResultadoPadrao<List<ExtratoResponse>>.Falha("Conta não encontrada", 404);

            var extrato = await db.TabelaTransacoes.Where(e => e.ContaDestinoId == conta.Id || e.ContaOrigemId == conta.Id)
                .OrderByDescending(c => c.CriadoEm)
                .Select(c => new ExtratoResponse
                {
                    Tipo = c.Tipo,
                    Valor = c.Valor,
                    Data = c.CriadoEm,
                    Direcao = c.ContaDestinoId == conta.Id ? "Entrada" : "Saída"
                }).ToListAsync();
            return ResultadoPadrao<List<ExtratoResponse>>.Ok(extrato);
        }

      

        private TabelaConta AtualizarSaldo(TabelaConta conta, decimal valor)
        {
            decimal novoSaldo = conta.Saldo + valor;
            if (novoSaldo < 0)
            {
                return null;
            }
            conta.Saldo = novoSaldo;

            return conta;
        }


        private DadosUsuarioResponse ObterDadosUsuarioLogado()
        {
            var user = _httpContextAccessor.HttpContext?.User;

            var userId = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userId, out int id))
                throw new Exception("Usuário não autenticado ou ID inválido");

            var dados = (from u in db.TabelaUsuarios
                        join c in db.TabelaContas on u.Id equals c.IdUsuario
                        where u.Id == id
                        select new DadosUsuarioResponse
                        {
                            Nome = u.Nome,
                            CPF = u.Cpf,
                            Email = u.Email,
                            Telefone = u.Telefone,
                            Saldo = c.Saldo
                        }).FirstOrDefault();

            return dados;
        }

        private TabelaConta ObterContaUsuarioLogado()
        {
            var user = _httpContextAccessor.HttpContext?.User;

            var userId = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userId, out int id))
                throw new Exception("Usuário não autenticado ou ID inválido");

            var conta = db.TabelaContas.FirstOrDefault(c => c.IdUsuario == id);

            return conta;
        }
    }
}
