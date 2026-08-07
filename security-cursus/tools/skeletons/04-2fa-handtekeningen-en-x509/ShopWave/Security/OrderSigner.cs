using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace ShopWave.Security
{
    // Dit is de klasse zoals je ze in de theorie van les 4 opbouwt. In oefening 3
    // haal je de gemeenschappelijke logica hieruit naar een basisklasse zodat
    // InvoiceSigner ze kan hergebruiken.
    public class OrderSigner
    {
        private readonly X509Certificate2 certificate;

        public OrderSigner(X509Certificate2 certificate)
        {
            this.certificate = certificate;
        }

        public string Sign(string orderData)
        {
            RSA privateKey = certificate.GetRSAPrivateKey()!;

            byte[] dataBytes      = Encoding.UTF8.GetBytes(orderData);
            byte[] signatureBytes = privateKey.SignData(
                dataBytes,
                HashAlgorithmName.SHA256,
                RSASignaturePadding.Pkcs1);

            return Convert.ToBase64String(signatureBytes);
        }

        public bool Verify(string orderData, string signature)
        {
            RSA publicKey = certificate.GetRSAPublicKey()!;

            byte[] dataBytes      = Encoding.UTF8.GetBytes(orderData);
            byte[] signatureBytes = Convert.FromBase64String(signature);

            return publicKey.VerifyData(
                dataBytes,
                signatureBytes,
                HashAlgorithmName.SHA256,
                RSASignaturePadding.Pkcs1);
        }
    }
}
