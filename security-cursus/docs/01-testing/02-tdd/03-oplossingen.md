---
title: "Les 3: Oplossingen - Test Driven Development"
sidebar_label: "Oplossingen"
---

# Oplossingen: Test Driven Development

**Bekijk dit pas nadat je de oefeningen zelf geprobeerd hebt.** De waarde van TDD zit in het zelf doorlopen van de cyclus, niet in het lezen van het eindresultaat. Lees de toelichting ook als je het juist had.

---

## Oplossing 1: CartService via TDD

### CartItem.cs

```csharp
namespace ShopWave
{
    public class CartItem
    {
        public string Name     { get; set; }
        public double Price    { get; set; }
        public int    Quantity { get; set; }

        public CartItem(string name, double price, int quantity)
        {
            Name     = name;
            Price    = price;
            Quantity = quantity;
        }
    }
}
```

### CartService.cs

```csharp
namespace ShopWave
{
    public class CartService
    {
        private readonly Dictionary<string, CartItem> _items;

        public CartService()
        {
            _items = new Dictionary<string, CartItem>();
        }

        public double Total
        {
            get
            {
                double total = 0;

                foreach (CartItem item in _items.Values)
                {
                    total += item.Price * item.Quantity;
                }

                return total;
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

### CartServiceTests.cs

```csharp
using FluentAssertions;
using ShopWave;

namespace ShopWave.Tests
{
    public class CartServiceTests
    {
        [Fact]
        public void Total_EmptyCart_ReturnsZero()
        {
            CartService cart = new CartService();
            double result    = cart.Total;
            result.Should().Be(0.0);
        }

        [Fact]
        public void AddItem_SingleItem_UpdatesTotal()
        {
            CartService cart = new CartService();
            cart.AddItem("Laptop", 999.99);
            cart.Total.Should().Be(999.99);
        }

        [Fact]
        public void AddItem_MultipleItems_ReturnsCombinedTotal()
        {
            CartService cart = new CartService();
            cart.AddItem("Laptop", 999.99);
            cart.AddItem("Muis",    29.99);
            cart.Total.Should().Be(1029.98);
        }

        [Fact]
        public void AddItem_WithQuantity_MultipliesPriceByQuantity()
        {
            CartService cart = new CartService();
            cart.AddItem("Pen", 2.50, 4);
            cart.Total.Should().Be(10.0);
        }

        [Fact]
        public void RemoveItem_ExistingItem_UpdatesTotal()
        {
            CartService cart = new CartService();
            cart.AddItem("Laptop", 999.99);
            cart.AddItem("Muis",    29.99);
            cart.RemoveItem("Muis");
            cart.Total.Should().Be(999.99);
        }

        [Fact]
        public void AddItem_WithNegativeQuantity_ThrowsArgumentException()
        {
            CartService cart = new CartService();
            Action act = () => cart.AddItem("Laptop", 999.99, -1);
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void Clear_NonEmptyCart_ResetsTotal()
        {
            CartService cart = new CartService();
            cart.AddItem("Laptop", 999.99);
            cart.Clear();
            cart.Total.Should().Be(0.0);
        }
    }
}
```

### Toelichting

De volgorde van de tests volgt de testlijst: van eenvoudig naar complex. `Total_EmptyCart_ReturnsZero` dwingt je om de klasse aan te maken en `Total` als property te definiëren. `AddItem_SingleItem_UpdatesTotal` dwingt je om `AddItem` te implementeren. Elke test voegt precies één nieuwe vereiste toe.

**Veelgemaakte fout:** studenten implementeren `AddItem` direct met de volledige logica (inclusief duplicaatcontrole en hoeveelheid) na de eerste test. In TDD schrijf je pas die logica als er een test is die hem vereist. Begin met de eenvoudigste implementatie die de test doet slagen.

**Veelgemaakte fout:** floating point vergelijkingen. `999.99 + 29.99` geeft in C# soms `1029.9799999999998` door afrondingsfouten. FluentAssertions heeft hiervoor `BeApproximately`:

```csharp
cart.Total.Should().BeApproximately(1029.98, precision: 0.01);
```

Gebruik dit telkens als je meerdere `double`-waarden optelt.

---

## Oplossing 2: CartService met couponondersteuning

### ICouponService.cs

```csharp
namespace ShopWave
{
    public interface ICouponService
    {
        bool IsValid(string code);
        int  GetDiscount(string code);
        void MarkAsUsed(string code);
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
        private          double                        _couponDiscount;

        public CartService(ICouponService couponService)
        {
            _items          = new Dictionary<string, CartItem>();
            _couponService  = couponService;
            _couponDiscount = 0;
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

                return subtotal * (1 - _couponDiscount / 100.0);
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

### CartServiceTests.cs (coupon)

```csharp
using FluentAssertions;
using Moq;
using ShopWave;

namespace ShopWave.Tests
{
    public class CartServiceCouponTests
    {
        [Fact]
        public void ApplyCoupon_WithValidCoupon_ReducesTotal()
        {
            Mock<ICouponService> mockCoupon = new Mock<ICouponService>();
            mockCoupon.Setup(c => c.IsValid("ZOMER10")).Returns(true);
            mockCoupon.Setup(c => c.GetDiscount("ZOMER10")).Returns(10);

            CartService cart = new CartService(mockCoupon.Object);
            cart.AddItem("Laptop", 100.0);
            cart.ApplyCoupon("ZOMER10");

            cart.Total.Should().Be(90.0);
        }

        [Fact]
        public void ApplyCoupon_WithInvalidCoupon_DoesNotChangeTotal()
        {
            Mock<ICouponService> mockCoupon = new Mock<ICouponService>();
            mockCoupon.Setup(c => c.IsValid("ONGELDIG")).Returns(false);

            CartService cart = new CartService(mockCoupon.Object);
            cart.AddItem("Laptop", 100.0);
            cart.ApplyCoupon("ONGELDIG");

            cart.Total.Should().Be(100.0);
        }

        [Fact]
        public void ApplyCoupon_WithValidCoupon_CallsMarkAsUsedOnce()
        {
            Mock<ICouponService> mockCoupon = new Mock<ICouponService>();
            mockCoupon.Setup(c => c.IsValid("ZOMER10")).Returns(true);
            mockCoupon.Setup(c => c.GetDiscount("ZOMER10")).Returns(10);

            CartService cart = new CartService(mockCoupon.Object);
            cart.AddItem("Laptop", 100.0);
            cart.ApplyCoupon("ZOMER10");

            mockCoupon.Verify(c => c.MarkAsUsed("ZOMER10"), Times.Once);
        }

        [Fact]
        public void ApplyCoupon_WithInvalidCoupon_NeverCallsMarkAsUsed()
        {
            Mock<ICouponService> mockCoupon = new Mock<ICouponService>();
            mockCoupon.Setup(c => c.IsValid("ONGELDIG")).Returns(false);

            CartService cart = new CartService(mockCoupon.Object);
            cart.AddItem("Laptop", 100.0);
            cart.ApplyCoupon("ONGELDIG");

            mockCoupon.Verify(c => c.MarkAsUsed(It.IsAny<string>()), Times.Never);
        }
    }
}
```

### Toelichting

`ApplyCoupon` is een goede kandidaat voor TDD omdat de logica afhankelijk is van een externe service. Door `ICouponService` te mocken, test je de logica van `CartService` volledig geïsoleerd, zonder dat de echte `CouponService` betrokken is.

**Veelgemaakte fout:** studenten vergeten de `Verify`-test voor `MarkAsUsed`. Het resultaat van het totaal is correct, maar als `MarkAsUsed` nooit aangeroepen wordt, kan dezelfde coupon onbeperkt hergebruikt worden. De `Verify`-test legt dit gedrag expliciet vast.

**Alternatieve aanpak:** sommige studenten controleren of het totaal opnieuw correct is na een tweede `ApplyCoupon`-aanroep. Dat werkt ook, maar de `Verify`-test is directer: hij test het gedrag, niet enkel het resultaat.

---

## Oplossing 3: OrderService uitgebreid

### OrderService.cs (uitgebreid)

```csharp
namespace ShopWave
{
    public class OrderService
    {
        private readonly IPaymentGateway _gateway;
        private readonly IStockService   _stockService;
        private readonly ICouponService  _couponService;

        public OrderService(
            IPaymentGateway gateway,
            IStockService   stockService,
            ICouponService  couponService)
        {
            _gateway       = gateway;
            _stockService  = stockService;
            _couponService = couponService;
        }

        public string PlaceOrder(int productId, int quantity, double amount, string couponCode = "")
        {
            string result;

            if (amount <= 0)
            {
                throw new ArgumentException("Bedrag moet groter zijn dan nul.", nameof(amount));
            }

            if (couponCode != "" && !_couponService.IsValid(couponCode))
            {
                bool isUsed = false;

                // Onderscheid: onbekende coupon vs. al gebruikte coupon
                // We controleren dit door te kijken of IsValid false geeft terwijl de code niet leeg is.
                // De eenvoudigste aanpak: voeg IsUsed toe aan de interface,
                // of gebruik een aparte IsKnown-methode.
                // Hier: als IsValid false geeft voor een niet-lege code, melden we "Coupon reeds gebruikt."
                // enkel als de coupon al gebruikt is (extra methode op interface nodig).
                // Vereenvoudigde versie: zie toelichting.
                result = "Coupon reeds gebruikt.";
                return result;
            }

            bool inStock = _stockService.IsInStock(productId, quantity);

            if (!inStock)
            {
                result = "Product niet beschikbaar";
            }
            else
            {
                double finalAmount = amount;

                if (couponCode != "" && _couponService.IsValid(couponCode))
                {
                    int discount = _couponService.GetDiscount(couponCode);
                    finalAmount  = amount * (1 - discount / 100.0);
                    _couponService.MarkAsUsed(couponCode);
                }

                bool success = _gateway.ProcessPayment(finalAmount);

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

### OrderServiceTests.cs (coupon)

```csharp
using FluentAssertions;
using Moq;
using ShopWave;

namespace ShopWave.Tests
{
    public class OrderServiceCouponTests
    {
        private Mock<IPaymentGateway> _mockGateway;
        private Mock<IStockService>   _mockStock;
        private Mock<ICouponService>  _mockCoupon;
        private OrderService          _service;

        public OrderServiceCouponTests()
        {
            _mockGateway = new Mock<IPaymentGateway>();
            _mockStock   = new Mock<IStockService>();
            _mockCoupon  = new Mock<ICouponService>();
            _service     = new OrderService(_mockGateway.Object, _mockStock.Object, _mockCoupon.Object);
        }

        [Fact]
        public void PlaceOrder_WithoutCoupon_ProcessesFullAmount()
        {
            _mockStock.Setup(s => s.IsInStock(1, 1)).Returns(true);
            _mockGateway.Setup(g => g.ProcessPayment(100.0)).Returns(true);

            string result = _service.PlaceOrder(1, 1, 100.0);

            result.Should().Be("Bestelling bevestigd");
            _mockGateway.Verify(g => g.ProcessPayment(100.0), Times.Once);
        }

        [Fact]
        public void PlaceOrder_WithValidCoupon_ProcessesReducedAmount()
        {
            _mockStock.Setup(s => s.IsInStock(1, 1)).Returns(true);
            _mockCoupon.Setup(c => c.IsValid("ZOMER10")).Returns(true);
            _mockCoupon.Setup(c => c.GetDiscount("ZOMER10")).Returns(10);
            _mockGateway.Setup(g => g.ProcessPayment(90.0)).Returns(true);

            string result = _service.PlaceOrder(1, 1, 100.0, "ZOMER10");

            result.Should().Be("Bestelling bevestigd");
            _mockGateway.Verify(g => g.ProcessPayment(90.0), Times.Once);
        }

        [Fact]
        public void PlaceOrder_WithUsedCoupon_ReturnsCouponGebruikt()
        {
            _mockCoupon.Setup(c => c.IsValid("ZOMER10")).Returns(false);

            string result = _service.PlaceOrder(1, 1, 100.0, "ZOMER10");

            result.Should().Be("Coupon reeds gebruikt.");
        }

        [Fact]
        public void PlaceOrder_WithUsedCoupon_NeverCallsProcessPayment()
        {
            _mockCoupon.Setup(c => c.IsValid("ZOMER10")).Returns(false);

            _service.PlaceOrder(1, 1, 100.0, "ZOMER10");

            _mockGateway.Verify(g => g.ProcessPayment(It.IsAny<double>()), Times.Never);
        }

        [Fact]
        public void PlaceOrder_WhenNotInStock_ReturnsNietBeschikbaar()
        {
            _mockStock.Setup(s => s.IsInStock(1, 1)).Returns(false);

            string result = _service.PlaceOrder(1, 1, 100.0);

            result.Should().Be("Product niet beschikbaar");
        }
    }
}
```

### Toelichting

De `OrderServiceCouponTests` gebruikt een constructor om de mocks en de service eenmalig aan te maken. Dit is een patroon dat xUnit ondersteunt: voor elke test wordt een nieuwe instantie van de testklasse gemaakt, dus de mocks zijn altijd schoon.

**Veelgemaakte fout:** studenten vergeten dat het bedrag dat naar `ProcessPayment` gaat, het bedrag na korting moet zijn. De `Verify`-aanroep met het exacte bedrag `90.0` maakt dit afdwingbaar.

**Reflectievraag 1:** `return true` als eerste implementatie is correct omdat wet 3 zegt dat je niet meer schrijft dan nodig. De volgende test zal je dwingen om verder te gaan.

**Reflectievraag 2:** de overgang van `List<string>` naar `List<Coupon>` toont dat tests het ontwerp sturen. Je zou dit principe "design by tests" kunnen noemen, maar in TDD-terminologie is het gewoon de normale werking van de Red-Green-Refactor cyclus.

**Reflectievraag 3:** in de groene fase is je enige doel de test laten slagen. Refactoring en implementatie tegelijk doen is twee dingen tegelijk doen. Je verliest het overzicht en riskeert dat je tests breekt zonder te weten waardoor.
