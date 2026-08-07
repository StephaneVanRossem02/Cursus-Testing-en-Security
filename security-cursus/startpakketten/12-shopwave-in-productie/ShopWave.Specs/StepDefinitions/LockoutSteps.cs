using Reqnroll;
using Xunit;

namespace ShopWave.Specs.StepDefinitions
{
    [Binding]
    public class LockoutSteps
    {
        private readonly LoginContext ctx;

        public LockoutSteps(LoginContext ctx)
        {
            this.ctx = ctx;
        }

        [When("de gebruiker drie keer inlogt met een fout wachtwoord")]
        public void WhenDrieKeerFoutWachtwoord()
        {
            ctx.AccountRepository.Login("bob@shopwave.be", "fout1");
            ctx.AccountRepository.Login("bob@shopwave.be", "fout2");
            ctx.AccountRepository.Login("bob@shopwave.be", "fout3");
        }

        [When("de gebruiker inlogt met het correcte wachtwoord {string}")]
        public void WhenInloggenMetCorrecteWachtwoord(string wachtwoord)
        {
            ctx.Result = ctx.AccountRepository.Login("bob@shopwave.be", wachtwoord);
        }

        [Then("is het account van {string} geblokkeerd")]
        public void ThenIsHetAccountGeblokkeerd(string email)
        {
            string result = ctx.AccountRepository.Login(email, "veiligPw");
            Assert.Equal("Account geblokkeerd.", result);
        }
    }
}
