---
title: "Les 1: Unit Testing & Mocking"
sidebar_label: "Unit Testing & Mocking"
---

# Les 1: Unit Testing & Mocking

## ShopWave

Doorheen de hele cursus werken we aan **ShopWave**, een fictieve webshop waarop klanten producten kunnen bekijken, bestellen en betalen. Elke les voegen we een nieuw stukje toe of testen we een bestaand stukje grondiger. Zo bouw je stap voor stap een volledige test-suite op voor een realistische applicatie.

---

## Theorie

### 1. Wat is een unit test?

Een **unit test** is een stuk code dat één methode of klasse test, volledig **geïsoleerd** van de rest van de applicatie. Met "geïsoleerd" bedoelen we: geen database, geen netwerkverbinding, geen bestanden op schijf — uitsluitend de logica van de methode zelf wordt gecontroleerd.

Dit isolement is belangrijk om twee redenen:

**Snelheid.** Een unit test die geen externe systemen aanspreekt, wordt uitgevoerd in milliseconden. Als een project honderden unit tests heeft, moeten ze allemaal samen in enkele seconden klaar zijn. Zodra tests traag worden, worden ze niet meer regelmatig uitgevoerd.

**Betrouwbaarheid.** Als een unit test faalt, moet de oorzaak liggen in de code die je test — niet in een tijdelijk onbeschikbare database of een wisselvallende netwerkverbinding. Een test die soms slaagt en soms faalt zonder dat de code veranderd is, is onbruikbaar.

Een goede unit test voldoet aan de volgende eigenschappen:

- **Geautomatiseerd en herhaalbaar** — de test kan op elk moment opnieuw uitgevoerd worden en geeft steeds hetzelfde resultaat
- **Snel** — uitvoertijd in milliseconden
- **Geïsoleerd** — de test is onafhankelijk van andere tests en van externe systemen
- **Eenduidig bij falen** — als de test faalt, is het onmiddellijk duidelijk welk stukje code het probleem veroorzaakt
- **Actueel** — de test blijft relevant zolang de bijhorende code bestaat

Een unit test die aan al deze eigenschappen voldoet, doet ook dienst als **levende documentatie**: een nieuwe collega kan de tests lezen om te begrijpen wat een klasse precies doet en welke randgevallen er bestaan.

---

### 2. Het Arrange-Act-Assert patroon (AAA)

Elke unit test volgt hetzelfde drieledige patroon. Dit patroon heet **Arrange-Act-Assert** of kortweg **AAA**.

```
Arrange   —   Zet alles klaar: maak objecten aan, definieer de invoerwaarden
Act       —   Roep de methode aan die je wil testen
Assert    —   Controleer of het resultaat overeenkomt met wat je verwacht
```

Dit patroon geeft structuur aan elke test. Door het consequent toe te passen, worden tests leesbaar voor iedereen — ook voor mensen die de code niet zelf schreven.

**Voorbeeld:**

```csharp
[Fact]
public void CalculateTotal_WithOneProduct_ReturnsCorrectTotal()
{
    // Arrange
    PriceCalculator calculator = new PriceCalculator();
    double price = 9.99;
    int quantity = 1;
    double expected = 9.99;

    // Act
    double result = calculator.CalculateTotal(price, quantity);

    // Assert
    result.Should().Be(expected);
}
```

Je ziet meteen: wat wordt er klaargezet, wat wordt er gedaan, wat wordt er gecontroleerd.

---

### 3. ZOMBIES — welke tests schrijf je?

Eén van de moeilijkste vragen bij het schrijven van tests is: **welke gevallen test ik?** Het acroniem **ZOMBIES** van James W. Grenning biedt hiervoor een houvast.

| Letter | Betekenis | Voorbeeld voor een winkelmandje |
|--------|-----------|----------------------------------|
| **Z**ero | Nulwaarden of lege toestand | Leeg mandje — totaal is 0 |
| **O**ne | Precies één element | Mandje met 1 product |
| **M**any | Meerdere elementen | Mandje met 3 producten |
| **B**oundary | Grensgevallen | Exact 50 euro — gratis verzending begint hier |
| **I**nterface | De publieke interface van de klasse | Retourneert de methode het juiste type? |
| **E**xception | Foutgevallen | Negatieve prijs — gooit de methode een exception? |
| **S**imple | Eenvoud | Elke test test precies één scenario |

ZOMBIES helpt je ook de **volgorde** bepalen: begin bij Zero en One, want die zijn het eenvoudigst te implementeren. De complexere gevallen (Many, Boundary) komen daarna vanzelf.

---

### 4. xUnit — het testframework

In deze cursus gebruiken we **xUnit** als testframework. xUnit is de huidige standaard in de .NET-wereld en wordt actief onderhouden door de gemeenschap.

Twee annotaties zijn fundamenteel:

**`[Fact]`** — markeert een methode als een test met één vast testgeval.

```csharp
[Fact]
public void MethodName_StateUnderTest_ExpectedBehavior()
{
    // één concreet scenario
}
```

**`[Theory]`** met **`[InlineData]`** — markeert een methode als een test die meerdere keren uitgevoerd wordt, telkens met andere invoerwaarden. Dit voorkomt dat je tien bijna-identieke testmethoden moet schrijven.

```csharp
[Theory]
[InlineData(10.00, 1,  10.00)]
[InlineData( 5.00, 3,  15.00)]
[InlineData( 2.50, 4,  10.00)]
public void CalculateTotal_WithValidInputs_ReturnsCorrectTotal(
    double price, int quantity, double expected)
{
    PriceCalculator calculator = new PriceCalculator();
    double result = calculator.CalculateTotal(price, quantity);
    result.Should().Be(expected);
}
```

xUnit voert deze test drie keer uit — eén keer per rij. In de Test Explorer zie je elk geval afzonderlijk staan.

**Naamgevingsconventie voor testmethoden:**

Een goede testmethode krijgt een naam in drie delen: `MethodName_StateUnderTest_ExpectedBehavior`.

Voorbeelden:
- `CalculateTotal_WithZeroQuantity_ReturnsZero`
- `ApplyDiscount_WithNegativePercent_ThrowsArgumentException`
- `PlaceOrder_WhenPaymentFails_ReturnsBetalingMislukt`

Deze naam leest als een zin: "Als ik `CalculateTotal` aanroep met een hoeveelheid nul, verwacht ik dat het resultaat nul is."

---

### 5. FluentAssertions

**FluentAssertions** is een bibliotheek die de standaard `Assert`-methoden van xUnit vervangt door leesbaardere alternatieven.

| Situatie | xUnit standaard | FluentAssertions |
|----------|----------------|------------------|
| Waarde controleren | `Assert.Equal(5.0, result)` | `result.Should().Be(5.0)` |
| Null controleren | `Assert.Null(result)` | `result.Should().BeNull()` |
| Exception verwachten | `Assert.Throws<ArgumentException>(...)` | `act.Should().Throw<ArgumentException>()` |
| Collectie controleren | `Assert.Contains(item, list)` | `list.Should().Contain(item)` |

Het verschil is subtiel maar merkbaar: FluentAssertions leest als een zin in gewone taal. Bij een falende test geeft het ook duidelijkere foutmeldingen.

---

### 6. Dependency Injection en het probleem van externe afhankelijkheden

Niet alle klassen zijn even eenvoudig te testen. Beschouw deze klasse:

```csharp
public class OrderService
{
    public string PlaceOrder(double amount)
    {
        PaymentGateway gateway = new PaymentGateway(); // vaste afhankelijkheid
        bool success = gateway.ProcessPayment(amount);
        return success ? "Bestelling bevestigd" : "Betaling mislukt";
    }
}
```

`OrderService` maakt zelf een `PaymentGateway` aan. Die gateway communiceert met een extern betaalsysteem. Als je `PlaceOrder` wil testen, heb je altijd een echte verbinding met dat externe systeem nodig. Dat is een probleem:

- De test is traag (netwerkcommunicatie)
- De test kan falen door een storing bij de betaalprovider — niet door een fout in jouw code
- Je kan niet controleren wat de gateway teruggeeft (slagen of falen)

De oplossing heet **Dependency Injection (DI)**. In plaats van de afhankelijkheid zelf aan te maken, ontvang je ze van buitenaf:

```csharp
public class OrderService
{
    private readonly IPaymentGateway _gateway;

    public OrderService(IPaymentGateway gateway) // ontvangen via de constructor
    {
        _gateway = gateway;
    }

    public string PlaceOrder(double amount)
    {
        bool success = _gateway.ProcessPayment(amount);
        return success ? "Bestelling bevestigd" : "Betaling mislukt";
    }
}
```

In productie geef je een echte `PaymentGateway` mee. In tests geef je een **nep-versie** mee die je volledig onder controle hebt. Die nep-versie noemen we een **mock**.

---

### 7. Mocking met Moq

**Moq** is een framework dat automatisch een nep-implementatie genereert voor een interface. Je hoeft geen aparte klasse te schrijven — Moq doet dat voor jou.

**Een mock aanmaken:**

```csharp
Mock<IPaymentGateway> mockGateway = new Mock<IPaymentGateway>();
```

**Gedrag instellen met `Setup`:**

```csharp
mockGateway.Setup(x => x.ProcessPayment(50.0)).Returns(true);
```

Dit zegt: "Als `ProcessPayment` aangeroepen wordt met het bedrag 50, geef dan `true` terug." Alles wat je niet instelt, geeft de standaardwaarde terug (`false` voor `bool`, `null` voor objecten, `0` voor getallen).

Je kan ook `It.IsAny<T>()` gebruiken als de exacte waarde niet uitmaakt:

```csharp
mockGateway.Setup(x => x.ProcessPayment(It.IsAny<double>())).Returns(true);
```

**De mock doorgeven:**

```csharp
OrderService service = new OrderService(mockGateway.Object);
```

`.Object` geeft de daadwerkelijke nep-instantie terug die je kan meegeven aan de constructor.

**Verifiëren met `Verify`:**

Naast het controleren van het resultaat kan je ook verifiëren **of** een methode aangeroepen werd en **hoe vaak**:

```csharp
mockGateway.Verify(x => x.ProcessPayment(50.0), Times.Once);
```

Andere opties: `Times.Never`, `Times.Exactly(3)`, `Times.AtLeastOnce`.

Dit is nuttig om te bewijzen dat jouw klasse de juiste methoden aanroept — los van het resultaat.

---

## Demo

We bouwen `OrderService` voor ShopWave stap voor stap, schrijven eerst de tests en daarna de implementatie.

### Stap 1 — Projectstructuur aanmaken

Maak in Visual Studio een nieuwe **Console App** aan met de naam `ShopWave`.

Voeg een nieuw project toe aan de solution: **xUnit Test Project**, naam `ShopWave.Tests`.

> Via de .NET CLI kan je dit ook doen:
> ```
> dotnet new xunit -n ShopWave.Tests
> dotnet sln add ShopWave.Tests
> dotnet add ShopWave.Tests/ShopWave.Tests.csproj reference ShopWave/ShopWave.csproj
> ```

Voeg in `ShopWave.Tests` een projectreferentie toe:
- Rechtermuisklik op **Dependencies** in `ShopWave.Tests`
- **Add Project Reference**
- Vink `ShopWave` aan

Installeer via **NuGet** in het project `ShopWave.Tests`:
- `Moq`
- `FluentAssertions`

---

### Stap 2 — Interface en klassen aanmaken

Maak in `ShopWave` de volgende bestanden aan:

**IPaymentGateway.cs**
```csharp
namespace ShopWave
{
    public interface IPaymentGateway
    {
        bool ProcessPayment(double amount);
    }
}
```

**PaymentGateway.cs** — de echte implementatie, voor gebruik in productie
```csharp
namespace ShopWave
{
    public class PaymentGateway : IPaymentGateway
    {
        public bool ProcessPayment(double amount)
        {
            // Verbinding met extern betaalsysteem
            return true;
        }
    }
}
```

**OrderService.cs** — de klasse die we gaan testen
```csharp
namespace ShopWave
{
    public class OrderService
    {
        private readonly IPaymentGateway _gateway;

        public OrderService(IPaymentGateway gateway)
        {
            _gateway = gateway;
        }

        public string PlaceOrder(double amount)
        {
            string result;

            if (amount <= 0)
            {
                throw new ArgumentException("Bedrag moet groter zijn dan nul.", nameof(amount));
            }

            bool success = _gateway.ProcessPayment(amount);

            if (success)
            {
                result = "Bestelling bevestigd";
            }
            else
            {
                result = "Betaling mislukt";
            }

            return result;
        }
    }
}
```

---

### Stap 3 — Eerste tests schrijven

Hernoem `UnitTest1.cs` in `ShopWave.Tests` naar `OrderServiceTests.cs` en vervang de inhoud:

```csharp
using FluentAssertions;
using Moq;
using ShopWave;

namespace ShopWave.Tests
{
    public class OrderServiceTests
    {
        // ZOMBIES: Exception — negatief bedrag
        [Fact]
        public void PlaceOrder_WithNegativeAmount_ThrowsArgumentException()
        {
            // Arrange
            Mock<IPaymentGateway> mockGateway = new Mock<IPaymentGateway>();
            OrderService service = new OrderService(mockGateway.Object);

            // Act
            Action act = () => service.PlaceOrder(-10.0);

            // Assert
            act.Should().Throw<ArgumentException>();
        }

        // ZOMBIES: Zero — bedrag is nul
        [Fact]
        public void PlaceOrder_WithZeroAmount_ThrowsArgumentException()
        {
            // Arrange
            Mock<IPaymentGateway> mockGateway = new Mock<IPaymentGateway>();
            OrderService service = new OrderService(mockGateway.Object);

            // Act
            Action act = () => service.PlaceOrder(0.0);

            // Assert
            act.Should().Throw<ArgumentException>();
        }
    }
}
```

Open **Test Explorer** (menu: Test > Test Explorer) en klik op **Run All**. Beide tests slagen.

Let op: de mock doet hier nog niets. We hebben hem enkel nodig om `OrderService` te kunnen aanmaken, want de constructor vereist een `IPaymentGateway`.

---

### Stap 4 — Setup gebruiken

Nu testen we het eigenlijke gedrag — wat doet `PlaceOrder` als de betaling slaagt of faalt?

```csharp
// ZOMBIES: One — betaling slaagt
[Fact]
public void PlaceOrder_WhenPaymentSucceeds_ReturnsBevestigd()
{
    // Arrange
    Mock<IPaymentGateway> mockGateway = new Mock<IPaymentGateway>();
    mockGateway.Setup(x => x.ProcessPayment(50.0)).Returns(true);
    OrderService service = new OrderService(mockGateway.Object);

    // Act
    string result = service.PlaceOrder(50.0);

    // Assert
    result.Should().Be("Bestelling bevestigd");
}

// ZOMBIES: One — betaling mislukt
[Fact]
public void PlaceOrder_WhenPaymentFails_ReturnsMislukt()
{
    // Arrange
    Mock<IPaymentGateway> mockGateway = new Mock<IPaymentGateway>();
    mockGateway.Setup(x => x.ProcessPayment(50.0)).Returns(false);
    OrderService service = new OrderService(mockGateway.Object);

    // Act
    string result = service.PlaceOrder(50.0);

    // Assert
    result.Should().Be("Betaling mislukt");
}
```

Voer uit — beide tests slagen. Het verschil zit volledig in de `Setup`: de mock bepaalt wat de gateway teruggeeft en wij controleren hoe `OrderService` daarop reageert.

---

### Stap 5 — Verify gebruiken

Tot slot verifiëren we dat `OrderService` de gateway ook daadwerkelijk aanroept:

```csharp
[Fact]
public void PlaceOrder_WithValidAmount_CallsProcessPaymentExactlyOnce()
{
    // Arrange
    Mock<IPaymentGateway> mockGateway = new Mock<IPaymentGateway>();
    mockGateway.Setup(x => x.ProcessPayment(It.IsAny<double>())).Returns(true);
    OrderService service = new OrderService(mockGateway.Object);

    // Act
    service.PlaceOrder(75.0);

    // Assert
    mockGateway.Verify(x => x.ProcessPayment(75.0), Times.Once);
}
```

`It.IsAny<double>()` in de `Setup` zorgt ervoor dat de mock voor gelijk welk decimaal getal `true` teruggeeft. De `Verify` controleert achteraf dat de methode precies eenmaal aangeroepen werd met het bedrag 75.

---

## Oefeningen

Werk de oefeningen in volgorde. Elke oefening bouwt voort op de vorige.

Schrijf telkens **eerst de tests**, implementeer daarna de klasse. Voer na elke stap de tests uit.

---

### Oefening 1 — DiscountCalculator

ShopWave biedt procentuele kortingen aan op bestellingen.

Schrijf een klasse `DiscountCalculator` met de methode:

```csharp
    public double ApplyDiscount(double originalPrice, int discountPercent)
{
    if (discountPercent < 0 || discountPercent > 100)
        throw new ArgumentException(
            "Kortingspercentage moet tussen 0 en 100 liggen.",
            nameof(discountPercent));

    return originalPrice * (1 - discountPercent / 100.0);
}
```



Schrijf minstens zes tests via ZOMBIES (Zero, One, Boundary, Exception). Implementeer daarna de klasse zodat alle tests slagen.

---

### Oefening 2 — [Theory] toepassen

Herschrijf de geldige testgevallen van Oefening 1 als één `[Theory]` met `[InlineData]`.

Gebruik minstens vijf combinaties van `originalPrice` en `discountPercent` die elk een ander scenario dekken.

---

### Oefening 3 — StockService met Moq

ShopWave controleert de voorraad voordat een bestelling verwerkt wordt.

> ⚠️ **Let op:** het toevoegen van een tweede constructor-parameter aan `OrderService` zorgt ervoor dat de bestaande tests voor de eerste twee oefeningen **niet meer compileren** — die maken `OrderService` aan met maar één argument. Je zal die tests moeten aanpassen om ook een gemockte `IStockService` mee te geven.

Maak de volgende interface aan:

```csharp
public interface IStockService
{
    bool IsInStock(int productId, int quantity);
}
```

Voeg aan `OrderService` een tweede constructor-parameter toe van het type `IStockService`.

Pas `PlaceOrder` aan zodat:
- Als het product niet op voorraad is, de methode de tekst `"Product niet beschikbaar"` teruggeeft zonder de gateway aan te roepen
- Als het product op voorraad is, de bestaande logica verder loopt

Schrijf tests voor alle gevallen. Gebruik Moq voor beide interfaces.

---

### Oefening 4 — Verify correct toepassen

Voeg aan de tests van Oefening 3 de volgende drie `Verify`-controles toe:

1. Als het product **niet** op voorraad is, wordt `ProcessPayment` **nooit** aangeroepen
2. Als het product **wel** op voorraad is en de betaling verwerkt wordt, wordt `ProcessPayment` **precies eenmaal** aangeroepen
3. Als `PlaceOrder` een `ArgumentException` gooit door een ongeldig bedrag, wordt `IsInStock` **nooit** aangeroepen

---

### Oefening 5 — CheckoutService (uitbreiding)

Maak een nieuwe klasse `CheckoutService` met de volgende methode:

```csharp
public double CalculateFinalTotal(double unitPrice, int quantity, int discountPercent)
```

Die methode:
1. Berekent het subtotaal (`unitPrice` vermenigvuldigd met `quantity`)
2. Past de korting toe via `DiscountCalculator`
3. Vraagt de verzendkosten op via onderstaande interface
4. Retourneert het bedrag na korting verhoogd met de verzendkosten

```csharp
public interface IShippingService
{
    double GetShippingCost(double orderTotal);
}
```

Schrijf minstens zes tests voor `CalculateFinalTotal`. Mock `IShippingService` volledig met Moq. Gebruik `Verify` om te controleren dat `GetShippingCost` precies eenmaal aangeroepen wordt per aanroep van `CalculateFinalTotal`.

---

## Modeloplossing

> De modeloplossing is beschikbaar na het indienen van de labo-opdracht via Digitap.

---

### Modeloplossing Oefening 1 — DiscountCalculator

**DiscountCalculator.cs**

```csharp
namespace ShopWave
{
    public class DiscountCalculator
    {
        public double ApplyDiscount(double originalPrice, int discountPercent)
        {
            if (discountPercent < 0 || discountPercent > 100)
                throw new ArgumentException(
                    "Kortingspercentage moet tussen 0 en 100 liggen.",
                    nameof(discountPercent));

            return originalPrice * (1 - discountPercent / 100.0);
        }
    }
}
```

**DiscountCalculatorTests.cs**

```csharp
using FluentAssertions;
using ShopWave;

namespace ShopWave.Tests
{
    public class DiscountCalculatorTests
    {
        // Zero — geen korting
        [Fact]
        public void ApplyDiscount_WithZeroPercent_ReturnsOriginalPrice()
        {
            DiscountCalculator calculator = new DiscountCalculator();
            double result = calculator.ApplyDiscount(100.0, 0);
            result.Should().Be(100.0);
        }

        // One — een korting
        [Fact]
        public void ApplyDiscount_With25Percent_ReturnsCorrectPrice()
        {
            DiscountCalculator calculator = new DiscountCalculator();
            double result = calculator.ApplyDiscount(80.0, 25);
            result.Should().Be(60.0);
        }

        // Boundary — volledige korting
        [Fact]
        public void ApplyDiscount_With100Percent_ReturnsZero()
        {
            DiscountCalculator calculator = new DiscountCalculator();
            double result = calculator.ApplyDiscount(100.0, 100);
            result.Should().Be(0.0);
        }

        // Exception — negatief percentage
        [Fact]
        public void ApplyDiscount_WithNegativePercent_ThrowsArgumentException()
        {
            DiscountCalculator calculator = new DiscountCalculator();
            Action act = () => calculator.ApplyDiscount(100.0, -1);
            act.Should().Throw<ArgumentException>();
        }

        // Exception — percentage boven 100
        [Fact]
        public void ApplyDiscount_WithPercentOver100_ThrowsArgumentException()
        {
            DiscountCalculator calculator = new DiscountCalculator();
            Action act = () => calculator.ApplyDiscount(100.0, 101);
            act.Should().Throw<ArgumentException>();
        }

        // Boundary — grens bij 0 en 100
        [Theory]
        [InlineData(100.0,  0, 100.0)]
        [InlineData(100.0, 10,  90.0)]
        [InlineData(100.0, 50,  50.0)]
        [InlineData(100.0, 75,  25.0)]
        [InlineData(100.0, 100,   0.0)]
        public void ApplyDiscount_WithValidPercents_ReturnsCorrectPrice(
            double originalPrice, int discountPercent, double expected)
        {
            DiscountCalculator calculator = new DiscountCalculator();
            double result = calculator.ApplyDiscount(originalPrice, discountPercent);
            result.Should().Be(expected);
        }
    }
}
```

---

### Modeloplossing Oefening 3 & 4 — StockService met Verify

**Aangepaste OrderService.cs**

```csharp
namespace ShopWave
{
    public class OrderService
    {
        private readonly IPaymentGateway _gateway;
        private readonly IStockService _stockService;

        public OrderService(IPaymentGateway gateway, IStockService stockService)
        {
            _gateway = gateway;
            _stockService = stockService;
        }

        public string PlaceOrder(int productId, int quantity, double amount)
        {
            string result;

            if (amount <= 0)
            {
                throw new ArgumentException("Bedrag moet groter zijn dan nul.", nameof(amount));
            }

            bool inStock = _stockService.IsInStock(productId, quantity);

            if (!inStock)
            {
                result = "Product niet beschikbaar";
            }
            else
            {
                bool success = _gateway.ProcessPayment(amount);

                if (success)
                {
                    result = "Bestelling bevestigd";
                }
                else
                {
                    result = "Betaling mislukt";
                }
            }

            return result;
        }
    }
}
```

**OrderServiceTests.cs — uitbreiding met Verify**

```csharp
using FluentAssertions;
using Moq;
using ShopWave;

namespace ShopWave.Tests
{
    public class OrderServiceTests
    {
        [Fact]
        public void PlaceOrder_WhenNotInStock_ReturnsProductNietBeschikbaar()
        {
            // Arrange
            Mock<IPaymentGateway> mockGateway = new Mock<IPaymentGateway>();
            Mock<IStockService> mockStock = new Mock<IStockService>();
            mockStock.Setup(x => x.IsInStock(1, 2)).Returns(false);

            OrderService service = new OrderService(mockGateway.Object, mockStock.Object);

            // Act
            string result = service.PlaceOrder(productId: 1, quantity: 2, amount: 50.0);

            // Assert
            result.Should().Be("Product niet beschikbaar");
            mockGateway.Verify(x => x.ProcessPayment(It.IsAny<double>()), Times.Never);
        }

        [Fact]
        public void PlaceOrder_WhenInStock_CallsProcessPaymentExactlyOnce()
        {
            // Arrange
            Mock<IPaymentGateway> mockGateway = new Mock<IPaymentGateway>();
            Mock<IStockService> mockStock = new Mock<IStockService>();
            mockGateway.Setup(x => x.ProcessPayment(50.0)).Returns(true);
            mockStock.Setup(x => x.IsInStock(1, 2)).Returns(true);

            OrderService service = new OrderService(mockGateway.Object, mockStock.Object);

            // Act
            service.PlaceOrder(productId: 1, quantity: 2, amount: 50.0);

            // Assert
            mockGateway.Verify(x => x.ProcessPayment(50.0), Times.Once);
        }

        [Fact]
        public void PlaceOrder_WithInvalidAmount_NeverCallsIsInStock()
        {
            // Arrange
            Mock<IPaymentGateway> mockGateway = new Mock<IPaymentGateway>();
            Mock<IStockService> mockStock = new Mock<IStockService>();

            OrderService service = new OrderService(mockGateway.Object, mockStock.Object);

            // Act
            Action act = () => service.PlaceOrder(productId: 1, quantity: 2, amount: 0.0);

            // Assert
            act.Should().Throw<ArgumentException>();
            mockStock.Verify(x => x.IsInStock(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }
    }
}
```

---

## Samenvatting

| Concept | Wat je moet onthouden |
|---------|-----------------------|
| `[Fact]` | Markeert een methode als test met één vast scenario |
| `[Theory]` + `[InlineData]` | Meerdere testscenario's in één testmethode |
| AAA-patroon | Arrange – Act – Assert: de vaste structuur van elke test |
| ZOMBIES | Richtlijn om te bepalen welke testgevallen je schrijft |
| Interface | Maakt dependency injection en mocking mogelijk |
| DI via constructor | Afhankelijkheid van buitenaf meegeven — niet zelf aanmaken |
| `new Mock<IFoo>()` | Aanmaken van een mock met Moq |
| `mock.Setup(...).Returns(...)` | Bepaalt wat de mock teruggeeft bij een bepaalde aanroep |
| `It.IsAny<T>()` | Geldt voor gelijk welke waarde van het opgegeven type |
| `mock.Verify(...)` | Controleert of een methode aangeroepen werd en hoe vaak |
| `result.Should().Be(x)` | FluentAssertions: controleert een waarde |
| `act.Should().Throw<T>()` | FluentAssertions: controleert dat een exception gegooid wordt |

---

## Volgende les

In Les 2 gaan we dieper in op **Security: CIA-model, hashing en encryptie**. We bekijken hoe ShopWave wachtwoorden correct opslaat, waarom plain-text opslag een ernstig beveiligingsprobleem is en hoe je hashing toepast in C#.