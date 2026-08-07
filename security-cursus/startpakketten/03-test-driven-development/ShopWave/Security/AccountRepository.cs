namespace ShopWave.Security
{
    public class AccountRepository
    {
        private readonly Dictionary<string, CustomerAccount> accounts;
        private readonly Dictionary<string, int>             failedAttempts;
        private const int MaxAttempts = 3;
        private readonly PasswordValidator validator;

        public AccountRepository()
        {
            accounts       = new Dictionary<string, CustomerAccount>();
            failedAttempts = new Dictionary<string, int>();
            validator      = new PasswordValidator();
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
                string error = validator.GetErrorMessage(password);

                if (error != string.Empty)
                {
                    result = error;
                }
                else
                {
                    CustomerAccount account = new CustomerAccount(email, password);
                    accounts[email]        = account;
                    failedAttempts[email]  = 0;

                    result = "Registratie geslaagd.";
                }
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
                    failedAttempts[email] = 0;
                    result = "Inloggen geslaagd.";
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
                        result = "Inloggen mislukt.";
                    }
                }
            }

            return result;
        }
    }
}
