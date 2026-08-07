---
title: "Les 6: Oefeningen - HTTPS en TLS"
sidebar_label: "Oefeningen"
---

# Oefeningen: HTTPS en TLS

Werk de oefeningen in volgorde. Elke oefening bouwt verder op de vorige. Kijk niet vooraf in de oplossingen.

Je werkt verder in de ShopWave-solution. Oefening 1 maakt `ShopWave.Api` aan als dat nog niet bestaat.

---

## Startpakket downloaden

[Download het startpakket van les 6](/downloads/shopwave-start-06-https-en-tls.zip) (ZIP)

Hierin staat alles wat je in de vorige lessen gebouwd hebt, samen met de code die je
tijdens de theorie van deze les opbouwt. Wat je in de oefeningen zelf moet schrijven,
staat erin als skelet met de melding `// jouw code hier`.

De webshop zit erbij. Je hoeft geen Razor te kennen: start hem met
`dotnet run --project ShopWave.Web` en open https://localhost:5443. Zo zie je meteen wat je code doet.

---

<h3 class="opdracht-titel">Opdracht</h3>

## Oefening 1: ShopWave API op HTTPS

**Leerdoel:** je configureert een ASP.NET Core Minimal API om op HTTPS te draaien met een self-signed certificaat.

**Moeilijkheidsgraad:** basis

**Situatie:** ShopWave wil zijn API bereikbaar maken via HTTPS. Intern gebruik je het self-signed certificaat dat je in les 4 leerde aanmaken via `CertificateHelper`.

**Wat je doet:**

Voeg een nieuw project toe aan de ShopWave-solution: rechtsklik op de solution > "Add > New Project" > "ASP.NET Core Web API" > naam `ShopWave.Api`. Zet "Use minimal APIs" aan.

Voeg een projectreferentie toe van `ShopWave.Api` naar `ShopWave` zodat de API toegang heeft tot de klassen in `ShopWave/Security/`.

Vervang de inhoud van `ShopWave.Api/Program.cs` zodat de API:

1. Draait op `https://localhost:5001` met een self-signed certificaat voor `"localhost"`.
2. Een endpoint `GET /` heeft dat de tekst `"ShopWave API actief op HTTPS"` teruggeeft.
3. Een endpoint `GET /certificaat` heeft dat het subject, de issuer, de geldigheidsdatum en `SelfSigned: true/false` teruggeeft als JSON.

**Startcode:**

```csharp
using ShopWave.Security;
using System.Security.Cryptography.X509Certificates;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(5001, listenOptions =>
    {
        // jouw code hier: laad het certificaat en activeer HTTPS
    });
});

WebApplication app = builder.Build();

// jouw endpoints hier

app.Run();
```

**Controleer je werk:** start de API en open `https://localhost:5001/certificaat` in de browser. Je ziet een beveiligingswaarschuwing. Klik door. Verwacht resultaat:

```json
{
  "subject": "CN=ShopWave",
  "issuer": "CN=ShopWave",
  "validUntil": "2027-06-15",
  "selfSigned": true
}
```

---

<h3 class="opdracht-titel">Opdracht</h3>

## Oefening 2: TLS-handshake simuleren met AES

**Leerdoel:** je simuleert de kern van de TLS-handshake in C# en ziet concreet hoe RSA en AES samenwerken.

**Moeilijkheidsgraad:** gemiddeld

**Situatie:** je wil aan een medestudent uitleggen waarom TLS twee soorten cryptografie gebruikt. In plaats van een tekening te maken, schrijf je een programma dat de handshake stap voor stap doorloopt.

**Wat je doet:**

Voeg tijdelijk toe aan `ShopWave/Program.cs`. Implementeer de volgende stappen in volgorde:

1. De server genereert een RSA-sleutelpaar van 2048 bits. Dit stelt de private/publieke sleutel voor die normaal in het X.509-certificaat zit.
2. De client genereert een willekeurige sessiesleutel van 32 bytes via `RandomNumberGenerator.Fill`.
3. De client versleutelt de sessiesleutel met de publieke sleutel van de server via `RSA.Encrypt` en `RSAEncryptionPadding.OaepSHA256`.
4. De server ontsleutelt de sessiesleutel met zijn private sleutel via `RSA.Decrypt`.
5. Controleer dat de originele en de ontvangen sessiesleutel identiek zijn via `SequenceEqual`.
6. Gebruik de gedeelde sessiesleutel om een bericht te versleutelen via `AesEncryptor` en ontsleutel het daarna opnieuw.

Druk bij elke stap een regel af zodat je de voortgang kan volgen.

**Startcode:**

```csharp
using System.Security.Cryptography;
using ShopWave.Security;

// Stap 1: server genereert RSA-sleutelpaar
RSA serverRsa = RSA.Create(2048);

// Stap 2: client genereert sessiesleutel
byte[] sessionKey = new byte[32];
// jouw code hier

// Stap 3: client versleutelt sessiesleutel met RSA
// jouw code hier

// Stap 4: server ontsleutelt
// jouw code hier

// Stap 5: vergelijk
// jouw code hier

// Stap 6: communicatie via AES
string message = "alice@shopwave.be | Laptop | 999.99 EUR";
// jouw code hier

serverRsa.Dispose();
```

**Verwacht resultaat:**

```csharp
Sessiesleutel (origineel):  A3F9C2E1B7D408F2...
Verstuurd (versleuteld):    8F2K9XPQR1LM3T7Y...
Sessiesleutel (ontvangen):  A3F9C2E1B7D408F2...
Sleutels gelijk:            True

Bericht:     alice@shopwave.be | Laptop | 999.99 EUR
Versleuteld: a3Fk9mNpQ2rBk3aL...
Ontsleuteld: alice@shopwave.be | Laptop | 999.99 EUR
```

**Reflectievraag:** wat zou er gebeuren als de client de sessiesleutel als plain text zou versturen in stap 3? Wie kan hem dan onderscheppen en wat kan die persoon daarna doen?

---

<h3 class="opdracht-titel">Opdracht</h3>

## Oefening 3: HTTP versus HTTPS vergelijken

**Leerdoel:** je ziet concreet het verschil tussen een beveiligde en een onbeveiligde verbinding vanuit de perspectief van C#-code.

**Moeilijkheidsgraad:** gemiddeld

**Situatie:** de klantenservice van ShopWave vraagt of het echt nodig is om HTTPS te gebruiken. Ze willen een concreet voorbeeld zien van wat er misgaat zonder HTTPS.

**Wat je doet:**

Voeg twee endpoints toe aan `ShopWave.Api/Program.cs`:

1. `GET /onveilig/inlog` dat een antwoord simuleert als HTTP-tekst (plain text, alsof er geen TLS is): `"email=alice@shopwave.be&password=wachtwoord123"`.
2. `GET /veilig/certificaatinfo` dat de certificaatgegevens teruggeeft als JSON (zoals in oefening 1).

Schrijf daarna in `ShopWave/Program.cs` twee console-methoden:

- `ToonOnveiligVerkeer()`: maakt een verbinding via `HttpClient` naar `https://localhost:5001/onveilig/inlog`, toont de response en drukt af: `"Wat een aanvaller op HTTP zou zien: [response]"`.
- `ToonCertificaatInfo()`: maakt een verbinding via `HttpClient` met `ServerCertificateCustomValidationCallback` naar `https://localhost:5001/veilig/certificaatinfo` en toont subject, issuer en of het self-signed is.

**Vereisten:**

- Gebruik `ServerCertificateCustomValidationCallback` voor `ToonCertificaatInfo` zodat het self-signed certificaat geaccepteerd wordt.
- Voeg een duidelijke commentaarregel toe in `ToonOnveiligVerkeer`: `// In productie zou dit endpoint via HTTP bereikbaar zijn. Dan is de response onversleuteld.`

**Controleer je werk:** verwacht resultaat in de console:

```csharp
=== Onveilig scenario ===
Wat een aanvaller op HTTP zou zien: email=alice@shopwave.be&password=wachtwoord123

=== Certificaatinfo ===
Subject:    CN=ShopWave
Issuer:     CN=ShopWave
Self-signed: True
```

**Reflectievraag:** het endpoint `/onveilig/inlog` draait nu ook op HTTPS (poort 5001). In een echt onveilig scenario draait het op HTTP (poort 80). Wat is het fundamentele verschil voor de aanvaller?

---

<h3 class="opdracht-titel">Opdracht</h3>

## Oefening 4: Security headers en HSTS

**Leerdoel:** je voegt beveiligingsheaders toe aan de ShopWave API en begrijpt welke aanval elke header tegenhoudt.

**Moeilijkheidsgraad:** uitdaging

**Situatie:** na een security audit krijgt ShopWave de opmerking dat de API geen beveiligingsheaders verstuurt. Een aanvaller kan de API in een iframe laden (clickjacking) of de browser misleiden met verkeerde content types (MIME-sniffing).

**Wat je doet:**

Voeg in `ShopWave.Api/Program.cs` een middleware toe die bij elke response de volgende headers toevoegt:

- `X-Content-Type-Options: nosniff`
- `X-Frame-Options: DENY`
- `X-XSS-Protection: 1; mode=block`

Activeer ook HSTS:

- Voeg `app.UseHsts()` toe.
- Configureer `AddHsts` in de builder met een `MaxAge` van 365 dagen en `IncludeSubDomains: true`.

Voeg tot slot een nieuw endpoint toe: `GET /headers` dat alle response-headers van het huidige verzoek teruggeeft als JSON, zodat je kan verifiëren dat de headers correct ingesteld zijn.

**Startcode voor de middleware:**

```csharp
app.Use(async (context, next) =>
{
    // jouw headers hier
    await next();
});
```

**Controleer je werk:** ga naar `https://localhost:5001/headers`. Verwacht resultaat:

```json
{
  "x-content-type-options": "nosniff",
  "x-frame-options": "DENY",
  "x-xss-protection": "1; mode=block",
  "strict-transport-security": "max-age=31536000; includeSubDomains"
}
```

Open ook de developer tools (F12 > Network > klik op een request > Response Headers) en verifieer dat de headers aanwezig zijn.

**Reflectievragen:**

1. `X-Frame-Options: DENY` voorkomt dat de ShopWave API in een iframe geladen wordt. Leg in eigen woorden uit hoe een aanvaller een iframe kan misbruiken om een klant te misleiden.
2. HSTS stuurt de browser de instructie om altijd HTTPS te gebruiken. Waarom is dat nuttig als een klant per ongeluk `http://shopwave.be` typt?

---

<h3 class="opdracht-titel">Opdracht</h3>

## Oefening 5: Echt certificaat analyseren en CIA koppelen

**Leerdoel:** je analyseert een productiecertificaat en koppelt de beveiligingsgaranties van HTTPS aan het CIA-model.

**Moeilijkheidsgraad:** basis

**Situatie:** een klant vraagt of de verbinding met ShopWave veilig is. Jij legt uit wat het slotje in de adresbalk betekent en welke garanties HTTPS precies geeft.

**Wat je doet:**

Ga in je browser naar `https://ap.be`. Klik op het slotje in de adresbalk en open de certificaatdetails.

Beantwoord de volgende vragen op papier of in een tekstbestand:

1. Wat is de issuer van het certificaat? Is dat een Root CA of een Intermediate CA?
2. Tot wanneer is het certificaat geldig?
3. Welk algoritme staat bij "Signature Algorithm"?
4. Wat is het verschil tussen dit certificaat en het self-signed certificaat uit de demo? Waarom vertrouwt de browser dit certificaat wel?
5. ShopWave verstuurt een betaling via HTTPS. Welke CIA-pijler beschermt de versleuteling? Welke beschermt de integriteitscontrole? En welke beschermt de authenticatie van de server?
6. Een aanvaller slaagt erin de response van de ShopWave API te onderscheppen en het bedrag te wijzigen van 999 EUR naar 1 EUR. De verbinding gebruikt HTTPS. Is dit mogelijk? Leg uit waarom wel of niet.

---

## Controleer je werk in de webshop

Start de webshop met `dotnet run --project ShopWave.Web` en open https://localhost:5443. Zo zie je je eigen code draaien in plaats van alleen een groene testbalk.

| Wat je doet | Wat je ziet als je code klopt |
|-------------|-------------------------------|
| Start de webshop en open `https://localhost:5443` | Een certificaatwaarschuwing. Die hoort er te zijn bij een self-signed certificaat. |
| Bekijk het certificaat in je browser | Subject en issuer zijn allebei `CN=ShopWave`: het certificaat tekent zichzelf |
| Open de netwerktab van je browser en bekijk de response headers | `X-Content-Type-Options`, `X-Frame-Options` en `Strict-Transport-Security` |
| Probeer `http://localhost:5443` | Dat werkt niet: de poort spreekt alleen TLS |

Onder elk resultaat staat uit welke klasse het komt. Zie je iets anders dan hierboven, dan weet je meteen welke methode je moet nakijken.
