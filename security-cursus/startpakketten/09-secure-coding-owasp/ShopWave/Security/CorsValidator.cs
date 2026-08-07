namespace ShopWave.Security
{
    // STARTCODE voor oefening 4 van les 9.
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
            // jouw code hier

            return false;
        }
    }
}
