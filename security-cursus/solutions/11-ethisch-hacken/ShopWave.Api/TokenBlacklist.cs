namespace ShopWave.Api
{
    public class TokenBlacklist
    {
        private readonly HashSet<string> revokedTokens;

        public TokenBlacklist()
        {
            revokedTokens = new HashSet<string>();
        }

        public void Revoke(string token)
        {
            revokedTokens.Add(token);
        }

        public bool IsRevoked(string token)
        {
            return revokedTokens.Contains(token);
        }
    }
}
