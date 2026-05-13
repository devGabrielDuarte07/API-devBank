using API_devbank.Common;
using API_devbank.DTOs.Usuario;
using API_devbank.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace API_devbank.Services
{
    public class UsuarioService
    {
        private readonly DevbankContext db;
        private static readonly Regex SenhaRegex = new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$");

        public UsuarioService(DevbankContext context)
        {
            db = context;
        }

        public async Task<ResultadoPadrao<object>> CriarUsuario(
     CriarUsuarioRequest dto
 )
        {
            if (!SenhaRegex.IsMatch(dto.Senha))
            {
                return ResultadoPadrao<object>.Falha(
                    "A senha deve ter no mínimo 8 caracteres, com letra maiúscula, minúscula e número",
                    400
                );
            }

            var existe = await db.TabelaUsuarios.AnyAsync(
                u => u.Cpf == dto.CPF || u.Email == dto.Email
            );

            if (existe)
            {
                return ResultadoPadrao<object>.Falha(
                    "CPF ou email ja cadastrado",
                    404
                );
            }

            try
            {
                var usuario = new TabelaUsuario
                {
                    Nome = dto.Nome,
                    Cpf = dto.CPF,
                    Email = dto.Email,
                    Telefone = dto.Telefone,
                    Senha = BCrypt.Net.BCrypt.HashPassword(dto.Senha),
                    IsAtivo = true
                };

                db.TabelaUsuarios.Add(usuario);

                await db.SaveChangesAsync();

                var conta = new TabelaConta
                {
                    IdUsuario = usuario.Id,
                    Saldo = 0
                };

                db.TabelaContas.Add(conta);

                await db.SaveChangesAsync();

                return ResultadoPadrao<object>.Ok(
                    "Usuario criado com sucesso"
                );
            }
            catch (Exception ex)
            {
                return ResultadoPadrao<object>.Falha(
                    ex.InnerException?.Message ?? ex.Message
                );
            }
        }
    }
}
