namespace shopflowerproject.Models;

public class ShoppingCart
{
    public List<Flowers> Flowers { get; set; } = new List<Flowers>();
    public decimal? ThanhTien {get;set;}
}