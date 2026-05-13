using API_devbank.Common;
using API_devbank.DTOs.Auth;
using API_devbank.Enums;
using API_devbank.Models;
using API_devbank.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace API_devbank.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : BaseController
    {

        private readonly LoginService _loginService;
        
        public LoginController(LoginService loginService)
        {
            _loginService = loginService;
        }

        [HttpPost]
        public IActionResult Login(LoginRequest dto)
        {
            return Resultado(_loginService.Login(dto));
        }
    }
}
