using Data.Abstract;
using Entity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace UrunStokTakip.Controllers;

public class CartController : Controller
{
    private readonly IUnitOfWork _uow;

    public CartController(IUnitOfWork uow)
    {
        _uow = uow;
    }

    //  Sepeti Listeleme
    public async Task<IActionResult> Index()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var cartItems = await _uow.GetRepository<Cart>()
                                  .GetAll()
                                  .Where(x => x.UserId == userId)
                                  .Include(x => x.Product)
                                  .ToListAsync();
        return View(cartItems);
    }

    //  Sepete Ürün Ekleme
    public async Task<IActionResult> AddToCart(Guid productId, int quantity)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var cartRepo = _uow.GetRepository<Cart>();

        // Sepette bu ürün zaten var mı?
        var existingItem = await cartRepo.GetAll()
            .FirstOrDefaultAsync(x => x.ProductId == productId && x.UserId == userId);

        if (existingItem != null)
        {
            existingItem.Quantity += quantity;
            cartRepo.Update(existingItem);
        }
        else
        {
            var cartItem = new Cart
            {
                ProductId = productId,
                Quantity = quantity,
                UserId = userId!
            };
            await cartRepo.AddAsync(cartItem);
        }

        await _uow.SaveAsync();
        return RedirectToAction(nameof(Index));
    }
}