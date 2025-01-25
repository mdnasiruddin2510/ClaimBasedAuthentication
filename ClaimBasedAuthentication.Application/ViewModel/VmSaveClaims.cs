using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClaimBasedAuthentication.Application.ViewModel
{
    public class VmSaveClaims
    {
        public string RoleId { get; set; }
        public List<string> Claims { get; set; }
    }
}
