---
title: "Les 7: Oefeningen - JWT en OAuth2"
sidebar_label: "Oefeningen"
---

# Oefeningen: JWT en OAuth2

Werk de oefeningen in volgorde. Elke oefening bouwt verder op de vorige. Kijk niet vooraf in de oplossingen.

Je werkt verder in de bestaande ShopWave-solution. Nieuwe klassen maak je aan in `ShopWave/Security/` of `ShopWave.Api/`.

---

<h3 class="opdracht-titel">Opdracht</h3>

## Oefening 1: /me endpoint uitbreiden

**Leerdoel:** je leest claims uit een JWT-token via de `HttpContext` en begrijpt dat de server de claims zelf levert na validatie.

**Moeilijkheidsgraad:** basis

**Situatie:** een klant van ShopWave wil weten welke informatie de API over hem opgeslagen heeft in zijn token. Je voegt een `/me`-endpoint toe dat de claims uit het token van de ingelogde gebruiker teruggeeft.

**Wat je doet:**

Voeg in `ShopWave.Api/Program.cs` een `/me`-endpoint toe dat enkel toegankelijk is voor geauthenticeerde gebruikers. Het endpoint leest het e-mailadres en de rol uit het token en geeft die terug als JSON.

**Vereisten:**

- Gebruik `context.User.FindFirst(...)` om claims op te halen. Je hebt `HttpContext context` nodig als parameter.
- Gebruik `JwtRegisteredClaimNames.Sub` voor het e-mailadres en `ClaimTypes.Role` voor de rol.
- Als een claim ontbreekt, geef dan een lege string terug.
- Het endpoint vereist een geldig token via `.RequireAuthorization()`.
- Gebruik geen `?.Value` (null-conditional). Gebruik `if`-blokken.

**Startcode:**

```csharp
app.MapGet("/me", HandleMe).RequireAuthorization();

IResult HandleMe(HttpContext context)
{
    string email = string.Empty;
    string role  = string.Empty;

    // jouw code hier: haal de "sub"-claim op voor het e-mailadres
    // jouw code hier: haal de rol-claim op

    return Results.Ok(new { Email = email, Role = role });
}
```

**Controleer je werk:** start de API en roep `/me` aan vanuit de console met een geldig token:

```csharp
client.DefaultRequestHeaders.Authorization =
    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

HttpResponseMessage meResponse = client.GetAsync("/me").Result;
Console.WriteLine(meResponse.Content.ReadAsStringAsync().Result);
```

Verwacht resultaat:

```json
{"email":"alice@shopwave.be","role":"user"}
```

Roep daarna `/me` aan zonder token. Verwacht: `401 Unauthorized`.

---

<h3 class="opdracht-titel">Opdracht</h3>

## Oefening 2: Admin-rol en rolgebaseerde toegang

**Leerdoel:** je implementeert rolgebaseerde autorisatie en begrijpt het verschil tussen `401 Unauthorized` en `403 Forbidden`.

**Moeilijkheidsgraad:** gemiddeld

**Situatie:** ShopWave heeft medewerkers die alle bestellingen moeten kunnen inzien. Klanten mogen enkel hun eigen bestellingen zien. Je voegt een `/admin/orders`-endpoint toe dat enkel toegankelijk is voor admins.

**Wat je doet:**

Breid `ShopWave.Api/Program.cs` uit:

1. Zorg dat `admin@shopwave.be` de rol `"admin"` krijgt bij het genereren van het token. Andere gebruikers krijgen de rol `"user"`. Maak hiervoor een aparte methode `DetermineRole(string email)`.
2. Voeg een `/admin/orders`-endpoint toe dat enkel toegankelijk is voor gebruikers met de rol `"admin"`. Het endpoint geeft een JSON-object terug met een gesimuleerde lijst van bestellingen.

**Vereisten:**

- Gebruik `.RequireAuthorization(policy => policy.RequireRole("admin"))`.
- De `DetermineRole`-methode gebruikt een `if`-blok, geen ternary.
- Registreer het admin-account via `accountRepository.Register("admin@shopwave.be", "admin123")`.

**Startcode:**

```csharp
string DetermineRole(string email)
{
    string role;

    // jouw code hier

    return role;
}

app.MapGet("/admin/orders", HandleAdminOrders)
   .RequireAuthorization(policy => policy.RequireRole("admin"));

IResult HandleAdminOrders()
{
    // jouw code hier: geef een gesimuleerde lijst van bestellingen terug
    return Results.Ok(new { });
}
```

**Controleer je werk:** test de volgende twee scenario's vanuit de console:

```csharp
// Scenario 1: alice (user) roept /admin/orders aan
client.DefaultRequestHeaders.Authorization =
    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", aliceToken);

HttpResponseMessage aliceResponse = client.GetAsync("/admin/orders").Result;
Console.WriteLine($"Alice: {aliceResponse.StatusCode}");

// Scenario 2: admin roept /admin/orders aan
client.DefaultRequestHeaders.Authorization =
    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

HttpResponseMessage adminResponse = client.GetAsync("/admin/orders").Result;
Console.WriteLine($"Admin: {adminResponse.StatusCode}");
Console.WriteLine(adminResponse.Content.ReadAsStringAsync().Result);
```

Verwacht resultaat:

```
Alice: Forbidden
Admin: OK
{"orders":[...]}
```

---

<h3 class="opdracht-titel">Opdracht</h3>

## Oefening 3: Token vervaltijd valideren

**Leerdoel:** je implementeert en test de vervaltijd van een JWT en begrijpt waarom een korte levensduur een veiligheidsmaatregel is.

**Moeilijkheidsgraad:** gemiddeld

**Situatie:** een gestolen JWT geeft een aanvaller toegang zolang het token geldig is. Hoe korter de vervaltijd, hoe kleiner de schade. Je schrijft een methode die demonstreert wat er gebeurt met een verlopen token.

**Wat je doet:**

Maak in `ShopWave/Program.cs` een methode `DemoExpiredToken()` die:

1. Een `JwtTokenService` aanmaakt met een vervaltijd van 0 minuten.
2. Een token genereert voor `alice@shopwave.be` met rol `"user"`.
3. 2 seconden wacht via `Thread.Sleep(2000)`.
4. Het verlopen token gebruikt om `/orders/alice@shopwave.be` aan te roepen.
5. De statuscode afdrukt.

Beantwoord daarna schriftelijk: waarom is een korte vervaltijd een veiligheidsmaatregel? Wat zou er kunnen misgaan als een token nooit verloopt?

**Vereisten:**

- Gebruik `System.Threading.Thread.Sleep(2000)` voor de wachttijd.
- Gebruik een `HttpClient` met `ServerCertificateCustomValidationCallback` die altijd `true` teruggeeft.
- De methode maakt een eigen `HttpClient` aan en ruimt die op met `Dispose()`.

**Startcode:**

```csharp
void DemoExpiredToken()
{
    JwtTokenService shortLived = new JwtTokenService(
        "ShopWaveGeheimeSleutel2024!!XYZ#",
        "shopwave-api",
        "shopwave-client",
        expiresMinutes: 0);

    string expiredToken = shortLived.GenerateToken("alice@shopwave.be", "user");

    // jouw code hier: wacht 2 seconden

    HttpClientHandler handler = new HttpClientHandler();
    handler.ServerCertificateCustomValidationCallback =
        (message, certificate, chain, errors) => true;

    HttpClient client = new HttpClient(handler);
    client.BaseAddress = new Uri("https://localhost:5001");

    // jouw code hier: stuur een request met het verlopen token en druk de statuscode af

    client.Dispose();
    handler.Dispose();
}
```

**Controleer je werk:** verwacht resultaat:

```
Verlopen token statuscode: Unauthorized
```

---

<h3 class="opdracht-titel">Opdracht</h3>

## Oefening 4: TokenBlacklist implementeren

**Leerdoel:** je implementeert een uitlogmechanisme voor JWT en begrijpt waarom dat extra infrastructuur vereist tegenover de stateless aard van tokens.

**Moeilijkheidsgraad:** uitdaging

**Situatie:** JWT-tokens zijn geldig tot ze verlopen. Er is geen ingebouwd mechanisme om een token te annuleren. Als een klant uitlogt of als een token gestolen wordt, blijft het token bruikbaar tot de vervaltijd. ShopWave wil een uitlogendpoint toevoegen dat tokens onmiddellijk invalideert.

**Wat je doet:**

Maak `ShopWave.Api/TokenBlacklist.cs` aan. Deze klasse slaat tokens op die uitgelogd zijn en biedt twee methoden:

- `Revoke(string token)`: voegt het token toe aan de blacklist.
- `IsRevoked(string token)`: geeft `true` terug als het token op de blacklist staat.

Voeg daarna in `ShopWave.Api/Program.cs` een `/logout`-endpoint toe dat:

1. Het token uit de `Authorization`-header leest.
2. Het token toevoegt aan de `TokenBlacklist`.
3. `200 OK` teruggeeft.

Voeg ten slotte middleware toe die bij elke request controleert of het token op de blacklist staat. Als het token gerevoked is, geeft de middleware `401 Unauthorized` terug zonder de rest van de pipeline uit te voeren.

**Vereisten:**

- `TokenBlacklist` gebruikt een `HashSet<string>` intern.
- De blacklist-middleware staat na `app.UseAuthentication()` maar voor `app.UseAuthorization()`.
- Lees de `Authorization`-header via `context.Request.Headers["Authorization"].ToString()`. Verwijder het `"Bearer "`-prefix via `.Replace("Bearer ", string.Empty)`.

**Startcode:**

```csharp
namespace ShopWave.Api
{
    public class TokenBlacklist
    {
        private readonly HashSet<string> _revokedTokens;

        public TokenBlacklist()
        {
            _revokedTokens = new HashSet<string>();
        }

        public void Revoke(string token)
        {
            // jouw code hier
        }

        public bool IsRevoked(string token)
        {
            // jouw code hier
            return false;
        }
    }
}
```

Middleware in `Program.cs`:

```csharp
TokenBlacklist tokenBlacklist = new TokenBlacklist();

app.UseAuthentication();

app.Use(async (context, next) =>
{
    string authHeader = context.Request.Headers["Authorization"].ToString();
    string token      = authHeader.Replace("Bearer ", string.Empty);

    // jouw code hier: controleer of het token gerevoked is
    // als ja: zet statuscode op 401 en return

    await next();
});

app.UseAuthorization();
```

**Controleer je werk:**

```csharp
// Voor uitloggen: verwacht 200 OK
client.DefaultRequestHeaders.Authorization =
    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

HttpResponseMessage beforeLogout = client.GetAsync("/orders/alice@shopwave.be").Result;
Console.WriteLine($"Voor uitloggen: {beforeLogout.StatusCode}");

// Uitloggen
HttpResponseMessage logoutResponse = client.PostAsync("/logout", null).Result;
Console.WriteLine($"Uitloggen: {logoutResponse.StatusCode}");

// Na uitloggen: verwacht 401 Unauthorized
HttpResponseMessage afterLogout = client.GetAsync("/orders/alice@shopwave.be").Result;
Console.WriteLine($"Na uitloggen: {afterLogout.StatusCode}");
```

Verwacht resultaat:

```
Voor uitloggen: OK
Uitloggen: OK
Na uitloggen: Unauthorized
```

---

<h3 class="opdracht-titel">Opdracht</h3>

## Oefening 5: JWT en OAuth 2.0 koppelen aan CIA

**Leerdoel:** je verbindt de technische keuzes uit de vorige oefeningen met het CIA-model en het OAuth 2.0-protocol.

**Moeilijkheidsgraad:** basis (reflectie)

Beantwoord de volgende vragen op papier of in een tekstbestand.

1. Een JWT-signature garandeert dat de payload niet gewijzigd is. Welke CIA-pijler beschermt de signature? Kan de signature ook confidentiality garanderen? Leg uit.

2. In oefening 3 bouw je een verlopen token dat `401 Unauthorized` geeft. Welke CIA-pijler staat centraal bij het instellen van een vervaltijd? Wat zou er misgaan als tokens nooit verlopen?

3. In oefening 4 implementeer je een `TokenBlacklist`. JWT-tokens zijn van nature stateless: de server slaat niets op. De blacklist doorbreekt dat principe. Leg uit wat het nadeel is van een blacklist op het vlak van schaalbaarheid. Hoe lost een korte vervaltijd dat probleem deels op?

4. OAuth 2.0 gebruikt scopes om toegang te beperken. Een fitness-app vraagt `calendar.events.write`. Welke CIA-pijler staat hier centraal? Hoe helpt het principe van least privilege bij het ontwerpen van scopes?

5. In de JWT-flow stuurt de client het token mee in de `Authorization`-header. Als de verbinding niet via HTTPS loopt, is die header zichtbaar voor iedereen op het netwerk. Leg de rol uit van HTTPS (les 6) en JWT (les 7) samen. Wat beschermt elk van de twee?
