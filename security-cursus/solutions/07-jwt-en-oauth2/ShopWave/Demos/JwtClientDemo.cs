using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.IdentityModel.Tokens.Jwt;

namespace ShopWave
{
    // Demo uit de theorie: de volledige JWT-flow vanuit de console-client tegen ShopWave.Api.
    // Interactief (leest de 2FA-code van de console) en vereist dat ShopWave.Api draait op
    // https://localhost:5001. Compileert altijd; uitvoeren kan alleen met een draaiende API.
    public static class JwtClientDemo
    {
        public static void Run()
        {
            HttpClientHandler handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback =
                (message, certificate, chain, errors) => true;

            HttpClient client = new HttpClient(handler);
            client.BaseAddress = new Uri("https://localhost:5001");

            Console.WriteLine("=== Stap 1: Login ===");

            string        loginPayload  = JsonSerializer.Serialize(new { email = "alice@shopwave.be", password = "wachtwoord123" });
            StringContent loginContent  = new StringContent(loginPayload, Encoding.UTF8, "application/json");
            HttpResponseMessage loginResponse = client.PostAsync("/login", loginContent).Result;

            Console.WriteLine(loginResponse.Content.ReadAsStringAsync().Result);

            Console.Write("Voer de 2FA-code in (staat in de API-console): ");
            string twoFactorCode = Console.ReadLine() ?? string.Empty;

            Console.WriteLine("=== Stap 2: Verify + Token ophalen ===");

            string        verifyPayload  = JsonSerializer.Serialize(new { email = "alice@shopwave.be", code = twoFactorCode });
            StringContent verifyContent  = new StringContent(verifyPayload, Encoding.UTF8, "application/json");
            HttpResponseMessage verifyResponse = client.PostAsync("/verify", verifyContent).Result;

            string verifyBody = verifyResponse.Content.ReadAsStringAsync().Result;
            Console.WriteLine(verifyBody);

            JsonDocument verifyDoc = JsonDocument.Parse(verifyBody);
            string token = verifyDoc.RootElement.GetProperty("token").GetString() ?? string.Empty;

            Console.WriteLine("=== Stap 3: Met token (verwacht 200 OK) ===");

            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            HttpResponseMessage ordersResponse = client.GetAsync("/orders/alice@shopwave.be").Result;
            Console.WriteLine($"Status: {ordersResponse.StatusCode}");
            Console.WriteLine(ordersResponse.Content.ReadAsStringAsync().Result);

            Console.WriteLine("=== Stap 4: Zonder token (verwacht 401) ===");

            client.DefaultRequestHeaders.Authorization = null;
            HttpResponseMessage noTokenResponse = client.GetAsync("/orders/alice@shopwave.be").Result;
            Console.WriteLine($"Status: {noTokenResponse.StatusCode}");

            Console.WriteLine("=== Stap 5: JWT-payload leesbaar zonder sleutel ===");

            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            JwtSecurityToken        parsedToken  = tokenHandler.ReadJwtToken(token);

            Console.WriteLine($"Subject:  {parsedToken.Subject}");
            Console.WriteLine($"Verloopt: {parsedToken.ValidTo}");

            foreach (System.Security.Claims.Claim claim in parsedToken.Claims)
            {
                Console.WriteLine($"  {claim.Type}: {claim.Value}");
            }

            handler.Dispose();
            client.Dispose();
        }
    }
}
