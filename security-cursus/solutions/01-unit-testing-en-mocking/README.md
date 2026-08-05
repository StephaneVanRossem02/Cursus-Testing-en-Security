# Les 1: Unit Testing en Mocking

Uitgewerkt, compileerbaar ShopWave-oplossingsproject voor deze les. Dit is de eerste les, dus hier begint de cumulatieve rode draad.

## Wat is nieuw in deze les

Dit is het startpunt van ShopWave. De volgende klassen komen erbij:

- `DiscountCalculator` - kortingsberekening, puur logica, zonder afhankelijkheden.
- `IPaymentGateway` - interface voor betaling (Dependency Injection).
- `IStockService` - interface voor voorraadcontrole.
- `OrderService` - plaatst een bestelling, met voorraadcontrole en betaling via mocks.
- `IShippingService` en `CheckoutService` - berekent het eindtotaal met korting en verzendkost.

In het testproject:

- `DiscountCalculatorTests` - tests zonder mock (pure logica).
- `OrderServiceTests` - tests met Moq (`Setup`, `Verify`, `It.IsAny`, `Times`).
- `CheckoutServiceTests` - tests met een gemockte `IShippingService`.

### Opmerking over demo en oefening

De demo uit de theorie bouwt precies dezelfde klassen op (`DiscountCalculator`, `OrderService` met Dependency Injection, `IPaymentGateway`). Om een coherent, compileerbaar project te houden staat elke klasse er een keer in, in de meest uitgebreide versie zoals de oplossingen ze tonen. Concreet: `OrderService` staat hier in de versie met voorraadcontrole (`PlaceOrder(int productId, int quantity, double amount)`). De eenvoudiger demo-versie zonder voorraad (`PlaceOrder(double amount)`) is daarmee vervangen, want beide versies samen zouden niet compileren.

## NuGet-pakketten

Alleen in `ShopWave.Tests`:

- `Microsoft.NET.Test.Sdk`
- `xunit` en `xunit.runner.visualstudio`
- `Moq`
- `FluentAssertions`

## Bouwen en testen

```bash
dotnet build
dotnet test
```

Beide horen groen te zijn (16 tests slagen).

## Waarschuwing

Dit is een startpunt en referentie. Bekijk het pas nadat je de oefeningen zelf geprobeerd hebt. De waarde van de oefeningen zit in het zelf denken, niet in het kopieren.
