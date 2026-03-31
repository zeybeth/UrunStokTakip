using System.Collections.Generic;

namespace Entity;

public class ProductListViewModel
{
    // Sayfada gösterilen ürün listesi
    public List<Product> Products { get; set; }

    // Sayfa bilgileri
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public int TotalCount { get; set; } //toplam öğe sayısı

    // kategori bilgisi (filtreleme için kullanıcaz)
    public int? CategoryId { get; set; }
}