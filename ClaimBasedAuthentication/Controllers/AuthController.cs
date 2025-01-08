using ClaimBasedAuthentication.Application.IRepository;
using ClaimBasedAuthentication.Application.ViewModel;
using ClaimBasedAuthentication.Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
            HttpContext.Session.SetString("token", response.JWToken);
            HttpContext.Session.SetString("userId", response.Id);
            HttpContext.Session.SetString("userName", response.UserName);
            return Ok(response);
        }
    }
}
