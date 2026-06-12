---
title: "Les 1: Unit Testing & Mocking — Theorie"
sidebar_label: "Theorie"
---

# Les 1: Unit Testing & Mocking — Theorie

## Wat is een unit test?

Een unit test test één methode of klasse volledig geïsoleerd van de rest van de applicatie — geen database, geen netwerk, geen bestanden op schijf. Goede unit tests zijn geautomatiseerd, snel (milliseconden), geïsoleerd, eenduidig bij falen en actueel. Ze dienen ook als levende documentatie.

---

## Arrange-Act-Assert (AAA)

Elke test volgt hetzelfde drieledige patroon:

- **Arrange** — maak objecten aan en stel invoerwaarden in
- **Act** — roep de methode aan die je wil testen
- **Assert** — controleer of het resultaat overeenkomt met de verwachting

```csharp
[Fact]
public void CalculateTotal_WithOneProduct_ReturnsCorrectTotal()
{
    // Arrange
    PriceCalculator calculator = new PriceCalculator();
    double price    = 9.99;
    int    quantity = 1;
    double expected = 9.99;

    // Act
    double result = calculator.CalculateTotal(price, quantity);

    // Assert
    result.Should().Be(expected);
}
```

---

## ZOMBIES

Het acroniem van James W. Grenning helpt bepalen welke testgevallen je schrijft:

| Letter    | Betekenis              | Voorbeeld (winkelmandje)          |
|-----------|------------------------|-----------------------------------|
| Zero      | Nulwaarden / lege toestand | Leeg mandje — totaal is 0     |
| One       | Precies één element    | Mandje met 1 product              |
| Many      | Meerdere elementen     | Mandje met 3 producten            |
| Boundary  | Grensgevallen          | Exact 50 euro — gratis verzending |
| Interface | Publieke interface     | Retourneert de methode het juiste type? |
| Exception | Foutgevallen           | Negatieve prijs gooit een exception |
| Simple    | Eenvoud                | Elke test test precies één scenario |

Begin altijd bij Zero en One — die zijn het eenvoudigst te implementeren.

---

## xUnit

- **`[Fact]`** — markeert een methode als test met één vast scenario
- **`[Theory]`** + **`[InlineData]`** — meerdere scenario's in één testmethode, vermijdt bijna-identieke testmethoden

Naamgevingsconventie: `MethodNaam_ToestandOnderTest_VerwachtGedrag`

Voorbeelden:
- `CalculateTotal_WithZeroQuantity_ReturnsZero`
- `ApplyDiscount_WithNegativePercent_ThrowsArgumentException`
- `PlaceOrder_WhenPaymentFails_ReturnsMislukt`

---

## FluentAssertions

Vervangt standaard asserts door leesbaardere alternatieven:

| Situatie             | FluentAssertions                              |
|----------------------|-----------------------------------------------|
| Waarde controleren   | `result.Should().Be(5.0)`                     |
| Null controleren     | `result.Should().BeNull()`                    |
| Exception verwachten | `act.Should().Throw<ArgumentException>()`     |
| Collectie controleren | `list.Should().Contain(item)`                |
| Niet leeg            | `result.Should().NotBeNullOrEmpty()`          |

---

## Dependency Injection en mocking

Een klasse die haar afhankelijkheden zelf aanmaakt, is moeilijk te testen. Via **Dependency Injection (DI)** worden afhankelijkheden via de constructor meegegeven. In tests geef je een **mock** mee — een nep-versie die je volledig controleert.

```csharp
// Slecht — vaste afhankelijkheid, niet testbaar
public class OrderService
{
    public string PlaceOrder(double amount)
    {
        PaymentGateway gateway = new PaymentGateway(); // altijd de echte gateway
        bool success = gateway.ProcessPayment(amount);
        // ...
    }
}

// Goed — afhankelijkheid via constructor meegegeven
public class OrderService
{
    private readonly IPaymentGateway gateway;

    public OrderService(IPaymentGateway gateway)
    {
        this.gateway = gateway;
    }
}
```

---

## Moq

| Actie                    | Code                                                                 |
|--------------------------|----------------------------------------------------------------------|
| Mock aanmaken            | `Mock<IPaymentGateway> mock = new Mock<IPaymentGateway>()`          |
| Gedrag instellen         | `mock.Setup(x => x.ProcessPayment(50.0)).Returns(true)`             |
| Elke waarde accepteren   | `mock.Setup(x => x.ProcessPayment(It.IsAny<double>())).Returns(true)` |
| Nep-instantie doorgeven  | `new OrderService(mock.Object)`                                      |
| Aanroep verifiëren       | `mock.Verify(x => x.ProcessPayment(50.0), Times.Once)`              |
| Nooit aangeroepen        | `mock.Verify(x => x.ProcessPayment(...), Times.Never)`              |