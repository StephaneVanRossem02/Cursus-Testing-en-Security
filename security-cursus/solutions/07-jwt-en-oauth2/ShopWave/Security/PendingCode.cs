namespace ShopWave.Security
{
    public class PendingCode
    {
        public string   Code      { get; }
        public DateTime ExpiresAt { get; }

        public PendingCode(string code, DateTime expiresAt)
        {
            Code      = code;
            ExpiresAt = expiresAt;
        }
    }
}
