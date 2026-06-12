---
title: "Les 3: TDD — Red-Green-Refactor — Oefeningen"
sidebar_label: "Oefeningen"
---

# Les 3: TDD — Red-Green-Refactor — Oefeningen

> **Code-afspraken:** geen top-level statements · altijd `{}` · max één `return` · geen `break`/`continue` · geen underscore-prefix op parameters · geen geneste klassen · geen ternary/null-conditional · geen tuples · `double` i.p.v. `decimal` · identifiers Engels · tekst Nederlands

---

## Oefening 1 — CartService via TDD

**Opgave:** Bouw `CartService` volledig via TDD (Red-Green-Refactor). Stel eerst je testlijst op. De klasse moet ondersteunen: leeg mandje heeft totaal 0, artikel toevoegen, meerdere artikelen, artikel verwijderen, negatief aantal → `ArgumentException`, mandje leegmaken.

**CartItem.cs**

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
            this.Name     = name;
            this.Price    = price;
            this.Quantity = quantity;
        }
    }
}
```

**CartService.cs**

```csharp
namespace ShopWave
{
    public class CartService
    {
        private readonly Dictionary<string, CartItem> items;

        public CartService()
        {
            this.items = new Dictionary<string, CartItem>();
        }

        public double Total
        {
            get
            {
                double total = 0;

                foreach (CartItem item in this.items.Values)
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

            if (this.items.ContainsKey(name))
            {
                this.items[name].Quantity += quantity;
            }
            else
            {
                this.items[name] = new CartItem(name, price, quantity);
            }
        }

        public void RemoveItem(string name)
        {
            if (this.items.ContainsKey(name))
            {
                this.items.Remove(name);
            }
        }

        public void Clear()
        {
            this.items.Clear();
        }
    }
}
```

**CartServiceTests.cs**

```csharp
using FluentAssertions;
using ShopWave;

namespace ShopWave.Tests
{
    public class CartServiceTests
    {
        // ZOMBIES: Zero
        [Fact]
        public void Total_EmptyCart_ReturnsZero()
        {
            CartService cart = new CartService();
            double result    = cart.Total;
            result.Should().Be(0.0);
        }

        // ZOMBIES: One
        [Fact]
        public void AddItem_SingleItem_UpdatesTotal()
        {
            CartService cart = new CartService();
            cart.AddItem("Laptop", 999.99);
            cart.Total.Should().Be(999.99);
        }

        // ZOMBIES: Many
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

        // ZOMBIES: Exception
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

---

## Oefening 2 — CartService met coupon (ICouponService)

**Opgave:** Breid `CartService` uit met coupon-ondersteuning via `ICouponService` (Dependency Injection). Geldige coupon verlaagt het totaal; ongeldige heeft geen effect; na gebruik wordt coupon gemarkeerd; dezelfde coupon kan niet twee keer gebruikt worden.

**ICouponService.cs**

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

**CartService.cs (met coupon)**

```csharp
namespace ShopWave
{
    public class CartService
    {
        private readonly Dictionary<string, CartItem> items;
        private readonly ICouponService               couponService;
        private          double                        couponDiscount;

        public CartService(ICouponService couponService)
        {
            this.items          = new Dictionary<string, CartItem>();
            this.couponService  = couponService;
            this.couponDiscount = 0;
        }

        public double Total
        {
            get
            {
                double subtotal = 0;

                foreach (CartItem item in this.items.Values)
                {
                    subtotal += item.Price * item.Quantity;
                }

                double result = subtotal * (1 - this.couponDiscount / 100.0);

                return result;
            }
        }

        public void AddItem(string name, double price, int quantity = 1)
        {
            if (quantity < 0)
            {
                throw new ArgumentException("Aantal mag niet negatief zijn.", nameof(quantity));
            }

            if (this.items.ContainsKey(name))
            {
                this.items[name].Quantity += quantity;
            }
            else
            {
                this.items[name] = new CartItem(name, price, quantity);
            }
        }

        public void ApplyCoupon(string code)
        {
            if (this.couponService.IsValid(code))
            {
                this.couponDiscount = this.couponService.GetDiscount(code);
                this.couponService.MarkAsUsed(code);
            }
        }

        public void RemoveItem(string name)
        {
            if (this.items.ContainsKey(name))
            {
                this.items.Remove(name);
            }
        }

        public void Clear()
        {
            this.items.Clear();
        }
    }
}
```

---

## Oefening 3 — OrderService uitbreiden via TDD

**Opgave:** Breid `OrderService.PlaceOrder` uit met coupon-ondersteuning. Geldige coupon verlaagt het bedrag; ongeldige coupon → bestelling toch geplaatst zonder korting; al gebruikte coupon → `"Coupon reeds gebruikt."` Gebruik Moq voor alle drie interfaces.

```csharp
public string PlaceOrder(int productId, int quantity, double amount, string couponCode = "")
```

Tests omvatten minstens: geen coupon, geldige coupon, ongeldige coupon, al gebruikte coupon, en combinatie met niet-beschikbaar product.