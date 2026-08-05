using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace ShopWave.Security
{
    public abstract class DocumentSigner
    {
        private readonly X509Certificate2 certificate;

        protected DocumentSigner(X509Certificate2 certificate)
        {
            this.certificate = certificate;
        }

        public string Sign(string data)
        {
            RSA privateKey = certificate.GetRSAPrivateKey()!;

            byte[] dataBytes      = Encoding.UTF8.GetBytes(data);
            byte[] signatureBytes = privateKey.SignData(
                dataBytes,
                HashAlgorithmName.SHA256,
                RSASignaturePadding.Pkcs1);

            return Convert.ToBase64String(signatureBytes);
        }

        public bool Verify(string data, string signature)
        {
            RSA publicKey = certificate.GetRSAPublicKey()!;

            byte[] dataBytes      = Encoding.UTF8.GetBytes(data);
            byte[] signatureBytes = Convert.FromBase64String(signature);

            return publicKey.VerifyData(
                dataBytes,
                signatureBytes,
                HashAlgorithmName.SHA256,
                RSASignaturePadding.Pkcs1);
        }
    }
}
