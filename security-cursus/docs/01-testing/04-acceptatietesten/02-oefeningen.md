---
title: "Les 8: BDD / Acceptatietesten met Reqnroll — Oefeningen"
sidebar_label: "Oefeningen"
---

# Les 8: BDD / Acceptatietesten met Reqnroll — Oefeningen

> **Code-afspraken:** geen top-level statements · altijd `{}` · max één `return` · geen `break`/`continue` · geen underscore-prefix op parameters · geen geneste klassen · geen ternary/null-conditional · geen tuples · `double` i.p.v. `decimal` · identifiers Engels · tekst Nederlands

---

## Oefening 1 — 2FA-scenario

**Opgave:** Schrijf `TwoFactor.feature` en `TwoFactorSteps.cs` voor twee scenario's: succesvol inloggen inclusief 2FA, en 2FA-code is fout.

**ShopWave.Specs/Features/TwoFactor.feature**

```gherkin
Feature: Twee-factor authenticatie bij ShopWave

  Scenario: Succesvol inloggen met correcte 2FA-code
    Given er is een account voor "charlie@shopwave.be" met wachtwoord "pw123"
    When de gebruiker inlogt met het correcte wachtwoord voor "charlie@shopwave.be"
    And de gebruiker voert de correcte 2FA-code in voor "charlie@shopwave.be"
    Then is de gebruiker "charlie@shopwave.be" ingelogd

  Scenario: Inloggen met foute 2FA-code
    Given er is een account voor "charlie@shopwave.be" met wachtwoord "pw123"
    When de gebruiker inlogt met het correcte wachtwoord voor "charlie@shopwave.be"
    And de gebruiker voert een foute 2FA-code in voor "charlie@shopwave.be"
    Then ontvangt de gebruiker de melding "Ongeldige 2FA-code."
```

**ShopWave.Specs/LoginContext.cs**

```csharp
namespace ShopWave.Specs
{
    public class LoginContext
    {
        public AccountRepository AccountRepository { get; set; } = null!;
        public TwoFactorService  TwoFactorService  { get; set; } = null!;
        public string            LastCode          { get; set; } = string.Empty;
        public string            Result            { get; set; } = string.Empty;
    }
}
```

**ShopWave.Specs/StepDefinitions/CommonSteps.cs**

```csharp
using Reqnroll;
using ShopWave.Security;
using ShopWave.Specs;

namespace ShopWave.Specs.StepDefinitions
{
    [Binding]
    public class CommonSteps
    {
        private readonly LoginContext context;

        public CommonSteps(LoginContext context)
        {
            this.context = context;
        }

        [Given("er is een account voor {string} met wachtwoord {string}")]
        public void GivenErIsEenAccount(string email, string wachtwoord)
        {
            this.context.TwoFactorService = new TwoFactorService(
                onCodeGenerated: this.CaptureCode);

            this.context.AccountRepository = new AccountRepository(this.context.TwoFactorService);
            this.context.AccountRepository.Register(email, wachtwoord);
        }

        private void CaptureCode(string email, string code)
        {
            this.context.LastCode = code;
        }
    }
}
```

**ShopWave.Specs/StepDefinitions/TwoFactorSteps.cs**

```csharp
using Reqnroll;
using ShopWave.Specs;
using Xunit;

namespace ShopWave.Specs.StepDefinitions
{
    [Binding]
    public class TwoFactorSteps
    {
        private readonly LoginContext context;

        public TwoFactorSteps(LoginContext context)
        {
            this.context = context;
        }

        [When("de gebruiker inlogt met het correcte wachtwoord voor {string}")]
        public void WhenInloggenMetCorrecteWachtwoord(string email)
        {
            this.context.AccountRepository.Login(email, "pw123");
        }

        [When("de gebruiker voert de correcte 2FA-code in voor {string}")]
        public void WhenCorrecteTwoFactorCode(string email)
        {
            this.context.Result = this.context.AccountRepository
                .VerifyTwoFactor(email, this.context.LastCode);
        }

        [When("de gebruiker voert een foute 2FA-code in voor {string}")]
        public void WhenFouteTwoFactorCode(string email)
        {
            this.context.Result = this.context.AccountRepository
                .VerifyTwoFactor(email, "000000");
        }

        [Then("is de gebruiker {string} ingelogd")]
        public void ThenIsDeGebruikerIngelogd(string email)
        {
            Assert.Equal("Inloggen geslaagd.", this.context.Result);
        }

        [Then("ontvangt de gebruiker de melding {string}")]
        public void ThenOntvangtDeGebruikerMelding(string verwachteMelding)
        {
            Assert.Equal(verwachteMelding, this.context.Result);
        }
    }
}
```

---

## Oefening 3 — Registratie-feature

**Opgave:** `Registratie.feature` voor twee scenario's: registratie van nieuw account lukt, registratie van bestaand account geeft foutmelding.

**ShopWave.Specs/Features/Registratie.feature**

```gherkin
Feature: Registratie bij ShopWave

  Scenario: Registratie van een nieuw account
    Given er bestaat nog geen account voor "david@shopwave.be"
    When de gebruiker zich registreert met e-mailadres "david@shopwave.be" en wachtwoord "veiligPw99"
    Then is het account aangemaakt

  Scenario: Registratie van een bestaand account
    Given er is al een account voor "david@shopwave.be"
    When de gebruiker zich opnieuw registreert met hetzelfde e-mailadres "david@shopwave.be"
    Then ontvangt de gebruiker de registratiefout "Account bestaat al."
```

**ShopWave.Specs/StepDefinitions/RegistratieSteps.cs**

```csharp
using Reqnroll;
using ShopWave.Security;
using ShopWave.Specs;
using Xunit;

namespace ShopWave.Specs.StepDefinitions
{
    [Binding]
    public class RegistratieSteps
    {
        private readonly LoginContext context;
        private          string       registratieResultaat = string.Empty;

        public RegistratieSteps(LoginContext context)
        {
            this.context = context;
        }

        [Given("er bestaat nog geen account voor {string}")]
        public void GivenGeenAccountVoor(string email)
        {
            this.context.TwoFactorService  = new TwoFactorService();
            this.context.AccountRepository = new AccountRepository(this.context.TwoFactorService);
        }

        [Given("er is al een account voor {string}")]
        public void GivenAccountBestaatAl(string email)
        {
            this.context.TwoFactorService  = new TwoFactorService();
            this.context.AccountRepository = new AccountRepository(this.context.TwoFactorService);
            this.context.AccountRepository.Register(email, "bestaandWachtwoord");
        }

        [When("de gebruiker zich registreert met e-mailadres {string} en wachtwoord {string}")]
        public void WhenRegistreer(string email, string wachtwoord)
        {
            this.registratieResultaat = this.context.AccountRepository.Register(email, wachtwoord);
        }

        [When("de gebruiker zich opnieuw registreert met hetzelfde e-mailadres {string}")]
        public void WhenHerregistreer(string email)
        {
            this.registratieResultaat = this.context.AccountRepository.Register(email, "nieuwPw");
        }

        [Then("is het account aangemaakt")]
        public void ThenAccountAangemaakt()
        {
            Assert.Equal("Registratie geslaagd.", this.registratieResultaat);
        }

        [Then("ontvangt de gebruiker de registratiefout {string}")]
        public void ThenRegistratieFout(string verwachteFout)
        {
            Assert.Equal(verwachteFout, this.registratieResultaat);
        }
    }
}
```