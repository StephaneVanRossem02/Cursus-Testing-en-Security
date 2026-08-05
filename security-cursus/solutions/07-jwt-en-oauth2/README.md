# Les 7: JWT en OAuth2

Cumulatief oplossingsproject. Bevat alle code van les 1 t.e.m. les 6, plus de JWT-authenticatie- en autorisatielaag op `ShopWave.Api`.

## Wat is nieuw in deze les

In `ShopWave/Security/`:

- `JwtTokenService` - genereert een ondertekend JWT met claims (`sub`, `role`, `iat`). Deze klasse staat bewust in het gedeelde `ShopWave`-project: zo kunnen zowel `ShopWave.Api` als de console hem gebruiken. In `ShopWave.Api` zou dat niet kunnen, want dan zou de console een circulaire projectreferentie nodig hebben.

In `ShopWave.Api`:

- `TokenBlacklist` - houdt ingetrokken tokens bij (oefening 4).
- `Program.cs` - uitgebreid met JWT-bearer-authenticatie, rolgebaseerde autorisatie en beveiligde endpoints. De HTTPS/HSTS/security-headers uit les 6 blijven behouden (cumulatief). Nieuwe endpoints:
  - `POST /login` en `POST /verify` - loginflow met 2FA die bij succes een JWT teruggeeft.
  - `GET /me` - leest de claims uit het token (oefening 1).
  - `GET /orders/{email}` - beveiligd endpoint, vereist een geldig token.
  - `GET /admin/orders` - vereist de rol `admin` (oefening 2).
  - `POST /logout` - zet het token op de blacklist (oefening 4), met bijhorende middleware.

Demo-code uit de theorie (`ShopWave/Demos/`):

- `JwtClientDemo` - de volledige JWT-flow vanuit de console-client (login, verify, token, beveiligd endpoint met en zonder token, payload inspecteren). Interactief en tegen een draaiende API.
- `ExpiredTokenDemo` - maakt een token met vervaltijd 0 aan en toont dat de API het weigert (oefening 3).

Er zijn in deze les geen nieuwe unit tests (oefening 5 is een analyseoefening). De testsuite blijft op 54 tests en groen.

## Cumulatieve wijzigingen

- `ShopWave.Api/Program.cs` is de samengevoegde eindversie: de HTTPS/HSTS/headers en endpoints uit les 6 blijven, plus de JWT-configuratie en endpoints uit les 7. Het publieke `/`-endpoint toont nu de tekst met JWT. `HandleAdminOrders` geeft de lijst met bestellingen terug (oefening 2), niet de eenvoudige tekst uit de theorie-demo.
- De geheime sleutel wordt gelezen uit de omgevingsvariabele `JWT_SECRET_KEY` (nooit hardcoden). Bij het opstarten zonder die variabele gooit de API meteen een `InvalidOperationException`. Dit heeft geen effect op `dotnet build`/`dotnet test`, want die voeren de API niet uit.

### Opmerking over de plaats van JwtTokenService

Oefening 3 (`ExpiredTokenDemo`) draait vanuit het **consoleproject** en heeft `JwtTokenService` nodig. Daarom staat die klasse in `ShopWave/Security/` en niet in `ShopWave.Api`: het consoleproject kan nooit naar `ShopWave.Api` verwijzen, want dat zou een circulaire projectreferentie geven.

Dit was oorspronkelijk een structurele inconsistentie in de cursusbron (de theorie plaatste de klasse in `ShopWave.Api`, terwijl de oefening hem vanuit de console gebruikte). Die is intussen in de cursus zelf rechtgezet.

## NuGet-pakketten

- `ShopWave.Api`: `Microsoft.AspNetCore.Authentication.JwtBearer`, `System.IdentityModel.Tokens.Jwt`.
- `ShopWave`: `BCrypt.Net-Next`, `System.IdentityModel.Tokens.Jwt` (voor de JWT-clientdemo).
- `ShopWave.Tests`: `Microsoft.NET.Test.Sdk`, `xunit`, `xunit.runner.visualstudio`, `Moq`, `FluentAssertions`.

## Bouwen en testen

```bash
dotnet build
dotnet test
```

Beide horen groen te zijn (54 tests slagen, 1 waarschuwing uit de bron in `OrderService`).

De API draaien (met de geheime sleutel):

```powershell
$env:JWT_SECRET_KEY = "ShopWaveGeheimeSleutel2024!!XYZ#"
dotnet run --project ShopWave.Api
```

## Waarschuwing

Dit is een startpunt en referentie. Bekijk het pas nadat je de oefeningen zelf geprobeerd hebt.
