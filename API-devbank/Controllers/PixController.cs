using API_devbank.DTOs.Pix;
using API_devbank.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API_devbank.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PixController : BaseController
    {
        private readonly PixService _pixService;
        public PixController(PixService pixService)
        {
            _pixService = pixService;
        }

        [Authorize]
        [HttpGet]
        public IActionResult BuscarChavesDaConta()
        {
            return Resultado(_pixService.ListarChaveDaConta());
        }

        [Authorize]
        [HttpPost]
        public IActionResult CadastrarChave(CriarChaveRequest dto)
        {
            return Resultado(_pixService.CadastrarChavePix(dto));
        }

        [Authorize]
        [HttpDelete("{idChave}")]
        public IActionResult ExcluirChave(int idChave)
        {
            return Resultado(_pixService.ExcluirChave(idChave));
        }

        [Authorize]
        [HttpPost("enviar")]
        public async Task<IActionResult> EnviarPix(EnviarPixRequest dto)
        {
            return Resultado(await _pixService.EnviarPix(dto));
        }
    }
}
