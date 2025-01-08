using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using static ClaimBasedAuthentication.Domain.Constant.ClaimConstant;

namespace ClaimBasedAuthentication.Domain.Services.Claims
{
    public static class ClaimHelper
    {
        public static List<VmSelectListItemClaim> GetAllClaimList()
        {
            List<VmSelectListItemClaim> AllClaimList = new();
            AllClaimList.AddRange(GetPermissionList());
            return AllClaimList;
        }

        public static List<VmSelectListItemClaim> GetPermissionList()
        {
            Type t = typeof(Permission);
            FieldInfo[] fields = t.GetFields(BindingFlags.Static | BindingFlags.Public);
            List<VmSelectListItemClaim> HrClaimList = new();
            foreach (FieldInfo fi in fields)
            {
                VmSelectListItemClaim vmSelectListItem = new();
                vmSelectListItem.Text = fi.Name;
                vmSelectListItem.Value = fi.GetValue(null).ToString();
                HrClaimList.Add(vmSelectListItem);
            }
            return HrClaimList;
        }

        public static List<VmSelectListItemClaim> GetPermissionListOfAdmin(this IList<Claim> permittedClaimListForAdminRole)
        {
            Type t = typeof(Permission);
            FieldInfo[] fields = t.GetFields(BindingFlags.Static | BindingFlags.Public);
            List<VmSelectListItemClaim> HrClaimList = new();
            foreach (FieldInfo fi in fields)
            {
                if (permittedClaimListForAdminRole.Any(a => a.Type == fi.GetValue(null).ToString()))
                {
                    VmSelectListItemClaim vmSelectListItem = new();
                    vmSelectListItem.Text = fi.Name;
                    vmSelectListItem.Value = fi.GetValue(null).ToString();
                    HrClaimList.Add(vmSelectListItem);
                }
            }
            return HrClaimList;
        }

        public static List<VmSelectListItemClaim> SetGivenPermissionInClaimList(this List<VmSelectListItemClaim> claimSelectList, IList<Claim> permittedClaimList)
        {
            foreach (var claim in claimSelectList)
            {
                if (permittedClaimList.Any(a => a.Type == claim.Value))
                {
                    claim.Selected = true;
                }
                claim.ClaimParts = claim.Value.Split(":").ToList();
            }
            return claimSelectList;
        }

        public static List<VmApplicationClaim> ConvertClaimSelectListItemToClaimItem(this List<VmSelectListItemClaim> claimList)
        {
            var aplicationCalimList = new List<VmApplicationClaim>();

            foreach (var gByApp in claimList.GroupBy(g => g.ClaimParts[0]))
            {
                var aplicationCalim = new VmApplicationClaim
                {
                    AppName = gByApp.Key,
                };
                foreach (var gByController in gByApp.GroupBy(g => g.ClaimParts[1]))
                {
                    var ClaimItem = new VmClaimItem
                    {
                        ControllerName = gByController.Key,
                    };
                    foreach (var calim in gByController)
                    {
                        ClaimItem.Claims.Add(new VmSelectListItemClaim { Text = calim.ClaimParts[2], Value = calim.Value, Selected = calim.Selected });
                    }
                    if (ClaimItem.Claims.All(c => c.Selected))
                    {
                        ClaimItem.GroupCheckbox = true;
                    }
                    aplicationCalim.ClaimItems.Add(ClaimItem);
                }
                aplicationCalimList.Add(aplicationCalim);
            }
            return aplicationCalimList;
        }

        public static string AddSpacesToSentence(this string text, bool preserveAcronyms)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;
            StringBuilder newText = new(text.Length * 2);
            newText.Append(text[0]);
            for (int i = 1; i < text.Length; i++)
            {
                if (char.IsUpper(text[i]))
                    if ((text[i - 1] != ' ' && !char.IsUpper(text[i - 1])) ||
                        (preserveAcronyms && char.IsUpper(text[i - 1]) &&
                         i < text.Length - 1 && !char.IsUpper(text[i + 1])))
                        newText.Append(' ');
                newText.Append(text[i]);
            }
            return newText.ToString();
        }
    }
    public class VmApplicationClaim
    {
        public VmApplicationClaim()
        {
            ClaimItems = new List<VmClaimItem>();
        }
        public string AppName { get; set; }
        public List<VmClaimItem> ClaimItems { get; set; }
    }
    public class VmClaimItem
    {
        public VmClaimItem()
        {
            Claims = new List<VmSelectListItemClaim>();
        }
        public string ControllerName { get; set; }
        public bool GroupCheckbox { get; set; }
        public List<VmSelectListItemClaim> Claims { get; set; }
    }
    public class VmSelectListItemClaim
    {
        public List<string> ClaimParts { get; set; }
        public bool Selected { get; set; }
        public string Text { get; set; }
        public string Value { get; set; }
    }
}
