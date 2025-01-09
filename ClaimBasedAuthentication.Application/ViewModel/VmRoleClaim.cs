using ClaimBasedAuthentication.Domain.Services.Claims;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClaimBasedAuthentication.Application.ViewModel
{
    public class VmRoleClaim
    {
        public VmRoleClaim()
        {
            ClaimGroupList = new List<VmSelectListItemClaim>();
        }
        public string RoleName { get; set; }
        public string RoleId { get; set; }
        public string ClaimGroupName { get; set; }
        public List<VmSelectListItemClaim> ClaimGroupList { get; set; }
    }
}
