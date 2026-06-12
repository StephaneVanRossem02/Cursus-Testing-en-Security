---
title: "Les 10: Integration Testing — Mockoon"
sidebar_label: "Integration Testing (Mockoon)"
---

# Les 10: Integration Testing — Mockoon

## ShopWave

We werken verder in de **ShopWave**-solution. In Les 5 schreven we integratietesten die klassen samen testten — zonder mocks, met echte objecten. Dat werkte goed zolang alle klassen in ons eigen project stonden.

Maar ShopWave doet in productie meer dan alleen intern werken. De API roept externe services aan over HTTP: een verzendservice die het leveringstarief berekent, een betalingsprovider die transacties verwerkt. Die externe services kun je niet zomaar in een test aanspreken. In deze les leer je hoe je dat probleem oplost met een **mock server**.

---

## Theorie

### 1. Het probleem met Moq bij HTTP-calls

In Les 1 leerde je Moq kennen: een framework dat een nep-implementatie maakt van een interface. Je gebruikt het om externe afhankelijkheden te vervangen door objecten die je volledig onder controle hebt.

Stel dat `ShopWave.Api` een externe verzendservice aanspreekt via HTTP:

```csharp
public class ShippingClient
{
    private readonly HttpClient _httpClient;

    public ShippingClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<double> GetShippingRateAsync(string destination, double weight)
    {
        string url = $"http://verzendservice.be/api/tarief?bestemming={destination}&gewicht={weight}";
        HttpResponseMessage response = await _httpClient.GetAsync(url);
        string json = await response.Content.ReadAsStringAsync();
        ShippingResponse result = System.Text.Json.JsonSerializer.Deserialize<ShippingResponse>(json)!;
        return result.Tarief;
    }
}
```

Je kan `HttpClient` niet zomaar mocken met Moq. `HttpClient` is een concrete klasse — geen interface. Je kan wel `HttpMessageHandler` mocken, maar dat leidt tot ingewikkelde en fragiele testcode die nauwelijks leesbaar is. Bovendien test je daarmee niet de echte HTTP-call — je vervangt de kern van het probleem door een nep-versie.

En zelfs als je `IShippingClient` definieert en die mockt, heb je nog steeds een probleem: je test niet of de echte `ShippingClient` correct werkt. Je weet niet of de URL correct is opgebouwd, of de JSON-parsing klopt, of de HTTP-statuscode correct wordt afgehandeld. Die integratie blijft ongetest.

Dit is precies het probleem dat mock services oplossen.

---

### 2. Integratietesten versus unit testen

Een **unit test** test één klasse in isolatie. Alle afhankelijkheden worden vervangen door mocks. Dit is snel en eenvoudig, maar heeft een fundamenteel nadeel: je test niet of de klassen correct samenwerken.

Een **integratietest** test meerdere klassen samen, inclusief de echte afhankelijkheden. De test loopt zo dicht mogelijk bij de productieomgeving.

| Eigenschap | Unit test | Integratietest |
|-----------|-----------|----------------|
| Snelheid | Milliseconden | Seconden |
| Wat wordt getest | Één klasse, geïsoleerd | Meerdere klassen samen |
| Afhankelijkheden | Vervangen door mocks | Echte objecten |
| Fouten detecteren | Fouten in de logica van één klasse | Fouten in de samenwerking tussen klassen |
| Gevoelig voor refactoring | Ja — interne structuur wijzigen breekt de test | Nee — gedrag kan hetzelfde blijven |

Beide soorten tests zijn nodig. Unit tests zijn snel en precies — ze zeggen exact welke methode fout loopt. Integratietests zijn breder — ze zeggen of het systeem als geheel correct werkt.

De **testpiramide** geeft de verhouding aan: veel unit tests als fundament, minder integratietests in het midden, weinig end-to-end tests aan de top.

```
        /\
       /  \        End-to-end tests (weinig, traag)
      /----\
     /      \      Integratietests (matig)
    /--------\
   /          \    Unit tests (veel, snel)
  /____________\
```

---

### 3. Wat is een mock service?

Een **mock service** is een lokale HTTP-server die je zelf configureert om vaste antwoorden terug te geven. Wat Moq doet voor een klasse, doet een mock service voor een externe HTTP-service.

```
Moq:          IPaymentGateway (interface)  →  nep-object in geheugen
Mock service: http://externe-api.be       →  nep-server op localhost
```

Je configureert de mock server: "als je een GET-request krijgt op `/api/tarief`, geef dan dit JSON-antwoord terug." De `ShippingClient` maakt een echte HTTP-call — maar in plaats van naar de externe server, gaat die call naar de mock server op je eigen machine.

Dit geeft je het beste van beide werelden:

- Je test de **echte `ShippingClient`-code** — inclusief URL-opbouw, HTTP-call en JSON-parsing
- Je bent **niet afhankelijk van de externe service** — die hoeft niet online te zijn, je controleert wat hij teruggeeft
- Je test is **betrouwbaar en herhaalbaar** — de mock server geeft altijd hetzelfde antwoord

---

### 4. Wanneer gebruik je een mock service?

**Bij testen:**

Externe services zijn buiten je controle. Ze kunnen offline zijn, ze kunnen van response veranderen, ze kunnen kosten aanrekenen per API-call. Een mock service elimineert al die problemen. Je test slaagt of faalt uitsluitend op basis van jouw eigen code.

Bovendien kan je met een mock service specifieke scenario's forceren die moeilijk te reproduceren zijn in de echte service: een timeout, een 500-foutcode, een ongeldig JSON-antwoord.

**Bij ontkoppeling van ontwikkeling:**

Stel dat het team dat de verzendservice bouwt nog niet klaar is. Jij kan toch al beginnen met de `ShippingClient` implementeren en testen, op basis van de afgesproken API-structuur. De mock server simuleert de service die er nog niet is.

Als de externe service instabiel is of regelmatig wijzigt in de beginfase, werkt je eigen code gewoon door — de mock server verandert niet tenzij jij dat doet.

---

### 5. Mockoon

Er zijn verschillende mock server-tools beschikbaar.

**Mockoon** heeft een visuele interface waarmee je snel routes aanmaakt — ideaal om mee te starten. Dat is wat we in deze les gebruiken.

**WireMock.Net** is een NuGet-pakket dat de mock server in-process draait — geen aparte applicatie nodig. Ideaal voor geautomatiseerde CI/CD-pipelines waar je niet wil dat testers Mockoon handmatig moeten starten. Installeer met:

```
dotnet add package WireMock.Net
```

Voorbeeld:

```csharp
using WireMock.Server;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;

WireMockServer server = WireMockServer.Start(3001);

server.Given(
    Request.Create()
           .WithPath("/api/verzending")
           .UsingGet())
.RespondWith(
    Response.Create()
            .WithStatusCode(200)
            .WithBodyAsJson(new { vervoerder = "DHL", tarief = 5.99 }));

// ... test hier ...

server.Stop();
```

In je testklasse kan je WireMock.Net ook in `IDisposable` inpakken zodat de server automatisch stopt na elke test.

**Mockoon** is eenvoudiger voor manuele demos en heeft een visuele interface. Omdat het principe bij alle tools hetzelfde is, gebruiken we Mockoon als visueel startpunt in de demo.

Mockoon is beschikbaar op Windows, macOS en Linux. Downloaden via: `https://mockoon.com/#download`

Na de installatie open je Mockoon en zie je een lege omgeving. Je maakt een **mock environment** aan — een lokale server met een poortnummer (standaard 3000) — en voegt daaraan **routes** toe. Elke route is een combinatie van een HTTP-methode, een URL-pad en een response die je zelf definieert.

---

## Demo

We breiden `ShopWave.Api` uit met een `ShippingClient` die het leveringstarief ophaalt bij een externe verzendservice. We configureren Mockoon als die externe service en schrijven daarna een integratietest die de volledige flow test.

---

### Stap 1 — ShippingResponse aanmaken

Maak in het project `ShopWave` een nieuw bestand aan:

**Bestand: `ShopWave/ShippingResponse.cs`**

```csharp
namespace ShopWave
{
    public class ShippingResponse
    {
        public string Bestemming { get; set; } = string.Empty;
        public double Gewicht { get; set; }
        public double Tarief { get; set; }
        public string Vervoerder { get; set; } = string.Empty;
    }
}
```

Dit is het model dat de verzendservice teruggeeft als JSON.

---

### Stap 2 — ShippingClient aanmaken

**Bestand: `ShopWave/ShippingClient.cs`**

Eerst de klasse en de constructor:

```csharp
using System.Net.Http;
using System.Text.Json;

namespace ShopWave
{
    public class ShippingClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public ShippingClient(HttpClient httpClient, string baseUrl)
        {
            _httpClient = httpClient;
            _baseUrl    = baseUrl;
        }
```

De `baseUrl` wordt meegegeven via de constructor — niet hardcoded. Dat is precies wat we nodig hebben: in productie geven we de echte URL mee, in tests geven we de URL van Mockoon mee.

Voeg de methode toe die het tarief ophaalt:

```csharp
        public async Task<ShippingResponse> GetShippingRateAsync(
            string destination, double weight)
        {
            string url = $"{_baseUrl}/api/verzending?bestemming={destination}&gewicht={weight}";
            HttpResponseMessage response = await _httpClient.GetAsync(url);

            response.EnsureSuccessStatusCode();

            string json = await response.Content.ReadAsStringAsync();

            ShippingResponse result = JsonSerializer.Deserialize<ShippingResponse>(
                json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

            return result;
        }
    }
}
```

`EnsureSuccessStatusCode()` gooit een exception als de HTTP-statuscode geen 2xx is. Zo vangt de code netwerkfouten netjes op in plaats van stil te falen met een null-referentie later.

`PropertyNameCaseInsensitive = true` zorgt dat JSON-velden zoals `"tarief"` correct worden gemapt op de C#-property `Tarief`, ongeacht hoofdlettergebruik.

---

### Stap 3 — Mockoon configureren

Open Mockoon. Maak een nieuwe mock environment aan:

1. Klik op het `+`-icoontje naast "Mock local server"
2. Geef de omgeving de naam `ShopWave Verzendservice`
3. Stel poort `3001` in (om conflict met ShopWave.Api op 5001 te vermijden)

Voeg een route toe:

1. Klik op `Add route`
2. Stel in:
   - **Method:** GET
   - **Path:** `/api/verzending`
   - **Status:** 200
3. Plak in het **Body**-veld onderstaande JSON:

```json
{
  "bestemming": "Antwerpen",
  "gewicht": 2.5,
  "tarief": 6.99,
  "vervoerder": "DHL"
}
```

Klik op de groene **play**-knop om de mock server te starten. Navigeer in je browser naar:

```
http://localhost:3001/api/verzending?bestemming=Antwerpen&gewicht=2.5
```

Je ziet het JSON-antwoord dat je net configureerde. Mockoon gedraagt zich nu als een echte externe verzendservice — maar volledig onder jouw controle.

---

### Stap 4 — Handmatige test vanuit ShopWave

Voeg tijdelijk testcode toe in `ShopWave/Program.cs` om de integratie te testen:

**Bestand: `ShopWave/Program.cs`**

```csharp
using System.Net.Http;
using ShopWave;

HttpClient httpClient = new HttpClient();
ShippingClient shippingClient = new ShippingClient(httpClient, "http://localhost:3001");

Console.WriteLine("Tarief opvragen bij Mockoon...");

// Gebruik await, nooit .Result — .Result blokkeert de thread en kan deadlocks veroorzaken
ShippingResponse tarief = await shippingClient.GetShippingRateAsync(
    destination: "Antwerpen",
    weight: 2.5);

Console.WriteLine($"Vervoerder : {tarief.Vervoerder}");
Console.WriteLine($"Bestemming : {tarief.Bestemming}");
Console.WriteLine($"Gewicht    : {tarief.Gewicht} kg");
Console.WriteLine($"Tarief     : {tarief.Tarief} euro");

httpClient.Dispose();
```

Zorg dat Mockoon draait en start `ShopWave`. De output:

```
Tarief opvragen bij Mockoon...
Vervoerder : DHL
Bestemming : Antwerpen
Gewicht    : 2,5 kg
Tarief     : 6,99 euro
```

De `ShippingClient` maakt een echte HTTP-call — maar naar Mockoon in plaats van naar een externe server.

---

### Stap 5 — Integratietest schrijven

Nu schrijven we een geautomatiseerde test in `ShopWave.Tests`.

**Bestand: `ShopWave.Tests/ShippingClientIntegrationTests.cs`**

Eerst de klasse aanmaken:

```csharp
using FluentAssertions;
using System.Net.Http;
using ShopWave;

namespace ShopWave.Tests
{
    public class ShippingClientIntegrationTests
    {
        private const string MockoonBaseUrl = "http://localhost:3001";
```

De URL van Mockoon als constante zodat je hem op één plek kan aanpassen.

Voeg de eerste test toe — het normale scenario:

```csharp
        [Fact]
        public async Task GetShippingRateAsync_WithValidRequest_ReturnsTarief()
        {
            // Arrange
            HttpClient httpClient       = new HttpClient();
            ShippingClient client       = new ShippingClient(httpClient, MockoonBaseUrl);

            // Act
            ShippingResponse result = await client.GetShippingRateAsync(
                destination: "Antwerpen",
                weight: 2.5);

            // Assert
            result.Should().NotBeNull();
            result.Tarief.Should().Be(6.99);
            result.Vervoerder.Should().Be("DHL");

            httpClient.Dispose();
        }
```

Voeg een tweede test toe die controleert wat er gebeurt als de server een fout teruggeeft:

```csharp
        [Fact]
        public async Task GetShippingRateAsync_WhenServerReturns500_ThrowsException()
        {
            // Arrange
            // Configureer in Mockoon een tweede route op /api/verzending/fout met status 500
            HttpClient httpClient = new HttpClient();
            ShippingClient client = new ShippingClient(httpClient, MockoonBaseUrl);

            // Act
            Func<Task> act = async () =>
            {
                await client.GetShippingRateAsync(
                    destination: "FOUT",
                    weight: 1.0);
            };

            // Assert
            await act.Should().ThrowAsync<HttpRequestException>();

            httpClient.Dispose();
        }
    }
}
```

Voor de tweede test voeg je in Mockoon een extra route toe:

- **Method:** GET
- **Path:** `/api/verzending` met query parameter `bestemming` gelijk aan `FOUT`
- **Status:** 500
- **Body:** leeg

Mockoon laat je via **Rules** voorwaarden instellen op query parameters. Zo kan één route verschillende responses geven afhankelijk van de input.

---

### Stap 6 — Tests uitvoeren

Zorg dat Mockoon draait met de geconfigureerde routes. Open de Test Explorer in Visual Studio en voer de integratietests uit. De tests maken echte HTTP-calls naar Mockoon — geen mocks in geheugen.

Als Mockoon niet draait, falen de tests met een `HttpRequestException` omdat `localhost:3001` niet bereikbaar is. Dat is correct gedrag: een integratietest die afhankelijk is van een externe service, faalt als die service niet beschikbaar is. Dit is bewust — je wil weten wanneer de integratie kapot is.

---

## Oefeningen

### Oefening 1 — Tweede route in Mockoon

Voeg in Mockoon een tweede route toe die een ander tarief teruggeeft voor een andere bestemming:

- **Path:** `/api/verzending`
- **Bestemming:** `Brussel`
- **Status:** 200
- **Tarief:** `4.49`
- **Vervoerder:** `bpost`

Schrijf daarna in `ShippingClientIntegrationTests.cs` een test die controleert:
- Dat het tarief voor Brussel `4.49` is
- Dat de vervoerder `bpost` is

Maak gebruik van `[Theory]` en `[InlineData]` om beide bestemmingen (Antwerpen en Brussel) in één testmethode te testen.

---

### Oefening 2 — Timeout simuleren

Mockoon ondersteunt een **latency**-instelling per route. Hiermee kan je een vertraging simuleren.

1. Stel in Mockoon een latency in van 5000 milliseconden (5 seconden) op de bestaande route
2. Voeg in `ShippingClient` een timeout toe aan de `HttpClient`:

**Bestand: `ShopWave.Tests/ShippingClientIntegrationTests.cs`**

```csharp
[Fact]
public async Task GetShippingRateAsync_WhenRequestTimesOut_ThrowsException()
{
    // Arrange — stel timeout in op 2 seconden
    HttpClient httpClient   = new HttpClient();
    httpClient.Timeout      = TimeSpan.FromSeconds(2);
    ShippingClient client   = new ShippingClient(httpClient, MockoonBaseUrl);

    // Act
    Func<Task> act = async () =>
    {
        await client.GetShippingRateAsync(destination: "Antwerpen", weight: 1.0);
    };

    // Assert
    await act.Should().ThrowAsync<TaskCanceledException>();

    httpClient.Dispose();
}
```

Voer de test uit. De test slaagt als de timeout na 2 seconden een exception gooit, terwijl Mockoon pas na 5 seconden antwoordt.

Beantwoord:
1. Waarom is het belangrijk om timeouts te testen?
2. Wat zou er in productie gebeuren als je geen timeout instelt en de externe service niet reageert?

---

### Oefening 3 — ShippingClient integreren in ShopWave.Api

Voeg in `ShopWave.Api` een nieuw endpoint `/verzending` toe dat het tarief opvraagt bij de `ShippingClient`:

**Bestand: `ShopWave.Api/Program.cs`**

```csharp
app.MapGet("/verzending", async (string bestemming, double gewicht) =>
{
    // Maak een ShippingClient aan met de URL van de echte of gesimuleerde verzendservice
    // Roep GetShippingRateAsync aan
    // Geef het resultaat terug als JSON
});
```

Test het endpoint via de browser terwijl Mockoon draait:
```
https://localhost:5001/verzending?bestemming=Antwerpen&gewicht=2.5
```

Schrijf daarna een integratietest in `ShopWave.Tests` die het endpoint test via een echte `HttpClient`-call naar ShopWave.Api, terwijl ShopWave.Api op zijn beurt een call doet naar Mockoon. Dit is een keten van twee HTTP-calls — beide echt.

---

### Oefening 4 — Vergelijking Moq versus Mockoon

Beantwoord de volgende vragen op basis van wat je deze les leerde:

1. In welke situatie gebruik je Moq en in welke situatie gebruik je Mockoon?
2. Wat test je met Moq dat je niet kan testen met Mockoon, en omgekeerd?
3. `ShippingClient` heeft een `baseUrl`-parameter in de constructor. Waarom is dat een bewuste ontwerpbeslissing? Wat zou er mislopen als de URL hardcoded stond?
4. Als de echte verzendservice morgen zijn JSON-structuur wijzigt (bv. `tarief` wordt `prijs`), welke test zou als eerste falen? Leg uit waarom.
5. Kan je de integratietest van oefening 1 ook schrijven als unit test met Moq? Wat verlies je daarmee?

---

## Modeloplossing

> De modeloplossing is beschikbaar na het indienen van de labo-opdracht via Digitap.

---

### Modeloplossing Oefening 1 — Theory met twee bestemmingen

**Bestand: `ShopWave.Tests/ShippingClientIntegrationTests.cs`**

```csharp
[Theory]
[InlineData("Antwerpen", 2.5, 6.99, "DHL")]
[InlineData("Brussel",   1.0, 4.49, "bpost")]
public async Task GetShippingRateAsync_WithKnownDestination_ReturnsCorrectTarief(
    string destination, double weight, double expectedTarief, string expectedVervoerder)
{
    // Arrange
    HttpClient httpClient   = new HttpClient();
    ShippingClient client   = new ShippingClient(httpClient, MockoonBaseUrl);

    // Act
    ShippingResponse result = await client.GetShippingRateAsync(
        destination: destination,
        weight: Convert.ToDecimal(weight));

    // Assert
    result.Should().NotBeNull();
    result.Tarief.Should().Be(Convert.ToDecimal(expectedTarief));
    result.Vervoerder.Should().Be(expectedVervoerder);

    httpClient.Dispose();
}
```

`[InlineData]` werkt niet met `double` — dat is een .NET-beperking op attribuutparameters. We gebruiken `double` als parameter en converteren via `Convert.ToDecimal()`.

---

## Samenvatting

| Concept | Wat je moet onthouden |
|---------|-----------------------|
| Unit test met Moq | Test één klasse in isolatie — vervangt afhankelijkheden door nep-objecten in geheugen |
| Integratietest | Test meerdere klassen samen met echte objecten — dichter bij productie |
| Beperking van Moq bij HTTP | `HttpClient` is niet eenvoudig mockbaar — de echte HTTP-call wordt niet getest |
| Mock service | Lokale HTTP-server die vaste antwoorden geeft — vervangt een externe service tijdens tests |
| Mockoon | Visuele tool om snel routes en responses te configureren |
| `baseUrl` via constructor | Maakt de klasse testbaar — URL is vervangbaar zonder code te wijzigen |
| `EnsureSuccessStatusCode()` | Gooit een exception bij HTTP-foutcodes — voorkomt stille fouten |
| Testpiramide | Veel unit tests, matig integratietests, weinig end-to-end tests |

---

## Volgende les

Les 11 is **Ethisch Hacken**. We draaien de rollen om: in plaats van ShopWave te bouwen en beveiligen, gaan we als ethische hacker te werk op onze eigen lokale applicatie. We onderscheppen HTTP-requests met Burp Suite, manipuleren JWT-tokens, testen de loginflow op brute force-kwetsbaarheden en voeren een basispentest uit met OWASP ZAP — volledig legaal, volledig op onze eigen omgeving.