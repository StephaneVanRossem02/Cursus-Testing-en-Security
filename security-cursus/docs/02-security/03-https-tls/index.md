---
title: "Les 6: Security — HTTPS, TLS en certificaten"
sidebar_label: "HTTPS & TLS"
---

# Les 6: Security — HTTPS, TLS en certificaten

## ShopWave

ShopWave heeft ondertussen een solide beveiligingsbasis: wachtwoorden worden gehashed met salt, 2FA beschermt de loginflow en orderbevestigingen worden digitaal ondertekend. Maar al die beveiliging heeft weinig zin als de communicatie tussen de client en de server onbeveiligd verloopt. Iemand die het netwerk afluistert kan wachtwoorden en tokens gewoon meelezen.

In deze les voegen we HTTPS toe aan ShopWave en begrijpen we precies wat er technisch achter de schermen gebeurt.

---

## Theorie

### 1. HTTP versus HTTPS

**HTTP** is het protocol waarmee een browser of client communiceert met een server. Elke request en response wordt verstuurd als **leesbare tekst**. Iedereen die het netwerkverkeer kan onderscheppen — op een open wifi-netwerk, via een man-in-the-middle-aanval — kan alles meelezen en zelfs aanpassen.

```
HTTP-request (leesbaar voor iedereen op het netwerk):
POST /login HTTP/1.1
Host: shopwave.be

{ "email": "alice@shopwave.be", "password": "wachtwoord123" }
```

**HTTPS** voegt een beveiligingslaag toe bovenop HTTP via het **TLS-protocol** (Transport Layer Security). TLS zorgt voor drie garanties die we herkennen uit het CIA-model:

| Garantie | CIA-pijler | Omschrijving |
|----------|-----------|-------------|
| Vertrouwelijkheid | Confidentiality | Berichten zijn versleuteld — niemand kan meelezen |
| Integriteit | Integrity | Berichten kunnen onderweg niet ongemerkt gewijzigd worden |
| Authenticatie | — | De client weet dat de server echt is wie hij beweert te zijn |

---

### 2. De TLS-handshake

Voordat er ook maar één byte aan applicatiedata verstuurd wordt, voeren client en server de **TLS-handshake** uit. Die regelt drie dingen:

1. Identiteitscontrole van de server via het X.509-certificaat
2. Overeenstemming over de te gebruiken algoritmes
3. Uitwisseling van een gedeelde sessiesleutel

```
Client                                    Server
  |                                          |
  |--- ClientHello -----------------------> |
  |    (TLS-versie, willekeurig getal)      |
  |                                          |
  |<-- ServerHello + Certificaat ---------- |
  |    (X.509-certificaat met public key)   |
  |                                          |
  |  [Client valideert certificaat:         |
  |   CA vertrouwd? Niet verlopen?          |
  |   Domein klopt?]                        |
  |                                          |
  |--- Sessiesleutel (versleuteld) -------> |
  |    (versleuteld met public key server)  |
  |                                          |
  |  [Beide berekenen dezelfde sleutel]     |
  |                                          |
  |=== Beveiligde verbinding actief ======= |
  |    (AES-symmetrische encryptie)         |
```

**Waarom twee soorten cryptografie?**

Asymmetrische cryptografie (RSA) is veilig maar traag — enkel geschikt voor kleine hoeveelheden data. Symmetrische cryptografie (AES) is snel maar vereist dat beide partijen dezelfde sleutel al kennen.

TLS combineert beide: RSA om de sessiesleutel veilig uit te wisselen, AES voor alle communicatie daarna.

---

### 3. X.509-certificaten en de trust chain

In les 4 maakten we self-signed certificaten. In productie wordt een certificaat uitgegeven door een **Certificate Authority (CA)** die browsers vertrouwen.

```
Root CA  (ingebouwd in Windows/macOS trust store)
  └── Intermediate CA  (ondertekend door Root CA)
       └── Servercertificaat  (ondertekend door Intermediate CA)
```

Wanneer een browser verbinding maakt, valideert hij de volledige keten. Pas als alle stappen slagen, is de verbinding veilig.

Een **self-signed certificaat** heeft geen CA. De browser vertrouwt het niet en toont een waarschuwing. Voor development is dat geen probleem — de verbinding is wél versleuteld.

---

### 4. Alles komt samen

TLS is geen nieuw concept — het combineert alles wat we al gezien hebben:

| Concept | Les | Rol in TLS |
|---------|-----|------------|
| Hashing (SHA-256) | Les 2 | Integriteitscontrole van berichten |
| Symmetrische encryptie (AES) | Les 2 | Versleuteling na de handshake |
| Asymmetrische cryptografie (RSA) | Les 4 | Uitwisseling van de sessiesleutel |
| Digitale handtekening | Les 4 | Server bewijst zijn identiteit |
| X.509-certificaat | Les 4 | Koppelt public key aan identiteit van de server |

---

## Demo

We bouwen een ShopWave Minimal API die op HTTPS draait. Daarna inspecteren we het certificaat vanuit een console-client en simuleren we de kern van de TLS-handshake.

### Projectstructuur

Voeg een nieuw project toe aan de bestaande solution:

```
ShopWave          ← Console App (bestaand)
ShopWave.Tests    ← xUnit (bestaand)
ShopWave.Api      ← ASP.NET Core Web API (nieuw)
```

Rechtsklik op de solution → Add → New Project → ASP.NET Core Web API → naam: `ShopWave.Api`.

Voeg daarna een project reference toe van `ShopWave.Api` naar `ShopWave` zodat de API de bestaande Security-klassen kan gebruiken.

---

### Stap 1 — ShopWave.Api configureren met HTTPS

**Bestand: `ShopWave.Api/Program.cs`**

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

app.MapGet("/orders/{email}", (string email) =>
{
    X509Certificate2 certificate = CertificateHelper.CreateSelfSignedCertificate("ShopWave");
    OrderSigner      signer      = new OrderSigner(certificate);
    string           orderData   = $"{email} | Laptop | 999.99 EUR";
    string           signature   = signer.Sign(orderData);

    return new { Order = orderData, Signature = signature };
});

app.Run();
```

Start de API. De server draait nu op `https://localhost:5001`.

Navigeer in de browser naar `https://localhost:5001`. Je ziet een beveiligingswaarschuwing — precies omdat het certificaat self-signed is en niet door een CA is uitgegeven. De verbinding is wél versleuteld.

---

### Stap 2 — Certificaat inspecteren vanuit de console

**Bestand: `ShopWave/Program.cs`** (tijdelijk toevoegen onderaan)

```csharp
using System.Net.Http;

HttpClientHandler handler = new HttpClientHandler();

handler.ServerCertificateCustomValidationCallback =
    (message, certificate, chain, errors) =>
    {
        Console.WriteLine($"Subject:    {certificate.Subject}");
        Console.WriteLine($"Issuer:     {certificate.Issuer}");
        Console.WriteLine($"Geldig van: {certificate.NotBefore}");
        Console.WriteLine($"Geldig tot: {certificate.NotAfter}");

        // Voor development: self-signed certificaat accepteren
        return true;
    };

HttpClient          client   = new HttpClient(handler);
HttpResponseMessage response = client.GetAsync("https://localhost:5001/").Result;

Console.WriteLine($"HTTP status: {response.StatusCode}");
Console.WriteLine(response.Content.ReadAsStringAsync().Result);

handler.Dispose();
client.Dispose();
```

Observeer in de output: `Subject` en `Issuer` zijn identiek. Dat is het kenmerk van een self-signed certificaat — de server heeft het zelf ondertekend, er is geen CA betrokken.

---

### Stap 3 — De TLS-handshake simuleren

De kern van TLS is dat de client een sessiesleutel veilig naar de server stuurt. We simuleren dat hier volledig in `Program.cs`.

Omdat we in dit programma zowel de client- als de server-kant simuleren, gebruiken we hetzelfde RSA-object voor versleutelen en ontsleutelen. In een echte verbinding hebben client en server elk hun eigen stukje: de client heeft alleen de public key (via het certificaat), de server heeft de private key.

**Bestand: `ShopWave/Program.cs`** (tijdelijk toevoegen)

```csharp
using System.Security.Cryptography;

// SERVER: genereert een RSA-sleutelpaar
// (in het echt zit dit in het X.509-certificaat)
RSA serverRsa = RSA.Create(2048);

// CLIENT: genereert een willekeurige sessiesleutel
byte[] sessionKey = new byte[32];
RandomNumberGenerator.Fill(sessionKey);

Console.WriteLine($"Sessiesleutel (origineel):   {Convert.ToHexString(sessionKey)}");

// CLIENT: versleutelt de sessiesleutel met de public key van de server
// (in het echt: ontvangen via het certificaat tijdens de handshake)
byte[] encryptedKey = serverRsa.Encrypt(sessionKey, RSAEncryptionPadding.OaepSHA256);

Console.WriteLine($"Versleuteld (verstuurd):      {Convert.ToHexString(encryptedKey).Substring(0, 40)}...");

// SERVER: ontsleutelt de sessiesleutel met zijn private key
byte[] decryptedKey = serverRsa.Decrypt(encryptedKey, RSAEncryptionPadding.OaepSHA256);

Console.WriteLine($"Sessiesleutel (ontsleuteld):  {Convert.ToHexString(decryptedKey)}");

bool match = sessionKey.SequenceEqual(decryptedKey);
Console.WriteLine($"Sleutels komen overeen:       {match}");

serverRsa.Dispose();
```

Wat dit toont:
- De sessiesleutel wordt versleuteld verstuurd — niemand die het netwerk onderschept kan ze achterhalen zonder de private key
- Na de handshake kennen beide partijen dezelfde sessiesleutel en schakelen ze over op snelle AES-encryptie
- Dit is de essentie van de TLS-handshake

---

## Oefeningen

### Oefening 1 — ShopWave API uitbreiden

Breid `ShopWave.Api/Program.cs` uit met de volgende endpoints:

1. `POST /register` — accepteert `{ email, password }`, registreert een account via `AccountRepository`
2. `POST /login` — accepteert `{ email, password }`, start de loginflow en retourneert `{ status }` met de melding
3. `POST /verify` — accepteert `{ email, code }`, verifieert de 2FA-code

Zorg dat de API actief blijft op `https://localhost:5001`.

Test elk endpoint via de browser-adresbalk (voor GET) of via `HttpClient` in het console-project.

---

### Oefening 2 — Certificaat inspecteren en CIA koppelen

Schrijf in `ShopWave/Program.cs` een stuk code dat verbinding maakt met `https://localhost:5001` en het volgende afdrukt:

- Subject
- Issuer
- Geldig van / geldig tot
- Of Subject en Issuer gelijk zijn (als bool)

Beantwoord daarna schriftelijk:

1. Welke CIA-pijler garandeert HTTPS via het certificaat?
2. Waarom toont de browser een waarschuwing bij een self-signed certificaat?
3. Wat zou er anders zijn als het certificaat door Let's Encrypt was uitgegeven?

---

### Oefening 3 — Handshake uitbreiden met AES

Breid de handshake-simulatie uit stap 3 van de demo uit.

Na de uitwisseling van de sessiesleutel schakelt TLS over op AES. Simuleer dat:

1. Genereer een RSA-sleutelpaar (server)
2. Genereer een willekeurige AES-sleutel van 32 bytes (client)
3. Versleutel de AES-sleutel met RSA (client verstuurt dit)
4. Ontsleutel de AES-sleutel met RSA (server ontvangt dit)
5. Versleutel nu een bericht met de AES-sleutel — gebruik `AesEncryptor` uit les 2
6. Ontsleutel het bericht met dezelfde sleutel aan de serverkant
7. Druk het originele bericht, het versleutelde bericht en het ontsleutelde bericht af

Alles staat in `ShopWave/Program.cs`.

Beantwoord:
- Waarom is het veilig om de AES-sleutel versleuteld te versturen via RSA?
- Wat zou er mislopen als de client de AES-sleutel in plain text zou versturen?

---

### Oefening 4 — Handtekening in de TLS-context

In TLS ondertekent de server berichten tijdens de handshake om zijn identiteit te bewijzen. Dit is dezelfde techniek als in les 4.

Schrijf in `ShopWave/Program.cs`:

1. Maak een RSA-sleutelpaar aan
2. Maak een bericht: `"ShopWave server actief op " + DateTime.UtcNow.ToString()`
3. Onderteken het bericht met de private key
4. Verifieer de handtekening met de public key
5. Pas het bericht daarna aan (één letter anders) en probeer de handtekening opnieuw te verifiëren

Gebruik `RSA.SignData` en `RSA.VerifyData` met `HashAlgorithmName.SHA256` en `RSASignaturePadding.Pkcs1`.

Beantwoord:
- Wat garandeert een geldige handtekening?
- Wat detecteert de verificatie na de aanpassing?
- Hoe past dit in de TLS-handshake?

---

### Oefening 5 — Echt certificaat analyseren

Navigeer in je browser naar `https://ap.be`. Klik op het slotje in de adresbalk en bekijk het certificaat.

Beantwoord:
1. Wie is de issuer?
2. Hoe lang is het certificaat geldig?
3. Welk algoritme staat er bij "Signature Algorithm"?
4. Wat is het verschil met het self-signed certificaat uit de demo?
5. Waarom geeft de browser hier geen waarschuwing terwijl hij dat bij ons localhost-certificaat wel doet?

---

## Modeloplossing

> De modeloplossing is beschikbaar na het indienen van de labo-opdracht via Digitap.

---

### Modeloplossing Oefening 3 — Handshake met AES

**Bestand: `ShopWave/Program.cs`**

```csharp
using ShopWave.Security;
using System.Security.Cryptography;
using System.Text;

// Stap 1: server genereert RSA-sleutelpaar
RSA serverRsa = RSA.Create(2048);

// Stap 2: client genereert willekeurige AES-sleutel
byte[] rawKey = new byte[32];
RandomNumberGenerator.Fill(rawKey);

// AesEncryptor verwacht een string van 32 tekens als sleutel
string aesKey = Convert.ToBase64String(rawKey).Substring(0, 32);
string aesIv  = Convert.ToBase64String(rawKey).Substring(0, 16);

// Stap 3: client versleutelt de AES-sleutel met RSA
byte[] encryptedKey = serverRsa.Encrypt(Encoding.UTF8.GetBytes(aesKey), RSAEncryptionPadding.OaepSHA256);
byte[] encryptedIv  = serverRsa.Encrypt(Encoding.UTF8.GetBytes(aesIv),  RSAEncryptionPadding.OaepSHA256);

// Stap 4: server ontsleutelt de AES-sleutel met zijn private key
string decryptedKey = Encoding.UTF8.GetString(serverRsa.Decrypt(encryptedKey, RSAEncryptionPadding.OaepSHA256));
string decryptedIv  = Encoding.UTF8.GetString(serverRsa.Decrypt(encryptedIv,  RSAEncryptionPadding.OaepSHA256));

// Stap 5: versleutel een bericht met AES (client-kant)
string originalMessage  = "alice@shopwave.be | Laptop | 999.99 EUR";
AesEncryptor encryptor  = new AesEncryptor(aesKey, aesIv);
string encryptedMessage = encryptor.Encrypt(originalMessage);

// Stap 6: ontsleutel het bericht (server-kant, met de ontvangen sleutel)
AesEncryptor serverEncryptor = new AesEncryptor(decryptedKey, decryptedIv);
string decryptedMessage      = serverEncryptor.Decrypt(encryptedMessage);

// Stap 7: resultaat
Console.WriteLine($"Origineel:   {originalMessage}");
Console.WriteLine($"Versleuteld: {encryptedMessage}");
Console.WriteLine($"Ontsleuteld: {decryptedMessage}");
Console.WriteLine($"Overeenkomst: {originalMessage == decryptedMessage}");

serverRsa.Dispose();
```

---

## Samenvatting

| Concept | Wat je moet onthouden |
|---------|-----------------------|
| HTTPS | HTTP + TLS: versleuteld, integer en geauthenticeerd |
| TLS-handshake | Certificaat valideren → sessiesleutel uitwisselen → communiceren via AES |
| TLS 1.2 vs 1.3 | TLS 1.2 wisselt de AES-sleutel via RSA uit; TLS 1.3 (aanbevolen) gebruikt Diffie-Hellman — ook als de server-private key later uitlekt, blijft oude communicatie beschermd (forward secrecy) |
| Self-signed certificaat | Geen CA betrokken, browser vertrouwt het niet, enkel voor development |
| Trust chain | Root CA → Intermediate CA → Servercertificaat |
| `UseHttps` | Kestrel-configuratie voor HTTPS in ASP.NET Core |
| HSTS | HTTP Strict Transport Security: browser weigert HTTP-verbindingen na eerste HTTPS-bezoek (`UseHsts()` in ASP.NET Core) |
| Security headers | `X-Content-Type-Options`, `X-Frame-Options`, `Content-Security-Policy` — voeg toe via middleware of NuGet `NWebsec` |
| `ServerCertificateCustomValidationCallback` | Handmatige certificaatvalidatie in `HttpClient` |
| `RSA.Encrypt` / `RSA.Decrypt` | Asymmetrische encryptie (basis van TLS 1.2-sleuteluitwisseling) |

---

## Volgende les

Les 7 is **Security: JWT en OAuth2**. We breiden de ShopWave API uit met tokengebaseerde authenticatie: na een geslaagde login krijgt de gebruiker een JWT-token dat hij meestuurt bij elke request naar beveiligde endpoints.