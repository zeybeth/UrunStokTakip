using Data.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace UrunStokTakip.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork _uow;

        public OrderController(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<IActionResult> Index()
        {
            // Tüm satışları; ürün ve kullanıcı bilgileriyle birlikte çekiyor, (en yeni en üstte olacak şekilde)
            var orders = await _uow.Sales.GetAll()
                .Include(s => s.Product)
                .Include(s => s.User)
                .OrderByDescending(s => s.CreatedDate)
                .ToListAsync();

            return View(orders);
        }
    }
}