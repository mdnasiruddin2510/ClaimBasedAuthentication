using ClaimBasedAuthentication.Domain.Entities;
using ClaimBasedAuthentication.Domain.Services.Claims;
using ClaimBasedAuthentication.Persistence.Context;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ClaimBasedAuthentication.Persistence.Services
{
    public static class IdentityServiceExtensions
    {
        public static void AddIdentityServices(this IServiceCollection services, IConfiguration configuration)
        {

            services.AddDbContext<ApplicationDbContext>(config =>
            {
                config.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
            });

            services.AddIdentity<ApplicationUser, IdentityRole>()
                    .AddEntityFrameworkStores<ApplicationDbContext>()
                    .AddDefaultTokenProviders();
            services.Configure<IdentityOptions>(options =>
            {
                options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
                options.User.RequireUniqueEmail = false;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = 6;
                options.Password.RequiredUniqueChars = 0;
                options.SignIn.RequireConfirmedEmail = false;
                options.SignIn.RequireConfirmedPhoneNumber = false;
            });
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(o =>
            {
                var auth = configuration.GetSection("Auth");
                o.Authority = auth["Authority"];
                o.Audience = auth["Audience"];
                o.RequireHttpsMetadata = false;
                o.SaveToken = true;
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    //ValidateAudience = true,
                    //ValidateIssuer = true,
                    //ValidateIssuerSigningKey = true,
                    //IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(auth["Secret"])),
                    //ClockSkew = TimeSpan.Zero

                    RequireExpirationTime = false,
                    ClockSkew = TimeSpan.Zero,
                    ValidateLifetime = true,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidAudience = auth["Audience"],
                    ValidIssuer = auth["Authority"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(auth["Secret"]))

                };
            });
            services.AddAuthorization(authOption =>
            {
                authOption.AddPolicy("ApiScope", builder =>
                {
                    builder.RequireAuthenticatedUser()
                           .RequireClaim("scope", "https://pronali.net");
                });
            });
            services.AddCors(o =>
            {
                o.AddPolicy("AllowAllOrigin", builder =>
                {
                    builder
                    .AllowAnyOrigin()
                    .SetIsOriginAllowedToAllowWildcardSubdomains()
                    .AllowAnyHeader()
                    .AllowAnyMethod();
                });
            });
            //services.AddSession(options =>
            //{
            //    options.IdleTimeout = TimeSpan.FromMinutes(300);
            //});
        }
        public static async void CreateAdministrator(this IServiceCollection services)
        {
            var _userManager = services.BuildServiceProvider().GetRequiredService<UserManager<ApplicationUser>>();
            var _roleManager = services.BuildServiceProvider().GetRequiredService<RoleManager<IdentityRole>>();
            var _db = services.BuildServiceProvider().GetRequiredService<ApplicationDbContext>();
            string[] roleNames = { "Admin", "General"};
            var role = new IdentityRole();
            foreach (var roleName in roleNames)
            {
                //var roleExist = await _roleManager.RoleExistsAsync(roleName);
                //if (!roleExist)
                //{
                //    await _roleManager.CreateAsync(new IdentityRole(roleName));
                //}

                var roleExist = await _db.Roles.FirstOrDefaultAsync(x => x.Name.Equals(roleName));
                if (roleExist is null)
                {
                    var newRole = new IdentityRole(roleName);
                    newRole.NormalizedName = roleName.ToUpper();
                    newRole.ConcurrencyStamp = Guid.NewGuid().ToString("N");
                    await _db.Roles.AddAsync(newRole);
                    if (newRole.Name is "Admin")
                    {
                        role = newRole;
                    }
                }
                else if (roleExist.Name is "Admin")
                {
                    role = roleExist;
                }

            }
            ApplicationUser SuperAdmin = await _userManager.FindByNameAsync("admin");
            if (SuperAdmin == null)
            {
                SuperAdmin = new ApplicationUser()
                {
                    UserName = "admin",
                    Email = "md.nasiruddin2510@gmail.com",
                    FullName = "Md. Nasir Uddin",
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true,
                    NormalizedEmail = "md.nasiruddin2510@gmail.com",
                    NormalizedUserName = "ADMIN",
                    IsActive = true,
                };
                var result = await _userManager.CreateAsync(SuperAdmin, "123456");
            }

            var userRoleEsixt = await _db.UserRoles.FirstOrDefaultAsync(x => x.UserId.Equals(SuperAdmin.Id) && x.RoleId == role.Id);
            if (userRoleEsixt is null)
            {
                var userRole = new IdentityUserRole<string>
                {
                    UserId = SuperAdmin.Id,
                    RoleId = role.Id,
                };
                await _db.UserRoles.AddAsync(userRole);
                _db.SaveChanges();
            }

            //await _userManager.AddToRoleAsync(SuperAdmin, "Admin");
            //await _userManager.AddToRoleAsync(GeneralUser, "General");
            await AddPermissionToRoleAsync(role, _db);
        }
        public static async void CreateGeneralUser(this IServiceCollection services)
        {
            var _userManager = services.BuildServiceProvider().GetRequiredService<UserManager<ApplicationUser>>();
            var _roleManager = services.BuildServiceProvider().GetRequiredService<RoleManager<IdentityRole>>();
            var _db = services.BuildServiceProvider().GetRequiredService<ApplicationDbContext>();
            string[] roleNames = { "General" };
            var role = new IdentityRole();
            foreach (var roleName in roleNames)
            {
                //var roleExist = await _roleManager.RoleExistsAsync(roleName);
                //if (!roleExist)
                //{
                //    await _roleManager.CreateAsync(new IdentityRole(roleName));
                //}

                var roleExist = await _db.Roles.FirstOrDefaultAsync(x => x.Name.Equals(roleName));
                if (roleExist is null)
                {
                    var newRole = new IdentityRole(roleName);
                    newRole.NormalizedName = roleName.ToUpper();
                    newRole.ConcurrencyStamp = Guid.NewGuid().ToString("N");
                    await _db.Roles.AddAsync(newRole);
                    if (newRole.Name is "General")
                    {
                        role = newRole;
                    }
                }
                else if (roleExist.Name is "Admin")
                {
                    role = roleExist;
                }

            }
            ApplicationUser GeneralUser = await _userManager.FindByNameAsync("nasir");

            if (GeneralUser == null)
            {
                GeneralUser = new ApplicationUser()
                {
                    UserName = "nasir",
                    Email = "md.nasiruddin2510@gmail.com",
                    FullName = "Md. Nasir Uddin",
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true,
                    NormalizedEmail = "md.nasiruddin2510@gmail.com",
                    NormalizedUserName = "NASIR",
                    IsActive = true,
                };
                var result = await _userManager.CreateAsync(GeneralUser, "123456");
            }

            var userRoleEsixt = await _db.UserRoles.FirstOrDefaultAsync(x => x.UserId.Equals(GeneralUser.Id) && x.RoleId == role.Id);
            if (userRoleEsixt is null)
            {
                var userRole = new IdentityUserRole<string>
                {
                    UserId = GeneralUser.Id,
                    RoleId = role.Id,
                };
                await _db.UserRoles.AddAsync(userRole);
                _db.SaveChanges();
            }

            //await _userManager.AddToRoleAsync(SuperAdmin, "Admin");
            //await _userManager.AddToRoleAsync(GeneralUser, "General");
            await AddPermissionToRoleAsync(role, _db);
        }
        static async Task AddPermissionToRoleAsync(IdentityRole role, ApplicationDbContext _db)
        {
            var permittedClaimList = await _db.RoleClaims.Where(x => x.RoleId.Equals(role.Id)).ToListAsync();
            var claimList = ClaimHelper.GetAllClaimList();
            var identityRoleClaims = new List<IdentityRoleClaim<string>>();
            foreach (var claim in claimList)
            {
                if (!permittedClaimList.Any(x => x.ClaimType.Equals(claim.Value)))
                {
                    var cl = new Claim(claim.Value, role.Id);
                    var roleClaim = new IdentityRoleClaim<string>
                    {
                        RoleId = role.Id,
                        ClaimType = claim.Value,
                        ClaimValue = role.Id,
                    };
                    roleClaim.ToClaim();
                    identityRoleClaims.Add(roleClaim);
                }
            }
            if (identityRoleClaims.Count > 0)
            {
                await _db.RoleClaims.AddRangeAsync(identityRoleClaims);
                await _db.SaveChangesAsync();
            }
        }
    }
}
