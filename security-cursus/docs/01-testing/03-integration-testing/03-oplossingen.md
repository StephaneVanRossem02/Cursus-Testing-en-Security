---
title: "Les 5: Oplossingen - Integration Testing"
sidebar_label: "Oplossingen"
---

# Oplossingen: Integration Testing

**Bekijk dit pas nadat je de oefeningen zelf geprobeerd hebt.** De waarde van integration testing zit in het zelf ontdekken van integratiefouten, niet in het lezen van het eindresultaat. Lees de toelichting ook als je het juist had.

---

## Oplossing 1: CheckoutService integreren

### CheckoutService.cs

```csharp
namespace ShopWave
{
    public class CheckoutService
    {
        private readonly CartService     _cartService;
        private readonly IPaymentGateway _gateway;

        public CheckoutService(CartService cartService, IPaymentGateway gateway)
        {
            _cartService = cartService;
            _gateway     = gateway;
        }

        public string Checkout()
        {
            double amount = _cartService.Total;
            string result;

            if (amount <= 0)
            {
                result = "Mandje is leeg";
            }
            else
            {
                bool success = _gateway.ProcessPayment(amount);
                result = success ? "Betaling geslaagd" : "Betaling mislukt";
            }

            return result;
        }
    }
}
```

### CheckoutServiceIntegrationTests.cs

```csharp
using FluentAssertions;
using Moq;
using ShopWave;

namespace ShopWave.Tests
{
    public class CheckoutServiceIntegrationTests
    {
        [Fact]
        public void Checkout_WithOneItem_ProcessesCorrectAmount()
        {
            // Arrange
            Mock<IPaymentGateway> mockGateway = new Mock<IPaymentGateway>();
            mockGateway.Setup(g => g.ProcessPayment(It.IsAny<double>())).Returns(true);

            CouponService  couponService  = new CouponService();
            CartService    cartService    = new CartService(couponService);
            CheckoutService checkoutService = new CheckoutService(cartService, mockGateway.Object);

            cartService.AddItem("Laptop", 100.0);

            // Act
            string result = checkoutService.Checkout();

            // Assert
            result.Should().Be("Betaling geslaagd");
            mockGateway.Verify(g => g.ProcessPayment(100.0), Times.Once);
        }

        [Fact]
        public void Checkout_EmptyCart_ReturnsMandjeLegeMelding()
        {
            // Arrange
            Mock<IPaymentGateway> mockGateway = new Mock<IPaymentGateway>();

            CouponService  couponService  = new CouponService();
            CartService    cartService    = new CartService(couponService);
            CheckoutService checkoutService = new CheckoutService(cartService, mockGateway.Object);

            // Act
            string result = checkoutService.Checkout();

            // Assert
            result.Should().Be("Mandje is leeg");
            mockGateway.Verify(g => g.ProcessPayment(It.IsAny<double>()), Times.Never);
        }

        [Fact]
        public void Checkout_WithValidCoupon_ProcessesReducedAmount()
        {
            // Arrange
            Mock<IPaymentGateway> mockGateway = new Mock<IPaymentGateway>();
            mockGateway.Setup(g => g.ProcessPayment(It.IsAny<double>())).Returns(true);

            CouponService  couponService  = new CouponService();
            CartService    cartService    = new CartService(couponService);
            CheckoutService checkoutService = new CheckoutService(cartService, mockGateway.Object);

            cartService.AddItem("Laptop", 100.0);
            cartService.ApplyCoupon("ZOMER10");

            // Act
            string result = checkoutService.Checkout();

            // Assert
            result.Should().Be("Betaling geslaagd");
            mockGateway.Verify(g => g.ProcessPayment(90.0), Times.Once);
        }

        [Fact]
        public void Checkout_WhenPaymentFails_ReturnsMislukt()
        {
            // Arrange
            Mock<IPaymentGateway> mockGateway = new Mock<IPaymentGateway>();
            mockGateway.Setup(g => g.ProcessPayment(It.IsAny<double>())).Returns(false);

            CouponService  couponService  = new CouponService();
            CartService    cartService    = new CartService(couponService);
            CheckoutService checkoutService = new CheckoutService(cartService, mockGateway.Object);

            cartService.AddItem("Laptop", 100.0);

            // Act
            string result = checkoutService.Checkout();

            // Assert
            result.Should().Be("Betaling mislukt");
        }
    }
}
```

### Toelichting

`CartService` en `CouponService` zijn echt. Ze werken samen zoals in productie. De `Verify`-aanroep in het couponscenario is essentieel: die bewijst dat het gecombineerde totaal van `CartService.Total` na `ApplyCoupon` het juiste bedrag doorstuurt naar de gateway.

**Veelgemaakte fout:** studenten mocken `CouponService` ook. Dat maakt de test een unit test, geen integration test. Als je `CouponService` mockt, test je niet of de echte couponlogica correct integreert met `CartService`.

---

## Oplossing 2: DiscountCalculator integreren

### DiscountCalculator.cs

```csharp
namespace ShopWave
{
    public class DiscountCalculator
    {
        public double Apply(double amount, int discountPercent)
        {
            if (discountPercent < 0 || discountPercent > 100)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(discountPercent),
                    "Kortingspercentage moet tussen 0 en 100 liggen.");
            }

            return amount * (1 - discountPercent / 100.0);
        }
    }
}
```

### CartService.cs (uitgebreid)

```csharp
namespace ShopWave
{
    public class CartService
    {
        private readonly Dictionary<string, CartItem> _items;
        private readonly ICouponService               _couponService;
        private readonly DiscountCalculator           _discountCalculator;
        private          int                           _couponDiscount;

        public CartService(ICouponService couponService, DiscountCalculator discountCalculator)
        {
            _items              = new Dictionary<string, CartItem>();
            _couponService      = couponService;
            _discountCalculator = discountCalculator;
            _couponDiscount     = 0;
        }

        public double Total
        {
            get
            {
                double subtotal = 0;

                foreach (CartItem item in _items.Values)
                {
                    subtotal += item.Price * item.Quantity;
                }

                return _couponDiscount > 0
                    ? _discountCalculator.Apply(subtotal, _couponDiscount)
                    : subtotal;
            }
        }

        public void AddItem(string name, double price, int quantity = 1)
        {
            if (quantity < 0)
            {
                throw new ArgumentException("Aantal mag niet negatief zijn.", nameof(quantity));
            }

            if (_items.ContainsKey(name))
            {
                _items[name].Quantity += quantity;
            }
            else
            {
                _items[name] = new CartItem(name, price, quantity);
            }
        }

        public void ApplyCoupon(string code)
        {
            if (_couponService.IsValid(code))
            {
                _couponDiscount = _couponService.GetDiscount(code);
                _couponService.MarkAsUsed(code);
            }
        }

        public void RemoveItem(string name)
        {
            if (_items.ContainsKey(name))
            {
                _items.Remove(name);
            }
        }

        public void Clear()
        {
            _items.Clear();
        }
    }
}
```

### DiscountIntegrationTests.cs

```csharp
using FluentAssertions;
using ShopWave;

namespace ShopWave.Tests
{
    public class DiscountIntegrationTests
    {
        [Fact]
        public void Total_WithValidCouponZOMER10_AppliesTenPercent()
        {
            // Arrange
            CouponService       couponService       = new CouponService();
            DiscountCalculator  discountCalculator  = new DiscountCalculator();
            CartService         cartService         = new CartService(couponService, discountCalculator);

            cartService.AddItem("Laptop", 100.0);
            cartService.ApplyCoupon("ZOMER10");

            // Act
            double result = cartService.Total;

            // Assert
            result.Should().BeApproximately(90.0, precision: 0.01);
        }

        [Fact]
        public void Total_WithInvalidCoupon_ReturnsFullAmount()
        {
            // Arrange
            CouponService       couponService       = new CouponService();
            DiscountCalculator  discountCalculator  = new DiscountCalculator();
            CartService         cartService         = new CartService(couponService, discountCalculator);

            cartService.AddItem("Laptop", 100.0);
            cartService.ApplyCoupon("ONGELDIG");

            // Act
            double result = cartService.Total;

            // Assert
            result.Should().BeApproximately(100.0, precision: 0.01);
        }

        [Fact]
        public void Total_WithCouponWELKOM20_AppliesTwentyPercent()
        {
            // Arrange
            CouponService       couponService       = new CouponService();
            DiscountCalculator  discountCalculator  = new DiscountCalculator();
            CartService         cartService         = new CartService(couponService, discountCalculator);

            cartService.AddItem("Artikel", 50.0);
            cartService.ApplyCoupon("WELKOM20");

            // Act
            double result = cartService.Total;

            // Assert
            result.Should().BeApproximately(40.0, precision: 0.01);
        }

        [Fact]
        public void Total_TwoItemsWithCouponTROUWE5_AppliesFivePercent()
        {
            // Arrange
            CouponService       couponService       = new CouponService();
            DiscountCalculator  discountCalculator  = new DiscountCalculator();
            CartService         cartService         = new CartService(couponService, discountCalculator);

            cartService.AddItem("ArtikelA", 100.0);
            cartService.AddItem("ArtikelB", 100.0);
            cartService.ApplyCoupon("TROUWE5");

            // Act
            double result = cartService.Total;

            // Assert
            result.Should().BeApproximately(190.0, precision: 0.01);
        }
    }
}
```

### Toelichting

Geen enkele eigen klasse is gemockt. De integration test bewijst dat de keten `ApplyCoupon` op `CartService` de juiste waarde haalt via `CouponService`, die waarde doorgeeft aan `DiscountCalculator.Apply`, en dat `Total` het gecorrigeerde bedrag teruggeeft.

**Veelgemaakte fout:** floating point vergelijkingen met `==`. Gebruik altijd `BeApproximately` als je meerdere `double`-waarden combineert. `100.0 * 0.9` kan `89.99999...` geven door afrondingsfouten.

**Reflectievraag:** wat zou er falen als `CouponService.GetDiscount` voor `ZOMER10` per ongeluk `1` teruggeeft in plaats van `10`? De unit test van `DiscountCalculator` blijft slagen. De unit test van `CartService` met een mock ook, want de mock geeft terug wat jij instelt. Alleen de integration test vangt dit op.

---

## Oplossing 3: de volledige bestelflow

### FullCheckoutFlowIntegrationTests.cs

```csharp
using FluentAssertions;
using Moq;
using ShopWave;

namespace ShopWave.Tests
{
    public class FullCheckoutFlowIntegrationTests
    {
        [Fact]
        public void PlaceOrder_WithValidCouponZOMER10_ProcessesReducedAmount()
        {
            // Arrange
            Mock<IPaymentGateway> mockGateway = new Mock<IPaymentGateway>();
            mockGateway.Setup(g => g.ProcessPayment(It.IsAny<double>())).Returns(true);

            Mock<IStockService> mockStock = new Mock<IStockService>();
            mockStock.Setup(s => s.IsInStock(It.IsAny<int>(), It.IsAny<int>())).Returns(true);

            CouponService      couponService      = new CouponService();
            DiscountCalculator discountCalculator = new DiscountCalculator();
            OrderService       orderService       = new OrderService(
                mockGateway.Object,
                mockStock.Object,
                couponService);

            CartService cartService = new CartService(couponService, discountCalculator);
            cartService.AddItem("Laptop", 100.0);
            cartService.ApplyCoupon("ZOMER10");

            // Act
            string result = orderService.PlaceOrder(1, 1, cartService.Total);

            // Assert
            result.Should().Be("Bestelling bevestigd");
            mockGateway.Verify(g => g.ProcessPayment(90.0), Times.Once);
        }

        [Fact]
        public void PlaceOrder_WithInvalidCoupon_ProcessesFullAmount()
        {
            // Arrange
            Mock<IPaymentGateway> mockGateway = new Mock<IPaymentGateway>();
            mockGateway.Setup(g => g.ProcessPayment(It.IsAny<double>())).Returns(true);

            Mock<IStockService> mockStock = new Mock<IStockService>();
            mockStock.Setup(s => s.IsInStock(It.IsAny<int>(), It.IsAny<int>())).Returns(true);

            CouponService      couponService      = new CouponService();
            DiscountCalculator discountCalculator = new DiscountCalculator();
            OrderService       orderService       = new OrderService(
                mockGateway.Object,
                mockStock.Object,
                couponService);

            CartService cartService = new CartService(couponService, discountCalculator);
            cartService.AddItem("Laptop", 100.0);
            cartService.ApplyCoupon("ONGELDIG");

            // Act
            string result = orderService.PlaceOrder(1, 1, cartService.Total);

            // Assert
            result.Should().Be("Bestelling bevestigd");
            mockGateway.Verify(g => g.ProcessPayment(100.0), Times.Once);
        }

        [Fact]
        public void PlaceOrder_MultipleItemsNoCoupon_ProcessesCorrectTotal()
        {
            // Arrange
            Mock<IPaymentGateway> mockGateway = new Mock<IPaymentGateway>();
            mockGateway.Setup(g => g.ProcessPayment(It.IsAny<double>())).Returns(true);

            Mock<IStockService> mockStock = new Mock<IStockService>();
            mockStock.Setup(s => s.IsInStock(It.IsAny<int>(), It.IsAny<int>())).Returns(true);

            CouponService      couponService      = new CouponService();
            DiscountCalculator discountCalculator = new DiscountCalculator();
            OrderService       orderService       = new OrderService(
                mockGateway.Object,
                mockStock.Object,
                couponService);

            CartService cartService = new CartService(couponService, discountCalculator);
            cartService.AddItem("Laptop", 80.0);
            cartService.AddItem("Muis",   20.0);

            // Act
            string result = orderService.PlaceOrder(1, 1, cartService.Total);

            // Assert
            result.Should().Be("Bestelling bevestigd");
            mockGateway.Verify(g => g.ProcessPayment(100.0), Times.Once);
        }

        [Fact]
        public void PlaceOrder_WhenNotInStock_ReturnsNietBeschikbaar()
        {
            // Arrange
            Mock<IPaymentGateway> mockGateway = new Mock<IPaymentGateway>();

            Mock<IStockService> mockStock = new Mock<IStockService>();
            mockStock.Setup(s => s.IsInStock(It.IsAny<int>(), It.IsAny<int>())).Returns(false);

            CouponService      couponService      = new CouponService();
            DiscountCalculator discountCalculator = new DiscountCalculator();
            OrderService       orderService       = new OrderService(
                mockGateway.Object,
                mockStock.Object,
                couponService);

            CartService cartService = new CartService(couponService, discountCalculator);
            cartService.AddItem("Laptop", 100.0);

            // Act
            string result = orderService.PlaceOrder(1, 1, cartService.Total);

            // Assert
            result.Should().Be("Product niet beschikbaar");
            mockGateway.Verify(g => g.ProcessPayment(It.IsAny<double>()), Times.Never);
        }
    }
}
```

### Toelichting

De mocks zijn alleen voor `IPaymentGateway` en `IStockService`. Dit zijn externe diensten. Alle eigen klassen zijn echt.

**Veelgemaakte fout:** het totaal berekenen in de test in plaats van via `CartService.Total`. Schrijf `orderService.PlaceOrder(1, 1, cartService.Total)`, niet `orderService.PlaceOrder(1, 1, 90.0)`. Als je het bedrag hardcodeert, test je de samenwerking niet.

**Veelgemaakte fout:** `CouponService` zowel doorgeven aan `CartService` als aan `OrderService`, maar met twee aparte instanties. Beide klassen moeten dezelfde `CouponService`-instantie gebruiken. Als `CartService.ApplyCoupon` de coupon markeert als gebruikt, moet `OrderService` dat ook weten.

---

## Oplossing 4: de callback-techniek

### OrderConfirmationService.cs

```csharp
namespace ShopWave
{
    public class OrderConfirmationService
    {
        private readonly Action<string> _onConfirmationCodeGenerated;

        public OrderConfirmationService()
        {
            _onConfirmationCodeGenerated = null;
        }

        public OrderConfirmationService(Action<string> onConfirmationCodeGenerated)
        {
            _onConfirmationCodeGenerated = onConfirmationCodeGenerated;
        }

        public string GenerateConfirmationCode(int orderId)
        {
            string code = $"ORD-{orderId:D6}-{Guid.NewGuid().ToString("N")[..4].ToUpper()}";

            if (_onConfirmationCodeGenerated != null)
            {
                _onConfirmationCodeGenerated(code);
            }

            return code;
        }

        public bool ValidateCode(string code)
        {
            return code != null && code.StartsWith("ORD-") && code.Length >= 12;
        }
    }
}
```

### OrderConfirmationServiceIntegrationTests.cs

```csharp
using FluentAssertions;
using ShopWave;

namespace ShopWave.Tests
{
    public class OrderConfirmationServiceIntegrationTests
    {
        [Fact]
        public void GenerateConfirmationCode_ForOrderId1_StartsWithORD()
        {
            // Arrange
            string capturedCode = string.Empty;

            OrderConfirmationService service = new OrderConfirmationService(
                onConfirmationCodeGenerated: code => { capturedCode = code; });

            // Act
            string result = service.GenerateConfirmationCode(1);

            // Assert
            result.Should().StartWith("ORD-");
            capturedCode.Should().StartWith("ORD-");
        }

        [Fact]
        public void GenerateConfirmationCode_CallbackReceivesSameValueAsReturn()
        {
            // Arrange
            string capturedCode = string.Empty;

            OrderConfirmationService service = new OrderConfirmationService(
                onConfirmationCodeGenerated: code => { capturedCode = code; });

            // Act
            string result = service.GenerateConfirmationCode(42);

            // Assert
            capturedCode.Should().Be(result);
        }

        [Fact]
        public void GenerateConfirmationCode_Result_PassesValidation()
        {
            // Arrange
            OrderConfirmationService service = new OrderConfirmationService();

            // Act
            string code = service.GenerateConfirmationCode(1);
            bool   isValid = service.ValidateCode(code);

            // Assert
            isValid.Should().BeTrue();
        }

        [Fact]
        public void GenerateConfirmationCode_TwoCalls_ProduceUniqueCode()
        {
            // Arrange
            OrderConfirmationService service = new OrderConfirmationService();

            // Act
            string code1 = service.GenerateConfirmationCode(1);
            string code2 = service.GenerateConfirmationCode(1);

            // Assert
            code1.Should().NotBe(code2);
        }
    }
}
```

### Toelichting

De callback geeft je de waarde op het moment dat hij gegenereerd wordt. In productie gebruik je de constructor zonder callback. In tests geef je een lambda mee die de waarde opvangt in een lokale variabele.

**Veelgemaakte fout:** `capturedCode` initialiseren zonder `string.Empty`. Als de callback nooit opgeroepen wordt en je controleert `capturedCode`, is die `null` in plaats van een lege string. `string.Empty` maakt dit onderscheid duidelijk.

**Reflectievraag 3:** een publieke property `ConfirmationCode` toevoegen wijzigt de publieke interface van de klasse. Code die de klasse gebruikt, heeft nu toegang tot een eigenschap die alleen voor tests bedoeld is. De callback-techniek voegt niets toe aan de publieke interface: de productie-constructor heeft geen callback. Tests gebruiken de tweede constructor. De klasse blijft clean.

**Reflectievraag 4:** mogelijke oorzaken als unit tests slagen maar integration tests falen:
- Klasse A geeft in een randgeval een andere waarde terug dan de mock van klasse A ooit teruggaf
- De volgorde van methodeaanroepen tussen klassen is verkeerd
- Twee klassen gebruiken een impliciete afspraak over een formaat of type die in productie niet klopt
- Een shared instantie wordt verwacht maar beide klassen krijgen een aparte instantie mee
