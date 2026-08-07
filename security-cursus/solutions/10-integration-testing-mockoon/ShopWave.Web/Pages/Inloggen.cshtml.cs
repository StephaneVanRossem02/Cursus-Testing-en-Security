using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShopWave.Security;
using ShopWave.Web.Infrastructure;

namespace ShopWave.Web.Pages
{
    // Vanaf les 4 verloopt inloggen in twee stappen. Stap 1 controleert het wachtwoord
    // en laat TwoFactorService een code genereren. Stap 2 verifieert die code.
    public class InloggenModel : PageModel
    {
        private readonly AccountRepository accountRepository;
        private readonly DemoCodeHolder    codeHolder;

        public InloggenModel(AccountRepository accountRepository, DemoCodeHolder codeHolder)
        {
            this.accountRepository = accountRepository;
            this.codeHolder        = codeHolder;
        }

        [BindProperty]
        public string Email { get; set; } = string.Empty;

        [BindProperty]
        public string Wachtwoord { get; set; } = string.Empty;

        [BindProperty]
        public string Code { get; set; } = string.Empty;

        public string Melding { get; private set; } = string.Empty;

        public bool IsFout { get; private set; } = false;

        public bool WachtOpCode { get; private set; } = false;

        public string DemoCode { get; private set; } = string.Empty;

        public void OnGet()
        {
        }

        public IActionResult OnPostWachtwoord()
        {
            Melding = accountRepository.Login(Email, Wachtwoord);
            IsFout  = Melding != "Voer uw 2FA-code in.";

            if (!IsFout)
            {
                WachtOpCode = true;
                DemoCode    = codeHolder.GetCode(Email);
            }

            return Page();
        }

        public IActionResult OnPostCode()
        {
            Melding = accountRepository.VerifyTwoFactor(Email, Code);
            IsFout  = Melding != "Inloggen geslaagd.";

            if (IsFout)
            {
                WachtOpCode = true;
                DemoCode    = codeHolder.GetCode(Email);
            }

            return Page();
        }
    }
}
