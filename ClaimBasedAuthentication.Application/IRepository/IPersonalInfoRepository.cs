using ClaimBasedAuthentication.Application.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClaimBasedAuthentication.Application.IRepository
{
    public interface IPersonalInfoRepository
    {
        Task<VmResponseMessage> CreatePersonalInfo(VmPersonalInfo vm);
    }
}
