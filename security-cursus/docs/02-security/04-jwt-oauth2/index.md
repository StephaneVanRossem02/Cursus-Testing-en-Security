---
title: "Les 7: JWT en OAuth2"
sidebar_label: "Overzicht"
---

# Les 7: JWT en OAuth2

## ShopWave

In les 6 draait de ShopWave API op HTTPS. De verbinding tussen client en server is versleuteld. Maar elk endpoint is nog steeds toegankelijk voor iedereen die het adres kent. Iemand kan bestellingen opvragen van elke andere klant, zonder enige controle.

HTTPS beveiligt het **transport**. JWT beveiligt de **endpoints**. Na een geslaagde login krijgt de klant een token. Bij elke request naar een beveiligd endpoint stuurt de client dat token mee. De server controleert de geldigheid voordat hij antwoord geeft.

In deze les bouw je die laag van authenticatie en autorisatie zelf.

---

## Leerdoelen

Na deze les kan je:

1. uitleggen waarom tokengebaseerde authenticatie beter schaalt dan sessiegebaseerde authenticatie
2. de drie onderdelen van een JWT benoemen en verklaren wat elk onderdeel doet
3. uitleggen waarom JWT geen encryptie is en wat dat betekent voor de inhoud van de payload
4. een `JwtTokenService` schrijven die tokens aanmaakt met claims, vervaltijd en een signature
5. JWT-authenticatie registreren in een ASP.NET Core Minimal API via `AddJwtBearer`
6. endpoints beveiligen met `.RequireAuthorization()` en rolgebaseerde toegangscontrole met `.RequireRole(...)`
7. de Authorization Code Flow van OAuth 2.0 beschrijven en het verschil uitleggen met JWT

---

## NuGet packages

Je hebt twee extra packages nodig in het project `ShopWave.Api`:

```csharp
Microsoft.AspNetCore.Authentication.JwtBearer
System.IdentityModel.Tokens.Jwt
```

Installeer via: rechtsklik op `ShopWave.Api` in Solution Explorer, kies "Manage NuGet Packages" en zoek op de naam.

---

## Navigatie

| Pagina | Inhoud |
|--------|--------|
| [Theorie](./theorie) | JWT-opbouw, JWT-flow, rolgebaseerde autorisatie, OAuth 2.0, demo stap voor stap |
| [Oefeningen](./oefeningen) | 5 implementatieoefeningen op basis van ShopWave |
| [Oplossingen](./oplossingen) | Volledige oplossingen met toelichting |
