using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using where_we_go.Models;

namespace where_we_go.Config
{
    public static class AuthenticationConfig
    {
        public static IServiceCollection AddAuthenticationConfig(this IServiceCollection services)
        {
            var googleClientId = GlobalConfig.GetRequiredEnv(GlobalConfig.GoogleClientId);
            var googleClientSecret = GlobalConfig.GetRequiredEnv(GlobalConfig.GoogleClientSecret);

            services
            .AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddCookie(options =>
            {
                options.Cookie.HttpOnly = true;
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                options.LoginPath = "/auth/login";
                options.LogoutPath = "/auth/logout";
                options.AccessDeniedPath = "/";
                
                // Validate cookie on every request to check if user is banned (with caching)
                options.Events = new CookieAuthenticationEvents
                {
                    OnValidatePrincipal = async context =>
                    {
                        var userManager = context.HttpContext.RequestServices.GetRequiredService<UserManager<User>>();
                        var cache = context.HttpContext.RequestServices.GetRequiredService<IMemoryCache>();
                        var userId = context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                        
                        if (!string.IsNullOrEmpty(userId))
                        {
                            var cacheKey = $"user_ban_status_{userId}";
                            
                            // Try to get from cache first
                            if (!cache.TryGetValue(cacheKey, out bool isBanned))
                            {
                                // No cache, query database
                                var user = await userManager.FindByIdAsync(userId);
                                isBanned = user != null && user.IsBanned && user.BanExpiresAt > DateTime.UtcNow;
                                
                                // 2 minutes sliding expiration, 5 minutes absolute expiration
                                var cacheOptions = new MemoryCacheEntryOptions()
                                    .SetSlidingExpiration(TimeSpan.FromMinutes(2))
                                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(5));
                                
                                cache.Set(cacheKey, isBanned, cacheOptions);
                            }
                            
                            if (isBanned)
                            {
                                context.RejectPrincipal();
                                await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                                context.Response.Redirect("/auth/login?banned=true");
                            }
                        }
                    }
                };
            })
            .AddGoogle(options =>
            {
                options.ClientId = googleClientId;
                options.ClientSecret = googleClientSecret;
                options.SaveTokens = true;
            });

            return services;
        }
    }
}
