using System.Security.Cryptography.X509Certificates;

namespace ShopWave.Security
{
    public class InvoiceSigner : DocumentSigner
    {
        public InvoiceSigner(X509Certificate2 certificate) : base(certificate)
        {
        }
    }
}
