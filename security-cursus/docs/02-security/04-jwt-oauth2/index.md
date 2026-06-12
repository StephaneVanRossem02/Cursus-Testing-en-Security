---
title: "Les 7: Security — JWT en OAuth2"
sidebar_label: "JWT & OAuth2"
---

# Les 7: Security — JWT en OAuth2

## ShopWave

In les 6 draait de ShopWave API op HTTPS. De verbinding is beveiligd. Maar elk endpoint is nog steeds toegankelijk voor iedereen — er is geen controle op wie de request stuurt. Iemand die het adres kent, kan bestellingen opvragen van elke gebruiker.

In deze les voegen we **authenticatie en autorisatie** toe via JWT. Na een geslaagde login krijgt de gebruiker een token dat hij meestuurt bij elke request naar beveiligde endpoints.

---

## Theorie

### 1. Sessies versus tokens

De traditionele aanpak is **sessiegebaseerde authenticatie**:

1. De gebruiker logt in
2. De server maakt een sessie aan en slaat die op in geheugen of database
3. De server stuurt een sessie-ID terug als cookie
4. Bij elke request zoekt de server de sessie op via dat ID

Dat werkt goed voor één server. Maar bij meerdere servers (load balancing, microservices) is het een probleem: server A kent de sessie niet die server B aanmaakte.

**Tokengebaseerde authenticatie** lost dit op. Het token bevat alle nodige informatie al in zich. De server hoeft niets op te slaan.

1. De gebruiker logt in
2. De server maakt een token aan met alle nodige informatie erin
3. De client stuurt het token mee bij elke request
4. Elke server kan het token valideren zonder een centrale opslag te raadplegen

---

### 2. Opbouw van een JWT

Een **JSON Web Token** bestaat uit drie delen gescheiden door punten:

```
header.payload.signature
```

**Header** — welk algoritme wordt gebruikt:
```json
{
  "alg": "HS256",
  "typ": "JWT"
}
```

**Payload** — de gegevens over de gebruiker (claims):
```json
{
  "sub": "alice@shopwave.be",
  "role": "user",
  "exp": 1716000000
}
```

| Claim | Betekenis |
|-------|-----------|
| `sub` | Subject — de gebruiker (e-mail of ID) |
| `exp` | Expiration — vervaltijd als Unix-timestamp |
| `iat` | Issued At — tijdstip van aanmaak |
| `iss` | Issuer — wie het token uitschreef |
| `aud` | Audience — voor wie het token bedoeld is |

**Signature** — berekend als:
```
HMACSHA256(
  base64Url(header) + "." + base64Url(payload),
  geheimeSleutel
)
```

De signature garandeert **integriteit**: als iemand de payload aanpast, klopt de signature niet meer en weigert de server het token.

---

### 3. JWT is geen encryptie

JWT ziet er onleesbaar uit maar is **niet versleuteld** — het is Base64-gecodeerd. Iedereen kan de payload decoderen:

```
eyJzdWIiOiJhbGljZUBzaG9wd2F2ZS5iZSJ9
→ {"sub":"alice@shopwave.be"}
```

De veiligheid zit in de **signature**, niet in geheimhouding. Stop nooit gevoelige gegevens zoals wachtwoorden of betaalgegevens in een JWT-payload.

---

### 4. De JWT-flow in ShopWave

```
Client                          ShopWave.Api
  |                                  |
  |--- POST /login ----------------> |  stap 1: login met wachtwoord
  |--- POST /verify ---------------> |  stap 2: 2FA-code bevestigen
  |<-- { token: "eyJ..." } --------- |  stap 3: JWT-token ontvangen
  |                                  |
  |--- GET /orders/alice ----------> |  stap 4: beveiligd endpoint aanroepen
  |    Authorization: Bearer eyJ... |         met token in de header
  |<-- 200 OK + orderdata ---------- |  stap 5: server valideert token, geeft data
```

---

### 5. OAuth 2.0

**JWT** is een tokenformaat. **OAuth 2.0** is een autorisatieprotocol — een manier om toegang te delegeren zonder wachtwoorden te delen.

Voorbeeld: een fitness-app wil trainingen inplannen in je Google Agenda. Via OAuth log je in bij Google zelf — de fitness-app ziet jouw wachtwoord nooit. Google geeft de app een token met beperkte rechten (scopes).

**De vier rollen:**

| Rol | Voorbeeld |
|-----|-----------|
| Resource Owner | Jij |
| Client | De fitness-app |
| Authorization Server | Google (login + toestemming) |
| Resource Server | Google Calendar API |

**Scopes** bepalen precies welke toegang een app krijgt. Bijvoorbeeld: `calendar.events.write` geeft enkel schrijftoegang tot agenda-items — niet je e-mail, niet je contacten.

OAuth 2.0 en JWT zijn complementair: OAuth regelt *hoe* toegang wordt verleend, JWT is het *formaat* van het token dat daarna gebruikt wordt.

---

## Demo

We breiden `ShopWave.Api` uit les 6 uit met JWT. Geen nieuw project — alles in de bestaande solution.

### NuGet packages toevoegen aan ShopWave.Api

Rechtsklik op `ShopWave.Api` → Manage NuGet Packages → installeer:

```
Microsoft.AspNetCore.Authentication.JwtBearer
System.IdentityModel.Tokens.Jwt
```

---

### Stap 1 — JwtTokenService aanmaken

Maak een nieuw bestand aan voor de klasse die tokens genereert.

**Bestand: `ShopWave.Api/JwtTokenService.cs`**

Begin met de velden en constructor:

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

De klasse ontvangt vier parameters: de geheime sleutel waarmee tokens ondertekend worden, de issuer (wie het token uitschreef), de audience (voor wie het bedoeld is) en hoe lang een token geldig blijft.

Voeg nu de methode toe die het eigenlijke token aanmaakt. Voeg dit toe **binnen de klasse**, na de constructor:

```csharp
public string GenerateToken(string email, string role)
{
    byte[]               keyBytes    = Encoding.UTF8.GetBytes(_secretKey);
    SymmetricSecurityKey securityKey = new SymmetricSecurityKey(keyBytes);
    SigningCredentials   credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
```

`SymmetricSecurityKey` converteert de geheime sleutelstring naar een sleutelobject dat de JWT-bibliotheek begrijpt. `SigningCredentials` koppelt die sleutel aan het HMACSHA256-algoritme — dat is het algoritme dat de signature berekent.

```csharp
    List<Claim> claims = new List<Claim>
    {
        new Claim(JwtRegisteredClaimNames.Sub, email),
        new Claim(ClaimTypes.Role, role),
        new Claim(JwtRegisteredClaimNames.Iat,
            DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString())
    };
```

Claims zijn de gegevens die in de payload komen. `Sub` is het e-mailadres van de gebruiker, `Role` bepaalt wat de gebruiker mag doen, `Iat` is het tijdstip van aanmaak.

```csharp
    JwtSecurityToken token = new JwtSecurityToken(
        issuer:             _issuer,
        audience:           _audience,
        claims:             claims,
        expires:            DateTime.UtcNow.AddMinutes(_expiresMinutes),
        signingCredentials: credentials
    );

    return new JwtSecurityTokenHandler().WriteToken(token);
}
```

`JwtSecurityToken` assembleert alle onderdelen. `WriteToken` serialiseert het naar de `header.payload.signature`-string die de client zal ontvangen.

---

### Stap 2 — JWT-authenticatie registreren

**Bestand: `ShopWave.Api/Program.cs`**

Voeg bovenaan de nodige usings toe:

```csharp
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using ShopWave.Api;
using ShopWave.Security;
using System.Security.Cryptography.X509Certificates;
```

Sla de JWT-sleutel veilig op via **.NET User Secrets** — zo staat hij nooit hardcoded in je broncode:

```
dotnet user-secrets init
dotnet user-secrets set "Jwt:SecretKey" "ShopWaveGeheimeSleutel2024!!XYZ#"
```

Lees de waarde daarna op via `IConfiguration`:

```csharp
string secretKey = builder.Configuration["Jwt:SecretKey"]
    ?? throw new InvalidOperationException("JWT SecretKey ontbreekt in configuratie.");
```

> ⚠️ Een hardcoded sleutel in broncode is een beveiligingsrisico — iedereen die toegang heeft tot de repository kan tokens namaken. Gebruik User Secrets lokaal en omgevingsvariabelen of Azure Key Vault in productie.

Definieer daarna de overige constanten:

```csharp
const string Issuer   = "shopwave-api";
const string Audience = "shopwave-client";
```

De sleutel moet minstens 32 tekens lang zijn voor HMACSHA256.

Maak de builder aan en configureer HTTPS zoals in les 6:

```csharp
WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(5001, listenOptions =>
    {
        X509Certificate2 certificate = CertificateHelper.CreateSelfSignedCertificate("localhost");
        listenOptions.UseHttps(certificate);
    });
});
```

Registreer nu JWT-authenticatie als service:

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
                                           Encoding.UTF8.GetBytes(SecretKey))
        };
    });

builder.Services.AddAuthorization();
```

`TokenValidationParameters` vertelt de server wat hij moet controleren bij elk binnenkomend token:
- `ValidateIssuer` — klopt de issuer?
- `ValidateAudience` — is het token voor deze API bedoeld?
- `ValidateLifetime` — is het token nog niet verlopen?
- `ValidateIssuerSigningKey` — klopt de signature?

Bouw de applicatie en activeer authenticatie en autorisatie in de juiste volgorde:

```csharp
WebApplication app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();
```

De volgorde is belangrijk: authenticatie (wie ben je?) moet altijd vóór autorisatie (wat mag je?).

---

### Stap 3 — Services en testaccount aanmaken

Maak de nodige service-objecten aan en registreer een testgebruiker:

```csharp
TwoFactorService  twoFactorService  = new TwoFactorService();
AccountRepository accountRepository = new AccountRepository(twoFactorService);
JwtTokenService   jwtTokenService   = new JwtTokenService(SecretKey, Issuer, Audience);

accountRepository.Register("alice@shopwave.be", "wachtwoord123");
```

---

### Stap 4 — Endpoints toevoegen

Het publieke endpoint — geen token vereist:

```csharp
app.MapGet("/", () => "ShopWave API actief op HTTPS met JWT");
```

Het login-endpoint start de loginflow:

```csharp
app.MapPost("/login", (LoginRequest request) =>
{
    string result = accountRepository.Login(request.Email, request.Password);
    return Results.Ok(new { status = result });
});
```

Het verify-endpoint bevestigt de 2FA-code en geeft bij succes een JWT terug:

```csharp
app.MapPost("/verify", (VerifyRequest request) =>
{
    string result = accountRepository.VerifyTwoFactor(request.Email, request.Code);

    if (result != "Inloggen geslaagd.")
    {
        return Results.Unauthorized();
    }

    string token = jwtTokenService.GenerateToken(request.Email, "user");
    return Results.Ok(new { token = token });
});
```

Het beveiligde orders-endpoint — `.RequireAuthorization()` weigert elke request zonder geldig token:

```csharp
app.MapGet("/orders/{email}", (string email) =>
{
    X509Certificate2 certificate = CertificateHelper.CreateSelfSignedCertificate("ShopWave");
    OrderSigner      signer      = new OrderSigner(certificate);
    string           orderData   = $"{email} | Laptop | 999.99 EUR";
    string           signature   = signer.Sign(orderData);

    return Results.Ok(new { Order = orderData, Signature = signature });
}).RequireAuthorization();
```

Voeg onderaan de request-records toe:

```csharp
record LoginRequest(string Email, string Password);
record VerifyRequest(string Email, string Code);
```

`record` is een compacte manier om een klasse aan te maken met enkel properties — ideaal voor request-objecten die je nergens anders voor nodig hebt.

Sluit af:

```csharp
app.Run();
```

---

### Stap 5 — De flow testen vanuit de console

**Bestand: `ShopWave/Program.cs`** (tijdelijk toevoegen)

Maak een `HttpClient` aan die self-signed certificaten accepteert:

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

Stuur de login-request:

```csharp
Console.WriteLine("=== Stap 1: Login ===");

string        loginJson     = JsonSerializer.Serialize(new { email = "alice@shopwave.be", password = "wachtwoord123" });
StringContent loginContent  = new StringContent(loginJson, Encoding.UTF8, "application/json");
HttpResponseMessage loginResponse = client.PostAsync("/login", loginContent).Result;

Console.WriteLine(loginResponse.Content.ReadAsStringAsync().Result);
```

De API toont de 2FA-code in zijn eigen console. Lees die code in:

```csharp
Console.Write("Voer de 2FA-code in: ");
string twoFactorCode = Console.ReadLine() ?? string.Empty;
```

Verifieer de code en haal het token op:

```csharp
Console.WriteLine("=== Stap 2: Verify + Token ===");

string        verifyJson    = JsonSerializer.Serialize(new { email = "alice@shopwave.be", code = twoFactorCode });
StringContent verifyContent = new StringContent(verifyJson, Encoding.UTF8, "application/json");
HttpResponseMessage verifyResponse = client.PostAsync("/verify", verifyContent).Result;

string verifyBody = verifyResponse.Content.ReadAsStringAsync().Result;
Console.WriteLine(verifyBody);

JsonDocument verifyDoc = JsonDocument.Parse(verifyBody);
string token = verifyDoc.RootElement.GetProperty("token").GetString() ?? string.Empty;
```

Roep het beveiligde endpoint aan mét token:

```csharp
Console.WriteLine("=== Stap 3: Beveiligd endpoint met token ===");

client.DefaultRequestHeaders.Authorization =
    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

HttpResponseMessage ordersResponse = client.GetAsync("/orders/alice@shopwave.be").Result;
Console.WriteLine($"Status: {ordersResponse.StatusCode}");
Console.WriteLine(ordersResponse.Content.ReadAsStringAsync().Result);
```

Roep hetzelfde endpoint aan zónder token:

```csharp
Console.WriteLine("=== Stap 4: Zonder token (verwacht 401) ===");

client.DefaultRequestHeaders.Authorization = null;
HttpResponseMessage noTokenResponse = client.GetAsync("/orders/alice@shopwave.be").Result;
Console.WriteLine($"Status: {noTokenResponse.StatusCode}");
```

Observeer: mét token → `200 OK`, zonder token → `401 Unauthorized`.

---

### Stap 6 — JWT-payload inspecteren

Voeg toe in `ShopWave/Program.cs` na stap 5:

```csharp
using System.IdentityModel.Tokens.Jwt;

JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
JwtSecurityToken        parsedToken  = tokenHandler.ReadJwtToken(token);
```

`ReadJwtToken` decodeert het token zonder de signature te valideren. Dat toont aan dat de payload leesbaar is zonder de geheime sleutel.

```csharp
Console.WriteLine("=== JWT Payload (leesbaar zonder sleutel!) ===");
Console.WriteLine($"Subject:  {parsedToken.Subject}");
Console.WriteLine($"Verloopt: {parsedToken.ValidTo}");

foreach (System.Security.Claims.Claim claim in parsedToken.Claims)
{
    Console.WriteLine($"  {claim.Type}: {claim.Value}");
}
```

Opruimen:

```csharp
handler.Dispose();
client.Dispose();
```

---

## Oefeningen

### Oefening 1 — JWT decoderen en begrijpen

Kopieer het token dat de API teruggeeft en plak het op [https://jwt.io](https://jwt.io).

Beantwoord:
1. Welke claims staan er in de payload?
2. Wat is de vervaltijd (`exp`) in leesbare datum/tijd?
3. Waarom toont jwt.io de payload zonder dat je de geheime sleutel moet ingeven?
4. Waarom stop je geen wachtwoord in een JWT-payload?

---

### Oefening 2 — Admin-rol toevoegen

Voeg in `Program.cs` een tweede testaccount toe:

```csharp
accountRepository.Register("admin@shopwave.be", "admin123");
```

Pas het `/verify`-endpoint aan zodat admins een token krijgen met `role = "admin"`:

```csharp
string role = "user";

if (request.Email == "admin@shopwave.be")
{
    role = "admin";
}

string token = jwtTokenService.GenerateToken(request.Email, role);
```

Voeg een admin-endpoint toe dat enkel toegankelijk is voor admins:

```csharp
app.MapGet("/admin/orders", () =>
{
    return Results.Ok(new { bericht = "Alle bestellingen — enkel voor admins" });
}).RequireAuthorization(policy => policy.RequireRole("admin"));
```

Test vanuit de console:
1. Login als `alice@shopwave.be` → roep `/admin/orders` aan → verwacht `403 Forbidden`
2. Login als `admin@shopwave.be` → roep `/admin/orders` aan → verwacht `200 OK`

---

### Oefening 3 — Vervaltijd testen

Maak in de console een `JwtTokenService` aan met een vervaltijd van 0 minuten:

```csharp
JwtTokenService kortLevend = new JwtTokenService(
    "ShopWaveGeheimeSleutel2024!!XYZ#",
    "shopwave-api",
    "shopwave-client",
    expiresMinutes: 0
);
```

Genereer een token, wacht 2 seconden en stuur een request naar `/orders/alice@shopwave.be`.

```csharp
string kortToken = kortLevend.GenerateToken("alice@shopwave.be", "user");
System.Threading.Thread.Sleep(2000);

client.DefaultRequestHeaders.Authorization =
    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", kortToken);

HttpResponseMessage verlopen = client.GetAsync("/orders/alice@shopwave.be").Result;
Console.WriteLine($"Verlopen token: {verlopen.StatusCode}");
```

Verwacht: `401 Unauthorized`.

Beantwoord: waarom is een korte vervaltijd een veiligheidsmaatregel?

---

### Oefening 4 — /me endpoint

Voeg een `/me`-endpoint toe dat de claims uit het token uitleest:

```csharp
app.MapGet("/me", (HttpContext context) =>
{
    string email = context.User.FindFirst(
                       System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value
                   ?? "onbekend";

    string role = context.User.FindFirst(
                      System.Security.Claims.ClaimTypes.Role)?.Value
                  ?? "geen";

    return Results.Ok(new { Email = email, Rol = role });
}).RequireAuthorization();
```

`context.User` bevat de claims die de server uit het token uitgelezen heeft na validatie. Je hoeft het token zelf niet opnieuw te decoderen.

Test: roep `/me` aan mét geldig token, dan zónder token.

---

### Oefening 5 — OAuth 2.0 analyse

Beantwoord schriftelijk op basis van de theorie:

1. Beschrijf de Authorization Code Flow in eigen woorden in vier stappen
2. Waarom ziet de fitness-app het Google-wachtwoord van de gebruiker nooit?
3. Wat is het verschil tussen een access token en een refresh token?
4. Geef een voorbeeld van een scope die je als ontwikkelaar zou aanvragen voor een agenda-app
5. Is OAuth hetzelfde als JWT? Leg het verschil uit

---

## Modeloplossing

> De modeloplossing is beschikbaar na het indienen van de labo-opdracht via Digitap.

---

### Modeloplossing Oefening 2 — Admin-rol, volledig verify-endpoint

**Bestand: `ShopWave.Api/Program.cs`**

```csharp
app.MapPost("/verify", (VerifyRequest request) =>
{
    string result = accountRepository.VerifyTwoFactor(request.Email, request.Code);

    if (result != "Inloggen geslaagd.")
    {
        return Results.Unauthorized();
    }

    string role = "user";

    if (request.Email == "admin@shopwave.be")
    {
        role = "admin";
    }

    string token = jwtTokenService.GenerateToken(request.Email, role);
    return Results.Ok(new { token = token });
});
```

---

## Samenvatting

| Concept | Wat je moet onthouden |
|---------|-----------------------|
| JWT | Zelfvoorzienend, ondertekend token — geen server-side sessie nodig |
| Header | Algoritme en tokentype |
| Payload | Claims: `sub`, `role`, `exp`, `iss`, `aud` |
| Signature | Garandeert integriteit — JWT is ondertekend, niet versleuteld |
| `RequireAuthorization()` | Endpoint beveiligen in Minimal API |
| `RequireRole("admin")` | Rolgebaseerde toegangscontrole |
| Bearer token | Meegestuurd via `Authorization: Bearer <token>` |
| Access token | Kortlevend JWT voor API-toegang (bv. 15 minuten geldig) |
| Refresh token | Langlevend token waarmee een nieuw access token aangevraagd wordt zonder opnieuw in te loggen |
| User Secrets | Sla geheimen lokaal op via `dotnet user-secrets` — nooit hardcoden in broncode |
| OAuth 2.0 | Protocol voor toegangsdelegatie zonder wachtwoord te delen |
| Scopes | Fijne controle over welke toegang een app krijgt |

---

## Volgende les

Les 8 is **Testing: Mocking verdieping**. We bekijken geavanceerdere Moq-technieken: callbacks, sequences en het testen van exception-scenario's in ShopWave.