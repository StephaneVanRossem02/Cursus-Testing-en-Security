---
title: "Les 4: Security — 2FA, digitale handtekeningen en X.509-certificaten"
sidebar_label: "2FA, Handtekeningen & X.509"
---

# Les 4: Security — 2FA, digitale handtekeningen en X.509-certificaten

## ShopWave

ShopWave groeit. De `AccountRepository` uit Les 2 beveiligt wachtwoorden correct met hashing en salt. Maar een wachtwoord alleen is niet genoeg. In deze les voegen we een tweede verificatielaag toe aan de loginflow en leren we hoe ShopWave de integriteit van orderbevestigingen kan garanderen met digitale handtekeningen.

---

## Theorie

### 1. Two-Factor Authentication (2FA)

#### Waarom is een wachtwoord niet genoeg?

Een wachtwoord is iets wat je **weet**. Het probleem is dat wachtwoorden op veel manieren kunnen uitlekken:

- **Phishing** — een nep-loginpagina die het wachtwoord onderschept
- **Datalek** — een andere site waar je hetzelfde wachtwoord gebruikt, wordt gehackt
- **Brute force** — zwakke wachtwoorden worden geraden
- **Social engineering** — iemand wordt misleid om zijn wachtwoord te onthullen

Als een wachtwoord uitgelekt is, heeft een aanvaller volledige toegang. Er is niets dat hem tegenhoudt.

#### De oplossing: twee factoren combineren

**Two-Factor Authentication (2FA)** vereist twee onafhankelijke bewijzen van identiteit uit twee verschillende categorieën:

| Factor | Omschrijving | Voorbeeld |
|--------|-------------|-----------|
| Iets wat je **weet** | Kennis | Wachtwoord, pincode |
| Iets wat je **hebt** | Bezit | Smartphone, hardware token, smartcard |
| Iets wat je **bent** | Biometrie | Vingerafdruk, gezichtsherkenning |

2FA combineert minstens twee van deze factoren. In de praktijk is dat bijna altijd: wachtwoord + code op je smartphone.

Het resultaat: zelfs als een aanvaller je wachtwoord kent, kan hij nog steeds niet inloggen zonder ook je smartphone te hebben.

---

#### Hoe werkt TOTP?

De meest gebruikte 2FA-methode is **TOTP** — Time-based One-Time Password. Dit is de technologie achter apps zoals Google Authenticator en Microsoft Authenticator.

**Stap 1 — Setup (eenmalig):**
Bij de activatie van 2FA deelt de server een **geheime sleutel** (een lange willekeurige string) met de authenticator-app. Dit gebeurt via een QR-code. Zowel de server als de app kennen nu die sleutel.

**Stap 2 — Codegeneratie (elke 30 seconden):**
De app berekent een 6-cijferige code op basis van twee dingen:
- De geheime sleutel
- De huidige tijd (afgerond op 30 seconden)

Dit is een wiskundige berekening op basis van **HMAC-SHA1**. Omdat zowel de server als de app dezelfde sleutel en dezelfde tijd kennen, berekenen ze onafhankelijk van elkaar dezelfde code.

**Stap 3 — Verificatie:**
Bij het inloggen voert de gebruiker de code uit de app in. De server berekent ook de verwachte code. Als ze overeenkomen, is de verificatie geslaagd. Codes zijn typisch 30 seconden geldig.

```
Geheime sleutel + Huidige tijd  →  HMAC-SHA1  →  6-cijferige code
```

Waarom is dit veilig?
- De code verandert elke 30 seconden — een onderschepte code is snel onbruikbaar
- De geheime sleutel verlaat nooit de server of de app — alleen codes worden uitgewisseld
- Zonder de geheime sleutel kan niemand de codes voorspellen

---

### 2. Digitale handtekeningen

#### Wat is een digitale handtekening?

Een **digitale handtekening** is een cryptografisch mechanisme dat twee dingen garandeert:

- **Integriteit** — het bestand of bericht is onderweg niet gewijzigd
- **Authenticiteit** — het bericht is afkomstig van de verwachte afzender

Dit is anders dan encryptie. Encryptie maakt data onleesbaar. Een digitale handtekening maakt data controleerbaar.

#### Hoe werkt het — stap voor stap

Digitale handtekeningen maken gebruik van **asymmetrische cryptografie**: een sleutelpaar bestaande uit een private key en een public key.

**Bij de verzender:**

1. Bereken een hash van het bericht (bv. SHA-256)
2. Versleutel die hash met de **private key** — dit is de handtekening
3. Stuur het bericht + de handtekening mee

```
Bericht  →  SHA-256  →  Hash  →  RSA (private key)  →  Handtekening
```

**Bij de ontvanger:**

1. Bereken zelf een hash van het ontvangen bericht
2. Ontsleutel de meegestuurde handtekening met de **public key** van de verzender — dit geeft de originele hash
3. Vergelijk beide hashes

```
Ontvangen bericht  →  SHA-256  →  Hash A
Handtekening  →  RSA (public key)  →  Hash B

Hash A == Hash B  →  Bericht is authentiek en ongewijzigd
Hash A != Hash B  →  Bericht is gemanipuleerd of van verkeerde verzender
```

Waarom werkt dit?
- Alleen de houder van de **private key** kan een geldige handtekening maken
- Iedereen met de **public key** kan de handtekening verifiëren
- Elke wijziging in het bericht geeft een andere hash → verificatie mislukt

---

#### Verschil: encryptie versus digitale handtekening

| | Encryptie | Digitale handtekening |
|--|-----------|----------------------|
| Doel | Data verbergen | Data verifiëren |
| CIA-pijler | Confidentiality | Integrity + Authenticiteit |
| Sleutel om te beschermen | Public key van de ontvanger | Private key van de verzender |
| Sleutel om te ontsloten | Private key van de ontvanger | Public key van de verzender |

In de praktijk worden ze vaak gecombineerd: je versleutelt het bericht (confidentiality) én je ondertekent het (integrity + authenticiteit).

---

### 3. X.509-certificaten

#### Wat is een X.509-certificaat?

In de vorige secties werken we met sleutelparen: een private key en een public key. Maar hoe weet je dat een public key echt toebehoort aan de partij die beweert hem te bezitten? Iemand kan een sleutelpaar aanmaken en beweren dat de public key van ShopWave is.

Een **X.509-certificaat** lost dit probleem op. Het is een digitaal document dat een public key **koppelt aan een identiteit** en dat gekoppeld is ondertekend door een vertrouwde derde partij: een **Certificate Authority (CA)**.

Een X.509-certificaat bevat:
- De public key van de eigenaar
- De identiteit van de eigenaar (naam, organisatie, domein)
- De geldigheidsduur
- De digitale handtekening van de CA

Wanneer je een X.509-certificaat ontvangt, weet je:
- Wie de eigenaar is (identiteit, geverifieerd door de CA)
- Wat de public key is van die eigenaar
- Of het certificaat nog geldig is

#### Self-signed certificaten

In een productieomgeving worden certificaten uitgegeven door een vertrouwde CA (zoals DigiCert of Let's Encrypt). Voor ontwikkeling en testen maak je een **self-signed certificaat**: je ondertekent het certificaat zelf. Een browser of besturingssysteem vertrouwt zo'n certificaat niet automatisch, maar voor leer- en testdoeleinden is het perfect bruikbaar.

In .NET werk je met X.509-certificaten via de klasse `X509Certificate2`.

#### Waar komt X.509 voor?

- **HTTPS** — elke beveiligde website heeft een X.509-certificaat
- **Code signing** — uitvoerbare bestanden (EXE, DLL) worden ondertekend met een certificaat
- **E-mailbeveiliging (S/MIME)** — e-mails ondertekenen en versleutelen
- **Digitale handtekeningen op documenten** — PDF-documenten ondertekenen

---

## Demo

We breiden de ShopWave-applicatie uit met twee nieuwe features: een gesimuleerde 2FA-stap voor de loginflow en digitale handtekeningen voor orderbevestigingen.

Alle nieuwe klassen komen in de bestaande map `Security` van het project `ShopWave`.

---

### Deel 1 — Two-Factor Authentication simuleren

#### Stap 1 — TwoFactorService aanmaken

In een echte applicatie gebruikt TOTP een geheime sleutel en de huidige tijd. Voor deze demo simuleren we dat met een willekeurige 6-cijferige code die tijdelijk geldig is.

Maak `Security/TwoFactorService.cs` aan:

```csharp
using System.Security.Cryptography;

namespace ShopWave.Security
{
    public class TwoFactorService
    {
        private readonly Dictionary<string, PendingCode> pendingCodes;
        private readonly int validitySeconds;

        public TwoFactorService(int validitySeconds = 30)
        {
            pendingCodes = new Dictionary<string, PendingCode>();
            this.validitySeconds = validitySeconds;
        }

        public string GenerateCode(string email)
        {
            string code = RandomNumberGenerator.GetInt32(100000, 999999).ToString();
            DateTime expiresAt = DateTime.UtcNow.AddSeconds(validitySeconds);

            pendingCodes[email] = new PendingCode(code, expiresAt);

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

#### Stap 2 — AccountRepository uitbreiden met 2FA

Pas `Security/AccountRepository.cs` uit Les 2 aan. Voeg een `TwoFactorService`-afhankelijkheid toe via de constructor:

```csharp
namespace ShopWave.Security
{
    public class AccountRepository
    {
        private readonly Dictionary<string, CustomerAccount> _accounts;
        private readonly Dictionary<string, int>             _failedAttempts;
        private readonly TwoFactorService                    _twoFactorService;
        private const int MaxAttempts = 3;

        public AccountRepository(TwoFactorService twoFactorService)
        {
            _accounts         = new Dictionary<string, CustomerAccount>();
            _failedAttempts   = new Dictionary<string, int>();
            _twoFactorService = twoFactorService;
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
                    // Wachtwoord correct → genereer en stuur 2FA-code
                    string code = _twoFactorService.GenerateCode(email);

                    // In een echte applicatie: stuur de code via sms of e-mail
                    // Voor de demo tonen we hem in de console
                    Console.WriteLine($"[2FA] Code voor {email}: {code}");

                    _failedAttempts[email] = 0;
                    result = "2FA vereist.";
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

        public string VerifyTwoFactor(string email, string code)
        {
            string result;
            bool valid = _twoFactorService.VerifyCode(email, code);

            if (valid)
            {
                result = "Inloggen geslaagd.";
            }
            else
            {
                result = "Ongeldige of verlopen 2FA-code.";
            }

            return result;
        }
    }
}
```

Demonstreer de volledige loginflow in `Program.cs`:

```csharp
using ShopWave.Security;

TwoFactorService twoFactorService = new TwoFactorService(validitySeconds: 30);
AccountRepository repository = new AccountRepository(twoFactorService);

repository.Register("alice@shopwave.be", "wachtwoord123");

// Stap 1: wachtwoord
string loginResult = repository.Login("alice@shopwave.be", "wachtwoord123");
Console.WriteLine(loginResult); // "2FA vereist." + code in console

// Stap 2: 2FA-code invoeren
Console.Write("Voer de 2FA-code in: ");
string code = Console.ReadLine() ?? string.Empty;
string verifyResult = repository.VerifyTwoFactor("alice@shopwave.be", code);
Console.WriteLine(verifyResult);
```

---

### Deel 2 — Digitale handtekeningen met X.509

#### Stap 3 — Self-signed certificaat aanmaken

Maak `Security/CertificateHelper.cs` aan. Deze klasse genereert een self-signed X.509-certificaat voor gebruik in de demo:

```csharp
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace ShopWave.Security
{
    public static class CertificateHelper
    {
        public static X509Certificate2 CreateSelfSignedCertificate(string subjectName)
        {
            using (RSA rsa = RSA.Create(2048))
            {
                CertificateRequest request = new CertificateRequest(
                    $"CN={subjectName}",
                    rsa,
                    HashAlgorithmName.SHA256,
                    RSASignaturePadding.Pkcs1
                );

                X509Certificate2 certificate = request.CreateSelfSigned(
                    DateTimeOffset.UtcNow,
                    DateTimeOffset.UtcNow.AddYears(1)
                );

                return certificate;
            }
        }
    }
}
```

#### Stap 4 — OrderSigner aanmaken

Maak `Security/OrderSigner.cs` aan. Deze klasse ondertekent orderbevestigingen en verifieert handtekeningen:

```csharp
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace ShopWave.Security
{
    public class OrderSigner
    {
        private readonly X509Certificate2 _certificate;

        public OrderSigner(X509Certificate2 certificate)
        {
            _certificate = certificate;
        }

        public string Sign(string orderData)
        {
            RSA privateKey = _certificate.GetRSAPrivateKey();

            if (privateKey == null)
            {
                throw new InvalidOperationException("Certificaat bevat geen private key.");
            }

            byte[] dataBytes      = Encoding.UTF8.GetBytes(orderData);
            byte[] signatureBytes = privateKey.SignData(dataBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

            return Convert.ToBase64String(signatureBytes);
        }

        public bool Verify(string orderData, string signature)
        {
            RSA publicKey = _certificate.GetRSAPublicKey();

            if (publicKey == null)
            {
                throw new InvalidOperationException("Certificaat bevat geen public key.");
            }

            byte[] dataBytes      = Encoding.UTF8.GetBytes(orderData);
            byte[] signatureBytes = Convert.FromBase64String(signature);

            return publicKey.VerifyData(dataBytes, signatureBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        }
    }
}
```

Demonstreer in `Program.cs`:

```csharp
using ShopWave.Security;

// Certificaat aanmaken
System.Security.Cryptography.X509Certificates.X509Certificate2 certificate =
    CertificateHelper.CreateSelfSignedCertificate("ShopWave");

OrderSigner signer = new OrderSigner(certificate);

string orderData  = "ORD-2024-00042 | Alice | Laptop | 999.99 EUR";
string signature  = signer.Sign(orderData);

Console.WriteLine($"Orderdata:    {orderData}");
Console.WriteLine($"Handtekening: {signature}");

// Verificatie — originele data
bool validOriginal = signer.Verify(orderData, signature);
Console.WriteLine($"Geldig (origineel): {validOriginal}");

// Verificatie — gemanipuleerde data
string manipulatedData = "ORD-2024-00042 | Alice | Laptop | 1.00 EUR";
bool validManipulated  = signer.Verify(manipulatedData, signature);
Console.WriteLine($"Geldig (gemanipuleerd): {validManipulated}");
```

Output:
- Originele data → handtekening is geldig
- Gemanipuleerde prijs → handtekening is ongeldig: de integriteit is gebroken

---

## Oefeningen

Alle oefeningen bouwen verder op de bestaande `ShopWave` solution.

---

### Oefening 1 — TwoFactorService testen

Schrijf een testklasse `TwoFactorServiceTests.cs` in `ShopWave.Tests`.

Test minstens de volgende scenario's:

1. Een gegenereerde code voor een e-mailadres is niet leeg
2. De correcte code wordt geaccepteerd als geldig
3. Een foute code wordt geweigerd
4. Een code voor een onbekend e-mailadres wordt geweigerd
5. Een code kan maar één keer gebruikt worden — de tweede verificatie met dezelfde code faalt
6. Een verlopen code wordt geweigerd

Voor scenario 6: maak de `TwoFactorService` aan met `validitySeconds: 0` zodat de code onmiddellijk verloopt. Voeg een korte `Thread.Sleep(10)` toe zodat de vervaltijd zeker is bereikt.

---

### Oefening 2 — AccountRepository uitbreiden met 2FA testen

Schrijf tests voor de aangepaste `AccountRepository` in `AccountRepositoryTests.cs`.

Gebruik Moq om `TwoFactorService` te mocken — je wil niet afhankelijk zijn van willekeurige codes in je tests.

Maak hiervoor eerst een interface aan:

```csharp
public interface ITwoFactorService
{
    string GenerateCode(string email);
    bool VerifyCode(string email, string code);
}
```

Pas `TwoFactorService` aan zodat hij deze interface implementeert. Pas ook `AccountRepository` aan om `ITwoFactorService` te gebruiken in plaats van de concrete klasse.

Test de volgende scenario's:

1. Na een correcte loginpoging retourneert `Login` de tekst `"2FA vereist."`
2. `GenerateCode` wordt precies eenmaal aangeroepen na een geslaagde loginpoging
3. `VerifyTwoFactor` met een geldige code retourneert `"Inloggen geslaagd."`
4. `VerifyTwoFactor` met een ongeldige code retourneert de juiste foutmelding
5. Na een mislukte loginpoging (fout wachtwoord) wordt `GenerateCode` nooit aangeroepen

---

### Oefening 3 — OrderSigner testen

Schrijf een testklasse `OrderSignerTests.cs` in `ShopWave.Tests`.

Test minstens:

1. Een handtekening van originele orderdata is geldig
2. Een handtekening van gemanipuleerde orderdata is ongeldig
3. Een lege string als orderdata levert een handtekening op die ook als leeg geverifieerd kan worden
4. Een handtekening gemaakt met certificaat A is niet geldig als je hem verifieert met certificaat B

Voor test 4: maak twee aparte certificaten aan via `CertificateHelper.CreateSelfSignedCertificate`.

---

### Oefening 4 — Volledige orderflow: sign en verify

Maak een klasse `SecureOrderService.cs` in de map `Security`:

```csharp
public class SecureOrderService
{
    public OrderResult CreateSignedOrder(
        string customerEmail,
        string productName,
        double price) { ... }

    public bool ValidateOrder(string orderData, string signature) { ... }
}
```

`SecureOrderService` gebruikt `OrderSigner` intern. Maak het certificaat aan via `CertificateHelper` bij het aanmaken van de `SecureOrderService`.

`CreateSignedOrder`:
- Bouwt een orderstring op in het formaat: `"{customerEmail} | {productName} | {price} EUR"`
- Ondertekent die string
- Retourneert zowel de orderstring als de handtekening

`ValidateOrder`:
- Verifieert of de handtekening overeenkomt met de orderdata

Schrijf minstens vier tests voor `SecureOrderService` in `SecureOrderServiceTests.cs`.

---

### Oefening 5 — CIA-koppeling

Koppel de concepten uit deze les terug aan het CIA-model uit Les 2.

Beantwoord schriftelijk:

1. Welke CIA-pijler beschermt **2FA**? Leg uit waarom.
2. Welke CIA-pijler beschermt een **digitale handtekening**? Leg uit waarom.
3. Welke CIA-pijler beschermt **encryptie** (uit Les 2)? Leg uit waarom.
4. Een aanvaller slaagt erin de orderdata van een ShopWave-bestelling te wijzigen (prijs van 999 EUR naar 1 EUR) maar kan de handtekening niet aanpassen. Wat merkt het systeem? Welke CIA-pijler is hier aan het werk?

---

## Modeloplossing

> De modeloplossing is beschikbaar na het indienen van de labo-opdracht via Digitap.

---

### Modeloplossing Oefening 1 — TwoFactorServiceTests

```csharp
using FluentAssertions;
using ShopWave.Security;

namespace ShopWave.Tests
{
    public class TwoFactorServiceTests
    {
        [Fact]
        public void GenerateCode_ForValidEmail_ReturnsNonEmptyCode()
        {
            // Arrange
            TwoFactorService service = new TwoFactorService();

            // Act
            string code = service.GenerateCode("alice@shopwave.be");

            // Assert
            code.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void VerifyCode_WithCorrectCode_ReturnsTrue()
        {
            // Arrange
            TwoFactorService service = new TwoFactorService();
            string code = service.GenerateCode("alice@shopwave.be");

            // Act
            bool result = service.VerifyCode("alice@shopwave.be", code);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void VerifyCode_WithWrongCode_ReturnsFalse()
        {
            // Arrange
            TwoFactorService service = new TwoFactorService();
            service.GenerateCode("alice@shopwave.be");

            // Act
            bool result = service.VerifyCode("alice@shopwave.be", "000000");

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void VerifyCode_ForUnknownEmail_ReturnsFalse()
        {
            // Arrange
            TwoFactorService service = new TwoFactorService();

            // Act
            bool result = service.VerifyCode("onbekend@shopwave.be", "123456");

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void VerifyCode_UsedCodeVerifiedAgain_ReturnsFalse()
        {
            // Arrange
            TwoFactorService service = new TwoFactorService();
            string code = service.GenerateCode("alice@shopwave.be");
            service.VerifyCode("alice@shopwave.be", code); // eerste gebruik

            // Act
            bool result = service.VerifyCode("alice@shopwave.be", code); // tweede gebruik

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void VerifyCode_ExpiredCode_ReturnsFalse()
        {
            // Arrange
            TwoFactorService service = new TwoFactorService(validitySeconds: 0);
            string code = service.GenerateCode("alice@shopwave.be");
            System.Threading.Thread.Sleep(10);

            // Act
            bool result = service.VerifyCode("alice@shopwave.be", code);

            // Assert
            result.Should().BeFalse();
        }
    }
}
```

---

### Modeloplossing Oefening 3 — OrderSignerTests

```csharp
using FluentAssertions;
using ShopWave.Security;
using System.Security.Cryptography.X509Certificates;

namespace ShopWave.Tests
{
    public class OrderSignerTests
    {
        private readonly X509Certificate2 _certificate;
        private readonly OrderSigner      _signer;

        public OrderSignerTests()
        {
            _certificate = CertificateHelper.CreateSelfSignedCertificate("ShopWave");
            _signer      = new OrderSigner(_certificate);
        }

        [Fact]
        public void Verify_OriginalOrderData_ReturnsTrue()
        {
            // Arrange
            string orderData  = "ORD-001 | Alice | Laptop | 999.99 EUR";
            string signature  = _signer.Sign(orderData);

            // Act
            bool result = _signer.Verify(orderData, signature);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void Verify_ManipulatedOrderData_ReturnsFalse()
        {
            // Arrange
            string originalData    = "ORD-001 | Alice | Laptop | 999.99 EUR";
            string manipulatedData = "ORD-001 | Alice | Laptop | 1.00 EUR";
            string signature       = _signer.Sign(originalData);

            // Act
            bool result = _signer.Verify(manipulatedData, signature);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void Verify_SignatureFromDifferentCertificate_ReturnsFalse()
        {
            // Arrange
            X509Certificate2 otherCertificate = CertificateHelper.CreateSelfSignedCertificate("OtherParty");
            OrderSigner      otherSigner      = new OrderSigner(otherCertificate);

            string orderData  = "ORD-001 | Alice | Laptop | 999.99 EUR";
            string signature  = otherSigner.Sign(orderData); // ondertekend met ander certificaat

            // Act
            bool result = _signer.Verify(orderData, signature); // geverifieerd met ons certificaat

            // Assert
            result.Should().BeFalse();
        }
    }
}
```

---

## Samenvatting

| Concept | Wat je moet onthouden |
|---------|-----------------------|
| 2FA | Twee onafhankelijke bewijzen van identiteit — wachtwoord + code op je toestel |
| TOTP | Time-based One-Time Password — code op basis van geheime sleutel + huidige tijd |
| Digitale handtekening | Garandeert integriteit en authenticiteit — niet te verwarren met encryptie |
| Asymmetrische sleutels | Private key om te ondertekenen, public key om te verifiëren |
| X.509-certificaat | Koppelt een public key aan een identiteit, uitgegeven door een CA |
| Self-signed certificaat | Zelf ondertekend — bruikbaar voor ontwikkeling en testen |
| `X509Certificate2` | .NET-klasse voor werken met X.509-certificaten |
| `RSA.SignData` | Ondertekenen van data met een private key |
| `RSA.VerifyData` | Verifiëren van een handtekening met een public key |

---

## Volgende les

In Les 5 gaan we dieper in op **Integration Testing**. We testen de complete loginflow van ShopWave — inclusief de 2FA-stap die we vandaag hebben gebouwd — met echte klassen en zonder mocks, zodat we zeker zijn dat de onderdelen correct samenwerken.