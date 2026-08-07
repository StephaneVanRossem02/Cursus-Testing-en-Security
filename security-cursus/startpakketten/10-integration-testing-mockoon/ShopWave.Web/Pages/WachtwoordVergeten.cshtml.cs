using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShopWave.Security;
using ShopWave.Web.Infrastructure;

namespace ShopWave.Web.Pages
{
    // Wachtwoordreset via PasswordResetService uit les 4. De resetcode is 15 minuten
    // geldig en werkt maar een keer.
    public class WachtwoordVergetenModel : PageModel
    {
        private readonly PasswordResetService resetService;
        private readonly AccountRepository    accountRepository;
        private readonly DemoCodeHolder       codeHolder;

        public WachtwoordVergetenModel(
            PasswordResetService resetService,
            AccountRepository    accountRepository,
            DemoCodeHolder       codeHolder)
        {
            this.resetService      = resetService;
            this.accountRepository = accountRepository;
            this.codeHolder        = codeHolder;
        }

        [BindProperty]
        public string Email { get; set; } = string.Empty;

        [BindProperty]
        public string Code { get; set; } = string.Empty;

        [BindProperty]
        public string NieuwWachtwoord { get; set; } = string.Empty;

        public string Melding { get; private set; } = string.Empty;

        public bool IsFout { get; private set; } = false;

        public bool WachtOpCode { get; private set; } = false;

        public string DemoCode { get; private set; } = string.Empty;

        public void OnGet()
        {
        }

        public IActionResult OnPostAanvragen()
        {
            if (Email == string.Empty)
            {
                Melding = "E-mailadres is verplicht.";
                IsFout  = true;
            }
            else
            {
                resetService.RequestReset(Email, (mail, code) => codeHolder.Store(mail, code));

                Melding     = "Er is een resetcode aangemaakt.";
                WachtOpCode = true;
                DemoCode    = codeHolder.GetCode(Email);
            }

            return Page();
        }

        public IActionResult OnPostResetten()
        {
            Melding = resetService.ResetPassword(Email, Code, NieuwWachtwoord, accountRepository);
            IsFout  = Melding != "Wachtwoord gewijzigd.";

            if (IsFout)
            {
                WachtOpCode = true;
            }

            return Page();
        }
    }
}
