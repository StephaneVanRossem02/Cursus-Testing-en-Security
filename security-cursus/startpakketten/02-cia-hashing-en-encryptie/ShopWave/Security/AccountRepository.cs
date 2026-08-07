namespace ShopWave.Security
{
    // STARTCODE voor oefening 1 en 2 van les 2. De signaturen staan er al in zodat
    // de webshop compileert en je je eigen code meteen ziet werken. De bodies vul
    // je zelf aan. Zolang je dat niet doet, doet registreren en inloggen nog niets.
    public class AccountRepository
    {
        private readonly Dictionary<string, CustomerAccount> accounts;
        private readonly Dictionary<string, int>             failedAttempts;
        private const int MaxAttempts = 3;

        public AccountRepository()
        {
            accounts       = new Dictionary<string, CustomerAccount>();
            failedAttempts = new Dictionary<string, int>();
        }

        public string Register(string email, string password)
        {
            // jouw code hier

            return string.Empty;
        }

        public string Login(string email, string password)
        {
            // jouw code hier

            return string.Empty;
        }
    }
}
