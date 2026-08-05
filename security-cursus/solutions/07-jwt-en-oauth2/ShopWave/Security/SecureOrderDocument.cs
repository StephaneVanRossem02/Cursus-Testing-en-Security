namespace ShopWave.Security
{
    public class SecureOrderDocument
    {
        private readonly AesEncryptor encryptor;
        private readonly OrderSigner  signer;

        public SecureOrderDocument()
        {
            encryptor = new AesEncryptor("ShopWaveGeheimeSleutel!!");
            signer    = new OrderSigner(
                CertificateHelper.CreateSelfSignedCertificate("ShopWave"));
        }

        public ProtectedOrder Protect(string orderData)
        {
            string encryptedData = encryptor.Encrypt(orderData);
            string signature     = signer.Sign(encryptedData);

            return new ProtectedOrder(encryptedData, signature);
        }

        public string Unprotect(string encryptedData, string signature)
        {
            bool signatureValid = signer.Verify(encryptedData, signature);

            if (!signatureValid)
            {
                throw new InvalidOperationException(
                    "Handtekening ongeldig. Data mogelijk gemanipuleerd.");
            }

            return encryptor.Decrypt(encryptedData);
        }
    }
}
