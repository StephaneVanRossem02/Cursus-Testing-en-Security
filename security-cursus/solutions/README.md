# ShopWave oplossingsprojecten per les

Uitgewerkte, compileerbare ShopWave-oplossingen die de cursus **Testing en Security** cumulatief opbouwen. Elke map is een op zichzelf staand, bouwbaar project: les N bevat de code van les N-1 plus wat les N toevoegt.

Deze projecten staan los van de Docusaurus-cursus (`docs/`) en beinvloeden de site niet.

## Lesvolgorde (afwisselend Testing / Security)

| # | Map | Type |
|---|-----|------|
| 01 | `01-unit-testing-en-mocking` | Testing |
| 02 | `02-cia-hashing-en-encryptie` | Security |
| 03 | `03-test-driven-development` | Testing |
| 04 | `04-2fa-handtekeningen-en-x509` | Security |
| 05 | `05-integration-testing` | Testing |
| 06 | `06-https-en-tls` | Security |
| 07 | `07-jwt-en-oauth2` | Security |
| 08 | `08-acceptatietesten` | Testing |
| 09 | `09-secure-coding-owasp` | Security |
| 10 | `10-integration-testing-mockoon` | Testing |
| 11 | `11-ethisch-hacken` | Security |
| 12 | `12-shopwave-in-productie` | Security |

## Projectindeling per les

- `ShopWave` - Console App (.NET 8) met de domeincode en de demo's (`Demos/`).
- `ShopWave.Tests` - xUnit unit- en integratietests (demo-tests in `Demos/`).
- `ShopWave.Api` - vanaf les 6: de ASP.NET Core API (HTTPS, JWT, ...).
- `ShopWave.Specs` - vanaf les 8: Reqnroll-acceptatietests (Gherkin).
- `ShopWave.sln` en `README.md` per les.

## Conventie demo versus oefening

- Code uit de **theorie** (demo van de docent) staat in `Demos/`-mappen wanneer het niet door een oefening wordt uitgebreid.
- Code uit de **oplossingen** (uitgewerkte oefeningen) staat in de gewone mappen.
- Waar demo en oefening dezelfde klasse definieren, staat er een coherente eindversie; elke afwijking of samenvoeging is per les in de README beschreven.

## Bouwen en testen

Per lesmap:

```bash
dotnet build
dotnet test
```

Elke les hoort groen te zijn. De README per les vermeldt het exacte aantal tests en eventuele overgeslagen (Skip) tests.

## Belangrijke aandachtspunten

- **Toolchain:** de projecten targeten `net8.0`. De cursus gebruikt .NET 8.
- **Secrets:** `ShopWave.Api` leest vanaf les 7 de JWT-sleutel uit de omgevingsvariabele `JWT_SECRET_KEY`. Dat is enkel nodig om de API te *draaien*, niet om te bouwen of te testen.
- **Mockoon (les 10):** de Mockoon-integratietests staan op `Skip` omdat ze een handmatig gestarte Mockoon-server vereisen. De WireMock.Net-tests draaien wel automatisch.
- **Solutionformaat:** elke les gebruikt een klassiek `ShopWave.sln` (niet het nieuwe `.slnx`), zodat Visual Studio 2022 de solution zonder problemen opent.
- **Bron-inconsistenties:** waar de cursusbron intern tegenstrijdig was (bv. de acceptatietests van les 8 versus de security-klassen van les 4), is dat in de betreffende README beschreven en in overleg opgelost.

## Waarschuwing voor studenten

Elk project is een startpunt en referentie. Bekijk het pas nadat je de oefeningen zelf geprobeerd hebt.
