namespace ShopWave
{
    public class CartItem
    {
        public string Name     { get; set; }
        public double Price    { get; set; }
        public int    Quantity { get; set; }

        public CartItem(string name, double price, int quantity)
        {
            Name     = name;
            Price    = price;
            Quantity = quantity;
        }
    }
}
