using ClaimBasedAuthentication.Application.Exceptions;
using ClaimBasedAuthentication.Application.IRepository;
using ClaimBasedAuthentication.Application.ViewModel;
using ClaimBasedAuthentication.Domain.Common;
using ClaimBasedAuthentication.Domain.Entities;
using ClaimBasedAuthentication.Domain.Services.Authentication;
using ClaimBasedAuthentication.Domain.Services.Claims;
using ClaimBasedAuthentication.Persistence.Context;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ClaimBasedAuthentication.Persistence.Repositories
{
    public class UserRepository : IUserRepository
    {
        protected readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _config;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public UserRepository(ApplicationDbContext db, UserManager<ApplicationUser> userManager, 
            SignInManager<ApplicationUser> signInManager, RoleManager<IdentityRole> roleManager,
            IConfiguration config, IWebHostEnvironment webHostEnvironment)
        {
            _db = db;
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _config = config;
            _webHostEnvironment = webHostEnvironment;
        }
        public async Task<AuthenticationResponse> Login(AuthenticationRequest request)
        {
            var user = await _userManager.FindByNameAsync(request.Username);
            if (user == null)
            {
                throw new ApiException($"No Accounts Registered with {request.Username}.");
            }
            if (!user.IsActive)
            {
                throw new ApiException($"Your account is currently inactive {request.Username}.");
            }
            JwtSecurityToken jwtSecurityToken = await GenerateJWToken(user);
            
            AuthenticationResponse response = new()
            {
                Id = user.Id,
                JWToken = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken),
                FullName = user.FullName,
                UserName = user.UserName,
                UserAvatar = ""
            };
            //var rolesList = await _userManager.GetRolesAsync(user).ConfigureAwait(false);
            //response.Roles = rolesList.ToList();
            var userRole = await _db.UserRoles.FirstOrDefaultAsync(x => x.UserId.Equals(user.Id));
            var role = await _db.Roles.FirstOrDefaultAsync(x => x.Id.Equals(userRole.RoleId));
            response.Roles = new List<string>
            {
                role.Name
            };
            var claims = new[] //if Claim
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("uid", user.Id),
                new Claim("username", user.UserName),
                new Claim("fullname", user.FullName),
                new Claim("avatar", user.AvartarUrl??""),
                new Claim("roles", role.Name),
            };
            response.ClaimList = claims;
            var refreshToken = GenerateRefreshToken();
            response.RefreshToken = refreshToken.Token;
            return response;
        }
        private RefreshToken GenerateRefreshToken()
        {
            return new RefreshToken
            {
                Token = RandomTokenString(),
                Expires = DateTime.Now.AddMonths(12),
                Created = DateTime.Now
            };
        }
        private async Task<JwtSecurityToken> GenerateJWToken(ApplicationUser user)
        {
            var userRole = await _db.UserRoles.FirstOrDefaultAsync(x => x.UserId.Equals(user.Id));
            var role = await _db.Roles.FirstOrDefaultAsync(x => x.Id.Equals(userRole.RoleId));
            var claims = new[] //if Claim
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("uid", user.Id),
                new Claim("username", user.UserName),
                new Claim("fullname", user.FullName),
                new Claim("avatar", user.AvartarUrl??""),
                new Claim("roles", role.Name),
            };


            var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Auth:Secret"]));
            var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);

            var jwtSecurityToken = new JwtSecurityToken(
                issuer: _config["Auth:Authority"],
                audience: _config["Auth:Audience"],
                claims: claims,
                expires: DateTime.Today.AddDays(7),
                signingCredentials: signingCredentials);
            return jwtSecurityToken;
        }
        private string RandomTokenString()
        {
            int length = 20;
            const string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            StringBuilder res = new();
            using (RNGCryptoServiceProvider rng = new())
            {
                byte[] uintBuffer = new byte[sizeof(uint)];

                while (length-- > 0)
                {
                    rng.GetBytes(uintBuffer);
                    uint num = BitConverter.ToUInt32(uintBuffer, 0);
                    res.Append(valid[(int)(num % (uint)valid.Length)]);
                }
            }

            return res.ToString();
        }
        public async Task<List<VmSelectListItem>> GetDrpRole()
        {
            return await _db.Roles
                            .Select(s => new VmSelectListItem
                            {
                                Text = s.Name,
                                Value = s.Id.ToString()
                            }).ToListAsync();
        }
        public async Task<VmClaimGrid> GetAllClaimsAsync(string roleId, string currentRole)
        {
            var claimGrid = new VmClaimGrid();
            var role = await _roleManager.FindByIdAsync(roleId);
            var roleClaim = await _roleManager.GetClaimsAsync(role);
            var currentUserRole = await _roleManager.FindByNameAsync(currentRole);
            var currrentUserRoleClaim = await _roleManager.GetClaimsAsync(currentUserRole);
            var claimList = currentUserRole.Name switch
            {
                "Admin" => ClaimHelper.GetPermissionList(),
                _ => currrentUserRoleClaim.GetPermissionListOfAdmin(),
            };
            claimList = claimList.SetGivenPermissionInClaimList(roleClaim);
            claimGrid.AplicationCalimList = claimList.ConvertClaimSelectListItemToClaimItem();
            claimGrid.RoleId = role.Id;
            claimGrid.RoleName = role.Name;
            if (claimGrid.AplicationCalimList.All(a => a.ClaimItems.All(c => c.GroupCheckbox)))
            {
                claimGrid.AllCheckbox = true;
            }
            return claimGrid;
        }
        public async Task SaveClaimsAsync(VmSaveClaims vm)
        {
            try
            {
                var role = await _roleManager.FindByIdAsync(vm.RoleId);
                var allClaims = await _db.RoleClaims.Where(x => x.RoleId == vm.RoleId).ToListAsync();
                //foreach (var item in allClaims)
                //{
                //	_db.Attach(item);
                //}

                _db.RoleClaims.RemoveRange(allClaims);
                await _db.SaveChangesAsync();
                foreach (var item in vm.Claims)
                {
                    await _roleManager.AddClaimAsync(role, new Claim(item, role.Id));
                }
            }
            catch (Exception)
            { }
        }
    }
}
