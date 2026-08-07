namespace ShopWave.Web.Infrastructure
{
    // DEMO-INFRASTRUCTUUR. De cursus definieert IShippingService maar mockt hem in de
    // tests. Deze versie rekent een vast tarief en levert gratis vanaf 50 euro, zodat
    // het resultaat altijd voorspelbaar is.
    public class DemoShippingService : IShippingService
    {
        private const double StandardCost   = 4.99;
        private const double FreeFromAmount = 50.0;

        public double GetShippingCost(double totalAfterDiscount)
        {
            double result;

            if (totalAfterDiscount >= FreeFromAmount)
            {
                result = 0.0;
            }
            else
            {
                result = StandardCost;
            }

            return result;
        }
    }
}
