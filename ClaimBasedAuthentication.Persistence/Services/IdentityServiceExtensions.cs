using ClaimBasedAuthentication.Domain.Entities;
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
            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    RequireExpirationTime = false,
                    ClockSkew = TimeSpan.Zero,
                    ValidateLifetime = true,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidAudience = configuration["JwtSettings:Audience"],
                    ValidIssuer = configuration["JwtSettings:Issuer"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JwtSettings:Key"]))
                };
                options.Events = new JwtBearerEvents()
                {
                    OnTokenValidated = async (context) =>
                    {
                        var principal = context.Principal;
                        if (principal != null)
                        {
                            var identity = principal.Identity;
                            if (identity != null)
                            {
                                var name = identity.Name;
                                var path = context.HttpContext.Request.Path.Value;
                                var memCache = context.HttpContext.RequestServices.GetService<IMemoryCache>();
                                if (path != null && path.ToLower().Contains("logout"))
                                {
                                    if (memCache != null)
                                        memCache.Remove(name);
                                }
                                else
                                {
                                    if (memCache == null && name != null)
                                    {
                                        var _userManager = services.BuildServiceProvider().GetRequiredService<UserManager<ApplicationUser>>();
                                        var user = await _userManager.FindByNameAsync(name);
                                        var authClaims = new List<Claim>
                                        {
                                            new Claim(ClaimTypes.Name, user!.UserName!),
                                            new Claim("UserId", user.Id.ToString()),
                                            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                                            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                                        };
                                        var authAdd = new ClaimsIdentity();
                                        authAdd.AddClaims(authClaims);
                                        context.HttpContext.User.AddIdentity(authAdd);
                                        //mem cache
                                        var cacheExpiryOptions = new MemoryCacheEntryOptions
                                        {
                                            AbsoluteExpiration = DateTime.Now.AddDays(300),
                                            Priority = CacheItemPriority.High,
                                            SlidingExpiration = TimeSpan.FromDays(200)
                                        };
                                        memCache.Set(name, authClaims, cacheExpiryOptions);
                                    }
                                    else if (memCache != null && name != null)
                                    {
                                        var authClaims = memCache.Get<List<Claim>>(name);
                                        var authAdd = new ClaimsIdentity();
                                        authAdd.AddClaims(authClaims);
                                        context.HttpContext.User.AddIdentity(authAdd);
                                    }
                                }
                            }
                        }
                    }
                };
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
            string[] roleNames = { "Admin", "General"};
            foreach (var roleName in roleNames)
            {
                var roleExist = await _roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    await _roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }
            ApplicationUser SuperAdmin = await _userManager.FindByNameAsync("admin");
            ApplicationUser GeneralUser = await _userManager.FindByNameAsync("nasir");
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

            if (GeneralUser == null)
            {
                GeneralUser  = new ApplicationUser()
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
            await _userManager.AddToRoleAsync(SuperAdmin, "Admin");
            await _userManager.AddToRoleAsync(GeneralUser, "General");
        }
    }
}
