using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Playlist.Models;
using Playlist.Repositories;
using Playlist.Services;

namespace Playlist.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserRepository _userRepository;
        private readonly IConfiguration _configuration;
        private readonly FileStorageOptions _storage;
        private readonly AchievementService _achievementService;

        public AccountController(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            UserRepository userRepository,
            IConfiguration configuration,
            FileStorageOptions storage,
            AchievementService achievementService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _userRepository = userRepository;
            _configuration = configuration;
            _storage = storage;
            _achievementService = achievementService;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            var model = new LoginViewModel { ReturnUrl = returnUrl ?? Url.Content("~/") };
            ViewBag.RequiresAuthentication = !string.IsNullOrWhiteSpace(returnUrl)
                && returnUrl != Url.Content("~/");
            if (TempData["ExternalLoginError"] is string externalLoginError)
            {
                ModelState.AddModelError(string.Empty, externalLoginError);
            }
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied(string? returnUrl = null)
        {
            Response.StatusCode = StatusCodes.Status403Forbidden;
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);
            if (result.Succeeded)
            {
                return LocalRedirect(model.ReturnUrl ?? Url.Content("~/"));
            }

            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View(model);
        }

        [HttpGet]
        public IActionResult Register(string? returnUrl = null)
        {
            return View(new RegisterViewModel { ReturnUrl = returnUrl ?? Url.Content("~/") });
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = new AppUser
            {
                UserName = model.Email,
                Email = model.Email,
                OIB = model.OIB,
                JMBG = model.JMBG,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, false);
                return LocalRedirect(model.ReturnUrl ?? Url.Content("~/"));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult ExternalLogin(string provider, string? returnUrl = null)
        {
            if (provider.Equals("Google", StringComparison.OrdinalIgnoreCase))
            {
                var googleClientId = _configuration["Authentication:Google:ClientId"];
                var googleClientSecret = _configuration["Authentication:Google:ClientSecret"];
                if (string.IsNullOrWhiteSpace(googleClientId) || string.IsNullOrWhiteSpace(googleClientSecret))
                {
                    TempData["ExternalLoginError"] = "Google login is not configured.";
                    return RedirectToAction(nameof(Login), new { returnUrl });
                }
            }

            var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return Challenge(properties, provider);
        }

        [HttpGet]
        public async Task<IActionResult> ExternalLoginCallback(string? returnUrl = null, string? remoteError = null)
        {
            if (!string.IsNullOrEmpty(remoteError))
            {
                ModelState.AddModelError(string.Empty, $"Error from external provider: {remoteError}");
                return View(nameof(Login), new LoginViewModel { ReturnUrl = returnUrl ?? Url.Content("~/") });
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return RedirectToAction(nameof(Login));
            }

            var signInResult = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, false);
            if (signInResult.Succeeded)
            {
                return LocalRedirect(returnUrl ?? Url.Content("~/"));
            }

            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            if (email == null)
            {
                ModelState.AddModelError(string.Empty, "Email claim is required from the external provider.");
                return View(nameof(Login), new LoginViewModel { ReturnUrl = returnUrl ?? Url.Content("~/") });
            }

            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser == null)
            {
                existingUser = new AppUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true,
                    OIB = "00000000000",
                    JMBG = "0000000000000"
                };

                var createResult = await _userManager.CreateAsync(existingUser);
                if (!createResult.Succeeded)
                {
                    foreach (var error in createResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return View(nameof(Login), new LoginViewModel { ReturnUrl = returnUrl ?? Url.Content("~/") });
                }
            }

            await _userManager.AddLoginAsync(existingUser, info);
            await _signInManager.SignInAsync(existingUser, false);
            return LocalRedirect(returnUrl ?? Url.Content("~/"));
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                return RedirectToAction(nameof(Login));
            }

            var appUser = await _userManager.GetUserAsync(User);
            if (appUser == null)
            {
                return RedirectToAction(nameof(Login));
            }

            var domainUser = _userRepository.GetByEmail(appUser.Email ?? string.Empty);
            if (domainUser == null)
            {
                domainUser = new User
                {
                    Username = appUser.UserName ?? appUser.Email ?? "Unknown",
                    Email = appUser.Email ?? string.Empty,
                    RegistrationDate = DateTime.UtcNow,
                    IsPremium = false
                };
                _userRepository.Add(domainUser);
            }

            ViewBag.ProfileImageUrl = appUser.ProfileImageUrl;
            ViewBag.ProfileMessage = TempData["ProfileMessage"];
            ViewBag.Achievements = await _achievementService.GetForUserAsync(domainUser.UserId);
            return View(domainUser);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfileName(ProfileNameViewModel model)
        {
            var appUser = await _userManager.GetUserAsync(User);
            if (appUser == null)
            {
                return RedirectToAction(nameof(Login));
            }

            var displayName = model.Username?.Trim() ?? string.Empty;
            if (!ModelState.IsValid || displayName.Length < 2 || displayName.Length > 100)
            {
                TempData["ProfileMessage"] = "Display name must be between 2 and 100 characters.";
                return RedirectToAction(nameof(Profile));
            }

            var domainUser = _userRepository.GetByEmail(appUser.Email ?? string.Empty);
            if (domainUser == null)
            {
                domainUser = new User
                {
                    Username = displayName,
                    Email = appUser.Email ?? string.Empty,
                    RegistrationDate = DateTime.UtcNow,
                    IsPremium = false
                };
                _userRepository.Add(domainUser);
            }
            else if (!_userRepository.UpdateUsername(domainUser.UserId, displayName))
            {
                TempData["ProfileMessage"] = "Display name could not be updated.";
                return RedirectToAction(nameof(Profile));
            }

            TempData["ProfileMessage"] = "Display name updated.";
            return RedirectToAction(nameof(Profile));
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadProfileImage(IFormFile profileImage)
        {
            var appUser = await _userManager.GetUserAsync(User);
            if (appUser == null)
            {
                return RedirectToAction(nameof(Login));
            }

            if (profileImage == null || profileImage.Length == 0)
            {
                TempData["ProfileMessage"] = "Choose an image before uploading.";
                return RedirectToAction(nameof(Profile));
            }

            const long maxFileSize = 5 * 1024 * 1024;
            var allowedTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "image/jpeg",
                "image/png",
                "image/webp"
            };

            var extension = Path.GetExtension(profileImage.FileName).ToLowerInvariant();
            var allowedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                ".jpg",
                ".jpeg",
                ".png",
                ".webp"
            };

            if (profileImage.Length > maxFileSize ||
                !allowedTypes.Contains(profileImage.ContentType) ||
                !allowedExtensions.Contains(extension))
            {
                TempData["ProfileMessage"] = "Use a JPG, PNG or WebP image smaller than 5 MB.";
                return RedirectToAction(nameof(Profile));
            }

            var uploadDirectory = Path.Combine(_storage.UploadsRoot, "profile-images");
            Directory.CreateDirectory(uploadDirectory);

            var oldProfileImageUrl = appUser.ProfileImageUrl;

            var fileName = $"{appUser.Id}_{Guid.NewGuid():N}{extension}";
            var filePath = Path.Combine(uploadDirectory, fileName);

            await using (var stream = System.IO.File.Create(filePath))
            {
                await profileImage.CopyToAsync(stream);
            }

            appUser.ProfileImageUrl = $"/uploads/profile-images/{fileName}";
            var updateResult = await _userManager.UpdateAsync(appUser);
            if (!updateResult.Succeeded)
            {
                System.IO.File.Delete(filePath);
                TempData["ProfileMessage"] = "The profile image could not be saved.";
                return RedirectToAction(nameof(Profile));
            }

            if (!string.IsNullOrWhiteSpace(oldProfileImageUrl))
            {
                var oldFileName = Path.GetFileName(oldProfileImageUrl);
                var oldFilePath = Path.Combine(uploadDirectory, oldFileName);
                if (System.IO.File.Exists(oldFilePath))
                {
                    System.IO.File.Delete(oldFilePath);
                }
            }

            TempData["ProfileMessage"] = "Profile image updated.";
            return RedirectToAction(nameof(Profile));
        }
    }

    public class LoginViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }

        public string? ReturnUrl { get; set; }
    }

    public class ProfileNameViewModel
    {
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string Username { get; set; } = string.Empty;
    }

    public class RegisterViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required]
        [StringLength(11, MinimumLength = 11)]
        [RegularExpression("^[0-9]*$", ErrorMessage = "OIB must contain only digits.")]
        [Display(Name = "OIB")]
        public string OIB { get; set; } = string.Empty;

        [Required]
        [StringLength(13, MinimumLength = 13)]
        [RegularExpression("^[0-9]*$", ErrorMessage = "JMBG must contain only digits.")]
        [Display(Name = "JMBG")]
        public string JMBG { get; set; } = string.Empty;

        public string? ReturnUrl { get; set; }
    }
}
