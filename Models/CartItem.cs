namespace shopflowerproject.Models;

public class CartItem : Flowers{
    public Flowers flowers { get; set; }
    public int Quantity { get; set; }
    
}