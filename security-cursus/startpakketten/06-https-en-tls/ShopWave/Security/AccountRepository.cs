namespace ShopWave.Security
{
    public class AccountRepository
    {
        private readonly Dictionary<string, CustomerAccount> accounts;
        private readonly Dictionary<string, int>             failedAttempts;
        private readonly TwoFactorService                    twoFactorService;
        private readonly Action<string, string>              onCodeGenerated;
        private const int MaxAttempts = 3;

        public AccountRepository(
            TwoFactorService       twoFactorService,
            Action<string, string> onCodeGenerated = null)
        {
            accounts              = new Dictionary<string, CustomerAccount>();
            failedAttempts        = new Dictionary<string, int>();
            this.twoFactorService = twoFactorService;
            this.onCodeGenerated  = onCodeGenerated;
        }

        public string Register(string email, string password)
        {
            string result;

            if (accounts.ContainsKey(email))
            {
                result = "Account bestaat al.";
            }
            else
            {
                CustomerAccount account = new CustomerAccount(email, password);
                accounts[email]        = account;
                failedAttempts[email]  = 0;

                result = "Registratie geslaagd.";
            }

            return result;
        }

        public string Login(string email, string password)
        {
            string result;

            if (!accounts.ContainsKey(email))
            {
                result = "Gebruiker niet gevonden.";
            }
            else if (failedAttempts[email] >= MaxAttempts)
            {
                result = "Account geblokkeerd.";
            }
            else
            {
                bool correct = accounts[email].VerifyPassword(password);

                if (correct)
                {
                    string code = twoFactorService.GenerateCode(email);

                    if (onCodeGenerated != null)
                    {
                        onCodeGenerated(email, code);
                    }

                    failedAttempts[email] = 0;
                    result = "Voer uw 2FA-code in.";
                }
                else
                {
                    failedAttempts[email]++;

                    if (failedAttempts[email] >= MaxAttempts)
                    {
                        result = "Account geblokkeerd.";
                    }
                    else
                    {
                        result = "Ongeldig wachtwoord.";
                    }
                }
            }

            return result;
        }

        public string VerifyTwoFactor(string email, string code)
        {
            string result;
            bool   valid = twoFactorService.VerifyCode(email, code);

            if (valid)
            {
                result = "Inloggen geslaagd.";
            }
            else
            {
                result = "Ongeldige 2FA-code.";
            }

            return result;
        }

        public bool ChangePassword(string email, string newPassword)
        {
            bool result;

            if (!accounts.ContainsKey(email))
            {
                result = false;
            }
            else
            {
                accounts[email] = new CustomerAccount(email, newPassword);
                result = true;
            }

            return result;
        }
    }
}
