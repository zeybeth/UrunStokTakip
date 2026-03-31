using Data.Abstract;
using Entity;
using Microsoft.AspNetCore.Mvc;

namespace UrunStokTakip.Areas.Admin.Controllers
{
    [Area("Admin")]
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

            // Satış tablon boş bile olsa hata vermemesi için Count() kullanıyoruz
            ViewBag.SaleCount = _uow.GetRepository<Sale>().GetAll().Count();

            return View();
        }
    }
}