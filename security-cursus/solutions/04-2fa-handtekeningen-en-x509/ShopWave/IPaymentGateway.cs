namespace ShopWave
{
    public interface IPaymentGateway
    {
        bool ProcessPayment(double amount);
    }
}
