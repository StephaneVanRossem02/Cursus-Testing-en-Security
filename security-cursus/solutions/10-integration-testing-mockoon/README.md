# Les 10: Integration Testing (Mockoon)

Cumulatief oplossingsproject. Bevat alle code van les 1 t.e.m. les 9, plus de mock-server-integratietests van deze les.

## Wat is nieuw in deze les

In `ShopWave`:

- `ShippingResponse` - het gedeserialiseerde antwoord van de verzendservice.
- `ShippingClient` - roept een externe verzendservice aan via HTTP; de basis-URL komt via de constructor zodat de klasse testbaar is.

In `ShopWave.Tests`:

- `ShippingClientIntegrationTests` - integratietests tegen **Mockoon** (geldig tarief, tweede bestemming via `[Theory]`, foutscenario's, timeout). **Deze tests zijn gemarkeerd met `Skip`** (zie hieronder).
- `ShippingClientWireMockTests` - dezelfde tests met **WireMock.Net**, een in-process mock server. Deze draaien wel automatisch en zijn groen.

## Belangrijk: Mockoon-tests staan op Skip

De Mockoon-integratietests doen een echte HTTP-call naar een Mockoon-server op `http://localhost:3001`. Die server moet je handmatig starten (Mockoon is een aparte GUI-applicatie). In een geautomatiseerde `dotnet test` draait Mockoon niet, dus die tests zouden falen met een `HttpRequestException`. Om de testrun groen te houden staan ze op `Skip`, met een duidelijke reden in de code.

Wil je ze zelf draaien? Start Mockoon met de routes uit de les (poort 3001, route `GET /api/verzending` met de bijhorende Rules per bestemming) en verwijder de `Skip` in `ShippingClientIntegrationTests`.

De **WireMock.Net-tests** (oefening 4) tonen precies waarom WireMock beter geschikt is voor CI/CD: de mock server start en stopt in het testproces zelf, zonder handmatige stap. Zij draaien altijd mee.

## Testresultaat

- `ShopWave.Tests`: 56 geslaagd, 5 overgeslagen (de Mockoon-tests).
- `ShopWave.Specs`: 11 geslaagd.

## NuGet-pakketten

- `ShopWave.Tests`: extra `WireMock.Net` (naast de bestaande test-pakketten).
- Overige projecten: ongewijzigd t.o.v. les 9.

## Bouwen en testen

```bash
dotnet build
dotnet test
```

Groen: 56 geslaagd + 5 overgeslagen (`ShopWave.Tests`), 11 geslaagd (`ShopWave.Specs`).

## Waarschuwing

Dit is een startpunt en referentie. Bekijk het pas nadat je de oefeningen zelf geprobeerd hebt.
