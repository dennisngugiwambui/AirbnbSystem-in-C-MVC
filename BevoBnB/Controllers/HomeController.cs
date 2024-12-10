using System.Diagnostics;
using BevoBnB.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using BevoBnB.ViewModels;

namespace BevoBnB.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<Users> _userManager;
        private readonly SignInManager<Users> _signInManager;

        public HomeController(
            ILogger<HomeController> logger,
            UserManager<Users> userManager,
            SignInManager<Users> signInManager)
        {
            _logger = logger;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<IActionResult> Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                return await RedirectToUserDashboard();
            }
            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }

        public IActionResult Properties()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Login()
        {
            if (User.Identity.IsAuthenticated)
            {
                return await RedirectToUserDashboard();
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);

                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return View(model);
                }

                if (!user.HireStatus)
                {
                    ModelState.AddModelError(string.Empty, "Your account has been deactivated.");
                    return View(model);
                }

                var result = await _signInManager.PasswordSignInAsync(
                    user.UserName,
                    model.Password,
                    model.RememberMe,
                    lockoutOnFailure: true);

                if (result.Succeeded)
                {
                    _logger.LogInformation($"User {user.UserName} logged in successfully");
                    var userType = user.UserType;
                    return userType switch
                    {
                        UserType.Customer => RedirectToAction("Index", "Customer"),
                        UserType.Host => RedirectToAction("Index", "Host"),
                        UserType.Admin => RedirectToAction("Index", "Admin"),
                        _ => RedirectToAction("Index", "Home")
                    };
                }

                if (result.IsLockedOut)
                {
                    _logger.LogWarning($"User {user.UserName} account locked out");
                    return View("Lockout");
                }

                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Register()
        {
            if (User.Identity.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(User);
                return user?.UserType switch
                {
                    UserType.Customer => RedirectToAction("Index", "Customer"),
                    UserType.Host => RedirectToAction("Index", "Host"),
                    UserType.Admin => RedirectToAction("Index", "Admin"),
                    _ => RedirectToAction("Index", "Home")
                };
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Validate age
                var birthDate = model.Birthday;
                var today = DateTime.Today;
                var age = today.Year - birthDate.Year;
                if (birthDate > today.AddYears(-age)) age--;

                if (age < 18)
                {
                    ModelState.AddModelError("Birthday", "You must be at least 18 years old to register.");
                    return View(model);
                }

                var existingUser = await _userManager.FindByEmailAsync(model.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Email", "This email is already registered.");
                    return View(model);
                }

                var normalizedUsername = $"{model.FirstName}{model.LastName}".ToUpper();

                var user = new Users
                {
                    UserName = normalizedUsername,
                    NormalizedUserName = normalizedUsername,
                    Email = model.Email,
                    NormalizedEmail = model.Email.ToUpper(),
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Birthday = model.Birthday,
                    Phone = model.Phone,
                    Address = model.Address,
                    UserType = model.UserType,
                    HireStatus = true,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation($"User {normalizedUsername} created a new account.");
                    await _userManager.AddToRoleAsync(user, model.UserType.ToString());
                    await _signInManager.SignInAsync(user, isPersistent: false);

                    return user.UserType switch
                    {
                        UserType.Customer => RedirectToAction("Index", "Customer"),
                        UserType.Host => RedirectToAction("Index", "Host"),
                        UserType.Admin => RedirectToAction("Index", "Admin"),
                        _ => RedirectToAction("Index", "Home")
                    };
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");
            return RedirectToAction(nameof(Index));
        }

        public IActionResult ForgotPassword()
        {
            return View();
        }

        private async Task<IActionResult> RedirectToUserDashboard()
        {
            if (User.Identity.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    return user.UserType switch
                    {
                        UserType.Customer => RedirectToAction("Index", "Customer"),
                        UserType.Host => RedirectToAction("Index", "Host"),
                        UserType.Admin => RedirectToAction("Index", "Admin"),
                        _ => RedirectToAction("Index", "Home")
                    };
                }
            }
            return RedirectToAction("Index", "Home");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}