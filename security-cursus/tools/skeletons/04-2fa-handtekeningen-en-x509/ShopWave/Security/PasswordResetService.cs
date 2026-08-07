using System.Security.Cryptography;

namespace ShopWave.Security
{
    // STARTCODE voor oefening 1 van les 4.
    public class PasswordResetService
    {
        private readonly Dictionary<string, PendingCode> pendingResets;

        public PasswordResetService()
        {
            pendingResets = new Dictionary<string, PendingCode>();
        }

        public void RequestReset(string email, Action<string, string> onCodeSent)
        {
            // jouw code hier
        }

        public bool VerifyCode(string email, string code)
        {
            // jouw code hier

            return false;
        }

        public string ResetPassword(
            string            email,
            string            code,
            string            newPassword,
            AccountRepository accounts)
        {
            // jouw code hier

            return string.Empty;
        }
    }
}
