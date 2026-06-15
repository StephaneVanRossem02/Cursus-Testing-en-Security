---
title: "Les 2: Oefeningen - CIA, Hashing en Encryptie"
sidebar_label: "Oefeningen"
---

# Oefeningen: CIA, Hashing en Encryptie

Werk de oefeningen in volgorde. Elke oefening bouwt verder op de vorige. Kijk niet vooraf in de oplossingen.

Je werkt in de bestaande ShopWave-solution uit les 1. Alle nieuwe klassen maak je aan in de map `ShopWave/Security/`. Alle testklassen maak je aan in `ShopWave.Tests/`.

---

<h3 class="opdracht-titel">Opdracht</h3>

## Oefening 1: CustomerAccount testen

**Leerdoel:** je leert tests schrijven voor een beveiligingsklasse en begrijpt welke eigenschappen van BCrypt je kan verifiëren zonder de interne werking te kennen.

**Moeilijkheidsgraad:** basis

**Situatie:** de demo bouwde `CustomerAccount` met BCrypt-hashing. Jij schrijft de tests die bewijzen dat de klasse correct werkt.

**Wat je doet:**

Maak `ShopWave.Tests/CustomerAccountTests.cs` aan en schrijf tests voor de volgende vijf scenario's:

1. Na het aanmaken van een `CustomerAccount` is `PasswordHash` niet leeg
2. De `PasswordHash` begint met `$2a$` of `$2b$` (het BCrypt-formaat)
3. `VerifyPassword` geeft `true` terug bij het correcte wachtwoord
4. `VerifyPassword` geeft `false` terug bij een fout wachtwoord
5. Twee `CustomerAccount`-objecten met hetzelfde wachtwoord hebben een **andere** `PasswordHash`

Gebruik FluentAssertions voor alle assertions.

**Startcode:**

```csharp
using FluentAssertions;
using ShopWave.Security;

namespace ShopWave.Tests
{
    public class CustomerAccountTests
    {
        [Fact]
        public void CustomerAccount_AfterCreation_PasswordHashIsNotEmpty()
        {
            // Arrange & Act
            CustomerAccount account = new CustomerAccount("alice@shopwave.be", "wachtwoord123");

            // Assert
            // jouw assertion hier
        }

        [Fact]
        public void CustomerAccount_PasswordHash_StartsWithBCryptPrefix()
        {
            // Arrange & Act
            CustomerAccount account = new CustomerAccount("alice@shopwave.be", "wachtwoord123");

            // Assert
            // Tip: controleer op "$2a$" OF "$2b$"
        }

        // Voeg de overige drie tests zelf toe
    }
}
```

**Verwacht resultaat:**

```
✓ CustomerAccount_AfterCreation_PasswordHashIsNotEmpty
✓ CustomerAccount_PasswordHash_StartsWithBCryptPrefix
✓ VerifyPassword_WithCorrectPassword_ReturnsTrue
✓ VerifyPassword_WithWrongPassword_ReturnsFalse
✓ TwoAccountsWithSamePassword_HaveDifferentHashes
```

---

<h3 class="opdracht-titel">Opdracht</h3>

## Oefening 2: AccountRepository implementeren

**Leerdoel:** je implementeert een klasse die meerdere beveiligingsregels combineert: wachtwoordverificatie, foutentelling en lockout.

**Moeilijkheidsgraad:** basis

**Situatie:** ShopWave heeft een inlogsysteem nodig. Na drie opeenvolgende foute pogingen wordt een account geblokkeerd om brute-force-aanvallen te voorkomen.

**Wat je doet:**

Maak `ShopWave/Security/AccountRepository.cs` aan met de volgende signatuur:

```csharp
namespace ShopWave.Security
{
    public class AccountRepository
    {
        public void Register(string email, string password) { ... }
        public string Login(string email, string password)  { ... }
    }
}
```

`Login` geeft precies één van de volgende strings terug:

| Situatie | Returnwaarde |
|----------|-------------|
| Correct wachtwoord | `"Inloggen geslaagd."` |
| Fout wachtwoord | `"Inloggen mislukt."` |
| Drie opeenvolgende foute pogingen | `"Account geblokkeerd."` |
| E-mailadres bestaat niet | `"Gebruiker niet gevonden."` |

**Aanvullende regels:**
- De foutenteller wordt gereset na een succesvolle login
- Een geblokkeerd account blijft geblokkeerd, ook als daarna het juiste wachtwoord ingevoerd wordt
- Gebruik `CustomerAccount` intern voor wachtwoordopslag en verificatie

**Tip:** je hebt twee interne datastructuren nodig: één voor de accounts en één voor de foutentellers.

---

<h3 class="opdracht-titel">Opdracht</h3>

## Oefening 3: AccountRepository testen

**Leerdoel:** je leert tests schrijven voor een klasse met toestand (de foutenteller verandert tussen aanroepen).

**Moeilijkheidsgraad:** gemiddeld

**Situatie:** je hebt `AccountRepository` geïmplementeerd in oefening 2. Nu schrijf je de tests die bewijzen dat alle scenario's correct werken.

**Wat je doet:**

Maak `ShopWave.Tests/AccountRepositoryTests.cs` aan en schrijf tests voor minstens de volgende vijf scenario's:

1. Registreer een gebruiker en log in met het juiste wachtwoord: resultaat is `"Inloggen geslaagd."`
2. Log in met een fout wachtwoord: resultaat is `"Inloggen mislukt."`
3. Log in met een onbestaand e-mailadres: resultaat is `"Gebruiker niet gevonden."`
4. Drie opeenvolgende foute pogingen, daarna het juiste wachtwoord: resultaat is `"Account geblokkeerd."`
5. Na een succesvolle login wordt de teller gereset: een nieuwe foute poging is `"Inloggen mislukt."` (niet meteen `"Account geblokkeerd."`)

**Startcode:**

```csharp
using FluentAssertions;
using ShopWave.Security;

namespace ShopWave.Tests
{
    public class AccountRepositoryTests
    {
        [Fact]
        public void Login_WithCorrectPassword_ReturnsGeslaagd()
        {
            // Arrange
            AccountRepository repository = new AccountRepository();
            repository.Register("alice@shopwave.be", "wachtwoord123");

            // Act
            string result = repository.Login("alice@shopwave.be", "wachtwoord123");

            // Assert
            result.Should().Be("Inloggen geslaagd.");
        }

        // Voeg de overige vier tests zelf toe
    }
}
```

**Tip voor de lockout-test:** roep `Login` drie keer aan met een fout wachtwoord in het Arrange-gedeelte, daarna de vierde aanroep in het Act-gedeelte.

**Verwacht resultaat:**

```
✓ Login_WithCorrectPassword_ReturnsGeslaagd
✓ Login_WithWrongPassword_ReturnsMislukt
✓ Login_WithUnknownEmail_ReturnsNietGevonden
✓ Login_AfterThreeWrongAttempts_ReturnsGeblokkeerd
✓ Login_AfterSuccessfulLogin_ResetsFailedCounter
```

---

<h3 class="opdracht-titel">Opdracht</h3>

## Oefening 4: OrderEncryptor implementeren en testen

**Leerdoel:** je implementeert een klasse die `AesEncryptor` omhult en schrijft tests die de eigenschappen van AES-encryptie verifiëren.

**Moeilijkheidsgraad:** gemiddeld

**Situatie:** ShopWave wil gevoelige ordergegevens versleuteld opslaan. `AesEncryptor` uit de demo werkt rechtstreeks met een sleutel. `OrderEncryptor` verbergt die sleutel intern zodat de rest van de applicatie er niet over hoeft na te denken.

**Wat je doet:**

Maak `ShopWave/Security/OrderEncryptor.cs` aan:

```csharp
namespace ShopWave.Security
{
    public class OrderEncryptor
    {
        public string EncryptOrderData(string orderData)     { ... }
        public string DecryptOrderData(string encryptedData) { ... }
    }
}
```

- Definieer de AES-sleutel als `private const string` in de klasse
- Gebruik `AesEncryptor` intern
- De IV wordt automatisch willekeurig gegenereerd per encryptie via `AesEncryptor`

Maak daarna `ShopWave.Tests/OrderEncryptorTests.cs` aan en schrijf tests voor minstens drie scenario's:

1. Versleutelde data is niet gelijk aan de originele string
2. Ontsleutelen van versleutelde data geeft de originele string terug
3. Twee keer dezelfde string versleutelen geeft twee **verschillende** ciphertexts, maar ontsleutelen van elk resultaat geeft telkens de originele string terug

**Startcode voor de tests:**

```csharp
using FluentAssertions;
using ShopWave.Security;

namespace ShopWave.Tests
{
    public class OrderEncryptorTests
    {
        [Fact]
        public void EncryptOrderData_ReturnsValueDifferentFromOriginal()
        {
            // Arrange
            OrderEncryptor encryptor = new OrderEncryptor();
            string original          = "alice@shopwave.be | Laptop | 999.99 EUR";

            // Act
            string encrypted = encryptor.EncryptOrderData(original);

            // Assert
            // jouw assertion hier
        }

        // Voeg de overige twee tests zelf toe
    }
}
```

**Verwacht resultaat:**

```
✓ EncryptOrderData_ReturnsValueDifferentFromOriginal
✓ DecryptOrderData_ReturnsOriginalString
✓ EncryptOrderData_TwiceSameInput_GivesDifferentCiphertexts
```

---

<h3 class="opdracht-titel">Opdracht</h3>

## Oefening 5: CIA-analyse van ShopWave

**Leerdoel:** je past het CIA-model toe op concrete situaties en koppelt beveiligingsproblemen aan technische oplossingen.

**Moeilijkheidsgraad:** basis

Bepaal voor elke situatie hieronder:
- Welke CIA-pijler(s) worden geschonden?
- Wat is de concrete impact voor ShopWave en haar klanten?
- Welke technische maatregel lost het probleem op?

Schrijf je antwoorden op papier of in een tekstbestand.

**Situatie A:** een aanvaller heeft toegang gekregen tot de database van ShopWave. Alle wachtwoorden zijn opgeslagen als plain text.

**Situatie B:** een aanvaller slaagt erin de prijs van een product te wijzigen van 999 euro naar 1 euro in de database, zonder dat iemand dit merkt.

**Situatie C:** ShopWave wordt aangevallen met een DDoS-aanval. De webshop is vier uur lang niet bereikbaar tijdens een drukke promotieperiode.

**Situatie D:** een medewerker stuurt per ongeluk de volledige klantenlijst, inclusief adressen en telefoonnummers, via e-mail naar een verkeerde ontvanger.
