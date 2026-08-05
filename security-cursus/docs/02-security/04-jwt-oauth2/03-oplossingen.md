---
title: "Les 7: Oplossingen - JWT en OAuth2"
sidebar_label: "Oplossingen"
---

# Oplossingen: JWT en OAuth2

**Bekijk dit pas nadat je de oefeningen zelf geprobeerd hebt.** Lees de toelichting ook als je het juist had.

---

## Oplossing 1: /me endpoint

### ShopWave.Api/Program.cs

```csharp
app.MapGet("/me", HandleMe).RequireAuthorization();

IResult HandleMe(HttpContext context)
{
    string email = string.Empty;
    string role  = string.Empty;

    System.Security.Claims.Claim emailClaim = context.User.FindFirst(
        System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub);

    System.Security.Claims.Claim roleClaim = context.User.FindFirst(
        System.Security.Claims.ClaimTypes.Role);

    if (emailClaim != null)
    {
        email = emailClaim.Value;
    }

    if (roleClaim != null)
    {
        role = roleClaim.Value;
    }

    return Results.Ok(new { Email = email, Role = role });
}
```

### Toelichting

`context.User` bevat de claims die de JWT-middleware uit het token uitgelezen heeft na succesvolle validatie. Je hoeft het token zelf niet opnieuw te decoderen. De middleware doet dat automatisch als onderdeel van `app.UseAuthentication()`.

`JwtRegisteredClaimNames.Sub` is de standaard claim-naam voor het subject. De JWT-bibliotheek mapt `sub` intern naar deze constante. `ClaimTypes.Role` is de .NET-naam voor de rol-claim.

`.RequireAuthorization()` zorgt dat `context.User` altijd gevuld is als het endpoint bereikt wordt. Zonder die aanroep kan `context.User` null zijn.

**Veelgemaakte fout:** studenten gebruiken `context.User.FindFirst("sub")?.Value`. Dat werkt soms niet omdat de JWT-middleware de claim-naam intern omzet naar een langere URI-naam. Gebruik de constantes `JwtRegisteredClaimNames.Sub` en `ClaimTypes.Role` om die mapping te vermijden.

---

## Oplossing 2: Admin-rol en rolgebaseerde toegang

### ShopWave.Api/Program.cs

```csharp
// Registreer beide accounts
accountRepository.Register("alice@shopwave.be", "wachtwoord123");
accountRepository.Register("admin@shopwave.be", "admin123");

// Verify-endpoint met rolbepaling
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

// Admin-endpoint
app.MapGet("/admin/orders", HandleAdminOrders)
   .RequireAuthorization(policy => policy.RequireRole("admin"));

IResult HandleAdminOrders()
{
    return Results.Ok(new
    {
        Orders = new[]
        {
            new { OrderId = "ORD-001", Customer = "alice@shopwave.be",  Total = 999.99  },
            new { OrderId = "ORD-002", Customer = "bob@shopwave.be",    Total = 249.50  },
            new { OrderId = "ORD-003", Customer = "carol@shopwave.be",  Total = 1499.00 }
        }
    });
}
```

### Toelichting

`DetermineRole` is een aparte methode zodat de logica later eenvoudig uitgebreid kan worden (bv. rol ophalen uit een database). De rol wordt opgeslagen in de JWT-payload als claim. Bij elke request leest de middleware die claim uit en stelt `context.User` in.

`.RequireAuthorization(policy => policy.RequireRole("admin"))` vertelt de middleware dat enkel gebruikers met de claim `role = "admin"` het endpoint mogen aanroepen. Alle anderen krijgen `403 Forbidden`, ook als ze een geldig token hebben.

Het verschil tussen `401` en `403` is belangrijk:
- `401 Unauthorized`: geen geldig token meegestuurd.
- `403 Forbidden`: geldig token, maar de gebruiker heeft niet de juiste rechten.

**Veelgemaakte fout:** studenten plaatsen de rolbepaling buiten het verify-endpoint, bv. als hardcoded waarde bij `Register`. De rol hoort in het token te zitten, niet in de accountopslag. De token bepaalt wat een gebruiker mag doen tijdens die sessie.

---

## Oplossing 3: Token vervaltijd valideren

### ShopWave/Program.cs

```csharp
using ShopWave.Security;

void DemoExpiredToken()
{
    string secretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY")
        ?? throw new InvalidOperationException("Omgevingsvariabele JWT_SECRET_KEY ontbreekt.");

    JwtTokenService shortLived = new JwtTokenService(
        secretKey,
        "shopwave-api",
        "shopwave-client",
        expiresMinutes: 0);

    string expiredToken = shortLived.GenerateToken("alice@shopwave.be", "user");

    System.Threading.Thread.Sleep(2000);

    HttpClientHandler handler = new HttpClientHandler();
    handler.ServerCertificateCustomValidationCallback =
        (message, certificate, chain, errors) => true;

    HttpClient client = new HttpClient(handler);
    client.BaseAddress = new Uri("https://localhost:5001");

    client.DefaultRequestHeaders.Authorization =
        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", expiredToken);

    HttpResponseMessage response = client.GetAsync("/orders/alice@shopwave.be").Result;

    Console.WriteLine($"Verlopen token statuscode: {response.StatusCode}");

    client.Dispose();
    handler.Dispose();
}
```

### Toelichting

`expiresMinutes: 0` geeft het token een vervaltijd van `DateTime.UtcNow`. Na 2 seconden is de `exp`-claim al overschreden. De JWT-middleware controleert `ValidateLifetime = true` en weigert het token.

**Antwoord op de reflectievraag:** een korte vervaltijd beperkt de schade bij een gestolen token. Als een aanvaller een token onderschept, heeft hij maar een beperkte tijd om het te misbruiken. Bij een token zonder vervaldatum heeft de aanvaller permanent toegang, tenzij het systeem de geheime sleutel vervangt (wat alle tokens van alle gebruikers invalideert).

In productie combineer je een korte vervaltijd voor het access token (bv. 15 minuten) met een refresh token (langlevend) om de gebruiker opnieuw te authenticeren zonder opnieuw in te moeten loggen.

**Veelgemaakte fout:** studenten gebruiken `Thread.Sleep(200)` (200 milliseconden) in plaats van `Thread.Sleep(2000)`. `expiresMinutes: 0` geeft de huidige seconde als vervaltijd. De klokresolutie van JWT is seconden, niet milliseconden. Een wacht van 200 ms is soms te kort.

---

## Oplossing 4: TokenBlacklist implementeren

### ShopWave.Api/TokenBlacklist.cs

```csharp
namespace ShopWave.Api
{
    public class TokenBlacklist
    {
        private readonly HashSet<string> revokedTokens;

        public TokenBlacklist()
        {
            revokedTokens = new HashSet<string>();
        }

        public void Revoke(string token)
        {
            revokedTokens.Add(token);
        }

        public bool IsRevoked(string token)
        {
            return revokedTokens.Contains(token);
        }
    }
}
```

### ShopWave.Api/Program.cs (relevante toevoegingen)

```csharp
TokenBlacklist tokenBlacklist = new TokenBlacklist();

app.UseAuthentication();

app.Use(async (context, next) =>
{
    string authHeader = context.Request.Headers["Authorization"].ToString();
    string token      = authHeader.Replace("Bearer ", string.Empty);

    if (tokenBlacklist.IsRevoked(token))
    {
        context.Response.StatusCode = 401;
        return;
    }

    await next();
});

app.UseAuthorization();

// Logout-endpoint
app.MapPost("/logout", HandleLogout).RequireAuthorization();

IResult HandleLogout(HttpContext context)
{
    string authHeader = context.Request.Headers["Authorization"].ToString();
    string token      = authHeader.Replace("Bearer ", string.Empty);

    tokenBlacklist.Revoke(token);

    return Results.Ok(new { Message = "Uitgelogd." });
}
```

### Toelichting

`HashSet<string>` is de juiste keuze voor een blacklist: `Contains` is O(1), ook bij grote aantallen tokens.

De middleware staat na `app.UseAuthentication()` zodat de JWT al gevalideerd is voordat de blacklist gecontroleerd wordt. Tokens die niet geldig zijn (verlopen, foute signature) worden al eerder geweigerd.

`return;` zonder `await next()` stopt de pipeline. De endpoints worden niet bereikt.

**Nadeel van de blacklist op schaalbaarheid:** als je meerdere API-servers hebt, kent elke server alleen zijn eigen blacklist. Een token dat uitgelogd is op server A, is nog geldig op server B. Oplossingen zijn een gedeelde opslag (Redis, database) of een zo kort mogelijke vervaltijd zodat de blacklist snel irrelevant wordt.

**Veelgemaakte fout:** studenten plaatsen de middleware voor `app.UseAuthentication()`. Dan is het token nog niet gevalideerd en is `authHeader` soms leeg voor anonieme endpoints. Zet de middleware altijd na authenticatie.

---

## Oplossing 5: JWT en OAuth 2.0 koppelen aan CIA

**Vraag 1: signature en CIA**

De signature beschermt **Integrity**. Als iemand de payload aanpast, klopt de signature niet meer en weigert de server het token. De signature biedt geen Confidentiality: de payload is leesbaar voor iedereen die het token heeft, ook zonder de geheime sleutel. Confidentiality vereist encryptie van de payload (JWE, JSON Web Encryption), wat in de praktijk weinig gebruikt wordt.

**Vraag 2: vervaltijd en CIA**

De vervaltijd beperkt de tijdsduur van toegang en draagt bij aan **Confidentiality** en **Availability**. Een gestolen token geeft een aanvaller toegang tot vertrouwelijke gegevens (Confidentiality). Als tokens nooit verlopen, kan een aanvaller met een gestolen token permanent meelezen. Een korte vervaltijd beperkt dat tijdvenster.

**Vraag 3: blacklist en schaalbaarheid**

De blacklist is server-lokaal. Bij meerdere servers kent elke server alleen zijn eigen blacklist. Een token dat uitgelogd is op server A, is nog geldig op server B. Een gedeelde opslag (bv. Redis) lost dat op maar voegt een centraal storingspunt toe, wat Availability bedreigt. Een korte vervaltijd lost het probleem deels op: als tokens na 15 minuten verlopen, hoeft de blacklist maximaal 15 minuten lang tokens bij te houden. Daarna zijn ze sowieso ongeldig.

**Vraag 4: scopes en CIA**

Scopes beschermen **Confidentiality**. Ze beperken welke gegevens een applicatie kan zien of wijzigen. Het principe van least privilege houdt in dat een app enkel de scopes aanvraagt die ze echt nodig heeft. `calendar.readonly` in plaats van `calendar.events.write` geeft de app leestoegang maar geen schrijftoegang. Als de app gecompromitteerd wordt, kan een aanvaller geen agenda-items wijzigen of verwijderen.

**Vraag 5: HTTPS en JWT samen**

HTTPS (les 6) en JWT (les 7) beschermen twee verschillende lagen:

| Les | Techniek | Beschermt |
|-----|----------|-----------|
| 6 | HTTPS/TLS | Het transport: niemand kan het verkeer onderscheppen of lezen |
| 7 | JWT | De endpoints: niemand kan een endpoint aanroepen zonder geldig token |

Zonder HTTPS kan een aanvaller het JWT-token onderscheppen uit de `Authorization`-header en het hergebruiken. Zonder JWT zijn alle endpoints open voor iedereen, ook al is de verbinding versleuteld. Beide lagen zijn nodig: HTTPS beveiligt de verbinding, JWT beveiligt de toegang.

---

## Dit project downloaden

[Download het volledige ShopWave-project van les 7](/downloads/shopwave-07-jwt-en-oauth2.zip) (ZIP)

Bevat alle code tot en met deze les, klaar om te openen in Visual Studio. Bouwen en testen doe je met `dotnet build` en `dotnet test`. In de `README.md` staat wat er nieuw is en hoeveel tests er horen te slagen.

Alle lessen samen vind je op [Oplossingen downloaden](../../oplossingen-downloaden.md).
