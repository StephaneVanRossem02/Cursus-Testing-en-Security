---
title: "Les 12: Oefeningen - ShopWave in Productie"
sidebar_label: "Oefeningen"
---

# Oefeningen: ShopWave in Productie

Werk de oefeningen in volgorde. Elke oefening bouwt verder op de vorige. Kijk niet vooraf in de oplossingen.

Je werkt verder in de bestaande ShopWave-solution. Nieuwe klassen maak je aan in `ShopWave/Security/`.

---

<h3 class="opdracht-titel">Opdracht</h3>

## Oefening 1: Productieomgeving configureren

**Leerdoel:** je configureert ShopWave zodat het zich correct gedraagt als `ASPNETCORE_ENVIRONMENT=Production` is ingesteld.

**Moeilijkheidsgraad:** basis

**Situatie:** ShopWave is klaar om gedeployd te worden. Voor de deployment moet je drie zaken aanpassen: Swagger uitschakelen in productie, de Developer Exception Page beperken tot development, en logging instellen op `Warning`-niveau voor productie.

**Wat je doet:**

1. Voeg een `appsettings.Production.json` toe aan `ShopWave.Api/` met het volgende logging-niveau:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

2. Zorg in `ShopWave.Api/Program.cs` dat Swagger enkel beschikbaar is in development. Gebruik `app.Environment.IsDevelopment()`.

3. Voeg een `/crash`-endpoint toe dat een `InvalidOperationException` gooit. Verifieer het verschil in response tussen development en productie.

**Vereisten:**

- `appsettings.Production.json` bevat geen secrets.
- Swagger is volledig onbereikbaar in productie (404).
- In productie geeft `/crash` enkel een generieke foutmelding terug.

**Controleer je werk:**

```powershell
# Development
$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet run --project ShopWave.Api

# In tweede terminal
curl.exe -k https://localhost:5001/swagger/index.html   # verwacht: 200
curl.exe -k https://localhost:5001/crash                # verwacht: stack trace

# Stop de API, herstart in productie
$env:ASPNETCORE_ENVIRONMENT = "Production"
dotnet run --project ShopWave.Api

curl.exe -k https://localhost:5001/swagger/index.html   # verwacht: 404
curl.exe -k https://localhost:5001/crash                # verwacht: generieke foutmelding
```

Beantwoord daarna schriftelijk:

1. Waarom mag `appsettings.Production.json` wel in git staan maar `appsettings.Development.json` met gevoelige waarden niet?
2. Wat is het risico als Swagger beschikbaar blijft in productie?

---

<h3 class="opdracht-titel">Opdracht</h3>

## Oefening 2: `SecurityChecklist`-klasse implementeren

**Leerdoel:** je implementeert een klasse die beveiligingsitems bijhoudt met categorie, status en toelichting, en een overzichtelijk rapport genereert.

**Moeilijkheidsgraad:** gemiddeld

**Situatie:** voor elke release van ShopWave moet een developer de beveiligingsstatus kunnen controleren. Je bouwt een `SecurityChecklist`-klasse die items per categorie beheert en de status bijhoudt.

**Wat je doet:**

Maak `ShopWave/Security/SecurityChecklist.cs` met de volgende structuur:

- Een klasse `ChecklistItem` met properties: `Category` (string), `Description` (string), `Status` (string: `"Implemented"`, `"Partial"` of `"NotImplemented"`), `Notes` (string).
- Een klasse `SecurityChecklist` met:
  - Een `private readonly List<ChecklistItem> items` die initialiseerd wordt in de constructor.
  - Een methode `AddItem(string category, string description)` die een item toevoegt met `Status = "NotImplemented"` en lege `Notes`.
  - Een methode `SetStatus(string description, string status, string notes)` die de status en toelichting van het item met die beschrijving instelt.
  - Een methode `GetByStatus(string status)` die alle items met die status teruggeeft.
  - Een methode `GetByCategory(string category)` die alle items in die categorie teruggeeft.
  - Een methode `IsFullyImplemented()` die `true` teruggeeft als alle items de status `"Implemented"` hebben.
  - Een methode `PrintReport()` die per categorie alle items afdrukt in het formaat hieronder.

**Startcode:**

```csharp
namespace ShopWave.Security
{
    public class ChecklistItem
    {
        public string Category    { get; set; } = "";
        public string Description { get; set; } = "";
        public string Status      { get; set; } = "NotImplemented";
        public string Notes       { get; set; } = "";
    }

    public class SecurityChecklist
    {
        private readonly List<ChecklistItem> items;

        public SecurityChecklist()
        {
            // jouw code hier
        }

        public void AddItem(string category, string description)
        {
            // jouw code hier
        }

        public void SetStatus(string description, string status, string notes)
        {
            // jouw code hier
        }

        public List<ChecklistItem> GetByStatus(string status)
        {
            // jouw code hier
            return new List<ChecklistItem>();
        }

        public List<ChecklistItem> GetByCategory(string category)
        {
            // jouw code hier
            return new List<ChecklistItem>();
        }

        public bool IsFullyImplemented()
        {
            // jouw code hier
            return false;
        }

        public void PrintReport()
        {
            // jouw code hier
        }
    }
}
```

**Vereisten voor `PrintReport()`:** per categorie verschijnt een header. Per item verschijnt `[OK]`, `[!!]` of `[ ]` op basis van de status, gevolgd door de beschrijving en de toelichting op de volgende regel. Aan het einde verschijnt een teller.

**Controleer je werk:** voeg tijdelijk toe aan `ShopWave/Program.cs`:

```csharp
SecurityChecklist checklist = new SecurityChecklist();

checklist.AddItem("Auth", "Passwords hashed with BCrypt");
checklist.AddItem("Auth", "Rate limiting on login endpoint");
checklist.AddItem("Data", "AES-256 encryption with random IV");

checklist.SetStatus("Passwords hashed with BCrypt",    "Implemented", "BCrypt.Net-Next");
checklist.SetStatus("Rate limiting on login endpoint", "Partial",     "Configured but not in CI");

checklist.PrintReport();
Console.WriteLine($"Volledig geimplementeerd: {checklist.IsFullyImplemented()}");

Console.WriteLine("\nNiet-geimplementeerde items:");
foreach (ChecklistItem item in checklist.GetByStatus("NotImplemented"))
{
    Console.WriteLine($"  - [{item.Category}] {item.Description}");
}
```

Verwacht resultaat:

```csharp
=== ShopWave Security Checklist ===

[Auth]
  [OK] Passwords hashed with BCrypt
       BCrypt.Net-Next
  [!!] Rate limiting on login endpoint
       Configured but not in CI

[Data]
  [ ] AES-256 encryption with random IV

Geimplementeerd: 1/3   Gedeeltelijk: 1/3   Niet geimplementeerd: 1/3
Volledig geimplementeerd: False

Niet-geimplementeerde items:
  - [Data] AES-256 encryption with random IV
```

---

<h3 class="opdracht-titel">Opdracht</h3>

## Oefening 3: `CiaPijlerAnalyse`-klasse implementeren

**Leerdoel:** je implementeert een klasse die de CIA-pijlers documenteert met concrete voorbeelden uit ShopWave en een overzichtelijk rapport genereert.

**Moeilijkheidsgraad:** gemiddeld

**Situatie:** na een cursus of een project wil je kunnen aantonen hoe de drie CIA-pijlers beschermd worden. Je bouwt een `CiaPijlerAnalyse`-klasse die per pijler voorbeelden bijhoudt.

**Wat je doet:**

Maak `ShopWave/Security/CiaPijlerAnalyse.cs` met de volgende structuur:

- Een klasse `CiaPillar` met properties: `Name` (string: `"Confidentiality"`, `"Integrity"` of `"Availability"`), een `private readonly List<string> examples`, een methode `AddExample(string example)` en een property `Examples` die de lijst als `IReadOnlyList<string>` teruggeeft.
- Een klasse `CiaPijlerAnalyse` met:
  - Drie vaste properties: `Confidentiality`, `Integrity` en `Availability` (allemaal van het type `CiaPillar`).
  - Een constructor die de drie pijlers initialiseert.
  - Een methode `PrintAnalysis()` die per pijler de naam en alle voorbeelden afdrukt.

**Startcode:**

```csharp
namespace ShopWave.Security
{
    public class CiaPillar
    {
        public string Name { get; }
        private readonly List<string> examples;

        public CiaPillar(string name)
        {
            // jouw code hier
        }

        public void AddExample(string example)
        {
            // jouw code hier
        }

        public IReadOnlyList<string> Examples => examples;
    }

    public class CiaPijlerAnalyse
    {
        public CiaPillar Confidentiality { get; }
        public CiaPillar Integrity       { get; }
        public CiaPillar Availability    { get; }

        public CiaPijlerAnalyse()
        {
            // jouw code hier
        }

        public void PrintAnalysis()
        {
            // jouw code hier
        }
    }
}
```

**Controleer je werk:** voeg tijdelijk toe aan `ShopWave/Program.cs`:

```csharp
CiaPijlerAnalyse analyse = new CiaPijlerAnalyse();

analyse.Confidentiality.AddExample("BCrypt: wachtwoorden onleesbaar bij diefstal van de database");
analyse.Confidentiality.AddExample("AES-256: orderdata versleuteld opgeslagen");
analyse.Confidentiality.AddExample("HTTPS/TLS: data onleesbaar tijdens transport");
analyse.Confidentiality.AddExample("JWT met rolclaims: enkel admins bereiken /admin/orders");

analyse.Integrity.AddExample("RSA digitale handtekeningen: prijsmanipulatie detecteerbaar");
analyse.Integrity.AddExample("X.509-certificaat: server-identiteit gegarandeerd in TLS-handshake");
analyse.Integrity.AddExample("Input validatie: kwaadaardige payloads afgewezen");
analyse.Integrity.AddExample("Parameterized queries: SQL Injection geblokkeerd");

analyse.Availability.AddExample("Account lockout na 3 mislukte pogingen");
analyse.Availability.AddExample("Rate limiting: maximaal 5 loginpogingen per minuut");
analyse.Availability.AddExample("HttpClient timeout: applicatie blokkeert niet bij trage services");

analyse.PrintAnalysis();
```

Verwacht resultaat:

```csharp
=== CIA-pijleranalyse ShopWave ===

Confidentiality (4 voorbeelden)
  - BCrypt: wachtwoorden onleesbaar bij diefstal van de database
  - AES-256: orderdata versleuteld opgeslagen
  - HTTPS/TLS: data onleesbaar tijdens transport
  - JWT met rolclaims: enkel admins bereiken /admin/orders

Integrity (4 voorbeelden)
  - RSA digitale handtekeningen: prijsmanipulatie detecteerbaar
  - X.509-certificaat: server-identiteit gegarandeerd in TLS-handshake
  - Input validatie: kwaadaardige payloads afgewezen
  - Parameterized queries: SQL Injection geblokkeerd

Availability (3 voorbeelden)
  - Account lockout na 3 mislukte pogingen
  - Rate limiting: maximaal 5 loginpogingen per minuut
  - HttpClient timeout: applicatie blokkeert niet bij trage services
```

---

<h3 class="opdracht-titel">Opdracht</h3>

## Oefening 4: Secrets audit

**Leerdoel:** je spoort hardcoded secrets op in ShopWave en vervangt ze door omgevingsvariabelen.

**Moeilijkheidsgraad:** gemiddeld

**Situatie:** voor de deployment voer je een secrets audit uit op de volledige ShopWave-codebase. Je zoekt naar hardcoded sleutels, wachtwoorden of tokens die niet in de broncode mogen staan.

**Wat je doet:**

1. Zoek in de ShopWave-codebase naar alle strings die op een secret lijken. Gebruik daarvoor de volgende PowerShell-zoekopdrachten:

```powershell
# Zoek naar hardcoded sleutels en wachtwoorden
Select-String -Path "ShopWave*\*.cs" -Pattern "password|secret|key|token" -CaseSensitive:$false |
    Where-Object { $_.Line -notmatch "//|GetEnvironmentVariable|configuration\[" } |
    Select-Object Filename, LineNumber, Line
```

2. Maak een klasse `SecretsAudit` in `ShopWave/Security/SecretsAudit.cs` die gesimuleerde code-fragmenten controleert op hardcoded secrets:

- Een methode `IsHardcoded(string codeLine)` die `true` teruggeeft als de regel een hardcoded waarde bevat. Gebruik de volgende definitie: een regel is hardcoded als hij een string-literal bevat (aanhalingstekens) na een van de trefwoorden `password`, `secret`, `key`, `token` of `connectionstring` (hoofdletterongevoelig) en het trefwoord `GetEnvironmentVariable` niet bevat.
- Een methode `AuditLines(List<string> codeLines)` die alle hardcoded regels uit de lijst teruggeeft.
- Een methode `PrintAuditReport(List<string> codeLines)` die de resultaten afdrukt.

**Startcode:**

```csharp
namespace ShopWave.Security
{
    public class SecretsAudit
    {
        private readonly List<string> secretKeywords;

        public SecretsAudit()
        {
            secretKeywords = new List<string>
            {
                "password", "secret", "key", "token", "connectionstring"
            };
        }

        public bool IsHardcoded(string codeLine)
        {
            // jouw code hier
            return false;
        }

        public List<string> AuditLines(List<string> codeLines)
        {
            // jouw code hier
            return new List<string>();
        }

        public void PrintAuditReport(List<string> codeLines)
        {
            // jouw code hier
        }
    }
}
```

**Controleer je werk:** voeg tijdelijk toe aan `ShopWave/Program.cs`:

```csharp
SecretsAudit audit = new SecretsAudit();

List<string> codeLines = new List<string>
{
    "string secretKey = \"ShopWaveGeheimeSleutel2024!!XYZ#\";",
    "string secretKey = Environment.GetEnvironmentVariable(\"JWT_SECRET_KEY\");",
    "string password = \"admin123\";",
    "string connectionString = builder.Configuration[\"ConnectionStrings:Default\"];",
    "// Dit is een commentaar over de secret key"
};

audit.PrintAuditReport(codeLines);
```

Verwacht resultaat:

```csharp
=== Secrets Audit ===

Mogelijke hardcoded secrets gevonden: 2

  Regel 1: string secretKey = "ShopWaveGeheimeSleutel2024!!XYZ#";
  Regel 3: string password = "admin123";

Aanbeveling: vervang hardcoded waarden door Environment.GetEnvironmentVariable(...).
```

---

<h3 class="opdracht-titel">Opdracht</h3>

## Oefening 5: Eindreflectie ShopWave

**Leerdoel:** je synthetiseert de volledige cursus door de beveiligingsarchitectuur van ShopWave te analyseren en te documenteren.

**Moeilijkheidsgraad:** basis (reflectie)

**Situatie:** ShopWave is klaar voor productie. Je schrijft een eindanalyse als afsluitend document.

**Wat je doet:**

Gebruik de `SecurityChecklist` uit oefening 2 en de `CiaPijlerAnalyse` uit oefening 3. Vul beide volledig in voor de ShopWave-codebase zoals die na 10 lessen bestaat. Druk beide rapporten af.

Beantwoord daarna schriftelijk de volgende vragen:

1. Welke beveiligingsmaatregel heeft de meeste impact gehad op de Confidentiality-pijler? Onderbouw je keuze.

2. OWASP A09 (Logging and Monitoring Failures) is als "Gedeeltelijk" gemarkeerd in de checklist. Wat ontbreekt er, en wat zou je toevoegen als ShopWave naar een echte productieserver zou gaan?

3. Een collega stelt voor om de JWT-sleutel op te slaan in `appsettings.json` omdat "dat makkelijker is dan omgevingsvariabelen". Hoe reageer je? Geef minstens twee concrete redenen.

4. Welke stap in de DevSecOps-pipeline uit de theorie heeft de meeste waarde voor een klein team van twee developers? Waarom?

5. Je stage-bedrijf gebruikt nog geen HTTPS. Je wil dit aankaarten. Welke twee concrete bevindingen uit ShopWave gebruik je om het risico te illustreren?
