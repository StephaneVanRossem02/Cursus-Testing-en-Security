---
title: "Les 11: Theorie - Ethisch Hacken"
sidebar_label: "Theorie"
---

# Les 11: Theorie - Ethisch Hacken

## 1. Wat is ethisch hacken?

In de vorige lessen bouw je beveiliging in: je fixt SQL Injection, je configureert CORS, je voegt rate limiting toe. Maar het schrijven van de fix is niet hetzelfde als bewijzen dat de fix werkt. Een aanvaller zoekt niet naar de code. Hij zoekt naar het gedrag van de draaiende applicatie.

**Ethisch hacken** is het systematisch testen van een systeem op kwetsbaarheden, vanuit het perspectief van een aanvaller. Het verschil met kwaadaardige hacking is toestemming en intentie. Een ethisch hacker heeft schriftelijke toestemming van de eigenaar en rapporteert zijn bevindingen zodat ze opgelost worden.

**Het wettelijk kader is niet onderhandelbaar.** In België valt ongeautoriseerd hacken onder artikel 550bis van het Strafwetboek (computervredebreuk). De Europese NIS2-richtlijn legt daarnaast verplichtingen op aan organisaties om systemen te testen. "Ik wilde alleen kijken of het kon" is geen juridisch verweer. Zonder schriftelijke toestemming is elke test een misdrijf, ook als je niets kapotmaakt en niets steelt.

In deze les werk je uitsluitend op je eigen lokale ShopWave-omgeving.

**Minicontrole:** een student test de inlogpagina van een webshop van een lokaal bedrijf om te oefenen. Hij verandert niets en steelt niets. Is dit legaal? Leg uit.

---

## 2. De drie rollen in security

Professionele securityteams werken vaak met drie rollen:

| Rol | Naam | Wat ze doen |
|-----|------|-------------|
| Aanvaller | Red Team | Probeert het systeem te compromitteren via realistische aanvallen |
| Verdediger | Blue Team | Bouwt en bewaakt de beveiliging, detecteert aanvallen |
| Beide | Purple Team | Werken samen: Red Team deelt technieken, Blue Team past maatregelen aan en verifieert |

In de lessen tot nu toe was je Blue Team: je bouwde de verdediging. In deze les ben je Red Team: je probeert je eigen verdediging te breken.

**Minicontrole:** wat is het voordeel van een Purple Team tegenover aparte Red en Blue Teams?

---

## 3. De vijf fasen van een pentest

Een professionele pentest volgt een vaste methodologie. Zonder structuur mis je kwetsbaarheden.

**Fase 1: Verkenning**

Informatie verzamelen over het doelwit zonder het systeem aan te vallen. Welke endpoints bestaan er? Welk framework wordt gebruikt? Welke versie? Welke foutmeldingen geeft het systeem?

**Fase 2: Scannen en enumereren**

Actief de aanvalsoppervlakte in kaart brengen. Poorten, endpoints, HTTP-methodes, responsen op ongeldige input testen.

**Fase 3: Exploitatie**

Kwetsbaarheden misbruiken om toegang te krijgen of data te lekken. Dit is de "aanvalsfase".

**Fase 4: Post-exploitatie**

Begrijpen wat een aanvaller met de verkregen toegang kan doen. Kan hij andere systemen bereiken? Welke data is beschikbaar? Hoe lang blijft hij onopgemerkt?

**Fase 5: Rapporteren**

Bevindingen documenteren met bewijs, risicoclassificatie en concrete aanbevelingen. Zonder rapport heeft een pentest geen waarde voor de organisatie.

**Minicontrole:** een pentester vindt een SQL Injection-kwetsbaarheid maar besluit ze niet te exploiteren om schade te vermijden. Hij rapporteert ze wel. Is zijn pentest professioneel uitgevoerd? Leg uit.

---

## 4. Tools

**curl** is een commandoregelgereedschap voor HTTP-requests. Het is ingebouwd in Windows 11, macOS en Linux. In deze les gebruik je de PowerShell-versie.

```powershell
# Basis GET-request naar ShopWave
curl.exe -k https://localhost:5001/

# POST-request met JSON-body
curl.exe -k -X POST https://localhost:5001/login `
  -H "Content-Type: application/json" `
  -d '{"email":"alice@shopwave.be","password":"wachtwoord123"}'

# Request met Authorization-header
curl.exe -k https://localhost:5001/orders/alice@shopwave.be `
  -H "Authorization: Bearer eyJhbGci..."
```

De `-k` vlag slaat certificaatvalidatie over voor self-signed certificaten. Gebruik dit enkel lokaal in development.

**OWASP ZAP** (Zed Attack Proxy) is een gratis, open-source scanner die automatisch kwetsbaarheden opspoort in webapplicaties. Je configureert een doel-URL en ZAP stuurt honderden geautomatiseerde aanvalsverzoeken. Het rapport toont kwetsbaarheden per risiconiveau.

Download via: `https://www.zaproxy.org/download/`

**Burp Suite Community Edition** is een intercepting proxy. Je stuurt al je HTTP-verkeer via Burp, die elk request onderschept en toont voordat het de server bereikt. Zo kan je requests aanpassen en opnieuw sturen.

Download via: `https://portswigger.net/burp/communitydownload`

**Minicontrole:** wat is het verschil tussen OWASP ZAP en Burp Suite? Wanneer gebruik je welke?

---

## 5. JWT-aanvallen

### Rolmanipulatie

In les 7 leerde je dat de JWT-payload Base64-gecodeerd is, niet versleuteld. Iedereen die een token heeft, kan de payload lezen. Maar kan een aanvaller de payload ook aanpassen?

De aanval:

1. Aanvaller onderschept een geldig token van `alice@shopwave.be` met `"role": "user"`.
2. Hij decodeert de payload (Base64), verandert `"role"` naar `"admin"` en hercoddeert.
3. Hij plakt de aangepaste payload in het token en stuurt het naar `/admin/orders`.

**Wat er mislukt:** de signature is berekend over de originele header en payload. Na het aanpassen van de payload klopt de signature niet meer. De server berekent de signature opnieuw en vergelijkt. Ze komen niet overeen. Het token wordt geweigerd met `401 Unauthorized`.

De aanvaller kan de signature niet opnieuw berekenen omdat hij de geheime sleutel niet heeft.

### De `alg:none`-aanval

Een klassieke JWT-aanval is het veranderen van het algoritme in de header naar `"none"`:

```json
{
  "alg": "none",
  "typ": "JWT"
}
```

Bij `alg: none` is er geen signature. Sommige slecht geconfigureerde bibliotheken accepteren zo'n token zonder validatie: "het algoritme is `none`, er is geen signature te controleren, het token is geldig."

.NET weigert tokens met `alg: none` standaard. De `JwtBearerAuthentication`-middleware accepteert enkel het algoritme dat je expliciet configureert via `TokenValidationParameters`. Een token met een ander algoritme wordt onmiddellijk geweigerd.

**Minicontrole:** een aanvaller stuurt een token met `"alg": "none"` en een payload `"role": "admin"`. De ShopWave API geeft `401 Unauthorized`. Welke beveiligingsmaatregel blokkeert de aanval?

---

## 6. Het pentestreport

Een pentest zonder rapport is waardeloos voor de organisatie. Het rapport is het enige tastbare resultaat dat de opdrachtgever ontvangt.

Een professioneel rapport bevat per bevinding:

| Veld | Inhoud |
|------|--------|
| ID | Unieke identifier, bv. `FINDING-01` |
| Titel | Korte beschrijving van de kwetsbaarheid |
| Risico | Informational, Low, Medium, High of Critical |
| CVSS-score | Numerieke risicoscore van 0.0 tot 10.0 |
| Beschrijving | Wat is het probleem en hoe werkt het? |
| Bewijs | HTTP-request/response, screenshot of log-uitvoer |
| Aanbeveling | Concrete technische maatregel |
| Status | Open of Gesloten |

**Risicoclassificatie** combineert twee factoren:

```csharp
              | Lage impact | Hoge impact
Waarschijnlijk|   Medium    |    High
Onwaarschijnlijk|  Low      |   Medium
```

Een SQL Injection op een publiek zoekendpoint scoort hoog op beide assen: het is eenvoudig uit te voeren (waarschijnlijk) en geeft toegang tot alle klantdata (hoge impact). Risico: High.

**Minicontrole:** een aanvaller kan via de Swagger-UI alle endpoints zien, maar heeft geen toegang tot de data zonder geldig token. Hoe classificeer je deze bevinding op waarschijnlijkheid en impact?

---

## 7. DevSecOps

**DevSecOps** integreert security in elk stadium van het softwareontwikkelingsproces. In plaats van security te controleren als laatste stap voor een release, bouw je het in van bij het eerste commit.

```csharp
Commit --> Build --> Test --> Security Scan --> Package --> Deploy
                      |            |
               Unit tests     SAST: code-analyse
               Integration    dotnet list --vulnerable
               Acceptatie     DAST: OWASP ZAP op draaiende app
```

- **SAST** (Static Application Security Testing): analyseert broncode zonder de applicatie uit te voeren. Vindt hardcoded geheimen, onveilige API-aanroepen en bekende patronen van kwetsbaarheden.
- **DAST** (Dynamic Application Security Testing): test de draaiende applicatie van buitenaf, zoals OWASP ZAP. Vindt kwetsbaarheden die pas zichtbaar zijn tijdens uitvoering.

Als developer op stage of in je eerste job kom je DevSecOps tegen in de CI/CD-pipeline van het bedrijf. Een pipeline die `dotnet list package --vulnerable` uitvoert, blokkeert automatisch een build als een NuGet-package een bekende kwetsbaarheid heeft.

**Minicontrole:** een SAST-tool vindt een hardcoded wachtwoord in de broncode. Een DAST-tool vindt een SQL Injection in een draaiende applicatie. Welke tool vindt elk type probleem en waarom?

---

## 8. Demo: pentest op ShopWave

Start de ShopWave API via `dotnet run` in het project `ShopWave.Api`. De API draait op `https://localhost:5001`. Open daarna een tweede terminal voor de curl-commando's.

---

### Stap 8a: Verkenning - endpoints in kaart brengen

Voer de volgende requests uit en noteer de responses:

```powershell
curl.exe -k https://localhost:5001/
curl.exe -k https://localhost:5001/health
curl.exe -k https://localhost:5001/swagger
curl.exe -k https://localhost:5001/bestaaniet
```

**Wat je ziet:** het rootendpoint antwoordt met de welkomsttekst. `/swagger` geeft mogelijk een UI of een 404. `/bestaaniet` geeft een 404. Noteer alle statuscode: die vertellen je welke endpoints bestaan en welke niet.

---

### Stap 8b: JWT-token ophalen

Log in als Alice en sla het token op. Doe dit in twee stappen: eerst login, dan verify met de 2FA-code die in de API-console verschijnt.

```powershell
# Stap 1: login sturen
curl.exe -k -X POST https://localhost:5001/login `
  -H "Content-Type: application/json" `
  -d '{"email":"alice@shopwave.be","password":"wachtwoord123"}'
```

**Wat je ziet in de API-console:** de 2FA-code verschijnt.

```powershell
# Stap 2: verify met de 2FA-code (vervang 483920 door de echte code)
curl.exe -k -X POST https://localhost:5001/verify `
  -H "Content-Type: application/json" `
  -d '{"email":"alice@shopwave.be","code":"483920"}'
```

**Wat je ziet:**

```json
{"token":"eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJhbGljZUBzaG9wd2F2ZS5iZSIsInJvbGUiOiJ1c2VyIn0.xyz"}
```

---

### Stap 8c: JWT-payload decoderen

Splits het token op de punten. Neem het middelste deel (de payload) en decodeer het via PowerShell:

```powershell
$token   = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJhbGljZUBzaG9wd2F2ZS5iZSIsInJvbGUiOiJ1c2VyIn0.xyz"
$parts   = $token.Split(".")
$payload = $parts[1]

# Base64url naar Base64
$payload = $payload.Replace("-", "+").Replace("_", "/")
$padding = 4 - ($payload.Length % 4)
if ($padding -ne 4) { $payload += "=" * $padding }

$decoded = [System.Text.Encoding]::UTF8.GetString([System.Convert]::FromBase64String($payload))
Write-Output $decoded
```

**Wat je ziet:**

```json
{"sub":"alice@shopwave.be","role":"user","iat":1715996400,"exp":1715998200}
```

---

### Stap 8d: Rolmanipulatie - methode aanmaken

Voeg bovenaan `ShopWave/Program.cs` de using toe en maak de methode aan:

```csharp
using System.Text;

void TryRoleManipulation(string validToken)
{
    Console.WriteLine("=== JWT-rolmanipulatie poging ===");

    string[] parts   = validToken.Split('.');
    string   payload = parts[1];
}
```

**Wat je ziet:** nog niets. De methode bestaat maar doet nog niets. Je bouwt hem stap voor stap op.

---

### Stap 8e: Rolmanipulatie - payload decoderen

Voeg de decodering toe aan `TryRoleManipulation`:

```csharp
// Padding herstellen voor Base64-decodering
int padLength = 4 - (payload.Length % 4);
if (padLength != 4)
{
    payload += new string('=', padLength);
}

// Base64url -> Base64
payload = payload.Replace('-', '+').Replace('_', '/');

string decodedPayload = Encoding.UTF8.GetString(Convert.FromBase64String(payload));
Console.WriteLine($"Originele payload: {decodedPayload}");
```

**Wat je ziet:**

```csharp
Originele payload: {"sub":"alice@shopwave.be","role":"user","iat":1715996400}
```

De payload is leesbaar. Je ziet de role-claim staat op `"user"`.

---

### Stap 8f: Rolmanipulatie - payload aanpassen en hercoderen

Verander de rol en codeer de payload terug naar Base64url:

```csharp
// Payload aanpassen: user -> admin
string manipulatedPayload = decodedPayload.Replace("\"role\":\"user\"", "\"role\":\"admin\"");
Console.WriteLine($"Aangepaste payload: {manipulatedPayload}");

// Hercoderen naar Base64url
byte[] manipulatedBytes = Encoding.UTF8.GetBytes(manipulatedPayload);
string reEncodedPayload = Convert.ToBase64String(manipulatedBytes)
    .Replace('+', '-').Replace('/', '_').TrimEnd('=');

// Token samenstellen met originele header en signature maar aangepaste payload
string manipulatedToken = $"{parts[0]}.{reEncodedPayload}.{parts[2]}";
Console.WriteLine($"Gemanipuleerd token (begin): {manipulatedToken[..60]}...");
```

**Wat je ziet:**

```csharp
Aangepaste payload: {"sub":"alice@shopwave.be","role":"admin","iat":1715996400}
Gemanipuleerd token (begin): eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOi...
```

Je hebt nu een token met een aangepaste payload maar de originele signature.

---

### Stap 8g: Rolmanipulatie - token sturen en resultaat zien

Stuur het gemanipuleerde token naar het admin-endpoint:

```csharp
HttpClientHandler handler = new HttpClientHandler();
handler.ServerCertificateCustomValidationCallback =
    (message, certificate, chain, errors) => true;

HttpClient client = new HttpClient(handler);
client.BaseAddress = new Uri("https://localhost:5001");
client.DefaultRequestHeaders.Authorization =
    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", manipulatedToken);

HttpResponseMessage response = client.GetAsync("/admin/orders").Result;
Console.WriteLine($"Resultaat met gemanipuleerd token: {response.StatusCode}");
Console.WriteLine("Verwacht: Unauthorized (signature klopt niet meer)");

client.Dispose();
handler.Dispose();
```

Roep de methode aan in `Main`:

```csharp
// Haal eerst een geldig token op via het verify-endpoint
TryRoleManipulation(aliceToken);
```

**Wat je ziet:**

```csharp
=== JWT-rolmanipulatie poging ===
Originele payload: {"sub":"alice@shopwave.be","role":"user","iat":1715996400}
Aangepaste payload: {"sub":"alice@shopwave.be","role":"admin","iat":1715996400}
Gemanipuleerd token (begin): eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOi...
Resultaat met gemanipuleerd token: Unauthorized
Verwacht: Unauthorized (signature klopt niet meer)
```

De aanval mislukt. De server berekent de signature opnieuw over de aangepaste payload. Die komt niet overeen met de originele signature. Het token wordt geweigerd.

---

### Stap 8h: `alg:none` - header en payload coderen

Voeg een nieuwe methode `TryAlgNoneAttack()` toe. Begin met de header en payload:

```csharp
void TryAlgNoneAttack()
{
    Console.WriteLine("=== alg:none aanval ===");

    // Header met alg:none coderen naar Base64url
    string header = Convert.ToBase64String(
        Encoding.UTF8.GetBytes("{\"alg\":\"none\",\"typ\":\"JWT\"}"))
        .Replace('+', '-').Replace('/', '_').TrimEnd('=');

    // Payload met admin-rol coderen naar Base64url
    string payload = Convert.ToBase64String(
        Encoding.UTF8.GetBytes("{\"sub\":\"admin@shopwave.be\",\"role\":\"admin\"}"))
        .Replace('+', '-').Replace('/', '_').TrimEnd('=');

    Console.WriteLine($"Header:  {header}");
    Console.WriteLine($"Payload: {payload}");
}
```

**Wat je ziet:**

```csharp
Header:  eyJhbGciOiJub25lIiwidHlwIjoiSldUIn0
Payload: eyJzdWIiOiJhZG1pbkBzaG9wd2F2ZS5iZSIsInJvbGUiOiJhZG1pbiJ9
```

Twee Base64url-blokken. Er is nog geen signature.

---

### Stap 8i: `alg:none` - token sturen zonder signature

Bouw het token met een lege signature-sectie en stuur het:

```csharp
// Geen signature: de derde sectie is leeg, maar de punt blijft
string noneToken = $"{header}.{payload}.";

HttpClientHandler handler = new HttpClientHandler();
handler.ServerCertificateCustomValidationCallback =
    (message, certificate, chain, errors) => true;

HttpClient client = new HttpClient(handler);
client.BaseAddress = new Uri("https://localhost:5001");
client.DefaultRequestHeaders.Authorization =
    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", noneToken);

HttpResponseMessage response = client.GetAsync("/admin/orders").Result;
Console.WriteLine($"Resultaat alg:none token: {response.StatusCode}");
Console.WriteLine("Verwacht: Unauthorized (.NET weigert alg:none standaard)");

client.Dispose();
handler.Dispose();
```

**Wat je ziet:**

```csharp
=== alg:none aanval ===
Header:  eyJhbGciOiJub25lIiwidHlwIjoiSldUIn0
Payload: eyJzdWIiOiJhZG1pbkBzaG9wd2F2ZS5iZSIsInJvbGUiOiJhZG1pbiJ9
Resultaat alg:none token: Unauthorized
Verwacht: Unauthorized (.NET weigert alg:none standaard)
```

De .NET-middleware vergelijkt het algoritme in de header met de geconfigureerde lijst. `none` staat daar niet in. Het token wordt onmiddellijk geweigerd.

---

### Stap 8j: Brute-force test op het login-endpoint

Test of rate limiting werkt door zes loginpogingen na elkaar te sturen:

```powershell
for ($i = 1; $i -le 6; $i++) {
    $body     = "{`"email`":`"alice@shopwave.be`",`"password`":`"fout$i`"}"
    $response = curl.exe -k -s -o NUL -w "%{http_code}" `
        -X POST https://localhost:5001/login `
        -H "Content-Type: application/json" `
        -d $body
    Write-Output "Poging $i : $response"
}
```

**Wat je ziet als rate limiting actief is:**

```csharp
Poging 1 : 200
Poging 2 : 200
Poging 3 : 200
Poging 4 : 200
Poging 5 : 200
Poging 6 : 429
```

Als poging 6 ook `200` of `401` geeft, is rate limiting niet geactiveerd. Noteer dit als bevinding in je rapport.

---

### Stap 8k: SQL Injection verificatie

Verifieer dat de SQL Injection-fix uit les 9 effectief werkt:

```powershell
# Normale zoekopdracht
curl.exe -k "https://localhost:5001/orders/zoek?email=alice@shopwave.be"

# Injectie-poging
curl.exe -k "https://localhost:5001/orders/zoek?email=' OR '1'='1"

# DROP-poging
curl.exe -k "https://localhost:5001/orders/zoek?email='; DROP TABLE orders --"
```

**Wat je ziet bij een correcte fix:** de injectiepogingen geven een lege lijst terug, geen fout en geen gelekte data. Noteer als geslaagde beveiligingstest.

---

### Stap 8l: CORS-headers inspecteren

Stuur een request met een kwaadaardige `Origin`-header en inspecteer de response:

```powershell
curl.exe -k -v -H "Origin: https://aanvaller.be" https://localhost:5001/ 2>&1 | Select-String "Access-Control"
```

**Wat je ziet bij correcte CORS-configuratie:** geen `Access-Control-Allow-Origin: *` in de response. Als de header ontbreekt of de aanvallersorigin niet bevat, is CORS correct ingesteld. Noteer als geslaagde beveiligingstest.

---

### Stap 8m: Informatielekkage testen

Test of foutmeldingen interne informatie lekken:

```powershell
# Crashendpoint aanroepen
curl.exe -k https://localhost:5001/crash

# Ongeldige JSON sturen
curl.exe -k -X POST https://localhost:5001/login `
  -H "Content-Type: application/json" `
  -d "GEEN_GELDIGE_JSON"
```

**Wat je ziet in development:** de volledige stack trace met bestandspaden en interne details.

**Wat je ziet in productie (`ASPNETCORE_ENVIRONMENT=Production`):** enkel `Er is een fout opgetreden.` Geen stack trace, geen interne paden. Als je in development werkt en de Developer Exception Page actief is, noteer je dit als potentieel risico voor productie.

---

## 9. Samenvatting

| Concept | Wat je moet onthouden |
|---------|-----------------------|
| Ethisch hacken | Gecontroleerd aanvallen met toestemming. Nooit zonder. |
| Wettelijk kader | Art. 550bis Strafwetboek (BE) en NIS2. Ongeautoriseerd hacken is strafbaar. |
| Pentesting-fasen | Verkenning, scannen, exploitatie, post-exploitatie, rapportering |
| Red Team | Aanvaller. Probeert het systeem te compromitteren. |
| Blue Team | Verdediger. Bouwt en bewaakt de beveiliging. |
| JWT-rolmanipulatie | Mislukt omdat de signature niet meer klopt na aanpassing van de payload |
| `alg:none`-aanval | Geblokkeerd door .NET-middleware die enkel geconfigureerde algoritmen accepteert |
| Brute-force test | Verifieert dat rate limiting (`429 Too Many Requests`) werkt na 5 pogingen |
| CORS-inspect | `Access-Control-Allow-Origin: *` is een bevinding. Specifieke origins zijn correct. |
| Pentestreport | Bevindingen documenteren met ID, risico, bewijs en aanbeveling |
| CVSS-score | Numerieke risicoscore van 0.0 tot 10.0 |
| SAST | Analyseert broncode statisch. Vindt hardcoded geheimen en onveilige patronen. |
| DAST | Test de draaiende applicatie. Vindt kwetsbaarheden in gedrag. |
| DevSecOps | Security ingebakken in elke fase van het ontwikkelproces |
