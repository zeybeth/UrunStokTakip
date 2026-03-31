namespace Entity
{
    public abstract class BaseModel
    {
        public Guid ID { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? UpdateDate { get; set; }
        public bool IsActive { get; set; } = true;
        public string? Picture { get; set; }
        public decimal Price { get; set; }
    }
}
