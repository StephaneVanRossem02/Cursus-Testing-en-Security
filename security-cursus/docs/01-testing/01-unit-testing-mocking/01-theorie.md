---
title: "Les 1: Theorie - Unit Testing en Mocking"
sidebar_label: "Theorie"
---

# Theorie: Unit Testing en Mocking

## 1. Wat gaat er mis zonder tests?

Voordat je leert hoe je tests schrijft, is het belangrijk om te begrijpen waarom je ze schrijft.

In juli 2012 verloor het Amerikaanse handelsbedrijf Knight Capital Group 440 miljoen dollar in 45 minuten. De oorzaak was een softwarefout in een geautomatiseerd handelssysteem. De code was niet grondig getest voor de uitrol. Het bedrijf ging daarna failliet.

Je hoeft niet op die schaal te werken om hetzelfde patroon te zien. Een webshop die per ongeluk een negatieve prijs toepast, een bestelformulier dat dubbele bestellingen plaatst, een kortingsberekening die afrondt op de verkeerde manier: elk van die fouten kost geld, kost klanten en kost reputatie.

De kernvraag is: op welk moment vind je een bug het liefst?

| Moment | Wie vindt de bug | Kostprijs |
|--------|-----------------|-----------|
| Terwijl je programmeert | Jijzelf | Minuten |
| In een code review | Een collega | Uren |
| Tijdens testen voor release | Een tester | Dagen |
| Na de release | Een klant | Weken en reputatieschade |

Automatische tests verschuiven het moment van ontdekking naar links. Hoe vroeger je een bug vindt, hoe goedkoper die is om te repareren.

**Link met security:** hetzelfde principe geldt voor beveiligingsfouten. Een SQL injection die je zelf ontdekt terwijl je aan het programmeren bent, is een leermoment. Dezelfde SQL injection die een aanvaller ontdekt na de release, is een datalek. In les 2 en verder leer je hoe je ook beveiligingsproblemen automatisch kunt opsporen met tests.

---

## 2. Wat is een unit test?

Een **unit test** is een stuk code dat één methode of klasse controleert, volledig losgemaakt van de rest van de applicatie. Met "losgemaakt" bedoelen we: geen database, geen netwerkverbinding, geen bestanden op schijf. Alleen de logica van de methode zelf wordt gecontroleerd.

Een goede unit test heeft vijf eigenschappen.

| Eigenschap | Uitleg |
|-----------|--------|
| Snel | De test wordt uitgevoerd in milliseconden. Honderden tests samen duren maximaal enkele seconden. |
| Geïsoleerd | De test is onafhankelijk van andere tests en van externe systemen. |
| Herhaalbaar | De test geeft altijd hetzelfde resultaat, ongeacht het moment of de machine waarop hij draait. |
| Eenduidig bij falen | Als de test faalt, weet je meteen welk stukje code het probleem veroorzaakt. |
| Actueel | De test blijft relevant zolang de bijhorende code bestaat. |

Een bijkomend voordeel: een goed geschreven test werkt als **levende documentatie**. Een nieuwe collega die de testklasse leest, begrijpt meteen wat de klasse doet en welke randgevallen er bestaan. In tegenstelling tot een Word-document raakt een test nooit verouderd: als de code verandert en de test klopt niet meer, faalt hij.

**Mini-controle:** stel dat een test verbindt met een database om zijn resultaat te controleren. Is dit een goede unit test? Nee. De test is niet geïsoleerd. Als de database tijdelijk onbeschikbaar is, faalt de test zonder dat er iets mis is met jouw code. Zo'n test noemen we een integratietest, niet een unit test.

---

## 3. Het Arrange-Act-Assert patroon

Elke unit test volgt hetzelfde drieledige patroon, het **Arrange-Act-Assert patroon**, afgekort **AAA**.

```
Arrange   Zet alles klaar: maak objecten aan, stel invoerwaarden in.
Act       Roep de methode aan die je wil testen.
Assert    Controleer of het resultaat overeenkomt met wat je verwacht.
```

Dit patroon geeft structuur aan elke test. Door het consequent toe te passen, worden tests leesbaar voor iedereen, ook voor mensen die de code niet zelf schreven.

**Voorbeeld:**

```csharp
[Fact]
public void CalculateTotal_WithOneProduct_ReturnsCorrectTotal()
{
    // Arrange
    PriceCalculator calculator = new PriceCalculator();
    double price = 9.99;
    int quantity = 1;
    double expected = 9.99;

    // Act
    double result = calculator.CalculateTotal(price, quantity);

    // Assert
    result.Should().Be(expected);
}
```

Je ziet meteen wat er klaargezet wordt, wat er gedaan wordt en wat er gecontroleerd wordt.

**Veelgemaakte fout:** schrijf nooit meerdere `Act`-stappen in één test. Elke test controleert precies één scenario. Moet je twee dingen testen? Schrijf twee tests.

**Mini-controle:** kijk naar de volgende test. Welk deel is Arrange, welk deel is Act en welk deel is Assert?

```csharp
[Fact]
public void ApplyDiscount_WithZeroPercent_ReturnsOriginalPrice()
{
    DiscountCalculator calculator = new DiscountCalculator();
    double result = calculator.ApplyDiscount(100.0, 0);
    result.Should().Be(100.0);
}
```

De eerste regel is Arrange, de tweede regel is Act, de derde regel is Assert.

---

## 4. ZOMBIES: welke testgevallen schrijf je?

Een van de moeilijkste vragen bij het schrijven van tests is: welke gevallen test ik? Het acroniem **ZOMBIES**, bedacht door James W. Grenning, biedt een houvast.

| Letter | Betekenis | Voorbeeld voor een winkelmandje |
|--------|-----------|--------------------------------|
| **Z**ero | Nulwaarden of lege toestand | Leeg mandje, totaal is 0 |
| **O**ne | Precies één element | Mandje met 1 product |
| **M**any | Meerdere elementen | Mandje met 3 producten |
| **B**oundary | Grensgevallen | Exact 50 euro, gratis verzending begint hier |
| **I**nterface | De publieke interface van de klasse | Retourneert de methode het juiste type? |
| **E**xception | Foutgevallen | Negatieve prijs gooit een exception |
| **S**imple | Eenvoud | Elke test controleert precies één scenario |

ZOMBIES helpt je ook de **volgorde** bepalen. Begin bij Zero en One, want die zijn het eenvoudigst te implementeren. De complexere gevallen (Many, Boundary) komen daarna vanzelf.

**Toegepast op `ApplyDiscount`:**

- **Zero:** kortingspercentage is 0, prijs verandert niet
- **One:** een normaal kortingspercentage, bv. 10%
- **Many:** niet van toepassing op één getal, maar wel: meerdere combinaties via `[Theory]`
- **Boundary:** kortingspercentage is precies 0 of precies 100
- **Interface:** de methode geeft een `double` terug, geen `int` of `string`
- **Exception:** kortingspercentage is negatief of groter dan 100
- **Simple:** elke test controleert één combinatie van invoerwaarden

**Mini-controle:** een methode `Divide(double a, double b)` deelt twee getallen. Welke ZOMBIES-gevallen zou je testen? Schrijf de gevallen op voor je verder leest. Mogelijk antwoord: Zero (b is 0, verwacht exception), One (a is 1 of b is 1), Boundary (heel groot of heel klein getal), Exception (b is 0 gooit een DivideByZeroException), Simple (elke test controleert één combinatie).

---

## 5. xUnit: het testframework

In deze cursus gebruiken we **xUnit** als testframework. xUnit is de huidige standaard in de .NET-wereld en wordt actief onderhouden.

### 5.1 [Fact]: één vast testgeval

**`[Fact]`** markeert een methode als een test met één concreet scenario.

```csharp
[Fact]
public void MethodName_StateUnderTest_ExpectedBehavior()
{
    // één concreet scenario
}
```

De naamgevingsconventie bestaat uit drie delen: `MethodNaam_ToestandOnderTest_VerwachtGedrag`.

Voorbeelden:
- `CalculateTotal_WithZeroQuantity_ReturnsZero`
- `ApplyDiscount_WithNegativePercent_ThrowsArgumentException`
- `PlaceOrder_WhenPaymentFails_ReturnsMislukt`

Deze naam leest als een zin: "Als ik `CalculateTotal` aanroep met hoeveelheid nul, verwacht ik dat het resultaat nul is." Als de test faalt, weet je meteen wat er mis is zonder de code te openen.

### 5.2 [Theory]: meerdere scenario's in één methode

**`[Theory]`** met **`[InlineData]`** voert dezelfde testmethode meerdere keren uit, telkens met andere invoerwaarden. Dit voorkomt dat je tien bijna-identieke testmethoden moet schrijven.

```csharp
[Theory]
[InlineData(10.00, 1,  10.00)]
[InlineData( 5.00, 3,  15.00)]
[InlineData( 2.50, 4,  10.00)]
public void CalculateTotal_WithValidInputs_ReturnsCorrectTotal(
    double price, int quantity, double expected)
{
    PriceCalculator calculator = new PriceCalculator();
    double result = calculator.CalculateTotal(price, quantity);
    result.Should().Be(expected);
}
```

xUnit voert deze test drie keer uit, één keer per rij met `[InlineData]`. In de Test Explorer in Visual Studio zie je elk geval afzonderlijk staan, met de exacte invoerwaarden als naam.

**Wat gebeurt er onder de motorkap?** Het attribuut `[Fact]` is een aanduiding voor het xUnit-framework. Wanneer je op "Run All" klikt in de Test Explorer, scant xUnit via **reflectie** alle klassen in het testproject. Het zoekt naar methoden met `[Fact]` of `[Theory]` en voert die uit. Het resultaat (geslaagd of gefaald) geeft het terug aan Visual Studio. Je schrijft dus gewone C#-methoden; xUnit zorgt voor de rest.

**Mini-controle:** wanneer gebruik je `[Fact]` en wanneer gebruik je `[Theory]`? Gebruik `[Fact]` als je één concreet scenario test. Gebruik `[Theory]` als je dezelfde logica wil testen met meerdere combinaties van invoerwaarden, bv. vijf verschillende kortingspercentages.

---

## 6. FluentAssertions

**FluentAssertions** is een bibliotheek die de standaard `Assert`-methoden van xUnit vervangt door leesbaardere alternatieven.

| Situatie | xUnit standaard | FluentAssertions |
|----------|----------------|-----------------|
| Waarde controleren | `Assert.Equal(5.0, result)` | `result.Should().Be(5.0)` |
| Null controleren | `Assert.Null(result)` | `result.Should().BeNull()` |
| Exception verwachten | `Assert.Throws<ArgumentException>(act)` | `act.Should().Throw<ArgumentException>()` |
| Collectie controleren | `Assert.Contains(item, list)` | `list.Should().Contain(item)` |
| Niet leeg | `Assert.NotEmpty(result)` | `result.Should().NotBeNullOrEmpty()` |

Het verschil zit niet alleen in leesbaarheid. Bij een falende test geeft FluentAssertions ook een duidelijkere foutmelding:

```
xUnit:           Expected: 90.0  Actual: 9.0
FluentAssertions: Expected result to be 90.0, but found 9.0.
```

De FluentAssertions-melding bevat de naam van de variabele en een volledige zin, wat het debuggen versnelt.

---

## 7. Het testbaarheidsprobleem

Niet alle klassen zijn even eenvoudig te testen. Beschouw deze klasse:

```csharp
public class OrderService
{
    public string PlaceOrder(double amount)
    {
        PaymentGateway gateway = new PaymentGateway();
        bool success = gateway.ProcessPayment(amount);
        return success ? "Bestelling bevestigd" : "Betaling mislukt";
    }
}
```

`OrderService` maakt zelf een `PaymentGateway` aan. Die gateway communiceert met een extern betaalsysteem. Als je `PlaceOrder` wil testen, heb je altijd een echte verbinding nodig met dat externe systeem.

Dat levert drie concrete problemen op.

1. **De test is traag.** Netwerkcommunicatie duurt tientallen milliseconden, soms langer. Bij honderd zulke tests wacht je minuten.
2. **De test kan falen door een oorzaak buiten jouw code.** Als de betaalprovider een storing heeft, falen jouw tests, ook al is jouw code correct.
3. **Je hebt geen controle over het antwoord.** Je kan niet instellen dat de gateway eens `true` en eens `false` teruggeeft om beide scenario's te testen.

Dit probleem heet **strakke koppeling**: `OrderService` is vastgekleefd aan `PaymentGateway`. Je kan de ene niet vervangen zonder de andere aan te passen.

---

## 8. Dependency Injection

De oplossing heet **Dependency Injection**, afgekort **DI**. In plaats van de afhankelijkheid zelf aan te maken, ontvang je die van buitenaf via de constructor.

**Stap 1: definieer een interface**

Een **interface** beschrijft welke methoden beschikbaar zijn, zonder te zeggen hoe ze werken. Dat maakt het mogelijk om later een andere implementatie in te pluggen.

```csharp
namespace ShopWave
{
    public interface IPaymentGateway
    {
        bool ProcessPayment(double amount);
    }
}
```

**Stap 2: laat `OrderService` de interface ontvangen via de constructor**

```csharp
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
            throw new ArgumentException("Bedrag moet groter zijn dan nul.", nameof(amount));
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
```

**In productie** geef je een echte `PaymentGateway` mee:

```csharp
OrderService service = new OrderService(new PaymentGateway());
```

**In tests** geef je een nep-versie mee die je volledig controleert. Die nep-versie noemen we een **mock**.

**Waarom werkt dit?** `OrderService` kent alleen de interface `IPaymentGateway`, niet de concrete klasse `PaymentGateway`. Hierdoor kan je om het even welk object meegeven, zolang het de interface implementeert. In tests gebruik je een mock; in productie gebruik je de echte gateway. De code van `OrderService` verandert niet.

**Mini-controle:** je hebt een klasse `ReportGenerator` die een `DatabaseConnection` aanmaakt in haar constructor. Hoe zou je die klasse testbaar maken? Maak een interface `IDatabaseConnection`, laat `ReportGenerator` die ontvangen via de constructor in plaats van zelf een `DatabaseConnection` aan te maken. In tests geef je een mock mee; in productie de echte verbinding.

---

## 9. Mocking met Moq

**Moq** is een bibliotheek die automatisch een nep-implementatie genereert voor een interface. Je hoeft geen aparte klasse te schrijven.

### 9.1 Een mock aanmaken

```csharp
Mock<IPaymentGateway> mockGateway = new Mock<IPaymentGateway>();
```

`mockGateway` is een object dat `IPaymentGateway` implementeert. Standaard geeft elke methode de standaardwaarde terug: `false` voor `bool`, `null` voor objecten, `0` voor getallen.

### 9.2 Gedrag instellen met Setup

```csharp
mockGateway.Setup(x => x.ProcessPayment(50.0)).Returns(true);
```

Dit zegt: "Als `ProcessPayment` aangeroepen wordt met het bedrag 50, geef dan `true` terug."

Gebruik `It.IsAny<T>()` als de exacte waarde niet uitmaakt:

```csharp
mockGateway.Setup(x => x.ProcessPayment(It.IsAny<double>())).Returns(true);
```

### 9.3 De mock doorgeven

```csharp
OrderService service = new OrderService(mockGateway.Object);
```

`.Object` geeft de daadwerkelijke nep-instantie terug. Dat is wat je meegeeft aan de constructor.

### 9.4 Verifiëren met Verify

Naast het controleren van het resultaat kan je ook nagaan of een methode aangeroepen werd en hoe vaak:

```csharp
mockGateway.Verify(x => x.ProcessPayment(50.0), Times.Once);
```

| Optie | Betekenis |
|-------|----------|
| `Times.Once` | Precies eenmaal aangeroepen |
| `Times.Never` | Nooit aangeroepen |
| `Times.Exactly(3)` | Precies drie keer aangeroepen |
| `Times.AtLeastOnce` | Minstens eenmaal aangeroepen |

Dit is nuttig om te bewijzen dat jouw klasse de juiste methoden aanroept, los van het resultaat.

**Volledig voorbeeld:**

```csharp
[Fact]
public void PlaceOrder_WhenPaymentSucceeds_ReturnsBevestigd()
{
    // Arrange
    Mock<IPaymentGateway> mockGateway = new Mock<IPaymentGateway>();
    mockGateway.Setup(x => x.ProcessPayment(50.0)).Returns(true);
    OrderService service = new OrderService(mockGateway.Object);

    // Act
    string result = service.PlaceOrder(50.0);

    // Assert
    result.Should().Be("Bestelling bevestigd");
    mockGateway.Verify(x => x.ProcessPayment(50.0), Times.Once);
}
```

**Veelgemaakte fout:** studenten vergeten `Times.Never` te gebruiken om te bewijzen dat een methode niet aangeroepen werd. Als het product niet op voorraad is, moet de betaalgateway nooit aangesproken worden. Controleer dat altijd expliciet met `Verify(..., Times.Never)`, anders test je dat gedrag niet.

**Mini-controle:** wat is het verschil tussen `Setup` en `Verify`? `Setup` bepaalt wat de mock teruggeeft als een methode aangeroepen wordt. `Verify` controleert achteraf of een methode daadwerkelijk aangeroepen werd, en hoe vaak. Je kan `Verify` gebruiken zonder `Setup`, en omgekeerd.

---

## 10. Testing en security: de link

Je hebt nu de basis van unit testing in handen. Maar waarom staat dit vak "Testing en Security"? Wat heeft testen te maken met beveiliging?

Het antwoord is eenvoudig: **beveiligingsfouten zijn ook gewoon fouten in code**. En fouten in code vind je met tests.

Beschouw een methode die een wachtwoord valideert:

```csharp
public bool ValidatePassword(string password)
{
    return password == "admin123";
}
```

Dit is een ernstig beveiligingsprobleem. Een unit test maakt dat meteen zichtbaar:

```csharp
[Fact]
public void ValidatePassword_WithHardcodedAdmin_FailsSecurityCheck()
{
    PasswordValidator validator = new PasswordValidator();
    bool result = validator.ValidatePassword("admin123");
    result.Should().BeFalse("hardcoded wachtwoorden zijn een beveiligingsrisico");
}
```

Vanaf les 2 gaan we verder op dit pad: hashing, encryptie, JWT-tokens en SQL injection. Al die concepten kan je testen met dezelfde technieken die je vandaag geleerd hebt.

## 11. Samenvatting

| Concept | Wat je moet onthouden |
|--------|-----------------------|
| Unit test | Controleert één methode, geïsoleerd, snel, herhaalbaar |
| AAA | Arrange, Act, Assert: de vaste structuur van elke test |
| ZOMBIES | Houvast voor het bepalen van testgevallen |
| `[Fact]` | Eén vast testgeval |
| `[Theory]` + `[InlineData]` | Meerdere scenario's in één testmethode |
| FluentAssertions | Leesbaardere asserts met betere foutmeldingen |
| Strakke koppeling | Klasse maakt afhankelijkheid zelf aan, moeilijk te testen |
| Dependency Injection | Afhankelijkheid ontvangen via constructor, niet zelf aanmaken |
| Interface | Beschrijft wat een klasse kan, zonder hoe, maakt mocking mogelijk |
| `Mock<T>` | Nep-implementatie van een interface, volledig onder controle |
| `Setup(...).Returns(...)` | Bepaalt wat de mock teruggeeft |
| `It.IsAny<T>()` | Geldt voor elke waarde van het opgegeven type |
| `Verify(...)` | Controleert of een methode aangeroepen werd en hoe vaak |
