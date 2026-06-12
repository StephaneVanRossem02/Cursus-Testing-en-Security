---
title: "Les 7: Security — JWT en OAuth2 — Oefeningen"
sidebar_label: "Oefeningen"
---

# Les 7: Security — JWT en OAuth2 — Oefeningen

> **Code-afspraken:** geen top-level statements · altijd `{}` · max één `return` · geen `break`/`continue` · geen underscore-prefix op parameters · geen geneste klassen · geen ternary/null-conditional · geen tuples · `double` i.p.v. `decimal` · identifiers Engels · tekst Nederlands

---

## Oefening 1 — JWT decoderen en begrijpen

**Opgave:** Kopieer het token dat de API teruggeeft en plak het op [https://jwt.io](https://jwt.io).

**Antwoorden op de vragen:**

1. **Welke claims staan er in de payload?** `sub` (e-mailadres), `role`, `iss` (shopwave-api), `aud` (shopwave-client), `exp` (vervaltijd als Unix timestamp), `iat` (aanmaaktijd)
2. **Wat is de vervaltijd in leesbare datum/tijd?** Gebruik jwt.io — de `exp`-waarde wordt automatisch getoond als datum/tijd.
3. **Waarom toont jwt.io de payload zonder de geheime sleutel?** JWT is Base64url-gecodeerd, niet versleuteld. Iedereen kan de header en payload lezen.
4. **Waarom stop je geen wachtwoord in een JWT-payload?** Omdat iedereen met het token de payload kan lezen — encryptie is nodig om data echt verborgen te houden.

---

## Oefening 2 — Admin-rol toevoegen

**Opgave:** Voeg een admin-account toe. Pas `/verify` aan zodat admins `role = "admin"` krijgen. Voeg een `/admin/orders`-endpoint toe dat enkel toegankelijk is voor admins.

**Fragment in ShopWave.Api/Program.cs**

```csharp
// Verify-endpoint met rolbepaling
app.MapPost("/verify", HandleVerify);

IResult HandleVerify(VerifyRequest request)
{
    string verifyResult = accountRepository.VerifyTwoFactor(request.Email, request.Code);

    if (verifyResult != "Inloggen geslaagd.")
    {
        return Results.Unauthorized();
    }

    string role  = DetermineRole(request.Email);
    string token = jwtTokenService.GenerateToken(request.Email, role);

    return Results.Ok(new { token = token });
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
    return Results.Ok(new { bericht = "Alle bestellingen — enkel voor admins" });
}
```

**Test vanuit console:**
- Alice (rol user) roept `/admin/orders` aan → `403 Forbidden`
- Admin roept `/admin/orders` aan → `200 OK`

---

## Oefening 3 — Vervaltijd testen

**Opgave:** Maak een `JwtTokenService` met `expiresMinutes: 0`, genereer een token, wacht 2 seconden, stuur een request → verwacht `401 Unauthorized`.

```csharp
// In ShopWave/Program.cs
private static void DemoVervalToken()
{
    JwtTokenService kortLevend = new JwtTokenService(
        "ShopWaveGeheimeSleutel2024!!XYZ#",
        "shopwave-api",
        "shopwave-client",
        expiresMinutes: 0);

    string kortToken = kortLevend.GenerateToken("alice@shopwave.be", "user");

    System.Threading.Thread.Sleep(2000);

    HttpClient client = new HttpClient();
    client.DefaultRequestHeaders.Authorization =
        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", kortToken);

    HttpResponseMessage response =
        client.GetAsync("https://localhost:5001/orders/alice@shopwave.be").Result;

    Console.WriteLine($"Verlopen token statuscode: {response.StatusCode}");
    // Verwacht: Unauthorized

    client.Dispose();
}
```

**Antwoord:** Een korte vervaltijd is een veiligheidsmaatregel omdat een gestolen token snel onbruikbaar wordt. Bij een gestolen token van 15 minuten heeft de aanvaller maar 15 minuten toegang; bij een token zonder vervaldatum is toegang permanent.

---

## Oefening 4 — /me endpoint

**Opgave:** Voeg een `/me`-endpoint toe dat de claims uit het token uitleest.

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

    return Results.Ok(new { Email = email, Rol = role });
}
```

---

## Oefening 5 — OAuth2 analyse (schriftelijk)

1. **Authorization Code Flow in vier stappen:** (1) Gebruiker klikt op "Inloggen met Google" → (2) Browser gaat naar Google-loginpagina → (3) Gebruiker logt in en geeft toestemming → (4) Google stuurt een code terug, de app wisselt die in voor een access token (server-to-server).
2. **Waarom ziet de fitness-app het Google-wachtwoord nooit?** De gebruiker logt in op de Google-pagina, niet op de fitness-app. Alleen Google ziet het wachtwoord.
3. **Verschil access token vs. refresh token:** access token is kortlevend (bv. 15 min) en geeft toegang tot API's; refresh token is langlevend en wordt gebruikt om een nieuw access token aan te vragen zonder opnieuw in te loggen.
4. **Scope voor een agenda-app:** `calendar.read` of `calendar.events.readonly` — enkel leestoegang tot agenda-items, geen schrijftoegang.
5. **OAuth2 versus JWT:** OAuth2 is een protocol (regelt wie toegang krijgt tot wat); JWT is een tokenformaat (hoe ziet een token eruit). OAuth2-servers gebruiken vaak JWT als formaat voor hun access tokens.