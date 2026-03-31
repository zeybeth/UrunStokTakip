using Data;
using Data.Abstract;
using Entity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace UrunStokTakip.Areas.Admin.Controllers;

[Area("Admin")]
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
        try
        {
            if (file == null || file.Length == 0)
                return null;

            if (string.IsNullOrEmpty(_env.WebRootPath))
                return null;

            var uploadsFolder = Path.Combine(_env.WebRootPath, "img");

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return uniqueFileName;
        }
        catch (Exception)
        {
            return null;
        }
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
            var test = ValidateImageFile(imageFile);
            if (test != null)
            {
                ModelState.AddModelError("", test);
                ViewBag.Kategoriler = await _uow.Categories.GetAll().ToListAsync();
                return View(product);
            }

            product.ID = Guid.NewGuid();
            product.CreatedDate = DateTime.Now;
            product.IsActive = true;

            if (imageFile != null && imageFile.Length > 0)
            {
                var fileName = await SaveImageFile(imageFile);
                if (!string.IsNullOrEmpty(fileName))
                {
                    product.Picture = fileName;
                }
            }

            await _uow.Products.AddAsync(product);
            await _uow.SaveAsync();

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", $"HATA: {ex.Message}");
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
    public async Task<IActionResult> Edit(Guid id, Product product, IFormFile? imageFile)
    {
        try
        {
            var validationError = ValidateImageFile(imageFile);
            if (validationError != null)
            {
                ModelState.AddModelError("", validationError);
                ViewBag.Kategoriler = await _uow.Categories.GetAll().ToListAsync();
                return View(product);
            }

            if (imageFile != null && imageFile.Length > 0)
            {
                var fileName = await SaveImageFile(imageFile);
                if (!string.IsNullOrEmpty(fileName))
                {
                    product.Picture = fileName;
                }
            }

            product.UpdateDate = DateTime.Now;

            _uow.Products.Update(product);
            await _uow.SaveAsync();

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", $"Hata oluştu: {ex.Message}");
            ViewBag.Kategoriler = await _uow.Categories.GetAll().ToListAsync();
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