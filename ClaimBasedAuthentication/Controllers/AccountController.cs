using ClaimBasedAuthentication.Application.IRepository;
using ClaimBasedAuthentication.Application.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ClaimBasedAuthentication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AccountController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        public AccountController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }
        [Route("GetRole")]
        [HttpGet]
        //[AllowAnonymous]
        public async Task<ActionResult<List<VmSelectList>>> GetRole()
        {
            return Ok(await _userRepository.GetDrpRole());
        }
        [Route("GetAllClaims")]
        [HttpGet]
        public async Task<ActionResult<VmRoleClaim>> GetAllClaims(string roleId)
        {
            var currentUserRole = HttpContext.User.FindFirstValue(ClaimTypes.Role);
            var claims = await _userRepository.GetAllClaimsAsync(roleId, currentUserRole);
            return Ok(claims);
        }
        [Route("SaveClaims")]
        [HttpPost]
        public async Task<ActionResult> SaveClaims(VmSaveClaims vm)
        {
            await _userRepository.SaveClaimsAsync(vm);
            return Ok();
        }
    }
}
