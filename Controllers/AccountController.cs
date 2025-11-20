using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PRISM.Models.Authmodels;
using System.Security.Claims;
using PRISM.Dto;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace PRISM.Controllers
{
    //[Authorize]
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            AppDbContext context,
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            ILogger<AccountController> logger)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        #region Profile
        /// <summary>
        /// Display user profile information
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                _logger.LogWarning("User not found when accessing profile");
                return RedirectToAction("Register", "Register");
            }

            return View(user);
        }
        #endregion

        #region Edit Name
        /// <summary>
        /// Display edit name form
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> EditName()
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
                return RedirectToAction("Register", "Register");

            var model = new UpdateNameViewModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName
            };

            return View(model);
        }

        /// <summary>
        /// Update user's first and last name
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditName(UpdateNameViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Please correct the errors and try again.";
                return View(model);
            }

            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("Register", "Register");
            }

            try
            {
                user.FirstName = model.FirstName.Trim();
                user.LastName = model.LastName.Trim();
                user.UpdatedAt = DateTime.UtcNow;

                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User {UserId} updated name successfully", user.Id);
                    TempData["SuccessMessage"] = "Name updated successfully!";
                    return RedirectToAction(nameof(Profile));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                TempData["ErrorMessage"] = "Failed to update name.";
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating name for user {UserId}", user.Id);
                TempData["ErrorMessage"] = "An error occurred while updating your name.";
                return View(model);
            }
        }
        #endregion

        #region Change Password
        /// <summary>
        /// Display change password form
        /// </summary>
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        /// <summary>
        /// Change user password
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordDto model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Please correct the errors and try again.";
                return View(model);
            }

            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("Register", "Register");
            }

            try
            {
                var result = await _userManager.ChangePasswordAsync(
                    user,
                    model.CurrentPassword,
                    model.NewPassword
                );

                if (result.Succeeded)
                {
                    _logger.LogInformation("User {UserId} changed password successfully", user.Id);

                    // Refresh sign-in to update security stamp
                    await _signInManager.RefreshSignInAsync(user);

                    TempData["SuccessMessage"] = "Password changed successfully!";
                    return RedirectToAction(nameof(Profile));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                TempData["ErrorMessage"] = "Failed to change password. Please check your current password.";
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password for user {UserId}", user.Id);
                TempData["ErrorMessage"] = "An error occurred while changing your password.";
                return View(model);
            }
        }
        #endregion

        #region Change Email
        /// <summary>
        /// Display change email form
        /// </summary>
        [HttpGet]
        public IActionResult ChangeEmail()
        {
            return View();
        }

        /// <summary>
        /// Change user email address
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeEmail(ChangeEmailDto model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Please correct the errors and try again.";
                return View(model);
            }

            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("Register", "Register");
            }

            try
            {
                // Verify current password
                var passwordValid = await _userManager.CheckPasswordAsync(user, model.Password);
                if (!passwordValid)
                {
                    ModelState.AddModelError(string.Empty, "Invalid password.");
                    TempData["ErrorMessage"] = "The password you entered is incorrect.";
                    return View(model);
                }

                // Check if email is already taken
                var existingUser = await _userManager.FindByEmailAsync(model.NewEmail);
                if (existingUser != null && existingUser.Id != user.Id)
                {
                    ModelState.AddModelError("NewEmail", "This email address is already in use.");
                    TempData["ErrorMessage"] = "This email address is already registered.";
                    return View(model);
                }

                // Generate email change token
                var token = await _userManager.GenerateChangeEmailTokenAsync(user, model.NewEmail);

                // Change email
                var result = await _userManager.ChangeEmailAsync(user, model.NewEmail, token);

                if (result.Succeeded)
                {
                    // Update username to match new email
                    user.UserName = model.NewEmail;
                    user.UpdatedAt = DateTime.UtcNow;
                    await _userManager.UpdateAsync(user);

                    _logger.LogInformation("User {UserId} changed email successfully to {NewEmail}", user.Id, model.NewEmail);

                    // Refresh sign-in
                    await _signInManager.RefreshSignInAsync(user);

                    TempData["SuccessMessage"] = "Email changed successfully!";
                    return RedirectToAction(nameof(Profile));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                TempData["ErrorMessage"] = "Failed to change email.";
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing email for user {UserId}", user.Id);
                TempData["ErrorMessage"] = "An error occurred while changing your email.";
                return View(model);
            }
        }
        #endregion

        #region Logout
        /// <summary>
        /// Logout user
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out");

            TempData["SuccessMessage"] = "You have been logged out successfully.";
            return RedirectToAction("Register", "Register");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogoutPost()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out");

            return RedirectToAction("Register", "Register");
        }
        #endregion

        #region Helper Methods
        /// <summary>
        /// Get the current logged-in user
        /// </summary>
        private async Task<AppUser?> GetCurrentUserAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return null;

            return await _userManager.FindByIdAsync(userId);
        }
        #endregion
    }

    #region View Models
    public class UpdateNameViewModel
    {
        [Required(ErrorMessage = "First name is required")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "First name must be between 2 and 50 characters")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Last name must be between 2 and 50 characters")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }
    }
    #endregion
}