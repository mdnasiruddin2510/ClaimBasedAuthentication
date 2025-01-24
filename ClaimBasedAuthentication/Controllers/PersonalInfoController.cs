using ClaimBasedAuthentication.Application.IRepository;
using ClaimBasedAuthentication.Application.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClaimBasedAuthentication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PersonalInfoController : ControllerBase
    {
        private readonly IPersonalInfoRepository _personalInfoRepository;
        public PersonalInfoController(IPersonalInfoRepository personalInfoRepository)
        {
            _personalInfoRepository = personalInfoRepository;   
        }
        [HttpPost("CreatePersonalInfo")]
        public async Task<IActionResult> CreatePersonalInfo([FromForm] VmPersonalInfo vm)
        {
            var response = await _personalInfoRepository.CreatePersonalInfo(vm);
            return Ok(response);
        }
    }
}
