---
title: "Oplossingsprojecten downloaden"
sidebar_label: "Oplossingen downloaden"
sidebar_position: 3
---

# Oplossingsprojecten downloaden

Bij elke les hoort een volledig uitgewerkt ShopWave-project dat je kan openen in Visual Studio en meteen kan bouwen en testen.

**Eerst zelf proberen.** Deze projecten zijn een startpunt en referentie, geen huiswerk om over te schrijven. De waarde van de oefeningen zit in het zelf denken. Download een project pas nadat je de oefening zelf geprobeerd hebt, of als je vastzit en wil zien hoe het eindpunt van de vorige les eruitziet.

## Alles in een keer

[**Alle twaalf de lessen downloaden**](/downloads/shopwave-oplossingen-alle-lessen.zip) (ZIP, ongeveer 400 kB)

## Per les

Elk project is **cumulatief**: les 5 bevat alles van les 1 tot en met 4, plus wat les 5 toevoegt. Zit je vast in een les, dan download je die les en heb je meteen een werkend vertrekpunt.

| Les | Onderwerp | Download |
|-----|-----------|----------|
| 1 | Unit Testing en Mocking | [ZIP](/downloads/shopwave-01-unit-testing-en-mocking.zip) |
| 2 | CIA, Hashing en Encryptie | [ZIP](/downloads/shopwave-02-cia-hashing-en-encryptie.zip) |
| 3 | Test Driven Development | [ZIP](/downloads/shopwave-03-test-driven-development.zip) |
| 4 | 2FA, Handtekeningen en X.509 | [ZIP](/downloads/shopwave-04-2fa-handtekeningen-en-x509.zip) |
| 5 | Integration Testing | [ZIP](/downloads/shopwave-05-integration-testing.zip) |
| 6 | HTTPS en TLS | [ZIP](/downloads/shopwave-06-https-en-tls.zip) |
| 7 | JWT en OAuth2 | [ZIP](/downloads/shopwave-07-jwt-en-oauth2.zip) |
| 8 | Acceptatietesten | [ZIP](/downloads/shopwave-08-acceptatietesten.zip) |
| 9 | Secure Coding (OWASP) | [ZIP](/downloads/shopwave-09-secure-coding-owasp.zip) |
| 10 | Integration Testing (Mockoon) | [ZIP](/downloads/shopwave-10-integration-testing-mockoon.zip) |
| 11 | Ethisch Hacken | [ZIP](/downloads/shopwave-11-ethisch-hacken.zip) |
| 12 | ShopWave in Productie | [ZIP](/downloads/shopwave-12-shopwave-in-productie.zip) |

## Hoe gebruik je het

1. Pak de ZIP uit op een plek naar keuze.
2. Open `ShopWave.sln` in Visual Studio. Of werk vanaf de commandolijn.
3. Bouw en voer de tests uit:

```csharp
dotnet build
dotnet test
```

Alles hoort groen te zijn. In de `README.md` van elke les staat wat er nieuw is ten opzichte van de vorige les, welke NuGet-pakketten nodig zijn en hoeveel tests er horen te slagen.

## Wat zit erin

| Project | Vanaf | Inhoud |
|---------|-------|--------|
| `ShopWave` | les 1 | De domeincode. Demo's uit de theorie staan in `Demos/`. |
| `ShopWave.Tests` | les 1 | Unit- en integratietests met xUnit, Moq en FluentAssertions. |
| `ShopWave.Api` | les 6 | De ASP.NET Core API met HTTPS, JWT en de securitymaatregelen. |
| `ShopWave.Specs` | les 8 | Acceptatietests in Gherkin met Reqnroll. |

## Goed om te weten

- Je hebt de **.NET 8 SDK** nodig. De projecten targeten `net8.0`.
- De **Mockoon-tests in les 10 en verder staan op `Skip`**. Die hebben een handmatig gestarte Mockoon-server nodig op poort 3001. De WireMock-varianten testen hetzelfde en draaien wel automatisch mee.
- Vanaf les 7 leest `ShopWave.Api` de JWT-sleutel uit de omgevingsvariabele `JWT_SECRET_KEY`. Die heb je alleen nodig om de API te **draaien**, niet om te bouwen of te testen.

Liever de broncode bekijken zonder te downloaden? Alles staat ook op [GitHub](https://github.com/StephaneVanRossem02/Cursus-Testing-en-Security/tree/main/security-cursus/solutions).
