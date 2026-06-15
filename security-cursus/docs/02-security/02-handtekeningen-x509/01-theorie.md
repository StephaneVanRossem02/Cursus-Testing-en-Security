---
title: "Les 4: Theorie - 2FA, Handtekeningen en X.509"
sidebar_label: "Theorie"
---

# Theorie: 2FA, Handtekeningen en X.509

## 1. Waarom een wachtwoord niet genoeg is

In les 2 leerde je wachtwoorden correct opslaan met BCrypt. Als de database van ShopWave gestolen wordt, ziet een aanvaller alleen onleesbare hashes. Dat is een grote verbetering.

Maar er is een groter probleem: een wachtwoord kan uitlekken zonder dat de database van ShopWave ooit gestolen wordt.

**Hoe lekken wachtwoorden?**

- **Phishing**: een aanvaller maakt een nep-loginpagina die identiek lijkt aan die van ShopWave. De klant tikt zijn wachtwoord in en stuurt het rechtstreeks naar de aanvaller.
- **Hergebruik**: de klant gebruikt hetzelfde wachtwoord op tien andere sites. Eén van die sites wordt gehackt. De aanvaller probeert het gelekte wachtwoord ook bij ShopWave.
- **Brute force**: zwakke wachtwoorden zoals "shopwave123" of "welkom01" worden geraden in minuten.
- **Social engineering**: iemand belt de klant op als "medewerker van ShopWave" en vraagt het wachtwoord telefonisch.

In al die gevallen helpt BCrypt niet. Het wachtwoord is correct: de aanvaller kent het gewoon. BCrypt beschermt alleen als de database gestolen wordt.

De oplossing is een tweede verificatielaag die een aanvaller niet kan stelen ook al kent hij het wachtwoord.

**Mini-controle:** je database is perfect beveiligd en je wachtwoorden zijn gehasht met BCrypt. Een klant wordt slachtoffer van phishing. Heeft BCrypt hier geholpen? Nee. De aanvaller heeft het plain-text wachtwoord direct van de klant gekregen, zonder ooit de database te raken.

---

## 2. Two-Factor Authentication: de drie factoren

**Two-Factor Authentication (2FA)** vereist twee onafhankelijke bewijzen van identiteit uit twee verschillende categorieën.

| Factor | Omschrijving | Voorbeeld |
|--------|-------------|-----------|
| Iets wat je **weet** | Kennis | Wachtwoord, pincode |
| Iets wat je **hebt** | Bezit | Smartphone, hardware token, smartcard |
| Iets wat je **bent** | Biometrie | Vingerafdruk, gezichtsherkenning |

2FA combineert minstens twee van die factoren. In de praktijk is dat bijna altijd: wachtwoord (iets wat je weet) + code op je smartphone (iets wat je hebt).

Het resultaat: zelfs als een aanvaller je wachtwoord kent via phishing of hergebruik, kan hij niet inloggen zonder ook je smartphone te hebben. Die twee dingen tegelijk stelen is aanzienlijk moeilijker.

2FA beschermt de **Confidentiality**-pijler uit het CIA-model. Het beschermt de toegang tot het account, ook als de eerste factor gecompromitteerd is.

**Mini-controle:** een aanvaller steelt via een datalek het wachtwoord van een ShopWave-klant. ShopWave heeft 2FA actief. Wat kan de aanvaller doen? Hij kan het wachtwoord invullen en de eerste stap doorlopen. Maar hij stuit op de 2FA-stap en heeft de smartphone van de klant nodig om verder te komen.

---

## 3. TOTP: hoe werkt het?

De meest gebruikte 2FA-methode is **TOTP**, Time-based One-Time Password. Dit is de technologie achter apps zoals Google Authenticator en Microsoft Authenticator.

**Stap 1: eenmalige setup**

Bij de activatie van 2FA deelt de server een **geheime sleutel** met de authenticator-app. Dit gebeurt via een QR-code die de gebruiker scant. Na de scan kennen zowel de server als de app die sleutel. De sleutel verlaat daarna nooit de server of de app.

**Stap 2: codegeneratie elke 30 seconden**

De app berekent een 6-cijferige code op basis van twee dingen: de geheime sleutel en de huidige tijd, afgerond op 30 seconden. Die berekening gebruikt **HMAC-SHA1**.

```csharp
Geheime sleutel + Huidige tijd  →  HMAC-SHA1  →  6-cijferige code
```

Omdat zowel de server als de app dezelfde sleutel en dezelfde tijd kennen, berekenen ze onafhankelijk van elkaar dezelfde code. Ze hoeven die code niet uit te wisselen.

**Stap 3: verificatie bij het inloggen**

De gebruiker voert de code in die zijn app toont. De server berekent zelf ook de verwachte code op basis van de sleutel en de huidige tijd. Als ze overeenkomen, slaagt de verificatie. Codes zijn typisch 30 seconden geldig.

**Waarom is dit veilig?**

- De code verandert elke 30 seconden. Een onderschepte code is snel onbruikbaar.
- De geheime sleutel verlaat nooit de server of de app. Alleen codes worden uitgewisseld via het loginformulier.
- Zonder de geheime sleutel kan niemand de codes voorspellen.

In ShopWave simuleren we TOTP met een willekeurige 6-cijferige code en een configureerbare vervaltijd. Dat is eenvoudiger dan een volledige TOTP-implementatie en volstaat om de concepten te begrijpen.

**Mini-controle:** een aanvaller onderschept de 2FA-code van een klant via een man-in-the-middle-aanval. De code is 30 seconden geldig. Wat moet de aanvaller doen om toch in te loggen? Hij moet de code gebruiken binnen die 30 seconden, én tegelijk het wachtwoord kennen. Dat maakt een geautomatiseerde aanval aanzienlijk moeilijker.

---

## 4. Digitale handtekeningen: wat en waarom

In les 2 leerde je dat **Integrity** een van de drie CIA-pijlers is: data moet correct, volledig en ongewijzigd zijn.

Stel dat ShopWave een orderbevestiging verstuurt: "Alice bestelt een Laptop voor 999,99 EUR." Wat als een aanvaller die bevestiging onderschept en de prijs wijzigt naar 1 EUR? Als er geen mechanisme is om de integriteit te controleren, merkt niemand het.

Een **digitale handtekening** lost dit op. Ze garandeert twee dingen:

- **Integriteit**: het bericht is onderweg niet gewijzigd. Elke wijziging, hoe klein ook, maakt de handtekening ongeldig.
- **Authenticiteit**: het bericht is afkomstig van de verwachte afzender. Niemand anders kan een geldige handtekening maken.

**Verschil met encryptie:**

| | Encryptie | Digitale handtekening |
|--|-----------|----------------------|
| Doel | Data verbergen | Data verifiëren |
| CIA-pijler | Confidentiality | Integrity |
| Resultaat | Onleesbare ciphertext | Controleerbare handtekening |

Encryptie verbergt de inhoud. Een digitale handtekening bewijst de echtheid. In de praktijk worden ze vaak gecombineerd: je versleutelt het bericht én je ondertekent het.

**Mini-controle:** ShopWave verstuurt een orderbevestiging zonder digitale handtekening maar wel versleuteld met AES. Een aanvaller onderschept het bericht, ontsleutelt het niet maar wijzigt een byte willekeurig in de ciphertext. Welk CIA-aspect faalt hier? Integrity: de wijziging is niet detecteerbaar zonder een handtekening. De aanvaller hoeft de inhoud niet eens te kennen om schade te berokkenen.

---

## 5. Digitale handtekeningen: hoe werkt het?

Digitale handtekeningen gebruiken **asymmetrische cryptografie**: een sleutelpaar bestaande uit een private sleutel en een publieke sleutel.

- De **private sleutel** is geheim. Alleen de eigenaar kent hem. Hij wordt gebruikt om te ondertekenen.
- De **publieke sleutel** is openbaar. Iedereen mag hem kennen. Hij wordt gebruikt om de handtekening te verifiëren.

**Bij de verzender (ShopWave):**

```csharp
Orderdata  →  SHA-256  →  Hash  →  RSA (private sleutel)  →  Handtekening
```

1. Bereken een hash van de orderdata met SHA-256
2. Versleutel die hash met de private sleutel. Dat resultaat is de handtekening.
3. Stuur de orderdata en de handtekening samen.

**Bij de ontvanger (klant of systeem):**

```csharp
Ontvangen orderdata  →  SHA-256  →  Hash A
Handtekening  →  RSA (publieke sleutel)  →  Hash B

Hash A == Hash B  →  data is authentiek en ongewijzigd
Hash A != Hash B  →  data is gemanipuleerd of van verkeerde verzender
```

1. Bereken zelf een hash van de ontvangen orderdata.
2. Ontsleutel de handtekening met de publieke sleutel van de verzender. Dat geeft de originele hash terug.
3. Vergelijk beide hashes.

**Waarom werkt dit?**

- Alleen de houder van de private sleutel kan een geldige handtekening maken. Niemand anders kan die private sleutel kennen.
- Iedereen met de publieke sleutel kan de handtekening verifiëren, maar kan er geen nieuwe geldige handtekening mee maken.
- Elke wijziging in de orderdata geeft een andere SHA-256-hash. De vergelijking mislukt.

In C# doe je dit met de klassen `RSA` en `X509Certificate2`:

```csharp
// Ondertekenen (private sleutel)
byte[] dataBytes      = Encoding.UTF8.GetBytes(orderData);
byte[] signatureBytes = rsa.SignData(
    dataBytes,
    HashAlgorithmName.SHA256,
    RSASignaturePadding.Pkcs1);

// Verifiëren (publieke sleutel)
bool valid = rsa.VerifyData(
    dataBytes,
    signatureBytes,
    HashAlgorithmName.SHA256,
    RSASignaturePadding.Pkcs1);
```

**Mini-controle:** een aanvaller onderschept een ondertekende orderbevestiging en wijzigt de prijs van 999 EUR naar 1 EUR. Kan hij ook de handtekening aanpassen zodat de verificatie toch slaagt? Nee. Om een geldige handtekening te maken heeft hij de private sleutel van ShopWave nodig. Die heeft hij niet.

---

## 6. X.509-certificaten: publieke sleutel koppelen aan identiteit

Digitale handtekeningen werken met een publieke sleutel. Maar hoe weet je dat een publieke sleutel echt toebehoort aan ShopWave? Iemand kan een eigen sleutelpaar aanmaken en beweren dat de publieke sleutel van ShopWave is. Als jij die nep-sleutel vertrouwt, verifieer je handtekeningen van de aanvaller in plaats van van ShopWave.

Een **X.509-certificaat** lost dit op. Het is een digitaal document dat een publieke sleutel koppelt aan een identiteit en dat ondertekend is door een **Certificate Authority (CA)**: een vertrouwde derde partij die de identiteit heeft geverifieerd.

Een X.509-certificaat bevat:

| Veld | Inhoud |
|------|--------|
| Publieke sleutel | De sleutel van de eigenaar |
| Identiteit | Naam, organisatie, domeinnaam |
| Geldigheidsperiode | Begindatum en vervaldatum |
| Handtekening van de CA | Bewijs dat de CA de identiteit geverifieerd heeft |

Wanneer je een X.509-certificaat ontvangt, weet je drie dingen: wie de eigenaar is, wat zijn publieke sleutel is, en of het certificaat nog geldig is.

**Self-signed certificaten**

In productie worden certificaten uitgegeven door een vertrouwde CA zoals Let's Encrypt of DigiCert. Voor ontwikkeling en tests maak je een **self-signed certificaat**: je ondertekent het certificaat zelf, zonder externe CA.

Een browser of besturingssysteem vertrouwt zo'n certificaat niet automatisch, maar voor lokale tests is het perfect bruikbaar. Je omzeilt de vraag "wie bewijst de identiteit?" omdat je zelf zowel de eigenaar als de CA bent.

**Waar komt X.509 voor?**

- **HTTPS**: elke beveiligde website heeft een X.509-certificaat. Je ziet het slotje in de adresbalk.
- **Code signing**: uitvoerbare bestanden (EXE, DLL) worden ondertekend zodat je weet dat ze van de echte ontwikkelaar komen.
- **E-mailbeveiliging (S/MIME)**: e-mails ondertekenen en versleutelen.
- **Digitale handtekeningen op documenten**: PDF-documenten ondertekenen.

In .NET werk je met X.509-certificaten via de klasse `X509Certificate2` uit `System.Security.Cryptography.X509Certificates`.

**Mini-controle:** je ontvangt een e-mail van "ShopWave" met een digitale handtekening. De e-mail is ondertekend met een X.509-certificaat. Hoe weet je of de handtekening echt van ShopWave is en niet van een aanvaller? Je controleert of het certificaat uitgegeven is door een vertrouwde CA en of de identiteit in het certificaat overeenkomt met het domein van ShopWave.

---

## 7. Demo: 2FA en orderhandtekeningen stap voor stap

We bouwen twee nieuwe features in ShopWave. Alle nieuwe klassen komen in `ShopWave/Security/`.

---

### Stap 1: PendingCode aanmaken

`TwoFactorService` moet bijhouden welke code hij gegenereerd heeft en wanneer die verloopt. Daarvoor maken we eerst een hulpklasse aan.

Maak `ShopWave/Security/PendingCode.cs` aan:

```csharp
namespace ShopWave.Security
{
    public class PendingCode
    {
        public string   Code      { get; }
        public DateTime ExpiresAt { get; }

        public PendingCode(string code, DateTime expiresAt)
        {
            Code      = code;
            ExpiresAt = expiresAt;
        }
    }
}
```

`PendingCode` is een eenvoudig dataobject. Het bewaart twee dingen: de gegenereerde code en het tijdstip waarop die verloopt. `TwoFactorService` slaat straks voor elke gebruiker één `PendingCode` op in een dictionary.

Bouw de solution.

Wat je ziet:

```csharp
Build succeeded.
```

---

### Stap 2a: TwoFactorService - klasse en velden aanmaken

Maak `ShopWave/Security/TwoFactorService.cs` aan met alleen de klasse, de velden en de constructor:

```csharp
using System.Security.Cryptography;

namespace ShopWave.Security
{
    public class TwoFactorService
    {
        private readonly Dictionary<string, PendingCode> _pendingCodes;
        private readonly int                             _validitySeconds;

        public TwoFactorService(int validitySeconds = 30)
        {
            _pendingCodes    = new Dictionary<string, PendingCode>();
            _validitySeconds = validitySeconds;
        }
    }
}
```

`_pendingCodes` is een dictionary die per e-mailadres bijhoudt welke code er gegenereerd is. `_validitySeconds` bepaalt hoe lang een code geldig is. De standaardwaarde is 30 seconden, zoals bij echte TOTP-codes.

Bouw de solution.

Wat je ziet:

```csharp
Build succeeded.
```

---

### Stap 2b: TwoFactorService - GenerateCode toevoegen

Voeg de methode `GenerateCode` toe aan `TwoFactorService`:

```csharp
public string GenerateCode(string email)
{
    string   code      = RandomNumberGenerator.GetInt32(100000, 999999).ToString();
    DateTime expiresAt = DateTime.UtcNow.AddSeconds(_validitySeconds);

    _pendingCodes[email] = new PendingCode(code, expiresAt);

    return code;
}
```

Regel voor regel:

- `RandomNumberGenerator.GetInt32(100000, 999999)` genereert een willekeurig getal tussen 100000 en 999999. Dat zijn altijd 6 cijfers. Let op: gebruik nooit `Random` voor beveiligingsgevoelige codes. `Random` is voorspelbaar; `RandomNumberGenerator` is cryptografisch veilig.
- `DateTime.UtcNow.AddSeconds(_validitySeconds)` berekent het tijdstip waarop de code verloopt.
- `_pendingCodes[email] = new PendingCode(code, expiresAt)` slaat de code op in de dictionary. Als er al een code stond voor dit e-mailadres, wordt die overschreven.
- De methode geeft de code terug zodat de aanroeper hem kan doorsturen (via e-mail, sms of callback).

Bouw de solution.

Wat je ziet:

```csharp
Build succeeded.
```

---

### Stap 2c: TwoFactorService - VerifyCode toevoegen

Voeg de methode `VerifyCode` toe aan `TwoFactorService`:

```csharp
public bool VerifyCode(string email, string code)
{
    bool isValid = false;

    if (_pendingCodes.ContainsKey(email))
    {
        PendingCode pending = _pendingCodes[email];

        if (DateTime.UtcNow <= pending.ExpiresAt && pending.Code == code)
        {
            isValid = true;
        }

        _pendingCodes.Remove(email);
    }

    return isValid;
}
```

Regel voor regel:

- `_pendingCodes.ContainsKey(email)` controleert of er überhaupt een code bestaat voor dit e-mailadres. Als de gebruiker nooit ingelogd heeft, staat er niets in de dictionary.
- `DateTime.UtcNow <= pending.ExpiresAt` controleert of de code nog niet verlopen is.
- `pending.Code == code` vergelijkt de ingevoerde code met de opgeslagen code.
- `_pendingCodes.Remove(email)` staat **buiten** de `if`. Dat is bewust: of de code nu geldig is of niet, hij wordt altijd verwijderd na één verificatiepoging. Zo is elke code eenmalig bruikbaar. Een aanvaller kan niet onbeperkt codes blijven proberen.

Bouw de solution.

Wat je ziet:

```csharp
Build succeeded.
```

---

### Stap 3a: AccountRepository - constructor en Register aanpassen

In les 2 schreef je `AccountRepository` zonder 2FA. Nu passen we hem aan.

Het probleem: als `AccountRepository` zelf `Console.WriteLine` gebruikt om de 2FA-code te tonen, kan je die output niet opvangen in een test. Je weet dan niet welke code er gegenereerd werd.

De oplossing is een **callback**: een methode die je meegeeft via de constructor. Wanneer een 2FA-code aangemaakt wordt, roept `AccountRepository` die callback aan. In een demo geef je een lambda mee die de code toont in de console. In een test geef je een lambda mee die de code opslaat in een variabele.

Pas de constructor van `AccountRepository` aan:

```csharp
namespace ShopWave.Security
{
    public class AccountRepository
    {
        private readonly Dictionary<string, CustomerAccount> _accounts;
        private readonly Dictionary<string, int>             _failedAttempts;
        private readonly TwoFactorService                    _twoFactorService;
        private readonly Action<string, string>              _onCodeGenerated;
        private const int MaxAttempts = 3;

        public AccountRepository(
            TwoFactorService       twoFactorService,
            Action<string, string> onCodeGenerated = null)
        {
            _accounts         = new Dictionary<string, CustomerAccount>();
            _failedAttempts   = new Dictionary<string, int>();
            _twoFactorService = twoFactorService;
            _onCodeGenerated  = onCodeGenerated;
        }

        public void Register(string email, string password)
        {
            CustomerAccount account = new CustomerAccount(email, password);
            _accounts[email]        = account;
            _failedAttempts[email]  = 0;
        }
    }
}
```

`Action<string, string>` is een delegate die twee strings accepteert en niets teruggeeft. De eerste string is het e-mailadres, de tweede is de gegenereerde code. De parameter `onCodeGenerated = null` betekent dat de callback optioneel is.

Bouw de solution.

Wat je ziet:

```csharp
Build succeeded.
```

---

### Stap 3b: AccountRepository - Login methode toevoegen

Voeg de methode `Login` toe aan `AccountRepository`:

```csharp
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
            string code = _twoFactorService.GenerateCode(email);

            if (_onCodeGenerated != null)
            {
                _onCodeGenerated(email, code);
            }

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
```

De volgorde van controles is bewust:

1. Bestaat het e-mailadres? Zo niet: stop.
2. Is het account geblokkeerd? Zo ja: stop. Deze controle staat vóór `VerifyPassword`, zodat een geblokkeerd account geen BCrypt-berekening triggert.
3. Is het wachtwoord correct? Als ja: genereer een 2FA-code en roep de callback aan. Als nee: tel de mislukte poging en controleer of het maximum bereikt is.

`_twoFactorService.GenerateCode(email)` genereert de code. `_onCodeGenerated(email, code)` stuurt die code door via de callback (als die meegegeven is).

Bouw de solution.

Wat je ziet:

```csharp
Build succeeded.
```

---

### Stap 3c: AccountRepository - VerifyTwoFactor toevoegen

Voeg de methode `VerifyTwoFactor` toe aan `AccountRepository`:

```csharp
public string VerifyTwoFactor(string email, string code)
{
    string result;
    bool   valid = _twoFactorService.VerifyCode(email, code);

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
```

`VerifyCode` doet het echte werk: hij controleert of de code geldig is en verwijdert hem daarna. `VerifyTwoFactor` vertaalt het resultaat naar een leesbare string.

Bouw de solution.

Wat je ziet:

```csharp
Build succeeded.
```

---

### Stap 4: de volledige loginflow uitproberen

Voeg tijdelijk toe aan `Program.cs`:

```csharp
using ShopWave.Security;

TwoFactorService  twoFactorService = new TwoFactorService(validitySeconds: 30);
AccountRepository repository       = new AccountRepository(
    twoFactorService,
    onCodeGenerated: (email, code) => Console.WriteLine($"[2FA] Code voor {email}: {code}"));

repository.Register("alice@shopwave.be", "wachtwoord123");

string loginResult = repository.Login("alice@shopwave.be", "wachtwoord123");
Console.WriteLine(loginResult);

Console.Write("Voer de 2FA-code in: ");
string input        = Console.ReadLine() ?? string.Empty;
string verifyResult = repository.VerifyTwoFactor("alice@shopwave.be", input);
Console.WriteLine(verifyResult);
```

Voer uit en voer de getoonde code in.

Wat je ziet:

```csharp
[2FA] Code voor alice@shopwave.be: 482917
2FA vereist.
Voer de 2FA-code in: 482917
Inloggen geslaagd.
```

De lambda `(email, code) => Console.WriteLine(...)` is de callback. In productie zou je hier een e-mail of sms sturen. In tests gebruik je de callback om de code op te vangen in een variabele, zonder `Console.WriteLine` en zonder mocks.

---

### Stap 5a: CertificateHelper - klasse aanmaken en RSA-sleutelpaar genereren

Voor digitale handtekeningen hebben we een X.509-certificaat nodig. Maak `ShopWave/Security/CertificateHelper.cs` aan met de klasse en de eerste twee regels van de methode:

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
                // wordt verder uitgewerkt in stap 5b
                throw new NotImplementedException();
            }
        }
    }
}
```

`RSA.Create(2048)` genereert een nieuw RSA-sleutelpaar van 2048 bits. 2048 bits is de minimale aanbevolen lengte voor RSA. Het sleutelpaar bestaat uit een private sleutel (om te ondertekenen) en een publieke sleutel (om te verifiëren).

`using` zorgt dat het RSA-object vrijgegeven wordt na gebruik. RSA-sleutels bevatten gevoelige cryptografische data en moeten zo snel mogelijk vrijgegeven worden.

Bouw de solution.

Wat je ziet:

```csharp
Build succeeded.
```

---

### Stap 5b: CertificateHelper - certificaat aanmaken en teruggeven

Vervang de `throw new NotImplementedException()` door de rest van de methode:

```csharp
using (RSA rsa = RSA.Create(2048))
{
    CertificateRequest request = new CertificateRequest(
        $"CN={subjectName}",
        rsa,
        HashAlgorithmName.SHA256,
        RSASignaturePadding.Pkcs1);

    X509Certificate2 certificate = request.CreateSelfSigned(
        DateTimeOffset.UtcNow,
        DateTimeOffset.UtcNow.AddYears(1));

    return certificate;
}
```

Regel voor regel:

- `CertificateRequest` is een aanvraag voor een certificaat. Je geeft drie dingen mee: de identiteit (`CN=ShopWave`), het RSA-sleutelpaar, en het hash-algoritme voor de handtekening van het certificaat zelf.
- `CN={subjectName}` is de **Common Name**: de naam van de eigenaar in het certificaat. Voor ShopWave is dat `"ShopWave"`.
- `CreateSelfSigned` ondertekent het certificaat met de private sleutel die we zelf aangemaakt hebben. Er is geen externe Certificate Authority betrokken. Het certificaat is 1 jaar geldig.

Bouw de solution.

Wat je ziet:

```csharp
Build succeeded.
```

---

### Stap 6a: OrderSigner - klasse en Sign methode aanmaken

Maak `ShopWave/Security/OrderSigner.cs` aan met de klasse en alleen de `Sign`-methode:

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
            RSA privateKey = _certificate.GetRSAPrivateKey()!;

            byte[] dataBytes      = Encoding.UTF8.GetBytes(orderData);
            byte[] signatureBytes = privateKey.SignData(
                dataBytes,
                HashAlgorithmName.SHA256,
                RSASignaturePadding.Pkcs1);

            return Convert.ToBase64String(signatureBytes);
        }
    }
}
```

Regel voor regel:

- `_certificate.GetRSAPrivateKey()` haalt de private sleutel op uit het certificaat. Die is nodig om te ondertekenen. Het uitroepteken (`!`) zegt aan de compiler dat we zeker weten dat de sleutel er is.
- `Encoding.UTF8.GetBytes(orderData)` zet de tekst om naar een byte-array. `SignData` werkt met bytes, niet met strings.
- `privateKey.SignData(...)` berekent intern een SHA-256-hash van `dataBytes` en versleutelt die hash met RSA. Het resultaat is de handtekening als byte-array.
- `Convert.ToBase64String(signatureBytes)` zet de byte-array om naar een leesbare Base64-string, zodat je hem eenvoudig kan opslaan of meesturen.

Bouw de solution.

Wat je ziet:

```csharp
Build succeeded.
```

---

### Stap 6b: OrderSigner - Verify methode toevoegen

Voeg de methode `Verify` toe aan `OrderSigner`:

```csharp
public bool Verify(string orderData, string signature)
{
    RSA publicKey = _certificate.GetRSAPublicKey()!;

    byte[] dataBytes      = Encoding.UTF8.GetBytes(orderData);
    byte[] signatureBytes = Convert.FromBase64String(signature);

    return publicKey.VerifyData(
        dataBytes,
        signatureBytes,
        HashAlgorithmName.SHA256,
        RSASignaturePadding.Pkcs1);
}
```

Regel voor regel:

- `_certificate.GetRSAPublicKey()` haalt de **publieke** sleutel op. Die is nodig om te verifiëren. Verifiëren vereist nooit de private sleutel.
- `Convert.FromBase64String(signature)` zet de Base64-string terug naar een byte-array. Dat is het omgekeerde van wat `Sign` deed.
- `publicKey.VerifyData(...)` berekent opnieuw de SHA-256-hash van `dataBytes` en vergelijkt die met de hash die uit de handtekening ontsleuteld wordt. Als beide hashes overeenkomen, geeft `VerifyData` `true` terug.

Het verschil met `Sign`: `Sign` gebruikt de private sleutel en maakt een handtekening. `Verify` gebruikt de publieke sleutel en controleert een handtekening.

Bouw de solution.

Wat je ziet:

```csharp
Build succeeded.
```

---

### Stap 7: handtekeningen uitproberen

Voeg toe aan `Program.cs`:

```csharp
using ShopWave.Security;
using System.Security.Cryptography.X509Certificates;

X509Certificate2 certificate = CertificateHelper.CreateSelfSignedCertificate("ShopWave");
OrderSigner      signer      = new OrderSigner(certificate);

string orderData = "ORD-2024-00042 | alice@shopwave.be | Laptop | 999.99 EUR";
string signature = signer.Sign(orderData);

Console.WriteLine($"Orderdata:    {orderData}");
Console.WriteLine($"Handtekening: {signature[..40]}...");

bool geldigOrigineel     = signer.Verify(orderData, signature);
bool geldigGemanipuleerd = signer.Verify(
    "ORD-2024-00042 | alice@shopwave.be | Laptop | 1.00 EUR",
    signature);

Console.WriteLine($"Geldig (origineel):     {geldigOrigineel}");
Console.WriteLine($"Geldig (gemanipuleerd): {geldigGemanipuleerd}");
```

Voer uit.

Wat je ziet:

```csharp
Orderdata:    ORD-2024-00042 | alice@shopwave.be | Laptop | 999.99 EUR
Handtekening: Xy7mNpQ2rBk3aLvM8sRt1wJc9dFe6hGi...
Geldig (origineel):     True
Geldig (gemanipuleerd): False
```

De prijs is gewijzigd van 999.99 naar 1.00. De SHA-256-hash van de gemanipuleerde data verschilt van de hash in de handtekening. `VerifyData` detecteert dat en geeft `False` terug.

---

### Stap 8: wat je hebt gebouwd

Overzicht van alle klassen die na deze demo in `ShopWave/Security/` staan:

| Klasse | Functie |
|--------|---------|
| `PasswordHasher` | BCrypt-hashing (les 2) |
| `CustomerAccount` | Account met gehashte wachtwoordopslag (les 2) |
| `AesEncryptor` | AES-encryptie en -ontsleuteling (les 2) |
| `AccountRepository` | Login met lockout en 2FA-callback |
| `PendingCode` | Hulpklasse voor 2FA-codes met vervaltijd |
| `TwoFactorService` | Genereren en verifiëren van 2FA-codes |
| `CertificateHelper` | Self-signed X.509-certificaat aanmaken |
| `OrderSigner` | Ondertekenen en verifiëren van orderdata |

Elke klasse heeft één verantwoordelijkheid. De `AccountRepository` weet niet hoe codes verstuurd worden: dat is de taak van de callback. De `OrderSigner` weet niet hoe certificaten aangemaakt worden: dat is de taak van `CertificateHelper`.

---

## 8. CIA-koppeling

| Technologie | CIA-pijler | Waarom |
|-------------|-----------|--------|
| Wachtwoord + BCrypt (les 2) | Confidentiality | Beschermt de inhoud van de database |
| AES-encryptie (les 2) | Confidentiality | Verbergt gevoelige data voor onbevoegden |
| 2FA | Confidentiality | Beschermt toegang tot het account, ook bij gestolen wachtwoord |
| Digitale handtekening | Integrity | Elke wijziging in de data maakt de handtekening ongeldig |
| X.509-certificaat | Integrity + Confidentiality | Koppelt identiteit aan publieke sleutel; basis voor HTTPS |

**Opmerking over 2FA en Confidentiality:** 2FA beschermt primair de toegang, en toegangsbescherming valt onder Confidentiality. Het is geen Integrity-maatregel: 2FA verandert niets aan de data zelf.

---

## 9. Samenvatting

| Concept | Wat je moet onthouden |
|--------|-----------------------|
| 2FA | Twee onafhankelijke bewijzen van identiteit uit twee verschillende categorieën |
| Drie factoren | Iets wat je weet, iets wat je hebt, iets wat je bent |
| TOTP | Code op basis van geheime sleutel en huidige tijd; 30 seconden geldig; eenmalig bruikbaar |
| Digitale handtekening | Garandeert integriteit en authenticiteit; niet te verwarren met encryptie |
| Private sleutel | Gebruikt om te ondertekenen; nooit delen |
| Publieke sleutel | Gebruikt om te verifiëren; mag iedereen kennen |
| X.509-certificaat | Koppelt publieke sleutel aan identiteit; uitgegeven door een CA |
| Self-signed certificaat | Zelf ondertekend; bruikbaar voor ontwikkeling en tests |
| `RSA.SignData` | Ondertekenen van data met een private sleutel |
| `RSA.VerifyData` | Verifiëren van een handtekening met een publieke sleutel |
| Callback-techniek | Methode meegeven via constructor zodat de code testbaar blijft zonder console-output |
