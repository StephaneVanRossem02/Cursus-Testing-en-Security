---
title: "Les 10: Oplossingen - Integration Testing (Mockoon)"
sidebar_label: "Oplossingen"
---

# Oplossingen: Integration Testing (Mockoon)

**Bekijk dit pas nadat je de oefeningen zelf geprobeerd hebt.** Lees de toelichting ook als je het juist had.

---

## Oplossing 1: tweede bestemming via `[Theory]`

### Mockoon-configuratie

Voeg een tweede route toe op hetzelfde pad `/api/verzending`. Gebruik **Rules** om de route te activeren als de query parameter `bestemming` gelijk is aan `Brussel`.

Klik op de bestaande route, klik op **Rules** en voeg een rule toe:
- **Target:** Query param
- **Modifier:** `bestemming`
- **Operator:** equals
- **Value:** `Brussel`

Doe hetzelfde voor de Antwerpen-route zodat ook die alleen activeert als `bestemming=Antwerpen`. Zo weet Mockoon welke route hij moet kiezen.

Response voor Brussel:

```json
{
  "bestemming": "Brussel",
  "gewicht": 1.0,
  "tarief": 4.49,
  "vervoerder": "bpost"
}
```

### ShippingClientIntegrationTests.cs

```csharp
[Theory]
[InlineData("Antwerpen", 2.5, 6.99, "DHL")]
[InlineData("Brussel",   1.0, 4.49, "bpost")]
public async Task GetShippingRateAsync_WithKnownDestination_ReturnsCorrectTarief(
    string destination,
    double weight,
    double expectedTarief,
    string expectedVervoerder)
{
    // Arrange
    HttpClient     httpClient = new HttpClient();
    ShippingClient client     = new ShippingClient(httpClient, MockoonBaseUrl);

    // Act
    ShippingResponse result = await client.GetShippingRateAsync(
        destination: destination,
        weight:      weight);

    // Assert
    result.Should().NotBeNull();
    result.Tarief.Should().Be(expectedTarief);
    result.Vervoerder.Should().Be(expectedVervoerder);

    httpClient.Dispose();
}
```

### Toelichting

`[Theory]` laat je één testmethode schrijven voor meerdere invoerwaarden. xUnit voert de methode twee keer uit, een keer per `[InlineData]`-rij. De Test Explorer toont beide uitvoeringen als aparte resultaten.

Je geeft `double`-waarden mee als `[InlineData]`-parameter. Dat werkt hier omdat `double` een primitief type is dat door .NET-attributen ondersteund wordt.

**Veelgemaakte fout:** studenten voegen de tweede route toe in Mockoon maar vergeten Rules in te stellen op beide routes. Mockoon kiest dan de eerste route die aan de request voldoet, ongeacht de `bestemming`-parameter. Beide tests sturen de response van Antwerpen terug en de Brussel-test faalt.

**Veelgemaakte fout:** studenten vergeten `httpClient.Dispose()` op te roepen. Bij veel tests lopen er open HTTP-verbindingen op. In productie gebruik je `IHttpClientFactory` om dit te vermijden. In een les-context volstaat `Dispose()` aan het einde van elke test.

---

## Oplossing 2: foutscenarios uitbreiden

### Mockoon-configuratie

Voeg twee extra routes toe met Rules op de query parameter `bestemming`:

- Route A: `bestemming=ONBEKEND`, status 404, leeg body
- Route B: `bestemming=OFFLINE`, status 503, leeg body

### ShippingClientIntegrationTests.cs

```csharp
[Theory]
[InlineData("FOUT")]
[InlineData("ONBEKEND")]
[InlineData("OFFLINE")]
public async Task GetShippingRateAsync_WithErrorDestination_ThrowsHttpRequestException(
    string destination)
{
    // Arrange
    HttpClient     httpClient = new HttpClient();
    ShippingClient client     = new ShippingClient(httpClient, MockoonBaseUrl);

    // Act
    Func<Task> act = async () =>
    {
        await client.GetShippingRateAsync(destination: destination, weight: 1.0);
    };

    // Assert
    await act.Should().ThrowAsync<HttpRequestException>();

    httpClient.Dispose();
}
```

### Toelichting

`EnsureSuccessStatusCode()` gooit een `HttpRequestException` voor elke statuscode die geen 2xx is: 404, 500, 503 geven allemaal dezelfde exception. Je hoeft niet per statuscode een andere exception te verwachten.

Dat is ook het ontwerp van `ShippingClient`: de klasse behandelt alle HTTP-fouten op dezelfde manier. Als je in productie wil onderscheiden of het een 404 (resource niet gevonden) of een 503 (service onbeschikbaar) is, lees je de statuscode van de `HttpRequestException`:

```csharp
catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
{
    // resource niet gevonden
}
```

**Veelgemaakte fout:** studenten schrijven `act.Should().Throw<HttpRequestException>()` in plaats van `await act.Should().ThrowAsync<HttpRequestException>()`. De synchrone versie vangt geen async exceptions op. De test slaagt altijd, ook als er helemaal geen exception gegooid wordt.

---

## Oplossing 3: timeout en latency

### Mockoon-configuratie

Klik op de route voor Antwerpen, zoek het veld **Latency (ms)** en vul `5000` in. Start Mockoon opnieuw.

### ShippingClientIntegrationTests.cs

```csharp
// Waarom timeouts testen?
// Zonder timeout blokkeert de applicatie onbepaald als de externe service niet reageert.
// Bij genoeg gelijktijdige requests raken alle threads op en reageert de service niet meer.
//
// HttpClient gooit TaskCanceledException (niet HttpRequestException) bij een timeout.
// HttpRequestException gooit EnsureSuccessStatusCode bij een foutieve statuscode.
// TaskCanceledException gooit HttpClient als de timeout verstrijkt.
[Fact]
public async Task GetShippingRateAsync_WhenRequestTimesOut_ThrowsTaskCanceledException()
{
    // Arrange
    HttpClient httpClient = new HttpClient();
    httpClient.Timeout    = TimeSpan.FromSeconds(2);
    ShippingClient client = new ShippingClient(httpClient, MockoonBaseUrl);

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

### Toelichting

`HttpClient.Timeout` stelt de maximale wachttijd in voor de volledige request, inclusief verbinding maken en het antwoord ontvangen. Als de server na die tijd niet geantwoord heeft, annuleert `HttpClient` de request intern via een `CancellationToken`. Dat resulteert in een `TaskCanceledException`.

`HttpRequestException` en `TaskCanceledException` zijn twee verschillende exceptions voor twee verschillende oorzaken. Studenten vergissen ze soms omdat beide optreden bij "iets mis met de HTTP-call".

**Vergeet na de test de latency te verwijderen.** Als je die laat staan, falen alle andere tests na 2 seconden. Zet de latency terug naar 0 in Mockoon en controleer dat alle tests opnieuw slagen.

**Veelgemaakte fout:** studenten verwachten `HttpRequestException` in plaats van `TaskCanceledException`. De test faalt dan omdat de werkelijke exception een `TaskCanceledException` is. Lees de foutmelding in Test Explorer: die toont het werkelijke type van de gegoyde exception.

---

## Oplossing 4: WireMock.Net

### Installatie

```csharp
dotnet add package WireMock.Net
```

Of via **Tools > NuGet Package Manager > Manage NuGet Packages for Solution** en zoek op `WireMock.Net`.

### ShippingClientWireMockTests.cs

```csharp
using WireMock.Server;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using FluentAssertions;
using ShopWave;

namespace ShopWave.Tests
{
    public class ShippingClientWireMockTests
    {
        [Fact]
        public async Task GetShippingRateAsync_WithWireMock_ReturnsTarief()
        {
            // Arrange
            WireMockServer server = WireMockServer.Start();

            server.Given(
                Request.Create()
                       .WithPath("/api/verzending")
                       .UsingGet())
            .RespondWith(
                Response.Create()
                        .WithStatusCode(200)
                        .WithBodyAsJson(new
                        {
                            bestemming = "Antwerpen",
                            gewicht    = 2.5,
                            tarief     = 6.99,
                            vervoerder = "DHL"
                        }));

            HttpClient     httpClient = new HttpClient();
            ShippingClient client     = new ShippingClient(httpClient, server.Url!);

            // Act
            ShippingResponse result = await client.GetShippingRateAsync(
                destination: "Antwerpen",
                weight: 2.5);

            // Assert
            result.Should().NotBeNull();
            result.Tarief.Should().Be(6.99);
            result.Vervoerder.Should().Be("DHL");

            httpClient.Dispose();
            server.Stop();
        }

        [Fact]
        public async Task GetShippingRateAsync_WithWireMock_WhenServerReturns500_ThrowsHttpRequestException()
        {
            // Arrange
            WireMockServer server = WireMockServer.Start();

            server.Given(
                Request.Create()
                       .WithPath("/api/verzending")
                       .UsingGet())
            .RespondWith(
                Response.Create()
                        .WithStatusCode(500));

            HttpClient     httpClient = new HttpClient();
            ShippingClient client     = new ShippingClient(httpClient, server.Url!);

            // Act
            Func<Task> act = async () =>
            {
                await client.GetShippingRateAsync(destination: "Antwerpen", weight: 1.0);
            };

            // Assert
            await act.Should().ThrowAsync<HttpRequestException>();

            httpClient.Dispose();
            server.Stop();
        }
    }
}
```

### Toelichting

`WireMockServer.Start()` zonder argument start de server op een willekeurige vrije poort. `server.Url` geeft de volledige basis-URL terug, inclusief poortnummer. Je geeft die URL mee aan `ShippingClient` via de constructor. Dezelfde code als in de demo, andere URL.

De tests in `ShippingClientWireMockTests` vereisen geen draaiende Mockoon-instantie. De server start bij het begin van elke test en stopt bij het einde. Elke test is volledig onafhankelijk.

**Veelgemaakte fout:** studenten roepen `server.Stop()` niet aan. De server blijft dan draaien na de test. Bij veel tests kunnen poorten opraken of kan een volgende test een server vinden die al bezet is. Roep altijd `server.Stop()` aan, of gebruik een `using`-blok als `WireMockServer` `IDisposable` implementeert.

**Veelgemaakte fout:** studenten vergissen `server.Url` en `server.Urls`. `server.Url` is een `string?` met de eerste URL. `server.Urls` is een array van alle URLs waarop de server luistert. Gebruik `server.Url!` (met null-forgiving operator) als je zeker bent dat de server gestart is.

---

## Oplossing 5: reflectie

**Vraag 1: verschil Moq en mock server**

Moq vervangt een C#-interface door een nep-object in het geheugen van het testproces. Je test de logica van de klasse die de interface gebruikt, maar niet de communicatie via HTTP. Een mock server simuleert een volledige HTTP-server via een echte TCP-verbinding op localhost. Je test de echte HTTP-call, inclusief URL-opbouw, headers en JSON-parsing.

Gebruik Moq als je de logica van één klasse in isolatie wil testen. Gebruik een mock server als je de HTTP-communicatie van een klasse wil testen.

**Vraag 2: welke bug detecteert welk type?**

Unit test (les 1): een bug in de kortingsberekening van `DiscountCalculator`. Die klasse staat in isolatie, de test richt zich op de formule.

Integration test zonder HTTP (les 5): een bug in de samenwerking tussen `CartService` en `CouponService`. De unit tests van beide klassen slagen, maar samen gedragen ze zich anders dan verwacht.

Integration test met mock server (les 10): een bug in de JSON-parsing van `ShippingClient`. De veldnaam in de JSON-response komt niet overeen met de C#-property. Unit tests en les-5-integration tests raken nooit de JSON-deserialisatie.

**Vraag 3: welke test faalt als `"tarief"` verandert naar `"prijs"`?**

De integration test met Mockoon faalt als eerste. Die test roept de echte `GetShippingRateAsync` aan, die de JSON parseert. Als het JSON-veld `"prijs"` heet maar de C#-property `Tarief` heet, geeft `JsonSerializer.Deserialize` `0.0` terug voor `Tarief`. De assertion `result.Tarief.Should().Be(6.99)` faalt.

Unit tests en les-5-integration tests merken niets: die raken de JSON-parsing niet.

**Vraag 4: Mockoon vs WireMock.Net in CI/CD**

WireMock.Net is beter geschikt voor CI/CD. De server start en stopt automatisch als onderdeel van het testproces. Geen handmatige stap nodig. Mockoon vereist dat een persoon de applicatie opstart voor de tests draaien, wat niet mogelijk is in een geautomatiseerde pipeline.

---

## Dit project downloaden

[Download het volledige ShopWave-project van les 10](/downloads/shopwave-10-integration-testing-mockoon.zip) (ZIP)

Bevat alle code tot en met deze les, klaar om te openen in Visual Studio. Bouwen en testen doe je met `dotnet build` en `dotnet test`. In de `README.md` staat wat er nieuw is en hoeveel tests er horen te slagen.
