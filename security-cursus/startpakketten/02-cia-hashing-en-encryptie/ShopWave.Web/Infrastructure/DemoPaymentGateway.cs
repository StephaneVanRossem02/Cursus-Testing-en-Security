namespace ShopWave.Web.Infrastructure
{
    public class DemoPaymentGateway : IPaymentGateway
    {
        public bool ProcessPayment(double amount)
        {
            bool result = amount > 0;

            return result;
        }
    }
}
