using Reqnroll;
using Xunit;

namespace ShopWave.Specs.StepDefinitions
{
    [Binding]
    public class LoginSteps
    {
        private readonly LoginContext ctx;

        public LoginSteps(LoginContext ctx)
        {
            this.ctx = ctx;
        }

        [When("de gebruiker inlogt met {string} en {string}")]
        public void WhenDeGebruikerInlogt(string email, string wachtwoord)
        {
            ctx.Result = ctx.AccountRepository.Login(email, wachtwoord);
        }

        [Then("ontvangt de gebruiker de melding {string}")]
        public void ThenOntvangtDeGebruikerDeMelding(string verwachteMelding)
        {
            Assert.Equal(verwachteMelding, ctx.Result);
        }
    }
}
