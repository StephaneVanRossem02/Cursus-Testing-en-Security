namespace ShopWave
{
    public class CheckoutService
    {
        private readonly IShippingService shippingService;
        private readonly DiscountCalculator discountCalculator;

        public CheckoutService(IShippingService shippingService)
        {
            this.shippingService = shippingService;
            discountCalculator = new DiscountCalculator();
        }

        public double CalculateFinalTotal(double unitPrice, int quantity, int discountPercent)
        {
            double subtotal = unitPrice * quantity;
            double afterDiscount = discountCalculator.ApplyDiscount(subtotal, discountPercent);
            double shippingCost = shippingService.GetShippingCost(afterDiscount);
            double finalTotal = afterDiscount + shippingCost;

            return finalTotal;
        }
    }
}
