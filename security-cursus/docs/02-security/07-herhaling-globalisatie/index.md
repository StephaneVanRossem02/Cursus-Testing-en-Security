---
title: "Les 12: Herhaling en Globalisatie — ShopWave compleet"
sidebar_label: "Herhaling & Globalisatie"
---

# Les 12: Herhaling en Globalisatie — ShopWave compleet

## ShopWave

Twaalf lessen lang hebben we ShopWave gebouwd. Van de eerste unit test in Les 1 tot de eerste pentest in Les 11 — elke les voegde een nieuwe laag toe aan een applicatie die nu zowel functioneel als beveiligd is. In deze laatste les maken we de cirkel rond.

We kijken terug op wat we gebouwd hebben, brengen het grote plaatje in kaart, en kijken vooruit: hoe integreer je security en testing in een professionele DevSecOps-pipeline?

---

## De ShopWave-architectuur: het grote plaatje

### Componentenoverzicht

```
┌─────────────────────────────────────────────────────────────────┐
│                        ShopWave API                             │
│                                                                 │
│  ┌──────────────┐   ┌──────────────┐   ┌──────────────────┐    │
│  │ AccountRepo  │   │ OrderService │   │ CheckoutService  │    │
│  │ (Les 2)      │   │ (Les 1, 3)   │   │ (Les 5)          │    │
│  └──────┬───────┘   └──────┬───────┘   └────────┬─────────┘    │
│         │                  │                    │               │
│  ┌──────▼───────┐   ┌──────▼───────┐   ┌────────▼─────────┐    │
│  │ Password-    │   │ CouponService│   │ ShippingClient   │    │
│  │ Hasher(BCrypt│   │ (Les 3)      │   │ (Les 10)         │    │
│  │ Les 2)       │   └──────────────┘   └──────────────────┘    │
│  └──────┬───────┘                                              │
│         │                                                       │
│  ┌──────▼───────┐   ┌──────────────┐   ┌──────────────────┐    │
│  │ TwoFactor-   │   │ JwtToken-    │   │ AesEncryptor     │    │
│  │ Service (Les4│   │ Service(Les 7│   │ (Les 2, 6)       │    │
│  └──────────────┘   └──────────────┘   └──────────────────┘    │
└─────────────────────────────────────────────────────────────────┘
         ↑                    ↑                    ↑
  HTTPS/TLS (Les 6)    JWT Bearer (Les 7)   Mockoon/WireMock (Les 10)
```

### De beveiligingslagen

Beveiliging is geen één-op-één maatregel maar een gelaagde verdediging (*defense in depth*). ShopWave heeft vier lagen:

| Laag | Wat beschermt het? | Technologie in ShopWave |
|------|--------------------|------------------------|
| **Datalaaag** | Data-at-rest | BCrypt (wachtwoorden), AES-256 met random IV (gevoelige data) |
| **Transportlaag** | Data-in-transit | HTTPS/TLS 1.3, Kestrel met X.509-certificaat |
| **Applicatielaag** | Toegangscontrole | JWT Bearer tokens, 2FA, account lockout, rate limiting |
| **Codelaag** | Kwetsbaarheden in code | Parameterized queries, output encoding, input validatie, CORS |

Een aanvaller die door laag 3 breekt (een JWT steelt), stuit nog op laag 1 (versleutelde data in de database). Zo beperkt elke laag de schade.

---

## Herhaling: alle concepten

### Testing

| Les | Teststrategie | Wanneer? | Tool |
|-----|--------------|----------|------|
| 1 | Unit testing | Geïsoleerde logica | xUnit, FluentAssertions, Moq |
| 3 | TDD | Nieuwe functionaliteit | xUnit — test first |
| 5 | Integration testing | Samenwerking tussen klassen | xUnit met echte objecten |
| 8 | Acceptatietesten (BDD) | Gedragsspecificaties | Reqnroll, Gherkin |
| 10 | Integration testing (HTTP) | Externe services | Mockoon, WireMock.Net |
| 11 | Penetration testing | Beveiligingsverificatie | Burp Suite, OWASP ZAP |

**De testpiramide:**

```
            /\
           /  \
          / E2E\       ← Weinig — duur, traag
         /------\
        / Integr-\     ← Matig — echte klassen of externe mock
       /----------\
      / Unit tests  \  ← Veel — snel, geïsoleerd, altijd groen
     /--------------\
```

### Security

| Les | Concept | CIA-pijler | Implementatie |
|-----|---------|-----------|---------------|
| 2 | Wachtwoord-hashing | Confidentiality | BCrypt.Net-Next |
| 2 | Symmetrische encryptie | Confidentiality | AES-256 + random IV |
| 4 | 2FA | Confidentiality | TOTP-simulatie + callback |
| 4 | Digitale handtekening | Integrity | RSA + SHA-256 |
| 4 | X.509-certificaten | Confidentiality, Integrity | Self-signed voor dev |
| 6 | HTTPS/TLS | Confidentiality, Integrity | Kestrel, TLS 1.3 |
| 7 | JWT | Confidentiality, Integrity | `Microsoft.AspNetCore.Authentication.JwtBearer` |
| 9 | SQL injection fix | Integrity, Confidentiality | Parameterized queries |
| 9 | XSS fix | Integrity | Output encoding |
| 9 | Rate limiting | Availability | `AddRateLimiter` |
| 9 | CORS | Confidentiality | `WithOrigins(...)` |
| 11 | Pentest | Alle drie | Burp Suite, OWASP ZAP |

### OWASP Top 10 — status na de cursus

| # | Kwetsbaarheid | Behandeld in | Status ShopWave |
|---|--------------|--------------|-----------------|
| A01 | Broken Access Control | Les 7 | ✔ JWT + rolgebaseerde autorisatie |
| A02 | Cryptographic Failures | Les 2, 6 | ✔ BCrypt, AES-256, TLS 1.3 |
| A03 | Injection | Les 9 | ✔ Parameterized queries, input validatie |
| A04 | Insecure Design | Les 2, 4 | ✔ Lockout, 2FA, defense in depth |
| A05 | Security Misconfiguration | Les 6, 7, 9 | ✔ Omgevingsconfiguratie, User Secrets, CORS |
| A06 | Vulnerable Components | Les 9, 11 | ✔ `dotnet list package --vulnerable` |
| A07 | Auth & Session Failures | Les 2, 4, 7 | ✔ BCrypt, 2FA, JWT met vervaldatum |
| A08 | Software & Data Integrity | Les 4 | ✔ Digitale handtekeningen |
| A09 | Logging & Monitoring | — | ⚠ Niet expliciet behandeld — zie verdere studie |
| A10 | SSRF | — | ⚠ Niet behandeld — zie verdere studie |

---

## DevSecOps: security in de CI/CD-pipeline

**DevSecOps** integreert beveiligingstests in het continue integratie- en leveringsproces. Beveiliging is niet een eindcontrole — het wordt bij elke commit gecontroleerd.

### Een typische DevSecOps-pipeline

```
Commit → Build → Test → Security Scan → Package → Deploy
                  ↑            ↑
           Unit tests    SAST + dependency scan
           Integration   (dotnet list --vulnerable)
           Acceptance    OWASP ZAP (DAST)
```

**SAST (Static Application Security Testing):** analyseert de broncode zonder de applicatie uit te voeren.
**DAST (Dynamic Application Security Testing):** test de draaiende applicatie — zoals OWASP ZAP.

### Minimale security-checks in een CI/CD-pipeline (GitHub Actions)

```yaml
name: ShopWave Security Pipeline

on: [push, pull_request]

jobs:
  security:
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

      - name: Unit & Integration Tests
        run: dotnet test --no-build

      - name: Check vulnerable packages (OWASP A06)
        run: |
          dotnet list package --vulnerable --include-transitive 2>&1 | tee vuln.txt
          if grep -q "critical\|high" vuln.txt; then
            echo "Kritieke kwetsbare packages gevonden!"
            exit 1
          fi
```

Deze pipeline weigert een merge als er kritieke of hoge kwetsbaarheden in NuGet-packages zitten.

---

## De ShopWave-beveiligingschecklist

Gebruik deze checklist vóór elke release van ShopWave:

### Authenticatie en autorisatie
- [ ] Wachtwoorden gehasht met BCrypt (nooit SHA-256 of MD5)
- [ ] JWT heeft een vervaldatum (`exp`-claim)
- [ ] JWT-sleutel opgeslagen in User Secrets / omgevingsvariabele (niet hardcoded)
- [ ] 2FA geactiveerd voor gevoelige accounts
- [ ] Account lockout na maximaal 3 mislukte pogingen
- [ ] Rate limiting geconfigureerd op het `/login`-endpoint

### Data
- [ ] Gevoelige data versleuteld met AES-256 en random IV
- [ ] Geen plain-text wachtwoorden of sleutels in de broncode of logs
- [ ] Database gebruikt parameterized queries (geen string interpolatie in SQL)

### Netwerk en configuratie
- [ ] HTTPS verplicht — geen HTTP-endpoints in productie
- [ ] HSTS geconfigureerd (`UseHsts()`)
- [ ] CORS beperkt tot bekende origins (`WithOrigins(...)`)
- [ ] Foutmeldingen geven geen stacktrace of interne paden terug in productie
- [ ] Debug-modus uitgeschakeld in productie (`ASPNETCORE_ENVIRONMENT=Production`)

### Dependencies
- [ ] `dotnet list package --vulnerable` — geen critical of high
- [ ] Alle packages up-to-date

### Testing
- [ ] Unit tests slagen — coverage > 80% voor kernlogica
- [ ] Integration tests slagen
- [ ] Acceptatietests (BDD) slagen
- [ ] Pentest uitgevoerd — bevindingen gedocumenteerd en opgelost

---

## Demo: het grote overzicht in code

Voeg in `Program.cs` van de ShopWave API alle beveiligingslagen samen:

```csharp
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Threading.RateLimiting;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// ── Authenticatie (Les 7) ─────────────────────────────────────────
string secretKey = builder.Configuration["Jwt:SecretKey"]
    ?? throw new InvalidOperationException("JWT SecretKey ontbreekt.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = "shopwave-api",
            ValidAudience            = "shopwave-client",
            IssuerSigningKey         = new SymmetricSecurityKey(
                                           Encoding.UTF8.GetBytes(secretKey))
        };
    });

builder.Services.AddAuthorization();

// ── Rate limiting (Les 9) ─────────────────────────────────────────
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("login", limiterOptions =>
    {
        limiterOptions.Window      = TimeSpan.FromMinutes(1);
        limiterOptions.PermitLimit = 5;
    });
});

// ── CORS (Les 9) ──────────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("ShopWavePolicy", policy =>
    {
        policy.WithOrigins("https://shopwave.be")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

WebApplication app = builder.Build();

// ── Middleware-pipeline ───────────────────────────────────────────
app.UseHsts();           // HSTS (Les 6)
app.UseHttpsRedirection();
app.UseCors("ShopWavePolicy");
app.UseRateLimiter();
app.UseAuthentication(); // JWT
app.UseAuthorization();

// ── Endpoints ─────────────────────────────────────────────────────
app.MapPost("/login", (LoginRequest request) =>
{
    // ... authenticatie met AccountRepository (Les 2) + 2FA (Les 4)
    return Results.Ok(new { token = "..." });
}).RequireRateLimiting("login");

app.MapGet("/orders", () =>
{
    return Results.Ok(new[] { "ORD-001", "ORD-002" });
}).RequireAuthorization();

app.MapGet("/admin", () =>
{
    return Results.Ok("Welkom, admin");
}).RequireAuthorization().RequireRole("admin");

app.Run();

record LoginRequest(string Email, string Wachtwoord);
```

Dit is ShopWave in productie-staat: elke les heeft bijgedragen aan één of meer lagen van deze configuratie.

---

## Oefeningen

### Oefening 1 — Beveiligingschecklist voor je eigen project

Pas de ShopWave-beveiligingschecklist toe op een project dat je eerder hebt gemaakt (of op ShopWave zelf). Markeer elk item als:
- ✔ Geïmplementeerd
- ⚠ Gedeeltelijk — leg uit wat ontbreekt
- ✖ Niet geïmplementeerd — leg uit welk risico dit inhoudt

---

### Oefening 2 — CIA-analyse van de volledige applicatie

Beschrijf voor ShopWave als geheel hoe elk van de drie CIA-pijlers beschermd wordt:

1. **Confidentiality** — welke technologieën beschermen gevoelige data?
2. **Integrity** — welke mechanismen garanderen dat data correct en ongewijzigd is?
3. **Availability** — welke maatregelen zorgen dat de applicatie bereikbaar blijft?

Geef voor elke pijler minstens drie concrete voorbeelden uit de cursus.

---

### Oefening 3 — DevSecOps-pipeline uitbreiden

Breid de CI/CD-pipeline uit met een stap die OWASP ZAP uitvoert als DAST-scan:

```yaml
- name: OWASP ZAP Baseline Scan
  uses: zaproxy/action-baseline@v0.9.0
  with:
    target: 'https://localhost:5001'
    rules_file_name: '.zap/rules.tsv'
    cmd_options: '-a'
```

Onderzoek: wat rapporteert ZAP als bevindingen? Zijn er false positives? Hoe sluit je die uit?

---

### Oefening 4 — Reflectie

Beantwoord de volgende vragen op basis van de volledige cursus:

1. Welke les of concept had de grootste impact op hoe je over software-ontwikkeling nadenkt? Waarom?
2. Welke beveiligingsfout kom je in de praktijk het vaakst tegen? Hoe zou je die voorkomen?
3. Hoe zou je de testpiramide toepassen in een project van twee weken?
4. Een collega zegt: "We testen niet, we hebben geen tijd." Hoe reageer je?
5. Wat is het verschil tussen een beveiligingsaudit en een pentest?

---

## Samenvatting van de cursus

### Teststrategieën

| Strategie | Kern | Wanneer |
|-----------|------|---------|
| Unit testing | Test geïsoleerde logica met mocks | Altijd — bij elke klasse |
| TDD | Test eerst, implementeer daarna | Nieuwe functionaliteit |
| Integration testing | Test samenwerking van echte klassen | Na unit tests |
| BDD / Acceptatietests | Test gedrag in begrijpelijke taal | Wanneer requirements als scenario's |
| Penetration testing | Test beveiliging als een aanvaller | Vóór elke release |

### Beveiligingsprincipes

| Principe | Kern |
|----------|------|
| CIA-model | Confidentiality, Integrity, Availability — elke beveiligingsbeslissing |
| Defense in depth | Meerdere lagen — als één laag faalt, houdt de volgende stand |
| Least privilege | Geef alleen de rechten die nodig zijn |
| Fail securely | Bij een fout: weiger toegang, log de fout, geef geen details |
| Never trust input | Valideer altijd server-side, ongeacht client-side validatie |
| Secrets management | Nooit hardcoded — User Secrets, omgevingsvariabelen, Key Vault |

---

## Verdere studie

Je hebt nu een solide fundament. Hier zijn de logische vervolgstappen:

**Certificeringen:**
- **CompTIA Security+** — brede beveiligingscertificering, ideaal als eerste stap
- **CEH (Certified Ethical Hacker)** — focus op penetration testing
- **OSCP (Offensive Security Certified Professional)** — hands-on pentesting, geavanceerd niveau

**Bronnen:**
- **OWASP (owasp.org)** — volledige Top 10, cheat sheets, testgids
- **PortSwigger Web Security Academy (portswigger.net/web-security)** — gratis, interactieve labs voor elke OWASP-kwetsbaarheid
- **HackTheBox / TryHackMe** — legale oefenomgevingen voor ethisch hacken
- **Microsoft Learn — Security** — .NET-specifieke beveiligingsdocumentatie
- **"The Art of Software Testing" — Glenford Myers** — klassieker over testtechnieken

**Tools om te verkennen:**
- **SonarQube / SonarCloud** — statische code-analyse voor beveiligingsfouten
- **Snyk** — dependency scanning geïntegreerd in de IDE
- **OWASP Dependency-Check** — alternatief voor `dotnet list package --vulnerable`
- **Azure Key Vault / HashiCorp Vault** — professioneel secrets management

---

*ShopWave begon als een lege console-applicatie. Twaalf lessen later is het een beveiligde, geteste API met een solide beveiligingsarchitectuur, uitgebreide testdekking en een gestructureerde aanpak voor security testing. Dat is wat professionele software-ontwikkeling inhoudt: niet alleen code schrijven die werkt, maar code schrijven die bestand is.*