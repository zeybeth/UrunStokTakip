using System;
using System.Collections.Generic;

namespace Entity;

public partial class Cart :BaseModel
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public string UserId { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;
}
