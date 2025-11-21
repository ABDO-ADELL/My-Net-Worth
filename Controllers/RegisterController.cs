using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies; // ✅ Add this
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PRISM.Models.Authmodels;
using PRISM.Services;
using System.Security.Claims;

namespace PRISM.Controllers
{
    [AllowAnonymous]
    public class RegisterController : Controller
    {
        private readonly IAuthService _authService;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;
        private readonly ILogger<RegisterController> _logger;

        public RegisterController(
            IAuthService authService,
            SignInManager<AppUser> signInManager,
            UserManager<AppUser> userManager,
            ILogger<RegisterController> logger)
        {
            _authService = authService;
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: Register/Register
        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            return View();
        }

        // GET: Register/Login
        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            return View();
        }

        // POST: Register
        [HttpPost]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, message = "Invalid data provided" });
            }

            try
            {
                var result = await _authService.RegisterAsync(model);

                if (!result.IsAuthenticated)
                {
                    return BadRequest(new { success = false, message = result.Message });
                }

                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null)
                {
                    // ✅ Create claims including roles
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, user.Id),
                        new Claim(ClaimTypes.Name, user.UserName ?? user.Email),
                        new Claim(ClaimTypes.Email, user.Email),
                        new Claim("FullName", $"{user.FirstName} {user.LastName}")
                    };

                    // ✅ Add roles as claims
                    var roles = await _userManager.GetRolesAsync(user);
                    claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

                    var claimsIdentity = new ClaimsIdentity(
                        claims,
                        CookieAuthenticationDefaults.AuthenticationScheme); // ✅ Use this constant

                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = true,
                        ExpiresUtc = DateTimeOffset.UtcNow.AddDays(30),
                        AllowRefresh = true
                    };

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme, // ✅ Use this constant
                        new ClaimsPrincipal(claimsIdentity),
                        authProperties);

                    _logger.LogInformation("User {Email} registered and signed in successfully", model.Email);
                }

                return Ok(new
                {
                    success = true,
                    message = "Registration successful!",
                    redirectUrl = Url.Action("Index", "Dashboard")
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration");
                return StatusCode(500, new { success = false, message = "An error occurred during registration" });
            }
        }

        // POST: Login
        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, message = "Invalid email or password" });
            }

            try
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    return Unauthorized(new { success = false, message = "Invalid email or password" });
                }

                var result = await _signInManager.PasswordSignInAsync(
                    user,
                    model.Password,
                    isPersistent: true,
                    lockoutOnFailure: true
                );

                if (result.Succeeded)
                {
                    _logger.LogInformation("User {Email} logged in successfully", model.Email);
                    return Ok(new
                    {
                        success = true,
                        message = "Login successful!",
                        redirectUrl = Url.Action("Index", "Dashboard")
                    });
                }

                if (result.IsLockedOut)
                {
                    _logger.LogWarning("User {Email} account locked out", model.Email);
                    return Unauthorized(new { success = false, message = "Account locked due to too many failed attempts. Please try again later." });
                }

                return Unauthorized(new { success = false, message = "Invalid email or password" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login");
                return StatusCode(500, new { success = false, message = "An error occurred during login" });
            }
        }
    }
}