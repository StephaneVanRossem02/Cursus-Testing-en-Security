namespace ShopWave.Web.Infrastructure
{
    public class DemoProduct
    {
        public int    Id    { get; set; }
        public string Name  { get; set; }
        public double Price { get; set; }

        public DemoProduct(int id, string name, double price)
        {
            Id    = id;
            Name  = name;
            Price = price;
        }
    }
}
