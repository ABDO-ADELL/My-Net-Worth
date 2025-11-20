using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PRISM.Helpers;
using PRISM.Models.Authmodels;
using PRISM.Services;


namespace PRISM
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // builder.Services.AddControllersWithViews();


            configurationServices(builder.Services, builder.Configuration);


            void configurationServices(IServiceCollection services, IConfiguration configuration)
            {
                services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));



                services.AddScoped<IAuthService, AuthService>();


                //***
                services.Configure<JWT>(configuration.GetSection("JWT"));

                services.AddIdentity<AppUser, IdentityRole>(options =>
                {
                    // Password settings
                    options.Password.RequireDigit = true;
                    options.Password.RequireLowercase = true;
                    options.Password.RequireUppercase = false;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequiredLength = 6;

                    // Lockout settings
                    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                    options.Lockout.MaxFailedAccessAttempts = 5;
                    options.Lockout.AllowedForNewUsers = true;

                    // User settings
                    options.User.RequireUniqueEmail = true;
                    options.SignIn.RequireConfirmedEmail = false;
                })
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();

                // Configure cookie authentication for MVC
                services.ConfigureApplicationCookie(options =>
                {
                    options.LoginPath = "/Register/Login";
                    options.LogoutPath = "/Account/Logout";
                    options.AccessDeniedPath = "/Account/AccessDenied";
                    options.ExpireTimeSpan = TimeSpan.FromDays(30);
                    options.SlidingExpiration = true;
                    options.Cookie.HttpOnly = true;
                    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                });



            }




            builder.Services.AddControllersWithViews();
            builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = true;
    });

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapStaticAssets();
            app.MapControllerRoute(
                name: "default",
<<<<<<< HEAD
               pattern: "{controller=Register}/{action=Login}/{id?}")
               .WithStaticAssets();
           // app.MapControllerRoute(
   //name: "default",
   //pattern: "{controller=Home}/{action=Index}/{id?}");
            //app.MapControllers()
            //.WithStaticAssets();
=======
                pattern: "{controller=Dashboard}/{action=index}/{id?}")
                .WithStaticAssets();
            app.MapControllers();

>>>>>>> ba28e1e28b2ab9ac859f69cab00ca586fa874075
            app.Run();

        }
    }
}
