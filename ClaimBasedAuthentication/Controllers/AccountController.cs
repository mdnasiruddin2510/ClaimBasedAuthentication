using ClaimBasedAuthentication.Application.IRepository;
using ClaimBasedAuthentication.Application.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClaimBasedAuthentication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        public AccountController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }
        [Route("GetRole")]
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<List<VmSelectList>>> GetRole()
        {
            return Ok(await _userRepository.GetDrpRole());
        }
    }
}
