using Entity;

namespace UrunStokTakip.Models
{
    public class HomeViewModel
    {
        public IEnumerable<Category> Categories { get; set; } = new List<Category>();
        public IEnumerable<Product> Products { get; set; } = new List<Product>();
        public Guid? SelectedCategoryId { get; set; }
    }
}