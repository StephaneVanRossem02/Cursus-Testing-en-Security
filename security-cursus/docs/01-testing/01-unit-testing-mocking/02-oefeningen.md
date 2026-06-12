---
title: "Les 1: Oefeningen - Unit Testing en Mocking"
sidebar_label: "Oefeningen"
---

# Oefeningen: Unit Testing en Mocking

:::note Code-afspraken voor alle oefeningen
- Geen top-level statements
- Altijd accolades `{}` bij `if`, `for`, `while`, ook als er maar één regel in staat
- Maximaal één `return` per methode
- Geen `break` of `continue`
- Geen underscore-prefix op parameters
- Geen geneste klassen
- Geen ternary (`? :`), geen null-conditional (`?.`), geen null-coalescing (`??`)
- Geen tuples
- Gebruik `double` in plaats van `decimal`
- Identifiers in het Engels, tekst in het Nederlands
:::

---

## Oefening 1: DiscountCalculator

**Leerdoel:** je schrijft tests met het AAA-patroon en past ZOMBIES toe om testgevallen te kiezen.

**Moeilijkheidsgraad:** laag

### Opgave

Maak een klasse `DiscountCalculator` in de namespace `ShopWave` met de volgende methode:

```
double ApplyDiscount(double originalPrice, int discountPercent)
```

De methode berekent de prijs na korting. Als `discountPercent` kleiner is dan 0 of groter dan 100, gooit de methode een `ArgumentException`.

### Stap 1: schrijf eerst de tests

Schrijf minstens zes tests in een testklasse `DiscountCalculatorTests`. Gebruik ZOMBIES als houvast:

| Geval | Wat test je? |
|-------|-------------|
| Zero | kortingspercentage is 0 |
| One | een normaal kortingspercentage, bv. 25% |
| Boundary | kortingspercentage is exact 100 |
| Exception | kortingspercentage is -1 |
| Exception | kortingspercentage is 101 |
| Many | meerdere combinaties via `[Theory]` en `[InlineData]` |

### Stap 2: schrijf de implementatie

Schrijf daarna de klasse `DiscountCalculator` zodat alle tests slagen.

:::tip Verwacht resultaat
Als je 10% korting geeft op een prijs van 80 euro, is het resultaat 72 euro. Als je 100% korting geeft, is het resultaat 0.
:::

:::warning Controleer dit
Een `ArgumentException` gooit je zo:

```csharp
throw new ArgumentException("Foutmelding.", nameof(discountPercent));
```

Test of de exception ook echt gegooid wordt:

```csharp
Action act = () => calculator.ApplyDiscount(100.0, -1);
act.Should().Throw<ArgumentException>();
```
:::

---

## Oefening 2: OrderService zonder stock

**Leerdoel:** je maakt een klasse testbaar via Dependency Injection en schrijft een mock met Moq.

**Moeilijkheidsgraad:** gemiddeld

### Opgave

ShopWave verwerkt bestellingen via een betaalgateway. Maak de volgende interface en klasse:

**Interface `IPaymentGateway`:**

```
bool ProcessPayment(double amount)
```

**Klasse `OrderService`:**

```
string PlaceOrder(double amount)
```

De methode:
- gooit een `ArgumentException` als `amount` kleiner is dan of gelijk aan 0
- roept `ProcessPayment` aan op de betaalgateway
- geeft `"Bestelling bevestigd"` terug als de betaling geslaagd is
- geeft `"Betaling mislukt"` terug als de betaling mislukt is

`OrderService` mag `IPaymentGateway` **niet** zelf aanmaken. De gateway wordt meegegeven via de constructor.

### Tests

Schrijf tests voor alle drie de scenario's: ongeldig bedrag, betaling geslaagd en betaling mislukt. Gebruik `Mock<IPaymentGateway>` en `Setup(...).Returns(...)`.

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

### Opgave

Voeg een tweede afhankelijkheid toe aan `OrderService`: een `IStockService`.

**Interface `IStockService`:**

```
bool IsInStock(int productId, int quantity)
```

Pas de signatuur van `PlaceOrder` aan:

```
string PlaceOrder(int productId, int quantity, double amount)
```

De aangepaste logica:
- als `amount` kleiner is dan of gelijk aan 0: gooi een `ArgumentException`, controleer de voorraad **niet**
- als het product niet op voorraad is: geef `"Product niet beschikbaar"` terug, roep de gateway **niet** aan
- als het product op voorraad is en de betaling geslaagd is: geef `"Bestelling bevestigd"` terug
- als het product op voorraad is en de betaling mislukt is: geef `"Betaling mislukt"` terug

### Tests

Schrijf minstens vier tests. Voeg bij twee tests ook een `Verify`-controle toe:

1. Als het product niet op voorraad is, mag `ProcessPayment` **nooit** aangeroepen worden.
2. Als het bedrag ongeldig is, mag `IsInStock` **nooit** aangeroepen worden.

:::warning Let op
Als je `Times.Never` wil gebruiken, mag je geen `Setup` vergeten die het gedrag van de mock instelt. Anders geeft Moq een onverwachte aanroep-fout.

Controleer ook: als het product op voorraad is en de betaling slaagt, moet `ProcessPayment` **precies eenmaal** aangeroepen zijn.
:::

---

## Oefening 4: CheckoutService

**Leerdoel:** je combineert meerdere klassen en mocks in een realistische berekening.

**Moeilijkheidsgraad:** hoog

### Opgave

Maak een klasse `CheckoutService` die het eindbedrag berekent inclusief verzendkosten.

**Interface `IShippingService`:**

```
double GetShippingCost(double totalAfterDiscount)
```

**Klasse `CheckoutService`:**

```
double CalculateFinalTotal(double unitPrice, int quantity, int discountPercent)
```

De methode:
1. berekent het subtotaal: `unitPrice * quantity`
2. past de korting toe via `DiscountCalculator`
3. haalt de verzendkost op via `IShippingService`
4. telt het resultaat van stap 2 en stap 3 bij elkaar op

`CheckoutService` ontvangt `IShippingService` via de constructor. `DiscountCalculator` mag je intern aanmaken (die heeft geen externe afhankelijkheid).

### Tests

Schrijf minstens twee tests:

1. Controleer het correcte eindbedrag bij een concrete combinatie van prijs, hoeveelheid en kortingspercentage.
2. Controleer via `Verify` dat `GetShippingCost` precies eenmaal aangeroepen wordt.

:::tip Voorbeeld om te controleren
Drie producten aan 10 euro per stuk, 0% korting, verzendkost 5 euro. Verwacht resultaat: 35 euro.
:::

---

## Zelfreflectie

Beantwoord deze vragen voor jezelf voor je de oplossingen bekijkt:

1. Wat zou er gebeuren als je `DiscountCalculator` ook via een interface injecteert in `CheckoutService`? Wat is het voordeel? Wat is het nadeel?
2. In oefening 3 test je dat `ProcessPayment` nooit aangeroepen wordt als het product niet op voorraad is. Waarom is dat een waardevolle test, ook al test je al dat het resultaat `"Product niet beschikbaar"` is?
3. Stel dat `IShippingService.GetShippingCost` een netwerkoproep doet naar een externe API. Wat zou er gebeuren met je tests als je geen mock gebruikt?
