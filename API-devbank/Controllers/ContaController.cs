using API_devbank.DTOs.Conta;
using API_devbank.Enums;
using API_devbank.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Drawing;
using System.Security.Claims;
using System.Threading.Tasks.Dataflow;
using static System.Net.Mime.MediaTypeNames;

namespace API_devbank.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContaController : ControllerBase
    {
        private readonly DevbankContext db;
        public ContaController(DevbankContext context)
        {
            db = context;
        }
        
        [Authorize]
        [HttpGet]
        public IActionResult BuscarSaldo()
        {
            var conta = ObterContaUsuarioLogado();

            if (conta == null)
                return NotFound("Conta não encontrada");

            return Ok(new { saldo = conta.Saldo });
        }


        [Authorize]
        [HttpPost("deposito")]
        public async Task<IActionResult> Deposito(ValorRequest dto)
        {
            var conta = ObterContaUsuarioLogado();
            if (conta == null)
                return BadRequest("Conta não encotrada");

            if (dto.Valor <= 0)
                return BadRequest("Valor tem que ser positivo");

            using var transaction = await db.Database.BeginTransactionAsync();
            try
            {
               

                var ContaAtualizada = AtualizarSaldo(conta, dto.Valor);
                if (ContaAtualizada == null)
                {
                    await transaction.RollbackAsync();
                    return BadRequest("Erro ao atualizar saldo");
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
                return Ok($"Depósito feito com sucesso, saldo atual: {conta.Saldo }");
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return BadRequest("Erro ao fazer deposito");
            }
           
        }


        [Authorize]
        [HttpPost("saque")]
        public async Task<IActionResult> Saque(ValorRequest dto)
        {
            var conta = ObterContaUsuarioLogado();
            if (conta == null)
                return BadRequest("Conta não encotrada");

            if (dto.Valor <= 0)
                return BadRequest("Valor tem que ser positivo");

            using var transaction = await db.Database.BeginTransactionAsync();
            try
            {
                var valor = -dto.Valor;

                var ContaAtualizada = AtualizarSaldo(conta, valor);

                if (ContaAtualizada == null)
                {
                    await transaction.RollbackAsync();
                    return BadRequest("Saldo insuficiente");
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
                return Ok($"Saque feito com sucesso, saldo atual: {conta.Saldo}");
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return BadRequest("Erro ao fazer saque");
            }
        }


        [Authorize]
        [HttpPost("transferencia")]
        public async Task<IActionResult> Transferencia(TransferenciaRequest dto)
        {
            if(dto.Valor <= 0) 
            {
                return BadRequest("Tem que ser um valor positivo");
            }

            var contaDestino = db.TabelaContas.FirstOrDefault(c => c.IdUsuario == dto.ContaDestinoId);

            if(contaDestino == null)
            {
                return BadRequest("Conta destino não encontrada");
            }

            var contaOrigem = ObterContaUsuarioLogado();
            if(contaOrigem == null)
            {
                return BadRequest("Conta não encontrada");
            }

            if(contaOrigem.Id == contaDestino.Id)
            {
                return BadRequest("não pode transferir para própia conta");
            }

            using var transaction = await db.Database.BeginTransactionAsync();

            try
            {
                var transferir = AtualizarSaldo(contaOrigem, -dto.Valor);
                if(transferir == null)
                {
                    await transaction.RollbackAsync();
                    return BadRequest("Saldo insuficiente");
                }
                var receber = AtualizarSaldo(contaDestino, dto.Valor);
                if(receber == null)
                {
                    await transaction.RollbackAsync();
                    return BadRequest("Erro ao processar crédito na conta destino");
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
                return Ok($"Transferencia feita com sucesso, saldo atual: {contaOrigem.Saldo}");
            }
            catch (Exception) 
            {
                await transaction.RollbackAsync();
                return BadRequest("Erro ao fazer transferencia");
            }
           
        }


        [Authorize]
        [HttpGet("extrato")]
        public IActionResult Extrato()
        {
            var conta = ObterContaUsuarioLogado();
            if (conta == null)
                return BadRequest("Conta não encontrada");

            var extrato = db.TabelaTransacoes.Where(e => e.ContaDestinoId == conta.Id || e.ContaOrigemId == conta.Id)
                .OrderByDescending(c => c.CriadoEm)
                .Select(c => new ExtratoResponse
                {
                    Tipo = c.Tipo,
                    Valor = c.Valor,
                    Data = c.CriadoEm,
                    Direcao = c.ContaDestinoId == conta.Id ? "Entrada" : "Saída"
                }).ToList();
            return Ok(extrato);
        }




        private TabelaConta AtualizarSaldo(TabelaConta conta, decimal valor)
        {
            decimal novoSaldo = conta.Saldo + valor; 
            if(novoSaldo < 0)
            {
                return null;
            }
            conta.Saldo = novoSaldo;

            return conta;
        }

        private TabelaConta ObterContaUsuarioLogado()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userId, out int id))
            {
                return null;
            }
            var conta = db.TabelaContas.FirstOrDefault(c => c.IdUsuario == id);

            return conta;
        }
    }
}
