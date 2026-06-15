---
title: "Les 10: Oefeningen - Integration Testing (Mockoon)"
sidebar_label: "Oefeningen"
---

# Oefeningen: Integration Testing (Mockoon)

Werk de oefeningen in volgorde. Elke oefening bouwt verder op de vorige. Kijk niet vooraf in de oplossingen.

De solution die je gebruikt is dezelfde ShopWave-solution als in les 1, 3 en 5. Je werkt in `ShopWave.Tests/ShippingClientIntegrationTests.cs`.

---

<h3 class="opdracht-titel">Opdracht</h3>

## Oefening 1: tweede bestemming testen via `[Theory]`

**Leerdoel:** je leert meerdere scenarios samenvatten in één test met `[Theory]` en `[InlineData]`.

**Moeilijkheidsgraad:** basis

**Situatie:** de demo testte het tarief voor Antwerpen. ShopWave levert ook aan Brussel. De verzendservice geeft voor Brussel een ander tarief en een andere vervoerder terug.

**Wat je doet:**

1. Voeg in Mockoon een tweede route toe op hetzelfde pad `/api/verzending`. Gebruik **Rules** om de route alleen te activeren als de query parameter `bestemming` gelijk is aan `Brussel`.

   Configureer de response als:

   ```json
   {
     "bestemming": "Brussel",
     "gewicht": 1.0,
     "tarief": 4.49,
     "vervoerder": "bpost"
   }
   ```

2. Schrijf in `ShippingClientIntegrationTests.cs` een test met `[Theory]` die beide bestemmingen test in één testmethode.

**Startcode:**

```csharp
[Theory]
[InlineData("Antwerpen", 2.5, 6.99, "DHL")]
[InlineData("Brussel",   1.0, ???,  "???")]
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
    // ... jouw code hier ...

    // Assert
    result.Should().NotBeNull();
    // controleer Tarief en Vervoerder
}
```

**Verwacht resultaat:**

```csharp
✓ GetShippingRateAsync_WithKnownDestination_ReturnsCorrectTarief(destination: "Antwerpen", ...)
✓ GetShippingRateAsync_WithKnownDestination_ReturnsCorrectTarief(destination: "Brussel", ...)
```

---

<h3 class="opdracht-titel">Opdracht</h3>

## Oefening 2: foutscenarios uitbreiden

**Leerdoel:** je leert verschillende HTTP-foutcodes testen en begrijpt wat `EnsureSuccessStatusCode` afhandelt.

**Moeilijkheidsgraad:** basis

**Situatie:** de demo testte een HTTP 500-fout. Externe services geven ook andere foutcodes terug: een 404 als de resource niet bestaat, een 503 als de service tijdelijk niet beschikbaar is.

**Wat je doet:**

1. Voeg in Mockoon twee extra routes toe:
   - Route A: query parameter `bestemming` = `ONBEKEND`, status 404, leeg body
   - Route B: query parameter `bestemming` = `OFFLINE`, status 503, leeg body

2. Schrijf voor elke route een aparte test die verifieert dat `GetShippingRateAsync` een `HttpRequestException` gooit.

3. Schrijf daarna één test met `[Theory]` die alle drie de foutbestemmingen (`FOUT`, `ONBEKEND`, `OFFLINE`) in één testmethode combineert.

**Verwacht resultaat:**

```csharp
✓ GetShippingRateAsync_WithErrorDestination_ThrowsHttpRequestException(destination: "FOUT")
✓ GetShippingRateAsync_WithErrorDestination_ThrowsHttpRequestException(destination: "ONBEKEND")
✓ GetShippingRateAsync_WithErrorDestination_ThrowsHttpRequestException(destination: "OFFLINE")
```

**Tip:** de drie routes geven een andere statuscode terug, maar `EnsureSuccessStatusCode` gooit voor alle drie een `HttpRequestException`. Dat maakt de test eenvoudiger dan je misschien verwacht.

---

<h3 class="opdracht-titel">Opdracht</h3>

## Oefening 3: timeout en latency

**Leerdoel:** je leert een timeout instellen op `HttpClient` en verifieert dat je code correct reageert als een externe dienst te traag is.

**Moeilijkheidsgraad:** gemiddeld

**Situatie:** de verzendservice is soms traag. ShopWave mag niet onbepaald wachten op een antwoord. Je moet testen dat de applicatie na 2 seconden opgeeft.

**Wat je doet:**

1. Stel in Mockoon een latency in van 5000 milliseconden op de bestaande route voor Antwerpen.

2. Schrijf een test die een `HttpClient` aanmaakt met `Timeout` van 2 seconden en verifieert dat `GetShippingRateAsync` een `TaskCanceledException` gooit.

3. Nadat de test slaagt: verwijder de latency in Mockoon (zet terug naar 0). Voer alle tests opnieuw uit en controleer dat de andere tests nog steeds slagen.

4. Beantwoord in een commentaar boven de test:
   - Waarom is het belangrijk om timeouts te testen?
   - Wat gooit `HttpClient` als de timeout verstrijkt: `HttpRequestException` of `TaskCanceledException`? Waarom?

**Startcode:**

```csharp
[Fact]
public async Task GetShippingRateAsync_WhenRequestTimesOut_ThrowsTaskCanceledException()
{
    // Arrange
    HttpClient httpClient = new HttpClient();
    httpClient.Timeout    = TimeSpan.FromSeconds(???);
    ShippingClient client = new ShippingClient(httpClient, MockoonBaseUrl);

    // Act
    Func<Task> act = async () =>
    {
        // ... jouw code hier ...
    };

    // Assert
    await act.Should().ThrowAsync<???>();

    httpClient.Dispose();
}
```

---

<h3 class="opdracht-titel">Opdracht</h3>

## Oefening 4: WireMock.Net (uitdaging)

**Leerdoel:** je leert een mock server configureren in C#-code zonder externe tool, zodat de tests volledig zelfstandig draaien.

**Moeilijkheidsgraad:** uitdaging

**Situatie:** de tests uit de demo en oefeningen 1 t/m 3 vereisen dat Mockoon handmatig opgestart is. In een CI/CD-pipeline is dat niet praktisch. WireMock.Net lost dat op: de mock server start en stopt automatisch als onderdeel van het testproces.

**Wat je doet:**

1. Installeer het NuGet-pakket `WireMock.Net` in `ShopWave.Tests`.

2. Maak een nieuwe testklasse aan: `ShopWave.Tests/ShippingClientWireMockTests.cs`.

3. Schrijf in die klasse een test die:
   - Een `WireMockServer` start op een willekeurige vrije poort
   - Een route configureert die hetzelfde JSON-antwoord teruggeeft als de Mockoon-route voor Antwerpen
   - `GetShippingRateAsync` aanroept met de URL van de WireMock-server
   - Verifieert dat het tarief 6.99 is en de vervoerder "DHL"
   - De server stopt na afloop

4. Schrijf daarna een tweede test die een foutroute configureert (status 500) en verifieert dat een `HttpRequestException` gegooid wordt.

**Startcode:**

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
            WireMockServer server = WireMockServer.Start(); // start op willekeurige vrije poort

            server.Given(
                Request.Create()
                       .WithPath("/api/verzending")
                       .UsingGet())
            .RespondWith(
                Response.Create()
                        .WithStatusCode(200)
                        .WithBodyAsJson(new { bestemming = "Antwerpen", gewicht = 2.5, tarief = ???, vervoerder = "???" }));

            HttpClient     httpClient = new HttpClient();
            ShippingClient client     = new ShippingClient(httpClient, server.Url!);

            // Act
            // ... jouw code hier ...

            // Assert
            // ... jouw code hier ...

            httpClient.Dispose();
            server.Stop();
        }
    }
}
```

**Tip:** `server.Url` geeft de basis-URL terug van de WireMock-server, inclusief het poortnummer. Je geeft die URL mee aan `ShippingClient` via de constructor.

**Verwacht resultaat:**

```csharp
✓ GetShippingRateAsync_WithWireMock_ReturnsTarief
✓ GetShippingRateAsync_WithWireMock_WhenServerReturns500_ThrowsHttpRequestException
```

De tests draaien zonder dat Mockoon opgestart is.

---

<h3 class="opdracht-titel">Opdracht</h3>

## Oefening 5: reflectie

**Leerdoel:** je legt de verbanden tussen de verschillende testvormen die je in deze cursus geleerd hebt.

**Moeilijkheidsgraad:** basis

Beantwoord de volgende vragen. Schrijf je antwoorden op papier of in een tekstbestand.

1. In les 1 mockte je `IPaymentGateway` met Moq. In deze les gebruik je Mockoon als mock server. Wat is het fundamentele verschil tussen de twee aanpakken? Wanneer gebruik je welke?

2. Je hebt nu drie soorten tests voor ShopWave: unit tests (les 1), integration tests zonder HTTP (les 5) en integration tests met een mock server (les 10). Geef voor elk type een concreet voorbeeld van een bug die dat type wel detecteert maar de andere twee niet.

3. Stel dat de echte verzendservice morgen zijn JSON-structuur wijzigt: `"tarief"` wordt `"prijs"`. Welke test faalt als eerste? Leg uit waarom.

4. De tests uit oefening 1 tot 3 vereisen dat Mockoon draait. De tests uit oefening 4 niet. Welk type test is meer geschikt voor een CI/CD-pipeline? Waarom?
