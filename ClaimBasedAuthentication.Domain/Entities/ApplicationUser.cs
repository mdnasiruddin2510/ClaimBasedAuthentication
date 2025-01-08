
using Microsoft.AspNetCore.Identity;

namespace ClaimBasedAuthentication.Domain.Entities
{
#nullable disable
    public class ApplicationUser:IdentityUser
    {
        public string FullName { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
        public string DeletedBy { get; set; }
        public DateTime? DeletedDate { get; set; }
        public string AvartarUrl { get; set; }
    }
}
