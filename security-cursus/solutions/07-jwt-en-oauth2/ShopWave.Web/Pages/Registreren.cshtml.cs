using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShopWave.Security;

namespace ShopWave.Web.Pages
{
    // Registratie via AccountRepository uit les 2. De wachtwoordregels komen uit
    // PasswordValidator; de pagina toont alleen wat die klassen teruggeven.
    public class RegistrerenModel : PageModel
    {
        private readonly AccountRepository accountRepository;

        public RegistrerenModel(AccountRepository accountRepository)
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
            Melding = accountRepository.Register(Email, Wachtwoord);
            IsFout  = Melding != "Registratie geslaagd.";
        }
    }
}
