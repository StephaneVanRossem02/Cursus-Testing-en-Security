---
title: "Les 12: Herhaling en Globalisatie — Oefeningen"
sidebar_label: "Oefeningen"
---

# Les 12: Herhaling en Globalisatie — Oefeningen

> **Code-afspraken:** geen top-level statements · altijd `{}` · max één `return` · geen `break`/`continue` · geen underscore-prefix op parameters · geen geneste klassen · geen ternary/null-conditional · geen tuples · `double` i.p.v. `decimal` · identifiers Engels · tekst Nederlands

---

## Oefening 1 — Beveiligingschecklist

**Opgave:** Pas de `SecurityChecklist`-klasse toe op ShopWave. Markeer elk item als `Geimplementeerd`, `Gedeeltelijk` of `NietGeimplementeerd`.

```csharp
// In ShopWave/Program.cs — methode DemoBeveiligingschecklist()
private static void DemoBeveiligingschecklist()
{
    SecurityChecklist checklist = new SecurityChecklist();

    // Authenticatie
    checklist.VoegToe(new ChecklistItem("Authenticatie", "Wachtwoorden gehasht met BCrypt"));
    checklist.VoegToe(new ChecklistItem("Authenticatie", "Twee-factor authenticatie actief"));
    checklist.VoegToe(new ChecklistItem("Authenticatie", "JWT-tokens met vervaltijd"));

    // Vertrouwelijkheid
    checklist.VoegToe(new ChecklistItem("Vertrouwelijkheid", "AES-256 encryptie voor orderdata"));
    checklist.VoegToe(new ChecklistItem("Vertrouwelijkheid", "HTTPS via UseHttpsRedirection"));

    // Integriteit
    checklist.VoegToe(new ChecklistItem("Integriteit", "Digitale handtekeningen op orders"));
    checklist.VoegToe(new ChecklistItem("Integriteit", "Server-side input validatie op alle endpoints"));

    // Beschikbaarheid
    checklist.VoegToe(new ChecklistItem("Beschikbaarheid", "Rate limiting op login-endpoint"));

    // Statussen instellen
    checklist.StelStatusIn("Wachtwoorden gehasht met BCrypt",
        ChecklistStatus.Geimplementeerd, "BCrypt.Net-Next via PasswordHasher.cs");
    checklist.StelStatusIn("Twee-factor authenticatie actief",
        ChecklistStatus.Geimplementeerd, "TwoFactorService met callback-patroon");
    checklist.StelStatusIn("JWT-tokens met vervaltijd",
        ChecklistStatus.Geimplementeerd, "JwtTokenService — standaard 60 minuten");
    checklist.StelStatusIn("AES-256 encryptie voor orderdata",
        ChecklistStatus.Geimplementeerd, "AesEncryptor met willekeurige IV per encryptie");
    checklist.StelStatusIn("HTTPS via UseHttpsRedirection",
        ChecklistStatus.Geimplementeerd, "Geconfigureerd in ShopWave.Api/Program.cs");
    checklist.StelStatusIn("Digitale handtekeningen op orders",
        ChecklistStatus.Geimplementeerd, "OrderSigner met RSA via X.509-certificaat");
    checklist.StelStatusIn("Server-side input validatie op alle endpoints",
        ChecklistStatus.Gedeeltelijk, "Login en verify gevalideerd — zoekendpoint nog niet");
    checklist.StelStatusIn("Rate limiting op login-endpoint",
        ChecklistStatus.NietGeimplementeerd, "Risico: brute force — AddRateLimiter vereist");

    checklist.PrintRapport();
}
```

---

## Oefening 2 — CIA-analyse van de volledige applicatie

**Opgave:** Gebruik de `CiaPijlerAnalyse`-klasse om per CIA-pijler minstens drie concrete voorbeelden uit de cursus te beschrijven.

```csharp
// In ShopWave/Program.cs — methode DemoCiaAnalyse()
private static void DemoCiaAnalyse()
{
    CiaPijlerAnalyse analyse = new CiaPijlerAnalyse();

    CiaPijler confidentiality = new CiaPijler("Confidentiality");
    confidentiality.VoegVoorbeeldToe(
        "BCrypt-hashing: wachtwoorden onleesbaar bij diefstal van de database");
    confidentiality.VoegVoorbeeldToe(
        "AES-256 encryptie: orderdata onleesbaar zonder de sleutel");
    confidentiality.VoegVoorbeeldToe(
        "HTTPS/TLS: data onleesbaar tijdens transport");
    confidentiality.VoegVoorbeeldToe(
        "JWT met rolclaims: enkel admins bereiken /admin/orders");

    CiaPijler integrity = new CiaPijler("Integrity");
    integrity.VoegVoorbeeldToe(
        "RSA digitale handtekeningen: prijsmanipulatie onmiddellijk detecteerbaar");
    integrity.VoegVoorbeeldToe(
        "X.509-certificaat: server-identiteit gegarandeerd in TLS-handshake");
    integrity.VoegVoorbeeldToe(
        "Input validatie: kwaadaardige payloads afgewezen aan de poort");
    integrity.VoegVoorbeeldToe(
        "Parameterized queries: SQL Injection geblokkeerd");

    CiaPijler availability = new CiaPijler("Availability");
    availability.VoegVoorbeeldToe(
        "Account lockout na 3 mislukte pogingen: brute force beperkt");
    availability.VoegVoorbeeldToe(
        "Rate limiting via FixedWindowLimiter: DoS-bescherming op /login");
    availability.VoegVoorbeeldToe(
        "EnsureSuccessStatusCode: externe services falen snel en duidelijk");
    availability.VoegVoorbeeldToe(
        "HttpClient timeout: applicatie blokkeert niet bij trage externe services");

    analyse.VoegPijlerToe(confidentiality);
    analyse.VoegPijlerToe(integrity);
    analyse.VoegPijlerToe(availability);

    analyse.PrintAnalyse();
}
```

---

## Oefening 3 — DevSecOps-pipeline uitbreiden

**Opgave:** Breid de CI/CD-pipeline uit met SAST en DAST.

```yaml
# .github/workflows/devsecops.yml
name: ShopWave DevSecOps Pipeline

on:
  push:
    branches: [main]
  pull_request:
    branches: [main]

jobs:
  build-and-test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '8.0.x'
      - name: Herstel packages
        run: dotnet restore
      - name: Compileer
        run: dotnet build --no-restore
      - name: Tests uitvoeren
        run: dotnet test --no-build --verbosity normal

  sast:
    runs-on: ubuntu-latest
    needs: build-and-test
    steps:
      - uses: actions/checkout@v3
      - uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '8.0.x'
      - name: Herstel packages
        run: dotnet restore
      - name: Kwetsbare packages controleren (OWASP A06)
        run: dotnet list package --vulnerable --include-transitive

  dast:
    runs-on: ubuntu-latest
    needs: build-and-test
    steps:
      - uses: actions/checkout@v3
      - name: OWASP ZAP Baseline Scan
        uses: zaproxy/action-baseline@v0.9.0
        with:
          target: 'https://localhost:5001'
          rules_file_name: '.zap/rules.tsv'
          cmd_options: '-a'
```

---

## Modeloplossing — SecurityChecklistTests

```csharp
// SecurityChecklistTests.cs
using FluentAssertions;
using ShopWave;

namespace ShopWave.Tests
{
    public class SecurityChecklistTests
    {
        [Fact]
        public void NieuweChecklist_HeeftGeenItems()
        {
            SecurityChecklist checklist = new SecurityChecklist();
            checklist.GetItemsMetStatus(ChecklistStatus.Geimplementeerd).Should().BeEmpty();
        }

        [Fact]
        public void VoegToe_ItemVerschijntInJuisteCategorie()
        {
            // Arrange
            SecurityChecklist checklist = new SecurityChecklist();
            ChecklistItem     item      = new ChecklistItem("Authenticatie", "BCrypt-hashing");

            // Act
            checklist.VoegToe(item);

            // Assert
            checklist.GetItemsPerCategorie("Authenticatie").Should().HaveCount(1);
        }

        [Fact]
        public void StelStatusIn_WijzigtStatusVanItem()
        {
            // Arrange
            SecurityChecklist checklist = new SecurityChecklist();
            checklist.VoegToe(new ChecklistItem("Authenticatie", "BCrypt-hashing"));

            // Act
            checklist.StelStatusIn("BCrypt-hashing", ChecklistStatus.Geimplementeerd, "Via BCrypt.Net-Next");

            // Assert
            checklist.GetItemsMetStatus(ChecklistStatus.Geimplementeerd).Should().HaveCount(1);
        }

        [Fact]
        public void IsVolledigGeimplementeerd_MetNietGeimplementeerdItem_RetourneertFalse()
        {
            // Arrange
            SecurityChecklist checklist = new SecurityChecklist();
            checklist.VoegToe(new ChecklistItem("Auth", "BCrypt"));
            checklist.VoegToe(new ChecklistItem("Auth", "Rate limiting"));
            checklist.StelStatusIn("BCrypt", ChecklistStatus.Geimplementeerd, "OK");

            // Act
            bool result = checklist.IsVolledigGeimplementeerd();

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void IsVolledigGeimplementeerd_AlleItemsGeimplementeerd_RetourneertTrue()
        {
            // Arrange
            SecurityChecklist checklist = new SecurityChecklist();
            checklist.VoegToe(new ChecklistItem("Auth", "BCrypt"));
            checklist.VoegToe(new ChecklistItem("Auth", "Rate limiting"));
            checklist.StelStatusIn("BCrypt",        ChecklistStatus.Geimplementeerd, "OK");
            checklist.StelStatusIn("Rate limiting", ChecklistStatus.Geimplementeerd, "OK");

            // Act
            bool result = checklist.IsVolledigGeimplementeerd();

            // Assert
            result.Should().BeTrue();
        }
    }
}
```

---

## Oefening 4 — Reflectie (schriftelijk)

1. **Grootste impact?** Afhankelijk van eigen ervaring — TDD is voor velen een omslag: je denkt eerst na over het gewenste gedrag vóór je schrijft.
2. **Meest voorkomende fout?** Plain-text wachtwoorden of ontbrekende input validatie — beide zijn eenvoudig te voorkomen met BCrypt en server-side checks.
3. **Testpiramide in twee weken?** Veel unit tests voor alle klassen, een handvol integration tests voor de kritische flows (login, checkout), één of twee acceptatietesten voor de belangrijkste use cases.
4. **"We hebben geen tijd"?** Tests besparen tijd. Een bug die in productie gevonden wordt kost veel meer tijd dan een test die hem eerder opvangt.
5. **Beveiligingsaudit vs. pentest?** Een audit beoordeelt processen, beleid en configuraties (wat is er op papier?). Een pentest probeert actief in te breken (werkt het in de praktijk?).