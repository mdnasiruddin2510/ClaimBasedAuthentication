using ClaimBasedAuthentication.Domain.Services.Claims;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClaimBasedAuthentication.Application.ViewModel
{
    public class VmClaimGrid
    {
        public VmClaimGrid()
        {
            AplicationCalimList = new List<VmApplicationClaim>();
        }
        public bool AllCheckbox { get; set; }
        public string RoleId { get; set; }
        public string RoleName { get; set; }
        public List<VmApplicationClaim> AplicationCalimList { get; set; }
    }
}
