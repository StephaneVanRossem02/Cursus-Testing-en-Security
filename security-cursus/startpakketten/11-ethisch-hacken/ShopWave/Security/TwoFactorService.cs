using System.Security.Cryptography;

namespace ShopWave.Security
{
    public class TwoFactorService
    {
        private readonly Dictionary<string, PendingCode> pendingCodes;
        private readonly Dictionary<string, int>         failedAttempts;
        private readonly int                             validitySeconds;
        private readonly Action<string, string>          onCodeGenerated;

        public TwoFactorService(int validitySeconds = 30)
        {
            pendingCodes          = new Dictionary<string, PendingCode>();
            failedAttempts        = new Dictionary<string, int>();
            this.validitySeconds  = validitySeconds;
            onCodeGenerated       = null;
        }

        public TwoFactorService(Action<string, string> onCodeGenerated, int validitySeconds = 30)
        {
            pendingCodes          = new Dictionary<string, PendingCode>();
            failedAttempts        = new Dictionary<string, int>();
            this.validitySeconds  = validitySeconds;
            this.onCodeGenerated  = onCodeGenerated;
        }

        public string GenerateCode(string email)
        {
            string   code      = RandomNumberGenerator.GetInt32(100000, 999999).ToString();
            DateTime expiresAt = DateTime.UtcNow.AddSeconds(validitySeconds);

            pendingCodes[email]   = new PendingCode(code, expiresAt);
            failedAttempts[email] = 0;

            if (onCodeGenerated != null)
            {
                onCodeGenerated(email, code);
            }

            return code;
        }

        public bool VerifyCode(string email, string code)
        {
            bool isValid = false;

            if (pendingCodes.ContainsKey(email))
            {
                PendingCode pending = pendingCodes[email];

                if (DateTime.UtcNow <= pending.ExpiresAt && pending.Code == code)
                {
                    isValid               = true;
                    failedAttempts[email] = 0;
                    pendingCodes.Remove(email);
                }
                else
                {
                    failedAttempts[email] = failedAttempts.ContainsKey(email)
                        ? failedAttempts[email] + 1
                        : 1;

                    if (failedAttempts[email] >= 3)
                    {
                        pendingCodes.Remove(email);
                    }
                }
            }

            return isValid;
        }

        public int GetRemainingAttempts(string email)
        {
            int result;

            if (!failedAttempts.ContainsKey(email))
            {
                result = 3;
            }
            else
            {
                int remaining = 3 - failedAttempts[email];
                result = remaining < 0 ? 0 : remaining;
            }

            return result;
        }
    }
}
