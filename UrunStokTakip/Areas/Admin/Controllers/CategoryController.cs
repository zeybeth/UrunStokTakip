using Data.Abstract;
using Entity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace UrunStokTakip.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CategoryController : Controller
    {
        private readonly IUnitOfWork _uow;

        public CategoryController(IUnitOfWork uow)
        {
            _uow = uow;
        }

        //  Kategorileri Listeleme
        public async Task<IActionResult> Index()
        {
            var categories = await _uow.GetRepository<Category>().GetAll().ToListAsync();
            return View(categories);
        }

        // Yeni Kategori Ekleme Sayfasını Gösterme (GET)
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        //  Yeni Kategori Ekleme (POST)
        [HttpPost]
        public async Task<IActionResult> Create(Category category)
        {
            if (ModelState.IsValid)
            {
                await _uow.GetRepository<Category>().AddAsync(category);
                await _uow.SaveAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        //  Kategori Silme
        public async Task<IActionResult> Delete(Guid id)
        {
            var repository = _uow.GetRepository<Category>();
            var category = await repository.GetByIdAsync(id);

            if (category != null)
            {
                repository.Delete(category);
                await _uow.SaveAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // Güncelleme Sayfasını Aç (GET)
        public async Task<IActionResult> Edit(Guid id)
        {
            var category = await _uow.GetRepository<Category>().GetByIdAsync(id);
            if (category == null) return NotFound();
            return View(category);
        }

        // Güncelleme İşlemini Yap (POST)
        [HttpPost]
        public async Task<IActionResult> Edit(Category category)
        {
            if (ModelState.IsValid)
            {
                _uow.GetRepository<Category>().Update(category);
                await _uow.SaveAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }
    }
}