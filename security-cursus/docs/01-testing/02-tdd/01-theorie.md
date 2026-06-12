---
title: "Les 3: Theorie - Test Driven Development"
sidebar_label: "Theorie"
---

# Theorie: Test Driven Development

## 1. Waarom TDD?

In Les 1 schreven we eerst de klasse en daarna de tests. Dat klinkt logisch, maar het heeft een verborgen probleem.

Als je de implementatie al kent, schrijf je tests die bevestigen wat je hebt gebouwd. Je denkt onbewust vanuit de code die al bestaat. Blinde vlekken in de code worden blinde vlekken in de tests. De tests testen of de code werkt zoals je hem hebt geschreven, niet of hij werkt zoals hij moet werken.

Er is nog een tweede probleem. Code die "achteraf" getest wordt, is vaak moeilijk testbaar. Klassen hebben te veel verantwoordelijkheden. Methoden zijn te lang. Afhankelijkheden zijn direct aangemaakt in plaats van geïnjecteerd. De code werkt, maar hij is moeilijk te isoleren voor een test.

TDD lost beide problemen op. Je schrijft de test eerst, nog voor er ook maar één lijn implementatie bestaat. De test dwingt je na te denken over de interface van je klasse: welke methoden heeft die nodig, welke parameters verwacht hij, wat geeft hij terug? Je denkt vanuit de buitenkant, vanuit het gebruik, niet vanuit de interne structuur.

**Waarom TDD leren?** Op de werkvloer kom je TDD tegen in teams die werken met agile methodieken. Veel bedrijven verwachten dat nieuwe features gedekt zijn door tests voor ze gemerged worden. Als je TDD begrijpt, schrijf je van nature testbare code, ook als je niet strikt TDD toepast.

---

## 2. Wat is TDD?

**Test Driven Development (TDD)** is een ontwikkelmethodiek waarbij je de test schrijft vóór de productiecode.

Maar TDD gaat niet in de eerste plaats over testen. Het gaat over **ontwerp**. De tests zijn een middel, niet het doel. Door eerst de test te schrijven, denk je na over wat de code moet doen voordat je nadenkt over hoe hij het doet.

Dat klinkt als een klein verschil, maar het verandert fundamenteel hoe je code schrijft. Code die ontworpen is via TDD is automatisch losjes gekoppeld, heeft duidelijke interfaces en is testbaar. Niet omdat je er bewust voor koos, maar omdat de tests je daartoe dwongen.

**Mini-controle:** je schrijft de test voor de implementatie. De test verwijst naar een klasse die nog niet bestaat. Wat is dan de status van de test? De test kan niet compileren. Dat is de rode fase. Een compilatiefout telt in TDD als een falende test.

---

## 3. De drie regels van Uncle Bob

Robert C. Martin, beter bekend als Uncle Bob, formuleerde drie strikte regels die het TDD-proces definiëren.

**Regel 1:** je mag geen productiecode schrijven, behalve om een falende test te doen slagen.

**Regel 2:** je mag niet meer tests schrijven dan nodig om de test te doen falen. Een compilatiefout telt als falen.

**Regel 3:** je mag niet meer productiecode schrijven dan nodig om de falende test te doen slagen.

Deze drie regels dwingen je in een strak ritme van kleine stappen. Elke stap duurt maximaal enkele minuten. Je schrijft nooit grote stukken code in één keer.

Wet 3 heeft een belangrijk gevolg dat studenten moeilijk vinden: als de eenvoudigst mogelijke implementatie `return true` is, dan schrijf je `return true`. Niet meer. De volgende test zal je dwingen om verder te gaan.

**Mini-controle:** je hebt één test die controleert of een methode `true` teruggeeft voor een geldige invoer. Je schrijft `return true` als implementatie. De test slaagt. Heb je de wet overtreden? Nee. Je hebt precies genoeg code geschreven om de test te doen slagen. De volgende test zal je implementatie uitdagen.

---

## 4. De Red-Green-Refactor cyclus

Het TDD-proces wordt samengevat als **Red-Green-Refactor**. Dit is een iteratieve cyclus die je telkens opnieuw doorloopt.

```
Red      Schrijf een test die faalt.
         De productiecode bestaat nog niet of is onvolledig.
         De test beschrijft wat de code moet doen.

Green    Schrijf net genoeg productiecode om de test te doen slagen.
         Zo eenvoudig mogelijk, geen optimalisaties.
         Weersta de verleiding om meteen alles te bouwen.

Refactor Verbeter de code zonder het gedrag te wijzigen.
         Verwijder duplicatie, verbeter leesbaarheid, herstructureer.
         Alle tests moeten na de refactoring nog steeds slagen.
```

Daarna begin je opnieuw: schrijf de volgende test (die meteen rood is) en doorloop de cyclus opnieuw. Zo groeit de functionaliteit stap voor stap, altijd gedekt door tests.

**Belangrijke regel:** je mag pas een nieuwe test schrijven als alle bestaande tests groen zijn.

**Waarom niet refactoren tijdens de groene fase?** In de groene fase is je enige doel de test laten slagen. Refactoring en implementatie tegelijk doen is twee dingen tegelijk doen. Je verliest het overzicht. Houd de fasen strikt gescheiden.

**Mini-controle:** je bent in de refactorfase. Je verbetert de structuur van een methode. Daarna voer je alle tests uit. Twee tests falen. Wat doe je? Je maakt de refactoring ongedaan totdat alle tests weer groen zijn. Dan herbegin je de refactoring in kleinere stappen.

---

## 5. De testlijst: vooraf nadenken

Voordat je begint, noteer je de testgevallen die je verwacht nodig te hebben. Dit is geen volledige specificatie en ook geen contract. Het is een startpunt dat je helpt nadenken over de vereisten.

Je hoeft de testlijst niet volledig te maken voor je begint. Voeg gevallen toe naarmate je implementeert en nieuwe scenario's ontdekt. Maar door even stil te staan bij wat er moet werken, voorkom je dat je halverwege vastzit.

Voorbeeld voor een `CartService`:

```
[ ] Nieuw mandje heeft totaal 0
[ ] Eén artikel toevoegen verhoogt het totaal
[ ] Meerdere artikelen tellen correct op
[ ] Artikel verwijderen verlaagt het totaal
[ ] Negatief aantal artikelen geeft een ArgumentException
[ ] Mandje leegmaken reset het totaal naar 0
```

Je werkt de lijst van boven naar onder: begin bij het eenvoudigste geval. Een leeg mandje met totaal 0 is eenvoudiger dan het verwijderen van een artikel.

**Mini-controle:** waarom begin je met het eenvoudigste geval? Omdat je dan de minimale klasse-structuur opzet. Elke volgende test bouwt voort op wat al bestaat. Als je begint met een complex geval, heb je te veel dingen tegelijk te implementeren.

---

## 6. TDD versus tests achteraf schrijven

| Aspect | TDD | Tests achteraf |
|--------|-----|---------------|
| Wanneer schrijf je tests? | Voor de implementatie | Na de implementatie |
| Wat drijft het ontwerp? | De tests | De code |
| Testdekking | Hoog: je schrijft alleen wat getest is | Lager: blinde vlekken zijn mogelijk |
| Kans op niet-testbare code | Klein: de test dwingt goede structuur | Groot: je past de code niet meer aan |
| Refactoring | Veilig door de testbank | Risicovol zonder testdekking |

Tests achteraf testen of de code werkt zoals je hem hebt geschreven. TDD-tests testen of de code werkt zoals hij moet werken.

---

## 7. Demo: CouponService via TDD

We voegen een `CouponService` toe aan ShopWave. Die service valideert kortingscoupons die klanten kunnen gebruiken bij het afrekenen.

We weten vooraf nog niet hoe de klasse er precies uitzal uitzien. We laten de tests het ontwerp bepalen.

### Testlijst (vooraf opgesteld)

```
[ ] Een geldige couponcode geeft true terug bij validatie
[ ] Een onbekende couponcode geeft false terug
[ ] De juiste kortingswaarde wordt teruggegeven voor een geldige code
[ ] Een coupon die al gebruikt is, is niet meer geldig
[ ] Een coupon die nog niet gebruikt is, blijft geldig
```

### Stap 1: eerste test

In TDD begin je altijd met de eenvoudigst mogelijke test. Dit dwingt je om meteen een beslissing te nemen over de naam van de klasse en de naam van de methode. Je denkt vanuit de buitenkant: hoe wil ik deze klasse gebruiken?

Maak in `ShopWave.Tests` een bestand `CouponServiceTests.cs` aan:

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

De code compileert niet. `CouponService` bestaat nog niet. Dit is bewust: de compilatiefout is onze rode fase.

### Stap 2: minimale implementatie

Wet 3 van Uncle Bob: schrijf niet meer dan nodig. Maak in `ShopWave` een bestand `CouponService.cs` aan:

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

Voer de test uit. Hij slaagt. Groene fase.

### Stap 3: tweede test daagt de implementatie uit

`return true` is bewust naïef. Nu voegen we een test toe die onze implementatie onderuit haalt:

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

Voer alle tests uit. De eerste slaagt, de tweede faalt. Rode fase. We moeten nu nadenken over hoe `IsValid` echt werkt. We introduceren een lijst van geldige coupons:

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

Beide tests slagen. Groene fase. De code is eenvoudig en leesbaar, geen refactoring nodig.

### Stap 4: kortingswaarde ophalen

`IsValid` vertelt of een coupon bestaat. Maar om de korting toe te passen, hebben we ook de waarde nodig. We introduceren een methode `GetDiscount`:

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

Rode fase: `GetDiscount` bestaat nog niet.

Nu merken we iets: een coupon is meer dan een string. Hij heeft ook een kortingswaarde. Een `List<string>` volstaat niet meer. We introduceren een `Coupon`-klasse.

**Dit is een belangrijke les:** de test heeft ons naar een beter ontwerp geduwd. We hadden dit niet ontdekt als we de klasse van bovenaf hadden ontworpen.

Maak `Coupon.cs` aan:

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
            return coupon != null;
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

Alle drie de tests slagen. Groene fase.

### Stap 5: coupon mag maar eenmaal gebruikt worden

We testen twee kanten van hetzelfde gedrag: een gebruikte coupon wordt ongeldig, een niet-gebruikte blijft geldig. Eén test is niet genoeg om het gedrag volledig vast te leggen.

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

Rode fase: `MarkAsUsed` bestaat nog niet. Voeg de methode toe en pas `IsValid` aan:

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

Alle vijf de tests slagen. Groene fase.

Controleer de testlijst: alle gevallen zijn gedekt. `CouponService` is volledig via TDD gebouwd, stap voor stap, test voor test, zonder ook maar één lijn productiecode voor er een falende test was.

---

## 8. TDD en security: de link

TDD dwingt je na te denken over foutgevallen. Wat als de invoer ongeldig is? Wat als een waarde buiten de verwachte range valt? Dat zijn precies de vragen die ook bij security relevant zijn.

Een methode die wachtwoorden valideert, versleutelde tokens verifieert of toegangsrechten controleert: in al die gevallen schrijf je in TDD eerst de test voor het foutgeval. "Wat verwacht ik als het wachtwoord leeg is? Wat als het token verlopen is?" Die vragen stellen je in staat beveiligingsproblemen op te vangen voor ze in de code belanden.

In les 5 (Integration Testing) zullen we zien hoe je dit principe uitbreidt naar de volledige applicatielaag.

---

## 9. Samenvatting

| Concept | Wat je moet onthouden |
|--------|-----------------------|
| TDD | Test eerst schrijven, dan pas de implementatie |
| Ontwerp via tests | Tests sturen het ontwerp, klassen en methoden ontstaan door wat de test nodig heeft |
| Red | Schrijf een test die faalt, de implementatie bestaat nog niet |
| Green | Schrijf de minimale code om de test te doen slagen, niet meer |
| Refactor | Verbeter de code zonder het gedrag te wijzigen, alle tests blijven groen |
| Regel 1 | Geen productiecode zonder falende test |
| Regel 2 | Niet meer tests schrijven dan nodig om te falen |
| Regel 3 | Niet meer productiecode schrijven dan nodig om de test te doen slagen |
| Testlijst | Noteer vooraf welke gevallen je verwacht, dit is je plan van aanpak |
