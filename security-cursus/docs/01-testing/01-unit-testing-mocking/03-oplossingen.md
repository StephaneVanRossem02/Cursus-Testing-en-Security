---
title: "Les 1: Oplossingen - Unit Testing en Mocking"
sidebar_label: "Oplossingen"
---

# Oplossingen: Unit Testing en Mocking

**Bekijk dit pas nadat je de oefeningen zelf geprobeerd hebt.** De waarde van oefeningen zit in het zelf denken, niet in het kopiëren. Lees de toelichting ook als je het juist had: er staat uitleg bij over veelgemaakte fouten en alternatieve aanpakken.

---

## Oplossing 1: DiscountCalculator

### DiscountCalculator.cs

```csharp
namespace ShopWave
{
    public class DiscountCalculator
    {
        public double ApplyDiscount(double originalPrice, int discountPercent)
        {
            double result;

            if (discountPercent < 0 || discountPercent > 100)
            {
                throw new ArgumentException(
                    "Kortingspercentage moet tussen 0 en 100 liggen.",
                    nameof(discountPercent));
            }

            result = originalPrice * (1 - discountPercent / 100.0);

            return result;
        }
    }
}
```

### DiscountCalculatorTests.cs

```csharp
using FluentAssertions;
using ShopWave;

namespace ShopWave.Tests
{
    public class DiscountCalculatorTests
    {
        [Fact]
        public void ApplyDiscount_WithZeroPercent_ReturnsOriginalPrice()
        {
            DiscountCalculator calculator = new DiscountCalculator();
            double result = calculator.ApplyDiscount(100.0, 0);
            result.Should().Be(100.0);
        }

        [Fact]
        public void ApplyDiscount_With25Percent_ReturnsCorrectPrice()
        {
            DiscountCalculator calculator = new DiscountCalculator();
            double result = calculator.ApplyDiscount(80.0, 25);
            result.Should().Be(60.0);
        }

        [Fact]
        public void ApplyDiscount_With100Percent_ReturnsZero()
        {
            DiscountCalculator calculator = new DiscountCalculator();
            double result = calculator.ApplyDiscount(100.0, 100);
            result.Should().Be(0.0);
        }

        [Fact]
        public void ApplyDiscount_WithNegativePercent_ThrowsArgumentException()
        {
            DiscountCalculator calculator = new DiscountCalculator();
            Action act = () => calculator.ApplyDiscount(100.0, -1);
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void ApplyDiscount_WithPercentOver100_ThrowsArgumentException()
        {
            DiscountCalculator calculator = new DiscountCalculator();
            Action act = () => calculator.ApplyDiscount(100.0, 101);
            act.Should().Throw<ArgumentException>();
        }

        [Theory]
        [InlineData(100.0,   0, 100.0)]
        [InlineData(100.0,  10,  90.0)]
        [InlineData(100.0,  50,  50.0)]
        [InlineData(100.0,  75,  25.0)]
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

### Toelichting

De formule `originalPrice * (1 - discountPercent / 100.0)` werkt correct omdat `100.0` een `double` is. Als je `100` schrijft (zonder `.0`), deelt C# twee integers en is het resultaat altijd 0 voor percentages kleiner dan 100. Dat is een klassieke fout.

De `[Theory]` met `[InlineData]` vervangt vijf afzonderlijke `[Fact]`-methoden. Elke rij is een apart testgeval in de Test Explorer.

**Veelgemaakte fout:** sommige studenten schrijven de validatie als `discountPercent < 0 || discountPercent > 100` maar vergeten dat 0 en 100 geldige waarden zijn. Test altijd de grenzen expliciet: 0 moet slagen, -1 moet falen, 100 moet slagen, 101 moet falen.

---

## Oplossing 2: OrderService zonder stock

### IPaymentGateway.cs

```csharp
namespace ShopWave
{
    public interface IPaymentGateway
    {
        bool ProcessPayment(double amount);
    }
}
```

### OrderService.cs

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
                throw new ArgumentException(
                    "Bedrag moet groter zijn dan nul.",
                    nameof(amount));
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

### OrderServiceTests.cs

```csharp
using FluentAssertions;
using Moq;
using ShopWave;

namespace ShopWave.Tests
{
    public class OrderServiceTests
    {
        [Fact]
        public void PlaceOrder_WhenPaymentSucceeds_ReturnsBevestigd()
        {
            Mock<IPaymentGateway> mockGateway = new Mock<IPaymentGateway>();
            mockGateway.Setup(x => x.ProcessPayment(50.0)).Returns(true);
            OrderService service = new OrderService(mockGateway.Object);

            string result = service.PlaceOrder(50.0);

            result.Should().Be("Bestelling bevestigd");
        }

        [Fact]
        public void PlaceOrder_WhenPaymentFails_ReturnsMislukt()
        {
            Mock<IPaymentGateway> mockGateway = new Mock<IPaymentGateway>();
            mockGateway.Setup(x => x.ProcessPayment(50.0)).Returns(false);
            OrderService service = new OrderService(mockGateway.Object);

            string result = service.PlaceOrder(50.0);

            result.Should().Be("Betaling mislukt");
        }

        [Fact]
        public void PlaceOrder_WithZeroAmount_ThrowsArgumentException()
        {
            Mock<IPaymentGateway> mockGateway = new Mock<IPaymentGateway>();
            OrderService service = new OrderService(mockGateway.Object);

            Action act = () => service.PlaceOrder(0.0);

            act.Should().Throw<ArgumentException>();
        }
    }
}
```

### Toelichting

`mockGateway.Object` geeft de daadwerkelijke mock-instantie terug. Vergeet `.Object` niet: als je `mockGateway` zelf meegeeft, krijg je een type-fout, want dat is een `Mock<IPaymentGateway>`, niet een `IPaymentGateway`.

**Veelgemaakte fout:** sommige studenten maken `IPaymentGateway` niet als interface maar als abstracte klasse. Moq kan wel met abstracte klassen werken, maar een interface is de juiste aanpak voor Dependency Injection: het legt vast wat een klasse kan, zonder enige implementatiedetails.

---

## Oplossing 3: OrderService met stockcontrole

### IStockService.cs

```csharp
namespace ShopWave
{
    public interface IStockService
    {
        bool IsInStock(int productId, int quantity);
    }
}
```

### OrderService.cs (uitgebreid)

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
                throw new ArgumentException(
                    "Bedrag moet groter zijn dan nul.",
                    nameof(amount));
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

### OrderServiceTests.cs (uitgebreid)

```csharp
using FluentAssertions;
using Moq;
using ShopWave;

namespace ShopWave.Tests
{
    public class OrderServiceTests
    {
        [Fact]
        public void PlaceOrder_WhenNotInStock_ReturnsNietBeschikbaar()
        {
            Mock<IPaymentGateway> mockGateway = new Mock<IPaymentGateway>();
            Mock<IStockService> mockStock = new Mock<IStockService>();
            mockStock.Setup(s => s.IsInStock(1, 1)).Returns(false);
            OrderService service = new OrderService(mockGateway.Object, mockStock.Object);

            string result = service.PlaceOrder(1, 1, 50.0);

            result.Should().Be("Product niet beschikbaar");
        }

        [Fact]
        public void PlaceOrder_WhenNotInStock_NeverCallsProcessPayment()
        {
            Mock<IPaymentGateway> mockGateway = new Mock<IPaymentGateway>();
            Mock<IStockService> mockStock = new Mock<IStockService>();
            mockStock.Setup(s => s.IsInStock(It.IsAny<int>(), It.IsAny<int>())).Returns(false);
            OrderService service = new OrderService(mockGateway.Object, mockStock.Object);

            service.PlaceOrder(1, 1, 50.0);

            mockGateway.Verify(g => g.ProcessPayment(It.IsAny<double>()), Times.Never);
        }

        [Fact]
        public void PlaceOrder_WhenInStockAndPaymentSucceeds_ReturnsBevestigd()
        {
            Mock<IPaymentGateway> mockGateway = new Mock<IPaymentGateway>();
            Mock<IStockService> mockStock = new Mock<IStockService>();
            mockGateway.Setup(g => g.ProcessPayment(50.0)).Returns(true);
            mockStock.Setup(s => s.IsInStock(1, 1)).Returns(true);
            OrderService service = new OrderService(mockGateway.Object, mockStock.Object);

            string result = service.PlaceOrder(1, 1, 50.0);

            result.Should().Be("Bestelling bevestigd");
            mockGateway.Verify(g => g.ProcessPayment(50.0), Times.Once);
        }

        [Fact]
        public void PlaceOrder_WithInvalidAmount_NeverCallsIsInStock()
        {
            Mock<IPaymentGateway> mockGateway = new Mock<IPaymentGateway>();
            Mock<IStockService> mockStock = new Mock<IStockService>();
            OrderService service = new OrderService(mockGateway.Object, mockStock.Object);

            Action act = () => service.PlaceOrder(1, 1, -10.0);

            act.Should().Throw<ArgumentException>();
            mockStock.Verify(s => s.IsInStock(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }
    }
}
```

### Toelichting

De test `WhenNotInStock_NeverCallsProcessPayment` controleert iets wat je niet ziet in het resultaat. Het resultaat `"Product niet beschikbaar"` zegt niets over of de gateway al dan niet aangeroepen werd. Stel dat een student de code zo schrijft dat de gateway toch aangeroepen wordt maar het resultaat genegeerd wordt. Het resultaat is dan correct, maar er is een onnodige en dure netwerkoproep. `Verify` met `Times.Never` vangt precies dat soort fouten op.

**Alternatieve aanpak:** je kan `It.IsAny<int>()` vervangen door de exacte waarden `1` en `1`. Beide werken. `It.IsAny<int>()` is ruimer: de test slaagt ongeacht welke waarden meegegeven worden. Gebruik `It.IsAny` als de exacte waarde niet relevant is voor het scenario dat je test.

---

## Oplossing 4: CheckoutService

### IShippingService.cs

```csharp
namespace ShopWave
{
    public interface IShippingService
    {
        double GetShippingCost(double totalAfterDiscount);
    }
}
```

### CheckoutService.cs

```csharp
namespace ShopWave
{
    public class CheckoutService
    {
        private readonly IShippingService _shippingService;
        private readonly DiscountCalculator _discountCalculator;

        public CheckoutService(IShippingService shippingService)
        {
            _shippingService = shippingService;
            _discountCalculator = new DiscountCalculator();
        }

        public double CalculateFinalTotal(double unitPrice, int quantity, int discountPercent)
        {
            double subtotal = unitPrice * quantity;
            double afterDiscount = _discountCalculator.ApplyDiscount(subtotal, discountPercent);
            double shippingCost = _shippingService.GetShippingCost(afterDiscount);
            double finalTotal = afterDiscount + shippingCost;

            return finalTotal;
        }
    }
}
```

### CheckoutServiceTests.cs

```csharp
using FluentAssertions;
using Moq;
using ShopWave;

namespace ShopWave.Tests
{
    public class CheckoutServiceTests
    {
        [Fact]
        public void CalculateFinalTotal_WithNoDiscountAndShipping_ReturnsTotalPlusShipping()
        {
            Mock<IShippingService> mockShipping = new Mock<IShippingService>();
            mockShipping.Setup(s => s.GetShippingCost(It.IsAny<double>())).Returns(5.0);
            CheckoutService service = new CheckoutService(mockShipping.Object);

            double result = service.CalculateFinalTotal(10.0, 3, 0);

            result.Should().Be(35.0);
        }

        [Fact]
        public void CalculateFinalTotal_CallsGetShippingCostExactlyOnce()
        {
            Mock<IShippingService> mockShipping = new Mock<IShippingService>();
            mockShipping.Setup(s => s.GetShippingCost(It.IsAny<double>())).Returns(0.0);
            CheckoutService service = new CheckoutService(mockShipping.Object);

            service.CalculateFinalTotal(10.0, 1, 0);

            mockShipping.Verify(s => s.GetShippingCost(It.IsAny<double>()), Times.Once);
        }
    }
}
```

### Toelichting

`DiscountCalculator` heeft geen externe afhankelijkheid, dus je hoeft die niet te mocken. Een mock gebruik je alleen als de afhankelijkheid traag is (netwerk, database), niet-deterministisch is (klok, random), of het testgedrag stuurt (returns instellen). `DiscountCalculator` is puur logica, snel en deterministisch.

**Reflectievraag 1:** als je `DiscountCalculator` ook via een interface injecteert, kan je zijn gedrag instellen in tests. Dat is handig als je `CheckoutService` wil testen zonder afhankelijk te zijn van de correcte werking van `DiscountCalculator`. Het nadeel is extra complexiteit. Kies voor een interface zodra een klasse een externe afhankelijkheid krijgt of als je haar gedrag in tests wil sturen.

**Reflectievraag 2:** de `Verify`-test toont dat `GetShippingCost` precies eenmaal aangeroepen wordt. Zonder die test kan een foutieve implementatie die methode twee keer aanroepen terwijl het resultaat toevallig nog correct is.

**Reflectievraag 3:** zonder mock zou `GetShippingCost` een echte netwerkoproep doen. De test wordt traag, kan falen bij een storing bij de externe API en is niet meer herhaalbaar op elke machine.
