namespace ShopWave
{
    public interface IShippingService
    {
        double GetShippingCost(double totalAfterDiscount);
    }
}
