using ClaimBasedAuthentication.Application.ViewModel;
using ClaimBasedAuthentication.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClaimBasedAuthentication.Application.IRepository
{
    public interface IUserRepository
    {
        Task<AuthenticationResponse> Login(AuthenticationRequest request);
        Task<List<VmSelectListItem>> GetDrpRole();
    }
}
