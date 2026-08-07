namespace ShopWave.Security
{
    public class UserRepository
    {
        private readonly Dictionary<string, string> users = new Dictionary<string, string>
        {
            { "alice@shopwave.be", "mijnWachtwoord123" },
            { "bob@shopwave.be",   "qwerty" }
        };

        public bool Login(string email, string password)
        {
            bool result = false;

            if (users.ContainsKey(email))
            {
                result = users[email] == password;
            }

            return result;
        }
    }
}
