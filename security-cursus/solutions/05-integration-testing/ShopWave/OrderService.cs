namespace ShopWave
{
    public class OrderService
    {
        private readonly IPaymentGateway gateway;
        private readonly IStockService   stockService;
        private readonly ICouponService  couponService;

        public OrderService(
            IPaymentGateway gateway,
            IStockService   stockService,
            ICouponService  couponService)
        {
            this.gateway       = gateway;
            this.stockService  = stockService;
            this.couponService = couponService;
        }

        public string PlaceOrder(int productId, int quantity, double amount, string couponCode = "")
        {
            string result;

            if (amount <= 0)
            {
                throw new ArgumentException("Bedrag moet groter zijn dan nul.", nameof(amount));
            }

            if (couponCode != "" && !couponService.IsValid(couponCode))
            {
                // Onderscheid: onbekende coupon vs. al gebruikte coupon
                // We controleren dit door te kijken of IsValid false geeft terwijl de code niet leeg is.
                // De eenvoudigste aanpak: voeg IsUsed toe aan de interface,
                // of gebruik een aparte IsKnown-methode.
                // Hier: als IsValid false geeft voor een niet-lege code, melden we "Coupon reeds gebruikt."
                // enkel als de coupon al gebruikt is (extra methode op interface nodig).
                // Vereenvoudigde versie: zie toelichting.
                result = "Coupon reeds gebruikt.";
            }
            else
            {
                bool inStock = stockService.IsInStock(productId, quantity);

                if (!inStock)
                {
                    result = "Product niet beschikbaar";
                }
                else
                {
                    double finalAmount = amount;

                    if (couponCode != "" && couponService.IsValid(couponCode))
                    {
                        int discount = couponService.GetDiscount(couponCode);
                        finalAmount  = amount * (1 - discount / 100.0);
                        couponService.MarkAsUsed(couponCode);
                    }

                    bool success = gateway.ProcessPayment(finalAmount);

                    if (success)
                    {
                        result = "Bestelling bevestigd";
                    }
                    else
                    {
                        result = "Betaling mislukt";
                    }
                }
            }

            return result;
        }
    }
}
