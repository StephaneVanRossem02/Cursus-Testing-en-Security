using Reqnroll;
using ShopWave.Security;

namespace ShopWave.Specs.StepDefinitions
{
    [Binding]
    public class CommonSteps
    {
        private readonly LoginContext ctx;

        public CommonSteps(LoginContext ctx)
        {
            this.ctx = ctx;
        }

        [Given("er is een account voor {string} met wachtwoord {string}")]
        public void GivenErIsEenAccount(string email, string wachtwoord)
        {
            ctx.TwoFactorService = new TwoFactorService(
                onCodeGenerated: (mail, code) => { ctx.LastCode = code; });

            ctx.AccountRepository = new AccountRepository(ctx.TwoFactorService);
            ctx.AccountRepository.Register(email, wachtwoord);
        }
    }
}
