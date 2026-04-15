using Data.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace UrunStokTakip.Controllers
{
    // Sadece giriş yapmış müşteriler kendi siparişlerini görebilir
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork _uow;
        private readonly UserManager<IdentityUser> _userManager;

        public OrderController(IUnitOfWork uow, UserManager<IdentityUser> userManager)
        {
            _uow = uow;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            // O an giriş yapmış müşteriyi bul
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Sadece bu müşterinin alışveriş geçmişini çek
            var myPurchases = await _uow.Sales.GetAll()
                .Include(s => s.Product)
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.CreatedDate)
                .ToListAsync();

            // Veriyi View'a gönder
            return View(myPurchases);
        }
    }
}