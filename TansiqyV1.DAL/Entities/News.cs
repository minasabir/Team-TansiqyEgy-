namespace TansiqyV1.DAL.Entities;

public class News : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Description { get; set; } = string.Empty;
}

