---
title: "Les 9: Oefeningen - Secure Coding (OWASP)"
sidebar_label: "Oefeningen"
---

# Oefeningen: Secure Coding (OWASP)

Werk de oefeningen in volgorde. Elke oefening bouwt verder op de vorige. Kijk niet vooraf in de oplossingen.

Je werkt verder in de bestaande ShopWave-solution. Nieuwe klassen maak je aan in `ShopWave.Api/`.

---

## Startpakket downloaden

[Download het startpakket van les 9](/downloads/shopwave-start-09-secure-coding-owasp.zip) (ZIP)

Hierin staat alles wat je in de vorige lessen gebouwd hebt, samen met de code die je
tijdens de theorie van deze les opbouwt. Wat je in de oefeningen zelf moet schrijven,
staat erin als skelet met de melding `// jouw code hier`.

De webshop zit erbij. Je hoeft geen Razor te kennen: start hem met
`dotnet run --project ShopWave.Web` en open https://localhost:5443. Zo zie je meteen wat je code doet.

---

<h3 class="opdracht-titel">Opdracht</h3>

## Oefening 1: SQL Injection op productnaam

**Leerdoel:** je bouwt eerst een kwetsbaar endpoint, voert de aanval zelf uit en fixt het daarna. Zo begrijp je waarom string interpolatie gevaarlijk is.

**Moeilijkheidsgraad:** basis

**Situatie:** ShopWave wil klanten toestaan om te zoeken op productnaam. Je voegt een zoekendpoint toe dat orders filtert op productnaam. Je bouwt eerst de kwetsbare versie, test de aanval en fixt daarna.

**Wat je doet:**

Voeg een endpoint `/orders/zoek-product` toe aan `ShopWave.Api/Program.cs` dat:

1. In de kwetsbare versie: de productnaam via string interpolatie in een query plakt en via `Contains` filtert. Log de samengestelde query naar de console.
2. Na het testen: de veilige versie implementeert die de productnaam als losse parameter behandelt en enkel overeenkomsten in het tweede veld (productnaam) van de `orderDatabase` teruggeeft.

**Vereisten:**

- Het formaat van een rij in `orderDatabase` is: `"email|product|prijs"`.
- Gebruik `order.Split('|')` om de velden te splitsen.
- Gebruik `StringComparison.OrdinalIgnoreCase` voor de vergelijking.

**Startcode:**

```csharp
app.MapGet("/orders/zoek-product", HandleZoekProduct);

IResult HandleZoekProduct(string product)
{
    // Stap 1: kwetsbare versie
    string query = $"SELECT * FROM orders WHERE product = '{product}'";
    Console.WriteLine($"[KWETSBAAR] Query: {query}");

    List<string> results = orderDatabase
        .Where(order => order.Contains(product))
        .ToList();

    return Results.Ok(new { Query = query, Results = results });

    // Stap 2: vervang bovenstaande door de veilige versie na het testen
}
```

**Controleer je werk:**

Test 1: ga naar `https://localhost:5001/orders/zoek-product?product=Laptop`. Verwacht: enkel de Laptop-order van Alice.

Test 2: ga naar `https://localhost:5001/orders/zoek-product?product=' OR '1'='1`. Verwacht in de kwetsbare versie: alle orders. Verwacht in de veilige versie: lege lijst.

Beantwoord daarna schriftelijk:

1. Waarom maakt `OR '1'='1` de WHERE-voorwaarde altijd waar?
2. Wat doet `--` achteraan een SQL-injectie?
3. Wat is het concrete verschil tussen de kwetsbare en veilige implementatie?

---

<h3 class="opdracht-titel">Opdracht</h3>

## Oefening 2: Input validatie op login en verify

**Leerdoel:** je voegt server-side validatie toe aan bestaande endpoints en begrijpt dat client-side validatie onvoldoende is.

**Moeilijkheidsgraad:** basis

**Situatie:** de `/login`- en `/verify`-endpoints in ShopWave vertrouwen momenteel blindelings op de client. Een aanvaller kan lege strings of ongeldig gevormde codes sturen. Je voegt server-side validatie toe.

**Wat je doet:**

Breid de handlers van `/login` en `/verify` uit met validatiecontroles.

Voor `/login`:
- E-mailadres mag niet leeg zijn (`IsNullOrWhiteSpace`). Geef `400 Bad Request` terug met `{"Error": "E-mailadres is verplicht."}`.
- E-mailadres moet een `@` bevatten. Geef `400 Bad Request` terug met `{"Error": "Ongeldig e-mailadres."}`.
- Wachtwoord mag niet leeg zijn. Geef `400 Bad Request` terug met `{"Error": "Wachtwoord is verplicht."}`.

Voor `/verify`:
- E-mailadres mag niet leeg zijn.
- De 2FA-code mag niet leeg zijn.
- De 2FA-code moet exact 6 tekens lang zijn. Geef `400 Bad Request` terug met `{"Error": "2FA-code moet exact 6 cijfers bevatten."}`.
- De 2FA-code mag enkel cijfers bevatten. Gebruik `code.All(char.IsDigit)`.

**Vereisten:**

- Elke validatiecontrole staat in een apart `if`-blok met een eigen `return`.
- Gebruik `string.IsNullOrWhiteSpace(...)` voor lege-string-controles.
- Geen ternary-operatoren.

**Startcode:**

```csharp
IResult HandleLogin(LoginRequest request)
{
    // jouw validatie hier

    string result = accountRepository.Login(request.Email, request.Password);
    return Results.Ok(new { Status = result });
}

IResult HandleVerify(VerifyRequest request)
{
    // jouw validatie hier

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

**Controleer je werk:** test elk foutgeval vanuit de console:

```csharp
// Leeg e-mailadres
string emptyEmail = JsonSerializer.Serialize(new { email = "", password = "wachtwoord123" });
HttpResponseMessage r1 = client.PostAsync("/login",
    new StringContent(emptyEmail, Encoding.UTF8, "application/json")).Result;
Console.WriteLine($"Leeg e-mail: {r1.StatusCode}");

// Ongeldige 2FA-code (5 cijfers)
string shortCode = JsonSerializer.Serialize(new { email = "alice@shopwave.be", code = "12345" });
HttpResponseMessage r2 = client.PostAsync("/verify",
    new StringContent(shortCode, Encoding.UTF8, "application/json")).Result;
Console.WriteLine($"Korte code: {r2.StatusCode}");

// Niet-numerieke 2FA-code
string alphaCode = JsonSerializer.Serialize(new { email = "alice@shopwave.be", code = "abc123" });
HttpResponseMessage r3 = client.PostAsync("/verify",
    new StringContent(alphaCode, Encoding.UTF8, "application/json")).Result;
Console.WriteLine($"Niet-numeriek: {r3.StatusCode}");
```

Verwacht resultaat:

```csharp
Leeg e-mail: BadRequest
Korte code: BadRequest
Niet-numeriek: BadRequest
```

---

<h3 class="opdracht-titel">Opdracht</h3>

## Oefening 3: Rate limiting op het login-endpoint

**Leerdoel:** je implementeert rate limiting en begrijpt hoe het een brute-force aanval vertraagt.

**Moeilijkheidsgraad:** gemiddeld

**Situatie:** een aanvaller probeert het wachtwoord van `alice@shopwave.be` te raden via een geautomatiseerd script. Zonder rate limiting kan hij honderden pogingen per minuut doen. Je voegt een fixed window limiter toe die maximaal 5 loginpogingen per minuut toestaat per IP-adres.

**Wat je doet:**

1. Voeg `AddRateLimiter` toe aan de builder met een limiet van 5 requests per minuut voor de policy `"login"`.
2. Activeer `UseRateLimiter()` in de pipeline.
3. Koppel de policy aan het `/login`-endpoint via `.RequireRateLimiting("login")`.
4. Schrijf een consolemethode `DemoBruteForce()` die 7 loginpogingen na elkaar stuurt en de statuscode van elk afdrukt.

**Vereisten:**

- `QueueLimit` is 0: requests die de limiet overschrijden, worden onmiddellijk geweigerd, niet in de wachtrij geplaatst.
- `QueueProcessingOrder` is `OldestFirst`.
- De consolemethode gebruikt een eigen `HttpClient`.

**Startcode:**

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
        // jouw code hier: stuur een loginpoging en druk de statuscode af
    }

    client.Dispose();
    handler.Dispose();
}
```

**Controleer je werk:** verwacht resultaat:

```csharp
=== Brute-force simulatie ===
Poging 1: OK
Poging 2: OK
Poging 3: OK
Poging 4: OK
Poging 5: OK
Poging 6: TooManyRequests
Poging 7: TooManyRequests
```

---

<h3 class="opdracht-titel">Opdracht</h3>

## Oefening 4: CORS correct configureren

**Leerdoel:** je implementeert een CORS-policy en begrijpt het verschil tussen een open en een gesloten configuratie.

**Moeilijkheidsgraad:** gemiddeld

**Situatie:** ShopWave krijgt een frontend op `https://shopwave.be`. Die frontend moet de API kunnen aanroepen. Alle andere origins moeten geweigerd worden. Je configureert CORS zodat enkel de ShopWave-frontend toegang heeft.

**Wat je doet:**

1. Voeg `AddCors` toe aan de builder met een policy `"ShopWavePolicy"` die enkel `"https://shopwave.be"` en `"https://localhost:3000"` toestaat.
2. Activeer `UseCors("ShopWavePolicy")` in de pipeline, na `app.UseAuthentication()`.
3. Maak een klasse `CorsValidator` in `ShopWave/Security/CorsValidator.cs` met een methode `SimulateRequest(string origin)` die simuleert of een origin toegestaan is of niet. De methode geeft `true` terug als de origin in een vaste lijst staat en `false` anders.

**Vereisten voor `CorsValidator`:**

- De toegestane origins staan als `private readonly List<string>` in de klasse.
- De constructor vult die lijst.
- `SimulateRequest` gebruikt `allowedOrigins.Contains(origin)`.

**Startcode:**

```csharp
namespace ShopWave.Security
{
    public class CorsValidator
    {
        private readonly List<string> allowedOrigins;

        public CorsValidator()
        {
            allowedOrigins = new List<string>
            {
                "https://shopwave.be",
                "https://localhost:3000"
            };
        }

        public bool SimulateRequest(string origin)
        {
            // jouw code hier
            return false;
        }
    }
}
```

**Controleer je werk:** voeg tijdelijk toe aan `ShopWave/Program.cs`:

```csharp
CorsValidator validator = new CorsValidator();

Console.WriteLine($"shopwave.be:   {validator.SimulateRequest("https://shopwave.be")}");
Console.WriteLine($"aanvaller.be:  {validator.SimulateRequest("https://aanvaller.be")}");
Console.WriteLine($"localhost:3000:{validator.SimulateRequest("https://localhost:3000")}");
```

Verwacht resultaat:

```csharp
shopwave.be:   True
aanvaller.be:  False
localhost:3000:True
```

---

<h3 class="opdracht-titel">Opdracht</h3>

## Oefening 5: OWASP-analyse van een incident

**Leerdoel:** je koppelt concrete bevindingen aan OWASP-kwetsbaarheden, het CIA-model en concrete maatregelen.

**Moeilijkheidsgraad:** basis (reflectie)

**Situatie:** ShopWave is een maand live. Een beveiligingsonderzoeker meldt vier bevindingen.

Beantwoord per bevinding: welke OWASP-kwetsbaarheid is dit, welke CIA-pijler wordt geschonden, welke concrete maatregel had dit voorkomen en wat is de potentiële impact voor ShopWave en haar klanten?

**Bevinding 1:**

Via het zoekendpoint kon de onderzoeker alle orders van alle klanten opvragen door `' OR '1'='1` als e-mailadres te sturen.

**Bevinding 2:**

Via `/swagger` vond de onderzoeker een endpoint `/admin/stats` dat nergens gedocumenteerd was en toegankelijk was zonder authenticatie.

**Bevinding 3:**

De onderzoeker vond de JWT-sleutel `ShopWaveGeheimeSleutel2024!!XYZ#` terug in een commit van drie weken geleden in de publieke GitHub-repository. Iemand had de sleutel hardcoded ingesteld en daarna verwijderd, maar de git-geschiedenis bewaart alles.

**Bevinding 4:**

Via het `/crash`-endpoint kon de onderzoeker een week eerder de volledige databaseconnectiestring lezen, inclusief het IP-adres, de gebruikersnaam en het wachtwoord van de databasegebruiker.

---

## Controleer je werk in de webshop

Start de webshop met `dotnet run --project ShopWave.Web` en open https://localhost:5443. Zo zie je je eigen code draaien in plaats van alleen een groene testbalk.

| Wat je doet | Wat je ziet als je code klopt |
|-------------|-------------------------------|
| Ga naar **Zoeken** en zoek veilig op `alice@shopwave.be` | Alleen de orders van Alice |
| Zoek naïef op `alice@shopwave.be` | Hetzelfde resultaat. Zo lijkt de naïeve versie in orde. |
| Zoek naïef op `@shopwave.be` | **Alle** orders, ook die van de beheerder. Dat is het lek. |
| Zoek veilig op `@shopwave.be` | Niets, want er is geen account met dat adres |

Onder elk resultaat staat uit welke klasse het komt. Zie je iets anders dan hierboven, dan weet je meteen welke methode je moet nakijken.
