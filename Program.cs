using Microsoft.EntityFrameworkCore;
using dotenv.net;
using where_we_go.Database;
using where_we_go.Config;
using where_we_go.Service;
using Microsoft.AspNetCore.Identity;

namespace where_we_go
{
    public class Program
    {
        public static void Main(string[] args)
        {
            DotEnv.Load();
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            var connectionString = GlobalConfig.GetDBConnectionString();

            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(connectionString));



            builder.Services.AddIdentity<Models.User, IdentityRole>()
                .AddEntityFrameworkStores<AppDbContext>().AddDefaultTokenProviders();

            builder.Services.AddAuthenticationConfig();


            builder.Services.AddRazorPages();

            builder.Services.AddScoped<IUserService, UserService>();

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;

                SeedData.Initialize(services);
            }

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseRouting();
            app.UseHttpsRedirection();

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