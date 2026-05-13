using API_devbank.DTOs.Usuario;
using API_devbank.Models;
using API_devbank.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace API_devbank.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuarioController : BaseController
    {
        private readonly UsuarioService _usuarioService;
        public UsuarioController(UsuarioService usuarioService)
        {
            _usuarioService = usuarioService;
        }

        [HttpPost]
        public async Task<IActionResult> CriarUsuario(CriarUsuarioRequest dto)
        {
            return Resultado(await _usuarioService.CriarUsuario(dto));
        }
    }
}
