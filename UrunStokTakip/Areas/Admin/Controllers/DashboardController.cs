using Data.Abstract;
using Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace UrunStokTakip.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly IUnitOfWork _uow;

        public DashboardController(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public IActionResult Index()
        {
            // Sayıları çekip ViewBag ile Index'e gönderiyoruz
            ViewBag.ProductCount = _uow.GetRepository<Product>().GetAll().Count();
            ViewBag.CategoryCount = _uow.GetRepository<Category>().GetAll().Count();

            // Satış tablom boş olsa da hata almamak için
            ViewBag.SaleCount = _uow.GetRepository<Sale>().GetAll().Count();

            return View();
        }
    }
}