using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClaimBasedAuthentication.Domain.Constant
{
    public class ClaimConstant
    {
        public static class Permission
        {
            public const string CoreHomeMenu = "Core:Home:Index";
            public const string CorePrivacyMenu = "Core:Home:Privacy";
            public const string CorePermissionMenu = "Core:Permission:Role";
        }
    }
}
