using Data;

using Entity;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using UrunStokTakip.Models; 

namespace UrunStokTakip.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            // Ana sayfa açýldýðýnda burasý çalýþýr
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }
    }
}