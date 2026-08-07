namespace ShopWave.Web.Infrastructure
{
    // DEMO-INFRASTRUCTUUR. Een vaste productlijst in het geheugen, zodat de webshop
    // iets te tonen heeft. In een echte applicatie komt dit uit een database.
    public class DemoProductCatalog
    {
        private readonly List<DemoProduct> products;

        public DemoProductCatalog()
        {
            products = new List<DemoProduct>
            {
                new DemoProduct(1, "Laptop",      999.99),
                new DemoProduct(2, "Muis",         29.99),
                new DemoProduct(3, "Toetsenbord",  79.99),
                new DemoProduct(4, "Monitor",     249.50),
                new DemoProduct(5, "Webcam",       59.95)
            };
        }

        public List<DemoProduct> GetAll()
        {
            return products;
        }

        public DemoProduct GetById(int productId)
        {
            DemoProduct result = products.FirstOrDefault(p => p.Id == productId);

            return result;
        }
    }
}
