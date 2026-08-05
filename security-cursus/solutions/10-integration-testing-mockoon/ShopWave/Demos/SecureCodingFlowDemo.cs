using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace ShopWave
{
    // Demo uit de theorie: de volledige secure-coding-flow testen tegen de API
    // (normale zoekopdracht, SQL Injection-poging, input validatie, foutafhandeling).
    // Vereist een draaiende ShopWave.Api op https://localhost:5001.
    public static class SecureCodingFlowDemo
    {
        public static void Run()
        {
            HttpClientHandler handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback =
                (message, certificate, chain, errors) => true;

            HttpClient client = new HttpClient(handler);
            client.BaseAddress = new Uri("https://localhost:5001");

            // Test 1: normale zoekopdracht
            Console.WriteLine("=== Test 1: Normale zoekopdracht ===");
            HttpResponseMessage normalSearch = client.GetAsync(
                "/orders/zoek?email=alice@shopwave.be").Result;
            Console.WriteLine(normalSearch.Content.ReadAsStringAsync().Result);

            // Test 2: SQL Injection poging
            Console.WriteLine("=== Test 2: SQL Injection poging ===");
            HttpResponseMessage injectionAttempt = client.GetAsync(
                "/orders/zoek?email=' OR '1'='1").Result;
            Console.WriteLine(injectionAttempt.Content.ReadAsStringAsync().Result);

            // Test 3: input validatie
            Console.WriteLine("=== Test 3: Input validatie ===");
            string emptyEmail = JsonSerializer.Serialize(new { email = "", password = "wachtwoord123" });
            HttpResponseMessage emptyEmailResponse = client.PostAsync("/register",
                new StringContent(emptyEmail, Encoding.UTF8, "application/json")).Result;
            Console.WriteLine($"Leeg e-mail: {emptyEmailResponse.StatusCode}");
            Console.WriteLine(emptyEmailResponse.Content.ReadAsStringAsync().Result);

            string shortPassword = JsonSerializer.Serialize(new { email = "test@shopwave.be", password = "kort" });
            HttpResponseMessage shortPasswordResponse = client.PostAsync("/register",
                new StringContent(shortPassword, Encoding.UTF8, "application/json")).Result;
            Console.WriteLine($"Kort wachtwoord: {shortPasswordResponse.StatusCode}");
            Console.WriteLine(shortPasswordResponse.Content.ReadAsStringAsync().Result);

            // Test 4: foutafhandeling
            Console.WriteLine("=== Test 4: Foutafhandeling ===");
            HttpResponseMessage crashResponse = client.GetAsync("/crash").Result;
            Console.WriteLine($"Crash: {crashResponse.StatusCode}");
            Console.WriteLine(crashResponse.Content.ReadAsStringAsync().Result);

            handler.Dispose();
            client.Dispose();
        }
    }
}
