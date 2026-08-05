using System.Net.Http;
using System.Net.Security;

namespace ShopWave
{
    // Demo uit de oplossingen: HTTP versus HTTPS vergelijken door de ShopWave.Api te bevragen.
    // Vereist dat ShopWave.Api draait op https://localhost:5001. Compileert altijd; draaien
    // kan alleen als de API actief is.
    public static class HttpsComparisonDemo
    {
        public static void Run()
        {
            ToonOnveiligVerkeer();
            ToonCertificaatInfo();
        }

        static void ToonOnveiligVerkeer()
        {
            Console.WriteLine("=== Onveilig scenario ===");

            HttpClient client   = new HttpClient(new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback =
                    (msg, cert, chain, errors) => true
            });

            string response = client.GetStringAsync("https://localhost:5001/onveilig/inlog").Result;

            // In productie zou dit endpoint via HTTP bereikbaar zijn. Dan is de response onversleuteld.
            Console.WriteLine($"Wat een aanvaller op HTTP zou zien: {response}");
            Console.WriteLine();

            client.Dispose();
        }

        static void ToonCertificaatInfo()
        {
            Console.WriteLine("=== Certificaatinfo ===");

            string subject    = string.Empty;
            string issuer     = string.Empty;
            bool   selfSigned = false;

            HttpClientHandler handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback =
                (msg, cert, chain, errors) =>
                {
                    subject    = cert.Subject;
                    issuer     = cert.Issuer;
                    selfSigned = cert.Subject == cert.Issuer;
                    return true;
                };

            HttpClient client = new HttpClient(handler);
            // Broncorrectie: in de bron staat hier `client.GetStringAsync(...).Result;` als losse
            // instructie, wat niet compileert (CS0201). Het resultaat wordt weggegooid met `_ =`.
            _ = client.GetStringAsync("https://localhost:5001/veilig/certificaatinfo").Result;

            Console.WriteLine($"Subject:     {subject}");
            Console.WriteLine($"Issuer:      {issuer}");
            Console.WriteLine($"Self-signed: {selfSigned}");

            client.Dispose();
            handler.Dispose();
        }
    }
}
