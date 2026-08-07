namespace ShopWave.Web.Infrastructure
{
    // DEMO-INFRASTRUCTUUR. Deze klasse staat niet in de cursus. De cursus werkt met
    // productId's en bedragen, maar heeft geen productcatalogus. Om de webshop iets te
    // laten tonen hebben we een minimale productvoorstelling nodig.
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
