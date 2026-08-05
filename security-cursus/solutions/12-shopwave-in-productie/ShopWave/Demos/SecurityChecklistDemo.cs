using ShopWave.Security;

namespace ShopWave
{
    // Demo uit de theorie: de volledige deployment-checklist voor ShopWave doorlopen en printen.
    public static class SecurityChecklistDemo
    {
        public static void Run()
        {
            SecurityChecklist checklist = new SecurityChecklist();

            checklist.AddItem("Auth",    "Passwords hashed with BCrypt");
            checklist.AddItem("Auth",    "2FA active");
            checklist.AddItem("Auth",    "JWT with expiry");
            checklist.AddItem("Auth",    "Account lockout after 3 failed attempts");
            checklist.AddItem("Auth",    "Rate limiting on login endpoint");
            checklist.AddItem("Data",    "AES-256 encryption with random IV");
            checklist.AddItem("Data",    "No plaintext passwords in logs");
            checklist.AddItem("Network", "HTTPS active");
            checklist.AddItem("Network", "HSTS configured");
            checklist.AddItem("Network", "CORS restricted to known origins");
            checklist.AddItem("Network", "Swagger disabled in production");
            checklist.AddItem("Config",  "JWT_SECRET_KEY via environment variable");
            checklist.AddItem("Config",  "Developer Exception Page off in production");
            checklist.AddItem("Deps",    "No critical or high vulnerable packages");

            checklist.SetStatus("Passwords hashed with BCrypt",          "Implemented", "BCrypt.Net-Next via PasswordHasher");
            checklist.SetStatus("2FA active",                            "Implemented", "TwoFactorService with callback");
            checklist.SetStatus("JWT with expiry",                       "Implemented", "60 minutes via JwtTokenService");
            checklist.SetStatus("Account lockout after 3 failed attempts","Implemented","AccountRepository.Login()");
            checklist.SetStatus("Rate limiting on login endpoint",       "Implemented", "FixedWindowLimiter, 5 per minute");
            checklist.SetStatus("AES-256 encryption with random IV",     "Implemented", "AesEncryptor");
            checklist.SetStatus("No plaintext passwords in logs",        "Implemented", "BCrypt, nooit plain-text gelogd");
            checklist.SetStatus("HTTPS active",                          "Implemented", "UseHttpsRedirection + Kestrel");
            checklist.SetStatus("HSTS configured",                       "Implemented", "UseHsts()");
            checklist.SetStatus("CORS restricted to known origins",      "Implemented", "WithOrigins(allowedOrigins)");
            checklist.SetStatus("Swagger disabled in production",        "Implemented", "IsDevelopment()-check");
            checklist.SetStatus("JWT_SECRET_KEY via environment variable","Implemented","GetEnvironmentVariable");
            checklist.SetStatus("Developer Exception Page off in production","Implemented","IsDevelopment()-check");
            checklist.SetStatus("No critical or high vulnerable packages","Partial",    "Handmatig gecontroleerd, nog geen CI");

            checklist.PrintReport();
        }
    }
}
