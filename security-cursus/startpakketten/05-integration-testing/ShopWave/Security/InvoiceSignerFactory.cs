namespace ShopWave.Security
{
    public static class InvoiceSignerFactory
    {
        public static InvoiceSigner Create()
        {
            return new InvoiceSigner(
                CertificateHelper.CreateSelfSignedCertificate("ShopWave"));
        }
    }
}
