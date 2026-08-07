using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShopWave.Security;

namespace ShopWave.Web.Pages
{
    // Inloggen via AccountRepository uit les 2. Na drie foute pogingen blokkeert de
    // repository het account; de pagina toont enkel de melding die ze teruggeeft.
    public class InloggenModel : PageModel
    {
        private readonly AccountRepository accountRepository;

        public InloggenModel(AccountRepository accountRepository)
        {
            this.accountRepository = accountRepository;
        }

        [BindProperty]
        public string Email { get; set; } = string.Empty;

        [BindProperty]
        public string Wachtwoord { get; set; } = string.Empty;

        public string Melding { get; private set; } = string.Empty;

        public bool IsFout { get; private set; } = false;

        public void OnGet()
        {
        }

        public void OnPost()
        {
            Melding = accountRepository.Login(Email, Wachtwoord);
            IsFout  = Melding != "Inloggen geslaagd.";
        }
    }
}
