using LabSto.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using LabSto.Filters;

namespace LabSto.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        [TypeFilter(typeof(CustomActionFilterAttribute))]
        public IActionResult Index()
        {
            _logger.LogInformation("显示Index视图");
            _logger.LogWarning("Index视图警告信息");
            _logger.LogError("Index视图错误信息");
            return View();
        }

        [TypeFilter(typeof(CustomActionFilterAttribute))]
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [TypeFilter(typeof(CustomActionFilterAttribute))]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
