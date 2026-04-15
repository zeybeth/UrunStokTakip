using Entity; 
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using UrunStokTakip.Models;
using Data;

namespace UrunStokTakip.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        // ANA SAYFA VÝTRÝNÝ
        public async Task<IActionResult> Index(Guid? categoryId)
        {
            var categories = await _context.Categories.ToListAsync();
            var productsQuery = _context.Products.Include(p => p.Category).AsQueryable();

            if (categoryId.HasValue)
            {
                productsQuery = productsQuery.Where(p => p.CategoryId == categoryId.Value);
            }

            var viewModel = new HomeViewModel
            {
                Categories = categories,
                Products = await productsQuery.ToListAsync(),
                SelectedCategoryId = categoryId
            };

            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}