using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Identity;

namespace where_we_go.Config
{
    public static class AuthenticationConfig
    {
        public static IServiceCollection AddAuthenticationConfig(this IServiceCollection services)
        {
            var googleClientId = GlobalConfig.GetRequiredEnv(GlobalConfig.GoogleClientId);
            var googleClientSecret = GlobalConfig.GetRequiredEnv(GlobalConfig.GoogleClientSecret);

            // services.AddIdentity<Models.User, IdentityRole>()
            //     .AddEntityFrameworkStores<Database.AppDbContext>().AddDefaultTokenProviders();

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