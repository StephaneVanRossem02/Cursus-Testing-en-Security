---
title: "Les 8: Acceptatietesten — Theorie"
sidebar_label: "Theorie"
---

# Les 8: Acceptatietesten — BDD met Gherkin en Reqnroll — Theorie

## Acceptatietesten en de testpiramide

Acceptatietesten zitten **bovenaan** de testpiramide. Ze zijn langzamer, minder in aantal en geschreven vanuit **gebruikersperspectief** — ze beschrijven wat het systeem moet doen, niet hoe.

---

## Behavior Driven Development (BDD)

BDD schrijft specificaties als **voorbeelden van hoe het systeem zich gedraagt**. Niet "het systeem slaat een wachtwoord op", maar "als een gebruiker inlogt met een verkeerd wachtwoord, ziet hij een foutmelding".

Voordelen:
- Specificaties zijn leesbaar voor niet-programmeurs
- Scenario's dienen als directe acceptatiecriteria
- Tests leggen vast wat het systeem belooft te doen

---

## Gherkin

Gherkin is de taal voor BDD-scenario's, leesbaar voor zowel mensen als computer:

```gherkin
Feature: naam van het onderdeel

  Scenario: beschrijving van één situatie
    Given  de beginsituatie
    When   de actie die de gebruiker uitvoert
    Then   het verwachte resultaat
```

Extra keywords:

| Keyword           | Gebruik                                                   |
|-------------------|-----------------------------------------------------------|
| `And` / `But`     | Extra stappen toevoegen na Given/When/Then                |
| `Background`      | Herhaalde Given-stappen die voor elk scenario gelden      |
| `Scenario Outline` | Sjabloon-scenario met een `Examples`-tabel               |

---

## Reqnroll

Reqnroll is de open-source .NET-opvolger van SpecFlow. Structuur van een project:

```
Feature file (.feature)   → Gherkin-scenario's, leesbaar voor iedereen
Step definitions (.cs)    → C#-code die elke Gherkin-stap uitvoert
ShopWave-klassen          → De productiecode die getest wordt
```

### Step definitions

```csharp
[Binding]
public class LoginSteps
{
    private readonly LoginContext context;

    public LoginSteps(LoginContext context)
    {
        this.context = context;
    }

    [Given("er is een account voor {string} met wachtwoord {string}")]
    public void GivenErIsEenAccount(string email, string wachtwoord)
    {
        this.context.AccountRepository = new AccountRepository(new TwoFactorService());
        this.context.AccountRepository.Register(email, wachtwoord);
    }

    [When("de gebruiker inlogt met het correcte wachtwoord voor {string}")]
    public void WhenInloggen(string email)
    {
        this.context.Result = this.context.AccountRepository.Login(email, "pw123");
    }

    [Then("is de gebruiker {string} ingelogd")]
    public void ThenIngelogd(string email)
    {
        Assert.Equal("Inloggen geslaagd.", this.context.Result);
    }
}
```

---

## Context-klasse (Dependency Injection)

Reqnroll ondersteunt DI tussen step definition-klassen. Maak een context-klasse aan als gedeelde toestand:

```csharp
public class LoginContext
{
    public AccountRepository AccountRepository { get; set; } = null!;
    public TwoFactorService  TwoFactorService  { get; set; } = null!;
    public string            LastCode          { get; set; } = string.Empty;
    public string            Result            { get; set; } = string.Empty;
}
```

Reqnroll injecteert automatisch dezelfde instantie in elke step definition-klasse die die context nodig heeft — geen handmatige registratie vereist.

---

## CommonSteps.cs

Zet gedeelde stappen (bv. `Given er is een account voor...`) in één `CommonSteps.cs`. Dit vermijdt "Ambiguous step definition"-fouten wanneer meerdere step definition-klassen dezelfde stap definiëren.

---

## Scenario Outline

```gherkin
Scenario Outline: Login met verschillende wachtwoorden
  Given er is een account voor "alice@shopwave.be" met wachtwoord "pw123"
  When de gebruiker inlogt met wachtwoord "<wachtwoord>"
  Then ontvangt de gebruiker de melding "<melding>"

  Examples:
    | wachtwoord | melding               |
    | pw123      | 2FA vereist.          |
    | fout       | Inloggen mislukt.     |
```

Reqnroll voert dit scenario eenmaal uit per rij in de `Examples`-tabel.