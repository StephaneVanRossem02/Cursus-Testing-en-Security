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

## 4. Reqnroll: het framework

**SpecFlow** was het populaire BDD-framework voor .NET. Het is intussen gestopt met actieve ontwikkeling. **Reqnroll** is de open-source opvolger, gebouwd op dezelfde codebase. De syntax is nagenoeg identiek.

Een Reqnroll-project werkt met drie lagen:

**Feature file (.feature)** - beschrijft wat het systeem moet doen. Geschreven in Gherkin. Leesbaar voor iedereen.

**Step definitions (.cs)** - koppelt elke Gherkin-stap aan een C#-methode. Dit is de brug tussen taal en code.

**Productieklassen** - de echte code die getest wordt. `AccountRepository`, `CartService`, enzovoort.

De feature file beschrijft het scenario. De step definitions voeren het uit. De productieklassen bevatten de logica.

---

## 5. Demo: de loginfeature stap voor stap

We bouwen de acceptatietest voor de loginflow van ShopWave. We doen dit in kleine stappen. Na elke stap is er iets nieuws om te zien in Visual Studio.

### Stap 1: project opzetten

**Reqnroll-extensie installeren**

Ga in Visual Studio naar `Extensions` > `Manage Extensions` > zoek op `Reqnroll` > installeer `Reqnroll for Visual Studio 2022 and 2026` > herstart Visual Studio.

Deze extensie voegt de juiste projecttemplates en bestandstypes toe.

**Reqnroll-project aanmaken**

Rechtsklik op de solution > `Add` > `New Project` > zoek op `Reqnroll` > kies `Reqnroll Project` > naam: `ShopWave.Specs` > kies `xUnit` als testframework.

De solution ziet er nu zo uit:

```csharp
ShopWave           productieklassen
ShopWave.Tests     xUnit unit tests en integration tests
ShopWave.Specs     Reqnroll acceptatietests (nieuw)
```

**Project reference toevoegen**

Rechtsklik op `ShopWave.Specs` > `Add` > `Project Reference` > vink `ShopWave` aan.

Zonder deze reference kan `ShopWave.Specs` de klassen van ShopWave niet aanroepen.

**Voorbeeldbestanden verwijderen**

Reqnroll maakt automatisch een voorbeeldfeature en een voorbeeldstepfile aan. Verwijder die. We schrijven alles zelf.

---

### Stap 2: de feature file schrijven

Rechtsklik op `ShopWave.Specs` > `Add` > `New Item` > `Reqnroll Feature File` > naam: `Login.feature`.

Je ziet een leeg `.feature`-bestand. Schrijf de volgende inhoud:

```gherkin
Feature: Inloggen bij ShopWave

  Scenario: Succesvol inloggen met correct wachtwoord
    Given er is een account voor "alice@shopwave.be" met wachtwoord "wachtwoord123"
    When de gebruiker inlogt met "alice@shopwave.be" en "wachtwoord123"
    Then ontvangt de gebruiker de melding "Voer uw 2FA-code in."
```

Wat staat er hier?

- `Feature: Inloggen bij ShopWave` is de naam van het onderdeel dat we beschrijven. Er is er één per bestand.
- `Scenario:` beschrijft één specifieke situatie. Een feature file kan meerdere scenario's bevatten.
- `Given` beschrijft de beginsituatie. Wat is er al vóór de actie?
- `When` beschrijft de actie. Wat doet de gebruiker of het systeem?
- `Then` beschrijft het verwachte resultaat. Wat moet er gebeuren?

De teksten tussen aanhalingstekens (`"alice@shopwave.be"`, `"wachtwoord123"`) zijn parameters. Die waarden worden straks automatisch doorgegeven aan de C#-methode.

Bouw de solution. Open de Test Explorer. Je ziet het scenario verschijnen als één test, maar de test staat op **"Not run"**. Dat is normaal: we hebben nog geen C#-code geschreven die de stappen uitvoert.

---

### Stap 3: de Given-stap implementeren

Maak een map `StepDefinitions` aan in `ShopWave.Specs`. Maak daarin `LoginSteps.cs` aan.

Schrijf eerst alleen de basisstructuur:

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
    }
}
```

`[Binding]` vertelt Reqnroll dat deze klasse step definitions bevat. Zonder dit attribuut worden de methoden genegeerd.

Voeg nu de `Given`-stap toe:

```csharp
        [Given("er is een account voor {string} met wachtwoord {string}")]
        public void GivenErIsEenAccount(string email, string wachtwoord)
        {
            _accountRepository = new AccountRepository(new TwoFactorService());
            _accountRepository.Register(email, wachtwoord);
        }
```

Het attribuut `[Given("...")]` bevat de tekst uit de feature file. Die tekst moet exact overeenkomen.

`{string}` is een Reqnroll-placeholder: de waarde tussen aanhalingstekens in de feature file (`"alice@shopwave.be"`) wordt automatisch doorgegeven als parameter `email`. De tweede `{string}` wordt doorgegeven als `wachtwoord`. Andere placeholders zijn `{int}`, `{double}` en `{decimal}`.

Bouw de solution en voer de test uit. De test staat op **"Skipped"** of **"Pending"**. Reqnroll heeft de `Given`-stap gevonden, maar de `When`- en `Then`-stappen zijn nog niet gedefinieerd.

---

### Stap 4: When en Then toevoegen

Voeg de `When`-stap toe in `LoginSteps.cs`:

```csharp
        [When("de gebruiker inlogt met {string} en {string}")]
        public void WhenDeGebruikerInlogt(string email, string wachtwoord)
        {
            _result = _accountRepository.Login(email, wachtwoord);
        }
```

De `When`-stap roept `Login` aan en bewaart het resultaat in `_result`. Die waarde is beschikbaar voor de `Then`-stap in dezelfde klasse.

Voeg de `Then`-stap toe:

```csharp
        [Then("ontvangt de gebruiker de melding {string}")]
        public void ThenOntvangtDeGebruikerDeMelding(string verwachteMelding)
        {
            Assert.Equal(verwachteMelding, _result);
        }
```

`Assert.Equal` is de xUnit-assert die we al kennen. De verwachte waarde (`verwachteMelding`) komt uit de feature file. De echte waarde is `_result`, het antwoord van `Login`.

Bouw de solution en voer de test uit. De test slaagt nu.

Wat ziet de Test Explorer?

```csharp
✓ Succesvol inloggen met correct wachtwoord
```

De testname komt rechtstreeks uit de feature file. Geen technische naam zoals `ShouldReturnMessageWhenLoginSucceeds`. Gewone mensentaal.

---

### Stap 5: tweede scenario toevoegen

Voeg een tweede scenario toe aan `Login.feature`:

```gherkin
  Scenario: Inloggen met fout wachtwoord
    Given er is een account voor "alice@shopwave.be" met wachtwoord "wachtwoord123"
    When de gebruiker inlogt met "alice@shopwave.be" en "foutWachtwoord"
    Then ontvangt de gebruiker de melding "Ongeldig wachtwoord."
```

Bouw en voer de tests uit. Je ziet nu twee tests:

```csharp
✓ Succesvol inloggen met correct wachtwoord
✓ Inloggen met fout wachtwoord
```

Je hoefde geen nieuwe step definitions te schrijven. De stap-patronen (`Given`, `When`, `Then`) waren al gedefinieerd. Reqnroll herkent ze en vult automatisch de juiste parameterwaarden in.

**Dit is de kracht van BDD:** je voegt scenario's toe door tekst te schrijven. De C#-code voor elke stap schrijf je één keer.

---

### Stap 6: een tweede feature en het probleem van gedeelde stappen

Stel dat we nu een lockout-feature willen toevoegen. We maken `Lockout.feature` aan:

```gherkin
Feature: Account lockout bij ShopWave

  Scenario: Account vergrendeld na drie foute pogingen
    Given er is een account voor "bob@shopwave.be" met wachtwoord "veiligPw"
    When de gebruiker drie keer inlogt met een fout wachtwoord
    Then is het account van "bob@shopwave.be" geblokkeerd
```

We willen de `Given`-stap hergebruiken uit `LoginSteps.cs`. Maar als we `[Given("er is een account voor...")]` ook in een nieuwe `LockoutSteps.cs` definiëren, gooit Reqnroll een fout:

```csharp
Ambiguous step definition.
```

Reqnroll zoekt over **alle** `[Binding]`-klassen in het project naar stap-patronen. Als hetzelfde patroon twee keer voorkomt, weet het niet welke methode het moet aanroepen.

Er is ook een tweede probleem. De `_result`-field staat nu als private field in `LoginSteps`. Als de lockout-test een `Then`-stap in een andere klasse gebruikt, heeft die geen toegang tot `_result`.

De oplossing voor beide problemen: een **context-klasse** als gedeelde toestand.

---

### Stap 7: de context-klasse aanmaken

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

Dit is een gewone C#-klasse zonder attributen. Ze bevat alles wat stappen met elkaar moeten delen:
- `AccountRepository`: aangemaakt in `Given`, gebruikt in `When` en `Then`
- `TwoFactorService`: aangemaakt in `Given`, nodig voor de 2FA-stappen
- `LastCode`: de gegenereerde 2FA-code, opgeslagen via de callback-techniek uit les 5
- `Result`: het antwoord van `Login`, doorgegeven van `When` naar `Then`

Reqnroll injecteert automatisch dezelfde `LoginContext`-instantie in elke `[Binding]`-klasse die die context als constructor-parameter accepteert. Je hoeft niets te registreren. Reqnroll regelt dit zelf.

---

### Stap 8: CommonSteps aanmaken en LoginSteps aanpassen

Maak `ShopWave.Specs/StepDefinitions/CommonSteps.cs` aan. Zet hier de `Given`-stap in die door alle features gedeeld wordt:

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

De constructor ontvangt een `LoginContext`. Reqnroll injecteert die automatisch. De `TwoFactorService` wordt aangemaakt met de callback-techniek uit les 5: elke gegenereerde 2FA-code wordt opgeslagen in `_ctx.LastCode`, zodat andere stappen er bij kunnen.

Pas nu `LoginSteps.cs` aan: verwijder de eigen fields en de `Given`-stap, en gebruik de context:

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

De `Given`-stap is verdwenen uit `LoginSteps.cs`. Die staat nu alleen in `CommonSteps.cs` en is beschikbaar voor zowel de loginfeature als de lockoutfeature. `_result` is ook verdwenen: die staat nu als `Result` in de context, toegankelijk voor elke step definition-klasse.

Bouw en voer de tests uit. Beide loginscenario's slagen nog steeds.

**Mini-controle:** je hebt `CommonSteps.cs` aangemaakt maar je krijgt nog steeds een "Ambiguous step definition"-fout. Wat is de oorzaak? De oude `[Given]`-methode staat nog in `LoginSteps.cs`. Je moet die verwijderen.

---

## 6. Scenario Outline: meerdere gevallen in één sjabloon

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

## 7. Acceptatietesten en security

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

## 8. Samenvatting

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
