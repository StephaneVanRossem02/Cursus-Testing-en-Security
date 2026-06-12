---
title: "Les 3: Test Driven Development"
sidebar_label: "TDD"
---

# Les 3: Test Driven Development

## ShopWave

In de vorige lessen schreven we eerst de klasse, dan pas de tests. In deze les draaien we dat volledig om. We schrijven eerst de test — ook al bestaat de klasse nog niet — en pas daarna de implementatie. Dit klinkt vreemd, maar het verandert fundamenteel hoe je over code nadenkt. Je bent niet langer bezig met "hoe bouw ik dit", maar met "wat moet dit doen".

---

## Theorie

### 1. Wat is Test Driven Development?

**Test Driven Development (TDD)** is een ontwikkelmethodiek waarbij je de test schrijft vóór de productiecode. De test beschrijft het gewenste gedrag, de implementatie zorgt ervoor dat de test slaagt.

TDD gaat in essentie niet over testen. Het gaat over **ontwerp en ontwikkeling**. De tests zijn een middel, niet het doel. Door eerst de test te schrijven, dwing je jezelf na te denken over de interface van je klasse — de methodenamen, de parameters, de return-waarden — voordat je ook maar één lijn implementatie hebt geschreven.

---

### 2. De drie wetten van TDD (Robert C. Martin — Uncle Bob)

Uncle Bob formuleerde drie strikte regels die samen het TDD-proces definiëren:

**Wet 1:** Het is niet toegestaan productiecode te schrijven, behalve om een falende test te doen slagen.

**Wet 2:** Het is niet toegestaan meer tests te schrijven dan nodig om de test te doen falen — ook compilatiefouten tellen als falen.

**Wet 3:** Het is niet toegestaan meer productiecode schrijven dan nodig om de falende test te doen slagen.

Deze drie wetten dwingen je in een strak ritme van kleine stappen. Elke stap duurt maximaal enkele minuten.

---

### 3. De Red-Green-Refactor cyclus

Het TDD-proces wordt samengevat als **Red-Green-Refactor**. Dit is een iteratieve cyclus die je steeds opnieuw doorloopt:

```
Red      →  Schrijf een test die faalt
             De productiecode bestaat nog niet of is onvolledig.
             De test beschrijft wat de code moet doen.

Green    →  Schrijf net genoeg productiecode om de test te doen slagen
             Zo eenvoudig mogelijk — nog geen optimalisaties.
             Weersta de verleiding om meteen alles te bouwen.

Refactor →  Verbeter de code zonder het gedrag te wijzigen
             Verwijder duplicatie, verbeter leesbaarheid, herstructureer.
             Alle tests moeten na de refactoring nog steeds slagen.
```

Daarna begin je opnieuw: schrijf de volgende test, die meteen rood is, en doorloop de cyclus opnieuw. Zo groeit de functionaliteit van je applicatie stap voor stap, altijd gedekt door tests.

**Belangrijke regel:** je mag pas een nieuwe test schrijven als **alle** bestaande tests groen zijn.

---

### 4. Waarom TDD?

#### Voordelen

- **Minder bugs** — je schrijft alleen code die nodig is voor een gespecificeerde vereiste. Er is geen ruimte voor code "die later misschien handig is".
- **Betere architectuur** — code die testbaar is, is automatisch beter gestructureerd. Klassen met te veel verantwoordelijkheden zijn moeilijk te testen — TDD dwingt je om ze op te splitsen.
- **Veilig refactoren** — de testbank vangt fouten op als je de code herwerkt. Je kan gerust iets aanpassen wetende dat de tests je waarschuwen als je iets breekt.
- **Levende documentatie** — de tests beschrijven exact wat de code doet, in begrijpbare taal.

#### Nadelen

- **Initieel trager** — het schrijven van tests kost tijd die je niet onmiddellijk terugverdient.
- **Moeilijker voor complexe integraties** — voor UI, databases en externe services is TDD lastiger toe te passen.
- **Onderhoud van tests** — als requirements wijzigen, moeten ook de tests worden aangepast.

In de praktijk wegen de voordelen zwaarder door bij projecten met een langere levensduur.

---

### 5. Het verschil met "tests achteraf schrijven"

| | Tests achteraf | TDD |
|--|---------------|-----|
| Wanneer schrijf je tests? | Na de implementatie | Vóór de implementatie |
| Wat drijft het ontwerp? | De code | De tests |
| Testdekking | Wat je niet vergeet | Alles wat je implementeert |
| Kans op blinde vlekken | Hoog — je kent de code al | Laag — je denkt vanuit vereisten |

Tests achteraf testen of de code werkt zoals je hem hebt geschreven. TDD-tests testen of de code werkt zoals het **moet** werken.

---

### 6. De testlijst: vooraf nadenken

Voordat je begint, noteer je de testgevallen die je verwacht nodig te hebben. Dit is geen volledige specificatie — het is een startpunt dat je helpt nadenken over de vereisten.

De lijst is geen contract. Tijdens het implementeren ontdek je altijd nieuwe gevallen die je toevoegt. Maar door vooraf even stil te staan bij wat er moet werken, voorkom je dat je halverwege vastzit of iets belangrijks over het hoofd ziet.

---

## Demo

We voegen een `CouponService` toe aan ShopWave. Die service valideert en verwerkt kortingscoupons die klanten kunnen gebruiken bij het afrekenen.

We weten vooraf nog niet hoe de klasse er precies uitzal uitzien. We laten de tests het ontwerp bepalen.

### Testlijst (vooraf opgesteld)

Voordat we ook maar één lijn code schrijven, noteren we welke gevallen we verwachten:

```
- Een geldige couponcode geeft true terug bij validatie
- Een onbekende couponcode geeft false terug
- De juiste kortingswaarde wordt teruggegeven voor een geldige code
- Een coupon die al gebruikt is, is niet meer geldig
- Een gebruikte coupon kan niet opnieuw worden gebruikt
```

Dit is ons plan van aanpak. We beginnen bij het eenvoudigste geval bovenaan.

---

### Stap 1 — Eerste test: bestaat de klasse?

**Waarom deze stap?**
In TDD begin je altijd met de eenvoudigst mogelijke test. Dit dwingt je om meteen een beslissing te nemen over de naam van de klasse en de naam van de methode. Je denkt vanuit de buitenkant: hoe wil ik deze klasse gebruiken? Niet: hoe ga ik hem bouwen?

Maak in `ShopWave.Tests` een nieuw bestand `CouponServiceTests.cs` aan:

```csharp
using FluentAssertions;
using ShopWave;

namespace ShopWave.Tests
{
    public class CouponServiceTests
    {
        [Fact]
        public void IsValid_WithValidCouponCode_ReturnsTrue()
        {
            // Arrange
            CouponService service = new CouponService();

            // Act
            bool result = service.IsValid("ZOMER10");

            // Assert
            result.Should().BeTrue();
        }
    }
}
```

De code compileert niet — `CouponService` bestaat nog niet. Dit is bewust: **de compilatiefout is onze rode fase**. We hebben een falende test.

---

### Stap 2 — Minimale implementatie om de test groen te krijgen

**Waarom zo weinig code?**
Wet 3 van Uncle Bob: schrijf niet meer dan nodig. Het is verleidelijk om meteen een volledige klasse te bouwen, maar dat is niet TDD. Door zo weinig mogelijk te schrijven, blijf je gefocust op wat de test vraagt. De volgende test zal je dwingen om meer toe te voegen.

Maak in `ShopWave` een nieuw bestand `CouponService.cs` aan:

```csharp
namespace ShopWave
{
    public class CouponService
    {
        public bool IsValid(string code)
        {
            return true;
        }
    }
}
```

Voer de test uit in Test Explorer — hij slaagt. **Groene fase.**

---

### Stap 3 — Tweede test: ongeldige coupon

**Waarom nu pas deze test?**
De vorige implementatie is bewust naïef: ze geeft altijd `true` terug. Dat is genoeg om de eerste test te laten slagen. Maar nu voegen we een test toe die onze naïeve implementatie onderuit haalt. Dit is het moment waarop de echte logica begint te groeien.

Voeg een tweede test toe in `CouponServiceTests.cs`:

```csharp
[Fact]
public void IsValid_WithUnknownCouponCode_ReturnsFalse()
{
    // Arrange
    CouponService service = new CouponService();

    // Act
    bool result = service.IsValid("BESTAANIET");

    // Assert
    result.Should().BeFalse();
}
```

Voer alle tests uit. De eerste slaagt, de tweede faalt. **Rode fase.** De huidige implementatie `return true` is niet langer voldoende.

Nu moeten we nadenken over hoe `IsValid` echt moet werken. We introduceren een lijst van geldige coupons:

```csharp
namespace ShopWave
{
    public class CouponService
    {
        private readonly List<string> _validCoupons;

        public CouponService()
        {
            _validCoupons = new List<string>
            {
                "ZOMER10",
                "WELKOM20",
                "TROUWE5"
            };
        }

        public bool IsValid(string code)
        {
            return _validCoupons.Contains(code);
        }
    }
}
```

Voer alle tests uit — beide slagen. **Groene fase.**

**Refactor?** De code is eenvoudig en leesbaar. Geen refactoring nodig. We gaan door naar de volgende test.

---

### Stap 4 — Derde test: kortingswaarde ophalen

**Waarom een aparte methode voor de kortingswaarde?**
`IsValid` vertelt ons of een coupon bestaat. Maar om de korting toe te passen, hebben we ook de waarde nodig. We introduceren een tweede methode `GetDiscount`. Door dit als aparte test en aparte methode te behandelen, houden we de verantwoordelijkheden klein en duidelijk.

Voeg een derde test toe:

```csharp
[Fact]
public void GetDiscount_WithValidCouponCode_ReturnsCorrectDiscount()
{
    // Arrange
    CouponService service = new CouponService();

    // Act
    int discount = service.GetDiscount("ZOMER10");

    // Assert
    discount.Should().Be(10);
}
```

**Rode fase** — `GetDiscount` bestaat nog niet.

Nu merken we iets: een coupon is meer dan een string. Hij heeft ook een kortingswaarde. Een `string`-lijst volstaat niet meer. We introduceren een `Coupon`-klasse.

**Dit is een belangrijke TDD-les:** de test heeft ons naar een beter ontwerp geduwd. We hadden dit niet ontdekt als we de klasse van bovenaf hadden ontworpen.

Maak `Coupon.cs` aan in `ShopWave`:

```csharp
namespace ShopWave
{
    public class Coupon
    {
        public string Code            { get; set; }
        public int    DiscountPercent { get; set; }
        public bool   IsUsed          { get; set; }

        public Coupon(string code, int discountPercent)
        {
            Code            = code;
            DiscountPercent = discountPercent;
            IsUsed          = false;
        }
    }
}
```

Pas `CouponService.cs` aan:

```csharp
namespace ShopWave
{
    public class CouponService
    {
        private readonly List<Coupon> _coupons;

        public CouponService()
        {
            _coupons = new List<Coupon>
            {
                new Coupon("ZOMER10",  10),
                new Coupon("WELKOM20", 20),
                new Coupon("TROUWE5",   5)
            };
        }

        public bool IsValid(string code)
        {
            Coupon coupon = _coupons.Find(c => c.Code == code);
            bool isValid  = false;

            if (coupon != null)
            {
                isValid = true;
            }

            return isValid;
        }

        public int GetDiscount(string code)
        {
            Coupon coupon = _coupons.Find(c => c.Code == code);
            int discount  = 0;

            if (coupon != null)
            {
                discount = coupon.DiscountPercent;
            }

            return discount;
        }
    }
}
```

Alle drie de tests slagen. **Groene fase.**

**Refactor:** `IsValid` kan beknopter:

```csharp
public bool IsValid(string code)
{
    Coupon coupon = _coupons.Find(c => c.Code == code);
    return coupon != null;
}
```

Voer de tests opnieuw uit — nog steeds groen. Refactoring geslaagd.

---

### Stap 5 — Vierde en vijfde test: coupon mag maar één keer gebruikt worden

**Waarom twee tests voor dezelfde feature?**
We testen twee kanten van hetzelfde gedrag: enerzijds dat een gebruikte coupon ongeldig wordt, anderzijds dat een coupon die nog niet gebruikt is gewoon geldig blijft. Eén test is niet genoeg om het gedrag volledig vast te leggen.

Voeg twee tests toe:

```csharp
[Fact]
public void IsValid_AfterCouponIsUsed_ReturnsFalse()
{
    // Arrange
    CouponService service = new CouponService();
    service.MarkAsUsed("ZOMER10");

    // Act
    bool result = service.IsValid("ZOMER10");

    // Assert
    result.Should().BeFalse();
}

[Fact]
public void IsValid_BeforeCouponIsUsed_ReturnsTrue()
{
    // Arrange
    CouponService service = new CouponService();

    // Act
    bool result = service.IsValid("ZOMER10");

    // Assert
    result.Should().BeTrue();
}
```

**Rode fase** — `MarkAsUsed` bestaat nog niet.

Voeg `MarkAsUsed` toe en pas `IsValid` aan zodat het de `IsUsed`-vlag controleert:

```csharp
public void MarkAsUsed(string code)
{
    Coupon coupon = _coupons.Find(c => c.Code == code);

    if (coupon != null)
    {
        coupon.IsUsed = true;
    }
}

public bool IsValid(string code)
{
    Coupon coupon = _coupons.Find(c => c.Code == code);
    return coupon != null && !coupon.IsUsed;
}
```

Alle vijf de tests slagen. **Groene fase.**

Controleer de testlijst: alle gevallen zijn gedekt. De `CouponService` is volledig via TDD gebouwd — stap voor stap, test voor test, zonder ook maar één lijn productiecode te schrijven voordat er een falende test was.

---

## Oefeningen

Werk de oefeningen in volgorde. Schrijf bij elke oefening **eerst** de testlijst, dan de eerste test, dan de minimale implementatie, en refactor waar nodig. Volg de Red-Green-Refactor cyclus consequent.

---

### Oefening 1 — CartService: winkelmandje via TDD

ShopWave heeft een winkelmandje nodig. Bouw een `CartService` klasse volledig via TDD in het bestaande `ShopWave` project.

**Stel eerst je testlijst op.** Denk na over alle gevallen voordat je begint te coderen.

De `CartService` moet het volgende ondersteunen:

- Een nieuw aangemaakt mandje heeft een totaal van 0
- Een artikel toevoegen verhoogt het totaal met de prijs van dat artikel
- Meerdere artikelen optellen geeft het correcte totaal
- Een artikel verwijderen verlaagt het totaal
- Het aantal stuks van een artikel instellen beïnvloedt het totaal correct
- Een negatief aantal stuks is niet toegestaan en gooit een `ArgumentException`
- Het mandje leegmaken reset het totaal naar 0

Schrijf voor elk van deze gevallen een test en implementeer stap voor stap.

---

### Oefening 2 — CartService uitbreiden: coupon toepassen

Breid de `CartService` uit met de mogelijkheid om een coupon toe te passen. De `CartService` gebruikt hiervoor de `CouponService` uit de demo.

> ⚠️ **Moq werkt alleen met interfaces (of abstracte klassen).** `CouponService` is momenteel een concrete klasse — je kan die niet rechtstreeks mocken. Extraheer eerst een interface:
>
> ```csharp
> public interface ICouponService
> {
>     bool IsValid(string code);
>     int  GetDiscount(string code);
>     void MarkAsUsed(string code);
> }
> ```
>
> Laat `CouponService` deze interface implementeren: `public class CouponService : ICouponService`. Gebruik daarna `ICouponService` als het type van de constructor-parameter.

Pas Dependency Injection toe: geef een `ICouponService` mee via de constructor van `CartService`, net zoals we in Les 1 de `IPaymentGateway` meegaven aan de `OrderService`.

Vereisten:
- Een geldige coupon verlaagt het totaal met het opgegeven percentage
- Een ongeldige coupon heeft geen effect op het totaal
- Na het toepassen van een coupon wordt die als gebruikt gemarkeerd
- Dezelfde coupon kan niet tweemaal worden toegepast

Schrijf voor elk geval eerst een test. Gebruik Moq om `ICouponService` te mocken in je tests.

---

### Oefening 3 — OrderService uitbreiden via TDD

De `OrderService` uit Les 1 plaatst bestellingen en roept daarvoor de betaalgateway aan. We willen er nu ook kortingscoupons in verwerken.

Voeg via TDD de volgende functionaliteit toe aan `OrderService`:

- Als een geldige coupon meegegeven wordt, wordt het te betalen bedrag verlaagd voor de betaling
- Als een ongeldige coupon meegegeven wordt, wordt de bestelling toch geplaatst maar zonder korting
- Als een al gebruikte coupon meegegeven wordt, wordt de bestelling geweigerd met de melding `"Coupon reeds gebruikt."`

De signatuur van `PlaceOrder` wordt uitgebreid:

```csharp
public string PlaceOrder(int productId, int quantity, double amount, string couponCode = "")
```

Schrijf minstens vijf tests. Gebruik Moq voor zowel `IPaymentGateway`, `IStockService` als `ICouponService`.

---

### Oefening 4 — Reflectie

Beantwoord de volgende vragen in enkele zinnen:

1. In Stap 2 van de demo schreven we `return true` als volledige implementatie. Waarom is dat correct in TDD, ook al weet je dat het onvolledig is?
2. In Stap 4 dwong de test ons om van een `List<string>` naar een `List<Coupon>` over te stappen. Hoe noemen we dit principe — dat tests het ontwerp sturen?
3. Wat is het verschil tussen de refactorfase en de groene fase? Waarom mag je tijdens de groene fase geen refactoring doen?
4. Beschrijf een situatie in de oefeningen waarbij je tijdens het schrijven van een test iets ontdekte dat je niet had verwacht.

---

## Modeloplossing

> De modeloplossing is beschikbaar na het indienen van de labo-opdracht via Digitap.

---

### Modeloplossing Oefening 1 — CartService

**CartService.cs**

```csharp
namespace ShopWave
{
    public class CartService
    {
        private readonly Dictionary<string, (double Price, int Quantity)> _items;

        public CartService()
        {
            _items = new Dictionary<string, (double Price, int Quantity)>();
        }

        public double Total
        {
            get
            {
                double total = 0;

                foreach ((double price, int quantity) in _items.Values)
                {
                    total += price * quantity;
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
                (double existingPrice, int existingQuantity) = _items[name];
                _items[name] = (existingPrice, existingQuantity + quantity);
            }
            else
            {
                _items[name] = (price, quantity);
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

**CartServiceTests.cs**

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
            // Arrange
            CartService cart = new CartService();

            // Act
            double result = cart.Total;

            // Assert
            result.Should().Be(0.0);
        }

        [Fact]
        public void AddItem_SingleItem_UpdatesTotal()
        {
            // Arrange
            CartService cart = new CartService();

            // Act
            cart.AddItem("Laptop", 999.99);

            // Assert
            cart.Total.Should().Be(999.99);
        }

        [Fact]
        public void AddItem_MultipleItems_ReturnsCombinedTotal()
        {
            // Arrange
            CartService cart = new CartService();

            // Act
            cart.AddItem("Laptop",  999.99);
            cart.AddItem("Muis",     29.99);

            // Assert
            cart.Total.Should().Be(1029.98);
        }

        [Fact]
        public void AddItem_WithQuantity_MultipliesPriceByQuantity()
        {
            // Arrange
            CartService cart = new CartService();

            // Act
            cart.AddItem("Pen", 2.50, 4);

            // Assert
            cart.Total.Should().Be(10.00);
        }

        [Fact]
        public void RemoveItem_ExistingItem_UpdatesTotal()
        {
            // Arrange
            CartService cart = new CartService();
            cart.AddItem("Laptop", 999.99);
            cart.AddItem("Muis",    29.99);

            // Act
            cart.RemoveItem("Muis");

            // Assert
            cart.Total.Should().Be(999.99);
        }

        [Fact]
        public void AddItem_WithNegativeQuantity_ThrowsArgumentException()
        {
            // Arrange
            CartService cart = new CartService();

            // Act
            Action act = () => cart.AddItem("Laptop", 999.99, -1);

            // Assert
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void Clear_NonEmptyCart_ResetsTotal()
        {
            // Arrange
            CartService cart = new CartService();
            cart.AddItem("Laptop", 999.99);

            // Act
            cart.Clear();

            // Assert
            cart.Total.Should().Be(0.0);
        }
    }
}
```

---

## Samenvatting

| Concept | Wat je moet onthouden |
|---------|-----------------------|
| TDD | Test eerst schrijven, dan pas de implementatie |
| Red | Schrijf een test die faalt — de implementatie bestaat nog niet |
| Green | Schrijf de minimale code om de test te doen slagen — niet meer |
| Refactor | Verbeter de code zonder het gedrag te wijzigen — alle tests blijven groen |
| Wet 1 | Geen productiecode zonder falende test |
| Wet 2 | Niet meer tests schrijven dan nodig om te falen |
| Wet 3 | Niet meer productiecode schrijven dan nodig om de test te doen slagen |
| Testlijst | Noteer vooraf welke gevallen je verwacht — dit is je plan van aanpak |
| Ontwerp via tests | Tests sturen het ontwerp — klassen en methoden ontstaan door wat de test nodig heeft |

---

## Volgende les

In Les 4 gaan we dieper in op **Security: Two-Factor Authentication en digitale handtekeningen**. We voegen een tweede verificatielaag toe aan de ShopWave-loginflow en leren hoe digitale handtekeningen de integriteit van berichten garanderen.