using System.Net.Http;
using ShopWave.Security;

namespace ShopWave
{
    // Demo uit de oplossingen (oefening 3): een token met vervaltijd 0 wordt door de API
    // geweigerd. Vereist een draaiende ShopWave.Api op https://localhost:5001 en de
    // omgevingsvariabele JWT_SECRET_KEY.
    public static class ExpiredTokenDemo
    {
        public static void Run()
        {
            string secretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY")
                ?? throw new InvalidOperationException("Omgevingsvariabele JWT_SECRET_KEY ontbreekt.");

            JwtTokenService shortLived = new JwtTokenService(
                secretKey,
                "shopwave-api",
                "shopwave-client",
                expiresMinutes: 0);

            string expiredToken = shortLived.GenerateToken("alice@shopwave.be", "user");

            System.Threading.Thread.Sleep(2000);

            HttpClientHandler handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback =
                (message, certificate, chain, errors) => true;

            HttpClient client = new HttpClient(handler);
            client.BaseAddress = new Uri("https://localhost:5001");

            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", expiredToken);

            HttpResponseMessage response = client.GetAsync("/orders/alice@shopwave.be").Result;

            Console.WriteLine($"Verlopen token statuscode: {response.StatusCode}");

            client.Dispose();
            handler.Dispose();
        }
    }
}
