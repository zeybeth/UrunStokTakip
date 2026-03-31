namespace Entity;

public partial class Product : BaseModel
{
    public string? Name { get; set; }

    public string? Description { get; set; }

    public int Stock { get; set; }
    public Guid CategoryId { get; set; }

    public virtual Category? Category { get; set; }

    public virtual ICollection<Sale> Sales { get; set; } = new List<Sale>();
    public virtual ICollection<Cart> Carts { get; set; } = new List<Cart>();
}