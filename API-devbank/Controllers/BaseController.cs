using API_devbank.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API_devbank.Controllers
{
    [ApiController]
    public class BaseController : ControllerBase
    {
        protected IActionResult Resultado<T>(ResultadoPadrao<T> resultado)
        {
            return StatusCode(resultado.StatusCode, resultado);
        }
    }
}
