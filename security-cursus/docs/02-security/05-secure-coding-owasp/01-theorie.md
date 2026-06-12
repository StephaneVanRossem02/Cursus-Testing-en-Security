---
title: "Les 9: Secure Coding — OWASP Top 10 — Theorie"
sidebar_label: "Theorie"
---

# Les 9: Secure Coding — OWASP Top 10 — Theorie

## OWASP Top 10 (2021)

| #   | Naam                           | Kernprobleem                               | ShopWave-voorbeeld                            |
|-----|--------------------------------|--------------------------------------------|-----------------------------------------------|
| A01 | Broken Access Control          | Acties buiten eigen rechten                | Klant raadpleegt bestellingen van andere klant |
| A02 | Cryptographic Failures         | Data onvoldoende versleuteld               | Wachtwoorden als plain text opgeslagen        |
| A03 | Injection                      | Onbetrouwbare data als code uitgevoerd     | SQL Injection in zoekfunctie                  |
| A04 | Insecure Design                | Beveiliging ontbreekt in architectuur      | Geen lockout na herhaalde mislukte logins     |
| A05 | Security Misconfiguration      | Debug-info in productie                    | Stack trace zichtbaar in foutmelding          |
| A06 | Vulnerable Components          | Verouderde libraries                       | NuGet-pakket met bekende CVE                  |
| A07 | Auth & Session Failures        | Zwakke authenticatie                       | JWT zonder vervaldatum                        |
| A08 | Software & Data Integrity      | Onbetrouwbare updates                      | NuGet-pakket vervangen door malafide versie   |
| A09 | Logging & Monitoring Failures  | Aanvallen niet gedetecteerd                | Geen logging van mislukte logins              |
| A10 | SSRF                           | Server vraagt URL op gestuurd door aanvaller | API haalt aanvaller-gecontroleerde URL op   |

---

## SQL Injection (A03)

Ontstaat wanneer gebruikersinput rechtstreeks in een query gevoegd wordt via string-interpolatie:

```csharp
// GEVAARLIJK — aanvaller stuurt: ' OR '1'='1
string query = $"SELECT * FROM orders WHERE email = '{email}'";
```

De payload `' OR '1'='1` maakt de WHERE-voorwaarde altijd waar → alle orders worden teruggegeven.

**Oplossing in ShopWave** (geen SQL, maar zelfde principe — nooit input in querystructuur):

```csharp
// VEILIG — input staat los van de filterlogica
List<string> resultaten = new List<string>();

foreach (string order in orderDatabase)
{
    string[] delen   = order.Split('|');
    bool     gevonden = false;

    if (delen.Length >= 1)
    {
        gevonden = delen[0].Trim().Equals(email, StringComparison.OrdinalIgnoreCase);
    }

    if (gevonden)
    {
        resultaten.Add(order);
    }
}
```

---

## XSS — Cross-Site Scripting (A03)

Ontstaat wanneer gebruikersinput onbewerkt als HTML weergegeven wordt.

```html
<!-- Aanvaller voert dit in als naam: -->
<script>document.cookie = 'gestolen=' + document.cookie</script>
```

**Oplossing:** nooit `@Html.Raw()` gebruiken; gebruik altijd een template engine die automatisch escapet, of `WebUtility.HtmlEncode()` bij handmatige output.

---

## Security Misconfiguration (A05)

Stack traces en interne paden mogen nooit in productie zichtbaar zijn:

```csharp
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/error"); // generiek foutscherm
}
```

---

## Secrets management (A05)

JWT-sleutels, connection strings en API-sleutels horen **nooit** in de broncode:

| Omgeving    | Gebruik                                               |
|-------------|-------------------------------------------------------|
| Development | `dotnet user-secrets` — lokaal, buiten de repository |
| Productie   | Omgevingsvariabelen of Azure Key Vault               |

---

## Vulnerable Components (A06)

Controleer regelmatig:

```bash
dotnet list package --vulnerable --include-transitive
```

Update kwetsbare packages onmiddellijk. Gebruik Dependabot of Snyk voor automatische meldingen.

---

## Input validatie

Valideer altijd **server-side** — client-side validatie is voor gebruikerservaring, niet voor beveiliging:

```csharp
if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
{
    return Results.BadRequest(new { fout = "Ongeldig e-mailadres." });
}
```

---

## CORS (Cross-Origin Resource Sharing)

Gebruik nooit `AllowAnyOrigin()` in productie — specificeer de toegestane origins expliciet:

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