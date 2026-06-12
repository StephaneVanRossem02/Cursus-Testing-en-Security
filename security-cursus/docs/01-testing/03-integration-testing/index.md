---
title: "Les 5: Integration Testing"
sidebar_label: "Integration Testing"
---

# Les 5: Integration Testing

## ShopWave

In de voorbije vier lessen bouwden we klasse per klasse aan ShopWave. Elke klasse werd apart getest. `CustomerAccount` werkt correct. `TwoFactorService` werkt correct. `OrderSigner` werkt correct. Maar kloppen ze ook samen?

Dat is precies de vraag die integration testing beantwoordt.

---

## Theorie

### 1. Wat unit tests niet testen

Unit tests testen klassen in **isolatie**. Via mocks vervangen we elke afhankelijkheid door een nep-versie die we volledig controleren. Dat is hun kracht — maar ook hun beperking.

```
Unit test van OrderService:
  - IPaymentGateway  → Mock (nep)
  - IStockService    → Mock (nep)
  - OrderService     → Echt

Wat testen we? Alleen de logica in OrderService zelf.
Wat testen we NIET? Of OrderService en AccountRepository correct samenwerken.
```

Stel dat `AccountRepository.Login` in de echte implementatie de loginpoging logt in een interne lijst, en `CheckoutService` verwacht dat die lijst leeg is na een succesvolle 2FA. Een unit test met een mock van `AccountRepository` detecteert dat probleem nooit — de mock doet gewoon wat je hem vertelt.

Andere gevallen die unit tests missen:
- Een klasse retourneert `null` in een specifiek scenario dat de mock nooit nabootst
- De volgorde van aanroepen tussen twee klassen is verkeerd
- Een constructor in klasse A initialiseert iets dat klasse B nodig heeft, maar de mock doet dat niet

---

### 2. Wat is een integration test?

Een **integration test** test de samenwerking van twee of meer **echte klassen**, zonder mocks voor eigen code.

```
Integration test van de checkout-flow:
  - TwoFactorService   → Echt
  - AccountRepository  → Echt
  - CustomerAccount    → Echt
  - OrderSigner        → Echt
  - CertificateHelper  → Echt

Wat testen we? De volledige flow zoals die in productie ook werkt.
```

De klassen werken samen zoals in productie. Als er een integratieprobleem is — een verkeerde aanname over wat een methode teruggeeft, een volgordekwestie, een initialisatieprobleem — dan detecteert de integration test dat.

---

### 3. De testpiramide

Een goede teststrategie combineert drie niveaus, gevisualiseerd als een piramide:

```
          /\
         /  \
        / E2E\        ← weinig, traag, fragiel
       /------\
      /          \
     / Integration \   ← matig aantal, medium snelheid
    /--------------\
   /                \
  /   Unit tests    \  ← veel, snel, goedkoop
 /____________________\
```

| Niveau | Snelheid | Scope | Aantal |
|--------|----------|-------|--------|
| Unit | Zeer snel | Één klasse | Veel |
| Integration | Trager | Meerdere klassen samen | Matig |
| E2E | Traag | Volledige applicatie via UI | Weinig |

De brede basis zijn unit tests. Integration tests vormen de middelste laag: minder in aantal, maar ze testen wat unit tests niet kunnen. E2E tests komen bovenaan: ze testen de volledige applicatie maar zijn fragiel en traag.

---

### 4. Unit test vs integration test

| | Unit test | Integration test |
|--|-----------|-----------------|
| Eigen klassen | Gemockt (nep) | Echt |
| Externe diensten | Gemockt | Gemockt (mock server) |
| Snelheid | Snel | Trager |
| Foutdetectie | Logica in één klasse | Samenwerking tussen klassen |
| Test type | White box | Black box |
| Fragiliteit bij refactoring | Hoog | Laag |

**White box** — je weet precies welke methode in welke klasse je test en wat de interne vertakkingen zijn.

**Black box** — je test het gedrag van de combinatie. Je weet niet (en het maakt niet uit) welke interne stappen er precies doorlopen worden. Input en verwachte output zijn wat telt.

---

### 5. Wanneer gebruik je welk type?

**Unit test** wanneer:
- Je een specifieke berekenng of logica wil afzonderlijk verifiëren (zoals een kortingsberekening)
- Je randgevallen wil testen die moeilijk te reproduceren zijn met echte afhankelijkheden
- Je snel feedback wil tijdens ontwikkeling

**Integration test** wanneer:
- Je wil verifiëren dat klassen correct samenwerken
- Je een volledige use case wil testen zoals "gebruiker logt in, plaatst bestelling, bestelling wordt ondertekend"
- Je wil controleren dat een refactoring niets heeft gebroken in de samenwerking

---

## Demo

We testen de volledige ShopWave-flow met echte klassen. Geen mocks voor eigen code.

De flow die we testen:
1. Gebruiker registreert
2. Gebruiker logt in → 2FA-code wordt gegenereerd
3. Gebruiker verifyieert 2FA-code → inloggen geslaagd
4. Bestelling wordt aangemaakt en digitaal ondertekend
5. Handtekening wordt geverifieerd

---

### Stap 1 — TwoFactorService aanpassen voor testbaarheid

In een unit test mocken we `ITwoFactorService` en bepalen we de code zelf. In een integration test gebruiken we de echte `TwoFactorService` — maar dan kennen we de gegenereerde code niet.

De nette oplossing: een optionele **callback** die de code doorstuurt wanneer hij gegenereerd wordt. In productie laat je de callback weg. In tests geef je een lambda mee die de code opvangt.

Pas `Security/TwoFactorService.cs` aan:

```csharp
using System.Security.Cryptography;

namespace ShopWave.Security
{
    public class TwoFactorService : ITwoFactorService
    {
        private readonly Dictionary<string, PendingCode> pendingCodes;
        private readonly int validitySeconds;
        private readonly Action<string, string> onCodeGenerated;

        // Constructor voor productiegebruik — geen callback nodig
        public TwoFactorService(int validitySeconds = 30)
        {
            pendingCodes = new Dictionary<string, PendingCode>();
            this.validitySeconds = validitySeconds;
            onCodeGenerated = null;
        }

        // Constructor voor integration testing — callback vangt de gegenereerde code op
        public TwoFactorService(int validitySeconds, Action<string, string> onCodeGenerated)
        {
            pendingCodes = new Dictionary<string, PendingCode>();
            this.validitySeconds = validitySeconds;
            this.onCodeGenerated = onCodeGenerated;
        }

        public string GenerateCode(string email)
        {
            string code = RandomNumberGenerator.GetInt32(100000, 999999).ToString();
            DateTime expiresAt = DateTime.UtcNow.AddSeconds(validitySeconds);

            pendingCodes[email] = new PendingCode(code, expiresAt);

            if (onCodeGenerated != null)
            {
                onCodeGenerated(email, code);
            }

            return code;
        }

        public bool VerifyCode(string email, string code)
        {
            bool isValid = false;

            if (pendingCodes.ContainsKey(email))
            {
                PendingCode pending = pendingCodes[email];

                if (DateTime.UtcNow <= pending.ExpiresAt && pending.Code == code)
                {
                    isValid = true;
                }

                pendingCodes.Remove(email);
            }

            return isValid;
        }
    }
}
```

Waarom is dit een goede aanpak?
- De productionele code verandert niet in gedrag — de callback is optioneel
- We hoeven geen getter of publieke eigenschap toe te voegen aan `TwoFactorService`
- De testcode is volledig in de test zelf, niet verspreid over de klasse

---

### Stap 2 — CheckoutService aanmaken

`CheckoutService` coördineert de volledige checkout. Maak `CheckoutService.cs` aan in de map `ShopWave`:

```csharp
using ShopWave.Security;

namespace ShopWave
{
    public class CheckoutService
    {
        private readonly AccountRepository accountRepository;
        private readonly OrderSigner orderSigner;

        public CheckoutService(AccountRepository accountRepository, OrderSigner orderSigner)
        {
            this.accountRepository = accountRepository;
            this.orderSigner = orderSigner;
        }

        public CheckoutResult Checkout(
            string email,
            string twoFactorCode,
            string productName,
            double price)
        {
            CheckoutResult result;

            string verifyResult = accountRepository.VerifyTwoFactor(email, twoFactorCode);

            if (verifyResult != "Inloggen geslaagd.")
            {
                result = new CheckoutResult(false, "Checkout geweigerd: authenticatie mislukt.", string.Empty);
            }
            else
            {
                string priceText = price.ToString(System.Globalization.CultureInfo.InvariantCulture);
                string orderData = email + " | " + productName + " | " + priceText + " EUR";
                string signature = orderSigner.Sign(orderData);
                string message = "Bestelling bevestigd. Totaal: " + priceText + " EUR";
                result = new CheckoutResult(true, message, signature);
            }

            return result;
        }
    }
}
```

---

### Stap 3 — Integration test schrijven

Maak `CheckoutIntegrationTests.cs` aan in `ShopWave.Tests`:

```csharp
using FluentAssertions;
using ShopWave;
using ShopWave.Security;
using System.Security.Cryptography.X509Certificates;

namespace ShopWave.Tests
{
    public class CheckoutIntegrationTests
    {
        private string capturedCode = string.Empty;

        private void OnCodeGenerated(string email, string code)
        {
            capturedCode = code;
        }

        [Fact]
        public void Checkout_ValidAuthFlow_ReturnsSuccessAndValidSignature()
        {
            // Arrange — echte klassen, geen mocks
            TwoFactorService twoFactorService = new TwoFactorService(
                validitySeconds: 60,
                onCodeGenerated: OnCodeGenerated
            );

            AccountRepository accountRepository = new AccountRepository(twoFactorService);
            accountRepository.Register("alice@shopwave.be", "wachtwoord123");

            X509Certificate2 certificate = CertificateHelper.CreateSelfSignedCertificate("ShopWave");
            OrderSigner orderSigner = new OrderSigner(certificate);

            CheckoutService checkoutService = new CheckoutService(accountRepository, orderSigner);

            // Stap 1: login — dit triggert GenerateCode → OnCodeGenerated vangt de code op
            string loginResult = accountRepository.Login("alice@shopwave.be", "wachtwoord123");
            loginResult.Should().Be("2FA vereist.");
            capturedCode.Should().NotBeNullOrEmpty();

            // Act — checkout met de echte gegenereerde code
            CheckoutResult result = checkoutService.Checkout(
                "alice@shopwave.be", capturedCode, "Laptop", 999.99);

            // Assert
            result.Success.Should().BeTrue();
            result.Message.Should().Contain("Bestelling bevestigd");
            result.Signature.Should().NotBeNullOrEmpty();

            // Verifieer de handtekening ook — echte OrderSigner
            string priceText = (999.99).ToString(System.Globalization.CultureInfo.InvariantCulture);
            string expectedOrderData = "alice@shopwave.be | Laptop | " + priceText + " EUR";
            bool signatureValid = orderSigner.Verify(expectedOrderData, result.Signature);
            signatureValid.Should().BeTrue();
        }

        [Fact]
        public void Checkout_WithWrongTwoFactorCode_ReturnsFailure()
        {
            // Arrange
            TwoFactorService twoFactorService = new TwoFactorService();
            AccountRepository accountRepository = new AccountRepository(twoFactorService);
            accountRepository.Register("alice@shopwave.be", "wachtwoord123");

            X509Certificate2 certificate = CertificateHelper.CreateSelfSignedCertificate("ShopWave");
            OrderSigner orderSigner = new OrderSigner(certificate);

            CheckoutService checkoutService = new CheckoutService(accountRepository, orderSigner);

            accountRepository.Login("alice@shopwave.be", "wachtwoord123");

            // Act
            CheckoutResult result = checkoutService.Checkout(
                "alice@shopwave.be", "000000", "Laptop", 999.99);

            // Assert
            result.Success.Should().BeFalse();
            result.Signature.Should().BeEmpty();
        }
    }
}
```

Wat maakt dit een integration test en geen unit test?
- `TwoFactorService` is echt — willekeurige codes, echte tijdslimiet
- `AccountRepository` is echt — echte lockout, echte wachtwoordhashing
- `CustomerAccount` is echt — echte salt, echte SHA-256 hash
- `OrderSigner` is echt — echte RSA-handtekening via echt X.509-certificaat
- Geen enkele `Mock<>` in de test

---

### Stap 4 — Verschil tonen: hetzelfde scenario als unit test

Ter vergelijking: zo zou dezelfde checkout-test er als **unit test** uitzien:

```csharp
[Fact]
public void Checkout_UnitTest_ValidAuth_ReturnsSuccess()
{
    // Arrange — alles gemockt
    Mock<AccountRepository> mockRepo   = new Mock<AccountRepository>();
    Mock<OrderSigner>       mockSigner = new Mock<OrderSigner>();

    mockRepo.Setup(r => r.VerifyTwoFactor("alice@shopwave.be", "123456"))
            .Returns("Inloggen geslaagd.");

    mockSigner.Setup(s => s.Sign(It.IsAny<string>()))
              .Returns("nep-handtekening");

    CheckoutService service = new CheckoutService(mockRepo.Object, mockSigner.Object);

    // Act
    CheckoutResult result = service.Checkout("alice@shopwave.be", "123456", "Laptop", 999.99);

    // Assert
    result.Success.Should().BeTrue();
}
```

De unit test is sneller en test de logica van `CheckoutService` in isolatie. Maar ze garandeert niet dat het echte `AccountRepository` correct samenwerkt met de echte `TwoFactorService`. Beide types zijn nodig.

---

## Oefeningen

### Oefening 1 — Volledige loginflow als integration test

Schrijf `LoginFlowIntegrationTests.cs` in `ShopWave.Tests`.

Test de volledige loginflow met echte klassen:

1. Registreer een account en login met het correcte wachtwoord → verwacht `"2FA vereist."`
2. Verifieer de echte gegenereerde 2FA-code → verwacht `"Inloggen geslaagd."`
3. Probeer dezelfde 2FA-code een tweede keer → verwacht mislukking (code is al verbruikt)
4. Login met een fout wachtwoord → verwacht `"Inloggen mislukt."`
5. Login drie keer met een fout wachtwoord → verwacht `"Account geblokkeerd."` na de derde poging
6. Login na lockout met het correcte wachtwoord → verwacht nog steeds `"Account geblokkeerd."`

Gebruik de `onCodeGenerated`-callback om de 2FA-code op te vangen.

---

### Oefening 2 — Verlopen 2FA-code

Schrijf een integration test die verifieert dat een verlopen 2FA-code de checkout tegenhoudt.

Gebruik `TwoFactorService` met `validitySeconds: 0` en een korte `Thread.Sleep(10)` zodat de code zeker verlopen is voor de checkout-poging.

Verwacht resultaat: `Checkout.Success == false`.

---

### Oefening 3 — Handtekeningintegriteit na manipulatie

Schrijf een integration test `OrderIntegrityIntegrationTests.cs`:

1. Voer een volledige checkout uit voor een geldige gebruiker
2. Verifieer dat de handtekening geldig is voor de originele orderdata
3. Simuleer een manipulatie: verander de prijs in de orderstring
4. Verifieer dat de handtekening **ongeldig** is voor de gemanipuleerde data
5. Schrijf een duidelijk commentaar boven elke assert dat uitlegt wat je verifieert en waarom dat belangrijk is vanuit het CIA-model

---

### Oefening 4 — CheckoutService met CartService

Breid `CheckoutService` uit zodat hij ook de `CartService` uit les 3 integreert.

Voeg een overload toe aan `Checkout` die een `CartService` accepteert en de prijs berekent als `cartService.Total`.

Schrijf een integration test die:
1. Artikelen toevoegt aan de `CartService`
2. Een login + 2FA-flow uitvoert
3. Een checkout uitvoert op basis van het mandjebedrag
4. De handtekening verifieert met het correcte totaalbedrag

---

### Oefening 5 — Meerdere accounts tegelijk

Schrijf een integration test die twee accounts parallel door de flow loodst:

1. Registreer `alice@shopwave.be` en `bob@shopwave.be`
2. Login beide accounts
3. Vang de 2FA-codes op voor beide (twee aparte callbacks of één met een dictionary)
4. Verifieer beide codes
5. Voer een checkout uit voor beide accounts
6. Verifieer dat de handtekeningen van Alice en Bob **onderling niet uitwisselbaar zijn** — de handtekening van Alice is ongeldig voor de orderdata van Bob

---

## Modeloplossing

> De modeloplossing is beschikbaar na het indienen van de labo-opdracht via Digitap.

---

### Modeloplossing Oefening 1 — LoginFlowIntegrationTests

```csharp
using FluentAssertions;
using ShopWave.Security;

namespace ShopWave.Tests
{
    public class LoginFlowIntegrationTests
    {
        [Fact]
        public void Login_CorrectPassword_ThenVerifyCode_ReturnsGeslaagd()
        {
            // Arrange
            string capturedCode = string.Empty;

            TwoFactorService twoFactorService = new TwoFactorService(
                validitySeconds: 60,
                onCodeGenerated: (email, code) => { capturedCode = code; }
            );

            AccountRepository repository = new AccountRepository(twoFactorService);
            repository.Register("alice@shopwave.be", "wachtwoord123");

            // Act
            string loginResult  = repository.Login("alice@shopwave.be", "wachtwoord123");
            string verifyResult = repository.VerifyTwoFactor("alice@shopwave.be", capturedCode);

            // Assert
            loginResult.Should().Be("2FA vereist.");
            verifyResult.Should().Be("Inloggen geslaagd.");
        }

        [Fact]
        public void Login_SameCodeUsedTwice_SecondVerifyFails()
        {
            // Arrange
            string capturedCode = string.Empty;

            TwoFactorService twoFactorService = new TwoFactorService(
                validitySeconds: 60,
                onCodeGenerated: (email, code) => { capturedCode = code; }
            );

            AccountRepository repository = new AccountRepository(twoFactorService);
            repository.Register("alice@shopwave.be", "wachtwoord123");
            repository.Login("alice@shopwave.be", "wachtwoord123");

            // Act
            string firstVerify  = repository.VerifyTwoFactor("alice@shopwave.be", capturedCode);
            string secondVerify = repository.VerifyTwoFactor("alice@shopwave.be", capturedCode);

            // Assert
            firstVerify.Should().Be("Inloggen geslaagd.");
            secondVerify.Should().NotBe("Inloggen geslaagd.");
        }

        [Fact]
        public void Login_ThreeWrongPasswords_BlocksAccount()
        {
            // Arrange
            TwoFactorService  twoFactorService  = new TwoFactorService();
            AccountRepository repository        = new AccountRepository(twoFactorService);
            repository.Register("alice@shopwave.be", "wachtwoord123");

            // Act
            repository.Login("alice@shopwave.be", "fout1");
            repository.Login("alice@shopwave.be", "fout2");
            string thirdResult   = repository.Login("alice@shopwave.be", "fout3");
            string afterLockout  = repository.Login("alice@shopwave.be", "wachtwoord123");

            // Assert
            thirdResult.Should().Be("Account geblokkeerd.");
            afterLockout.Should().Be("Account geblokkeerd.");
        }
    }
}
```

---

## Samenvatting

| Concept | Wat je moet onthouden |
|---------|-----------------------|
| Integration test | Meerdere echte klassen samen — geen mocks voor eigen code |
| Testpiramide | Veel unit tests → minder integration tests → weinig E2E |
| Black box | Je test gedrag (input/output), niet interne implementatie |
| White box | Je test interne logica van één klasse, je kent de vertakkingen |
| Testcallback | Techniek om gegenereerde waarden op te vangen zonder de klasse te mocken |
| Wanneer unit? | Logica van één klasse, randgevallen, snelle feedback |
| Wanneer integration? | Samenwerking tussen klassen, volledige use case, robuust bij refactoring |

---

## Volgende les

Les 6 is **Security: HTTPS, TLS en certificaten**. We bekijken hoe X.509-certificaten uit les 4 gebruikt worden in het TLS-protocol, hoe HTTPS werkt en hoe je een beveiligde verbinding configureert voor een ShopWave-endpoint.