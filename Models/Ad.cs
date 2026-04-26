namespace UserAuthApi.Models
{
    public class Ad
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Category { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public int UserId { get; set; } // İlanı veren kullanıcının ID'si
        public string CreatedBy { get; set; } = string.Empty;
        public string? ImageUrl { get; internal set; }
    }
}
