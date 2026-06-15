---
title: "Les 8: Theorie - Acceptatietesten"
sidebar_label: "Theorie"
---

# Theorie: Acceptatietesten

## 1. Waarom acceptatietesten?

In les 1 bewezen we dat `DiscountCalculator.Apply` de juiste korting berekent. In les 5 bewezen we dat `CartService`, `CouponService` en `OrderService` correct samenwerken. Maar geen van die tests stelt de vraag die de klant stelt.

De klant vraagt niet: "werkt de `Apply`-methode correct?" De klant vraagt: "als ik een coupon invoer bij het afrekenen, krijg ik dan de juiste korting op mijn totaal?" Dat is een ander niveau van vraag. Het gaat over het volledige scenario vanuit de buitenkant, zonder enige kennis van de interne code.

**Unit tests en integration tests kijken van binnen naar buiten.** Ze kennen de klassen, de methoden en de interfaces. Een acceptatietest kijkt van buiten naar binnen. Die ziet alleen wat een gebruiker ziet: ik doe iets, ik verwacht dit resultaat.

Drie dingen die unit tests en integration tests niet beantwoorden:

- "Doet het systeem wat de klant heeft gevraagd in de specificatie?"
- "Begrijpen de ontwikkelaars en de klant hetzelfde onder 'succesvol inloggen'?"
- "Welke scenario's zijn al getest vanuit gebruikersperspectief?"

Acceptatietesten lossen dit op. Ze zijn geschreven in gewone taal. De klant, de productmanager en de ontwikkelaar kunnen ze allemaal lezen en bevestigen dat ze kloppen.

**Mini-controle:** je hebt 50 unit tests en 10 integration tests voor de loginflow. Alle tests slagen. Kan je hieruit concluderen dat het systeem doet wat de klant gevraagd heeft? Nee. Unit tests en integration tests bewijzen dat de technische implementatie correct is. Ze bewijzen niet dat de specificatie correct geïmplementeerd is.

---

## 2. Wat is BDD?

**Behavior Driven Development (BDD)** is een aanpak waarbij de specificaties van het systeem geschreven worden als voorbeelden van gedrag.

Niet: "het systeem slaat een gehashed wachtwoord op na registratie."
Wel: "als een gebruiker zich registreert met een e-mailadres en wachtwoord, kan die gebruiker daarna inloggen met die gegevens."

Het verschil is het perspectief. De technische beschrijving praat over implementatie. De BDD-beschrijving praat over gedrag vanuit de buitenkant.

BDD heeft drie voordelen:

- **Gedeeld begrip.** Klant, tester en ontwikkelaar schrijven de scenario's samen. Misverstanden over de vereisten komen eerder aan de oppervlakte.
- **Levende documentatie.** De scenario's zijn tegelijk de specificatie en de automatische test. Als de code verandert en de test faalt, is de documentatie verouderd.
- **Gebruikersperspectief.** Tests beschrijven wat het systeem belooft aan de gebruiker, niet hoe het intern werkt.

**Mini-controle:** een collega zegt "BDD is gewoon een andere manier om tests te schrijven." Wat klopt er niet aan die uitspraak? BDD gaat niet in de eerste plaats over tests. Het gaat over communicatie en gedeelde specificaties. De tests zijn een bijproduct van die specificaties.

---

## 3. Gherkin: de taal voor scenario's

**Gherkin** is de taal die BDD-scenario's beschrijft. Ze is bewust eenvoudig gehouden zodat ook niet-programmeurs ze kunnen lezen en schrijven.

Gherkin gebruikt een vaste structuur:

```gherkin
Feature: naam van het onderdeel dat je beschrijft

  Scenario: beschrijving van één specifieke situatie
    Given  de beginsituatie (wat is er al?)
    When   de actie die de gebruiker of het systeem uitvoert
    Then   het verwachte resultaat
```

Een concreet voorbeeld voor ShopWave:

```gherkin
Feature: Inloggen bij ShopWave

  Scenario: Succesvol inloggen met correct wachtwoord
    Given er is een account voor "alice@shopwave.be" met wachtwoord "wachtwoord123"
    When de gebruiker inlogt met "alice@shopwave.be" en "wachtwoord123"
    Then ontvangt de gebruiker de melding "Voer uw 2FA-code in."
```

`Given` beschrijft de beginsituatie: er moet een account bestaan. `When` beschrijft de actie: de gebruiker probeert in te loggen. `Then` beschrijft het verwachte resultaat: de melding die het systeem geeft.

**Extra keywords:**

| Keyword | Gebruik |
|---------|---------|
| `And` | Extra stap toevoegen na `Given`, `When` of `Then` |
| `But` | Variatie op `And`, gebruikt voor uitsluitingen |
| `Background` | Herhaalde `Given`-stappen die voor elk scenario in de feature gelden |
| `Scenario Outline` | Sjabloon-scenario met een `Examples`-tabel |

**Gherkin is geen code.** De computer voert de tekst niet rechtstreeks uit. Een apart C#-bestand met step definitions koppelt elke regel aan uitvoerbare code.

**Mini-controle:** schrijf een Gherkin-scenario voor "inloggen met fout wachtwoord". Welke drie keywords gebruik je en wat staat er onder elk keyword?

---

## 4. De drie lagen van een Reqnroll-project

**SpecFlow** was het populaire BDD-framework voor .NET. Het is intussen gestopt met actieve ontwikkeling. **Reqnroll** is de open-source opvolger, gebouwd op dezelfde codebase. De syntax is nagenoeg identiek.

Een Reqnroll-project bestaat uit drie lagen:

```
Feature file (.feature)      Gherkin-scenario's
                             Leesbaar voor iedereen
                                     |
                                     v
Step definitions (.cs)       C#-methoden die elke Gherkin-stap uitvoeren
                             Koppeling tussen taal en code
                                     |
                                     v
Productieklassen             De echte code die getest wordt
                             CartService, OrderService, AccountRepository...
```

De feature file beschrijft **wat** het systeem moet doen. De step definitions beschrijven **hoe** dat getest wordt. De productieklassen zijn de **echte code** die draait.

### Setup

**Stap 1: Reqnroll-extensie installeren**

Ga in Visual Studio naar `Extensions` > `Manage Extensions` > zoek op `Reqnroll` > installeer `Reqnroll for Visual Studio 2022 and 2026` > herstart Visual Studio.

**Stap 2: Reqnroll-project aanmaken**

Rechtsklik op de solution > `Add` > `New Project` > zoek op `Reqnroll` > kies `Reqnroll Project` > naam: `ShopWave.Specs` > kies `xUnit` als testframework.

De solution ziet er nu zo uit:

```
ShopWave           Productieklassen
ShopWave.Tests     xUnit unit tests en integration tests
ShopWave.Specs     Reqnroll acceptatietests (nieuw)
```

**Stap 3: Project reference toevoegen**

Rechtsklik op `ShopWave.Specs` > `Add` > `Project Reference` > vink `ShopWave` aan.

**Stap 4: Voorbeeldbestanden verwijderen**

Reqnroll maakt automatisch een voorbeeldfeature en een voorbeeldstepfile aan. Verwijder die. We schrijven alles zelf.

**Stap 5: Feature file aanmaken**

Rechtsklik op `ShopWave.Specs` > `Add` > `New Item` > `Reqnroll Feature File` > naam: `Login.feature`.

**Mini-controle:** je hebt Reqnroll geïnstalleerd maar ziet geen `Reqnroll Feature File` in het "Add New Item"-menu. Wat heb je vergeten? De Visual Studio-extensie installeren en herstarten.

---

## 5. Step definitions: Gherkin koppelen aan C#

Een step definition is een C#-methode die aan één Gherkin-stap gekoppeld is via een attribuut.

Maak `ShopWave.Specs/StepDefinitions/LoginSteps.cs` aan:

```csharp
using Reqnroll;
using ShopWave.Security;
using Xunit;

namespace ShopWave.Specs.StepDefinitions
{
    [Binding]
    public class LoginSteps
    {
        private AccountRepository _accountRepository = null!;
        private string            _result            = string.Empty;

        [Given("er is een account voor {string} met wachtwoord {string}")]
        public void GivenErIsEenAccount(string email, string wachtwoord)
        {
            _accountRepository = new AccountRepository(new TwoFactorService());
            _accountRepository.Register(email, wachtwoord);
        }

        [When("de gebruiker inlogt met {string} en {string}")]
        public void WhenDeGebruikerInlogt(string email, string wachtwoord)
        {
            _result = _accountRepository.Login(email, wachtwoord);
        }

        [Then("ontvangt de gebruiker de melding {string}")]
        public void ThenOntvangtDeGebruikerDeMelding(string verwachteMelding)
        {
            Assert.Equal(verwachteMelding, _result);
        }
    }
}
```

**`[Binding]`** vertelt Reqnroll dat deze klasse step definitions bevat. Zonder dit attribuut worden de methoden genegeerd.

**`{string}`** is een Reqnroll-placeholder. De waarde tussen aanhalingstekens in de feature file ("alice@shopwave.be") wordt automatisch doorgegeven als parameter. Andere placeholders zijn `{int}`, `{double}` en `{decimal}`.

**`Assert.Equal`** is de xUnit-assert die we al kennen. De verwachte waarde komt uit de feature file. De echte waarde komt van de methode die we aanroepen in de `When`-stap.

Feature file voor de demo:

```gherkin
Feature: Inloggen bij ShopWave

  Scenario: Succesvol inloggen met correct wachtwoord
    Given er is een account voor "alice@shopwave.be" met wachtwoord "wachtwoord123"
    When de gebruiker inlogt met "alice@shopwave.be" en "wachtwoord123"
    Then ontvangt de gebruiker de melding "Voer uw 2FA-code in."

  Scenario: Inloggen met fout wachtwoord
    Given er is een account voor "alice@shopwave.be" met wachtwoord "wachtwoord123"
    When de gebruiker inlogt met "alice@shopwave.be" en "foutWachtwoord"
    Then ontvangt de gebruiker de melding "Ongeldig wachtwoord."
```

Build de solution en open de Test Explorer. Je ziet twee tests onder `ShopWave.Specs`, een per scenario. De namen komen rechtstreeks uit de feature file.

**Mini-controle:** je voegt een derde scenario toe aan de feature file maar de test verschijnt niet in de Test Explorer. Je hebt de step definition al geschreven. Wat is de meest waarschijnlijke oorzaak? De solution is niet opnieuw gebuild na het toevoegen van het scenario.

---

## 6. De context-klasse: gedeelde toestand via DI

Stel dat je een tweede feature wil toevoegen voor lockout. Je maakt `LockoutSteps.cs` aan en wil de `Given`-stap hergebruiken die het account aanmaakt. Maar als je dezelfde `[Given("er is een account voor...")]` in zowel `LoginSteps.cs` als `LockoutSteps.cs` definieert, gooit Reqnroll een **"Ambiguous step definition"**-fout. Reqnroll zoekt over alle `[Binding]`-klassen naar stap-patronen. Als hetzelfde patroon twee keer voorkomt, weet het niet welke methode het moet aanroepen.

De oplossing is een **context-klasse** als gedeelde toestand, gecombineerd met **Reqnroll Dependency Injection**.

Maak `ShopWave.Specs/LoginContext.cs` aan:

```csharp
using ShopWave.Security;

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

Reqnroll injecteert automatisch dezelfde `LoginContext`-instantie in elke `[Binding]`-klasse die die context als constructor-parameter accepteert. Geen handmatige registratie vereist.

**Mini-controle:** waarom bewaren we `Result` in de context-klasse en niet als private field in de step definition-klasse? Omdat `Then`-stappen in een andere klasse kunnen staan dan `When`-stappen. Als `Result` in `LoginSteps` staat en de `Then`-stap staat in `LockoutSteps`, heeft die geen toegang tot de waarde. De context-klasse is gedeeld tussen alle step definition-klassen binnen hetzelfde scenario.

---

## 7. CommonSteps: dubbele definities voorkomen

Zet gedeelde stappen die door meerdere features gebruikt worden in één aparte klasse `CommonSteps.cs`. Die klasse krijgt de context-klasse via de constructor.

Maak `ShopWave.Specs/StepDefinitions/CommonSteps.cs` aan:

```csharp
using Reqnroll;
using ShopWave.Security;

namespace ShopWave.Specs.StepDefinitions
{
    [Binding]
    public class CommonSteps
    {
        private readonly LoginContext _ctx;

        public CommonSteps(LoginContext ctx)
        {
            _ctx = ctx;
        }

        [Given("er is een account voor {string} met wachtwoord {string}")]
        public void GivenErIsEenAccount(string email, string wachtwoord)
        {
            _ctx.TwoFactorService = new TwoFactorService(
                onCodeGenerated: (mail, code) => { _ctx.LastCode = code; });

            _ctx.AccountRepository = new AccountRepository(_ctx.TwoFactorService);
            _ctx.AccountRepository.Register(email, wachtwoord);
        }
    }
}
```

De `TwoFactorService` wordt aangemaakt met de callback-techniek uit les 5. De gegenereerde 2FA-code wordt opgeslagen in `_ctx.LastCode`. Andere step definition-klassen kunnen die waarde dan ophalen zonder dat ze de `Given`-stap hoeven te dupliceren.

Pas daarna `LoginSteps.cs` aan zodat die de context gebruikt:

```csharp
using Reqnroll;
using Xunit;

namespace ShopWave.Specs.StepDefinitions
{
    [Binding]
    public class LoginSteps
    {
        private readonly LoginContext _ctx;

        public LoginSteps(LoginContext ctx)
        {
            _ctx = ctx;
        }

        [When("de gebruiker inlogt met {string} en {string}")]
        public void WhenDeGebruikerInlogt(string email, string wachtwoord)
        {
            _ctx.Result = _ctx.AccountRepository.Login(email, wachtwoord);
        }

        [Then("ontvangt de gebruiker de melding {string}")]
        public void ThenOntvangtDeGebruikerDeMelding(string verwachteMelding)
        {
            Assert.Equal(verwachteMelding, _ctx.Result);
        }
    }
}
```

De `Given`-stap is verdwenen uit `LoginSteps.cs`. Die staat nu alleen in `CommonSteps.cs` en is beschikbaar voor elke feature.

**Mini-controle:** je hebt `CommonSteps.cs` aangemaakt maar je krijgt nog steeds een "Ambiguous step definition"-fout. Wat is de oorzaak? De oude `[Given]`-methode staat nog in `LoginSteps.cs`. Je moet die verwijderen.

---

## 8. Scenario Outline: meerdere gevallen in één sjabloon

Als je dezelfde stappen wil herhalen met verschillende invoerwaarden, gebruik je een `Scenario Outline`. In plaats van drie identieke scenario's schrijf je één sjabloon en een `Examples`-tabel.

```gherkin
Scenario Outline: Inloggen met verschillende wachtwoorden
  Given er is een account voor "alice@shopwave.be" met wachtwoord "wachtwoord123"
  When de gebruiker inlogt met "alice@shopwave.be" en "<wachtwoord>"
  Then ontvangt de gebruiker de melding "<melding>"

  Examples:
    | wachtwoord     | melding                  |
    | wachtwoord123  | Voer uw 2FA-code in.     |
    | foutWachtwoord | Ongeldig wachtwoord.     |
    | kortPw         | Ongeldig wachtwoord.     |
```

`<wachtwoord>` en `<melding>` zijn placeholders die vervangen worden door elke rij uit de `Examples`-tabel. Dit genereert drie afzonderlijke tests. De step definitions hoef je niet aan te passen.

**Wanneer gebruik je `Scenario Outline`?** Als je hetzelfde gedrag wil testen met meerdere invoerwaarden. Denk aan grenswaarden ("een wachtwoord van 7 tekens, 8 tekens en 9 tekens"), equivalentieklassen ("een geldig e-mailadres, een e-mailadres zonder @, een leeg e-mailadres") of meerdere gebruikersrollen.

**Mini-controle:** je hebt een `Scenario Outline` met drie rijen in de `Examples`-tabel. Hoeveel tests verschijnen er in de Test Explorer? Drie, één per rij.

---

## 9. Acceptatietesten en security

Beveiligde flows zijn bij uitstek geschikt voor acceptatietesten. Een loginflow heeft meerdere scenario's die elk een specifieke beveiligingsvereiste uitdrukken. Die vereisten zijn gemakkelijk te beschrijven in Gherkin en moeilijk te vergeten als ze als scenario zijn vastgelegd.

Typische beveiligingsscenario's voor ShopWave:

```gherkin
Scenario: Account vergrendeld na drie foute pogingen
  Given er is een account voor "bob@shopwave.be" met wachtwoord "veiligPw"
  When de gebruiker drie keer inlogt met een fout wachtwoord
  Then is het account van "bob@shopwave.be" geblokkeerd

Scenario: 2FA-code is maar eenmaal geldig
  Given er is een account voor "alice@shopwave.be" met wachtwoord "wachtwoord123"
  When de gebruiker correct inlogt en een geldige 2FA-code gebruikt
  And de gebruiker probeert dezelfde 2FA-code opnieuw te gebruiken
  Then ontvangt de gebruiker de melding "Ongeldige 2FA-code."
```

Deze scenario's beschrijven beveiligingsvereisten in taal die ook een niet-technische productmanager begrijpt en kan valideren. Tegelijk zijn ze automatisch uitvoerbaar.

---

## 10. Samenvatting

| Concept | Wat je moet onthouden |
|--------|-----------------------|
| Acceptatietest | Test het volledige systeem vanuit gebruikersperspectief |
| BDD | Specificaties als voorbeelden van gedrag, gedeeld begrip tussen klant en ontwikkelaar |
| Gherkin | Taal voor scenario's: `Feature`, `Scenario`, `Given`, `When`, `Then` |
| Reqnroll | Open-source .NET-opvolger van SpecFlow, werkt met xUnit |
| `[Binding]` | Attribuut op klasse met step definitions |
| `{string}`, `{int}` | Placeholders die waarden uit Gherkin opvangen als parameters |
| Context-klasse | Gedeelde toestand tussen step definition-klassen via Reqnroll DI |
| `CommonSteps.cs` | Gedeelde stappen in één klasse om "Ambiguous step definition"-fouten te vermijden |
| `Scenario Outline` | Sjabloon-scenario met `Examples`-tabel, genereert meerdere tests |
| Callback-techniek | Gegenereerde waarden (2FA-code) opvangen in de context-klasse via een lambda |
