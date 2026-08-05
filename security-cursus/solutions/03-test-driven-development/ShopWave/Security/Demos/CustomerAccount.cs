namespace ShopWave.Security
{
    public class CustomerAccount
    {
        public string Email        { get; private set; }
        public string PasswordHash { get; private set; }

        private readonly PasswordHasher hasher;

        public CustomerAccount(string email, string password)
        {
            hasher      = new PasswordHasher();
            Email        = email;
            PasswordHash = hasher.Hash(password);
        }

        public bool VerifyPassword(string password)
        {
            return hasher.Verify(password, PasswordHash);
        }
    }
}
