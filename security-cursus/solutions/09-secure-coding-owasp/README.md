# Les 9: Secure Coding (OWASP)

Cumulatief oplossingsproject. Bevat alle code van les 1 t.e.m. les 8, plus de secure-coding-maatregelen van deze les op `ShopWave.Api`.

## Wat is nieuw in deze les

In `ShopWave.Api/Program.cs`:

- **SQL Injection**: `/orders/zoek` (op e-mail) en `/orders/zoek-product` (op productnaam), beide in de veilige versie die data en query-structuur scheidt. Een in-memory `orderDatabase` dient als datasource.
- **Security Misconfiguration**: een `/crash`-endpoint plus omgevingsafhankelijke foutafhandeling (Developer Exception Page enkel in development, generieke fout in productie).
- **Input validatie**: `/register` met volledige validatie, en extra validatie in `/login` en `/verify` (oefening 2).
- **CORS**: `AddCors` met een expliciete origin-lijst, geactiveerd na de authenticatie.
- **Rate limiting**: een fixed-window limiter op `/login` (5 pogingen per minuut, oefening 3).

In `ShopWave/Security/`:

- `CorsValidator` - simuleert de CORS-origincontrole (oefening 4).

Demo-code uit theorie en oplossingen (`ShopWave/Demos/`):

- `BruteForceDemo` - toont rate limiting door herhaalde loginpogingen (oefening 3).
- `SecureCodingFlowDemo` - test de volledige flow (zoekopdracht, SQL Injection-poging, input validatie, foutafhandeling).

Er zijn in deze les geen nieuwe unit tests of acceptatietests (oefening 5 is een analyseoefening). De testsuite blijft `ShopWave.Tests` (54) en `ShopWave.Specs` (11), beide groen.

## Cumulatieve wijzigingen

- `ShopWave.Api/Program.cs` is de samengevoegde eindversie: alle endpoints en middleware van les 6-8 plus de secure-coding-toevoegingen. De input-validatie is in de bestaande `HandleLogin`/`HandleVerify` verwerkt (oefening 2). De veilige zoekendpoints uit de theorie-demo (`/orders/zoek`) en de oefening (`/orders/zoek-product`) staan er allebei in.

### Broncorrectie

De rate-limiter-code uit de bron compileert niet zonder de using `Microsoft.AspNetCore.RateLimiting` (voor `AddFixedWindowLimiter`). Die using is toegevoegd. Dit is een kleine omissie in de cursusbron.

## NuGet-pakketten

- `ShopWave.Api`: ongewijzigd t.o.v. les 7 (`Microsoft.AspNetCore.Authentication.JwtBearer`, `System.IdentityModel.Tokens.Jwt`). CORS en rate limiting zitten in het ASP.NET Core framework.
- Overige projecten: ongewijzigd.

## Bouwen en testen

```bash
dotnet build
dotnet test
```

Beide horen groen te zijn: `ShopWave.Tests` (54) en `ShopWave.Specs` (11).

## Waarschuwing

Dit is een startpunt en referentie. Bekijk het pas nadat je de oefeningen zelf geprobeerd hebt.
