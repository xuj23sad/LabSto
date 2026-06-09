using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Text;
using LabSto.Data;
using LabSto.Models.Entities;
using LabSto.Models.ViewModels;
using LabSto.Filters;

namespace LabSto.Controllers
{
    public class AccountController(LabStoContext context, ILogger<AccountController> logger) : Controller
    {
        private readonly LabStoContext _context = context;
        private readonly ILogger<AccountController> _logger = logger;

        [TypeFilter(typeof(CustomActionFilterAttribute))]
        public IActionResult Index()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }
            return RedirectToAction("Login");
        }

        [TypeFilter(typeof(CustomActionFilterAttribute))]
        public IActionResult Login(string returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [TypeFilter(typeof(CustomActionFilterAttribute))]
        public async Task<IActionResult> Login(LoginVM model, string returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _context.User
                .FirstOrDefaultAsync(u => u.UserName == model.UserName);

            if (user == null || !VerifyPassword(model.Password, user.PasswordHash))
            {
                ModelState.AddModelError("", "用户名或密码错误");
                return View(model);
            }

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, $"{user.Id}"),
                new(ClaimTypes.Name, user.UserName),
                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.Role, user.Role)
            };

            var claimsIdentity = new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = model.RememberMe,
                ExpiresUtc = model.RememberMe
                    ? DateTimeOffset.UtcNow.AddDays(7)
                    : DateTimeOffset.UtcNow.AddHours(12)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            HttpContext.Session.SetString("UserName", user.UserName);
            HttpContext.Session.SetString("Role", user.Role);
            HttpContext.Session.SetInt32("UserId", user.Id);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        [TypeFilter(typeof(CustomActionFilterAttribute))]
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [TypeFilter(typeof(CustomActionFilterAttribute))]
        public async Task<IActionResult> Register(RegisterVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (await _context.User.AnyAsync(u => u.UserName == model.UserName))
            {
                ModelState.AddModelError("UserName", "用户名已存在");
                return View(model);
            }

            if (await _context.User.AnyAsync(u => u.Email == model.Email))
            {
                ModelState.AddModelError("Email", "邮箱已被注册");
                return View(model);
            }

            var user = new User
            {
                UserName = model.UserName,
                Email = model.Email,
                PasswordHash = HashPassword(model.Password),
                Role = "普通用户",
                CreateTime = DateTime.Now
            };

            _context.User.Add(user);
            await _context.SaveChangesAsync();

            TempData["Success"] = "注册成功，请登录";
            return RedirectToAction("Login");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [TypeFilter(typeof(CustomActionFilterAttribute))]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }

        [TypeFilter(typeof(CustomActionFilterAttribute))]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [TypeFilter(typeof(CustomActionFilterAttribute))]
        public IActionResult Profile()
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                return RedirectToAction("Login");
            }
            return View();
        }

        private static string HashPassword(string password)
        {
            var bytes = System.Security.Cryptography.SHA256.HashData(Encoding.UTF8.GetBytes(password + "LabSto_Salt_2026"));
            return Convert.ToBase64String(bytes);
        }

        private static bool VerifyPassword(string password, string hash)
        {
            return HashPassword(password) == hash;
        }
    }
}
