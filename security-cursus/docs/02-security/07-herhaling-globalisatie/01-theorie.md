---
title: "Les 10: Theorie - ShopWave in Productie"
sidebar_label: "Theorie"
---

# Les 10: Theorie - ShopWave in Productie

## 1. Development vs productie

Tijdens de cursus draait ShopWave op je eigen laptop. Dat is de development-omgeving. Studenten, docenten en jijzelf hebben toegang. Fouten zijn leerzaam en de schade is beperkt.

Een productie-omgeving is fundamenteel anders. Echte klanten vertrouwen hun gegevens toe aan de applicatie. Een fout is niet leerzaam maar schadelijk. Een gelekt wachtwoord, een gelekte JWT-sleutel of een zichtbare stack trace zijn niet meer een oefenfout maar een beveiligingsincident.

ASP.NET Core maakt het onderscheid via de omgevingsvariabele `ASPNETCORE_ENVIRONMENT`. Die variabele bepaalt welk gedrag de applicatie vertoont:

| Instelling | Development | Production |
|-----------|-------------|------------|
| Developer Exception Page | Aan: volledige stack trace | Uit: generieke foutpagina |
| Swagger UI | Aan: alle endpoints zichtbaar | Uit of beveiligd achter authenticatie |
| Logging-niveau | Debug: alle details | Warning: alleen echte problemen |
| Certificaat | Self-signed, `dotnet dev-certs` | Geldig certificaat van een CA |
| Secrets | Lokaal of hardcoded (nooit in productie) | Omgevingsvariabelen of Key Vault |

De meeste fouten bij de eerste deployment komen doordat een developer vergeet dat development-instellingen niet automatisch uitgeschakeld worden. Je moet productiegedrag expliciet configureren.

**Minicontrole:** een student deployt ShopWave naar een server maar vergeet `ASPNETCORE_ENVIRONMENT=Production` in te stellen. Welke drie concrete risico's ontstaan daardoor?

---

## 2. Secrets in productie

In les 7 leerde je omgevingsvariabelen gebruiken voor de JWT-sleutel:

```powershell
$env:JWT_SECRET_KEY = "ShopWaveGeheimeSleutel2024!!XYZ#"
```

Op je laptop werkt dat. Op een server werkt het ook, maar je stelt de variabele in via de serverconfiguratie en niet via een terminal die je daarna sluit.

### Omgevingsvariabelen op een server

Op een Linux-server stel je permanente omgevingsvariabelen in via `/etc/environment` of via de systemd-service van de applicatie:

```ini
# /etc/systemd/system/shopwave.service
[Service]
Environment="ASPNETCORE_ENVIRONMENT=Production"
Environment="JWT_SECRET_KEY=ProductieSleutelDieNooitInGitKomt"
Environment="AES_KEY=ProductieAesSleutelBase64Gecodeerd"
```

Op Windows Server gebruik je de systeeminstellingen of PowerShell als beheerder:

```powershell
[System.Environment]::SetEnvironmentVariable(
    "JWT_SECRET_KEY",
    "ProductieSleutelDieNooitInGitKomt",
    [System.EnvironmentVariableTarget]::Machine
)
```

### Azure Key Vault

Voor professionele productieomgevingen is een secrets manager de betere keuze. Azure Key Vault slaat secrets op, beheert wie er toegang toe heeft en houdt bij wanneer een secret werd bekeken of gewijzigd.

De flow voor ShopWave met Azure Key Vault:

```
ShopWave API (op Azure) --> Key Vault --> JWT_SECRET_KEY ophalen bij opstart
                       --> Key Vault --> AES_KEY ophalen bij opstart
```

In `Program.cs` voeg je de Key Vault-provider toe:

```csharp
string keyVaultUrl = Environment.GetEnvironmentVariable("AZURE_KEYVAULT_URL")
    ?? throw new InvalidOperationException("AZURE_KEYVAULT_URL ontbreekt.");

builder.Configuration.AddAzureKeyVault(
    new Uri(keyVaultUrl),
    new DefaultAzureCredential());
```

Na deze configuratie leest `builder.Configuration["JWT_SECRET_KEY"]` automatisch uit Key Vault. De code verandert niet. Enkel de bron van de waarde verandert.

**Het basisprincipe blijft altijd hetzelfde:** secrets staan nooit in de broncode en nooit in git. Lokaal gebruik je omgevingsvariabelen in de terminal. Op een server gebruik je permanente omgevingsvariabelen of een Key Vault.

**Minicontrole:** een developer slaat de JWT-sleutel op in `appsettings.json` en commit het bestand. Drie maanden later verwijdert hij de sleutel uit het bestand en commit opnieuw. Is de sleutel nu veilig? Leg uit.

---

## 3. HTTPS in productie

In les 6 heb je HTTPS geconfigureerd met een self-signed certificaat via `dotnet dev-certs`. Dat certificaat werkt lokaal omdat je browser het handmatig vertrouwt. Echte browsers van echte gebruikers vertrouwen self-signed certificaten niet. Ze tonen een waarschuwingspagina met "Verbinding is niet privé" en de meeste gebruikers klikken weg.

Voor productie heb je een certificaat nodig dat is uitgegeven door een Certificate Authority (CA) die browsers standaard vertrouwen. De meest gebruikte gratis CA is **Let's Encrypt**.

### Let's Encrypt

Let's Encrypt geeft gratis TLS-certificaten uit voor domeinnamen. Het certificaat is 90 dagen geldig en wordt automatisch verlengd via een tool als `certbot`.

Het proces:

1. Je hebt een domeinnaam die naar je server wijst (bv. `api.shopwave.be`).
2. `certbot` vraagt een certificaat aan bij Let's Encrypt.
3. Let's Encrypt verifieert dat jij de eigenaar bent van het domein door een bestand te plaatsen op `api.shopwave.be/.well-known/acme-challenge/`.
4. Het certificaat wordt geïnstalleerd en `certbot` zorgt voor automatische verlenging.

In `appsettings.Production.json` verwijs je naar het certificaat:

```json
{
  "Kestrel": {
    "Endpoints": {
      "Https": {
        "Url": "https://*:443",
        "Certificate": {
          "Path": "/etc/letsencrypt/live/api.shopwave.be/fullchain.pem",
          "KeyPath": "/etc/letsencrypt/live/api.shopwave.be/privkey.pem"
        }
      }
    }
  }
}
```

### Het verschil samengevat

| | Self-signed (development) | Let's Encrypt (productie) |
|-|--------------------------|--------------------------|
| Uitgifte | `dotnet dev-certs https --trust` | `certbot` + automatische verlenging |
| Vertrouwen | Enkel jouw eigen machine | Alle browsers wereldwijd |
| Geldigheid | Tot je hem verwijdert | 90 dagen, automatisch vernieuwd |
| Kosten | Gratis | Gratis |
| Vereiste | Geen | Domeinnaam die naar je server wijst |

**Minicontrole:** een API draait in productie met een self-signed certificaat. Een mobiele applicatie stuurt requests naar die API. Wat moet de mobiele applicatie doen om de requests te laten slagen, en waarom is dat een beveiligingsrisico?

---

## 4. Productieconfiguratie

ASP.NET Core laadt configuratie in een vaste volgorde. Elke stap overschrijft de vorige:

```
appsettings.json
    + appsettings.{Environment}.json
        + omgevingsvariabelen
            + command-line argumenten
```

In development laadt de applicatie `appsettings.json` en daarna `appsettings.Development.json`. In productie laadt hij `appsettings.json` en daarna `appsettings.Production.json`.

Gebruik dit systeem om development-specifieke instellingen te overschrijven zonder ze te verwijderen:

**`appsettings.json`** (gedeelde basisinstellingen, mag in git):

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  },
  "AllowedHosts": "*",
  "Cors": {
    "AllowedOrigins": ["https://shopwave.be"]
  }
}
```

**`appsettings.Development.json`** (development-overrides, mag in git):

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug"
    }
  },
  "Cors": {
    "AllowedOrigins": ["https://shopwave.be", "https://localhost:3000"]
  }
}
```

**`appsettings.Production.json`** (productie-overrides, mag in git want bevat geen secrets):

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning"
    }
  }
}
```

Secrets staan nooit in een van deze bestanden. Die worden uitgelezen via omgevingsvariabelen of Key Vault.

In `Program.cs` lees je de CORS-origins dynamisch uit de configuratie:

```csharp
string[] allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>()
    ?? Array.Empty<string>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("ShopWavePolicy", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});
```

**Minicontrole:** een developer voegt `appsettings.Production.json` toe aan `.gitignore`. Is dat correct? Leg uit waarom wel of niet.

---

## 5. Security headers

HTTPS versleutelt het transport. Security headers instrueren de browser om extra voorzorgsmaatregelen te nemen aan de kant van de client.

De belangrijkste headers voor ShopWave:

| Header | Wat het doet | In ASP.NET Core |
|--------|-------------|----------------|
| `Strict-Transport-Security` | Dwingt HTTPS af voor toekomstige requests | `UseHsts()` |
| `X-Content-Type-Options: nosniff` | Voorkomt MIME-type sniffing | Standaard in moderne .NET |
| `X-Frame-Options: DENY` | Voorkomt dat de pagina in een iframe geladen wordt | Via middleware |
| `Content-Security-Policy` | Beperkt welke bronnen de browser mag laden | Via middleware |

HSTS (HTTP Strict Transport Security) instrueer je de browser dat hij voor de volgende 365 dagen altijd HTTPS moet gebruiken voor dit domein. Ook als een gebruiker `http://shopwave.be` intypt, schakelt de browser automatisch over naar HTTPS zonder de server te vragen.

```csharp
app.UseHsts(); // Stuurt Strict-Transport-Security header mee
app.UseHttpsRedirection(); // Redirect HTTP naar HTTPS
```

`UseHsts()` werkt enkel als `ASPNETCORE_ENVIRONMENT` niet `Development` is. ASP.NET Core schakelt het automatisch uit in development omdat het hinderlijk is tijdens lokale tests.

**Minicontrole:** een aanvaller zit tussen een gebruiker en de server (man-in-the-middle). De gebruiker typt `http://shopwave.be`. Hoe beschermt HSTS de gebruiker in dit scenario?

---

## 6. OWASP Top 10: status van ShopWave

Na tien lessen heeft ShopWave alle behandelde OWASP-kwetsbaarheden aangepakt:

| # | Kwetsbaarheid | Behandeld in | Status ShopWave |
|---|--------------|--------------|-----------------|
| A01 | Broken Access Control | Les 7 | JWT met rolgebaseerde autorisatie |
| A02 | Cryptographic Failures | Les 1, 6 | BCrypt, AES-256 met random IV, TLS 1.3 |
| A03 | Injection | Les 8 | Parameterized queries, input validatie |
| A04 | Insecure Design | Les 1, 4 | Lockout, 2FA, defense in depth |
| A05 | Security Misconfiguration | Les 6, 7, 8 | Omgevingsvariabelen, CORS, productieconfiguratie |
| A06 | Vulnerable Components | Les 8, 9 | `dotnet list package --vulnerable` |
| A07 | Auth and Session Failures | Les 1, 4, 7 | BCrypt, 2FA, JWT met vervaldatum |
| A08 | Software and Data Integrity | Les 2 | Digitale handtekeningen op orders |
| A09 | Logging and Monitoring Failures | Les 10 | Productielogging via Warning-niveau, geen stack traces |
| A10 | SSRF | Niet behandeld | Buiten scope van deze cursus |

**Defense in depth** is het principe dat al deze maatregelen samen realiseren. Een aanvaller die door laag 1 breekt (een JWT steelt), stuit op laag 2 (versleutelde data in de database). Een aanvaller die de database inziet, leest geen leesbare wachtwoorden want die zijn gehasht met BCrypt. Elke laag beperkt de schade van een aanval op de lagen daarboven.

**Minicontrole:** een aanvaller slaagt erin een JWT-token te stelen van een ingelogde gebruiker. Welke beveiligingsmaatregelen in ShopWave beperken de schade, en welke schade kan hij alsnog aanrichten?

---

## 7. DevSecOps

**DevSecOps** integreert security in elk stadium van het ontwikkelproces. In plaats van security als laatste stap te controleren voor een release, bouw je het in van bij het eerste commit.

```
Commit --> Build --> Test --> Security Scan --> Package --> Deploy
                      |            |
               Unit tests     SAST: dotnet list --vulnerable
               Integration    DAST: OWASP ZAP op draaiende app
               Acceptatie
```

**SAST** (Static Application Security Testing) analyseert de broncode zonder de applicatie uit te voeren. Het vindt hardcoded geheimen, onveilige API-aanroepen en bekende kwetsbare patronen. `dotnet list package --vulnerable` is een eenvoudige SAST-check.

**DAST** (Dynamic Application Security Testing) test de draaiende applicatie van buitenaf. OWASP ZAP stuurt honderden geautomatiseerde aanvalsverzoeken en rapporteert kwetsbaarheden. Dit vindt problemen die pas zichtbaar zijn tijdens uitvoering, zoals ontbrekende security headers of CORS-misconfiguraties.

Een minimale GitHub Actions-pipeline voor ShopWave:

```yaml
name: ShopWave Security Pipeline

on: [push, pull_request]

jobs:
  build-and-test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.x'

      - name: Restore
        run: dotnet restore

      - name: Build
        run: dotnet build --no-restore

      - name: Tests
        run: dotnet test --no-build

      - name: Kwetsbare packages controleren
        run: |
          dotnet list package --vulnerable --include-transitive 2>&1 | tee vuln.txt
          if grep -q "Critical\|High" vuln.txt; then
            echo "Kritieke kwetsbare packages gevonden."
            exit 1
          fi
```

Deze pipeline weigert een merge als een NuGet-package een kritieke of hoge kwetsbaarheid heeft. Dat is OWASP A06 (Vulnerable Components) geautomatiseerd.

**Minicontrole:** een pipeline voert `dotnet list package --vulnerable` uit maar de build slaagt altijd, ook als er kwetsbare packages zijn. Wat ontbreekt er in de pipeline-configuratie?

---

## 8. Demo: ShopWave productie-ready maken

Start de ShopWave API via `dotnet run --project ShopWave.Api`. Open daarna een tweede terminal voor de tests.

---

### Stap 8a: Omgeving controleren

Controleer de huidige omgeving en wat dat betekent voor het gedrag van de API:

```powershell
# Huidige omgeving tonen
$env:ASPNETCORE_ENVIRONMENT

# Als de variabele leeg is, gedraagt de applicatie zich als Development
dotnet run --project ShopWave.Api
```

Roep daarna `/crash` aan en noteer de response:

```powershell
curl.exe -k https://localhost:5001/crash
```

**Wat je ziet:** de volledige stack trace met bestandspaden. Dit is development-gedrag.

---

### Stap 8b: Overschakelen naar productie

Stop de API. Stel de omgeving in op `Production` en herstart:

```powershell
$env:ASPNETCORE_ENVIRONMENT = "Production"
dotnet run --project ShopWave.Api
```

Roep `/crash` opnieuw aan:

```powershell
curl.exe -k https://localhost:5001/crash
```

**Wat je ziet:** alleen `Er is een fout opgetreden.` Geen stack trace, geen bestandspaden. Productiegedrag is actief.

---

### Stap 8c: Swagger uitschakelen in productie

Voeg in `ShopWave.Api/Program.cs` een omgevingscheck toe rond Swagger:

```csharp
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
```

Herstart in productie en controleer:

```powershell
curl.exe -k https://localhost:5001/swagger/index.html
```

**Wat je ziet:** `404 Not Found`. Swagger is niet beschikbaar in productie. Een aanvaller kan de endpoint-structuur niet meer via Swagger in kaart brengen.

---

### Stap 8d: Logging-niveau controleren

Voeg tijdelijk logging toe aan een endpoint in `Program.cs`:

```csharp
app.MapGet("/", (ILogger<Program> logger) =>
{
    logger.LogDebug("Debug-bericht: rootendpoint aangeroepen");
    logger.LogInformation("Informatie: rootendpoint aangeroepen");
    logger.LogWarning("Waarschuwing: rootendpoint aangeroepen");
    return Results.Ok("ShopWave API is actief.");
});
```

Start de API in development en roep het endpoint aan. Noteer welke berichten verschijnen in de console.

Stop de API, stel `ASPNETCORE_ENVIRONMENT=Production` in en herstart. Roep het endpoint opnieuw aan.

**Wat je ziet in development:** alle drie de berichten verschijnen.

**Wat je ziet in productie:** enkel het `LogWarning`-bericht verschijnt. `Debug` en `Information` worden onderdrukt door het `Warning`-logniveau in `appsettings.Production.json`.

---

### Stap 8e: `appsettings.Production.json` aanmaken

Maak `ShopWave.Api/appsettings.Production.json` aan:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "Cors": {
    "AllowedOrigins": ["https://shopwave.be"]
  }
}
```

**Wat je ziet:** de API laadt nu automatisch de productie-overrides wanneer `ASPNETCORE_ENVIRONMENT=Production`. Development-instellingen uit `appsettings.Development.json` worden genegeerd.

---

### Stap 8f: JWT-sleutel via omgevingsvariabele verifiëren

Controleer of de JWT-configuratie correct omgaat met een ontbrekende sleutel. Stop de API en verwijder de omgevingsvariabele tijdelijk:

```powershell
$env:JWT_SECRET_KEY = $null
dotnet run --project ShopWave.Api
```

**Wat je ziet:** de applicatie start niet op. Je ziet de foutmelding:

```
InvalidOperationException: Omgevingsvariabele JWT_SECRET_KEY ontbreekt.
```

Dit is het gewenste gedrag: de applicatie weigert te starten zonder de sleutel. Zo ontdek je een configuratiefout onmiddellijk bij de deployment, niet pas als de eerste gebruiker probeert in te loggen.

Herstel de sleutel:

```powershell
$env:JWT_SECRET_KEY = "ShopWaveGeheimeSleutel2024!!XYZ#"
dotnet run --project ShopWave.Api
```

---

### Stap 8g: Security headers inspecteren

Controleer welke security headers de API terugstuurt:

```powershell
curl.exe -k -v https://localhost:5001/ 2>&1 | Select-String "strict-transport|x-content|x-frame"
```

**Wat je ziet in development:** de `Strict-Transport-Security`-header ontbreekt. `UseHsts()` is uitgeschakeld in development.

Schakel over naar productie en herhaal:

```powershell
$env:ASPNETCORE_ENVIRONMENT = "Production"
dotnet run --project ShopWave.Api
curl.exe -k -v https://localhost:5001/ 2>&1 | Select-String "Strict-Transport"
```

**Wat je ziet:**

```
< strict-transport-security: max-age=2592000
```

HSTS is actief. De browser onthoudt dat dit domein 2.592.000 seconden (30 dagen) altijd HTTPS moet gebruiken.

---

### Stap 8h: Deployment-checklist doorlopen

Maak een instantie van de `SecurityChecklist`-klasse (gebouwd in oefening 2) en doorloop de volledige checklist voor ShopWave:

```csharp
SecurityChecklist checklist = new SecurityChecklist();

checklist.AddItem("Auth",    "Passwords hashed with BCrypt");
checklist.AddItem("Auth",    "2FA active");
checklist.AddItem("Auth",    "JWT with expiry");
checklist.AddItem("Auth",    "Account lockout after 3 failed attempts");
checklist.AddItem("Auth",    "Rate limiting on login endpoint");
checklist.AddItem("Data",    "AES-256 encryption with random IV");
checklist.AddItem("Data",    "No plaintext passwords in logs");
checklist.AddItem("Network", "HTTPS active");
checklist.AddItem("Network", "HSTS configured");
checklist.AddItem("Network", "CORS restricted to known origins");
checklist.AddItem("Network", "Swagger disabled in production");
checklist.AddItem("Config",  "JWT_SECRET_KEY via environment variable");
checklist.AddItem("Config",  "Developer Exception Page off in production");
checklist.AddItem("Deps",    "No critical or high vulnerable packages");

checklist.SetStatus("Passwords hashed with BCrypt",          "Implemented", "BCrypt.Net-Next via PasswordHasher");
checklist.SetStatus("2FA active",                            "Implemented", "TwoFactorService with callback");
checklist.SetStatus("JWT with expiry",                       "Implemented", "60 minutes via JwtTokenService");
checklist.SetStatus("Account lockout after 3 failed attempts","Implemented","AccountRepository.Login()");
checklist.SetStatus("Rate limiting on login endpoint",       "Implemented", "FixedWindowLimiter, 5 per minute");
checklist.SetStatus("AES-256 encryption with random IV",     "Implemented", "AesEncryptor");
checklist.SetStatus("No plaintext passwords in logs",        "Implemented", "BCrypt, nooit plain-text gelogd");
checklist.SetStatus("HTTPS active",                          "Implemented", "UseHttpsRedirection + Kestrel");
checklist.SetStatus("HSTS configured",                       "Implemented", "UseHsts()");
checklist.SetStatus("CORS restricted to known origins",      "Implemented", "WithOrigins(allowedOrigins)");
checklist.SetStatus("Swagger disabled in production",        "Implemented", "IsDevelopment()-check");
checklist.SetStatus("JWT_SECRET_KEY via environment variable","Implemented","GetEnvironmentVariable");
checklist.SetStatus("Developer Exception Page off in production","Implemented","IsDevelopment()-check");
checklist.SetStatus("No critical or high vulnerable packages","Partial",    "Handmatig gecontroleerd, nog geen CI");

checklist.PrintReport();
```

**Wat je ziet:**

```
=== ShopWave Security Checklist ===

[Auth]
  [OK] Passwords hashed with BCrypt
       BCrypt.Net-Next via PasswordHasher
  [OK] 2FA active
       TwoFactorService with callback
  ...

[Deps]
  [!!] No critical or high vulnerable packages
       Handmatig gecontroleerd, nog geen CI

Volledig geimplementeerd: 13/14
Gedeeltelijk: 1/14
Niet geimplementeerd: 0/14
```

---

## 9. Samenvatting

| Concept | Wat je moet onthouden |
|---------|-----------------------|
| Development vs productie | `ASPNETCORE_ENVIRONMENT` bepaalt het gedrag. Altijd `Production` instellen op een server. |
| Secrets | Nooit in broncode of git. Lokaal via omgevingsvariabelen. Op server via permanente env vars of Key Vault. |
| Self-signed certificaat | Alleen voor development. Browsers vertrouwen het niet in productie. |
| Let's Encrypt | Gratis, automatisch vernieuwd, vertrouwd door alle browsers. Vereist een domeinnaam. |
| `appsettings.Production.json` | Overschrijft development-instellingen. Bevat geen secrets. Mag wel in git. |
| HSTS | Instrueert de browser om altijd HTTPS te gebruiken. Automatisch uitgeschakeld in development. |
| Defense in depth | Meerdere lagen beveiliging. Als een laag faalt, houdt de volgende stand. |
| OWASP Top 10 | A01 t.e.m. A09 behandeld in ShopWave. A10 (SSRF) buiten scope. |
| SAST | Analyseert broncode. `dotnet list package --vulnerable` is een eenvoudige SAST-check. |
| DAST | Test de draaiende applicatie. OWASP ZAP is het meest gebruikte gratis DAST-tool. |
| DevSecOps | Security geautomatiseerd in elke CI/CD-run. Kwetsbare packages blokkeren de build. |
