---
title: "Les 5: Oefeningen - Integration Testing"
sidebar_label: "Oefeningen"
---

# Oefeningen: Integration Testing

Werk de oefeningen in volgorde. Gebruik bij elke oefening echte klassen voor eigen code. Mock alleen externe diensten zoals betaalgateways.

---

## Oefening 1: CheckoutService integreren

**Leerdoel:** je schrijft een integration test waarbij meerdere eigen klassen samenwerken zonder mocks voor eigen code.

**Moeilijkheidsgraad:** basis

### Startcode

Voeg deze klasse toe aan `ShopWave`. Je hoeft hem niet aan te passen.

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

Je hebt ook de `CartService` en `CouponService` uit les 3 nodig. Gebruik de versie met `ICouponService`-constructor.

<h3 class="opdracht-titel">Opdracht</h3>

Schrijf integration tests voor de volledige checkout-flow. Gebruik echte `CartService`- en `CouponService`-instanties. Mock alleen `IPaymentGateway`.

Test minstens de volgende scenario's:

| Scenario | Verwacht resultaat |
|---------|-------------------|
| Mandje met één artikel, betaling geslaagd | "Betaling geslaagd" |
| Leeg mandje | "Mandje is leeg" |
| Artikel toevoegen met geldige coupon, controleer het juiste bedrag naar de gateway | bedrag na korting |
| Betaling mislukt | "Betaling mislukt" |

**Controleer bij het couponscenario** via `mockGateway.Verify(...)` of het exacte bedrag na korting naar de gateway gestuurd wordt.

---

## Oefening 2: DiscountCalculator integreren

**Leerdoel:** je test de samenwerking tussen `CartService` en een berekeningsklasse zonder mock.

**Moeilijkheidsgraad:** basis

### Startcode

Voeg deze klasse toe aan `ShopWave`:

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

Pas `CartService` aan zodat die `DiscountCalculator` gebruikt in de `Total`-berekening als er een coupon is toegepast:

```csharp
// Constructor uitbreiden
public CartService(ICouponService couponService, DiscountCalculator discountCalculator)
{
    _items              = new Dictionary<string, CartItem>();
    _couponService      = couponService;
    _discountCalculator = discountCalculator;
    _couponDiscount     = 0;
}

// Total aanpassen
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
```

<h3 class="opdracht-titel">Opdracht</h3>

Schrijf integration tests waarbij `CartService`, `CouponService` en `DiscountCalculator` alle drie echt zijn. Mock niets van eigen code.

Test minstens:

| Scenario | Verwacht resultaat |
|---------|-------------------|
| Artikel 100.00, coupon ZOMER10 (10%), DiscountCalculator toegepast | 90.00 |
| Artikel 100.00, ongeldige coupon | 100.00 |
| Artikel 50.00, coupon WELKOM20 (20%) | 40.00 |
| Twee artikelen totaal 200.00, coupon TROUWE5 (5%) | 190.00 |

**Wat je controleert:** het eindtotaal is correct na de volledige keten van `CartService.ApplyCoupon` via `CouponService` via `DiscountCalculator.Apply`.

---

## Oefening 3: de volledige bestelflow

**Leerdoel:** je test een end-to-end scenario door alle eigen klassen samen te laten werken.

**Moeilijkheidsgraad:** gemiddeld

### Startcode

Je hebt de volgende klassen nodig uit les 1, les 3 en oefening 2:
- `CartService` (met `ICouponService` en `DiscountCalculator`)
- `CouponService` (implementeert `ICouponService`)
- `DiscountCalculator`
- `OrderService` (met `IPaymentGateway`, `IStockService`, `ICouponService`)

<h3 class="opdracht-titel">Opdracht</h3>

Schrijf integration tests voor de volledige flow: een klant vult een mandje, past een coupon toe en plaatst een bestelling via `OrderService`.

De flow:
1. Maak een `CartService` aan met een echte `CouponService` en `DiscountCalculator`
2. Voeg artikelen toe aan het mandje
3. Pas een coupon toe via `CartService.ApplyCoupon`
4. Haal het totaal op via `CartService.Total`
5. Roep `OrderService.PlaceOrder` aan met dat totaal

Test minstens:

| Scenario | Verwacht resultaat |
|---------|-------------------|
| Geldige coupon ZOMER10 (10%), basisprijs 100.00, betaling geslaagd | "Bestelling bevestigd", gateway ontvangt 90.00 |
| Ongeldige coupon, basisprijs 100.00, betaling geslaagd | "Bestelling bevestigd", gateway ontvangt 100.00 |
| Geen coupon, meerdere artikelen, betaling geslaagd | "Bestelling bevestigd", gateway ontvangt correct totaal |
| Product niet op voorraad | "Product niet beschikbaar" |

**Controleer via `Verify`** welk bedrag precies naar `ProcessPayment` gaat.

---

## Oefening 4: de callback-techniek

**Leerdoel:** je past de callback-techniek toe om een gegenereerde waarde op te vangen in een integration test.

**Moeilijkheidsgraad:** uitdaging

### Startcode

Voeg deze klasse toe aan `ShopWave`:

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

<h3 class="opdracht-titel">Opdracht</h3>

Schrijf integration tests die de samenwerking tussen `OrderConfirmationService` en zijn consumers testen. Gebruik de callback-techniek om de gegenereerde code op te vangen.

Test minstens:

| Scenario | Wat je verifieert |
|---------|------------------|
| Code genereren voor orderId 1 | code start met "ORD-", heeft het juiste formaat |
| Callback ontvangt exact dezelfde waarde als de returnwaarde | capturedCode == result |
| Gegenereerde code doorsturen naar `ValidateCode` | `ValidateCode` geeft true terug |
| Twee opeenvolgende codes zijn uniek | code1 != code2 |

**Verwachte structuur:**

```csharp
string capturedCode = string.Empty;

OrderConfirmationService service = new OrderConfirmationService(
    onConfirmationCodeGenerated: code => { capturedCode = code; });

string result = service.GenerateConfirmationCode(1);

// Gebruik capturedCode en result in je assertions
```

---

## Oefening 5: Reflectie

Beantwoord deze vragen voor jezelf voor je de oplossingen bekijkt.

1. In oefening 2 gebruik je drie echte klassen. Je hebt voor elk van die klassen ook unit tests geschreven in les 1 en les 3. Wat testen de integration tests in oefening 2 dat de unit tests niet testen?

2. Wanneer is het correct om een klasse te mocken in een integration test? Geef twee concrete voorbeelden uit de oefeningen.

3. In oefening 4 gebruik je een callback om de bevestigingscode op te vangen. Waarom is dit beter dan een publieke property `ConfirmationCode` toevoegen aan de klasse?

4. Je schrijft een integration test en hij faalt. De unit tests voor alle betrokken klassen slagen. Wat zijn mogelijke oorzaken van de fout?
