using System.Security.Cryptography;

namespace ShopWave.Security
{
    // Dit is de klasse zoals je ze in de theorie van les 4 opbouwt (stap 2a tot 2c).
    // In oefening 2 breid je ze uit met een pogingenteller.
    public class TwoFactorService
    {
        private readonly Dictionary<string, PendingCode> pendingCodes;
        private readonly int                             validitySeconds;
        private readonly Action<string, string>          onCodeGenerated;

        public TwoFactorService(int validitySeconds = 30)
        {
            pendingCodes         = new Dictionary<string, PendingCode>();
            this.validitySeconds = validitySeconds;
            onCodeGenerated      = null;
        }

        public TwoFactorService(Action<string, string> onCodeGenerated, int validitySeconds = 30)
        {
            pendingCodes         = new Dictionary<string, PendingCode>();
            this.validitySeconds = validitySeconds;
            this.onCodeGenerated = onCodeGenerated;
        }

        public string GenerateCode(string email)
        {
            string   code      = RandomNumberGenerator.GetInt32(100000, 999999).ToString();
            DateTime expiresAt = DateTime.UtcNow.AddSeconds(validitySeconds);

            pendingCodes[email] = new PendingCode(code, expiresAt);

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
                    isValid = true;
                }

                pendingCodes.Remove(email);
            }

            return isValid;
        }
    }
}
