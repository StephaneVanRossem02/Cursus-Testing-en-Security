---
title: "Les 8: Oplossingen - Acceptatietesten"
sidebar_label: "Oplossingen"
---

# Oplossingen: Acceptatietesten

> [Download het volledige ShopWave-project van les 8](/downloads/shopwave-08-acceptatietesten.zip) (ZIP). Bevat alle code tot en met deze les, klaar om te bouwen en te testen.

**Bekijk dit pas nadat je de oefeningen zelf geprobeerd hebt.** De waarde van BDD zit in het zelf schrijven van scenario's en het ontdekken van ambiguïteiten in de specificatie. Lees de toelichting ook als je het juist had.

---

## Oplossing 1: Scenario Outline voor de loginflow

### Login.feature

```gherkin
Feature: Inloggen bij ShopWave

  Scenario Outline: Inloggen met verschillende wachtwoorden
    Given er is een account voor "alice@shopwave.be" met wachtwoord "wachtwoord123"
    When de gebruiker inlogt met "alice@shopwave.be" en "<wachtwoord>"
    Then ontvangt de gebruiker de melding "<melding>"

    Examples:
      | wachtwoord     | melding                  |
      | wachtwoord123  | Voer uw 2FA-code in.     |
      | foutWachtwoord | Ongeldig wachtwoord.     |
      |                | Ongeldig wachtwoord.     |
```

### Toelichting

De twee afzonderlijke scenario's zijn vervangen door één sjabloon. De derde rij heeft een leeg wachtwoord. In Gherkin schrijf je een lege waarde als een lege cel in de tabel.

**Veelgemaakte fout:** studenten passen de step definitions aan om de `Scenario Outline` te laten werken. Dat is niet nodig. De step definitions van de twee afzonderlijke scenario's werken ongewijzigd, omdat de `{string}`-placeholder ook een lege string accepteert.

**Veelgemaakte fout:** studenten vergeten dat de placeholders in de stapregel (`<wachtwoord>`, `<melding>`) exact overeen moeten komen met de kolomnamen in de `Examples`-tabel, inclusief kleine letters en spaties.

---

## Oplossing 2: Lockout-feature

### Lockout.feature

```gherkin
Feature: Account lockout bij ShopWave

  Scenario: Account vergrendeld na drie foute pogingen
    Given er is een account voor "bob@shopwave.be" met wachtwoord "veiligPw"
    When de gebruiker drie keer inlogt met een fout wachtwoord
    Then is het account van "bob@shopwave.be" geblokkeerd

  Scenario: Na blokkering werkt ook het correcte wachtwoord niet meer
    Given er is een account voor "bob@shopwave.be" met wachtwoord "veiligPw"
    When de gebruiker drie keer inlogt met een fout wachtwoord
    And de gebruiker inlogt met het correcte wachtwoord "veiligPw"
    Then ontvangt de gebruiker de melding "Account geblokkeerd."
```

### LockoutSteps.cs

```csharp
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
```

### Toelichting

De `Given`-stap staat in `CommonSteps.cs`. `LockoutSteps.cs` bevat geen `[Given]`-methode. Reqnroll injecteert dezelfde `LoginContext`-instantie in zowel `CommonSteps` als `LockoutSteps`, zodat de `AccountRepository` die in `CommonSteps` aangemaakt wordt, beschikbaar is in `LockoutSteps`.

**Veelgemaakte fout:** studenten definiëren "er is een account voor..." opnieuw in `LockoutSteps.cs`. Dat geeft een "Ambiguous step definition"-fout. De stap staat al in `CommonSteps.cs` en is beschikbaar voor alle features.

**Veelgemaakte fout:** de `Then`-stap in scenario 1 roept `Login` aan om te controleren of het account geblokkeerd is. Studenten vergeten dat ze het resultaat van die aanroep moeten controleren. Ze schrijven `ctx.AccountRepository.Login(email, "veiligPw");` zonder `Assert`. De test slaagt dan altijd, ook als het account niet geblokkeerd is.

**Veelgemaakte fout:** in scenario 2 staat de `Then`-stap "ontvangt de gebruiker de melding". Die is al gedefinieerd in `LoginSteps.cs`. Je hoeft die niet opnieuw te definiëren in `LockoutSteps.cs`. Reqnroll zoekt over alle `[Binding]`-klassen naar het stap-patroon en vindt het in `LoginSteps.cs`.

---

## Oplossing 3: Registratie-feature

### Registratie.feature

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

### RegistratieSteps.cs

```csharp
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
```

### Toelichting

De `Given`-stappen "er bestaat nog geen account voor..." en "er is al een account voor..." zijn apart gedefinieerd in `RegistratieSteps.cs`. Ze zijn bewust anders dan "er is een account voor... met wachtwoord..." uit `CommonSteps.cs`. Die verschil is intentioneel: de registratiescenario's beginnen vanuit een andere beginsituatie.

**Veelgemaakte fout:** studenten bewaren `registratieResultaat` in `LoginContext.Result`. Dat werkt technisch, maar mengt de toestand van de registratieflow met die van de loginflow. Als je daarna een scenario combineert dat eerst registreert en daarna inlogt, overschrijft `ctx.Result` de registratiestatus. Gebruik een lokale field in `RegistratieSteps` voor het registratieresultaat.

**Veelgemaakte fout:** studenten hergebruiken de `Then`-stap voor loginmeldingen uit `LoginSteps.cs` ook voor de registratiefout. Dat werkt technisch, maar het is verwarrend: de stap-tekst suggereert een loginmelding. Schrijf een aparte `Then`-stap met een duidelijke naam voor de registratiecontext.

**Reflectievraag 1:** unit tests voor `AccountRepository.Login` en Gherkin-scenario's testen beide de loginflow, maar vanuit een ander perspectief. De unit test test de technische correctheid van de methode in isolatie. Het Gherkin-scenario beschrijft de vereiste vanuit gebruikersperspectief en legt vast wat het systeem belooft. Als de unit test slaagt maar het scenario faalt, is er een discrepantie tussen de technische implementatie en de gespecificeerde vereiste.

---

## Oplossing 4: 2FA-flow als Scenario Outline

### TwoFactor.feature

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

  Scenario Outline: 2FA-verificatie met verschillende codes
    Given er is een account voor "charlie@shopwave.be" met wachtwoord "pw123"
    When de gebruiker inlogt met het correcte wachtwoord voor "charlie@shopwave.be"
    And de gebruiker voert de 2FA-code "<type>" in voor "charlie@shopwave.be"
    Then ontvangt de gebruiker het resultaat "<resultaat>"

    Examples:
      | type    | resultaat            |
      | correct | Inloggen geslaagd.   |
      | fout    | Ongeldige 2FA-code.  |
```

### TwoFactorSteps.cs

```csharp
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
```

### Toelichting

De uitdaging bij de `Scenario Outline` is dat de twee scenario's verschillende `Then`-stappen hebben. De oplossing is een gecombineerde stap met een string-placeholder, hier "ontvangt de gebruiker het resultaat", die voor beide gevallen werkt. In de `Examples`-tabel staan dan de verwachte resultaten per rij.

De `When`-stap voor de 2FA-code vertaalt het type ("correct" of "fout") naar de echte code. Als het type "correct" is, gebruiken we `ctx.LastCode`. Als het type "fout" is, gebruiken we een bekende foute code.

**Veelgemaakte fout:** studenten proberen de `Then`-stap "is de gebruiker ingelogd" en "ontvangt de gebruiker de melding" te combineren in één `Scenario Outline`. Dat kan niet rechtstreeks als de stap-teksten fundamenteel verschillen. De oplossing is altijd een nieuwe, abstractere stap schrijven die beide gevallen dekt.

**Veelgemaakte fout:** studenten vergeten dat `ctx.LastCode` alleen gevuld is als de callback in `CommonSteps.cs` opgeroepen is. Die callback wordt opgeroepen op het moment van `Login(...)`. Als je `VerifyTwoFactor` aanroept zonder eerst `Login` aan te roepen, is `LastCode` leeg.

**Reflectievraag 3:** `Login(...)` geeft "Voer uw 2FA-code in." terug als het wachtwoord correct is. Die returnwaarde is de melding aan de gebruiker, niet de 2FA-code zelf. De 2FA-code wordt intern gegenereerd door `TwoFactorService` en naar de gebruiker gestuurd via een apart kanaal (in een echte applicatie: e-mail of sms). De callback-techniek laat ons die code opvangen zonder de interne implementatie te wijzigen.

**Reflectievraag 4:** een `Scenario Outline` is beter als meerdere gevallen precies hetzelfde patroon volgen met verschillende data, zoals grenswaarden of equivalentieklassen. Afzonderlijke scenario's zijn beter als elk geval een eigen context of eigen stap-volgorde heeft, of als de leesbaarheid van het scenario belangrijker is dan de beknoptheid.
