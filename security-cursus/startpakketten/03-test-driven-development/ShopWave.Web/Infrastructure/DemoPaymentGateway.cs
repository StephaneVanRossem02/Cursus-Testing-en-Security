namespace ShopWave.Web.Infrastructure
{
    // DEMO-INFRASTRUCTUUR. De cursus definieert IPaymentGateway maar mockt hem in de
    // tests. Om de webshop te laten draaien hebben we een concrete versie nodig. Deze
    // betaling slaagt altijd, zodat de flow voorspelbaar blijft.
    public class DemoPaymentGateway : IPaymentGateway
    {
        public bool ProcessPayment(double amount)
        {
            bool result = amount > 0;

            return result;
        }
    }
}
