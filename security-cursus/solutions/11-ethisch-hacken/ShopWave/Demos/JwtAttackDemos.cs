using System.Net.Http;
using System.Text;

namespace ShopWave
{
    // Demo uit de oplossingen: aanvalssimulaties op JWT (rolmanipulatie en alg:none).
    // Beide horen te mislukken tegen een correct geconfigureerde ShopWave.Api.
    // Vereist een draaiende API op https://localhost:5001.
    public static class JwtAttackDemos
    {
        public static void TryRoleManipulation(string validToken)
        {
            Console.WriteLine("=== JWT-rolmanipulatie poging ===");

            string[] parts   = validToken.Split('.');
            string   payload = parts[1];

            // Padding herstellen voor Base64-decodering
            int padLength = 4 - (payload.Length % 4);
            if (padLength != 4)
            {
                payload += new string('=', padLength);
            }

            // Base64url -> Base64
            payload = payload.Replace('-', '+').Replace('_', '/');

            // Decoderen
            string decodedPayload = Encoding.UTF8.GetString(Convert.FromBase64String(payload));
            Console.WriteLine($"Originele payload: {decodedPayload}");

            // Payload aanpassen
            string manipulatedPayload = decodedPayload.Replace("\"role\":\"user\"", "\"role\":\"admin\"");
            Console.WriteLine($"Aangepaste payload: {manipulatedPayload}");

            // Hercoderen naar Base64url
            byte[] manipulatedBytes = Encoding.UTF8.GetBytes(manipulatedPayload);
            string reEncodedPayload = Convert.ToBase64String(manipulatedBytes)
                .Replace('+', '-').Replace('/', '_').TrimEnd('=');

            // Token samenstellen met originele header en signature maar aangepaste payload
            string manipulatedToken = $"{parts[0]}.{reEncodedPayload}.{parts[2]}";

            // Versturen
            HttpClientHandler handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback =
                (message, certificate, chain, errors) => true;

            HttpClient client = new HttpClient(handler);
            client.BaseAddress = new Uri("https://localhost:5001");
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", manipulatedToken);

            HttpResponseMessage response = client.GetAsync("/admin/orders").Result;
            Console.WriteLine($"Resultaat met gemanipuleerd token: {response.StatusCode}");
            Console.WriteLine("Verwacht: Unauthorized (signature klopt niet meer)");

            client.Dispose();
            handler.Dispose();
        }

        public static void TryAlgNoneAttack()
        {
            Console.WriteLine("=== alg:none aanval ===");

            // Header coderen
            string header = Convert.ToBase64String(
                Encoding.UTF8.GetBytes("{\"alg\":\"none\",\"typ\":\"JWT\"}"))
                .Replace('+', '-').Replace('/', '_').TrimEnd('=');

            // Payload coderen
            string payload = Convert.ToBase64String(
                Encoding.UTF8.GetBytes("{\"sub\":\"admin@shopwave.be\",\"role\":\"admin\"}"))
                .Replace('+', '-').Replace('/', '_').TrimEnd('=');

            // Token samenstellen zonder signature
            string noneToken = $"{header}.{payload}.";

            HttpClientHandler handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback =
                (message, certificate, chain, errors) => true;

            HttpClient client = new HttpClient(handler);
            client.BaseAddress = new Uri("https://localhost:5001");
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", noneToken);

            HttpResponseMessage response = client.GetAsync("/admin/orders").Result;
            Console.WriteLine($"Resultaat alg:none token: {response.StatusCode}");
            Console.WriteLine("Verwacht: Unauthorized (.NET weigert alg:none standaard)");

            client.Dispose();
            handler.Dispose();
        }
    }
}
