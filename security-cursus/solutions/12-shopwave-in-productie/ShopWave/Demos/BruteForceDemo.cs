using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace ShopWave
{
    // Demo uit de oplossingen: brute-force op het login-endpoint om rate limiting te tonen.
    // Vereist een draaiende ShopWave.Api op https://localhost:5001.
    public static class BruteForceDemo
    {
        public static void Run()
        {
            HttpClientHandler handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback =
                (message, certificate, chain, errors) => true;

            HttpClient client = new HttpClient(handler);
            client.BaseAddress = new Uri("https://localhost:5001");

            Console.WriteLine("=== Brute-force simulatie ===");

            for (int attempt = 1; attempt <= 7; attempt++)
            {
                string payload = JsonSerializer.Serialize(
                    new { email = "alice@shopwave.be", password = $"poging{attempt}" });

                HttpResponseMessage response = client.PostAsync("/login",
                    new StringContent(payload, Encoding.UTF8, "application/json")).Result;

                Console.WriteLine($"Poging {attempt}: {response.StatusCode}");
            }

            client.Dispose();
            handler.Dispose();
        }
    }
}
