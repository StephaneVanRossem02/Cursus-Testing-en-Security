# Les 5: Integration Testing

Cumulatief oplossingsproject. Bevat alle code van les 1 t.e.m. les 4, plus de integration tests van deze les.

## Wat is nieuw in deze les

Oefening-oplossingen:

- `CheckoutService` - herzien: rekent een volledig mandje af via `CartService` en `IPaymentGateway` (oefening 1).
- `CheckoutServiceIntegrationTests` (4) - `CartService` en `CouponService` echt, alleen de gateway gemockt.
- `DiscountIntegrationTests` (4) - `CartService`, `CouponService` en `DiscountCalculator` werken echt samen.
- `FullCheckoutFlowIntegrationTests` (4) - de volledige bestelflow met echte eigen klassen.
- `OrderConfirmationService` en `OrderConfirmationServiceIntegrationTests` (4) - de callback-techniek (oefening 4).

Demo-code uit de theorie (`ShopWave/Demos/`, `ShopWave.Tests/Demos/`):

- `CheckoutFlowIntegrationTests` (2) - de bestelflow als integration test.
- `CouponGenerator` en `CouponGeneratorTests` (1) - vereenvoudigd voorbeeld van de callback-techniek.

Totaal: 54 tests.

## Cumulatieve wijzigingen (belangrijk)

Deze les past bestaande code aan. De aangepaste versies staan hier, zoals de bron ze toont.

- **`CheckoutService` is volledig herzien.** De versie uit les 1 (`IShippingService`, `CalculateFinalTotal`) is vervangen door de nieuwe versie (`CartService` + `IPaymentGateway`, `Checkout`). De oude `CheckoutServiceTests` van les 1 testten `CalculateFinalTotal`, een methode die niet meer bestaat, en zijn daarom verwijderd. `IShippingService` blijft bestaan maar wordt niet meer gebruikt.
- **`DiscountCalculator`** kreeg in les 5 (oplossing 2) een nieuwe methode `Apply` (met `ArgumentOutOfRangeException`). In de bron vervangt die de oude `ApplyDiscount`, maar om de tests van les 1 groen te houden staan hier beide methoden naast elkaar (`ApplyDiscount` uit les 1, `Apply` uit les 5).
- **`CartService`** kreeg een tweede constructor-parameter (`DiscountCalculator`, oplossing 2). De bestaande cart-tests en de demo-test zijn daarom aangepast met de nieuwe constructor. Er wordt geen coupon toegepast in de basistests, dus de totalen blijven identiek.

### Opmerking over CouponService en ICouponService

`CouponService` implementeert expliciet `ICouponService`. Dat is nodig omdat de integratiecode van les 5 een echte `CouponService` doorgeeft waar een `ICouponService` verwacht wordt.

Dit was oorspronkelijk een inconsistentie in de cursusbron (de klasse had de drie methoden wel, maar verklaarde de interface nergens). Die is intussen in les 3 van de cursus zelf rechtgezet.

## NuGet-pakketten

Ongewijzigd t.o.v. les 2. `ShopWave`: `BCrypt.Net-Next`. `ShopWave.Tests`: `Microsoft.NET.Test.Sdk`, `xunit`, `xunit.runner.visualstudio`, `Moq`, `FluentAssertions`.

## Bouwen en testen

```bash
dotnet build
dotnet test
```

Beide horen groen te zijn (54 tests slagen, 1 waarschuwing uit de bron in `OrderService`).

## Waarschuwing

Dit is een startpunt en referentie. Bekijk het pas nadat je de oefeningen zelf geprobeerd hebt.
