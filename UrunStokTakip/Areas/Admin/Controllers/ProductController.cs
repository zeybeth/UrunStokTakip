using Data;
using Data.Abstract;
using Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;
using Microsoft.AspNetCore.Hosting;
namespace UrunStokTakip.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class ProductController : Controller
{
    private readonly IUnitOfWork _uow;
    private readonly IWebHostEnvironment _env;

    public ProductController(IUnitOfWork uow, IWebHostEnvironment env)
    {
        _uow = uow;
        _env = env;
    }

    private static readonly string[] AllowedExt = { ".jpg", ".jpeg", ".png", ".webp" };
    private static readonly string[] AllowedMime = { "image/jpeg", "image/png", "image/webp" };
    private const long MaxUploadBytes = 5 * 1024 * 1024; // 5 MB

    private string? ValidateImageFile(IFormFile? file)
    {
        if (file == null || file.Length == 0)
            return null;

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        var mime = (file.ContentType ?? "").ToLowerInvariant();

        if (file.Length > MaxUploadBytes)
            return "Dosya boyutu 5MB'ı aşamaz.";
        if (!AllowedExt.Contains(ext) || !AllowedMime.Contains(mime))
            return "Sadece JPG, PNG veya WebP yükleyebilirsiniz.";

        return null;
    }

    private async Task<string?> SaveImageFile(IFormFile file)
    {
        if (file == null || file.Length == 0) return null;

        // wwwroot/img yolunu oluştur
        string uploadsFolder = Path.Combine(_env.WebRootPath, "img");

        // Klasör yoksa oluştur
        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

        using (var fileStream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(fileStream);
        }

        return uniqueFileName;
    }

    public async Task<IActionResult> Index()
    {
        
        var products = await _uow.Products
            .GetAll()
            .Include(x => x.Category)
            .ToListAsync();
        return View(products);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        ViewBag.Kategoriler = await _uow.Categories.GetAll().ToListAsync();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Product product, IFormFile? imageFile)
    {
        try
        {
            //  Resim Doğrulama
            var imageError = ValidateImageFile(imageFile);
            if (imageError != null)
            {
                ModelState.AddModelError("", imageError);
                ViewBag.Kategoriler = await _uow.Categories.GetAll().ToListAsync();
                return View(product);
            }

            //  Resim Kaydetme
            if (imageFile != null)
            {
                product.Picture = await SaveImageFile(imageFile);
            }

            //  Veri Hazırlama 
            product.CreatedDate = DateTime.Now;
            product.IsActive = true;

            await _uow.Products.AddAsync(product);
            await _uow.SaveAsync();

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", $"Hata: {ex.Message}");
            ViewBag.Kategoriler = await _uow.Categories.GetAll().ToListAsync();
            return View(product);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        var product = await _uow.Products.GetByIdAsync(id);
        if (product == null)
            return NotFound();

        ViewBag.Kategoriler = await _uow.Categories.GetAll().ToListAsync();
        return View(product);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, [FromForm] Product product, IFormFile? imageFile)
    {
        // Kontrol: URL'deki ID ile formdan gelen ID aynı mı?
        if (id != product.ID) return NotFound();

        try
        {
            // Veriyi Önce Çekiyoruz
            var existingProduct = await _uow.Products.GetByIdAsync(id);
            if (existingProduct == null) return NotFound();

            // Eğer yeni resim varsa güncelle, yoksa eskisini koru
            if (imageFile != null && imageFile.Length > 0)
            {
                existingProduct.Picture = await SaveImageFile(imageFile);
            }

            //Sadece Değişen Alanları Mevcut Nesneye Aktar
            existingProduct.Name = product.Name;
            existingProduct.CategoryId = product.CategoryId;
            existingProduct.Price = product.Price;
            existingProduct.Stock = product.Stock;
            existingProduct.IsActive = product.IsActive;
            existingProduct.UpdateDate = DateTime.Now;

            // Kaydet
            _uow.Products.Update(existingProduct);
            await _uow.SaveAsync();

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            // Hata durumunda kategorileri tekrar yükleyip sayfaya dön
            ViewBag.Kategoriler = await _uow.Categories.GetAll().ToListAsync();
            ModelState.AddModelError("", "Güncelleme sırasında bir hata oluştu: " + ex.Message);
            return View(product);
        }
    }

    public async Task<IActionResult> Delete(Guid id)
    {
        var product = await _uow.Products.GetByIdAsync(id);
        if (product != null)
        {
            _uow.Products.Delete(product);
            await _uow.SaveAsync();
        }
        return RedirectToAction(nameof(Index));
    }
}