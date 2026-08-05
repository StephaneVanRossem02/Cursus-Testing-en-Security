---
title: "Les 6: Oplossingen - HTTPS en TLS"
sidebar_label: "Oplossingen"
---

# Oplossingen: HTTPS en TLS

**Bekijk dit pas nadat je de oefeningen zelf geprobeerd hebt.** Lees de toelichting ook als je het juist had.

---

## Oplossing 1: ShopWave API op HTTPS

### ShopWave.Api/Program.cs

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

app.MapGet("/certificaat", () =>
{
    X509Certificate2 cert = CertificateHelper.CreateSelfSignedCertificate("ShopWave");

    return new
    {
        Subject    = cert.Subject,
        Issuer     = cert.Issuer,
        ValidUntil = cert.NotAfter.ToString("yyyy-MM-dd"),
        SelfSigned = cert.Subject == cert.Issuer
    };
});

app.Run();
```

### Toelichting

`CertificateHelper.CreateSelfSignedCertificate("localhost")` geeft een certificaat met `CN=localhost`. Dat is de domeinnaam die je gebruikt als je naar `https://localhost:5001` gaat. Als de domeinnaam in het certificaat niet overeenkomt met de domeinnaam in de URL, weigert de browser de verbinding. Voor lokale development gebruik je altijd `"localhost"`.

Het `/certificaat`-endpoint maakt een nieuw certificaat aan met `"ShopWave"` als naam. Dat is bedoeld als demonstratie. In een echte API zou je het certificaat dat al geladen is opvragen, niet een nieuw aanmaken per request.

`cert.Subject == cert.Issuer` werkt als snelle check voor self-signed. Bij een certificaat van een CA zijn Subject (`CN=shopwave.be`) en Issuer (`CN=R11, O=Let's Encrypt, C=US`) altijd verschillend.

**Veelgemaakte fout:** studenten gebruiken `"ShopWave"` als certificaatnaam voor Kestrel in plaats van `"localhost"`. Dan ziet de browser dat de domeinnaam in het certificaat (`CN=ShopWave`) niet overeenkomt met de URL (`localhost`) en weigert de verbinding.

---

## Oplossing 2: TLS-handshake simuleren met AES

```csharp
using System.Security.Cryptography;
using ShopWave.Security;

// Stap 1: server genereert RSA-sleutelpaar
RSA serverRsa = RSA.Create(2048);

// Stap 2: client genereert sessiesleutel
byte[] sessionKey = new byte[32];
RandomNumberGenerator.Fill(sessionKey);

Console.WriteLine($"Sessiesleutel (origineel):  {Convert.ToHexString(sessionKey)[..32]}...");

// Stap 3: client versleutelt sessiesleutel met publieke RSA-sleutel
byte[] encryptedSessionKey = serverRsa.Encrypt(
    sessionKey,
    RSAEncryptionPadding.OaepSHA256);

Console.WriteLine($"Verstuurd (versleuteld):    {Convert.ToHexString(encryptedSessionKey)[..32]}...");

// Stap 4: server ontsleutelt met private sleutel
byte[] decryptedSessionKey = serverRsa.Decrypt(
    encryptedSessionKey,
    RSAEncryptionPadding.OaepSHA256);

Console.WriteLine($"Sessiesleutel (ontvangen):  {Convert.ToHexString(decryptedSessionKey)[..32]}...");

// Stap 5: vergelijken
bool keysMatch = sessionKey.SequenceEqual(decryptedSessionKey);
Console.WriteLine($"Sleutels gelijk:            {keysMatch}");

// Stap 6: AES-communicatie met de gedeelde sessiesleutel
AesEncryptor encryptor = new AesEncryptor(sessionKey);
string message          = "alice@shopwave.be | Laptop | 999.99 EUR";
string encrypted        = encryptor.Encrypt(message);
string decrypted        = encryptor.Decrypt(encrypted);

Console.WriteLine($"\nBericht:     {message}");
Console.WriteLine($"Versleuteld: {encrypted[..40]}...");
Console.WriteLine($"Ontsleuteld: {decrypted}");

serverRsa.Dispose();
```

### Toelichting

`RSA.Create(2048)` genereert een sleutelpaar met zowel de private als de publieke sleutel in hetzelfde object. Dat is waarom we in deze simulatie hetzelfde `serverRsa`-object gebruiken voor zowel `Encrypt` als `Decrypt`. In een echte TLS-verbinding heeft de client alleen de publieke sleutel (via het certificaat) en heeft de server de private sleutel. Ze werken op twee aparte machines.

`RSAEncryptionPadding.OaepSHA256` is de aanbevolen padding. Padding voegt willekeurige bytes toe aan het bericht vóór encryptie. Dat zorgt ervoor dat hetzelfde bericht elke keer een andere ciphertext geeft, wat bepaalde statistische aanvallen onmogelijk maakt.

`SequenceEqual` vergelijkt twee byte-arrays element voor element. Je kan ze niet vergelijken met `==` omdat dat de referenties vergelijkt, niet de inhoud.

**Antwoord op de reflectievraag:** als de client de sessiesleutel als plain text verstuurt, kan elke tussenpersoon die het netwerk onderschept de sleutel lezen. Die persoon heeft dan de AES-sessiesleutel en kan alle verdere communicatie ontsleutelen. RSA zorgt ervoor dat de sessiesleutel alleen leesbaar is voor de houder van de private sleutel: de echte server.

**Veelgemaakte fout:** studenten vergeten `serverRsa.Dispose()`. `RSA`-objecten bevatten cryptografische sleutels in onbeheerd geheugen. `Dispose` zorgt dat die veilig gewist worden.

---

## Oplossing 3: HTTP versus HTTPS vergelijken

### ShopWave.Api/Program.cs (endpoints toevoegen)

```csharp
app.MapGet("/onveilig/inlog", () =>
{
    // In productie zou dit endpoint via HTTP bereikbaar zijn. Dan is de response onversleuteld.
    return "email=alice@shopwave.be&password=wachtwoord123";
});

app.MapGet("/veilig/certificaatinfo", () =>
{
    X509Certificate2 cert = CertificateHelper.CreateSelfSignedCertificate("ShopWave");
    return new
    {
        Subject    = cert.Subject,
        Issuer     = cert.Issuer,
        SelfSigned = cert.Subject == cert.Issuer
    };
});
```

### ShopWave/Program.cs (console-methoden)

```csharp
using System.Net.Http;
using System.Net.Security;

ToonOnveiligVerkeer();
ToonCertificaatInfo();

void ToonOnveiligVerkeer()
{
    Console.WriteLine("=== Onveilig scenario ===");

    HttpClient client   = new HttpClient(new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback =
            (msg, cert, chain, errors) => true
    });

    string response = client.GetStringAsync("https://localhost:5001/onveilig/inlog").Result;

    // In productie zou dit endpoint via HTTP bereikbaar zijn. Dan is de response onversleuteld.
    Console.WriteLine($"Wat een aanvaller op HTTP zou zien: {response}");
    Console.WriteLine();

    client.Dispose();
}

void ToonCertificaatInfo()
{
    Console.WriteLine("=== Certificaatinfo ===");

    string subject    = string.Empty;
    string issuer     = string.Empty;
    bool   selfSigned = false;

    HttpClientHandler handler = new HttpClientHandler();
    handler.ServerCertificateCustomValidationCallback =
        (msg, cert, chain, errors) =>
        {
            subject    = cert.Subject;
            issuer     = cert.Issuer;
            selfSigned = cert.Subject == cert.Issuer;
            return true;
        };

    HttpClient client = new HttpClient(handler);
    _ = client.GetStringAsync("https://localhost:5001/veilig/certificaatinfo").Result;

    Console.WriteLine($"Subject:     {subject}");
    Console.WriteLine($"Issuer:      {issuer}");
    Console.WriteLine($"Self-signed: {selfSigned}");

    client.Dispose();
    handler.Dispose();
}
```

### Toelichting

`ServerCertificateCustomValidationCallback` wordt aangeroepen tijdens de TLS-handshake, nadat de server zijn certificaat heeft gestuurd maar vóórdat de verbinding opgebouwd wordt. Je ontvangt het certificaat als parameter en kan het inspecteren. Door `return true` terug te geven, accepteer je het certificaat ook al is het self-signed.

In `ToonCertificaatInfo` sla je de certificaatgegevens op in variabelen die buiten de callback gedeclareerd zijn. De callback heeft toegang tot die variabelen via closure. Na `GetStringAsync` zijn de variabelen gevuld.

**Antwoord op de reflectievraag:** het `/onveilig/inlog`-endpoint draait nu op HTTPS (poort 5001), dus de response is wél versleuteld. In een echt onveilig scenario draait het op HTTP (poort 80). Dan stuurt de server de response als plain text en kan elke tussenpersoon die onderscheppen en lezen. HTTPS versleutelt de response zodat een onderschepte response onleesbaar is.

**Veelgemaakte fout:** studenten gebruiken `.GetAwaiter().GetResult()` of `.Wait()` in plaats van `.Result`. Alle drie blokkeren de thread, maar `.Result` is consistent met hoe de rest van de code in dit programma geschreven is.

---

## Oplossing 4: Security headers en HSTS

### ShopWave.Api/Program.cs

```csharp
using ShopWave.Security;
using System.Security.Cryptography.X509Certificates;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddHsts(options =>
{
    options.MaxAge            = TimeSpan.FromDays(365);
    options.IncludeSubDomains = true;
});

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(5001, listenOptions =>
    {
        X509Certificate2 certificate = CertificateHelper.CreateSelfSignedCertificate("localhost");
        listenOptions.UseHttps(certificate);
    });
});

WebApplication app = builder.Build();

app.UseHsts();

app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options",        "DENY");
    context.Response.Headers.Append("X-XSS-Protection",       "1; mode=block");
    await next();
});

app.MapGet("/", () => "ShopWave API actief op HTTPS");

app.MapGet("/headers", (HttpContext context) =>
{
    Dictionary<string, string> headers = new Dictionary<string, string>();

    foreach (var header in context.Response.Headers)
    {
        headers[header.Key.ToLower()] = header.Value.ToString();
    }

    return headers;
});

app.Run();
```

### Toelichting

De volgorde van middleware is belangrijk. `app.UseHsts()` en `app.Use(...)` moeten vóór `app.MapGet(...)` staan. ASP.NET Core verwerkt middleware in de volgorde waarin je ze registreert. Als je de headers-middleware ná de endpoints zet, worden de headers niet toegevoegd aan de responses van die endpoints.

`app.Use(async (context, next) => ...)` is inline middleware. `await next()` roept de volgende middleware aan in de pipeline. Als je `await next()` niet aanroept, stopt de pipeline en ontvangen de endpoints het verzoek nooit.

`context.Response.Headers.Append` voegt een header toe zonder bestaande headers te overschrijven. Gebruik `Append` in plaats van een directe toewijzing om te vermijden dat bestaande headers (zoals `Content-Type`) worden gewist.

**Antwoord op de reflectievragen:**

**Vraag 1 (clickjacking):** een aanvaller maakt een eigen webpagina en laadt de ShopWave-betaalpagina in een onzichtbaar iframe erover. De klant ziet de pagina van de aanvaller maar klikt onbewust op knoppen in de ShopWave-iframe: hij bevestigt een betaling zonder dat hij dat weet. `X-Frame-Options: DENY` voorkomt dat de browser de ShopWave-pagina in een iframe laadt.

**Vraag 2 (HSTS):** als een klant `http://shopwave.be` typt, stuurt de browser normaal een HTTP-request. Een aanvaller kan die request onderscheppen vóór de redirect naar HTTPS plaatsvindt (SSL stripping aanval). Als de browser eerder de HSTS-header van ShopWave ontvangen heeft, zet hij automatisch `http://` om naar `https://` nog voor het request verstuurd wordt. De aanvaller krijgt nooit de kans om het HTTP-verkeer te onderscheppen.

**Veelgemaakte fout:** studenten roepen `app.UseHsts()` aan zonder `builder.Services.AddHsts(...)` te configureren. Dan gebruikt ASP.NET Core de standaardwaarden (60 seconden in plaats van een jaar). Voor productie is dat te kort.

---

## Oplossing 5: Echt certificaat analyseren en CIA koppelen

**Vraag 1: issuer**

De issuer van het certificaat van `ap.be` is een Intermediate CA, typisch iets als `"Sectigo RSA Domain Validation Secure Server CA"`. Browsers werken zelden rechtstreeks met Root CAs. Een Root CA ondertekent Intermediate CAs, die op hun beurt servercertificaten ondertekenen. Dat verkleint het risico: als een Intermediate CA gecompromitteerd wordt, kan die ingetrokken worden zonder de Root CA aan te passen.

**Vraag 2: geldigheid**

Typisch 90 dagen (Let's Encrypt) of 1 jaar (commerciële CAs). Let's Encrypt hanteert 90 dagen bewust: korte geldigheidsperiodes beperken de schade als een privésleutel uitlekt. Na 90 dagen wordt het certificaat automatisch vernieuwd.

**Vraag 3: algoritme**

Typisch `sha256WithRSAEncryption` of `ecdsa-with-SHA256`. SHA-256 berekent de hash van het certificaat. RSA of ECDSA versleutelt die hash met de private sleutel van de CA. Dat is de handtekening van de CA op het certificaat.

**Vraag 4: verschil met self-signed**

Bij het certificaat van `ap.be` is de issuer een CA die opgenomen is in de trust store van het besturingssysteem. De browser heeft die CA al eerder vertrouwd. Bij een self-signed certificaat is de issuer dezelfde als het subject: de server heeft zichzelf ondertekend. Er is geen externe verificatie. De browser heeft geen reden om dat te vertrouwen.

**Vraag 5: CIA-koppeling**

- Versleuteling via AES: **Confidentiality**. De betalingsdata is onleesbaar voor iedereen zonder de sessiesleutel.
- Integriteitscontrole via MAC: **Integrity**. Elke wijziging in een pakket wordt gedetecteerd en de verbinding wordt verbroken.
- Authenticatie via certificaat: **buiten CIA**, maar nauw verwant aan Confidentiality en Integrity. Zonder authenticatie is versleuteling zinloos: je versleutelt naar de verkeerde server.

**Vraag 6: aanvaller wijzigt bedrag via HTTPS**

Nee, dat is niet mogelijk via een man-in-the-middle-aanval als de verbinding HTTPS gebruikt. TLS garandeert integriteit via een berichtauthenticatiecode (MAC). Elke byte van elk pakket is beschermd. Als een aanvaller ook maar één byte wijzigt, detecteert TLS dat onmiddellijk en verbreekt de verbinding. De aanvaller kan het pakket niet wijzigen zonder de sessiesleutel, en die heeft hij niet.

---

## Dit project downloaden

[Download het volledige ShopWave-project van les 6](/downloads/shopwave-06-https-en-tls.zip) (ZIP)

Bevat alle code tot en met deze les, klaar om te openen in Visual Studio. Bouwen en testen doe je met `dotnet build` en `dotnet test`. In de `README.md` staat wat er nieuw is en hoeveel tests er horen te slagen.

Alle lessen samen vind je op [Oplossingen downloaden](../../oplossingen-downloaden.md).
