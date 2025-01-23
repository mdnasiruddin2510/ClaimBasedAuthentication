using ClaimBasedAuthentication.Application.IRepository;
using ClaimBasedAuthentication.Application.ViewModel;
using ClaimBasedAuthentication.Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ClaimBasedAuthentication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AuthController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        public AuthController(IUserRepository userRepository)
        {
              _userRepository = userRepository;
        }
        [Route("Login")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] AuthenticationRequest request)
        {
            var response = await _userRepository.Login(request);
            var authAdd = new ClaimsIdentity();
            authAdd.AddClaims(response.ClaimList);
            HttpContext.User.AddIdentity(authAdd);

            HttpContext.Session.SetString("token", response.JWToken);
            HttpContext.Session.SetString("userId", response.Id);
            HttpContext.Session.SetString("userName", response.UserName);
            return Ok(response);
        }
    }
}
