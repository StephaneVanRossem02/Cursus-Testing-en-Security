---
title: "Les 8: Oplossingen - Secure Coding (OWASP)"
sidebar_label: "Oplossingen"
---

# Oplossingen: Secure Coding (OWASP)

**Bekijk dit pas nadat je de oefeningen zelf geprobeerd hebt.** Lees de toelichting ook als je het juist had.

---

## Oplossing 1: SQL Injection op productnaam

### Kwetsbare versie (ter demonstratie)

```csharp
app.MapGet("/orders/zoek-product", HandleZoekProduct);

IResult HandleZoekProduct(string product)
{
    string query = $"SELECT * FROM orders WHERE product = '{product}'";
    Console.WriteLine($"[KWETSBAAR] Query: {query}");

    List<string> results = orderDatabase
        .Where(order => order.Contains(product))
        .ToList();

    return Results.Ok(new { Query = query, Results = results });
}
```

### Veilige versie

```csharp
IResult HandleZoekProduct(string product)
{
    Console.WriteLine($"[VEILIG] Zoeken op product: {product}");

    List<string> results = orderDatabase
        .Where(order =>
        {
            string[] fields  = order.Split('|');
            bool     matched = false;

            if (fields.Length >= 2)
            {
                matched = fields[1].Equals(product, StringComparison.OrdinalIgnoreCase);
            }

            return matched;
        })
        .ToList();

    return Results.Ok(new { Results = results });
}
```

### Toelichting

De kwetsbare versie gebruikt `.Contains(product)`. Als de aanvaller `' OR '1'='1` invoert, bevat elke rij die string niet letterlijk, maar de gesimuleerde query toont wat er in een echte database zou gebeuren: de WHERE-voorwaarde wordt altijd waar omdat `'1'='1'` altijd evalueert naar `true`.

De veilige versie splitst de rij op `|` en vergelijkt enkel het tweede veld (productnaam) exact met de zoekopdracht. De string `' OR '1'='1` is geen geldige productnaam en geeft dus geen resultaten.

In een echte SQL-database gebruik je:

```csharp
SqlCommand command = new SqlCommand(
    "SELECT * FROM orders WHERE product = @product", connection);
command.Parameters.AddWithValue("@product", product);
```

De database ontvangt de query-structuur en de parameter als twee aparte berichten. Ze kan `@product` nooit interpreteren als SQL-code.

**Antwoorden op de reflectievragen:**

1. `OR '1'='1'` voegt een extra voorwaarde toe die altijd waar is. De WHERE-clausule wordt: `email = '' OR true`. Omdat `OR true` de hele expressie waar maakt, geeft de query alle rijen terug.

2. `--` is het SQL-commentaarteken. Alles na `--` wordt genegeerd door de database. Een aanvaller gebruikt dit om de rest van de originele query te neutraliseren: `'; DROP TABLE orders --` sluit de eerste query af met `;`, voert een tweede query uit en commentarieert de afsluitende aanhalingsteken weg.

3. De kwetsbare versie construeert een string waarbij de gebruikersinput deel uitmaakt van de query-structuur. De veilige versie houdt data en query-structuur volledig gescheiden.

**Veelgemaakte fout:** studenten denken dat `Replace("'", "''")` voldoende is. Dat werkt niet betrouwbaar. Er bestaan aanvalsvarianten die geen aanhalingstekens gebruiken, en het escapen is foutgevoelig. Gebruik altijd parameterized queries.

---

## Oplossing 2: Input validatie op login en verify

### ShopWave.Api/Program.cs

```csharp
IResult HandleLogin(LoginRequest request)
{
    if (string.IsNullOrWhiteSpace(request.Email))
    {
        return Results.BadRequest(new { Error = "E-mailadres is verplicht." });
    }

    if (!request.Email.Contains("@"))
    {
        return Results.BadRequest(new { Error = "Ongeldig e-mailadres." });
    }

    if (string.IsNullOrWhiteSpace(request.Password))
    {
        return Results.BadRequest(new { Error = "Wachtwoord is verplicht." });
    }

    string result = accountRepository.Login(request.Email, request.Password);
    return Results.Ok(new { Status = result });
}

IResult HandleVerify(VerifyRequest request)
{
    if (string.IsNullOrWhiteSpace(request.Email))
    {
        return Results.BadRequest(new { Error = "E-mailadres is verplicht." });
    }

    if (string.IsNullOrWhiteSpace(request.Code))
    {
        return Results.BadRequest(new { Error = "2FA-code is verplicht." });
    }

    if (request.Code.Length != 6)
    {
        return Results.BadRequest(new { Error = "2FA-code moet exact 6 cijfers bevatten." });
    }

    if (!request.Code.All(char.IsDigit))
    {
        return Results.BadRequest(new { Error = "2FA-code moet enkel cijfers bevatten." });
    }

    string result = accountRepository.VerifyTwoFactor(request.Email, request.Code);

    if (result != "Inloggen geslaagd.")
    {
        return Results.Unauthorized();
    }

    string role  = DetermineRole(request.Email);
    string token = jwtTokenService.GenerateToken(request.Email, role);

    return Results.Ok(new { Token = token });
}
```

### Toelichting

`string.IsNullOrWhiteSpace` vangt drie gevallen op: `null`, een lege string `""` en een string die enkel spaties bevat `"   "`. Een aanvaller die enkel spaties stuurt als e-mailadres, wordt ook afgewezen.

`request.Code.All(char.IsDigit)` controleert of elk teken in de string een cijfer is. `char.IsDigit` is een methodengroep die je direct als argument aan `All` kan meegeven zonder een lambda. Als de code `"12a456"` is, geeft `All(char.IsDigit)` `false` terug omdat `'a'` geen cijfer is.

De validatiecontroles staan in volgorde van meest voor de hand liggend naar meest specifiek. Eerst leeg, daarna formaat, daarna lengte, daarna inhoud. Die volgorde maakt de foutmeldingen begrijpelijker voor de gebruiker.

**Veelgemaakte fout:** studenten controleren de lengte voor de leeg-check. Als `request.Code` `null` is, gooit `request.Code.Length` een `NullReferenceException`. Controleer altijd eerst op `null` of `IsNullOrWhiteSpace`.

---

## Oplossing 3: Rate limiting op het login-endpoint

### ShopWave.Api/Program.cs

```csharp
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("login", limiterOptions =>
    {
        limiterOptions.Window      = TimeSpan.FromMinutes(1);
        limiterOptions.PermitLimit = 5;
        limiterOptions.QueueLimit  = 0;
        limiterOptions.QueueProcessingOrder =
            System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst;
    });
});
```

In de pipeline:

```csharp
app.UseRateLimiter();
```

Op het endpoint:

```csharp
app.MapPost("/login", HandleLogin)
   .RequireRateLimiting("login");
```

### ShopWave/Program.cs

```csharp
void DemoBruteForce()
{
    HttpClientHandler handler = new HttpClientHandler();
    handler.ServerCertificateCustomValidationCallback =
        (message, certificate, chain, errors) => true;

    HttpClient client = new HttpClient(handler);
    client.BaseAddress = new Uri("https://localhost:5001");

    Console.WriteLine("=== Brute-force simulatie ===");

    for (int attempt = 1; attempt <= 7; attempt++)
    {
        string payload = JsonSerializer.Serialize(
            new { email = "alice@shopwave.be", password = $"poging{attempt}" });

        HttpResponseMessage response = client.PostAsync("/login",
            new StringContent(payload, Encoding.UTF8, "application/json")).Result;

        Console.WriteLine($"Poging {attempt}: {response.StatusCode}");
    }

    client.Dispose();
    handler.Dispose();
}
```

### Toelichting

`PermitLimit = 5` staat vijf requests per minuut toe per fixed window. Na de vijfde request geeft de server `429 Too Many Requests` terug tot het venster van één minuut voorbij is.

`QueueLimit = 0` zorgt dat geweigerde requests onmiddellijk een `429` krijgen. Als `QueueLimit` groter is, wachten de requests in een wachtrij tot er capaciteit vrijkomt. Voor een loginendpoint wil je geen wachtrij: de aanvaller ervaart dan geen echte vertraging.

Rate limiting werkt per IP-adres in de standaardconfiguratie van ASP.NET Core. Dat is voldoende voor eenvoudige aanvallen. Een distributed brute-force aanval via 1000 IP-adressen omzeilt dit. Verdere maatregelen zijn: account lockout na een vast aantal pogingen (zoals `TwoFactorService` al doet), CAPTCHA en anomaly detection.

**Veelgemaakte fout:** studenten vergeten `app.UseRateLimiter()` toe te voegen aan de pipeline. De configuratie via `AddRateLimiter` registreert enkel de service. De middleware wordt pas actief via `UseRateLimiter()`.

---

## Oplossing 4: CORS correct configureren

### ShopWave.Api/Program.cs

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("ShopWavePolicy", policy =>
    {
        policy.WithOrigins("https://shopwave.be", "https://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});
```

In de pipeline, na `app.UseAuthentication()`:

```csharp
app.UseCors("ShopWavePolicy");
```

### ShopWave/Security/CorsValidator.cs

```csharp
namespace ShopWave.Security
{
    public class CorsValidator
    {
        private readonly List<string> _allowedOrigins;

        public CorsValidator()
        {
            _allowedOrigins = new List<string>
            {
                "https://shopwave.be",
                "https://localhost:3000"
            };
        }

        public bool SimulateRequest(string origin)
        {
            return _allowedOrigins.Contains(origin);
        }
    }
}
```

### Toelichting

`WithOrigins(...)` accepteert een of meer strings als toegestane origins. De origin in een HTTP-request is het schema plus de domeinnaam plus het poortnummer: `https://shopwave.be` is een andere origin dan `http://shopwave.be` (ander schema) en `https://shopwave.be:8080` (ander poortnummer).

CORS is een beveiligingsmechanisme in de browser. De browser stuurt een `Origin`-header mee bij cross-origin requests en controleert de response. Als de response geen `Access-Control-Allow-Origin`-header bevat die de origin toestaat, blokkeert de browser de response.

Een HTTP-client zoals Postman of curl stuurt geen `Origin`-header en voert geen CORS-controle uit. CORS beschermt enkel tegen aanvallen via browsers.

**Veelgemaakte fout:** studenten gebruiken `AllowAnyOrigin()` gecombineerd met `AllowCredentials()`. Die combinatie is niet toegestaan door de browser-specificatie en gooit een runtime exception. Als je credentials (cookies of Authorization-header) wil toestaan, moet je een expliciete lijst van origins opgeven.

---

## Oplossing 5: OWASP-analyse van een incident

**Bevinding 1: SQL Injection via zoekendpoint**

- OWASP: A03 Injection
- CIA-pijler: Confidentiality. De aanvaller leest gegevens die hij niet mag zien.
- Maatregel: parameterized queries. De input mag nooit deel uitmaken van de query-structuur.
- Impact: alle orders van alle klanten zijn gelekt, inclusief e-mailadressen en aankoopgeschiedenissen. GDPR-melding verplicht binnen 72 uur. Mogelijke boete tot 4% van de jaaromzet.

**Bevinding 2: Swagger publiek beschikbaar**

- OWASP: A05 Security Misconfiguration
- CIA-pijler: Confidentiality. De aanvaller kan de volledige API-structuur zien inclusief endpoints die niet bedoeld zijn voor publiek gebruik.
- Maatregel: Swagger beperken tot development via `if (app.Environment.IsDevelopment())`. Het `/admin/stats`-endpoint beveiligen met `.RequireAuthorization(policy => policy.RequireRole("admin"))`.
- Impact: de aanvaller kent alle endpoints en hun parameters. Hij kan gericht aanvallen plannen op endpoints die hij anders niet kende.

**Bevinding 3: JWT-sleutel in git-geschiedenis**

- OWASP: A05 Security Misconfiguration, A07 Auth and Session Failures
- CIA-pijler: Confidentiality en Integrity. Met de sleutel kan de aanvaller geldige JWT-tokens aanmaken voor elke gebruiker, inclusief admins.
- Maatregel: geheimen nooit hardcoden in broncode. Gebruik omgevingsvariabelen. Als een sleutel eenmaal in de git-geschiedenis staat, moet je hem roteren: een nieuwe sleutel genereren en alle bestaande tokens invalideren door de uitgifte-sleutel te wijzigen.
- Impact: de aanvaller kan tokens aanmaken voor `admin@shopwave.be` en alle bestellingen, klantgegevens en adminfuncties benaderen zonder in te loggen.

**Bevinding 4: Developer Exception Page in productie**

- OWASP: A05 Security Misconfiguration
- CIA-pijler: Confidentiality. De aanvaller leest interne systeemgegevens die bedoeld zijn voor ontwikkelaars.
- Maatregel: `UseDeveloperExceptionPage()` enkel in development. In productie een generieke foutpagina zonder interne details.
- Impact: de aanvaller heeft het IP-adres, de gebruikersnaam en het wachtwoord van de databaseserver. Hij kan rechtstreeks verbinding maken met de database, buiten de applicatie om, en alle data lezen, wijzigen of vernietigen.
