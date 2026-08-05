using Reqnroll;
using ShopWave.Security;
using Xunit;

namespace ShopWave.Specs.StepDefinitions
{
    [Binding]
    public class RegistratieSteps
    {
        private readonly LoginContext ctx;
        private          string       registratieResultaat = string.Empty;

        public RegistratieSteps(LoginContext ctx)
        {
            this.ctx = ctx;
        }

        [Given("er bestaat nog geen account voor {string}")]
        public void GivenGeenAccountVoor(string email)
        {
            ctx.TwoFactorService  = new TwoFactorService();
            ctx.AccountRepository = new AccountRepository(ctx.TwoFactorService);
        }

        [Given("er is al een account voor {string}")]
        public void GivenAccountBestaatAl(string email)
        {
            ctx.TwoFactorService  = new TwoFactorService();
            ctx.AccountRepository = new AccountRepository(ctx.TwoFactorService);
            ctx.AccountRepository.Register(email, "bestaandWachtwoord");
        }

        [When("de gebruiker zich registreert met e-mailadres {string} en wachtwoord {string}")]
        public void WhenRegistreer(string email, string wachtwoord)
        {
            registratieResultaat = ctx.AccountRepository.Register(email, wachtwoord);
        }

        [When("de gebruiker zich opnieuw registreert met hetzelfde e-mailadres {string}")]
        public void WhenHerregistreer(string email)
        {
            registratieResultaat = ctx.AccountRepository.Register(email, "nieuwPw");
        }

        [Then("is het account aangemaakt")]
        public void ThenAccountAangemaakt()
        {
            Assert.Equal("Registratie geslaagd.", registratieResultaat);
        }

        [Then("ontvangt de gebruiker de registratiefout {string}")]
        public void ThenRegistratieFout(string verwachteFout)
        {
            Assert.Equal(verwachteFout, registratieResultaat);
        }
    }
}
