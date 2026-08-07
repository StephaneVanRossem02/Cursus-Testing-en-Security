---
title: "Les 1: Oefeningen - Unit Testing en Mocking"
sidebar_label: "Oefeningen"
---

# Oefeningen: Unit Testing en Mocking

---

## Startpakket downloaden

[Download het startpakket van les 1](/downloads/shopwave-start-01-unit-testing-en-mocking.zip) (ZIP)

Hierin staat alles wat je in de vorige lessen gebouwd hebt, samen met de code die je
tijdens de theorie van deze les opbouwt. Wat je in de oefeningen zelf moet schrijven,
staat erin als skelet met de melding `// jouw code hier`.

De webshop zit erbij. Je hoeft geen Razor te kennen: start hem met
`dotnet run --project ShopWave.Web` en open http://localhost:5000. Zo zie je meteen wat je code doet.

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

<h3 class="opdracht-titel">Opdracht</h3>

Schrijf minstens zes tests in een klasse `DiscountCalculatorTests`. Gebruik ZOMBIES als houvast:

| Geval | Wat test je? |
|-------|-------------|
| Zero | kortingspercentage is 0 |
| One | een normaal kortingspercentage, bv. 25% |
| Boundary | kortingspercentage is exact 100 |
| Exception | kortingspercentage is -1 |
| Exception | kortingspercentage is 101 |
| Many | meerdere combinaties via `[Theory]` en `[InlineData]` |

**Verwacht resultaat:** 25% korting op 80 euro geeft 60 euro. 100% korting geeft 0.

**Exception testen** doe je zo:

```csharp
Action act = () => calculator.ApplyDiscount(100.0, -1);
act.Should().Throw<ArgumentException>();
```

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
        private readonly IPaymentGateway gateway;

        public OrderService(IPaymentGateway gateway)
        {
            this.gateway = gateway;
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

            bool success = gateway.ProcessPayment(amount);

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

<h3 class="opdracht-titel">Opdracht</h3>

Schrijf tests voor alle drie de scenario's: ongeldig bedrag, betaling geslaagd en betaling mislukt.

**Structuur van een test met Moq:**

```csharp
Mock<IPaymentGateway> mockGateway = new Mock<IPaymentGateway>();
mockGateway.Setup(x => x.ProcessPayment(50.0)).Returns(true);
OrderService service = new OrderService(mockGateway.Object);
```

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
        private readonly IPaymentGateway gateway;
        private readonly IStockService stockService;

        public OrderService(IPaymentGateway gateway, IStockService stockService)
        {
            this.gateway = gateway;
            this.stockService = stockService;
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

            bool inStock = stockService.IsInStock(productId, quantity);

            if (!inStock)
            {
                result = "Product niet beschikbaar";
            }
            else
            {
                bool success = gateway.ProcessPayment(amount);

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

<h3 class="opdracht-titel">Opdracht</h3>

Schrijf minstens vier tests. Voeg bij twee tests ook een `Verify`-controle toe:

1. Als het product niet op voorraad is, mag `ProcessPayment` **nooit** aangeroepen worden.
2. Als het bedrag ongeldig is, mag `IsInStock` **nooit** aangeroepen worden.

**`Times.Never` gebruiken** om te controleren dat een methode niet aangeroepen werd:

```csharp
mockGateway.Verify(g => g.ProcessPayment(It.IsAny<double>()), Times.Never);
```

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
        private readonly IShippingService shippingService;
        private readonly DiscountCalculator discountCalculator;

        public CheckoutService(IShippingService shippingService)
        {
            this.shippingService = shippingService;
            discountCalculator = new DiscountCalculator();
        }

        public double CalculateFinalTotal(double unitPrice, int quantity, int discountPercent)
        {
            double subtotal = unitPrice * quantity;
            double afterDiscount = discountCalculator.ApplyDiscount(subtotal, discountPercent);
            double shippingCost = shippingService.GetShippingCost(afterDiscount);
            double finalTotal = afterDiscount + shippingCost;

            return finalTotal;
        }
    }
}
```

<h3 class="opdracht-titel">Opdracht</h3>

Schrijf minstens twee tests:

1. Controleer het correcte eindbedrag bij een concrete combinatie van prijs, hoeveelheid en kortingspercentage.
2. Controleer via `Verify` dat `GetShippingCost` precies eenmaal aangeroepen wordt.

**Voorbeeld:** drie producten aan 10 euro per stuk, 0% korting, verzendkost 5 euro. Verwacht eindbedrag: 35 euro.

---

## Zelfreflectie

Beantwoord deze vragen voor jezelf voor je de oplossingen bekijkt:

1. Wat zou er gebeuren als je `DiscountCalculator` ook via een interface injecteert in `CheckoutService`? Wat is het voordeel? Wat is het nadeel?
2. In oefening 3 test je dat `ProcessPayment` nooit aangeroepen wordt als het product niet op voorraad is. Waarom is dat een waardevolle test, ook al test je al dat het resultaat `"Product niet beschikbaar"` is?
3. Stel dat `IShippingService.GetShippingCost` een netwerkoproep doet naar een externe API. Wat zou er gebeuren met je tests als je geen mock gebruikt?

---

## Controleer je werk in de webshop

Start de webshop met `dotnet run --project ShopWave.Web` en open http://localhost:5000. Zo zie je je eigen code draaien in plaats van alleen een groene testbalk.

| Wat je doet | Wat je ziet als je code klopt |
|-------------|-------------------------------|
| Ga naar **Producten** | De vijf producten met hun voorraad. De Webcam staat op 0. |
| Bestel 2 Laptops met 10 procent korting | `Bestelling bevestigd` en een totaal van **1799,98 EUR** |
| Bestel 1 Webcam | `Product niet beschikbaar`, want `IStockService` geeft 0 terug |
| Bestel 1 Laptop zonder korting | `Bestelling bevestigd` en een totaal van **999,99 EUR** |

Onder elk resultaat staat uit welke klasse het komt. Zie je iets anders dan hierboven, dan weet je meteen welke methode je moet nakijken.
