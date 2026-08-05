---
title: "Les 11: Oplossingen - Ethisch Hacken"
sidebar_label: "Oplossingen"
---

# Oplossingen: Ethisch Hacken

**Bekijk dit pas nadat je de oefeningen zelf geprobeerd hebt.** Lees de toelichting ook als je het juist had.

---

## Oplossing 1: JWT-manipulatie in C#

```csharp
void TryRoleManipulation(string validToken)
{
    Console.WriteLine("=== JWT-rolmanipulatie poging ===");

    string[] parts   = validToken.Split('.');
    string   payload = parts[1];

    // Padding herstellen voor Base64-decodering
    int padLength = 4 - (payload.Length % 4);
    if (padLength != 4)
    {
        payload += new string('=', padLength);
    }

    // Base64url -> Base64
    payload = payload.Replace('-', '+').Replace('_', '/');

    // Decoderen
    string decodedPayload = Encoding.UTF8.GetString(Convert.FromBase64String(payload));
    Console.WriteLine($"Originele payload: {decodedPayload}");

    // Payload aanpassen
    string manipulatedPayload = decodedPayload.Replace("\"role\":\"user\"", "\"role\":\"admin\"");
    Console.WriteLine($"Aangepaste payload: {manipulatedPayload}");

    // Hercoderen naar Base64url
    byte[] manipulatedBytes = Encoding.UTF8.GetBytes(manipulatedPayload);
    string reEncodedPayload = Convert.ToBase64String(manipulatedBytes)
        .Replace('+', '-').Replace('/', '_').TrimEnd('=');

    // Token samenstellen met originele header en signature maar aangepaste payload
    string manipulatedToken = $"{parts[0]}.{reEncodedPayload}.{parts[2]}";

    // Versturen
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
}
```

### Toelichting

De aanval mislukt op stap 7. De server valideert de JWT door de signature opnieuw te berekenen over `header.payload` met de geheime sleutel. De herberekende signature is de signature die zou horen bij de originele payload. De aanvaller heeft de payload aangepast maar de originele signature gelaten. Die twee komen niet overeen en de server weigert het token.

**Antwoorden op de reflectievragen:**

1. De aanvaller heeft de geheime sleutel niet. De HMAC-SHA256-signature is een eenrichtingsfunctie waarbij de uitvoer afhankelijk is van zowel de inputdata (header+payload) als de geheime sleutel. Zonder die sleutel kan hij de correcte signature voor de gemanipuleerde payload niet berekenen.

2. De aanvaller zou de geheime sleutel moeten kennen. Dat kan als de sleutel hardcoded in de broncode staat en uitgelekt is via git-geschiedenis (OWASP A05), via een stack trace (OWASP A05) of via een directe aanval op de server.

**Veelgemaakte fout:** studenten vergeten de Base64url-naar-Base64-conversie (`-` naar `+`, `_` naar `/`). Zonder die conversie gooit `Convert.FromBase64String` een `FormatException`.

---

## Oplossing 2: `alg:none`-aanval simuleren

```csharp
void TryAlgNoneAttack()
{
    Console.WriteLine("=== alg:none aanval ===");

    // Header coderen
    string header = Convert.ToBase64String(
        Encoding.UTF8.GetBytes("{\"alg\":\"none\",\"typ\":\"JWT\"}"))
        .Replace('+', '-').Replace('/', '_').TrimEnd('=');

    // Payload coderen
    string payload = Convert.ToBase64String(
        Encoding.UTF8.GetBytes("{\"sub\":\"admin@shopwave.be\",\"role\":\"admin\"}"))
        .Replace('+', '-').Replace('/', '_').TrimEnd('=');

    // Token samenstellen zonder signature
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
}
```

### Toelichting

**Antwoorden op de reflectievragen:**

1. Sommige vroege JWT-bibliotheken interpreteerden `alg: none` letterlijk: "geen algoritme, dus geen signature te valideren, dus het token is geldig." De aanvaller kan zo een token aanmaken met elke payload en elk algoritme zonder geheime sleutel.

2. De .NET `JwtBearerAuthentication`-middleware vergelijkt het algoritme in de header met de lijst van toegestane algoritmen in `TokenValidationParameters`. Als de `AddAuthentication`-configuratie enkel `SecurityAlgorithms.HmacSha256` toestaat, wordt elk token met een ander algoritme geweigerd. `alg: none` staat niet in die lijst en wordt onmiddellijk afgewezen.

**Veelgemaakte fout:** studenten vergeten de punt aan het einde van het token: `header.payload.`. De JWT-standaard vereist drie delen gescheiden door punten. Een token zonder derde punt is geen geldig JWT-formaat.

---

## Oplossing 3: Informatielekkage in development vs productie

### ShopWave.Api/Program.cs

```csharp
app.MapGet("/crash", () =>
{
    throw new InvalidOperationException("Gesimuleerde interne fout.");
});
```

### Toelichting

**Antwoorden op de reflectievragen:**

1. In development toont de Developer Exception Page:
   - De volledige stack trace met bestandspaden op de server (bv. `C:\Users\student\ShopWave\ShopWave.Api\Program.cs:42`)
   - De naam en versie van de middleware die de fout heeft opgegangen
   - De querystring, headers en cookies van het request dat de fout veroorzaakte
   - Interne variabelenamen en typenamen van het .NET-runtime

2. In productie geeft de server een generieke HTTP 500-response terug. De body bevat enkel een korte, niet-technische melding: `Er is een fout opgetreden.` Geen stack trace, geen bestandspaden, geen interne details.

3. Dit is OWASP A05 Security Misconfiguration. Een aanvaller gebruikt de stack trace om het aanvalsoppervlak in kaart te brengen: welk framework, welke versie, welke methodes worden aangeroepen, welke externe systemen zijn verbonden. Die informatie verkort de verkenningsfase van een pentest aanzienlijk en maakt gerichte aanvallen mogelijk op bekende kwetsbaarheden in de gelekte bibliotheekversies.

**Veelgemaakte fout:** studenten vergeten de omgevingsvariabele te zetten voor ze `dotnet run` uitvoeren. Als je `$env:ASPNETCORE_ENVIRONMENT = "Production"` instelt na het starten van de API, heeft dat geen effect. De variabele moet voor het starten gezet worden.

---

## Oplossing 4: PentestReport-klasse implementeren

### ShopWave/Security/PentestReport.cs

```csharp
namespace ShopWave.Security
{
    public class Finding
    {
        public string Id             { get; set; } = "";
        public string Title          { get; set; } = "";
        public string Risk           { get; set; } = "";
        public double CvssScore      { get; set; }
        public string Description    { get; set; } = "";
        public string Evidence       { get; set; } = "";
        public string Recommendation { get; set; } = "";
        public string Status         { get; set; } = "";
    }

    public class PentestReport
    {
        private readonly List<Finding> findings;

        public PentestReport()
        {
            findings = new List<Finding>();
        }

        public void AddFinding(Finding finding)
        {
            findings.Add(finding);
        }

        public List<Finding> GetByRisk(string risk)
        {
            return findings
                .Where(f => f.Risk == risk)
                .ToList();
        }

        public List<Finding> GetOpenFindings()
        {
            return findings
                .Where(f => f.Status == "Open")
                .ToList();
        }

        public void PrintSummary()
        {
            foreach (Finding f in findings)
            {
                Console.WriteLine($"[{f.Id}] {f.Risk} ({f.CvssScore}) - {f.Title} [{f.Status}]");
            }
        }
    }
}
```

### Toelichting

`Where` is een LINQ-methode die een predikaat (een functie die `bool` teruggeeft) accepteert en alle elementen teruggeeft waarvoor het predikaat `true` is. `ToList()` materialiseert het resultaat als een concrete `List<Finding>`. Zonder `ToList()` heb je een `IEnumerable<Finding>`, wat lazy evalueert. In dit geval maakt het geen functioneel verschil, maar `ToList()` maakt de return-types consistent.

`private readonly List<Finding>` garandeert dat de referentie naar de lijst niet vervangen kan worden na constructie. De lijst zelf kan nog wel groeien via `Add`. `readonly` beschermt de referentie, niet de inhoud.

**Veelgemaakte fout:** studenten gebruiken `f.Risk.Equals(risk)` in `GetByRisk`. Dat werkt, maar `==` is voldoende voor strings in C# via de overloaded equality operator. Een subtielere fout: studenten vergeten `StringComparison.OrdinalIgnoreCase`. Als de methode wordt aangeroepen met `"high"` in plaats van `"High"`, geeft `==` zonder case-insensitivity een lege lijst terug. De startcode specificeert `"High"` met hoofdletter, dus in dit geval is het correct, maar in een echte applicatie zou je case-insensitieve vergelijking toevoegen.

---

## Oplossing 5: Volledig pentestreport schrijven

Hieronder een voorbeeldoplossing. Jouw bevindingen kunnen variëren op basis van de configuratie van je ShopWave-instantie.

### ShopWave/Program.cs

```csharp
using ShopWave.Security;

PentestReport report = new PentestReport();

// Test 1: verkenning
// curl.exe -k https://localhost:5001/swagger -> 200 of 404
// Als 200: Swagger is publiek toegankelijk
report.AddFinding(new Finding
{
    Id             = "FINDING-01",
    Title          = "Swagger UI publiek toegankelijk",
    Risk           = "Informational",
    CvssScore      = 2.1,
    Description    = "De Swagger UI toont alle beschikbare endpoints inclusief beveiligde admin-endpoints. Een aanvaller kan de volledige API-structuur in kaart brengen zonder authenticatie.",
    Evidence       = "GET /swagger -> 200 OK. Response bevat /admin/orders en /admin/stats.",
    Recommendation = "Swagger beperken tot development: if (app.Environment.IsDevelopment()) { app.UseSwagger(); }",
    Status         = "Open"
});

// Test 2: JWT-rolmanipulatie
// TryRoleManipulation(aliceToken) -> Unauthorized
report.AddFinding(new Finding
{
    Id             = "FINDING-02",
    Title          = "JWT-rolmanipulatie correct geblokkeerd",
    Risk           = "Informational",
    CvssScore      = 0.0,
    Description    = "Poging om de role-claim in een geldig JWT-token te wijzigen van 'user' naar 'admin' mislukt. De signature-validatie detecteert de aanpassing.",
    Evidence       = "Gemanipuleerd token gestuurd naar GET /admin/orders -> 401 Unauthorized.",
    Recommendation = "Geen actie vereist. Signature-validatie werkt correct.",
    Status         = "Closed"
});

// Test 3: brute-force
// Poging 6 -> 429 als rate limiting actief is
report.AddFinding(new Finding
{
    Id             = "FINDING-03",
    Title          = "Rate limiting actief op login-endpoint",
    Risk           = "Informational",
    CvssScore      = 0.0,
    Description    = "Na 5 loginpogingen geeft het endpoint 429 Too Many Requests terug. Brute-force aanvallen worden vertraagd.",
    Evidence       = "Poging 1-5: 200 OK / 401 Unauthorized. Poging 6-7: 429 Too Many Requests.",
    Recommendation = "Geen actie vereist. Overweeg account lockout te combineren met rate limiting voor extra bescherming.",
    Status         = "Closed"
});

// Test 4: CORS
// Access-Control-Allow-Origin: * -> bevinding, aanwezig zonder origin -> correct
report.AddFinding(new Finding
{
    Id             = "FINDING-04",
    Title          = "CORS correct geconfigureerd",
    Risk           = "Informational",
    CvssScore      = 0.0,
    Description    = "Request met Origin: https://aanvaller.be geeft geen Access-Control-Allow-Origin-header terug. CORS-configuratie weigert onbekende origins correct.",
    Evidence       = "curl -H 'Origin: https://aanvaller.be' -> geen Access-Control-Allow-Origin header in response.",
    Recommendation = "Geen actie vereist.",
    Status         = "Closed"
});

// Test 5: informatielekkage
// In development: stacktrace zichtbaar -> bevinding
report.AddFinding(new Finding
{
    Id             = "FINDING-05",
    Title          = "Developer Exception Page actief in development",
    Risk           = "Medium",
    CvssScore      = 5.3,
    Description    = "In development toont het /crash-endpoint de volledige stack trace met bestandspaden. Dit is correct voor development maar moet geblokkeerd zijn in productie.",
    Evidence       = "GET /crash (Development) -> 500 met stack trace. GET /crash (Production) -> 500 met generieke foutmelding.",
    Recommendation = "Verifieer dat ASPNETCORE_ENVIRONMENT=Production is op de productieserver. Gebruik een monitoring-service zoals Application Insights voor stack traces in productie.",
    Status         = "Open"
});

Console.WriteLine("=== ShopWave Pentestreport ===");
report.PrintSummary();

Console.WriteLine("\n=== Open bevindingen ===");
foreach (Finding f in report.GetOpenFindings())
{
    Console.WriteLine($"  - {f.Id}: {f.Title}");
}
```

### Toelichting

**Antwoorden op de reflectievragen:**

1. `FINDING-05` (Developer Exception Page) heeft de hoogste CVSS-score van de echte bevindingen (5.3). Informatielekkage van stack traces, bestandspaden en runtime-details geeft een aanvaller een gedetailleerde kaart van het systeem. Dit verlaagt de drempel voor volgende aanvallen aanzienlijk.

2. Correct geconfigureerde maatregelen in ShopWave:
   - JWT signature-validatie blokkeert rolmanipulatie
   - Rate limiting blokkeert brute-force op het login-endpoint na 5 pogingen
   - CORS weigert requests van onbekende origins
   - 2FA voegt een tweede factor toe aan authenticatie

3. `FINDING-05` heeft de hoogste prioriteit. Als de Developer Exception Page actief is in productie, lekt de API databaseconnectiestrings, bestandspaden en bibliotheekinformatie. Een aanvaller gebruikt die gegevens om directe toegang te krijgen tot de database. Dat is een volledige compromittering van alle klantdata.

**Veelgemaakte fout:** studenten classificeren bevindingen waarvan de maatregel al correct is als `"High"`. Een bevinding beschrijft een risico of test. Als de test bewijst dat het systeem correct werkt, is de status `"Closed"` en het risiconiveau `"Informational"` of `0.0` CVSS.

---

## Dit project downloaden

[Download het volledige ShopWave-project van les 11](/downloads/shopwave-11-ethisch-hacken.zip) (ZIP)

Bevat alle code tot en met deze les, klaar om te openen in Visual Studio. Bouwen en testen doe je met `dotnet build` en `dotnet test`. In de `README.md` staat wat er nieuw is en hoeveel tests er horen te slagen.

Alle lessen samen vind je op [Oplossingen downloaden](../../oplossingen-downloaden.md).
