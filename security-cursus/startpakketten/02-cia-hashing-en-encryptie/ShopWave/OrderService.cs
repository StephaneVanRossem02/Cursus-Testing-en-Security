namespace ShopWave
{
    public class OrderService
    {
        private readonly IPaymentGateway gateway;
        private readonly IStockService stockService;

        public OrderService(IPaymentGateway gateway, IStockService stockService)
        {
            this.gateway = gateway;
            this.stockService = stockService;
        }

        public string PlaceOrder(int productId, int quantity, double amount)
        {
            string result;

            if (amount <= 0)
            {
                throw new ArgumentException(
                    "Bedrag moet groter zijn dan nul.",
                    nameof(amount));
            }

            bool inStock = stockService.IsInStock(productId, quantity);

            if (!inStock)
            {
                result = "Product niet beschikbaar";
            }
            else
            {
                bool success = gateway.ProcessPayment(amount);

                if (success)
                {
                    result = "Bestelling bevestigd";
                }
                else
                {
                    result = "Betaling mislukt";
                }
            }

            return result;
        }
    }
}
