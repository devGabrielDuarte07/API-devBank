using API_devbank.Common;
using API_devbank.DTOs.Auth;
using API_devbank.Enums;
using API_devbank.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace API_devbank.Services
{
    public class LoginService
    {
        private readonly DevbankContext db;
        private readonly IConfiguration _config;

        public LoginService(DevbankContext db, IConfiguration config)
        {
            this.db = db;
            _config = config;
        }

        public ResultadoPadrao<object> Login(LoginRequest dto)
        {
            var usuario = db.TabelaUsuarios.FirstOrDefault(u => u.Email == dto.Email);

            if (usuario == null || !BCrypt.Net.BCrypt.Verify(dto.Senha, usuario.Senha))
            {
                return ResultadoPadrao<object>.Falha("Email ou senha inválidos", 401);
            }

            string role = usuario.Perfil == PerfilEnum.A.ToString() ? "admin" : "cliente";

            var claims = new List<Claim> {
                       new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                    new Claim(ClaimTypes.Role, role)
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["Jwt:Key"])
            );

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddDays(7),
                signingCredentials: creds
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            return ResultadoPadrao<object>.Ok(new { token = tokenString });
        }
    }
}
