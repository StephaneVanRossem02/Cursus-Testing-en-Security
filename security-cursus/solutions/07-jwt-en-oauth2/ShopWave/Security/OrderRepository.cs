namespace ShopWave.Security
{
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
            orders[orderId] = encryptor.EncryptOrderData(orderData);
        }

        public string GetOrder(string orderId)
        {
            string result;

            if (!orders.ContainsKey(orderId))
            {
                result = string.Empty;
            }
            else
            {
                result = encryptor.DecryptOrderData(orders[orderId]);
            }

            return result;
        }
    }
}
