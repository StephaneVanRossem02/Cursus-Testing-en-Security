---
title: "Les 7: Theorie - JWT en OAuth2"
sidebar_label: "Theorie"
---

# Les 7: Theorie - JWT en OAuth2

## 1. Waarom sessies niet schalen

De klassieke aanpak voor authenticatie werkt met sessies:

1. De gebruiker logt in.
2. De server maakt een sessie aan en slaat die op in het geheugen.
3. De server stuurt een sessie-ID terug als cookie.
4. Bij elke volgende request zoekt de server die sessie op via het ID.

Dat werkt goed zolang er maar één server is. Zodra je meerdere servers gebruikt, loopt het mis. Server A kent de sessie niet die server B aanmaakte. De gebruiker is ingelogd op server A, maar als zijn volgende request op server B terechtkomt, ziet die geen sessie en weigert de toegang.

Oplossingen bestaan (gedeelde sessie-opslag, sticky sessions), maar ze voegen complexiteit en een centraal storingspunt toe.

**Minicontrole:** waarom werkt sessiegebaseerde authenticatie slecht bij meerdere servers?

---

## 2. Tokengebaseerde authenticatie

Tokengebaseerde authenticatie lost het schaalprobleem op. Het token bevat alle nodige informatie al in zich. De server hoeft niets op te slaan.

1. De gebruiker logt in.
2. De server maakt een token aan met alle nodige informatie erin.
3. De client slaat het token op (in geheugen of local storage).
4. Bij elke request stuurt de client het token mee in de `Authorization`-header.
5. Elke server kan het token zelf valideren zonder een centrale opslag te raadplegen.

Een server die het token ontvangt, controleert de geldigheid volledig lokaal. Geen database-opzoeking, geen gedeeld geheugen nodig.

**Minicontrole:** wat zit er in een token dat een server toelaat het te valideren zonder externe opslag?

---

## 3. JWT-opbouw stap voor stap

Een **JSON Web Token** bestaat uit drie delen, gescheiden door punten:

```
header.payload.signature
```

**Stap 1: de header**

De header beschrijft het tokentype en het gebruikte algoritme:

```json
{
  "alg": "HS256",
  "typ": "JWT"
}
```

`HS256` staat voor HMACSHA256: een symmetrisch algoritme waarbij dezelfde sleutel gebruikt wordt voor ondertekenen en valideren.

**Stap 2: de payload**

De payload bevat de claims: gegevens over de gebruiker en het token zelf.

```json
{
  "sub": "alice@shopwave.be",
  "role": "user",
  "iss": "shopwave-api",
  "aud": "shopwave-client",
  "exp": 1716000000,
  "iat": 1715996400
}
```

| Claim | Naam | Betekenis |
|-------|------|-----------|
| `sub` | Subject | De gebruiker (e-mail of ID) |
| `exp` | Expiration | Vervaltijd als Unix-timestamp |
| `iat` | Issued At | Tijdstip van aanmaak |
| `iss` | Issuer | Wie het token uitschreef |
| `aud` | Audience | Voor wie het token bedoeld is |
| `role` | (eigen claim) | Rol van de gebruiker in de applicatie |

**Stap 3: de signature**

De signature wordt berekend als:

```
HMACSHA256(
  base64url(header) + "." + base64url(payload),
  secretKey
)
```

De server berekent de signature opnieuw bij elke inkomende request en vergelijkt die met de signature in het token. Als ze niet overeenkomen, is het token gemanipuleerd of nep.

**Minicontrole:** welk deel van een JWT garandeert dat de payload niet gewijzigd is?

---

## 4. JWT is geen encryptie

JWT ziet er onleesbaar uit, maar is **niet versleuteld**. De header en payload zijn Base64url-gecodeerd. Codering is geen versleuteling: iedereen kan het decoderen zonder sleutel.

```
eyJzdWIiOiJhbGljZUBzaG9wd2F2ZS5iZSIsInJvbGUiOiJ1c2VyIn0
```

Decoderen geeft:

```json
{"sub":"alice@shopwave.be","role":"user"}
```

De veiligheid van JWT zit in de **signature**, niet in de leesbaarheid van de payload. De signature maakt het onmogelijk om het token te vervalsen of aan te passen. Maar de payload is leesbaar voor iedereen die het token heeft.

Stop nooit gevoelige informatie in een JWT-payload: geen wachtwoorden, geen creditcardnummers, geen BSN-nummers.

**Minicontrole:** stel dat je het e-mailadres in de payload van een JWT aanpast. Wat gebeurt er als de server het token valideert?

---

## 5. De JWT-flow in ShopWave

```
ShopWave console          ShopWave.Api
       |                       |
       |-- POST /login ------> |  stap 1: login met e-mail en wachtwoord
       |<- { status: ... } --- |  stap 2: 2FA-code verschijnt in API-console
       |                       |
       |-- POST /verify -----> |  stap 3: 2FA-code bevestigen
       |<- { token: "eyJ..." } |  stap 4: JWT-token ontvangen bij succes
       |                       |
       |-- GET /orders/alice   |  stap 5: beveiligd endpoint aanroepen
       |   Authorization:      |          met token in de header
       |   Bearer eyJ...       |
       |<- 200 OK + data ----- |  stap 6: server valideert token, geeft data terug
       |                       |
       |-- GET /orders/alice   |  stap 7: zelfde endpoint, maar zonder token
       |   (geen header)       |
       |<- 401 Unauthorized -- |  stap 8: geweigerd
```

De `Authorization: Bearer`-header is de standaardmanier om een JWT mee te sturen. `Bearer` betekent letterlijk "drager": wie het token draagt, heeft toegang.

**Minicontrole:** in welke stap beslist de server of een request toegestaan is?

---

## 6. Rolgebaseerde autorisatie

Authenticatie beantwoordt de vraag: wie ben je? Autorisatie beantwoordt de vraag: wat mag je?

In ShopWave krijgt een gewone klant de rol `"user"`. Een medewerker krijgt de rol `"admin"`. Die rol wordt opgeslagen in de JWT-payload als een claim.

```json
{
  "sub": "alice@shopwave.be",
  "role": "user"
}
```

```json
{
  "sub": "admin@shopwave.be",
  "role": "admin"
}
```

In de Minimal API bescherm je een endpoint tegen niet-admins met:

```csharp
app.MapGet("/admin/orders", HandleAdminOrders)
   .RequireAuthorization(policy => policy.RequireRole("admin"));
```

Als een klant met rol `"user"` dat endpoint aanroept, geeft de server `403 Forbidden` terug. Dat is anders dan `401 Unauthorized`. `401` betekent: geen geldig token. `403` betekent: geldig token, maar onvoldoende rechten.

**Minicontrole:** wat is het verschil tussen `401 Unauthorized` en `403 Forbidden`?

---

## 7. OAuth 2.0

**JWT** is een tokenformaat. **OAuth 2.0** is een autorisatieprotocol. Ze zijn verwant maar niet hetzelfde.

OAuth 2.0 lost een specifiek probleem op: hoe geef je een derde applicatie toegang tot jouw gegevens zonder je wachtwoord te delen?

Voorbeeld: een fitness-app wil trainingen plannen in je Google Agenda. Via OAuth log je in bij Google zelf. De fitness-app ziet jouw wachtwoord nooit. Google geeft de app een token met beperkte rechten.

**De vier rollen:**

| Rol | Voorbeeld |
|-----|-----------|
| Resource Owner | Jij als gebruiker |
| Client | De fitness-app |
| Authorization Server | Google (login en toestemming) |
| Resource Server | Google Calendar API |

**De Authorization Code Flow:**

1. Je klikt op "Inloggen met Google" in de fitness-app.
2. De app stuurt je door naar de Google-loginpagina (met `client_id` en `redirect_uri`).
3. Je logt in bij Google en geeft toestemming aan de fitness-app.
4. Google stuurt een `authorization code` terug naar de app via de `redirect_uri`.
5. De app wisselt die code in voor een access token (server-to-server, verborgen voor de browser).
6. De app gebruikt het access token om de Google Calendar API te benaderen.

**Scopes** bepalen precies welke toegang de app krijgt. `calendar.events.write` geeft schrijftoegang tot agenda-items. `calendar.readonly` geeft enkel leestoegang. De gebruiker ziet precies welke scopes een app aanvraagt en kan weigeren.

**OAuth 2.0 versus JWT:**

| | OAuth 2.0 | JWT |
|-|-----------|-----|
| Wat is het? | Protocol (regels voor toegangsdelegatie) | Tokenformaat (hoe ziet een token eruit) |
| Beantwoordt | Wie mag wat doen? | Hoe codeer je die beslissing? |
| Gebruikt JWT? | Vaak wel, maar niet verplicht | Onafhankelijk van OAuth |

OAuth 2.0-servers geven vaak een JWT terug als access token. Maar JWT kan ook gebruikt worden zonder OAuth 2.0, zoals in de ShopWave-demo.

**Minicontrole:** waarom ziet de fitness-app het Google-wachtwoord van de gebruiker nooit?

---

## 8. Demo: JWT toevoegen aan ShopWave

Je bouwt verder op de ShopWave.Api uit les 6. Geen nieuw project. De NuGet packages staan vermeld op de overzichtspagina.

---

### Stap 8a: JwtTokenService aanmaken - klasse en constructor

Maak een nieuw bestand aan: `ShopWave.Api/JwtTokenService.cs`.

Voeg de klasse aan met de velden en constructor:

```csharp
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace ShopWave.Api
{
    public class JwtTokenService
    {
        private readonly string _secretKey;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly int    _expiresMinutes;

        public JwtTokenService(string secretKey, string issuer, string audience, int expiresMinutes = 30)
        {
            _secretKey      = secretKey;
            _issuer         = issuer;
            _audience       = audience;
            _expiresMinutes = expiresMinutes;
        }
    }
}
```

De klasse ontvangt de geheime sleutel, de issuer, de audience en de vervaltijd via de constructor. Zo kan je dezelfde klasse hergebruiken met andere configuraties.

**Wat je ziet:** het project compileert. De klasse bestaat maar heeft nog geen methoden.

---

### Stap 8b: GenerateToken - sleutel en signing credentials

Voeg de methode toe **binnen de klasse**, na de constructor. Begin met de eerste drie regels:

```csharp
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

public string GenerateToken(string email, string role)
{
    byte[]               keyBytes    = Encoding.UTF8.GetBytes(_secretKey);
    SymmetricSecurityKey securityKey = new SymmetricSecurityKey(keyBytes);
    SigningCredentials   credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

    // wordt verder uitgebouwd in de volgende stap
    return string.Empty;
}
```

`Encoding.UTF8.GetBytes` zet de geheime sleutelstring om naar bytes. `SymmetricSecurityKey` verpakt die bytes in een sleutelobject dat de JWT-bibliotheek begrijpt. `SigningCredentials` koppelt die sleutel aan het HMACSHA256-algoritme. Dat algoritme berekent straks de signature.

**Wat je ziet:** het project compileert. De methode bestaat maar geeft nog een lege string terug.

---

### Stap 8c: GenerateToken - claims definiëren

Voeg de claims toe, direct na de `credentials`-regel en voor de `return`:

```csharp
    List<Claim> claims = new List<Claim>
    {
        new Claim(JwtRegisteredClaimNames.Sub, email),
        new Claim(ClaimTypes.Role, role),
        new Claim(JwtRegisteredClaimNames.Iat,
            DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString())
    };
```

Claims zijn de gegevens die in de payload komen. `Sub` is het e-mailadres van de gebruiker. `Role` bepaalt wat de gebruiker mag doen. `Iat` is het tijdstip van aanmaak als Unix-timestamp.

**Wat je ziet:** het project compileert. De lijst met claims staat klaar maar wordt nog nergens gebruikt.

---

### Stap 8d: GenerateToken - token assembleren en teruggeven

Vervang de tijdelijke `return string.Empty;` door de volgende code:

```csharp
    JwtSecurityToken token = new JwtSecurityToken(
        issuer:             _issuer,
        audience:           _audience,
        claims:             claims,
        expires:            DateTime.UtcNow.AddMinutes(_expiresMinutes),
        signingCredentials: credentials
    );

    return new JwtSecurityTokenHandler().WriteToken(token);
```

`JwtSecurityToken` assembleert header, payload en signature. `WriteToken` serialiseert het resultaat naar de `header.payload.signature`-string die de client ontvangt.

**Wat je ziet:** het project compileert. Je kan `jwtTokenService.GenerateToken("alice@shopwave.be", "user")` aanroepen en krijgt een lange JWT-string terug die begint met `eyJ`.

---

### Stap 8e: Program.cs - usings en de geheime sleutel via een omgevingsvariabele

Open `ShopWave.Api/Program.cs`. Voeg bovenaan de nodige usings toe:

```csharp
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using ShopWave.Api;
using ShopWave.Security;
using System.Security.Cryptography.X509Certificates;
```

De JWT-sleutel mag nooit hardcoded in de broncode staan. Een sleutel in de repository kan door iedereen met toegang tot de code gebruikt worden om geldige tokens te maken. Zelfs als je de sleutel later verwijdert, blijft hij zichtbaar in de git-geschiedenis.

De eenvoudigste oplossing is een **omgevingsvariabele**. Stel die in voor je de applicatie opstart. In PowerShell:

```powershell
$env:JWT_SECRET_KEY = "ShopWaveGeheimeSleutel2024!!XYZ#"
```

Deze variabele bestaat enkel in de huidige terminalsessie. Ze staat nooit in een bestand, dus ze kan ook nooit per ongeluk ingecheckt worden.

In productie stel je omgevingsvariabelen in op de server zelf, of je gebruikt een geheimenbeheerder zoals **Azure Key Vault**. De applicatiecode verandert niet: die leest altijd `JWT_SECRET_KEY` op, ongeacht waar de waarde vandaan komt.

Lees de sleutel op en definieer de constanten bovenaan `Program.cs`:

```csharp
WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

string secretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY")
    ?? throw new InvalidOperationException("Omgevingsvariabele JWT_SECRET_KEY ontbreekt.");

const string Issuer   = "shopwave-api";
const string Audience = "shopwave-client";
```

**Wat je ziet:** het project compileert. Start je de applicatie zonder de omgevingsvariabele in te stellen, dan gooit ze meteen een `InvalidOperationException`. Zo kan de applicatie nooit per ongeluk draaien zonder geldige configuratie.

---

### Stap 8f: Program.cs - HTTPS en JWT-authenticatie registreren

Voeg na de constanten de HTTPS-configuratie toe, zoals in les 6:

```csharp
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(5001, listenOptions =>
    {
        X509Certificate2 certificate = CertificateHelper.CreateSelfSignedCertificate("localhost");
        listenOptions.UseHttps(certificate);
    });
});
```

Registreer daarna JWT-authenticatie als service:

```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = Issuer,
            ValidAudience            = Audience,
            IssuerSigningKey         = new SymmetricSecurityKey(
                                           Encoding.UTF8.GetBytes(secretKey))
        };
    });

builder.Services.AddAuthorization();
```

`TokenValidationParameters` vertelt de server wat hij controleert bij elk binnenkomend token: klopt de issuer, is het token voor deze API bedoeld, is het nog niet verlopen, en klopt de signature?

**Wat je ziet:** het project compileert. De middleware is geconfigureerd maar nog niet geactiveerd.

---

### Stap 8g: Program.cs - applicatie bouwen en middleware activeren

Bouw de applicatie en zet de middleware in de juiste volgorde:

```csharp
WebApplication app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();
```

De volgorde is verplicht. `UseAuthentication` bepaalt wie de gebruiker is door het token te lezen en te valideren. `UseAuthorization` beslist daarna wat die gebruiker mag doen. Als je de volgorde omdraait, weet de autorisatiemiddleware niet wie de gebruiker is.

**Wat je ziet:** het project compileert. Er zijn nog geen endpoints, maar de middleware staat klaar.

---

### Stap 8h: Program.cs - services en testaccounts aanmaken

Voeg na de middleware de service-objecten en testgebruikers toe:

```csharp
TwoFactorService  twoFactorService  = new TwoFactorService();
AccountRepository accountRepository = new AccountRepository(twoFactorService);
JwtTokenService   jwtTokenService   = new JwtTokenService(secretKey, Issuer, Audience);

accountRepository.Register("alice@shopwave.be", "wachtwoord123");
accountRepository.Register("admin@shopwave.be", "admin123");
```

**Wat je ziet:** het project compileert. De services zijn aangemaakt maar er zijn nog geen endpoints.

---

### Stap 8i: Program.cs - publiek endpoint en login-endpoint

Voeg het publieke endpoint en het login-endpoint toe:

```csharp
app.MapGet("/", () => "ShopWave API actief op HTTPS met JWT");

app.MapPost("/login", HandleLogin);

IResult HandleLogin(LoginRequest request)
{
    string result = accountRepository.Login(request.Email, request.Password);
    return Results.Ok(new { Status = result });
}

record LoginRequest(string Email, string Password);
```

Het `/`-endpoint vereist geen token. Iedereen kan het aanroepen. Het `/login`-endpoint start de 2FA-flow: de gebruiker geeft e-mail en wachtwoord, de API stuurt een 2FA-code naar de console.

**Wat je ziet:** start de API en stuur een POST naar `/login` met `{"email":"alice@shopwave.be","password":"wachtwoord123"}`. De 2FA-code verschijnt in de API-console.

---

### Stap 8j: Program.cs - verify-endpoint met rolbepaling

Voeg het verify-endpoint toe dat bij succes een JWT teruggeeft:

```csharp
app.MapPost("/verify", HandleVerify);

IResult HandleVerify(VerifyRequest request)
{
    string result = accountRepository.VerifyTwoFactor(request.Email, request.Code);

    if (result != "Inloggen geslaagd.")
    {
        return Results.Unauthorized();
    }

    string role  = DetermineRole(request.Email);
    string token = jwtTokenService.GenerateToken(request.Email, role);

    return Results.Ok(new { Token = token });
}

string DetermineRole(string email)
{
    string role;

    if (email == "admin@shopwave.be")
    {
        role = "admin";
    }
    else
    {
        role = "user";
    }

    return role;
}

record VerifyRequest(string Email, string Code);
```

`DetermineRole` bepaalt de rol op basis van het e-mailadres. Die rol komt in de JWT-payload als claim. Elke server die het token valideert, kan de rol uitlezen zonder een database te raadplegen.

**Wat je ziet:** stuur een POST naar `/verify` met de code uit de vorige stap. Je krijgt een JSON-object terug met een `token`-veld dat begint met `eyJ`.

---

### Stap 8k: Program.cs - orders-endpoint en admin-endpoint

Voeg de beveiligde endpoints toe en sluit de applicatie af:

```csharp
app.MapGet("/orders/{email}", HandleOrders)
   .RequireAuthorization();

IResult HandleOrders(string email)
{
    X509Certificate2 certificate = CertificateHelper.CreateSelfSignedCertificate("ShopWave");
    OrderSigner      signer      = new OrderSigner(certificate);
    string           orderData   = $"{email} | Laptop | 999.99 EUR";
    string           signature   = signer.Sign(orderData);

    return Results.Ok(new { Order = orderData, Signature = signature });
}

app.MapGet("/admin/orders", HandleAdminOrders)
   .RequireAuthorization(policy => policy.RequireRole("admin"));

IResult HandleAdminOrders()
{
    return Results.Ok(new { Message = "Alle bestellingen - enkel voor admins" });
}

app.Run();
```

`.RequireAuthorization()` weigert elke request zonder geldig token met `401 Unauthorized`. `.RequireAuthorization(policy => policy.RequireRole("admin"))` weigert ook geldige tokens van niet-admins met `403 Forbidden`.

**Wat je ziet:** roep `/orders/alice@shopwave.be` aan zonder token. De server geeft `401 Unauthorized`. De endpoints zijn beveiligd.

---

### Stap 8l: Console - HttpClient aanmaken en login sturen

Open `ShopWave/Program.cs`. Voeg de nodige usings toe en maak een `HttpClient` aan die self-signed certificaten accepteert:

```csharp
using System.Net.Http;
using System.Text;
using System.Text.Json;

HttpClientHandler handler = new HttpClientHandler();
handler.ServerCertificateCustomValidationCallback =
    (message, certificate, chain, errors) => true;

HttpClient client = new HttpClient(handler);
client.BaseAddress = new Uri("https://localhost:5001");
```

Stuur de loginrequest en lees de 2FA-code in:

```csharp
Console.WriteLine("=== Stap 1: Login ===");

string        loginPayload  = JsonSerializer.Serialize(new { email = "alice@shopwave.be", password = "wachtwoord123" });
StringContent loginContent  = new StringContent(loginPayload, Encoding.UTF8, "application/json");
HttpResponseMessage loginResponse = client.PostAsync("/login", loginContent).Result;

Console.WriteLine(loginResponse.Content.ReadAsStringAsync().Result);

Console.Write("Voer de 2FA-code in (staat in de API-console): ");
string twoFactorCode = Console.ReadLine() ?? string.Empty;
```

**Wat je ziet:**

```
=== Stap 1: Login ===
{"status":"2FA-code verstuurd."}
Voer de 2FA-code in (staat in de API-console):
```

De 2FA-code staat in de console van de API, niet in de console van het console-project. Wissel tussen de twee vensters.

---

### Stap 8m: Console - 2FA-code verifiëren en token ophalen

Stuur de verify-request en haal het token uit de response:

```csharp
Console.WriteLine("=== Stap 2: Verify + Token ophalen ===");

string        verifyPayload  = JsonSerializer.Serialize(new { email = "alice@shopwave.be", code = twoFactorCode });
StringContent verifyContent  = new StringContent(verifyPayload, Encoding.UTF8, "application/json");
HttpResponseMessage verifyResponse = client.PostAsync("/verify", verifyContent).Result;

string verifyBody = verifyResponse.Content.ReadAsStringAsync().Result;
Console.WriteLine(verifyBody);

JsonDocument verifyDoc = JsonDocument.Parse(verifyBody);
string token = verifyDoc.RootElement.GetProperty("token").GetString() ?? string.Empty;
```

`JsonDocument.Parse` parseert de JSON-response. `GetProperty("token")` haalt de waarde op van het `token`-veld. Dat is de JWT-string die je bij de volgende requests meestuurt.

**Wat je ziet:**

```
=== Stap 2: Verify + Token ophalen ===
{"token":"eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJhbGljZ..."}
```

---

### Stap 8n: Console - beveiligd endpoint aanroepen met en zonder token

Test het beveiligde endpoint eerst met token, daarna zonder:

```csharp
Console.WriteLine("=== Stap 3: Met token (verwacht 200 OK) ===");

client.DefaultRequestHeaders.Authorization =
    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

HttpResponseMessage ordersResponse = client.GetAsync("/orders/alice@shopwave.be").Result;
Console.WriteLine($"Status: {ordersResponse.StatusCode}");
Console.WriteLine(ordersResponse.Content.ReadAsStringAsync().Result);

Console.WriteLine("=== Stap 4: Zonder token (verwacht 401) ===");

client.DefaultRequestHeaders.Authorization = null;
HttpResponseMessage noTokenResponse = client.GetAsync("/orders/alice@shopwave.be").Result;
Console.WriteLine($"Status: {noTokenResponse.StatusCode}");
```

**Wat je ziet:**

```
=== Stap 3: Met token (verwacht 200 OK) ===
Status: OK
{"order":"alice@shopwave.be | Laptop | 999.99 EUR","signature":"..."}
=== Stap 4: Zonder token (verwacht 401) ===
Status: Unauthorized
```

---

### Stap 8o: Console - JWT-payload inspecteren zonder sleutel

Voeg bovenaan het bestand de using toe en voeg daarna de volgende code toe na stap 8n:

```csharp
using System.IdentityModel.Tokens.Jwt;
```

```csharp
Console.WriteLine("=== Stap 5: JWT-payload leesbaar zonder sleutel ===");

JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
JwtSecurityToken        parsedToken  = tokenHandler.ReadJwtToken(token);

Console.WriteLine($"Subject:  {parsedToken.Subject}");
Console.WriteLine($"Verloopt: {parsedToken.ValidTo}");

foreach (System.Security.Claims.Claim claim in parsedToken.Claims)
{
    Console.WriteLine($"  {claim.Type}: {claim.Value}");
}

handler.Dispose();
client.Dispose();
```

`ReadJwtToken` decodeert de payload zonder de signature te valideren. Iedereen die het token heeft, kan de payload lezen. Stop nooit gevoelige informatie in de payload.

**Wat je ziet:**

```
=== Stap 5: JWT-payload leesbaar zonder sleutel ===
Subject:  alice@shopwave.be
Verloopt: 15/05/2024 14:30:00
  sub: alice@shopwave.be
  http://schemas.microsoft.com/ws/2008/06/identity/claims/role: user
  iat: 1715996400
```

---

## 9. Samenvatting

| Concept | Wat je moet onthouden |
|---------|-----------------------|
| JWT | Zelfvoorzienend, ondertekend token zonder server-side opslag |
| Header | Algoritme (`HS256`) en tokentype (`JWT`) |
| Payload | Claims: `sub`, `role`, `exp`, `iss`, `aud` |
| Signature | Garandeert integriteit. JWT is ondertekend, niet versleuteld |
| Geen encryptie | Payload is leesbaar zonder sleutel. Nooit gevoelige data erin |
| `exp` | Vervaltijd als Unix-timestamp. Altijd instellen |
| `RequireAuthorization()` | Endpoint beveiligen in Minimal API |
| `RequireRole("admin")` | Rolgebaseerde toegangscontrole |
| 401 vs 403 | 401: geen geldig token. 403: geldig token, onvoldoende rechten |
| Bearer token | Meegestuurd via `Authorization: Bearer token` |
| Omgevingsvariabele | Sla geheimen op buiten de code. Nooit hardcoden in broncode |
| Azure Key Vault | Beheer geheimen op productieschaal. Applicatiecode verandert niet |
| OAuth 2.0 | Protocol voor toegangsdelegatie zonder wachtwoord te delen |
| Scopes | Beperken welke toegang een app krijgt |
| Access token | Kortlevend token voor API-toegang (bv. 15 minuten) |
| Refresh token | Langlevend token om een nieuw access token aan te vragen |
