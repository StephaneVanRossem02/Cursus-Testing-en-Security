---
title: "Les 11: Ethisch Hacken — Oefeningen"
sidebar_label: "Oefeningen"
---

# Les 11: Ethisch Hacken — Oefeningen

> **Code-afspraken:** geen top-level statements · altijd `{}` · max één `return` · geen `break`/`continue` · geen underscore-prefix op parameters · geen geneste klassen · geen ternary/null-conditional · geen tuples · `double` i.p.v. `decimal` · identifiers Engels · tekst Nederlands

---

## Oefening 1 — Pentest: CORS-headers inspecteren

**Opgave:** Stuur een request met een `Origin`-header en inspecteer de response-headers op CORS-configuratie.

```bash
curl -k -H "Origin: https://kwaadaardige-site.com" https://localhost:5001/orders
```

**Wat je observeert:**
- Is `Access-Control-Allow-Origin` aanwezig in de response?
- Staat er `*` (wildcard — gevaarlijk) of een specifieke origin?

**Bevinding rapporteren:**
- Als `*` → Medium/High risico: elke website kan requests sturen namens de gebruiker
- Als geen CORS-header of specifieke origin → correct geconfigureerd

**Oplossing bij `*`:**
```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("ShopWavePolicy", policy =>
    {
        policy.WithOrigins("https://shopwave.be")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});
```

---

## Oefening 3 — Pentestreport schrijven (code-uitwerking)

**Opgave:** Schrijf een professioneel pentestreport via de `PentestReport`-klasse.

```csharp
// PentestReportTests.cs
using FluentAssertions;
using ShopWave.Security;

namespace ShopWave.Tests
{
    public class PentestReportTests
    {
        [Fact]
        public void PentestReport_MetBevindingen_KanAangemaakt_EnBevraagd_Worden()
        {
            // Arrange
            PentestReport rapport = new PentestReport(
                systeem:     "ShopWave API v1.0 (lokaal)",
                testdatum:   "2026-05-30",
                testmethode: "Grey box");

            PentestFinding finding1 = new PentestFinding(
                id:           "FINDING-01",
                titel:        "Rate limiting ontbreekt op /login",
                risico:       RisicoNiveau.Medium,
                beschrijving: "Onbeperkte loginpogingen zijn mogelijk",
                bewijs:       "HTTP 401 bij poging 6 in plaats van 429",
                aanbeveling:  "Activeer AddRateLimiter met FixedWindowLimiter");

            PentestFinding finding2 = new PentestFinding(
                id:           "FINDING-02",
                titel:        "JWT-sleutel hardcoded in broncode",
                risico:       RisicoNiveau.Hoog,
                beschrijving: "De JWT-signeringsleutel staat in plain text in Program.cs",
                bewijs:       "Sleutel zichtbaar in GitHub-repository",
                aanbeveling:  "Verplaats naar User Secrets of omgevingsvariabelen");

            rapport.VoegToe(finding1);
            rapport.VoegToe(finding2);

            // Assert
            rapport.GetBevindingen().Should().HaveCount(2);
            rapport.GetBevindingen(RisicoNiveau.Hoog).Should().HaveCount(1);
            rapport.GetBevindingen(RisicoNiveau.Medium).Should().HaveCount(1);
        }
    }
}
```

---

## Oefening 4 — Kwetsbare packages opsporen

**Opgave:** Voer `dotnet list package --vulnerable` uit. Noteer CVE-identifiers en minimale veilige versies. Update en voer het commando opnieuw uit.

```bash
# In de ShopWave solution-map:
dotnet list package --vulnerable --include-transitive
```

**Als unit test:**

```csharp
// VulnerablePackagesTests.cs
using FluentAssertions;
using System.Diagnostics;

namespace ShopWave.Tests
{
    public class VulnerablePackagesTests
    {
        [Fact]
        public void DotnetListPackage_HasNoVulnerablePackages()
        {
            // Arrange
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName               = "dotnet";
            startInfo.Arguments              = "list package --vulnerable";
            startInfo.RedirectStandardOutput = true;
            startInfo.UseShellExecute        = false;

            Process process = new Process();
            process.StartInfo = startInfo;
            process.Start();

            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            // Assert — output mag geen kwetsbare packages bevatten
            bool geenKwetsbaarheden = !output.Contains("has the following vulnerable packages");
            geenKwetsbaarheden.Should().BeTrue(
                $"Er zijn kwetsbare packages gevonden:\n{output}");
        }
    }
}
```