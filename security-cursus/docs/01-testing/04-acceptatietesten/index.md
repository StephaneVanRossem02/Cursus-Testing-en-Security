---
title: "Les 8: Acceptatietesten — BDD met Gherkin en Reqnroll"
sidebar_label: "Acceptatietesten (BDD)"
---

# Les 8: Acceptatietesten — BDD met Gherkin en Reqnroll

## ShopWave

We hebben unit tests, integration tests en een beveiligde API. Maar hoe weten we of het systeem doet wat de klant gevraagd heeft? Unit tests bewijzen dat een methode correct werkt — ze bewijzen niet dat het volledige systeem het juiste gedrag vertoont vanuit het standpunt van de gebruiker.

Dat is de rol van **acceptatietesten**.

---

## Theorie

### 1. De testpiramide en acceptatietesten

```
        /\
       /  \   Acceptatietesten
      /    \  (weinig, traag, hoog niveau)
     /------\
    /        \ Integration tests
   /          \
  /------------\
 /              \ Unit tests
/________________\ (veel, snel, laag niveau)
```

Acceptatietesten zitten bovenaan de piramide. Ze zijn:
- **Langzamer** — ze testen het volledige systeem, niet één klasse
- **Minder in aantal** — enkel de belangrijkste scenario's
- **Geschreven vanuit gebruikersperspectief** — niet vanuit technische implementatie

---

### 2. BDD en Gherkin

**Behavior Driven Development (BDD)** is een aanpak waarbij de specificaties van het systeem geschreven worden als voorbeelden van hoe het systeem zich gedraagt. Niet: "het systeem slaat een wachtwoord op", maar: "als een gebruiker inlogt met een verkeerd wachtwoord, ziet hij een foutmelding".

Die voorbeelden worden geschreven in **Gherkin** — een taal die zowel door mensen als door de computer begrepen wordt.

Gherkin gebruikt een vaste structuur:

```gherkin
Feature: naam van het onderdeel

  Scenario: beschrijving van één situatie
    Given  de beginsituatie
    When   de actie die de gebruiker uitvoert
    Then   het verwachte resultaat
```

Een concreet voorbeeld voor ShopWave:

```gherkin
Feature: Inloggen bij ShopWave

  Scenario: Succesvol inloggen
    Given er is een account voor "alice@shopwave.be" met wachtwoord "wachtwoord123"
    When de gebruiker inlogt met "alice@shopwave.be" en "wachtwoord123"
    Then ontvangt de gebruiker de melding "Voer uw 2FA-code in."
```

Gherkin is geen code — het is een specificatie in leesbare taal. Daarna koppel je elke stap aan C#-code die de stap uitvoert.

---

### 3. SpecFlow vs. Reqnroll

**SpecFlow** was het populaire BDD-framework voor .NET maar is intussen gestopt met actieve ontwikkeling. **Reqnroll** is de open-source opvolger, gebouwd op dezelfde codebase.

De syntax is nagenoeg identiek — alles wat je over SpecFlow leest, geldt ook voor Reqnroll. Het verschil zit in de NuGet-pakketten en de Visual Studio-extensie.

---

### 4. De drie lagen van een Reqnroll-project

```
Feature file (.feature)     → Gherkin-scenario's, leesbaar voor iedereen
Step definitions (.cs)      → C#-code die elke Gherkin-stap uitvoert
ShopWave-klassen            → De code die getest wordt
```

De feature file beschrijft **wat** het systeem moet doen.
De step definitions beschrijven **hoe** dat getest wordt.
De ShopWave-klassen zijn de **echte code** die draait.

---

## Demo

### Setup

**Stap 1 — Reqnroll extensie installeren**

Ga in Visual Studio naar `Extensions` → `Manage Extensions` → zoek op `Reqnroll` → installeer `Reqnroll for Visual Studio 2022 and 2026` → herstart Visual Studio.

**Stap 2 — Reqnroll-project aanmaken**

Rechtsklik op de solution → `Add` → `New Project` → zoek op `Reqnroll` → kies `Reqnroll Project` → naam: `ShopWave.Specs` → kies **xUnit** als testframework.

De solution ziet er nu zo uit:

```
ShopWave           ← Console App (bestaand)
ShopWave.Tests     ← xUnit (bestaand)
ShopWave.Api       ← Minimal API (bestaand)
ShopWave.Specs     ← Reqnroll acceptatietests (nieuw)
```

**Stap 3 — Project reference toevoegen**

Rechtsklik op `ShopWave.Specs` → `Add` → `Project Reference` → vink `ShopWave` aan.

De acceptatietests hebben toegang nodig tot `AccountRepository`, `TwoFactorService` enzovoort.

**Stap 4 — Voorbeeldbestanden verwijderen**

Reqnroll maakt automatisch een voorbeeldfeature en een voorbeeldstepfile aan. Verwijder die — we schrijven alles zelf.

---

### Stap 1 — Feature file aanmaken

**Bestand: `ShopWave.Specs/Features/Login.feature`** (nieuw bestand)

Rechtsklik op `ShopWave.Specs` → `Add` → `New Item` → `Reqnroll Feature File` → naam: `Login.feature`.

Vervang de inhoud door:

```gherkin
Feature: Inloggen bij ShopWave
```

Dit is de naam van het onderdeel dat we testen. Voeg daarna het eerste scenario toe:

```gherkin
  Scenario: Succesvol inloggen met correct wachtwoord
    Given er is een account voor "alice@shopwave.be" met wachtwoord "wachtwoord123"
    When de gebruiker inlogt met "alice@shopwave.be" en "wachtwoord123"
    Then ontvangt de gebruiker de melding "Voer uw 2FA-code in."
```

`Given` beschrijft de beginsituatie — er moet een account bestaan.
`When` beschrijft de actie — de gebruiker probeert in te loggen.
`Then` beschrijft het verwachte resultaat — de melding die terugkomt.

Voeg een tweede scenario toe voor het geval het wachtwoord fout is:

```gherkin
  Scenario: Inloggen met fout wachtwoord
    Given er is een account voor "alice@shopwave.be" met wachtwoord "wachtwoord123"
    When de gebruiker inlogt met "alice@shopwave.be" en "foutWachtwoord"
    Then ontvangt de gebruiker de melding "Ongeldig wachtwoord."
```

---

### Stap 2 — Step definitions aanmaken

Elke Gherkin-stap moet gekoppeld worden aan een C#-methode. Die koppeling heet een **step definition**.

**Bestand: `ShopWave.Specs/StepDefinitions/LoginSteps.cs`** (nieuw bestand)

Begin met de klasse-declaratie:

```csharp
using Reqnroll;
using ShopWave.Security;

namespace ShopWave.Specs.StepDefinitions
{
    [Binding]
    public class LoginSteps
    {
```

`[Binding]` vertelt Reqnroll dat deze klasse step definitions bevat.

Voeg de velden toe die de stappen met elkaar delen:

```csharp
        private AccountRepository _accountRepository = null!;
        private TwoFactorService  _twoFactorService  = null!;
        private string            _result            = string.Empty;
```

`_result` bewaart het antwoord van de login-methode zodat de `Then`-stap er bij kan.

Implementeer de `Given`-stap — de beginsituatie opzetten:

```csharp
        [Given("er is een account voor {string} met wachtwoord {string}")]
        public void GivenErIsEenAccount(string email, string wachtwoord)
        {
            _twoFactorService  = new TwoFactorService();
            _accountRepository = new AccountRepository(_twoFactorService);
            _accountRepository.Register(email, wachtwoord);
        }
```

`{string}` is een Reqnroll-placeholder die de waarde uit de Gherkin-stap opvangt en doorgeeft als parameter. De tekst tussen aanhalingstekens in de feature file (`"alice@shopwave.be"`) wordt automatisch doorgegeven als `email`.

Implementeer de `When`-stap — de actie uitvoeren:

```csharp
        [When("de gebruiker inlogt met {string} en {string}")]
        public void WhenDeGebruikerInlogt(string email, string wachtwoord)
        {
            _result = _accountRepository.Login(email, wachtwoord);
        }
```

Implementeer de `Then`-stap — het resultaat controleren:

```csharp
        [Then("ontvangt de gebruiker de melding {string}")]
        public void ThenOntvangtDeGebruikerDeMelding(string verwachteMelding)
        {
            Assert.Equal(verwachteMelding, _result);
        }
    }
}
```

`Assert.Equal` is de xUnit-assert die we al kennen. De verwachte waarde staat in de feature file — de `Then`-stap vergelijkt die met het echte resultaat.

---

### Stap 3 — Tests uitvoeren

Build de solution en open de Test Explorer. Je ziet nu twee tests onder `ShopWave.Specs`:

```
✓ Succesvol inloggen met correct wachtwoord
✓ Inloggen met fout wachtwoord
```

Elke Gherkin-scenario wordt een aparte test. De namen komen rechtstreeks uit de feature file — leesbaar voor iedereen, ook voor mensen zonder technische achtergrond.

---

### Stap 4 — Scenario Outline: meerdere gevallen in één scenario

In plaats van twee aparte scenario's schrijven we één scenario met een tabel van voorbeelden.

**Bestand: `ShopWave.Specs/Features/Login.feature`** — voeg toe:

```gherkin
  Scenario Outline: Inloggen met verschillende wachtwoorden
    Given er is een account voor "alice@shopwave.be" met wachtwoord "wachtwoord123"
    When de gebruiker inlogt met "alice@shopwave.be" en "<wachtwoord>"
    Then ontvangt de gebruiker de melding "<melding>"

    Examples:
      | wachtwoord    | melding                   |
      | wachtwoord123 | Voer uw 2FA-code in.      |
      | foutWachtwoord| Ongeldig wachtwoord.      |
      | kortPw        | Ongeldig wachtwoord.      |
```

`Scenario Outline` is een sjabloon. `<wachtwoord>` en `<melding>` zijn placeholders die vervangen worden door elke rij uit de `Examples`-tabel. Dit genereert drie afzonderlijke tests.

De step definitions die we al schreven werken automatisch — geen aanpassingen nodig.

---

### Stap 5 — Feature voor lockout

**Bestand: `ShopWave.Specs/Features/Lockout.feature`** (nieuw bestand)

```gherkin
Feature: Account lockout bij ShopWave

  Scenario: Account vergrendeld na drie foute pogingen
    Given er is een account voor "bob@shopwave.be" met wachtwoord "veiligPw"
    When de gebruiker drie keer inlogt met een fout wachtwoord
    Then is het account van "bob@shopwave.be" geblokkeerd
```

> ⚠️ **Probleem: dubbele step definition.** Als je `[Given("er is een account voor {string} met wachtwoord {string}")]` in zowel `LoginSteps.cs` als `LockoutSteps.cs` definieert, gooit Reqnroll een **"Ambiguous step definition"**-fout. Reqnroll zoekt over alle `[Binding]`-klassen in het project naar stap-patronen — als hetzelfde patroon twee keer voorkomt, weet het niet welke methode het moet aanroepen.
>
> **Oplossing: gedeelde toestand via een context-klasse.** Reqnroll heeft ingebouwde ondersteuning voor dependency injection. Maak een eenvoudige context-klasse aan die door alle step definition-klassen gedeeld wordt, en zet de `Given`-stap in één aparte `CommonSteps.cs`.

**Stap 5a — Maak `LoginContext.cs` aan** in de map `ShopWave.Specs`:

```csharp
using ShopWave.Security;

namespace ShopWave.Specs
{
    // Gedeelde toestand tussen step definition-klassen.
    // Reqnroll injecteert automatisch dezelfde instantie in elke [Binding]-klasse
    // die dit type als constructor-parameter accepteert.
    public class LoginContext
    {
        public AccountRepository AccountRepository { get; set; } = null!;
        public TwoFactorService  TwoFactorService  { get; set; } = null!;
        public string            Result            { get; set; } = string.Empty;
    }
}
```

**Stap 5b — Maak `CommonSteps.cs` aan** in `ShopWave.Specs/StepDefinitions`:

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

        // Eén definitie, gedeeld door alle features — nooit dupliceren
        [Given("er is een account voor {string} met wachtwoord {string}")]
        public void GivenErIsEenAccount(string email, string wachtwoord)
        {
            _ctx.TwoFactorService  = new TwoFactorService();
            _ctx.AccountRepository = new AccountRepository(_ctx.TwoFactorService);
            _ctx.AccountRepository.Register(email, wachtwoord);
        }
    }
}
```

**Stap 5c — Pas `LoginSteps.cs` aan** zodat het de gedeelde context gebruikt (verwijder de eigen velden en de `Given`-stap):

```csharp
using Reqnroll;

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

**Stap 5d — Maak `LockoutSteps.cs` aan** — zonder de dubbele `Given`-stap:

```csharp
using Reqnroll;

namespace ShopWave.Specs.StepDefinitions
{
    [Binding]
    public class LockoutSteps
    {
        private readonly LoginContext _ctx;

        public LockoutSteps(LoginContext ctx)
        {
            _ctx = ctx;
        }

        // Geen [Given] hier — CommonSteps.cs regelt dat voor alle features

        [When("de gebruiker drie keer inlogt met een fout wachtwoord")]
        public void WhenDrieKeerFoutWachtwoord()
        {
            _ctx.AccountRepository.Login("bob@shopwave.be", "fout1");
            _ctx.AccountRepository.Login("bob@shopwave.be", "fout2");
            _ctx.AccountRepository.Login("bob@shopwave.be", "fout3");
        }

        [Then("is het account van {string} geblokkeerd")]
        public void ThenIsHetAccountGeblokkeerd(string email)
        {
            string result = _ctx.AccountRepository.Login(email, "veiligPw");
            Assert.Equal("Account geblokkeerd.", result);
        }
    }
}
```

Reqnroll injecteert automatisch dezelfde `LoginContext`-instantie in `CommonSteps`, `LoginSteps` en `LockoutSteps` — alle stappen van één scenario delen zo dezelfde toestand. De `Then`-stap logt nog een keer in met het correcte wachtwoord: als het account geblokkeerd is, geeft `Login` "Account geblokkeerd." terug — ook al is het wachtwoord nu correct.

---

## Oefeningen

### Oefening 1 — 2FA-scenario

Schrijf een feature file `TwoFactor.feature` en bijbehorende step definitions voor de volgende scenario's:

**Scenario 1:** succesvol inloggen inclusief 2FA
```
Given er is een account voor "charlie@shopwave.be" met wachtwoord "pw123"
When de gebruiker inlogt met het correcte wachtwoord
And de gebruiker voert de correcte 2FA-code in
Then is de gebruiker ingelogd
```

**Scenario 2:** 2FA-code is fout
```
Given er is een account voor "charlie@shopwave.be" met wachtwoord "pw123"
When de gebruiker inlogt met het correcte wachtwoord
And de gebruiker voert een foute 2FA-code in
Then ontvangt de gebruiker de melding "Ongeldige 2FA-code."
```

Tip: `And` werkt als een extra `Given`, `When` of `Then` — het volgt de stijl van de vorige stap.

---

### Oefening 2 — Scenario Outline voor 2FA

Herschrijf de twee scenario's uit oefening 1 als één `Scenario Outline` met een `Examples`-tabel.

---

### Oefening 3 — Registratie-feature

Schrijf een feature file `Registratie.feature` voor:

- Registratie van een nieuw account lukt
- Registratie van een account dat al bestaat geeft een foutmelding

Schrijf ook de bijbehorende step definitions in `RegistratieSteps.cs`.

---

### Oefening 4 — CIA-koppeling

Bekijk de scenario's die je geschreven hebt en beantwoord:

1. Welke scenario's testen **authenticatie**?
2. Welke scenario's beschermen tegen **ongeautoriseerde toegang** (CIA: Confidentiality)?
3. Kan een acceptatietest ook de **integriteit** van een systeem aantonen? Geef een voorbeeld.

---

## Modeloplossing

> De modeloplossing is beschikbaar na het indienen van de labo-opdracht via Digitap.

---

### Modeloplossing Oefening 1 — TwoFactor.feature en TwoFactorSteps.cs

**Bestand: `ShopWave.Specs/Features/TwoFactor.feature`**

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

**Bestand: `ShopWave.Specs/StepDefinitions/TwoFactorSteps.cs`**

Voor 2FA-scenario's moet de `Given`-stap de callback-versie van `TwoFactorService` gebruiken om de gegenereerde code op te vangen. Pas daarvoor `LoginContext` aan zodat hij ook een `LastCode` bijhoudt, en update `CommonSteps.cs` om de callback te registreren:

```csharp
// LoginContext.cs — uitbreiding
public class LoginContext
{
    public AccountRepository AccountRepository { get; set; } = null!;
    public TwoFactorService  TwoFactorService  { get; set; } = null!;
    public string            LastCode          { get; set; } = string.Empty;
    public string            Result            { get; set; } = string.Empty;
}
```

```csharp
// CommonSteps.cs — Given met callback zodat TwoFactorSteps de code kan lezen
[Given("er is een account voor {string} met wachtwoord {string}")]
public void GivenErIsEenAccount(string email, string wachtwoord)
{
    _ctx.TwoFactorService = new TwoFactorService(onCodeGenerated: (mail, code) =>
    {
        _ctx.LastCode = code;
    });
    _ctx.AccountRepository = new AccountRepository(_ctx.TwoFactorService);
    _ctx.AccountRepository.Register(email, wachtwoord);
}
```

```csharp
using Reqnroll;

namespace ShopWave.Specs.StepDefinitions
{
    [Binding]
    public class TwoFactorSteps
    {
        private readonly LoginContext _ctx;

        public TwoFactorSteps(LoginContext ctx)
        {
            _ctx = ctx;
        }

        // Geen [Given] — CommonSteps.cs regelt dat inclusief de callback

        [When("de gebruiker inlogt met het correcte wachtwoord voor {string}")]
        public void WhenInloggenMetCorrecteWachtwoord(string email)
        {
            _ctx.AccountRepository.Login(email, "pw123");
        }

        [When("de gebruiker voert de correcte 2FA-code in voor {string}")]
        public void WhenCorrecteTwoFactorCode(string email)
        {
            _ctx.Result = _ctx.AccountRepository.VerifyTwoFactor(email, _ctx.LastCode);
        }

        [When("de gebruiker voert een foute 2FA-code in voor {string}")]
        public void WhenFouteTwoFactorCode(string email)
        {
            _ctx.Result = _ctx.AccountRepository.VerifyTwoFactor(email, "000000");
        }

        [Then("is de gebruiker {string} ingelogd")]
        public void ThenIsDeGebruikerIngelogd(string email)
        {
            Assert.Equal("Inloggen geslaagd.", _ctx.Result);
        }
    }
}
```

De `TwoFactorService` wordt aangemaakt met een callback `onCodeGenerated` in `CommonSteps.cs` — dezelfde techniek als in les 5. De code wordt opgeslagen in `LoginContext.LastCode`, zodat `TwoFactorSteps` er bij kan zonder de `Given`-stap te dupliceren.

---

## Samenvatting

| Concept | Wat je moet onthouden |
|---------|-----------------------|
| Acceptatietest | Test het volledige systeem vanuit gebruikersperspectief |
| BDD | Gedragsgedreven ontwikkeling — specificaties als voorbeelden |
| Gherkin | Taal voor scenario's: `Feature`, `Scenario`, `Given`, `When`, `Then` |
| Reqnroll | Open-source opvolger van SpecFlow voor .NET |
| `[Binding]` | Attribuut op de klasse met step definitions |
| `{string}`, `{int}` | Reqnroll-placeholders die waarden uit Gherkin opvangen |
| `Scenario Outline` | Sjabloon-scenario met een `Examples`-tabel |
| `And` | Extra stap die de stijl van de vorige stap volgt |
| Context-klasse (DI) | Gedeelde toestand tussen step definition-klassen via Reqnroll DI |
| `CommonSteps.cs` | Eén klasse met gedeelde stappen — voorkomt "Ambiguous step definition"-fouten |

---

## Volgende les

Les 9 is **Security: Secure Coding**. We bekijken veelgemaakte beveiligingsfouten in code — input validatie, SQL injection, XSS — en hoe je die voorkomt in de ShopWave API.