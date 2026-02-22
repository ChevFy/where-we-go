using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Identity;
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
                options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
            })
            .AddCookie(options =>
            {
                options.Cookie.HttpOnly = true;
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                options.LoginPath = "/auth/login";
                options.LogoutPath = "/auth/logout";
                options.AccessDeniedPath = "/";
                
                // Validate cookie on every request to check if user is banned
                options.Events = new CookieAuthenticationEvents
                {
                    OnValidatePrincipal = async context =>
                    {
                        var userManager = context.HttpContext.RequestServices.GetRequiredService<UserManager<User>>();
                        var userId = context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                        
                        if (!string.IsNullOrEmpty(userId))
                        {
                            var user = await userManager.FindByIdAsync(userId);
                            if (user != null && user.IsBanned && user.BanExpiresAt > DateTime.UtcNow)
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
