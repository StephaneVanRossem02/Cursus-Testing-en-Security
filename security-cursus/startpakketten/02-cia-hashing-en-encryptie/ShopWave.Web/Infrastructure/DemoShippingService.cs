namespace ShopWave.Web.Infrastructure
{
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
