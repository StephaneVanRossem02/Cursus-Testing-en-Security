namespace ShopWave.Security
{
    public class CorsValidator
    {
        private readonly List<string> allowedOrigins;

        public CorsValidator()
        {
            allowedOrigins = new List<string>
            {
                "https://shopwave.be",
                "https://localhost:3000"
            };
        }

        public bool SimulateRequest(string origin)
        {
            return allowedOrigins.Contains(origin);
        }
    }
}
