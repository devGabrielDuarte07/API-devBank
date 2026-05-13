using API_devbank.Common;
using API_devbank.DTOs.Pix;
using API_devbank.Enums;
using API_devbank.Models;
using System.Security.Claims;
using System.Text.RegularExpressions;
using static System.Net.Mime.MediaTypeNames;

namespace API_devbank.Services
{
    public class PixService
    {
        private readonly DevbankContext db;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public PixService(DevbankContext db, IHttpContextAccessor httpContextAccessor)
        {
            this.db = db;
            _httpContextAccessor = httpContextAccessor;
        }

        public ResultadoPadrao<object> CadastrarChavePix(CriarChaveRequest dto)
        {
            var chaveJaExiste = db.TabelaChavePix.Any(c => c.Chave == dto.Chave && c.IsAtivo);

            if (chaveJaExiste)
                return ResultadoPadrao<object>.Falha("Chave já cadastrada", 409);

            var conta = ObterContaUsuarioLogado();
            if (conta == null)
                return ResultadoPadrao<object>.Falha("Conta não encontrada", 404);


            switch (dto.Tipo)
            {
                case Enums.TipoChave.EMAIL:
                    if (!ValidarEmail(dto.Chave))
                    {
                        return ResultadoPadrao<object>.Falha("Email inválido", 400);
                    }
                    break;

                case Enums.TipoChave.CPF:
                    if (!ValidarCPF(dto.Chave))
                    {
                        return ResultadoPadrao<object>.Falha("CPF inválido", 400);
                    }
                    break;

                case Enums.TipoChave.TELEFONE:
                    if (!ValidarTelefone(dto.Chave))
                    {
                        return ResultadoPadrao<object>.Falha("Telefone inválido", 400);
                    }

                    break;
            }

            var newChave = new TabelaChavePix
            {
                Tipo = dto.Tipo,
                Chave = dto.Chave,
                ContaId = conta.Id
            };

            db.TabelaChavePix.Add(newChave);
            db.SaveChanges();

            return ResultadoPadrao<object>.Ok("Chave cadastrada");
        }

        public ResultadoPadrao<List<ChaveCadastradaResponse>> ListarChaveDaConta()
        {
            var conta = ObterContaUsuarioLogado();
            if (conta == null)
                return ResultadoPadrao<List<ChaveCadastradaResponse>>.Falha("Conta não encontrada", 404);

            var chaves = db.TabelaChavePix.Where(c => c.ContaId == conta.Id && c.IsAtivo).Select(c => new ChaveCadastradaResponse
            {
                Tipo = c.Tipo,
                Chave = c.Chave
            }).ToList();

            if (chaves.Count == 0)
                return ResultadoPadrao<List<ChaveCadastradaResponse>>.Falha("Nenhuma chave pix cadastrada encontrada");

            return ResultadoPadrao<List<ChaveCadastradaResponse>>.Ok(chaves);
        }

        public ResultadoPadrao<object> ExcluirChave(int idChave)
        {
            var conta = ObterContaUsuarioLogado();
            if (conta == null)
                return ResultadoPadrao<object>.Falha("Conta não encontrada", 404);

            var chave = db.TabelaChavePix.FirstOrDefault(c => c.ContaId == conta.Id && c.Id == idChave && c.IsAtivo);
            if (chave == null)
                return ResultadoPadrao<object>.Falha("Chave não encontrada");

            chave.IsAtivo = false;
            db.SaveChanges();
            return ResultadoPadrao<object>.Ok("Chave deletada");

        }

        public async Task<ResultadoPadrao<object>> EnviarPix(
    EnviarPixRequest dto
)
        {
            if (dto.Valor <= 0)
            {
                return ResultadoPadrao<object>.Falha(
                    "Tem que ser um valor positivo",
                    400
                );
            }

            var chave = dto.Chave =
                dto.Chave.Trim().ToLower();

            var pixDestino = db.TabelaChavePix
                .FirstOrDefault(c =>
                    c.Chave == chave &&
                    c.IsAtivo
                );

            if (pixDestino == null)
            {
                return ResultadoPadrao<object>.Falha(
                    "Nenhuma chave pix encontrada",
                    404
                );
            }

            var contaDestino = db.TabelaContas
                .FirstOrDefault(c =>
                    c.Id == pixDestino.ContaId
                );

            if (contaDestino == null)
            {
                return ResultadoPadrao<object>.Falha(
                    "Conta não encontrada",
                    404
                );
            }

            var contaOrigem = ObterContaUsuarioLogado();

            if (contaOrigem == null)
            {
                return ResultadoPadrao<object>.Falha(
                    "Conta não encontrada",
                    404
                );
            }

            if (contaOrigem.Id == contaDestino.Id)
            {
                return ResultadoPadrao<object>.Falha(
                    "Você não pode enviar PIX para si mesmo",
                    400
                );
            }

            var strategy =
                db.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction =
                    await db.Database.BeginTransactionAsync();

                try
                {
                    var contaOrigemAtualizada =
                        AtualizarSaldo(
                            contaOrigem,
                            -dto.Valor
                        );

                    if (contaOrigemAtualizada == null)
                    {
                        await transaction.RollbackAsync();

                        return ResultadoPadrao<object>.Falha(
                            "Saldo insuficiente",
                            400
                        );
                    }

                    var contaDestinoAtualizada =
                        AtualizarSaldo(
                            contaDestino,
                            dto.Valor
                        );

                    if (contaDestinoAtualizada == null)
                    {
                        await transaction.RollbackAsync();

                        return ResultadoPadrao<object>.Falha(
                            "Erro ao processar crédito na conta destino",
                            400
                        );
                    }

                    var pix = new TabelaTransaco
                    {
                        Tipo = TipoTransacao.P.ToString(),
                        Valor = dto.Valor,
                        ContaOrigemId = contaOrigem.Id,
                        ContaDestinoId = contaDestino.Id,
                    };

                    db.TabelaTransacoes.Add(pix);

                    await db.SaveChangesAsync();

                    await transaction.CommitAsync();

                    return ResultadoPadrao<object>.Ok(
                        null,
                        mensagem:
                            $"Pix feito com sucesso, saldo atual: {contaOrigemAtualizada.Saldo}"
                    );
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();

                    return ResultadoPadrao<object>.Falha(
                        ex.InnerException?.Message ??
                        ex.Message,
                        500
                    );
                }
            });
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
        private TabelaConta ObterContaUsuarioLogado()
        {
            var user = _httpContextAccessor.HttpContext?.User;

            var userId = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userId, out int id))
                throw new Exception("Usuário não autenticado ou ID inválido");

            var conta = db.TabelaContas.FirstOrDefault(c => c.IdUsuario == id);

            return conta;
        }
        private bool ValidarEmail(string email)
        {
            return Regex.IsMatch(
                email,
                @"^[^@\s]+@[^@\s]+\.[^@\s]+$"
            );
        }

        private bool ValidarTelefone(string telefone)
        {
            telefone = Regex.Replace(telefone, @"\D", "");

            return telefone.Length >= 10
                && telefone.Length <= 11;
        }

        private bool ValidarCPF(string cpf)
        {
            cpf = Regex.Replace(cpf, @"\D", "");

            if (cpf.Length != 11)
                return false;

            if (cpf.All(c => c == cpf[0]))
                return false;

            return true;
        }
    }
}
