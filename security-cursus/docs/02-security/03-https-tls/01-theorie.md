---
title: "Les 6: Theorie - HTTPS en TLS"
sidebar_label: "Theorie"
---

# Theorie: HTTPS en TLS

## 1. Wat een aanvaller ziet op HTTP

In les 2 leerde je dat een gestolen database minder gevaarlijk is als wachtwoorden gehasht zijn. Maar er is een aanval die helemaal niets met de database te maken heeft: het afluisteren van het netwerk.

Stel dat een klant van ShopWave inlogt op een open wifi-netwerk in een café. Een aanvaller op hetzelfde netwerk kan met een tool zoals Wireshark alle netwerkpakketjes onderscheppen. Als ShopWave op HTTP draait, ziet de aanvaller dit:

```
POST /login HTTP/1.1
Host: shopwave.be
Content-Type: application/json

{"email":"alice@shopwave.be","password":"wachtwoord123"}
```

Het wachtwoord staat er letterlijk in. BCrypt helpt hier niet: de aanvaller heeft het plain-text wachtwoord direct, zonder ooit de database te raken.

Hetzelfde geldt voor alles wat ShopWave verstuurt: orderdata, betaalgegevens, sessietokens. Alles is leesbaar. En niet alleen leesbaar: een aanvaller kan ook de inhoud van een antwoord aanpassen. Hij ontvangt het antwoord van de server, wijzigt de prijs van 999 EUR naar 1 EUR, en stuurt het aangepaste antwoord door naar de client. De client merkt niets.

Dit type aanval heet een **man-in-the-middle-aanval** (MITM): de aanvaller zit tussen client en server in en leest of wijzigt het verkeer.

**Mini-controle:** ShopWave verstuurt een orderbevestiging via HTTP. Een aanvaller op het netwerk onderschept die bevestiging en wijzigt het bedrag. Welke twee CIA-pijlers zijn hier in het geding? Confidentiality (de aanvaller kan de inhoud lezen) en Integrity (de aanvaller kan de inhoud wijzigen zonder dat iemand het merkt).

---

## 2. Wat HTTPS garandeert

**HTTPS** is HTTP met een extra beveiligingslaag: het **TLS-protocol** (Transport Layer Security). TLS staat tussen het HTTP-protocol en het TCP-netwerk in. De applicatie spreekt HTTP; TLS versleutelt alles voor het de draad opgaat.

TLS geeft drie garanties:

| Garantie | CIA-pijler | Wat het betekent |
|----------|-----------|-----------------|
| Vertrouwelijkheid | Confidentiality | Alle data is versleuteld. Een aanvaller die pakketjes onderschept, ziet alleen onleesbare bytes. |
| Integriteit | Integrity | Elke wijziging in een pakket wordt gedetecteerd. TLS gebruikt een berichtauthenticatiecode (MAC) die elke manipulatie zichtbaar maakt. |
| Authenticatie | Buiten CIA | De client weet zeker dat hij met de echte server praat, niet met een aanvaller die zich voordoet als de server. |

Authenticatie valt buiten het CIA-model maar is minstens even belangrijk. Zonder authenticatie kan een aanvaller een nep-ShopWave-server opzetten, een eigen TLS-certificaat aanmaken, en de client naar die server leiden. De verbinding is dan wél versleuteld, maar met de verkeerde server.

**Mini-controle:** een aanvaller zet een nep-ShopWave-server op en onderschept het verkeer. HTTPS met een geldig certificaat van ShopWave beschermt hier wel tegen. Waarom? De client valideert het certificaat van de server. Als het certificaat niet klopt (verkeerde domeinnaam, niet uitgegeven door een vertrouwde CA), weigert de browser de verbinding.

---

## 3. De TLS-handshake stap voor stap

Voordat er één byte applicatiedata verstuurd wordt, voeren client en server de **TLS-handshake** uit. Die handshake regelt drie dingen: de server bewijst zijn identiteit, ze spreken af welke algoritmen ze gebruiken, en ze wisselen een gedeelde sessiesleutel uit.

```
Client                                         Server
  |                                               |
  |--- 1. ClientHello --------------------------> |
  |    TLS-versie + willekeurig getal (nonce)    |
  |                                               |
  |<-- 2. ServerHello + Certificaat ------------ |
  |    Gekozen algoritmen + X.509-certificaat    |
  |    (bevat de publieke sleutel van de server) |
  |                                               |
  |    3. Certificaatvalidatie (client)          |
  |    - CA vertrouwd?                           |
  |    - Certificaat niet verlopen?              |
  |    - Domeinnaam klopt?                       |
  |                                               |
  |--- 4. Sessiesleutel ------------------------> |
  |    Versleuteld met de publieke sleutel       |
  |    van de server (RSA)                       |
  |                                               |
  |    5. Beide berekenen de gedeelde sleutel    |
  |                                               |
  |=== 6. Beveiligde verbinding (AES) ========= |
  |    Alle verdere data via symmetrische        |
  |    encryptie met de gedeelde sessiesleutel   |
```

**Stap 1 (ClientHello):** de client zegt hoe welke TLS-versies en algoritmen hij ondersteunt. Hij stuurt ook een willekeurig getal mee dat later gebruikt wordt om de sessiesleutel te berekenen.

**Stap 2 (ServerHello + certificaat):** de server kiest de sterkste gemeenschappelijke algoritmen en stuurt zijn X.509-certificaat. Dat certificaat bevat de publieke sleutel van de server, zijn identiteit en de handtekening van de Certificate Authority die hem heeft uitgegeven.

**Stap 3 (validatie door de client):** de client controleert het certificaat. Is de CA opgenomen in de trust store van het besturingssysteem? Is het certificaat nog niet verlopen? Klopt de domeinnaam overeen met de server waarmee hij wil praten? Als één controle mislukt, weigert de client de verbinding.

**Stap 4 (sleuteluitwisseling):** de client genereert een sessiesleutel en versleutelt die met de publieke sleutel van de server. Alleen de server kan die ontsleutelen, want alleen de server heeft de bijbehorende private sleutel.

**Stap 5 (gedeelde sleutel):** beide partijen berekenen nu dezelfde sessiesleutel. Ze sturen elkaar een bevestiging dat ze klaar zijn.

**Stap 6 (beveiligde verbinding):** alle verdere communicatie verloopt via AES met de sessiesleutel. De RSA-fase is voorbij.

**Mini-controle:** een aanvaller onderschept stap 4 en leest de versleutelde sessiesleutel. Kan hij de sessiesleutel ontsleutelen? Nee. De sessiesleutel is versleuteld met de publieke sleutel van de server. Alleen de private sleutel van de server kan hem ontsleutelen. Die private sleutel verlaat de server nooit.

---

## 4. Waarom twee soorten cryptografie

TLS gebruikt bewust twee soorten cryptografie: RSA en AES. Ze zijn allebei nodig, maar om verschillende redenen.

**RSA (asymmetrische cryptografie)** gebruikt twee sleutels: een publieke sleutel die iedereen mag kennen, en een private sleutel die alleen de server heeft. Wat met de publieke sleutel versleuteld wordt, kan alleen de private sleutel ontsleutelen. Dat is ideaal voor de sleuteluitwisseling: de client kan een sessiesleutel versleuteld opsturen zonder dat de twee partijen vooraf ooit iets uitgewisseld hebben.

RSA heeft één groot nadeel: het is traag. Een RSA-encryptie van een klein blokje data duurt tientallen tot honderden keer langer dan AES. Voor de handshake is dat acceptabel. Voor alle data daarna niet.

**AES (symmetrische cryptografie)** gebruikt één sleutel voor zowel versleutelen als ontsleutelen. Het is razendsnel, ook voor grote hoeveelheden data. Het nadeel: beide partijen moeten dezelfde sleutel al kennen voor ze kunnen beginnen communiceren. In een netwerk waarbij twee onbekende partijen voor het eerst contact maken, is dat een probleem. Hoe stuur je de sleutel over zonder dat iemand hem kan onderscheppen?

TLS lost dat op door beide te combineren:

| Fase | Cryptografie | Waarom |
|------|-------------|--------|
| Handshake | RSA | Veilig de sessiesleutel uitwisselen, ook met een onbekende server |
| Communicatie | AES | Snelle versleuteling van alle data daarna |

De sessiesleutel wordt veilig via RSA uitgewisseld. Daarna verloopt alles via AES. RSA deed zijn werk in de handshake en is dan klaar.

In les 2 bouwde je `AesEncryptor`. In les 4 gebruikte je `RSA.SignData` en `RSA.VerifyData`. TLS combineert precies die twee technieken.

**Mini-controle:** waarom versleutelt TLS niet alle communicatie met RSA in plaats van over te schakelen naar AES? RSA is te traag voor grote hoeveelheden data. Een webpagina van 1 MB versleutelen met RSA duurt seconden. Met AES duurt dat microseconden.

---

## 5. De trust chain en self-signed certificaten

In les 4 maakten we een self-signed certificaat met `CertificateHelper.CreateSelfSignedCertificate`. We ondertekenden het certificaat zelf. Er was geen externe partij die onze identiteit controleerde.

In productie werkt dat anders. Een **Certificate Authority (CA)** is een organisatie die identiteiten verifieert en certificaten uitgeeft. De bekendste zijn Let's Encrypt (gratis), DigiCert en Sectigo. Een CA ondertekent het certificaat van de server. De browser vertrouwt dat certificaat omdat hij de CA al vertrouwt.

Die vertrouwensketen heet de **trust chain**:

```
Root CA
  (ingebouwd in Windows, macOS, Android, iOS)
    |
    v
Intermediate CA
  (ondertekend door Root CA)
    |
    v
Servercertificaat shopwave.be
  (ondertekend door Intermediate CA)
```

De browser valideert de volledige keten van het servercertificaat omhoog tot de Root CA. Als een stap ontbreekt of niet klopt, weigert de browser de verbinding.

**Self-signed certificaten** hebben geen CA. De server ondertekent zijn eigen certificaat. De trust chain bestaat uit slechts één schakel: het certificaat zelf. De browser heeft geen reden om dat te vertrouwen en toont een waarschuwing.

Voor development is een self-signed certificaat perfect bruikbaar. De verbinding is wél versleuteld. De waarschuwing betekent alleen dat de identiteit van de server niet onafhankelijk geverifieerd is. Als je zelf de server bent, weet je al wie er aan de andere kant zit.

Let's Encrypt heeft het aanvragen van een gratis certificaat volledig geautomatiseerd. Voor elke publieke website is er geen reden meer om een self-signed certificaat te gebruiken.

**Mini-controle:** een aanvaller maakt een nep-ShopWave-server met een self-signed certificaat. Hij probeert klanten daarnaar te leiden. Waarom beschermt de browser de klant hier wel tegen? De browser ziet dat het certificaat self-signed is en niet ondertekend door een CA die hij vertrouwt. Hij toont een duidelijke waarschuwing. Klanten die die waarschuwing negeren, verbinden wel, maar de meeste klanten klikken weg.

---

## 6. Forward secrecy en TLS 1.3

In TLS 1.2 versleutelt de client de sessiesleutel met de publieke sleutel van de server. Die sessiesleutel is dan gekoppeld aan de private sleutel van de server. Als de private sleutel ooit uitlekt, kan een aanvaller die eerder opgenomen sessies had bewaard, die alsnog ontsleutelen. Hij ontsleutelt de sessiesleutel uit de handshake en ontsleutelt dan alle opgenomen communicatie.

**Forward secrecy** (ook wel perfect forward secrecy of PFS) lost dat op. In plaats van de sessiesleutel te versleutelen met de RSA-sleutel van de server, berekenen beide partijen de sessiesleutel via het **Diffie-Hellman-protocol**. Bij elke nieuwe sessie worden tijdelijke sleutelparen gegenereerd. Die worden weggegooid zodra de sessie afgelopen is. De sessiesleutel is nergens opgeslagen en is niet afleidbaar uit de private sleutel van de server.

Gevolg: als de private sleutel van de server morgen uitlakt, kan een aanvaller vroegere sessies niet ontsleutelen. Elke sessie was beschermd door een unieke, weggegooid tijdelijke sleutel.

| Versie | Sleuteluitwisseling | Forward secrecy | Aanbevolen? |
|--------|-------------------|----------------|-------------|
| TLS 1.2 | RSA of Diffie-Hellman | Optioneel | Acceptabel, maar verouderd |
| TLS 1.3 | Alleen Diffie-Hellman | Altijd | Ja, gebruik dit |

TLS 1.3 is ook sneller: de handshake heeft minder stappen nodig. ASP.NET Core op .NET 8 ondersteunt TLS 1.3 standaard.

**Mini-controle:** een aanvaller heeft het networkverkeer van ShopWave opgenomen gedurende zes maanden. Dan slaagt hij erin de private sleutel van de server te stelen. Met TLS 1.2 zonder forward secrecy kan hij alle zes maanden aan opnames ontsleutelen. Met TLS 1.3 kan hij niets ontsleutelen. Waarom? Elke sessie had een unieke tijdelijke Diffie-Hellman-sleutel die na de sessie vernietigd is. De private sleutel van de server was nooit betrokken bij de sessiesleutelberekening.

---

## 7. Demo: ShopWave API op HTTPS

We breiden de ShopWave-solution uit met een ASP.NET Core Minimal API die op HTTPS draait. Daarna simuleren we de kern van de TLS-handshake in code.

### Projectstructuur

Voeg een nieuw project toe aan de bestaande solution:

```
ShopWave          (Console App, bestaand)
ShopWave.Tests    (xUnit, bestaand)
ShopWave.Api      (ASP.NET Core Web API, nieuw)
```

Rechtsklik op de solution in Solution Explorer en kies "Add > New Project". Kies "ASP.NET Core Web API", geef het de naam `ShopWave.Api` en zet "Use minimal APIs" aan.

Voeg daarna een projectreferentie toe: rechtsklik op `ShopWave.Api > Add > Project Reference > ShopWave`. Zo heeft de API toegang tot alle klassen in `ShopWave/Security/`.

Bouw de solution.

Wat je ziet:

```
Build succeeded.
```

---

### Stap 7a: Kestrel configureren voor HTTPS

Kestrel is de ingebouwde webserver van ASP.NET Core. Standaard draait hij op HTTP. We configureren hem om een self-signed certificaat te laden.

Vervang de inhoud van `ShopWave.Api/Program.cs` door:

```csharp
using ShopWave.Security;
using System.Security.Cryptography.X509Certificates;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(5001, listenOptions =>
    {
        X509Certificate2 certificate = CertificateHelper.CreateSelfSignedCertificate("localhost");
        listenOptions.UseHttps(certificate);
    });
});

WebApplication app = builder.Build();

app.MapGet("/", () => "ShopWave API actief op HTTPS");

app.Run();
```

`ConfigureKestrel` configureert de webserver voor je applicatie start. `ListenLocalhost(5001, ...)` vertelt Kestrel op welke poort hij moet luisteren. `UseHttps(certificate)` laadt het self-signed certificaat dat we in les 4 leerden aanmaken.

Start de API via "Run" in Visual Studio of via `dotnet run` in de terminal.

Wat je ziet:

```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:5001
```

Open een browser en ga naar `https://localhost:5001`. Je ziet een beveiligingswaarschuwing. Dat is normaal: het certificaat is self-signed. Klik door naar de pagina.

Wat je ziet in de browser:

```
ShopWave API actief op HTTPS
```

---

### Stap 7b: Certifcaat inspecteren vanuit C#

Voeg een tweede endpoint toe in `ShopWave.Api/Program.cs`:

```csharp
app.MapGet("/certificaat", () =>
{
    X509Certificate2 cert = CertificateHelper.CreateSelfSignedCertificate("ShopWave");

    return new
    {
        Subject    = cert.Subject,
        Issuer     = cert.Issuer,
        ValidFrom  = cert.NotBefore.ToString("yyyy-MM-dd"),
        ValidUntil = cert.NotAfter.ToString("yyyy-MM-dd"),
        SelfSigned = cert.Subject == cert.Issuer
    };
});
```

Ga naar `https://localhost:5001/certificaat`.

Wat je ziet:

```json
{
  "subject": "CN=ShopWave",
  "issuer": "CN=ShopWave",
  "validFrom": "2026-06-15",
  "validUntil": "2027-06-15",
  "selfSigned": true
}
```

`Subject` en `Issuer` zijn identiek. Dat is het kenmerk van een self-signed certificaat: de server heeft zijn eigen certificaat ondertekend, er is geen CA betrokken. Bij een Let's Encrypt-certificaat zou `Issuer` zoiets zijn als `"CN=R11, O=Let's Encrypt, C=US"`.

---

### Stap 7c: Verbinding maken vanuit de console

Voeg tijdelijk toe aan `ShopWave/Program.cs`:

```csharp
using System.Net.Http;

HttpClientHandler handler = new HttpClientHandler();

handler.ServerCertificateCustomValidationCallback =
    (message, cert, chain, errors) =>
    {
        Console.WriteLine($"Subject:    {cert.Subject}");
        Console.WriteLine($"Issuer:     {cert.Issuer}");
        Console.WriteLine($"Geldig tot: {cert.NotAfter:yyyy-MM-dd}");
        Console.WriteLine($"Self-signed: {cert.Subject == cert.Issuer}");
        return true;
    };

HttpClient client   = new HttpClient(handler);
string     response = client.GetStringAsync("https://localhost:5001/").Result;

Console.WriteLine($"\nAntwoord: {response}");

client.Dispose();
handler.Dispose();
```

`ServerCertificateCustomValidationCallback` is een callback die de console-app aanroept zodra TLS het certificaat van de server ontvangt. De `return true` aan het einde zegt: accepteer dit certificaat ook al is het self-signed. In productie doe je dit nooit. Daar vertrouw je op de standaardvalidatie van .NET.

Start eerst de API, daarna de console-app.

Wat je ziet:

```
Subject:    CN=localhost
Issuer:     CN=localhost
Geldig tot: 2027-06-15
Self-signed: True

Antwoord: ShopWave API actief op HTTPS
```

---

### Stap 7d: De TLS-handshake simuleren

We simuleren de kern van de TLS-handshake: de client wisselt een AES-sleutel veilig uit via RSA, en gebruikt die sleutel daarna voor encryptie.

Voeg tijdelijk toe aan `ShopWave/Program.cs`:

```csharp
using System.Security.Cryptography;
using ShopWave.Security;

// SERVER: genereert een RSA-sleutelpaar
// In het echt zit dit in het X.509-certificaat dat de server stuurt.
RSA serverRsa = RSA.Create(2048);

// CLIENT: genereert een willekeurige AES-sessiesleutel (32 bytes = 256 bit)
byte[] sessionKeyBytes = new byte[32];
RandomNumberGenerator.Fill(sessionKeyBytes);

Console.WriteLine($"Sessiesleutel (origineel):  {Convert.ToHexString(sessionKeyBytes)[..32]}...");

// CLIENT: versleutelt de sessiesleutel met de publieke sleutel van de server
// In het echt: de publieke sleutel is ontvangen uit het certificaat tijdens de handshake.
byte[] encryptedSessionKey = serverRsa.Encrypt(
    sessionKeyBytes,
    RSAEncryptionPadding.OaepSHA256);

Console.WriteLine($"Verstuurd (versleuteld):    {Convert.ToHexString(encryptedSessionKey)[..32]}...");

// SERVER: ontsleutelt de sessiesleutel met zijn private sleutel
byte[] decryptedSessionKey = serverRsa.Decrypt(
    encryptedSessionKey,
    RSAEncryptionPadding.OaepSHA256);

Console.WriteLine($"Sessiesleutel (ontvangen):  {Convert.ToHexString(decryptedSessionKey)[..32]}...");
Console.WriteLine($"Sleutels gelijk:            {sessionKeyBytes.SequenceEqual(decryptedSessionKey)}");

// BEIDE PARTIJEN: gebruiken nu de gedeelde sessiesleutel voor AES-encryptie
AesEncryptor encryptor = new AesEncryptor(sessionKeyBytes);
string message          = "alice@shopwave.be | Laptop | 999.99 EUR";
string encrypted        = encryptor.Encrypt(message);
string decrypted        = encryptor.Decrypt(encrypted);

Console.WriteLine($"\nBericht:      {message}");
Console.WriteLine($"Versleuteld:  {encrypted[..40]}...");
Console.WriteLine($"Ontsleuteld:  {decrypted}");

serverRsa.Dispose();
```

`OaepSHA256` is de aanbevolen padding voor RSA-encryptie. Padding voegt willekeurige data toe aan het bericht voor de encryptie, zodat hetzelfde bericht elke keer een andere ciphertext geeft. Zonder padding is RSA kwetsbaar voor bepaalde aanvallen.

Wat je ziet:

```
Sessiesleutel (origineel):  A3F9C2E1B7D4...
Verstuurd (versleuteld):    8F2K9XPQR1LM...
Sessiesleutel (ontvangen):  A3F9C2E1B7D4...
Sleutels gelijk:            True

Bericht:      alice@shopwave.be | Laptop | 999.99 EUR
Versleuteld:  a3Fk9mNpQ2rBk3aLvM8sRt1wJc9dFe6h...
Ontsleuteld:  alice@shopwave.be | Laptop | 999.99 EUR
```

De sessiesleutel wordt versleuteld verstuurd. Een aanvaller die het netwerk onderschept, ziet alleen de versleutelde sessiesleutel, niet de sleutel zelf. Zonder de private sleutel van de server kan hij die niet ontsleutelen. Na de handshake verloopt alle communicatie via AES met de gedeelde sessiesleutel. Dit is de essentie van TLS.

---

### Stap 7e: Security headers toevoegen

HTTP-responses kunnen headers bevatten die de browser instrueren hoe hij met de pagina moet omgaan. Enkele belangrijke beveiligingsheaders:

Voeg toe aan `ShopWave.Api/Program.cs`, vóór `app.Run()`:

```csharp
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options",        "DENY");
    context.Response.Headers.Append("X-XSS-Protection",       "1; mode=block");
    await next();
});
```

| Header | Wat het doet |
|--------|-------------|
| `X-Content-Type-Options: nosniff` | Voorkomt MIME-sniffing: de browser voert een JavaScript-bestand niet uit als het als een afbeelding aangeleverd wordt. |
| `X-Frame-Options: DENY` | Voorkomt dat de pagina in een iframe geladen wordt. Beschermt tegen clickjacking: een aanvaller die een onzichtbare ShopWave-pagina over zijn eigen pagina legt om klikken te stelen. |
| `X-XSS-Protection: 1; mode=block` | Vraagt oudere browsers om XSS-aanvallen te blokkeren. Moderne browsers hebben dit ingebouwd. |

Ga naar `https://localhost:5001/` en open de developer tools (F12). Kijk onder het tabblad "Network" naar de response headers.

Wat je ziet:

```
x-content-type-options: nosniff
x-frame-options: DENY
x-xss-protection: 1; mode=block
```

---

### Stap 7f: HSTS activeren

**HSTS** (HTTP Strict Transport Security) is een header die de browser instrueert om voor alle toekomstige verzoeken naar dit domein automatisch HTTPS te gebruiken, ook als de gebruiker `http://` typt.

Voeg toe aan `ShopWave.Api/Program.cs`, vóór `app.Use(...)`:

```csharp
app.UseHsts();
```

Zet ook in de builder:

```csharp
builder.Services.AddHsts(options =>
{
    options.MaxAge           = TimeSpan.FromDays(365);
    options.IncludeSubDomains = true;
});
```

`MaxAge` bepaalt hoe lang de browser dit onthoudt. Een jaar is de aanbevolen waarde voor productie.

HSTS beschermt tegen een specifieke aanval: een aanvaller die probeert de client naar `http://shopwave.be` te leiden in plaats van `https://shopwave.be`. Als de browser eerder de HSTS-header heeft ontvangen, weigert hij de HTTP-verbinding automatisch en stuurt zichzelf naar HTTPS.

Wat je ziet in de response headers:

```
strict-transport-security: max-age=31536000; includeSubDomains
```

---

## 8. Samenvatting

| Concept | Wat je moet onthouden |
|--------|-----------------------|
| HTTP vs. HTTPS | HTTP verstuurt alles als leesbare tekst. HTTPS versleutelt via TLS. |
| TLS-garanties | Confidentiality (versleuteld), Integrity (onwijzigbaar), Authenticatie (juiste server) |
| TLS-handshake | 6 stappen: ClientHello, ServerHello + certificaat, validatie, sleuteluitwisseling, bevestiging, AES-communicatie |
| RSA in TLS | Alleen voor de sleuteluitwisseling tijdens de handshake. Te traag voor alle communicatie. |
| AES in TLS | Voor alle data na de handshake. Snel. Sleutel is veilig uitgewisseld via RSA. |
| Trust chain | Root CA ondertekent Intermediate CA, die het servercertificaat ondertekent. Browser valideert de volledige keten. |
| Self-signed | Geen CA betrokken. Browser vertrouwt het niet. Alleen voor development. |
| Forward secrecy | Sessiesleutel berekend via Diffie-Hellman, niet via RSA. Private sleutel lekt nooit de sessiesleutel. TLS 1.3 gebruikt altijd forward secrecy. |
| `UseHttps` | Kestrel-configuratie in ASP.NET Core. |
| `ServerCertificateCustomValidationCallback` | Handmatige validatie in `HttpClient`. In productie nooit `return true`. |
| HSTS | Browser gebruikt automatisch HTTPS, ook als je `http://` typt. |
| Security headers | `X-Content-Type-Options`, `X-Frame-Options`, `X-XSS-Protection` beschermen tegen veelvoorkomende browser-aanvallen. |
