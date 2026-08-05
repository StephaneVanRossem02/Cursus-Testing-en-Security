using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace ShopWave.Security
{
    public static class CertificateHelper
    {
        public static X509Certificate2 CreateSelfSignedCertificate(string subjectName)
        {
            using (RSA rsa = RSA.Create(2048))
            {
                CertificateRequest request = new CertificateRequest(
                    $"CN={subjectName}",
                    rsa,
                    HashAlgorithmName.SHA256,
                    RSASignaturePadding.Pkcs1);

                X509Certificate2 certificate = request.CreateSelfSigned(
                    DateTimeOffset.UtcNow,
                    DateTimeOffset.UtcNow.AddYears(1));

                return certificate;
            }
        }
    }
}
