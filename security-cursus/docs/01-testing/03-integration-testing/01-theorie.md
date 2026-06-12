---
title: "Les 5: Theorie - Integration Testing"
sidebar_label: "Theorie"
---

# Theorie: Integration Testing

## 1. Waarom integration testing?

In les 1 leerden we unit tests schrijven. We testten elke klasse in isolatie. Via mocks vervingen we elke afhankelijkheid door een nep-versie die we volledig controleerden.

Dat is de kracht van unit tests: ze zijn snel, betrouwbaar en wijzen je precies aan welke klasse een probleem veroorzaakt. Maar ze hebben een fundamentele beperking.

**Een mock doet wat jij hem vertelt.** Je stelt in dat `IStockService.IsInStock` `true` teruggeeft, en de mock geeft `true` terug. Maar de echte `StockService` kan anders gedragen. Die kan een andere returnwaarde geven in een randgeval. Die kan een exception gooien die je mock nooit nabootst. Die kan een waarde teruggeven in een formaat dat `OrderService` niet verwacht.

Concrete problemen die unit tests missen:

- Klasse A geeft `null` terug in een specifiek scenario dat de mock nooit nabootst
- De volgorde van aanroepen tussen twee klassen is verkeerd
- Klasse A initialiseert iets in haar constructor dat klasse B nodig heeft, maar de mock doet dat niet
- Twee klassen spreken een impliciete afspraak af over een berichtformaat, maar die afspraak klopt niet

**Mini-controle:** je hebt unit tests voor `CartService` en voor `CouponService`. Beide slagen. Maar als je `CartService.ApplyCoupon` aanroept met een echte `CouponService`, crasht de applicatie. Hoe is dat mogelijk? De mock van `CouponService` geeft altijd terug wat jij instelt. Als de echte `CouponService` een andere waarde teruggeeft in een randgeval, detecteert de unit test dat nooit.

---

## 2. Wat is een integration test?

Een **integration test** test de samenwerking van twee of meer echte klassen, zonder mocks voor eigen code.

```
Unit test van OrderService:
  IPaymentGateway   mock (nep)
  IStockService     mock (nep)
  ICouponService    mock (nep)
  OrderService      echt

  Wat testen we? Alleen de logica in OrderService.
  Wat testen we NIET? Of OrderService en CouponService correct samenwerken.

Integration test van de bestelflow:
  OrderService      echt
  CouponService     echt
  DiscountCalculator echt

  Wat testen we? De volledige flow zoals die in productie ook werkt.
```

De klassen werken samen zoals in productie. Als er een integratieprobleem is, een verkeerde aanname, een volgordekwestie of een initialisatieprobleem, dan detecteert de integration test dat.

**Externe diensten** zoals databases of betaal-API's mag je nog steeds mocken. Het punt is dat je eigen code echt is.

**Mini-controle:** je schrijft een test voor de checkout-flow. Je gebruikt een echte `CartService`, een echte `CouponService` en een echte `DiscountCalculator`. De betaalgateway is een mock. Is dit een unit test of een integration test? Het is een integration test: meerdere eigen klassen werken echt samen. De externe betaalgateway mag een mock zijn.

---

## 3. De testpiramide

Een goede teststrategie combineert drie niveaus. Die worden gevisualiseerd als een piramide.

```
        /\
       /  \
      / E2E \         weinig, traag, fragiel
     /--------\
    /          \
   / Integration \    matig aantal, gemiddelde snelheid
  /--------------\
 /                \
/   Unit tests     \  veel, snel, goedkoop
/____________________\
```

| Niveau | Snelheid | Scope | Aantal |
|--------|----------|-------|--------|
| Unit | Zeer snel | Één klasse in isolatie | Veel |
| Integration | Trager | Meerdere klassen samen | Matig |
| E2E | Traag | Volledige applicatie via UI of API | Weinig |

**Unit tests** vormen de brede basis. Je hebt er veel, ze draaien snel en je schrijft ze als eerste.

**Integration tests** vormen de middelste laag. Je hebt er minder, maar ze testen wat unit tests niet kunnen: de samenwerking.

**End-to-end tests** (E2E) komen bovenaan. Ze testen de volledige applicatie zoals een gebruiker die ervaart, via de interface of via een API-aanroep. Ze zijn traag en fragiel: een kleine wijziging in de UI kan tientallen E2E tests breken.

**Mini-controle:** je hebt 200 unit tests, 20 integration tests en 5 E2E tests. Is dit een gezonde piramide? Ja. Veel unit tests, een matig aantal integration tests en weinig E2E tests. Dat is de aanbevolen verhouding.

---

## 4. Unit test vs. integration test

| Kenmerk | Unit test | Integration test |
|---------|-----------|-----------------|
| Eigen klassen | Gemockt (nep) | Echt |
| Externe diensten | Gemockt | Gemockt |
| Snelheid | Snel | Trager |
| Foutdetectie | Logica in één klasse | Samenwerking tussen klassen |
| Teststijl | White box | Black box |
| Fragiliteit bij refactoring | Hoger | Lager |

**White box** betekent dat je de interne structuur van de klasse kent. Je weet welke methode je test, welke vertakkingen er zijn en wat de interne stappen zijn.

**Black box** betekent dat je het gedrag van de combinatie test. Je geeft invoer en controleert uitvoer. Wat er intern precies gebeurt, doet er niet toe. Als je de interne implementatie wijzigt maar het gedrag hetzelfde blijft, slaagt de integration test nog steeds.

Dit maakt integration tests robuuster bij refactoring. Als je de interne structuur van `CouponService` volledig herschrijft maar het gedrag blijft hetzelfde, breken de integration tests niet.

---

## 5. Wanneer gebruik je welk type?

**Gebruik een unit test wanneer:**
- je een specifieke berekening of logica afzonderlijk wil verifiëren, zoals een kortingsformule
- je randgevallen wil testen die moeilijk te reproduceren zijn met echte afhankelijkheden
- je snel feedback wil tijdens het ontwikkelen

**Gebruik een integration test wanneer:**
- je wil verifiëren dat klassen correct samenwerken
- je een volledige use case wil testen: "gebruiker voegt artikelen toe, past coupon toe, plaatst bestelling"
- je wil controleren dat een refactoring niets heeft gebroken in de samenwerking tussen klassen

**Gebruik beide.** Een integration test vervangt geen unit tests. Ze zijn complementair. Unit tests testen de logica van elke klasse apart. Integration tests testen of die klassen correct samenwerken.

**Mini-controle:** je hebt een methode `CalculateFinalTotal` in `CheckoutService` die korting en verzendkosten combineert. Welk testtype gebruik je om de kortingsformule te verifiëren, en welk type om te verifiëren dat `CheckoutService` en `DiscountCalculator` correct samenwerken? Unit test voor de formule in `DiscountCalculator`. Integration test voor de samenwerking tussen `CheckoutService` en `DiscountCalculator`.

---

## 6. Demo: de bestelflow als integration test

We testen de volledige bestelflow van ShopWave met echte klassen. De flow die we testen:

1. Gebruiker vult een winkelmandje
2. Een coupon wordt toegepast
3. Een bestelling wordt geplaatst via `OrderService`

We gebruiken de klassen die je al kent uit les 1 en les 3: `CartService`, `CouponService`, `DiscountCalculator` en `OrderService`. De betaalgateway is de enige mock, want die is een externe dienst.

### Wat we niet doen

We mocken `CouponService`, `CartService` en `DiscountCalculator` niet. Dat is het verschil met de unit tests uit les 1.

### De integration test

Maak `CheckoutFlowIntegrationTests.cs` aan in `ShopWave.Tests`:

```csharp
using FluentAssertions;
using Moq;
using ShopWave;

namespace ShopWave.Tests
{
    public class CheckoutFlowIntegrationTests
    {
        [Fact]
        public void CheckoutFlow_WithValidCoupon_ProcessesCorrectAmount()
        {
            // Arrange
            // Eigen klassen zijn echt, externe dienst is een mock
            Mock<IPaymentGateway> mockGateway = new Mock<IPaymentGateway>();
            mockGateway.Setup(g => g.ProcessPayment(It.IsAny<double>())).Returns(true);

            Mock<IStockService> mockStock = new Mock<IStockService>();
            mockStock.Setup(s => s.IsInStock(It.IsAny<int>(), It.IsAny<int>())).Returns(true);

            CouponService  couponService  = new CouponService();
            OrderService   orderService   = new OrderService(
                mockGateway.Object,
                mockStock.Object,
                couponService);

            CartService cartService = new CartService(couponService);
            cartService.AddItem("Laptop", 100.0);
            cartService.ApplyCoupon("ZOMER10");

            // Act
            string result = orderService.PlaceOrder(1, 1, cartService.Total);

            // Assert
            result.Should().Be("Bestelling bevestigd");
            mockGateway.Verify(
                g => g.ProcessPayment(90.0),
                Times.Once,
                "de betaling moet het bedrag na korting bevatten");
        }

        [Fact]
        public void CheckoutFlow_WithInvalidCoupon_ProcessesFullAmount()
        {
            // Arrange
            Mock<IPaymentGateway> mockGateway = new Mock<IPaymentGateway>();
            mockGateway.Setup(g => g.ProcessPayment(It.IsAny<double>())).Returns(true);

            Mock<IStockService> mockStock = new Mock<IStockService>();
            mockStock.Setup(s => s.IsInStock(It.IsAny<int>(), It.IsAny<int>())).Returns(true);

            CouponService couponService = new CouponService();
            OrderService  orderService  = new OrderService(
                mockGateway.Object,
                mockStock.Object,
                couponService);

            CartService cartService = new CartService(couponService);
            cartService.AddItem("Laptop", 100.0);
            cartService.ApplyCoupon("ONGELDIG");

            // Act
            string result = orderService.PlaceOrder(1, 1, cartService.Total);

            // Assert
            result.Should().Be("Bestelling bevestigd");
            mockGateway.Verify(
                g => g.ProcessPayment(100.0),
                Times.Once,
                "een ongeldige coupon mag het bedrag niet verlagen");
        }
    }
}
```

**Wat maakt dit een integration test?**
- `CouponService` is echt: echte couponlogica, echte validatie
- `CartService` is echt: echte totaalberekening, echte coupontoepassing
- `OrderService` is echt: echte logica, echte samenwerking met de andere klassen
- Alleen de betaalgateway en stockservice zijn mocks, want die zijn externe diensten

**Wat testen we hier dat unit tests niet testen?**
- Of `CartService.Total` na `ApplyCoupon` het juiste bedrag geeft dat `OrderService` doorstuurt naar de gateway
- Of `CouponService.IsValid` en `GetDiscount` correct werken in combinatie met `CartService.ApplyCoupon`
- Of de volledige flow van mandje tot betaling het juiste bedrag berekent

---

## 7. De callback-techniek

Sommige klassen genereren waarden die je niet op voorhand kent: tijdstempels, willekeurige codes, gegenereerde sleutels. In een unit test mock je die klassen en stel je de returnwaarde zelf in. In een integration test gebruik je de echte klassen. Maar hoe vang je dan de gegenereerde waarde op?

De oplossing is een **callback**: een methode die je meegeeft aan de klasse en die opgeroepen wordt op het moment dat de waarde gegenereerd wordt. In productie laat je de callback weg. In tests geef je een lambda mee die de waarde opvangt.

Voorbeeld: stel dat `CouponService` in de toekomst couponcodes genereert voor klanten. De `GenerateCode`-methode roept dan de callback op zodat de test de code kan opvangen:

```csharp
// In de klasse (vereenvoudigd voorbeeld)
public class CouponGenerator
{
    private readonly Action<string> _onCodeGenerated;

    // Constructor voor productie
    public CouponGenerator()
    {
        _onCodeGenerated = null;
    }

    // Constructor voor integration testing
    public CouponGenerator(Action<string> onCodeGenerated)
    {
        _onCodeGenerated = onCodeGenerated;
    }

    public string GenerateCode()
    {
        string code = Guid.NewGuid().ToString("N")[..8].ToUpper();

        if (_onCodeGenerated != null)
        {
            _onCodeGenerated(code);
        }

        return code;
    }
}
```

```csharp
// In de integration test
[Fact]
public void GenerateCode_ProducesValidCode()
{
    string capturedCode = string.Empty;

    CouponGenerator generator = new CouponGenerator(
        onCodeGenerated: code => { capturedCode = code; });

    string result = generator.GenerateCode();

    capturedCode.Should().NotBeNullOrEmpty();
    capturedCode.Should().Be(result);
}
```

**Waarom niet gewoon de returnwaarde gebruiken?** In complexere flows wordt de waarde diep in een keten van aanroepen gegenereerd. De callback laat je toe de waarde op te vangen op het moment dat hij gegenereerd wordt, zonder de klasse te mocken of een publieke getter toe te voegen.

**Mini-controle:** een klasse genereert een willekeurig token in haar constructor. Je wil die waarde opvangen in een integration test zonder de klasse te mocken. Welke twee opties heb je? Optie 1: voeg een publieke property toe aan de klasse (maar dat wijzigt de publieke interface). Optie 2: gebruik een callback die je meegeeft via de constructor (de klasse blijft ongewijzigd in productie).

---

## 8. Integration testing en security

Integration tests zijn bijzonder waardevol voor beveiligingsgevoelige flows. Een unit test kan bewijzen dat een hashfunctie correct werkt. Maar kan een unit test bewijzen dat het gehashte wachtwoord correct opgeslagen en daarna correct vergeleken wordt in de volledige loginflow? Nee. Daarvoor heb je een integration test nodig.

Typische beveiligingsflows die je met integration tests verifieert:

- De volledige loginflow: wachtwoord invoeren, hash vergelijken, 2FA genereren, 2FA verifiëren
- Token genereren, opslaan en valideren
- Handtekening zetten op data, data wijzigen, handtekening verifiëren (moet falen)

In les 5 van het security-traject zullen we dit in de praktijk brengen met de `AccountRepository` en `TwoFactorService`.

---

## 9. Samenvatting

| Concept | Wat je moet onthouden |
|--------|-----------------------|
| Integration test | Meerdere echte eigen klassen samen, geen mocks voor eigen code |
| Testpiramide | Veel unit tests, matig aantal integration tests, weinig E2E |
| White box | Je test de interne logica van één klasse, je kent de vertakkingen |
| Black box | Je test gedrag via input en output, interne implementatie doet er niet toe |
| Callback-techniek | Een methode meegeven aan een klasse om gegenereerde waarden op te vangen in tests |
| Wanneer unit? | Logica van één klasse, randgevallen, snelle feedback |
| Wanneer integration? | Samenwerking tussen klassen, volledige use case, robuust bij refactoring |
