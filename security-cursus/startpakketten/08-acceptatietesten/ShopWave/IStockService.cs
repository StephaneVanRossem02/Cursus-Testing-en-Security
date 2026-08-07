namespace ShopWave
{
    public interface IStockService
    {
        bool IsInStock(int productId, int quantity);
    }
}
