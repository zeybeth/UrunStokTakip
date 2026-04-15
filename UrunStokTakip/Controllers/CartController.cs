using Data.Abstract; 
using Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace UrunStokTakip.Controllers
{
    [Authorize] // Sadece giriş yapanlar sepete ürün ekleyebilir
    public class CartController : Controller
    {
        private readonly IUnitOfWork _uow;
        private readonly UserManager<IdentityUser> _userManager;

        public CartController(IUnitOfWork uow, UserManager<IdentityUser> userManager)
        {
            _uow = uow;
            _userManager = userManager;
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(Guid productId, int quantity = 1)
        {
            // O an giriş yapmış olan müşteriyi bulmak
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge(); // Giriş yapmamışsa Login sayfasına atar

            // 
            // Eğer bir müşteri F12'den (Inspect) kodu kurcalayıp adeti 0 veya eksi (-) yaparsa diye gerekliymiş
            if (quantity <= 0)
            {
                TempData["ErrorMessage"] = "Geçersiz bir miktar girdiniz! Lütfen çakallık yapmayalım :)";
                return RedirectToAction("Index", "Home");
            }
            //  Bu müşteri bu ürünü daha önce sepete eklemiş mi?
            var existingCartItem = _uow.Carts.GetAll()
                .FirstOrDefault(c => c.ProductId == productId && c.UserId == user.Id);

            if (existingCartItem != null)
            {
                //Ürün zaten sepette var, sadece adetini artır ve güncelle
                existingCartItem.Quantity += quantity;
                existingCartItem.UpdateDate = DateTime.Now;
                _uow.Carts.Update(existingCartItem);
            }
            else
            {
                // Ürün sepette yok, ilk defa ekleniyor
                var newCart = new Cart
                {
                    ProductId = productId,
                    UserId = user.Id,
                    Quantity = quantity,
                    CreatedDate = DateTime.Now,
                    IsActive = true
                };
                await _uow.Carts.AddAsync(newCart);
            }

            // Veritabanına kaydet
            await _uow.SaveAsync();

            //  (Vitrine) geri gönder
            return RedirectToAction("Index", "Home");
        }
        public async Task<IActionResult> Index()
        {
            // 1. O an giriş yapmış müşteriyi bul
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            // 2. Sadece bu müşteriye ait olan sepet öğelerini bul 
            // (Include ile Product'ı da çekiyoruz ki ürünün adını ve resmini gösterebilelim)
            var cartItems = await _uow.Carts.GetAll()
                .Include(c => c.Product)
                .Where(c => c.UserId == user.Id)
                .ToListAsync();

            // 3. Verileri View'a (HTML'e) gönder
            return View(cartItems);
        }

        //  Ürün Adetini Artırma
        [HttpPost]
        public async Task<IActionResult> Increase(Guid productId)
        {
            var user = await _userManager.GetUserAsync(User);
            var cartItem = _uow.Carts.GetAll().FirstOrDefault(c => c.ProductId == productId && c.UserId == user.Id);

            if (cartItem != null)
            {
                cartItem.Quantity++;
                _uow.Carts.Update(cartItem);
                await _uow.SaveAsync();
            }
            return RedirectToAction("Index");
        }

        // Ürün Adetini Azaltma
        [HttpPost]
        public async Task<IActionResult> Decrease(Guid productId)
        {
            var user = await _userManager.GetUserAsync(User);
            var cartItem = _uow.Carts.GetAll().FirstOrDefault(c => c.ProductId == productId && c.UserId == user.Id);

            if (cartItem != null)
            {
                if (cartItem.Quantity > 1)
                {
                    cartItem.Quantity--;
                    _uow.Carts.Update(cartItem);
                }
                else
                {
                    // Eğer adet 1 ise ve eksiye basarsa ürünü tamamen sileriz
                    _uow.Carts.Delete(cartItem);
                }
                await _uow.SaveAsync();
            }
            return RedirectToAction("Index");
        }

        // Ürünü Sepetten Tamamen Silme
        [HttpPost]
        public async Task<IActionResult> Remove(Guid productId)
        {
            var user = await _userManager.GetUserAsync(User);
            var cartItem = _uow.Carts.GetAll().FirstOrDefault(c => c.ProductId == productId && c.UserId == user.Id);

            if (cartItem != null)
            {
                _uow.Carts.Delete(cartItem);
                await _uow.SaveAsync();
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Checkout()
        {
            //  Müşteriyi bul
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            // Müşterinin sepetindeki tüm ürünleri çek
            var cartItems = await _uow.Carts.GetAll()
                .Include(c => c.Product)
                .Where(c => c.UserId == user.Id)
                .ToListAsync();

            if (!cartItems.Any()) return RedirectToAction("Index"); // Sepet boşsa geri dön

            //  Sepetteki her ürünü "Satış" (Sale) tablosuna aktar
            foreach (var item in cartItems)
            {
                // Eğer müşterinin sepetindeki adet, veritabanındaki stoktan fazlaysa satışı durdur!!.
                if (item.Product.Stock < item.Quantity)
                {
                    // Müşteriye hata mesajı göster ve sepete geri yolla
                    TempData["ErrorMessage"] = $"Üzgünüz, '{item.Product.Name}' ürününden stoklarımızda sadece {item.Product.Stock} adet kalmıştır!";
                    return RedirectToAction("Index");
                }

                var newSale = new Sale
                {
                    ProductId = item.ProductId,
                    UserId = item.UserId,
                    Quantity = item.Quantity,
                    Price = item.Product.Price, 
                    CreatedDate = DateTime.Now,
                    IsActive = true
                };

                await _uow.Sales.AddAsync(newSale); // Satışa ekle
                _uow.Carts.Delete(item);            // Sepetten sil
            }

            //  Veritabanını güncelle
            await _uow.SaveAsync();
            return RedirectToAction("Success");
        }
        public IActionResult Success()
        {
            return View();
        }

    }
}