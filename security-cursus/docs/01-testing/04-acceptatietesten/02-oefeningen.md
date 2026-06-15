---
title: "Les 8: Oefeningen - Acceptatietesten"
sidebar_label: "Oefeningen"
---

# Oefeningen: Acceptatietesten

Werk de oefeningen in volgorde. Schrijf bij elke oefening eerst de feature file, dan de step definitions. Gebruik de demo uit de theorie als referentie.

---

## Oefening 1: Scenario Outline voor de loginflow

**Leerdoel:** je vervangt meerdere identieke scenario's door één `Scenario Outline` met een `Examples`-tabel.

**Moeilijkheidsgraad:** basis

### Startcode

Je hebt de loginfeature uit de theorie als vertrekpunt. Die bevat twee afzonderlijke scenario's:

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

Je hebt ook de step definitions uit de theorie (`LoginSteps.cs`, `CommonSteps.cs`, `LoginContext.cs`).

<h3 class="opdracht-titel">Opdracht</h3>

Vervang de twee afzonderlijke scenario's door één `Scenario Outline`. Voeg daarna een derde geval toe aan de `Examples`-tabel: een leeg wachtwoord (`""`) geeft ook de melding `"Ongeldig wachtwoord."`.

Verwacht resultaat in de Test Explorer: drie tests, elk met een andere naam die het invoerwaarden-paar bevat.

De bestaande step definitions mogen niet aangepast worden. Een `Scenario Outline` vereist geen nieuwe step definitions als de stap-patronen al bestaan.

---

## Oefening 2: Lockout-feature

**Leerdoel:** je schrijft een volledige feature file en bijbehorende step definitions voor een nieuw scenario.

**Moeilijkheidsgraad:** basis

### Startcode

Maak `ShopWave.Specs/Features/Lockout.feature` aan.

De `CommonSteps.cs` uit de theorie bevat al de `Given`-stap voor het aanmaken van een account. Die mag je hergebruiken. Je hoeft die niet te dupliceren in een nieuwe step definitions-klasse.

<h3 class="opdracht-titel">Opdracht</h3>

Schrijf de volgende twee scenario's voor de lockout-feature en de bijbehorende `LockoutSteps.cs`.

**Scenario 1:** account geblokkeerd na drie foute pogingen

```csharp
Given er is een account voor "bob@shopwave.be" met wachtwoord "veiligPw"
When de gebruiker drie keer inlogt met een fout wachtwoord
Then is het account van "bob@shopwave.be" geblokkeerd
```

**Scenario 2:** na blokkering werkt ook het correcte wachtwoord niet meer

```csharp
Given er is een account voor "bob@shopwave.be" met wachtwoord "veiligPw"
When de gebruiker drie keer inlogt met een fout wachtwoord
And de gebruiker inlogt met het correcte wachtwoord "veiligPw"
Then ontvangt de gebruiker de melding "Account geblokkeerd."
```

**Aandachtspunt:** de `When`-stap "de gebruiker drie keer inlogt met een fout wachtwoord" roept `Login` drie keer aan met een fout wachtwoord. Je hoeft geen loop te schrijven in Gherkin. De herhaling zit in de C#-implementatie van die stap.

**Aandachtspunt:** gebruik de gedeelde `LoginContext` voor de `AccountRepository`. Maak geen tweede `AccountRepository`-instantie aan in `LockoutSteps.cs`.

---

## Oefening 3: Registratie-feature

**Leerdoel:** je schrijft zelfstandig een feature file en step definitions voor een nieuw domein-scenario zonder startcode.

**Moeilijkheidsgraad:** gemiddeld

<h3 class="opdracht-titel">Opdracht</h3>

Maak `ShopWave.Specs/Features/Registratie.feature` aan en schrijf de bijbehorende `RegistratieSteps.cs`.

Test de volgende scenario's:

| Scenario | Verwacht resultaat |
|---------|-------------------|
| Registratie van een nieuw e-mailadres met een geldig wachtwoord | "Registratie geslaagd." |
| Registratie van een e-mailadres dat al bestaat | "Account bestaat al." |

**Structuur van de feature file:**

```gherkin
Feature: Registratie bij ShopWave

  Scenario: Registratie van een nieuw account
    Given er bestaat nog geen account voor "david@shopwave.be"
    When ...
    Then ...

  Scenario: Registratie van een bestaand account
    Given er is al een account voor "david@shopwave.be"
    When ...
    Then ...
```

**Aandachtspunten:**
- De `Given`-stap "er bestaat nog geen account voor..." is anders dan "er is een account voor..." uit de theorie. Dit is een nieuwe stap die je zelf moet definiëren in `RegistratieSteps.cs`
- Bewaar het resultaat van `Register(...)` in een lokale field in `RegistratieSteps.cs`, niet in `LoginContext`. Dit scenario heeft geen relatie met de loginflow
- De `Then`-stap "ontvangt de gebruiker de melding" bestaat al in `LoginSteps.cs`. Gebruik die niet voor registratiemeldingen: de stap-tekst moet duidelijk maken dat het om een registratiemelding gaat, anders verwar je de twee flows

---

## Oefening 4: 2FA-flow als Scenario Outline

**Leerdoel:** je combineert de callback-techniek met een `Scenario Outline` om de volledige 2FA-flow te testen voor meerdere gevallen.

**Moeilijkheidsgraad:** uitdaging

### Startcode

Maak `ShopWave.Specs/Features/TwoFactor.feature` aan.

De `CommonSteps.cs` installeert al de callback op `TwoFactorService` en slaat de gegenereerde code op in `_ctx.LastCode`.

<h3 class="opdracht-titel">Opdracht</h3>

Schrijf een `TwoFactor.feature` met twee afzonderlijke scenario's en daarna een `Scenario Outline` die beide gevallen dekt.

**Scenario 1:** succesvol inloggen inclusief correcte 2FA-code

```csharp
Given er is een account voor "charlie@shopwave.be" met wachtwoord "pw123"
When de gebruiker inlogt met het correcte wachtwoord voor "charlie@shopwave.be"
And de gebruiker voert de correcte 2FA-code in voor "charlie@shopwave.be"
Then is de gebruiker "charlie@shopwave.be" ingelogd
```

**Scenario 2:** 2FA-code is fout

```csharp
Given er is een account voor "charlie@shopwave.be" met wachtwoord "pw123"
When de gebruiker inlogt met het correcte wachtwoord voor "charlie@shopwave.be"
And de gebruiker voert een foute 2FA-code in voor "charlie@shopwave.be"
Then ontvangt de gebruiker de melding "Ongeldige 2FA-code."
```

Schrijf daarna `TwoFactorSteps.cs` met de step definitions voor de `When`- en `Then`-stappen. De `Given`-stap staat al in `CommonSteps.cs`.

**Uitdaging:** herschrijf de twee scenario's daarna als één `Scenario Outline`. De uitdaging is dat de `Then`-stap verschilt per geval ("is de gebruiker ingelogd" versus "ontvangt de gebruiker de melding"). Hoe pak je dat aan?

**Hint:** je kan de twee `Then`-stappen samenvoegen tot één stap met een string-placeholder, bijvoorbeeld "ontvangt de gebruiker het resultaat" gevolgd door de verwachte tekst. Zet die verwachte teksten dan in de `Examples`-tabel: "Inloggen geslaagd." en "Ongeldige 2FA-code.".

---

## Oefening 5: Reflectie

Beantwoord deze vragen voor jezelf voor je de oplossingen bekijkt.

1. Je hebt unit tests voor `AccountRepository.Login` die alle gevallen dekken. Waarom schrijf je dan nog een Gherkin-scenario voor hetzelfde gedrag?

2. De `CommonSteps.cs` bevat de `Given`-stap voor het aanmaken van een account. Wat zou er fout gaan als je die stap in zowel `LoginSteps.cs` als `LockoutSteps.cs` zou definiëren?

3. In oefening 4 gebruik je `_ctx.LastCode` om de 2FA-code op te vangen. Waarom kan je de returnwaarde van `Login(...)` niet gebruiken om de code te achterhalen?

4. Wanneer is een `Scenario Outline` beter dan meerdere afzonderlijke scenario's? En wanneer schrijf je liever afzonderlijke scenario's?
