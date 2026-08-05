using System.Security.Cryptography;

namespace ShopWave.Security
{
    public class PasswordResetService
    {
        private readonly Dictionary<string, PendingCode> pendingResets;

        public PasswordResetService()
        {
            pendingResets = new Dictionary<string, PendingCode>();
        }

        public void RequestReset(string email, Action<string, string> onCodeSent)
        {
            string   code      = RandomNumberGenerator.GetInt32(100000, 999999).ToString();
            DateTime expiresAt = DateTime.UtcNow.AddMinutes(15);

            pendingResets[email] = new PendingCode(code, expiresAt);

            onCodeSent(email, code);
        }

        public bool VerifyCode(string email, string code)
        {
            bool isValid = false;

            if (pendingResets.ContainsKey(email))
            {
                PendingCode pending = pendingResets[email];

                if (DateTime.UtcNow <= pending.ExpiresAt && pending.Code == code)
                {
                    isValid = true;
                }

                pendingResets.Remove(email);
            }

            return isValid;
        }

        public string ResetPassword(
            string            email,
            string            code,
            string            newPassword,
            AccountRepository accounts)
        {
            string result;

            if (VerifyCode(email, code))
            {
                accounts.ChangePassword(email, newPassword);
                result = "Wachtwoord gewijzigd.";
            }
            else
            {
                result = "Ongeldige of verlopen code.";
            }

            return result;
        }
    }
}
