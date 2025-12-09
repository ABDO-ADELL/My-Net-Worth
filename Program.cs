using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using PRISM.DataAccess;
using PRISM.Helpers;
using PRISM.Models.Authmodels;
using PRISM.Services;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Remove JWT, use only Cookie authentication for MVC
        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

        // Keep AuthService for password hashing logic only
        builder.Services.AddScoped<IAuthService, AuthService>();
        builder.Services.Configure<JWT>(builder.Configuration.GetSection("JWT"));

        // Identity Configuration
        builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredLength = 6;

            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers = true;

            options.User.RequireUniqueEmail = true;
            options.SignIn.RequireConfirmedEmail = false;
        })
        .AddEntityFrameworkStores<AppDbContext>()
        .AddDefaultTokenProviders();

        //  Cookie Authentication
        builder.Services.ConfigureApplicationCookie(options =>
        {
            options.LoginPath = "/Register/Login";
            options.LogoutPath = "/Account/Logout";
            options.AccessDeniedPath = "/Account/AccessDenied";
            options.ExpireTimeSpan = TimeSpan.FromDays(30);
            options.SlidingExpiration = true;
            options.Cookie.HttpOnly = true;
           // options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
          //  options.Cookie.SameSite = SameSiteMode.Strict; 
            options.Cookie.Name = "PRISM.Auth";
            options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
            options.Cookie.SameSite = SameSiteMode.Lax;
            if (!builder.Environment.IsDevelopment())
            {
                options.Cookie.Domain = null; // Let the browser set it automatically
            }


        });
        // Handle proxy headers for RunASP
        builder.Services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            options.KnownNetworks.Clear();
            options.KnownProxies.Clear();
        });
        // Rate Limiting
        builder.Services.AddRateLimiter(options => {    
        
            options.AddFixedWindowLimiter("Fixed", opt => { 
                opt.PermitLimit = 100; 
                opt.Window = TimeSpan.FromMinutes(1); 
                opt.QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst; 
                opt.QueueLimit = 50; 
            });
        });


        builder.Services.AddControllersWithViews();
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<ViewBagPopulationService>();
        builder.Services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.ReferenceHandler =
                    System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
                options.JsonSerializerOptions.WriteIndented = true;
            });

        var app = builder.Build();

        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }
        // Enable forwarded headers
        app.UseForwardedHeaders();
        app.UseHttpsRedirection();
        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapStaticAssets();
        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Register}/{action=Login}/{id?}")
            .WithStaticAssets();

        app.MapControllers();

        app.Run();
    }
}