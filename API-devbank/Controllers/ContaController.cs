using API_devbank.DTOs.Conta;
using API_devbank.Enums;
using API_devbank.Models;
using API_devbank.Services;
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
    public class ContaController : BaseController
    {
        private readonly ContaService _contaService;
        public ContaController(ContaService contaService)
        {
            _contaService = contaService;
        }
        
        [Authorize]
        [HttpGet]
        public IActionResult BuscarSaldo()
        {
            return Resultado(_contaService.DadosUsuarioLogado());
        }


        [Authorize]
        [HttpPost("deposito")]
        public async Task<IActionResult>  Deposito(ValorRequest dto)
        {
            return Resultado(await _contaService.Depositar(dto));
        }


        [Authorize]
        [HttpPost("saque")]
        public async Task<IActionResult> Saque(ValorRequest dto)
        {
            return Resultado(await _contaService.Sacar(dto)); 
        }


        [Authorize]
        [HttpPost("transferencia")]
        public async Task<IActionResult> Transferencia(TransferenciaRequest dto)
        {
            return Resultado(await _contaService.Transferir(dto));
        }


        [Authorize]
        [HttpGet("extrato")]
        public async Task<IActionResult> Extrato()
        {
            return Resultado(await _contaService.Extrato());
        }




        
        
    }
}
