using Microsoft.EntityFrameworkCore;
using dotenv.net;
using WhereWeGo.Database;
using WhereWeGo.Config;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;

namespace WhereWeGo
{
    public class Program
    {
        public static void Main(string[] args)
        {
            DotEnv.Load();
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            // Construct database connection string
            var dbHost = Environment.GetEnvironmentVariable(GlobalConfig.DbHost)
                ?? throw new InvalidOperationException("Database host not found in environment variables.");
            var dbPort = Environment.GetEnvironmentVariable(GlobalConfig.DbPort)
                ?? throw new InvalidOperationException("Database port not found in environment variables.");
            var dbName = Environment.GetEnvironmentVariable(GlobalConfig.DbName)
                ?? throw new InvalidOperationException("Database name not found in environment variables.");
            var dbUser = Environment.GetEnvironmentVariable(GlobalConfig.DbUser)
                ?? throw new InvalidOperationException("Database user not found in environment variables.");
            var dbPassword = Environment.GetEnvironmentVariable(GlobalConfig.DbPassword)
                ?? throw new InvalidOperationException("Database password not found in environment variables.");

            var connectionString = $"Host={dbHost};Port={dbPort};Database={dbName};Username={dbUser};Password={dbPassword}";

            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(connectionString));

            var googleClientId = Environment.GetEnvironmentVariable(GlobalConfig.GoogleClientId)
                ?? throw new InvalidOperationException("Google Client ID not found in environment variables.");
            var googleClientSecret = Environment.GetEnvironmentVariable(GlobalConfig.GoogleClientSecret)
                ?? throw new InvalidOperationException("Google Client Secret not found in environment variables.");

            builder.Services
            .AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
            })
            .AddCookie()
            .AddGoogle(options =>
            {
                options.ClientId = googleClientId;
                options.ClientSecret = googleClientSecret;
            });

            builder.Services.AddRazorPages();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();

            // authentication & authorization
            app.UseAuthentication();
            app.UseAuthorization();


            app.MapStaticAssets();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();

            app.Run();
        }
    }
}