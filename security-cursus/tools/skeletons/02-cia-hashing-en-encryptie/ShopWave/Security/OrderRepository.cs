namespace ShopWave.Security
{
    // STARTCODE voor oefening 3 van les 2.
    public class OrderRepository
    {
        private readonly Dictionary<string, string> orders;
        private readonly OrderEncryptor             encryptor;

        public OrderRepository()
        {
            orders    = new Dictionary<string, string>();
            encryptor = new OrderEncryptor();
        }

        public void SaveOrder(string orderId, string orderData)
        {
            // jouw code hier
        }

        public string GetOrder(string orderId)
        {
            // jouw code hier

            return string.Empty;
        }
    }
}
