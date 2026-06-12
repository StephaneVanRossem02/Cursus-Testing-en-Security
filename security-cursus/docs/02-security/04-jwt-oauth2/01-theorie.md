---
title: "Les 7: Security — JWT en OAuth2 — Theorie"
sidebar_label: "Theorie"
---

# Les 7: Security — JWT en OAuth2 — Theorie

## Sessies versus tokens

| Methode        | Hoe werkt het?                                       | Nadeel                                   |
|----------------|------------------------------------------------------|------------------------------------------|
| Sessiegebaseerd | Server slaat sessie op; client stuurt sessie-ID mee | Slecht schaalbaat bij meerdere servers  |
| Tokengebaseerd  | Token bevat alle info; elke server kan valideren    | Token kan gestolen worden (korte levensduur aanbevolen) |

---

## Opbouw van een JWT

Een **JSON Web Token** bestaat uit drie Base64url-gecodeerde delen, gescheiden door punten:

```
header.payload.signature
```

- **Header** — algoritme (bv. `HS256`) en type (`JWT`)
- **Payload** — claims: `sub` (gebruiker), `exp` (vervaltijd), `role`, `iss` (uitgever), `aud` (ontvanger)
- **Signature** — HMACSHA256 van `header + "." + payload`, berekend met de geheime sleutel

De signature garandeert integriteit: wie de payload aanpast, maakt de signature ongeldig.

### Voorbeeld claim-set

```json
{
  "sub": "alice@shopwave.be",
  "role": "user",
  "iss": "shopwave-api",
  "aud": "shopwave-client",
  "exp": 1700000000
}
```

---

## JWT is geen encryptie

JWT is **Base64-gecodeerd**, niet versleuteld. Iedereen kan de payload lezen zonder de geheime sleutel.

> Zet nooit gevoelige informatie (wachtwoorden, betaalgegevens) in een JWT-payload. De signature garandeert alleen dat de inhoud niet gewijzigd is.

---

## Veelgemaakte JWT-fouten

| Fout                              | Gevolg                                       |
|-----------------------------------|----------------------------------------------|
| Geen vervaldatum (`exp`)          | Token geldig voor altijd                     |
| Geheime sleutel hardcoded         | Uitlekken via Git                            |
| `alg: none` accepteren            | Aanvaller kan eigen claims injecteren        |
| Gevoelige data in payload         | Leesbaar voor iedereen die het token heeft   |

---

## JWT in ASP.NET Core Minimal API

```csharp
// Authenticatie configureren
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(ConfigureJwtBearer);

builder.Services.AddAuthorization();

// Endpoint beveiligen
app.MapGet("/orders", HandleOrders).RequireAuthorization();

// Rolgebaseerde autorisatie
app.MapGet("/admin/orders", HandleAdminOrders)
    .RequireAuthorization(policy => policy.RequireRole("admin"));
```

---

## OAuth2

OAuth2 is een **autorisatieprotocol** — het regelt welke applicaties toegang krijgen tot welke resources, zonder dat de gebruiker zijn wachtwoord deelt.

### De Authorization Code Flow (vereenvoudigd)

1. Gebruiker klikt op "Inloggen met Google" in een app
2. App stuurt gebruiker door naar Google (met `client_id` en `redirect_uri`)
3. Gebruiker logt in bij Google en geeft toestemming
4. Google stuurt een **authorization code** terug naar de app
5. App wisselt die code in voor een **access token** (server-to-server, geheim)
6. App gebruikt het access token om Google-API's aan te roepen

### Belangrijke OAuth2-begrippen

| Begrip         | Betekenis                                                       |
|----------------|-----------------------------------------------------------------|
| Access token   | Kortlevend token voor API-toegang (bv. 15 minuten)             |
| Refresh token  | Langlevend token om een nieuw access token aan te vragen       |
| Scope          | Welke data/acties de app mag benaderen (bv. `read:orders`)     |
| Client ID      | Identificeert de applicatie bij de authorization server        |

---

## OAuth2 versus JWT

OAuth2 en JWT zijn **verschillende dingen** die vaak samen gebruikt worden:

- **OAuth2** is een protocol voor autorisatie (welke app mag wat)
- **JWT** is een formaat voor tokens (hoe ziet een token eruit)

OAuth2-servers geven vaak JWT's terug als access tokens — maar ze hoeven dat niet te doen.