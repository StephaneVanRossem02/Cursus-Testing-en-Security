namespace ShopWave.Web.Infrastructure
{
    // DEMO-INFRASTRUCTUUR. De cursus definieert IStockService maar mockt hem in de
    // tests. Deze versie houdt de voorraad in het geheugen bij, zodat je in de webshop
    // ziet wat er gebeurt als iets niet op voorraad is.
    public class DemoStockService : IStockService
    {
        private readonly Dictionary<int, int> stock;

        public DemoStockService()
        {
            stock = new Dictionary<int, int>
            {
                { 1, 5 },
                { 2, 25 },
                { 3, 12 },
                { 4, 3 },
                { 5, 0 }
            };
        }

        public bool IsInStock(int productId, int quantity)
        {
            bool result = false;

            if (stock.ContainsKey(productId))
            {
                result = stock[productId] >= quantity;
            }

            return result;
        }

        public int GetQuantity(int productId)
        {
            int result = 0;

            if (stock.ContainsKey(productId))
            {
                result = stock[productId];
            }

            return result;
        }
    }
}
