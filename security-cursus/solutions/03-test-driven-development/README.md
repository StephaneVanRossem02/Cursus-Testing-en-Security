# Les 3: Test Driven Development

Cumulatief oplossingsproject. Bevat alle code van les 1 en les 2 (ongewijzigd, behalve waar hieronder vermeld), plus de TDD-opbouw van deze les.

## Wat is nieuw in deze les

Demo-code uit de theorie (`ShopWave/Demos/`), stap voor stap via TDD opgebouwd:

- `Coupon` - een coupon met code, kortingspercentage en gebruikt-status.
- `CouponService` - concrete service die coupons valideert, de korting teruggeeft en als gebruikt markeert.

Bijhorende demo-tests: `ShopWave.Tests/Demos/CouponServiceTests.cs` (5 tests).

Oefening-oplossingen (`ShopWave/`):

- `CartItem` en `CartService` - winkelmandje, met couponondersteuning via `ICouponService`.
- `ICouponService` - interface voor de couponservice (voor mocking).
- Tests: `CartServiceTests` (7) en `CartServiceCouponTests` (4).
- `OrderServiceCouponTests` (5) - test de uitgebreide `OrderService`.

## Cumulatieve wijzigingen (belangrijk)

Deze les past bestaande code van les 1 aan. Dat is exact wat de bron voorschrijft; de aangepaste versies staan hier.

- **`OrderService`** kreeg een derde constructor-parameter (`ICouponService`) en `PlaceOrder` kreeg een optionele `couponCode`-parameter (oplossing 3). De bestaande `OrderServiceTests` uit les 1 zijn daarom aangepast: hun constructoraanroepen krijgen een extra mock. Er wordt geen coupon gebruikt, dus hun gedrag blijft identiek.
- **`CartService`** bestaat in de bron in twee stappen: eerst zonder coupon (oplossing 1, parameterloze constructor), daarna met verplichte `ICouponService` (oplossing 2). Het project bevat de uitgebreide versie. De basistests (`CartServiceTests`, oplossing 1) gebruikten de parameterloze constructor; hun constructoraanroepen zijn aangepast met een mock, zonder dat er een coupon toegepast wordt.

### Bron-opmerking

`OrderService.PlaceOrder` bevat in de bron een ongebruikte variabele `bool isUsed = false;` met een toelichtend commentaarblok. Dit is letterlijk overgenomen. De build geeft daardoor een waarschuwing (`CS0219`), geen fout. De tests slagen.

## NuGet-pakketten

Ongewijzigd t.o.v. les 2. `ShopWave`: `BCrypt.Net-Next`. `ShopWave.Tests`: `Microsoft.NET.Test.Sdk`, `xunit`, `xunit.runner.visualstudio`, `Moq`, `FluentAssertions`.

## Bouwen en testen

```bash
dotnet build
dotnet test
```

Beide horen groen te zijn (37 tests slagen, 1 waarschuwing uit de bron).

## Waarschuwing

Dit is een startpunt en referentie. Bekijk het pas nadat je de oefeningen zelf geprobeerd hebt.
