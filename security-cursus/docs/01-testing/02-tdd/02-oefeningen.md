---
title: "Les 3: Oefeningen - Test Driven Development"
sidebar_label: "Oefeningen"
---

# Oefeningen: Test Driven Development

Werk de oefeningen in volgorde. Schrijf bij elke oefening **eerst** de testlijst, dan de eerste test, dan de minimale implementatie, en refactor waar nodig. Volg de Red-Green-Refactor cyclus consequent.

---

## Oefening 1: CartService via TDD

**Leerdoel:** je bouwt een klasse volledig via TDD door de Red-Green-Refactor cyclus stap voor stap te doorlopen.

**Moeilijkheidsgraad:** basis

### Startcode

Maak deze klasse aan in `ShopWave`. Je hoeft hem niet aan te passen, maar je hebt hem nodig als bouwsteen voor `CartService`.

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

### Stap 1: stel je testlijst op

Noteer alle testgevallen die je verwacht nodig te hebben voor `CartService`. Schrijf ze op voor je ook maar één lijn code aanraakt. De lijst hieronder is een vertrekpunt, maar je mag gevallen toevoegen als je ze ontdekt tijdens het implementeren.

```
[ ] Nieuw aangemaakt mandje heeft een totaal van 0
[ ] Eén artikel toevoegen verhoogt het totaal met de prijs van dat artikel
[ ] Meerdere artikelen optellen geeft het correcte totaal
[ ] Hoeveelheid meegeven vermenigvuldigt de prijs correct
[ ] Een artikel verwijderen verlaagt het totaal
[ ] Een negatieve hoeveelheid geeft een ArgumentException
[ ] Het mandje leegmaken reset het totaal naar 0
```

### Stap 2: bouw CartService via TDD

Maak een lege `CartService`-klasse aan in `ShopWave` en een testklasse `CartServiceTests` in `ShopWave.Tests`. Werk de testlijst van boven naar onder af. Doorloop voor elk geval de volledige Red-Green-Refactor cyclus voor je naar het volgende gaat.

**Verwacht resultaat:**

| Actie | Verwacht totaal |
|-------|----------------|
| Leeg mandje | 0 |
| Laptop (999.99) toevoegen | 999.99 |
| Laptop + Muis (29.99) | 1029.98 |
| 4 pennen aan 2.50 | 10.00 |
| Laptop + Muis, dan Muis verwijderen | 999.99 |
| Laptop toevoegen, dan Clear() | 0 |

**Negatieve hoeveelheid testen** doe je zo:

```csharp
Action act = () => cart.AddItem("Laptop", 999.99, -1);
act.Should().Throw<ArgumentException>();
```

---

## Oefening 2: CartService uitbreiden met couponondersteuning

**Leerdoel:** je combineert TDD met Dependency Injection en Moq om een uitbreiding te schrijven op een bestaande klasse.

**Moeilijkheidsgraad:** gemiddeld

### Startcode

Moq werkt alleen met interfaces. Maak eerst deze interface aan in `ShopWave`:

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

Laat `CouponService` uit de demo deze interface implementeren:

```csharp
public class CouponService : ICouponService
```

Voeg daarna een constructor toe aan `CartService` die een `ICouponService` ontvangt, net zoals `OrderService` in Les 1 een `IPaymentGateway` ontving.

<h3 class="opdracht-titel">Opdracht</h3>

Breid `CartService` uit met een methode `ApplyCoupon(string code)`. Schrijf eerst de testlijst, dan de tests, dan de implementatie.

Vereisten:

- Een geldige coupon verlaagt het totaal met het opgegeven percentage
- Een ongeldige coupon heeft geen effect op het totaal
- Na het toepassen van een coupon wordt die als gebruikt gemarkeerd via `MarkAsUsed`
- Dezelfde coupon kan niet tweemaal worden toegepast

**Structuur van een test met twee mocks:**

```csharp
Mock<ICouponService> mockCoupon = new Mock<ICouponService>();
mockCoupon.Setup(c => c.IsValid("ZOMER10")).Returns(true);
mockCoupon.Setup(c => c.GetDiscount("ZOMER10")).Returns(10);

CartService cart = new CartService(mockCoupon.Object);
cart.AddItem("Laptop", 100.0);
cart.ApplyCoupon("ZOMER10");

cart.Total.Should().Be(90.0);
```

**Verwacht resultaat:**

| Situatie | Verwacht totaal bij basisprijs 100.00 |
|---------|--------------------------------------|
| Geen coupon | 100.00 |
| Geldige coupon van 10% | 90.00 |
| Ongeldige coupon | 100.00 |

Controleer ook via `Verify` dat `MarkAsUsed` precies eenmaal aangeroepen wordt bij een geldige coupon, en nooit bij een ongeldige.

---

## Oefening 3: OrderService uitbreiden via TDD

**Leerdoel:** je past TDD toe op een bestaande klasse en combineert meerdere mocks in één test.

**Moeilijkheidsgraad:** uitdaging

### Startcode

De `OrderService` uit Les 1 krijgt een extra parameter:

```csharp
public string PlaceOrder(int productId, int quantity, double amount, string couponCode = "")
```

`OrderService` ontvangt nu ook een `ICouponService` via de constructor, naast `IPaymentGateway` en `IStockService`.

### Opdracht

<h3 class="opdracht-titel">Opdracht</h3>

Stel eerst je testlijst op. Bouw daarna de uitbreiding via TDD.

Vereisten:

- Als geen coupon meegegeven wordt, verloopt de bestelling zoals in Les 1
- Een geldige coupon verlaagt het te betalen bedrag voor de betaling plaatsvindt
- Een ongeldige coupon heeft geen effect: de bestelling wordt toch geplaatst zonder korting
- Een al gebruikte coupon geeft de melding `"Coupon reeds gebruikt."` terug en plaatst geen bestelling

Schrijf minstens vijf tests. Gebruik Moq voor `IPaymentGateway`, `IStockService` en `ICouponService`.

**Verwacht resultaat per scenario:**

| Situatie | Verwacht resultaat |
|---------|-------------------|
| Geen coupon, betaling geslaagd | "Bestelling bevestigd" |
| Geldige coupon, betaling geslaagd | "Bestelling bevestigd" (lager bedrag doorgestuurd naar gateway) |
| Ongeldige coupon, betaling geslaagd | "Bestelling bevestigd" (origineel bedrag) |
| Al gebruikte coupon | "Coupon reeds gebruikt." |
| Product niet op voorraad | "Product niet beschikbaar" |

---

## Oefening 4: Reflectie

Beantwoord deze vragen voor jezelf voor je de oplossingen bekijkt.

1. In stap 2 van de demo schreven we `return true` als volledige implementatie. Waarom is dat correct in TDD, ook al weet je dat het onvolledig is?
2. In stap 4 dwong de test ons om van een `List<string>` naar een `List<Coupon>` over te stappen. Wat zegt dit over hoe TDD het ontwerp beinvloedt?
3. Wat is het verschil tussen de refactorfase en de groene fase? Waarom mag je tijdens de groene fase niet refactoren?
4. Beschrijf een moment tijdens het uitwerken van de oefeningen waarbij een test je dwong iets te ontdekken wat je niet had verwacht.
