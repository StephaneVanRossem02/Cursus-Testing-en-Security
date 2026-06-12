---
title: "Les 1: Oefeningen - Unit Testing en Mocking"
sidebar_label: "Oefeningen"
---

# Oefeningen: Unit Testing en Mocking

---

## Oefening 1: DiscountCalculator

**Leerdoel:** je schrijft tests met het AAA-patroon en past ZOMBIES toe om testgevallen te kiezen.

**Moeilijkheidsgraad:** laag

### De klasse

Maak een nieuw testproject aan en voeg onderstaande klasse toe. Jij schrijft de tests, niet de implementatie.

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

### Opdracht

Schrijf minstens zes tests in een klasse `DiscountCalculatorTests`. Gebruik ZOMBIES als houvast:

| Geval | Wat test je? |
|-------|-------------|
| Zero | kortingspercentage is 0 |
| One | een normaal kortingspercentage, bv. 25% |
| Boundary | kortingspercentage is exact 100 |
| Exception | kortingspercentage is -1 |
| Exception | kortingspercentage is 101 |
| Many | meerdere combinaties via `[Theory]` en `[InlineData]` |

:::tip
Als je 25% korting geeft op een prijs van 80 euro, verwacht je 60 euro terug. Als je 100% korting geeft, verwacht je 0.
:::

:::warning Exception testen
Test of een exception echt gegooid wordt zo:

```csharp
Action act = () => calculator.ApplyDiscount(100.0, -1);
act.Should().Throw<ArgumentException>();
```
:::

---

## Oefening 2: OrderService zonder stock

**Leerdoel:** je schrijft tests voor een klasse die een interface gebruikt als afhankelijkheid, en je maakt een mock aan met Moq.

**Moeilijkheidsgraad:** gemiddeld

### De klassen

```csharp
namespace ShopWave
{
    public interface IPaymentGateway
    {
        bool ProcessPayment(double amount);
    }
}
```

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

### Opdracht

Schrijf tests voor alle drie de scenario's: ongeldig bedrag, betaling geslaagd en betaling mislukt.

:::tip Structuur van een test met Moq
```csharp
Mock<IPaymentGateway> mockGateway = new Mock<IPaymentGateway>();
mockGateway.Setup(x => x.ProcessPayment(50.0)).Returns(true);
OrderService service = new OrderService(mockGateway.Object);
```
:::

---

## Oefening 3: OrderService met stockcontrole

**Leerdoel:** je werkt met meerdere mocks tegelijk en gebruikt `Verify` om aan te tonen dat methoden wel of niet aangeroepen werden.

**Moeilijkheidsgraad:** gemiddeld

### De klassen

```csharp
namespace ShopWave
{
    public interface IStockService
    {
        bool IsInStock(int productId, int quantity);
    }
}
```

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

### Opdracht

Schrijf minstens vier tests. Voeg bij twee tests ook een `Verify`-controle toe:

1. Als het product niet op voorraad is, mag `ProcessPayment` **nooit** aangeroepen worden.
2. Als het bedrag ongeldig is, mag `IsInStock` **nooit** aangeroepen worden.

:::tip
Gebruik `Times.Never` om te controleren dat een methode niet aangeroepen werd:
```csharp
mockGateway.Verify(g => g.ProcessPayment(It.IsAny<double>()), Times.Never);
```
:::

---

## Oefening 4: CheckoutService

**Leerdoel:** je test een klasse die meerdere afhankelijkheden combineert.

**Moeilijkheidsgraad:** hoog

### De klassen

```csharp
namespace ShopWave
{
    public interface IShippingService
    {
        double GetShippingCost(double totalAfterDiscount);
    }
}
```

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

### Opdracht

Schrijf minstens twee tests:

1. Controleer het correcte eindbedrag bij een concrete combinatie van prijs, hoeveelheid en kortingspercentage.
2. Controleer via `Verify` dat `GetShippingCost` precies eenmaal aangeroepen wordt.

:::tip Voorbeeld om te controleren
Drie producten aan 10 euro per stuk, 0% korting, verzendkost 5 euro. Verwacht eindbedrag: 35 euro.
:::

---

## Zelfreflectie

Beantwoord deze vragen voor jezelf voor je de oplossingen bekijkt:

1. Wat zou er gebeuren als je `DiscountCalculator` ook via een interface injecteert in `CheckoutService`? Wat is het voordeel? Wat is het nadeel?
2. In oefening 3 test je dat `ProcessPayment` nooit aangeroepen wordt als het product niet op voorraad is. Waarom is dat een waardevolle test, ook al test je al dat het resultaat `"Product niet beschikbaar"` is?
3. Stel dat `IShippingService.GetShippingCost` een netwerkoproep doet naar een externe API. Wat zou er gebeuren met je tests als je geen mock gebruikt?
