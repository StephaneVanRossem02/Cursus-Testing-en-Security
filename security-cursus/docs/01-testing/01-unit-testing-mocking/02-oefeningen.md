---
title: "Les 1: Unit Testing & Mocking — Oefeningen"
sidebar_label: "Oefeningen"
---

# Les 1: Unit Testing & Mocking — Oefeningen

> **Code-afspraken:** geen top-level statements · altijd `{}` · max één `return` · geen `break`/`continue` · geen underscore-prefix op parameters · geen geneste klassen · geen ternary/null-conditional · geen tuples · `double` i.p.v. `decimal` · identifiers Engels · tekst Nederlands

---

## Oefening 1 — DiscountCalculator

**Opgave:** Schrijf een klasse `DiscountCalculator` met methode `ApplyDiscount(double originalPrice, int discountPercent)`. Een negatief of >100% kortingspercentage gooit een `ArgumentException`. Schrijf minstens zes tests via ZOMBIES.

**DiscountCalculator.cs**

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

**DiscountCalculatorTests.cs**

```csharp
using FluentAssertions;
using ShopWave;

namespace ShopWave.Tests
{
    public class DiscountCalculatorTests
    {
        // ZOMBIES: Zero — geen korting
        [Fact]
        public void ApplyDiscount_WithZeroPercent_ReturnsOriginalPrice()
        {
            DiscountCalculator calculator = new DiscountCalculator();
            double result = calculator.ApplyDiscount(100.0, 0);
            result.Should().Be(100.0);
        }

        // ZOMBIES: One
        [Fact]
        public void ApplyDiscount_With25Percent_ReturnsCorrectPrice()
        {
            DiscountCalculator calculator = new DiscountCalculator();
            double result = calculator.ApplyDiscount(80.0, 25);
            result.Should().Be(60.0);
        }

        // ZOMBIES: Boundary — 100%
        [Fact]
        public void ApplyDiscount_With100Percent_ReturnsZero()
        {
            DiscountCalculator calculator = new DiscountCalculator();
            double result = calculator.ApplyDiscount(100.0, 100);
            result.Should().Be(0.0);
        }

        // ZOMBIES: Exception — negatief
        [Fact]
        public void ApplyDiscount_WithNegativePercent_ThrowsArgumentException()
        {
            DiscountCalculator calculator = new DiscountCalculator();
            Action act = () => calculator.ApplyDiscount(100.0, -1);
            act.Should().Throw<ArgumentException>();
        }

        // ZOMBIES: Exception — boven 100
        [Fact]
        public void ApplyDiscount_WithPercentOver100_ThrowsArgumentException()
        {
            DiscountCalculator calculator = new DiscountCalculator();
            Action act = () => calculator.ApplyDiscount(100.0, 101);
            act.Should().Throw<ArgumentException>();
        }

        // ZOMBIES: Many — via [Theory]
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

---

## Oefening 3 — StockService met Moq

**Opgave:** Voeg `IStockService` toe aan `OrderService`. Als het product niet op voorraad is, retourneert `PlaceOrder` `"Product niet beschikbaar"` zonder de gateway aan te roepen.

**IStockService.cs**

```csharp
namespace ShopWave
{
    public interface IStockService
    {
        bool IsInStock(int productId, int quantity);
    }
}
```

**OrderService.cs**

```csharp
namespace ShopWave
{
    public class OrderService
    {
        private readonly IPaymentGateway gateway;
        private readonly IStockService   stockService;

        public OrderService(IPaymentGateway gateway, IStockService stockService)
        {
            this.gateway      = gateway;
            this.stockService = stockService;
        }

        public string PlaceOrder(int productId, int quantity, double amount)
        {
            string result;

            if (amount <= 0)
            {
                throw new ArgumentException("Bedrag moet groter zijn dan nul.", nameof(amount));
            }

            bool inStock = this.stockService.IsInStock(productId, quantity);

            if (!inStock)
            {
                result = "Product niet beschikbaar";
            }
            else
            {
                bool success = this.gateway.ProcessPayment(amount);

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

**Relevante tests in OrderServiceTests.cs**

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
            // Arrange
            Mock<IPaymentGateway> mockGateway = new Mock<IPaymentGateway>();
            Mock<IStockService>   mockStock   = new Mock<IStockService>();
            mockStock.Setup(s => s.IsInStock(1, 1)).Returns(false);

            OrderService service = new OrderService(mockGateway.Object, mockStock.Object);

            // Act
            string result = service.PlaceOrder(1, 1, 50.0);

            // Assert
            result.Should().Be("Product niet beschikbaar");
        }

        [Fact]
        public void PlaceOrder_WhenInStockAndPaymentSucceeds_ReturnsBevestigd()
        {
            // Arrange
            Mock<IPaymentGateway> mockGateway = new Mock<IPaymentGateway>();
            Mock<IStockService>   mockStock   = new Mock<IStockService>();
            mockGateway.Setup(g => g.ProcessPayment(50.0)).Returns(true);
            mockStock.Setup(s => s.IsInStock(1, 1)).Returns(true);

            OrderService service = new OrderService(mockGateway.Object, mockStock.Object);

            // Act
            string result = service.PlaceOrder(1, 1, 50.0);

            // Assert
            result.Should().Be("Bestelling bevestigd");
        }
    }
}
```

---

## Oefening 4 — Verify correct toepassen

**Opgave:** Voeg drie `Verify`-controles toe: (1) niet in stock → `ProcessPayment` nooit aangeroepen, (2) succesvolle betaling → `ProcessPayment` precies eenmaal, (3) ongeldig bedrag → `IsInStock` nooit aangeroepen.

```csharp
[Fact]
public void PlaceOrder_WhenNotInStock_NeverCallsProcessPayment()
{
    // Arrange
    Mock<IPaymentGateway> mockGateway = new Mock<IPaymentGateway>();
    Mock<IStockService>   mockStock   = new Mock<IStockService>();
    mockStock.Setup(s => s.IsInStock(It.IsAny<int>(), It.IsAny<int>())).Returns(false);

    OrderService service = new OrderService(mockGateway.Object, mockStock.Object);
    service.PlaceOrder(1, 1, 50.0);

    // Assert
    mockGateway.Verify(g => g.ProcessPayment(It.IsAny<double>()), Times.Never);
}

[Fact]
public void PlaceOrder_WithInvalidAmount_NeverCallsIsInStock()
{
    // Arrange
    Mock<IPaymentGateway> mockGateway = new Mock<IPaymentGateway>();
    Mock<IStockService>   mockStock   = new Mock<IStockService>();
    OrderService service = new OrderService(mockGateway.Object, mockStock.Object);

    // Act
    Action act = () => service.PlaceOrder(1, 1, -10.0);

    // Assert
    act.Should().Throw<ArgumentException>();
    mockStock.Verify(s => s.IsInStock(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
}
```

---

## Oefening 5 — CheckoutService

**Opgave:** `CheckoutService.CalculateFinalTotal(double unitPrice, int quantity, int discountPercent)` berekent subtotaal, past korting toe en voegt verzendkosten toe via `IShippingService`.

**CheckoutService.cs**

```csharp
namespace ShopWave
{
    public class CheckoutService
    {
        private readonly IShippingService   shippingService;
        private readonly DiscountCalculator discountCalculator;

        public CheckoutService(IShippingService shippingService)
        {
            this.shippingService    = shippingService;
            this.discountCalculator = new DiscountCalculator();
        }

        public double CalculateFinalTotal(double unitPrice, int quantity, int discountPercent)
        {
            double subtotal      = unitPrice * quantity;
            double afterDiscount = this.discountCalculator.ApplyDiscount(subtotal, discountPercent);
            double shippingCost  = this.shippingService.GetShippingCost(afterDiscount);
            double finalTotal    = afterDiscount + shippingCost;

            return finalTotal;
        }
    }
}
```

**CheckoutServiceTests.cs**

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
            // Arrange
            Mock<IShippingService> mockShipping = new Mock<IShippingService>();
            mockShipping.Setup(s => s.GetShippingCost(It.IsAny<double>())).Returns(5.0);
            CheckoutService service = new CheckoutService(mockShipping.Object);

            // Act
            double result = service.CalculateFinalTotal(10.0, 2, 0);

            // Assert
            result.Should().Be(25.0);
        }

        [Fact]
        public void CalculateFinalTotal_CallsGetShippingCostExactlyOnce()
        {
            // Arrange
            Mock<IShippingService> mockShipping = new Mock<IShippingService>();
            mockShipping.Setup(s => s.GetShippingCost(It.IsAny<double>())).Returns(0.0);
            CheckoutService service = new CheckoutService(mockShipping.Object);

            // Act
            service.CalculateFinalTotal(10.0, 1, 0);

            // Assert
            mockShipping.Verify(s => s.GetShippingCost(It.IsAny<double>()), Times.Once);
        }
    }
}
```