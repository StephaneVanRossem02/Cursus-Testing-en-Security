using System.Security.Cryptography.X509Certificates;

namespace ShopWave.Security
{
    public class OrderSigner : DocumentSigner
    {
        public OrderSigner(X509Certificate2 certificate) : base(certificate)
        {
        }
    }
}
