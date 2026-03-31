using Data.Abstract;
using Entity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
namespace UrunStokTakip.Areas.Admin.Controllers;
[Area("Admin")]
public class SaleController : Controller
{
    private readonly IUnitOfWork _uow;

    public SaleController(IUnitOfWork uow)
    {
        _uow = uow;
    }

    // Yapılan Satışları Listeleme
    public async Task<IActionResult> Index()
    {
        var sales = await _uow.GetRepository<Sale>()
                              .GetAll()
                              .Include(x => x.Product)
                              .Include(x => x.User)
                              .OrderByDescending(x => x.CreatedDate)
                              .ToListAsync();
        return View(sales);
    }

    // Satış Yapma İşlemi
    [HttpPost]
    public async Task<IActionResult> MakeSale(Guid productId, int quantity)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var productRepo = _uow.GetRepository<Product>();
        var product = await productRepo.GetByIdAsync(productId);

        if (product != null && product.Stock >= quantity)
        {
            // Stoktan düş
            product.Stock -= quantity;
            productRepo.Update(product);

            //Satış kaydı oluştur
            var sale = new Sale
            {
                ProductId = productId,
                Quantity = quantity,
                Price = product.Price * quantity,
                UserId = userId ?? "Anonim" //
            };

            await _uow.GetRepository<Sale>().AddAsync(sale);

            // Unit of Work ile tek seferde kaydediliyor
            await _uow.SaveAsync();

            return RedirectToAction(nameof(Index));
        }

        
        return BadRequest("Stok yetersiz veya ürün bulunamadı.");
    }
}