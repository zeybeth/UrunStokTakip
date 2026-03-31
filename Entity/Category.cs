using System;
using System.Collections.Generic;

namespace Entity;

public partial class Category : BaseModel
{
    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
