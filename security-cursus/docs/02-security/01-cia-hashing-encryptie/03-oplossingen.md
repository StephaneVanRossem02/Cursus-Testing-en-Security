---
title: "Les 2: Oplossingen - CIA, Hashing en Encryptie"
sidebar_label: "Oplossingen"
---

# Oplossingen: CIA, Hashing en Encryptie

**Bekijk dit pas nadat je de oefeningen zelf geprobeerd hebt.** Lees de toelichting ook als je het juist had.

---

## Oplossing 1: CustomerAccount testen

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
            account.PasswordHash.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void CustomerAccount_PasswordHash_StartsWithBCryptPrefix()
        {
            // Arrange & Act
            CustomerAccount account = new CustomerAccount("alice@shopwave.be", "wachtwoord123");

            // Assert
            bool startsCorrect = account.PasswordHash.StartsWith("$2a$")
                              || account.PasswordHash.StartsWith("$2b$");
            startsCorrect.Should().BeTrue();
        }

        [Fact]
        public void VerifyPassword_WithCorrectPassword_ReturnsTrue()
        {
            // Arrange
            CustomerAccount account = new CustomerAccount("alice@shopwave.be", "wachtwoord123");

            // Act
            bool result = account.VerifyPassword("wachtwoord123");

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void VerifyPassword_WithWrongPassword_ReturnsFalse()
        {
            // Arrange
            CustomerAccount account = new CustomerAccount("alice@shopwave.be", "wachtwoord123");

            // Act
            bool result = account.VerifyPassword("foutWachtwoord");

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void TwoAccountsWithSamePassword_HaveDifferentHashes()
        {
            // Arrange & Act
            CustomerAccount alice = new CustomerAccount("alice@shopwave.be", "wachtwoord123");
            CustomerAccount bob   = new CustomerAccount("bob@shopwave.be",   "wachtwoord123");

            // Assert
            alice.PasswordHash.Should().NotBe(bob.PasswordHash);
        }
    }
}
```

### Toelichting

Test 1 verifieert de minimale vereiste: de hash is niet leeg. Als BCrypt niet correct geïnstalleerd of aangeroepen is, faalt deze test.

Test 2 verifieert het formaat. Een BCrypt-hash begint altijd met `$2a$` of `$2b$`. Die prefix bevat het algoritme en de kostfactor. Als je per ongeluk SHA-256 gebruikt in plaats van BCrypt, faalt deze test ook al slaagt test 1.

Test 5 is de belangrijkste. Als twee accounts met hetzelfde wachtwoord dezelfde hash zouden hebben, werkt de salt niet. Een aanvaller die de database steelt, zou kunnen zien welke gebruikers hetzelfde wachtwoord gebruiken. BCrypt genereert voor elke aanroep een nieuwe willekeurige salt, dus de hashes zijn altijd verschillend.

**Veelgemaakte fout:** studenten schrijven `account.PasswordHash.Should().Be(BCrypt.Net.BCrypt.HashPassword("wachtwoord123"))`. Dat faalt altijd, want BCrypt genereert elke keer een andere hash. Gebruik `Verify` om te controleren of een wachtwoord klopt, nooit een directe hashvergelijking.

**Veelgemaakte fout:** studenten slaan over dat BCrypt traag is. De tests van `CustomerAccount` duren merkbaar langer dan de unit tests uit les 1, omdat BCrypt meerdere honderd milliseconden nodig heeft per berekening. Dat is geen bug, dat is het ontwerp: bewuste traagheid beschermt tegen brute-force-aanvallen.

---

## Oplossing 2: AccountRepository

```csharp
namespace ShopWave.Security
{
    public class AccountRepository
    {
        private readonly Dictionary<string, CustomerAccount> _accounts;
        private readonly Dictionary<string, int>             _failedAttempts;
        private const int MaxAttempts = 3;

        public AccountRepository()
        {
            _accounts       = new Dictionary<string, CustomerAccount>();
            _failedAttempts = new Dictionary<string, int>();
        }

        public void Register(string email, string password)
        {
            CustomerAccount account = new CustomerAccount(email, password);
            _accounts[email]        = account;
            _failedAttempts[email]  = 0;
        }

        public string Login(string email, string password)
        {
            string result;

            if (!_accounts.ContainsKey(email))
            {
                result = "Gebruiker niet gevonden.";
            }
            else if (_failedAttempts[email] >= MaxAttempts)
            {
                result = "Account geblokkeerd.";
            }
            else
            {
                bool correct = _accounts[email].VerifyPassword(password);

                if (correct)
                {
                    _failedAttempts[email] = 0;
                    result = "Inloggen geslaagd.";
                }
                else
                {
                    _failedAttempts[email]++;

                    if (_failedAttempts[email] >= MaxAttempts)
                    {
                        result = "Account geblokkeerd.";
                    }
                    else
                    {
                        result = "Inloggen mislukt.";
                    }
                }
            }

            return result;
        }
    }
}
```

### Toelichting

De lockout-check staat bewust vóór de wachtwoordverificatie. Als het account al geblokkeerd is, controleer je het wachtwoord niet meer. Dat is correct: een geblokkeerd account mag niet meer proberen, ook niet met het juiste wachtwoord.

`MaxAttempts` is een private constante. Als je het maximum later wil wijzigen, pas je het op één plaats aan. Geen magisch getal verspreid door de code.

**Veelgemaakte fout:** studenten plaatsen de lockout-check na de wachtwoordverificatie. Dan kan je nog één extra poging doen nadat de teller op 3 staat: de derde foute poging zet de teller op 3 en geeft "Account geblokkeerd." terug, maar een vierde poging met het juiste wachtwoord zou dan nog kunnen slagen als de check te laat komt. De volgorde van de checks is cruciaal.

**Veelgemaakte fout:** studenten vergeten de foutenteller te resetten na een succesvolle login. De teller staat dan op 2 na twee foute pogingen. Na een succesvolle login staat hij nog steeds op 2. Eén foute poging daarna blokkeert het account direct. Dat is niet het gewenste gedrag.

---

## Oplossing 3: AccountRepositoryTests

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

        [Fact]
        public void Login_WithWrongPassword_ReturnsMislukt()
        {
            // Arrange
            AccountRepository repository = new AccountRepository();
            repository.Register("alice@shopwave.be", "wachtwoord123");

            // Act
            string result = repository.Login("alice@shopwave.be", "foutWachtwoord");

            // Assert
            result.Should().Be("Inloggen mislukt.");
        }

        [Fact]
        public void Login_WithUnknownEmail_ReturnsNietGevonden()
        {
            // Arrange
            AccountRepository repository = new AccountRepository();

            // Act
            string result = repository.Login("onbekend@shopwave.be", "wachtwoord");

            // Assert
            result.Should().Be("Gebruiker niet gevonden.");
        }

        [Fact]
        public void Login_AfterThreeWrongAttempts_ReturnsGeblokkeerd()
        {
            // Arrange
            AccountRepository repository = new AccountRepository();
            repository.Register("alice@shopwave.be", "wachtwoord123");
            repository.Login("alice@shopwave.be", "fout1");
            repository.Login("alice@shopwave.be", "fout2");
            repository.Login("alice@shopwave.be", "fout3");

            // Act
            string result = repository.Login("alice@shopwave.be", "wachtwoord123");

            // Assert
            result.Should().Be("Account geblokkeerd.");
        }

        [Fact]
        public void Login_AfterSuccessfulLogin_ResetsFailedCounter()
        {
            // Arrange
            AccountRepository repository = new AccountRepository();
            repository.Register("alice@shopwave.be", "wachtwoord123");
            repository.Login("alice@shopwave.be", "fout1");
            repository.Login("alice@shopwave.be", "fout2");
            repository.Login("alice@shopwave.be", "wachtwoord123"); // reset

            // Act
            string result = repository.Login("alice@shopwave.be", "foutNaReset");

            // Assert
            result.Should().Be("Inloggen mislukt.");
        }
    }
}
```

### Toelichting

Test 4 is de lockout-test. De drie foute pogingen staan in het Arrange-gedeelte, niet in het Act-gedeelte. Het Act-gedeelte is de vierde poging. Dat is de actie die je wil testen: wat geeft de repository terug als het account al geblokkeerd is?

Test 5 verifieert de reset. De succesvolle login in het Arrange-gedeelte is de actie die de teller reset. De Act is een foute poging daarna. Als de teller correct gereset is, geeft die foute poging "Inloggen mislukt." terug in plaats van "Account geblokkeerd."

**Veelgemaakte fout:** studenten schrijven de drie foute pogingen in het Act-gedeelte. Dat klopt conceptueel niet: het Act-gedeelte bevat de actie die je wil testen. De voorbereiding hoort in Arrange.

**Veelgemaakte fout:** studenten testen niet dat een geblokkeerd account ook geblokkeerd blijft als daarna het juiste wachtwoord ingevoerd wordt. Test 4 test dat expliciet: de vierde poging in Act gebruikt het juiste wachtwoord, maar het account is al geblokkeerd.

---

## Oplossing 4: OrderEncryptor

### OrderEncryptor.cs

```csharp
namespace ShopWave.Security
{
    public class OrderEncryptor
    {
        private const string Key = "ShopWaveGeheimeSleutel2024AB!@#$";

        public string EncryptOrderData(string orderData)
        {
            AesEncryptor encryptor = new AesEncryptor(Key);
            return encryptor.Encrypt(orderData);
        }

        public string DecryptOrderData(string encryptedData)
        {
            AesEncryptor encryptor = new AesEncryptor(Key);
            return encryptor.Decrypt(encryptedData);
        }
    }
}
```

### OrderEncryptorTests.cs

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
            encrypted.Should().NotBe(original);
        }

        [Fact]
        public void DecryptOrderData_ReturnsOriginalString()
        {
            // Arrange
            OrderEncryptor encryptor = new OrderEncryptor();
            string original          = "alice@shopwave.be | Laptop | 999.99 EUR";
            string encrypted         = encryptor.EncryptOrderData(original);

            // Act
            string decrypted = encryptor.DecryptOrderData(encrypted);

            // Assert
            decrypted.Should().Be(original);
        }

        [Fact]
        public void EncryptOrderData_TwiceSameInput_GivesDifferentCiphertexts()
        {
            // Arrange
            OrderEncryptor encryptor = new OrderEncryptor();
            string original          = "alice@shopwave.be | Laptop | 999.99 EUR";

            // Act
            string encrypted1 = encryptor.EncryptOrderData(original);
            string encrypted2 = encryptor.EncryptOrderData(original);

            // Assert
            encrypted1.Should().NotBe(encrypted2);
            encryptor.DecryptOrderData(encrypted1).Should().Be(original);
            encryptor.DecryptOrderData(encrypted2).Should().Be(original);
        }
    }
}
```

### Toelichting

Test 1 bewijst dat de versleutelde waarde verschilt van de originele. Dit klinkt triviaal maar is de minimale controle: als encryptie niets doet (geen sleutel, geen operatie), geeft `EncryptOrderData` de invoer ongewijzigd terug en slaagt elke test die `decrypted.Should().Be(original)` controleert.

Test 3 is de belangrijkste. Als de IV niet willekeurig is maar elke keer hetzelfde, zijn `encrypted1` en `encrypted2` identiek. Een aanvaller die twee identieke versleutelde waarden ziet in de database, weet dat de originele data ook identiek is. De laatste twee assertions in test 3 bewijzen dat beide ciphertexts correct ontsleuteld kunnen worden.

**Veelgemaakte fout:** studenten definiëren de `Key` als een constante van minder dan 32 tekens zonder rekening te houden met de padding in `AesEncryptor`. `AesEncryptor` doet `PadRight(32).Substring(0, 32)`, dus de sleutel wordt altijd afgekapt of aangevuld tot precies 32 tekens. Als je een sleutel van 32 tekens kiest, is het gedrag voorspelbaar.

**Veelgemaakte fout:** studenten proberen de `Key` mee te geven via de constructor van `OrderEncryptor`. De opdracht vraagt dat de sleutel intern verborgen is. De rest van de applicatie hoeft de sleutel niet te kennen.

---

## Oplossing 5: CIA-analyse

**Situatie A: plain-text wachtwoorden**

Pijler: Confidentiality.

Impact: een aanvaller met toegang tot de database heeft direct de wachtwoorden van alle klanten. Omdat veel mensen hetzelfde wachtwoord op meerdere sites gebruiken, kan de aanvaller ook andere accounts overnemen. ShopWave is juridisch aansprakelijk voor het lek.

Oplossing: wachtwoorden hashen met BCrypt. Als de database uitlekt, ziet een aanvaller alleen BCrypt-hashes. Die zijn computationeel onhaalbaar om terug te rekenen naar de originele wachtwoorden.

**Situatie B: prijs aangepast in database**

Pijler: Integrity.

Impact: ShopWave levert producten onder kostprijs zonder het te weten. Financiële schade. Als dit ontdekt wordt, schaadt het het vertrouwen van klanten en leveranciers.

Oplossing: digitale handtekeningen of hashcontroles op kritieke velden. Elke wijziging in een prijs wordt gedetecteerd omdat de hashwaarde niet meer klopt. Auditlogs bijhouden zodat alle wijzigingen traceerbaar zijn.

**Situatie C: DDoS-aanval**

Pijler: Availability.

Impact: vier uur niet beschikbaar tijdens een promotieperiode betekent verlies van alle bestellingen in die periode. Klanten gaan naar concurrenten. Reputatieschade die lang kan aanhouden.

Oplossing: DDoS-bescherming via een CDN (Content Delivery Network) of een gespecialiseerde service. Automatische failover naar een back-upserver. Rate limiting op de API.

**Situatie D: klantenlijst naar verkeerde ontvanger**

Pijler: Confidentiality.

Impact: persoonsgegevens van klanten zijn gelekt bij een onbevoegde. Dit is een inbreuk op de AVG (Algemene Verordening Gegevensbescherming). ShopWave is verplicht het lek te melden aan de Gegevensbeschermingsautoriteit en mogelijk ook aan de getroffen klanten. Boetes kunnen oplopen tot 4% van de jaaromzet.

Oplossing: toegangsbeheer op gevoelige exports (niet iedereen mag klantenlijsten exporteren), e-mailcontroles via DLP-software (Data Loss Prevention), training van medewerkers.

**Reflectievraag:** in situatie B is de aanvaller erin geslaagd de database te wijzigen. Dat wijst ook op een probleem met Confidentiality: de aanvaller had schrijftoegang tot de database. Beveiligingsproblemen zijn zelden gebonden aan één pijler.
