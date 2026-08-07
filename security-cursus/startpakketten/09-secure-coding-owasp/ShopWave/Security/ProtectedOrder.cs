namespace ShopWave.Security
{
    public class ProtectedOrder
    {
        public string EncryptedData { get; }
        public string Signature     { get; }

        public ProtectedOrder(string encryptedData, string signature)
        {
            EncryptedData = encryptedData;
            Signature     = signature;
        }
    }
}
