using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShopWave.Security;

namespace ShopWave.Web.Pages
{
    // Les 7: laat zien dat een JWT wel ondertekend is, maar niet versleuteld. De payload
    // wordt hier gelezen zonder de geheime sleutel, precies zoals in de theorie.
    public class TokenModel : PageModel
    {
        private const string Issuer   = "shopwave-api";
        private const string Audience = "shopwave-client";

        [BindProperty]
        public string Email { get; set; } = "alice@shopwave.be";

        [BindProperty]
        public string Rol { get; set; } = "user";

        public string Token { get; private set; } = string.Empty;

        public string Header { get; private set; } = string.Empty;

        public string Payload { get; private set; } = string.Empty;

        public string Signature { get; private set; } = string.Empty;

        public List<Claim> Claims { get; private set; } = new List<Claim>();

        public DateTime VerlooptOp { get; private set; } = DateTime.MinValue;

        public string Melding { get; private set; } = string.Empty;

        public bool IsFout { get; private set; } = false;

        public bool HeeftToken { get; private set; } = false;

        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            string secretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY");

            if (string.IsNullOrEmpty(secretKey))
            {
                Melding = "Zet eerst de omgevingsvariabele JWT_SECRET_KEY. " +
                          "De sleutel hoort niet in de broncode te staan, dus de webshop " +
                          "kan zonder die variabele geen token aanmaken.";
                IsFout  = true;
            }
            else if (Email == string.Empty)
            {
                Melding = "E-mailadres is verplicht.";
                IsFout  = true;
            }
            else
            {
                JwtTokenService service = new JwtTokenService(secretKey, Issuer, Audience);

                Token = service.GenerateToken(Email, Rol);

                string[] delen = Token.Split('.');
                Header    = delen[0];
                Payload   = delen[1];
                Signature = delen[2];

                // ReadJwtToken leest de payload zonder de sleutel te gebruiken.
                JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
                JwtSecurityToken        gelezen = handler.ReadJwtToken(Token);

                Claims     = gelezen.Claims.ToList();
                VerlooptOp = gelezen.ValidTo;
                HeeftToken = true;
                Melding    = "Token aangemaakt.";
            }

            return Page();
        }
    }
}
