namespace ShopWave.Security
{
    public class PasswordValidator
    {
        private const string SpecialCharacters = "!@#$%^&*";

        public bool IsValid(string password)
        {
            return GetErrorMessage(password) == string.Empty;
        }

        public string GetErrorMessage(string password)
        {
            string result = string.Empty;

            bool hasUppercase = false;
            bool hasDigit     = false;
            bool hasSpecial   = false;

            foreach (char c in password)
            {
                if (char.IsUpper(c))
                {
                    hasUppercase = true;
                }

                if (char.IsDigit(c))
                {
                    hasDigit = true;
                }

                if (SpecialCharacters.Contains(c))
                {
                    hasSpecial = true;
                }
            }

            if (password.Length < 8)
            {
                result = "Wachtwoord moet minstens 8 tekens lang zijn.";
            }
            else if (!hasUppercase)
            {
                result = "Wachtwoord moet minstens één hoofdletter bevatten.";
            }
            else if (!hasDigit)
            {
                result = "Wachtwoord moet minstens één cijfer bevatten.";
            }
            else if (!hasSpecial)
            {
                result = "Wachtwoord moet minstens één speciaal teken bevatten (!@#$%^&*).";
            }

            return result;
        }
    }
}
