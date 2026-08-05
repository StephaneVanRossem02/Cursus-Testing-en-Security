namespace ShopWave.Security
{
    public class OrderEncryptor
    {
        private const  string       KeyString = "ShopWaveOrderSleutel!!";
        private readonly AesEncryptor aes;

        public OrderEncryptor()
        {
            aes = new AesEncryptor(KeyString);
        }

        public string EncryptOrderData(string orderData)
        {
            return aes.Encrypt(orderData);
        }

        public string DecryptOrderData(string encryptedData)
        {
            return aes.Decrypt(encryptedData);
        }
    }
}
