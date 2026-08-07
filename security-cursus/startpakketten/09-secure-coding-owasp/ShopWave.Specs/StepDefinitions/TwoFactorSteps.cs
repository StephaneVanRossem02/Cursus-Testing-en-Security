using Reqnroll;
using Xunit;

namespace ShopWave.Specs.StepDefinitions
{
    [Binding]
    public class TwoFactorSteps
    {
        private readonly LoginContext ctx;

        public TwoFactorSteps(LoginContext ctx)
        {
            this.ctx = ctx;
        }

        [When("de gebruiker inlogt met het correcte wachtwoord voor {string}")]
        public void WhenInloggenMetCorrecteWachtwoord(string email)
        {
            ctx.AccountRepository.Login(email, "pw123");
        }

        [When("de gebruiker voert de correcte 2FA-code in voor {string}")]
        public void WhenCorrecteTwoFactorCode(string email)
        {
            ctx.Result = ctx.AccountRepository.VerifyTwoFactor(email, ctx.LastCode);
        }

        [When("de gebruiker voert een foute 2FA-code in voor {string}")]
        public void WhenFouteTwoFactorCode(string email)
        {
            ctx.Result = ctx.AccountRepository.VerifyTwoFactor(email, "000000");
        }

        [When("de gebruiker voert de 2FA-code {string} in voor {string}")]
        public void WhenTwoFactorCodeType(string type, string email)
        {
            string code = type == "correct" ? ctx.LastCode : "000000";
            ctx.Result = ctx.AccountRepository.VerifyTwoFactor(email, code);
        }

        [Then("is de gebruiker {string} ingelogd")]
        public void ThenIsDeGebruikerIngelogd(string email)
        {
            Assert.Equal("Inloggen geslaagd.", ctx.Result);
        }

        [Then("ontvangt de gebruiker het resultaat {string}")]
        public void ThenOntvangtDeGebruikerHetResultaat(string verwacht)
        {
            Assert.Equal(verwacht, ctx.Result);
        }
    }
}
