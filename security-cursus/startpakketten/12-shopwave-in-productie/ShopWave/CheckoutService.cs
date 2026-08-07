namespace ShopWave
{
    public class CheckoutService
    {
        private readonly CartService     cartService;
        private readonly IPaymentGateway gateway;

        public CheckoutService(CartService cartService, IPaymentGateway gateway)
        {
            this.cartService = cartService;
            this.gateway     = gateway;
        }

        public string Checkout()
        {
            double amount = cartService.Total;
            string result;

            if (amount <= 0)
            {
                result = "Mandje is leeg";
            }
            else
            {
                bool success = gateway.ProcessPayment(amount);
                result = success ? "Betaling geslaagd" : "Betaling mislukt";
            }

            return result;
        }
    }
}
