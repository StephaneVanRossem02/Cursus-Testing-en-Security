using System.Net.Http;
using System.Text.Json;

namespace ShopWave
{
    public class ShippingClient
    {
        private readonly HttpClient httpClient;
        private readonly string     baseUrl;

        public ShippingClient(HttpClient httpClient, string baseUrl)
        {
            this.httpClient = httpClient;
            this.baseUrl    = baseUrl;
        }

        public async Task<ShippingResponse> GetShippingRateAsync(
            string destination, double weight)
        {
            string url = $"{baseUrl}/api/verzending?bestemming={destination}&gewicht={weight}";
            HttpResponseMessage response = await httpClient.GetAsync(url);

            response.EnsureSuccessStatusCode();

            string json = await response.Content.ReadAsStringAsync();

            ShippingResponse result = JsonSerializer.Deserialize<ShippingResponse>(
                json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

            return result;
        }
    }
}
