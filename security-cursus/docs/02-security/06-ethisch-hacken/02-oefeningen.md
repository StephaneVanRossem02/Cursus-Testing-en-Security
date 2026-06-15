---
title: "Les 9: Oefeningen - Ethisch Hacken"
sidebar_label: "Oefeningen"
---

# Oefeningen: Ethisch Hacken

Werk de oefeningen in volgorde. Elke oefening bouwt verder op de vorige. Kijk niet vooraf in de oplossingen.

Je werkt verder in de bestaande ShopWave-solution. Nieuwe klassen maak je aan in `ShopWave/Security/`.

---

<h3 class="opdracht-titel">Opdracht</h3>

## Oefening 1: JWT-manipulatie in C#

**Leerdoel:** je simuleert een rolmanipulatie-aanval in code en begrijpt waarom de aanval mislukt dankzij signature-validatie.

**Moeilijkheidsgraad:** basis

**Situatie:** een beveiligingsonderzoeker wil aantonen dat ShopWave bestand is tegen JWT-rolmanipulatie. Jij schrijft de aanvalscode die bewijst dat het systeem correct valideert.

**Wat je doet:**

Maak een methode `TryRoleManipulation(string validToken)` in `ShopWave/Program.cs` die:

1. Het token opsplitst op `.` en de payload decodeert van Base64url naar een JSON-string.
2. De waarde `"role":"user"` vervangt door `"role":"admin"` in de gedecodeerde payload.
3. De aangepaste payload herencodeert naar Base64url.
4. Een nieuw token construeert met de originele header, de aangepaste payload en de originele signature.
5. Dat gemanipuleerde token stuurt naar `GET /admin/orders` via `HttpClient`.
6. De statuscode afdrukt en uitlegt wat er verwacht wordt.

**Vereisten:**

- Gebruik `token.Split('.')` om de drie delen te scheiden.
- Herstel de Base64url-padding: vervang `-` door `+`, `_` door `/`, voeg `=`-tekens toe als `payload.Length % 4 != 0`.
- Gebruik `HttpClientHandler` met `ServerCertificateCustomValidationCallback = (...) => true` voor het self-signed certificaat.
- Zet de `Authorization`-header als `Bearer {token}`.

**Startcode:**

```csharp
void TryRoleManipulation(string validToken)
{
    Console.WriteLine("=== JWT-rolmanipulatie poging ===");

    string[] parts   = validToken.Split('.');
    string   payload = parts[1];

    // Stap 1: padding herstellen
    // jouw code hier

    // Stap 2: Base64url -> Base64
    // jouw code hier

    // Stap 3: decoderen
    string decodedPayload = "";
    // jouw code hier

    Console.WriteLine($"Originele payload: {decodedPayload}");

    // Stap 4: payload aanpassen
    string manipulatedPayload = decodedPayload.Replace("\"role\":\"user\"", "\"role\":\"admin\"");
    Console.WriteLine($"Aangepaste payload: {manipulatedPayload}");

    // Stap 5: hercoderen naar Base64url
    string reEncodedPayload = "";
    // jouw code hier

    // Stap 6: token samenstellen
    string manipulatedToken = $"{parts[0]}.{reEncodedPayload}.{parts[2]}";

    // Stap 7: versturen
    // jouw code hier

    Console.WriteLine("Verwacht: Unauthorized (signature klopt niet meer)");
}
```

**Controleer je werk:** verwacht resultaat:

```csharp
=== JWT-rolmanipulatie poging ===
Originele payload: {"sub":"alice@shopwave.be","role":"user","iat":...}
Aangepaste payload: {"sub":"alice@shopwave.be","role":"admin","iat":...}
Resultaat met gemanipuleerd token: Unauthorized
Verwacht: Unauthorized (signature klopt niet meer)
```

Beantwoord daarna schriftelijk:

1. Waarom kan de aanvaller de signature niet opnieuw berekenen?
2. Wat zou er nodig zijn om de aanval toch te laten slagen?

---

<h3 class="opdracht-titel">Opdracht</h3>

## Oefening 2: `alg:none`-aanval simuleren

**Leerdoel:** je bouwt een `alg:none`-aanval en bevestigt dat .NET deze standaard weigert.

**Moeilijkheidsgraad:** basis

**Situatie:** de `alg:none`-aanval is een klassieke JWT-aanval. Jij construeert het token handmatig en test of ShopWave correct weigert.

**Wat je doet:**

Maak een methode `TryAlgNoneAttack()` in `ShopWave/Program.cs` die:

1. Een JWT-header construeert met `{"alg":"none","typ":"JWT"}` en die omzet naar Base64url.
2. Een JWT-payload construeert met `{"sub":"admin@shopwave.be","role":"admin"}` en die omzet naar Base64url.
3. Een token construeert zonder signature: `header.payload.` (let op de punt aan het einde, de signature-sectie is leeg).
4. Dat token stuurt naar `GET /admin/orders` via `HttpClient`.
5. De statuscode afdrukt.

**Vereisten:**

- Gebruik `Convert.ToBase64String(Encoding.UTF8.GetBytes(...))` en vervang daarna `+` door `-`, `/` door `_` en verwijder de `=`-tekens.
- Gebruik dezelfde `HttpClientHandler` als in oefening 1.

**Startcode:**

```csharp
void TryAlgNoneAttack()
{
    Console.WriteLine("=== alg:none aanval ===");

    // Stap 1: header coderen
    string header = "";
    // jouw code hier

    // Stap 2: payload coderen
    string payload = "";
    // jouw code hier

    // Stap 3: token samenstellen zonder signature
    string noneToken = $"{header}.{payload}.";

    // Stap 4: versturen naar /admin/orders
    // jouw code hier

    Console.WriteLine("Verwacht: Unauthorized (.NET weigert alg:none standaard)");
}
```

**Controleer je werk:** verwacht resultaat:

```csharp
=== alg:none aanval ===
Resultaat alg:none token: Unauthorized
Verwacht: Unauthorized (.NET weigert alg:none standaard)
```

Beantwoord daarna schriftelijk:

1. Waarom werkt de `alg:none`-aanval bij sommige JWT-bibliotheken wel?
2. Wat in de .NET-configuratie van ShopWave blokkeert de aanval?

---

<h3 class="opdracht-titel">Opdracht</h3>

## Oefening 3: Informatielekkage in development vs productie

**Leerdoel:** je toont aan dat de Developer Exception Page interne systeeminformatie lekt en configureert de productie-response correct.

**Moeilijkheidsgraad:** gemiddeld

**Situatie:** een pentester legt een rapport neer met bevinding: "Via `/crash` kan ik in development de volledige stacktrace zien, inclusief bestandspaden en bibliotheekinformatie." Jij voegt een `/crash`-endpoint toe, verifieert de lekkage in development en toont dat productie correct weerstand biedt.

**Wat je doet:**

1. Voeg een endpoint `/crash` toe aan `ShopWave.Api/Program.cs` dat een `InvalidOperationException` gooit.
2. Start de API in development. Roep `/crash` aan en noteer wat je ziet in de response-body.
3. Herstart met `ASPNETCORE_ENVIRONMENT=Production`. Roep `/crash` opnieuw aan en vergelijk.

**Vereisten voor stap 1:**

```csharp
app.MapGet("/crash", () =>
{
    throw new InvalidOperationException("Gesimuleerde interne fout.");
});
```

**PowerShell-commando's voor stap 2 en 3:**

```powershell
# Development starten
$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet run --project ShopWave.Api

# Aanroepen vanuit een tweede terminal
curl.exe -k https://localhost:5001/crash

# Daarna herstarten in productie
$env:ASPNETCORE_ENVIRONMENT = "Production"
dotnet run --project ShopWave.Api
curl.exe -k https://localhost:5001/crash
```

**Controleer je werk:**

Noteer het verschil in de response-body. Beantwoord daarna schriftelijk:

1. Wat zie je in de response-body bij development? Noem minstens drie soorten interne informatie die gelekt worden.
2. Wat zie je bij productie?
3. Welke OWASP-kwetsbaarheid dekt dit en waarom is dit een risico op een publieke server?

---

<h3 class="opdracht-titel">Opdracht</h3>

## Oefening 4: PentestReport-klasse implementeren

**Leerdoel:** je bouwt een C#-klasse die pentestbevindingen beheert, filtert en overzichten genereert.

**Moeilijkheidsgraad:** gemiddeld

**Situatie:** na elke pentest moet er een gestructureerd rapport komen. Je bouwt een `PentestReport`-klasse die bevindingen opslaat met risicoclassificatie, filterbaar maakt op risiconiveau en een samenvattingstabel genereert.

**Wat je doet:**

Maak `ShopWave/Security/PentestReport.cs` met de volgende structuur:

- Een klasse `Finding` met properties: `Id` (string), `Title` (string), `Risk` (string: `"Critical"`, `"High"`, `"Medium"`, `"Low"` of `"Informational"`), `CvssScore` (double), `Description` (string), `Evidence` (string), `Recommendation` (string), `Status` (string: `"Open"` of `"Closed"`).
- Een klasse `PentestReport` met:
  - Een `private readonly List<Finding> _findings` die initialiseerd wordt in de constructor.
  - Een methode `AddFinding(Finding finding)` die de bevinding toevoegt.
  - Een methode `GetByRisk(string risk)` die alle bevindingen met dat risiconiveau teruggeeft.
  - Een methode `GetOpenFindings()` die alle bevindingen met `Status == "Open"` teruggeeft.
  - Een methode `PrintSummary()` die elke bevinding afdrukt in het formaat `[{Id}] {Risk} ({CvssScore}) - {Title} [{Status}]`.

**Startcode:**

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
        private readonly List<Finding> _findings;

        public PentestReport()
        {
            // jouw code hier
        }

        public void AddFinding(Finding finding)
        {
            // jouw code hier
        }

        public List<Finding> GetByRisk(string risk)
        {
            // jouw code hier
            return new List<Finding>();
        }

        public List<Finding> GetOpenFindings()
        {
            // jouw code hier
            return new List<Finding>();
        }

        public void PrintSummary()
        {
            // jouw code hier
        }
    }
}
```

**Controleer je werk:** voeg tijdelijk toe aan `ShopWave/Program.cs`:

```csharp
PentestReport report = new PentestReport();

report.AddFinding(new Finding
{
    Id             = "FINDING-01",
    Title          = "SQL Injection op zoekendpoint",
    Risk           = "High",
    CvssScore      = 8.2,
    Description    = "Via het e-mailzoekveld kunnen alle orders worden opgezocht.",
    Evidence       = "GET /orders/zoek?email=' OR '1'='1 -> 200 OK met alle records",
    Recommendation = "Gebruik parameterized queries.",
    Status         = "Open"
});

report.AddFinding(new Finding
{
    Id             = "FINDING-02",
    Title          = "Developer Exception Page actief in productie",
    Risk           = "Medium",
    CvssScore      = 5.3,
    Description    = "Stack trace met databaseconnectiestring zichtbaar bij foutieve requests.",
    Evidence       = "GET /crash -> stack trace met connectiestring",
    Recommendation = "UseDeveloperExceptionPage() enkel in development.",
    Status         = "Closed"
});

report.AddFinding(new Finding
{
    Id             = "FINDING-03",
    Title          = "Swagger beschikbaar zonder authenticatie",
    Risk           = "Informational",
    CvssScore      = 2.1,
    Description    = "Swagger UI toont alle endpoints inclusief admin-endpoints.",
    Evidence       = "GET /swagger -> volledige endpoint-documentatie",
    Recommendation = "Swagger beperken tot development.",
    Status         = "Open"
});

Console.WriteLine("=== Alle bevindingen ===");
report.PrintSummary();

Console.WriteLine("\n=== Open bevindingen ===");
foreach (Finding f in report.GetOpenFindings())
{
    Console.WriteLine($"  - {f.Id}: {f.Title}");
}

Console.WriteLine("\n=== High-risico bevindingen ===");
foreach (Finding f in report.GetByRisk("High"))
{
    Console.WriteLine($"  - {f.Id}: {f.CvssScore} - {f.Title}");
}
```

Verwacht resultaat:

```csharp
=== Alle bevindingen ===
[FINDING-01] High (8,2) - SQL Injection op zoekendpoint [Open]
[FINDING-02] Medium (5,3) - Developer Exception Page actief in productie [Closed]
[FINDING-03] Informational (2,1) - Swagger beschikbaar zonder authenticatie [Open]

=== Open bevindingen ===
  - FINDING-01: SQL Injection op zoekendpoint
  - FINDING-03: Swagger beschikbaar zonder authenticatie

=== High-risico bevindingen ===
  - FINDING-01: 8,2 - SQL Injection op zoekendpoint
```

---

<h3 class="opdracht-titel">Opdracht</h3>

## Oefening 5: Volledig pentestreport schrijven

**Leerdoel:** je voert een gestructureerde pentest uit op je eigen ShopWave-API en documenteert de bevindingen professioneel.

**Moeilijkheidsgraad:** uitdagend

**Situatie:** ShopWave bereidt zich voor op een externe beveiligingsaudit. Jij voert intern een pentest uit op de draaiende API en levert een professioneel rapport af met alle bevindingen, bewijs en aanbevelingen.

**Wat je doet:**

Voer de volgende tests uit op de ShopWave API (gestart via `dotnet run`):

**Test 1: verkenning**

```powershell
curl.exe -k https://localhost:5001/
curl.exe -k https://localhost:5001/swagger
curl.exe -k https://localhost:5001/health
curl.exe -k https://localhost:5001/bestaaniet
```

**Test 2: JWT-rolmanipulatie**

Roep de methode `TryRoleManipulation(...)` aan met een geldig token van Alice.

**Test 3: brute-force op login**

```powershell
for ($i = 1; $i -le 7; $i++) {
    $body     = "{`"email`":`"alice@shopwave.be`",`"password`":`"fout$i`"}"
    $response = curl.exe -k -s -o NUL -w "%{http_code}" `
        -X POST https://localhost:5001/login `
        -H "Content-Type: application/json" `
        -d $body
    Write-Output "Poging $i : $response"
}
```

**Test 4: CORS-inspect**

```powershell
curl.exe -k -v -H "Origin: https://aanvaller.be" https://localhost:5001/ 2>&1 | Select-String "Access-Control"
```

**Test 5: informatielekkage**

```powershell
curl.exe -k https://localhost:5001/crash
```

**Vereisten voor het rapport:**

Maak een `PentestReport`-instantie (gebruik de klasse uit oefening 4) en voeg per test minstens één `Finding` toe met:

- Een uniek `Id`: `FINDING-01` tot `FINDING-05`
- De juiste `Risk`-classificatie op basis van de bevindingen
- Concreet `Evidence`: de statuscode en eventuele response-body
- Een concrete `Recommendation`

Druk het rapport af via `PrintSummary()` en beantwoord daarna:

1. Welke bevinding heeft de hoogste CVSS-score? Onderbouw je keuze.
2. Welke maatregelen zijn al correct geconfigureerd in ShopWave?
3. Welke bevinding heeft de hoogste prioriteit om op te lossen? Waarom?
