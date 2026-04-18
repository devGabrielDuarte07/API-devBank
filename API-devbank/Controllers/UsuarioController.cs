using API_devbank.DTOs.Usuario;
using API_devbank.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace API_devbank.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuarioController : ControllerBase
    {
        private readonly DevbankContext db;
        private static readonly Regex SenhaRegex = new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$");

        public UsuarioController(DevbankContext context)
        {
            db = context;
        }

        [HttpPost]
        public async Task<IActionResult> CriarUsuario(CriarUsuarioRequest dto)
        {
            if (!ModelState.IsValid) 
            { 
                return BadRequest(ModelState);
            }
            if (!SenhaRegex.IsMatch(dto.Senha))
            {
                return BadRequest("A senha deve ter no mínimo 8 caracteres, com letra maiúscula, minúscula e número");
            }
            var existe = db.TabelaUsuarios.Any(u => u.Cpf == dto.CPF || u.Email == dto.Email);
            if (existe)
            {
                return BadRequest("CPF ou email ja cadastrado");
            }

            using var transaction = await db.Database.BeginTransactionAsync();

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
                    Saldo = 00
                };

                db.TabelaContas.Add(conta);
                await db.SaveChangesAsync();

                await transaction.CommitAsync();
                return Ok("Usuario criado com sucesso");
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return BadRequest("Erro ao criar usuário");
            }
        }
    }
}
