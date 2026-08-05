---
title: "Les 12: Oplossingen - ShopWave in Productie"
sidebar_label: "Oplossingen"
---

# Oplossingen: ShopWave in Productie

> [Download het volledige ShopWave-project van les 12](/downloads/shopwave-12-shopwave-in-productie.zip) (ZIP). Bevat alle code tot en met deze les, klaar om te bouwen en te testen.

**Bekijk dit pas nadat je de oefeningen zelf geprobeerd hebt.** Lees de toelichting ook als je het juist had.

---

## Oplossing 1: Productieomgeving configureren

### ShopWave.Api/appsettings.Production.json

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

### ShopWave.Api/Program.cs (relevante fragmenten)

```csharp
// Swagger enkel in development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Crash-endpoint voor de test
app.MapGet("/crash", () =>
{
    throw new InvalidOperationException("Gesimuleerde interne fout.");
});
```

### Toelichting

`app.Environment.IsDevelopment()` evalueert de waarde van `ASPNETCORE_ENVIRONMENT`. Als die variabele niet is ingesteld, gedraagt ASP.NET Core zich als development. Op een server stel je de variabele altijd expliciet in op `Production`.

`appsettings.Production.json` mag in git staan omdat het geen secrets bevat. Het bevat enkel configuratie-overrides die in productie anders moeten zijn dan in development. Secrets staan nooit in een configuratiebestand, ook niet in het productiebestand.

**Antwoorden op de reflectievragen:**

1. `appsettings.Production.json` bevat geen waarden die een aanvaller kan gebruiken: enkel logniveaus en eventueel CORS-origins. `appsettings.Development.json` zou secrets kunnen bevatten als een developer onoplettend is. Maar de echte bescherming is dat secrets nooit in een configuratiebestand staan, ongeacht de omgeving.

2. Swagger toont de volledige structuur van de API: alle endpoints, alle request- en response-types, alle parameters. Een aanvaller gebruikt Swagger om gerichte aanvallen te plannen op endpoints die hij anders niet kende. Admin-endpoints zoals `/admin/orders` zijn zichtbaar zonder dat een aanvaller de broncode heeft.

**Veelgemaakte fout:** studenten plaatsen de Swagger-check na `app.Build()` maar voor `app.UseSwagger()`. De volgorde is correct, maar ze vergeten dat `app.UseSwaggerUI()` ook moet worden uitgeschakeld. Als enkel `UseSwagger()` wegvalt, is de Swagger-JSON nog steeds bereikbaar via `/swagger/v1/swagger.json`.

---

## Oplossing 2: `SecurityChecklist`-klasse implementeren

### ShopWave/Security/SecurityChecklist.cs

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
            items = new List<ChecklistItem>();
        }

        public void AddItem(string category, string description)
        {
            items.Add(new ChecklistItem
            {
                Category    = category,
                Description = description
            });
        }

        public void SetStatus(string description, string status, string notes)
        {
            ChecklistItem? item = items.FirstOrDefault(
                i => i.Description == description);

            if (item != null)
            {
                item.Status = status;
                item.Notes  = notes;
            }
        }

        public List<ChecklistItem> GetByStatus(string status)
        {
            return items
                .Where(i => i.Status == status)
                .ToList();
        }

        public List<ChecklistItem> GetByCategory(string category)
        {
            return items
                .Where(i => i.Category == category)
                .ToList();
        }

        public bool IsFullyImplemented()
        {
            return items.All(i => i.Status == "Implemented");
        }

        public void PrintReport()
        {
            Console.WriteLine("=== ShopWave Security Checklist ===");

            List<string> categories = items
                .Select(i => i.Category)
                .Distinct()
                .ToList();

            foreach (string category in categories)
            {
                Console.WriteLine($"\n[{category}]");

                List<ChecklistItem> categoryItems = GetByCategory(category);

                foreach (ChecklistItem item in categoryItems)
                {
                    string indicator = item.Status switch
                    {
                        "Implemented"    => "[OK]",
                        "Partial"        => "[!!]",
                        _                => "[ ]"
                    };

                    Console.WriteLine($"  {indicator} {item.Description}");

                    if (!string.IsNullOrWhiteSpace(item.Notes))
                    {
                        Console.WriteLine($"       {item.Notes}");
                    }
                }
            }

            int implemented    = GetByStatus("Implemented").Count;
            int partial        = GetByStatus("Partial").Count;
            int notImplemented = GetByStatus("NotImplemented").Count;
            int total          = items.Count;

            Console.WriteLine($"\nGeimplementeerd: {implemented}/{total}   " +
                              $"Gedeeltelijk: {partial}/{total}   " +
                              $"Niet geimplementeerd: {notImplemented}/{total}");
        }
    }
}
```

### Toelichting

`FirstOrDefault` geeft het eerste item terug dat aan de voorwaarde voldoet, of `null` als er geen is. De `?`-operator in `ChecklistItem?` markeert de variabele als nullable zodat de compiler waarschuwt als je `item` gebruikt zonder de `null`-check.

`items.All(i => i.Status == "Implemented")` geeft `true` als alle elementen de voorwaarde vervullen. Als de lijst leeg is, geeft `All` ook `true` terug. In dit geval is een lege checklist semantisch niet "volledig geimplementeerd", maar de oefening vereist die extra check niet.

`Distinct()` verwijdert dubbele waarden uit de categorie-lijst. Zo verschijnt elke categorie slechts één keer als sectieheader.

**Veelgemaakte fout:** studenten gebruiken een `foreach`-lus om de bestaande status te zoeken en te wijzigen. Dat werkt, maar `FirstOrDefault` is de idiomatische LINQ-aanpak. Een subtielere fout: studenten vergeten de `null`-check na `FirstOrDefault`. Als `description` niet overeenkomt met een bestaand item, geeft `FirstOrDefault` `null` terug en gooit het aanroepen van `item.Status = status` een `NullReferenceException`.

---

## Oplossing 3: `CiaPijlerAnalyse`-klasse implementeren

### ShopWave/Security/CiaPijlerAnalyse.cs

```csharp
namespace ShopWave.Security
{
    public class CiaPillar
    {
        public string Name { get; }
        private readonly List<string> examples;

        public CiaPillar(string name)
        {
            Name      = name;
            examples = new List<string>();
        }

        public void AddExample(string example)
        {
            examples.Add(example);
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
            Confidentiality = new CiaPillar("Confidentiality");
            Integrity       = new CiaPillar("Integrity");
            Availability    = new CiaPillar("Availability");
        }

        public void PrintAnalysis()
        {
            Console.WriteLine("=== CIA-pijleranalyse ShopWave ===");

            PrintPillar(Confidentiality);
            PrintPillar(Integrity);
            PrintPillar(Availability);
        }

        private void PrintPillar(CiaPillar pillar)
        {
            Console.WriteLine($"\n{pillar.Name} ({pillar.Examples.Count} voorbeelden)");

            foreach (string example in pillar.Examples)
            {
                Console.WriteLine($"  - {example}");
            }
        }
    }
}
```

### Toelichting

`IReadOnlyList<string>` exposeert de lijst als alleen-lezen. Externe code kan de lijst lezen maar geen elementen toevoegen of verwijderen. Dat is de juiste encapsulatie: de enige manier om een voorbeeld toe te voegen is via `AddExample`, niet via directe lijstmanipulatie.

`private void PrintPillar(CiaPillar pillar)` is een hulpmethode die de herhaling in `PrintAnalysis` elimineert. Dezelfde logica wordt drie keer toegepast op drie verschillende pijlers. Dat is de juiste toepassing van het DRY-principe (Don't Repeat Yourself) zonder onnodige abstractie.

**Veelgemaakte fout:** studenten maken `examples` `public` of gebruiken een `public List<string>` als property. Dat geeft externe code volledige controle over de lijst, inclusief `Clear()` en directe `Add()` zonder via de publieke methode te gaan. Gebruik altijd `IReadOnlyList<T>` voor leesbare collecties die intern beheerd worden.

---

## Oplossing 4: Secrets audit

### ShopWave/Security/SecretsAudit.cs

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
            string lowerLine = codeLine.ToLowerInvariant();

            bool containsKeyword = secretKeywords
                .Any(keyword => lowerLine.Contains(keyword));

            bool containsStringLiteral = codeLine.Contains("\"");

            bool usesEnvironmentVariable = lowerLine.Contains("getenvironmentvariable");

            bool usesConfiguration = lowerLine.Contains("configuration[") ||
                                     lowerLine.Contains("configuration.get");

            bool isComment = lowerLine.TrimStart().StartsWith("//");

            return containsKeyword
                && containsStringLiteral
                && !usesEnvironmentVariable
                && !usesConfiguration
                && !isComment;
        }

        public List<string> AuditLines(List<string> codeLines)
        {
            return codeLines
                .Where(line => IsHardcoded(line))
                .ToList();
        }

        public void PrintAuditReport(List<string> codeLines)
        {
            List<string> hardcodedLines = AuditLines(codeLines);

            Console.WriteLine("=== Secrets Audit ===");

            if (hardcodedLines.Count == 0)
            {
                Console.WriteLine("\nGeen hardcoded secrets gevonden.");
            }
            else
            {
                Console.WriteLine($"\nMogelijke hardcoded secrets gevonden: {hardcodedLines.Count}");
                Console.WriteLine();

                for (int index = 0; index < codeLines.Count; index++)
                {
                    if (IsHardcoded(codeLines[index]))
                    {
                        Console.WriteLine($"  Regel {index + 1}: {codeLines[index].Trim()}");
                    }
                }

                Console.WriteLine("\nAanbeveling: vervang hardcoded waarden door " +
                                  "Environment.GetEnvironmentVariable(...).");
            }
        }
    }
}
```

### Toelichting

`ToLowerInvariant()` zet de string om naar kleine letters zonder afhankelijk te zijn van de locale van het systeem. `ToLower()` kan anders gedragen op systemen met een Turkse locale, waar de hoofdletter `I` wordt omgezet naar `ı` (zonder punt) in plaats van `i`. `ToLowerInvariant()` is de veilige keuze voor string-vergelijkingen in code.

`secretKeywords.Any(keyword => lowerLine.Contains(keyword))` controleert of minstens één trefwoord in de regel voorkomt. `Any` stopt zodra het eerste overeenkomende element gevonden is, wat efficiënter is dan `Where(...).Count() > 0`.

De audit is bewust simpel. Een productiewaardige secrets-scanner gebruikt reguliere expressies voor betere detectie van string-literals en houdt rekening met multiline-strings, interpolated strings en verbatim strings. Tools zoals `trufflehog`, `gitleaks` of de ingebouwde GitHub secret scanning zijn betrouwbaarder voor productiegebruik.

**Veelgemaakte fout:** studenten vergeten de commentaar-check (`isComment`). Een regel als `// Gebruik nooit een hardcoded password` zou anders als bevinding worden gerapporteerd. Commentaarregels beginnen altijd met `//` na eventuele witruimte, vandaar `TrimStart().StartsWith("//")`.

---

## Oplossing 5: Eindreflectie ShopWave

Hieronder modelantwoorden. Jouw antwoorden mogen anders geformuleerd zijn als de redenering klopt.

**Vraag 1: meest impactvolle maatregel voor Confidentiality**

BCrypt-hashing heeft de meeste impact. Wachtwoorden zijn de sleutel tot alles: een gebruiker die zijn wachtwoord hergebruikt op andere sites, is bij een datalek van ShopWave kwetsbaar op alle andere sites. BCrypt maakt een database-dump nutteloos voor een aanvaller: hij heeft de gehashte waarden maar kan ze niet omzetten naar de originele wachtwoorden binnen een redelijke tijd. HTTPS beschermt het transport maar niet de opgeslagen data. JWT beschermt de toegang maar niet de data zelf.

**Vraag 2: OWASP A09 - wat ontbreekt**

ShopWave logt fouten naar de console. In productie verdwijnt console-output als niemand ernaar kijkt. Wat ontbreekt:
- Gecentraliseerde logging naar een persistent systeem (Azure Application Insights, Serilog naar een bestand of externe service).
- Alerting bij verdachte activiteit: meer dan 5 mislukte loginpogingen van hetzelfde IP-adres, een 401-storm op admin-endpoints.
- Audit trail: wie heeft wanneer welk order bekeken of aangepast?

Voor een echte productieserver voeg je minimaal Serilog toe met een file sink en stel je een alert in als het foutenpercentage boven een drempelwaarde stijgt.

**Vraag 3: JWT-sleutel in `appsettings.json`**

Twee concrete redenen:

1. `appsettings.json` staat in git. Iedereen met toegang tot de repository heeft dan de sleutel. Dat geldt ook voor toekomstige medewerkers, voor github.com als de repository ooit publiek wordt gemaakt, en voor iedereen die ooit een clone heeft gemaakt voor de sleutel werd verwijderd. Git-geschiedenis bewaart alles.

2. Als de sleutel eenmaal in git staat, is het niet genoeg om hem te verwijderen. De sleutel moet geroteerd worden: een nieuwe sleutel genereren en alle bestaande JWT-tokens invalideren door de uitgifte-sleutel te wijzigen. Dat is veel werk en veroorzaakt downtime voor alle ingelogde gebruikers.

**Vraag 4: meest waardevolle DevSecOps-stap voor een klein team**

`dotnet list package --vulnerable --include-transitive`. Twee developers hebben geen tijd voor een volledige DAST-scan bij elke commit. Een DAST-scan op OWASP ZAP vereist bovendien een draaiende applicatie in de pipeline, wat de build-tijd sterk verhoogt. De kwetsbare-packages-check is snel, geautomatiseerd en vangt OWASP A06 volledig af. Een NuGet-package met een kritieke kwetsbaarheid kan je de hele applicatie kosten. Dat is het meest concrete en onmiddellijk aantoonbare risico voor een klein team.

**Vraag 5: HTTPS illustreren voor je stage-bedrijf**

Twee concrete bevindingen uit ShopWave:

1. JWT-tokens worden bij elke API-request in de `Authorization`-header meegestuurd. Over HTTP zijn die headers leesbaar voor iedereen die het netwerkverkeer kan afluisteren. Wie het token heeft, heeft volledige toegang tot de account, inclusief admin-rechten als het een admin-token is. Dit is een man-in-the-middle aanval zonder dat de aanvaller ook maar één wachtwoord hoeft te raden.

2. Wachtwoorden worden bij de inlogstap verstuurd in de request-body. Over HTTP is die body leesbaar in plain text. Een aanvaller op hetzelfde wifi-netwerk (in een café, op een beurs, bij een klant) leest het wachtwoord direct mee. BCrypt beschermt het opgeslagen wachtwoord in de database, maar niet het wachtwoord dat over het netwerk reist. HTTPS sluit die aanvalsvector volledig.
