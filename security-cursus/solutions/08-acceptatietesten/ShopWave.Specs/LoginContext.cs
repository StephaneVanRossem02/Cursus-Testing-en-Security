using ShopWave.Security;

namespace ShopWave.Specs
{
    public class LoginContext
    {
        public AccountRepository AccountRepository { get; set; } = null!;
        public TwoFactorService  TwoFactorService  { get; set; } = null!;
        public string            LastCode          { get; set; } = string.Empty;
        public string            Result            { get; set; } = string.Empty;
    }
}
