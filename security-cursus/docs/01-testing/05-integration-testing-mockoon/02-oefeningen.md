---
title: "Les 10: Integration Testing met Mockoon — Oefeningen"
sidebar_label: "Oefeningen"
---

# Les 10: Integration Testing met Mockoon — Oefeningen

> **Code-afspraken:** geen top-level statements · altijd `{}` · max één `return` · geen `break`/`continue` · geen underscore-prefix op parameters · geen geneste klassen · geen ternary/null-conditional · geen tuples · `double` i.p.v. `decimal` · identifiers Engels · tekst Nederlands

---

## Oefening 1 — Twee bestemmingen via [Theory]

**Opgave:** Voeg een tweede route toe in Mockoon voor Brussel (tarief 4.49, vervoerder bpost). Test beide bestemmingen in één `[Theory]`.

**Mockoon-configuratie voor Brussel:**
- Path: `/api/verzending`
- Query parameter: `bestemming=Brussel`
- Response body:
```json
{ "bestemming": "Brussel", "tarief": 4.49, "vervoerder": "bpost" }
```

**ShippingClientIntegrationTests.cs**

```csharp
using FluentAssertions;
using ShopWave;

namespace ShopWave.Tests
{
    public class ShippingClientIntegrationTests
    {
        private const string MockoonBaseUrl = "http://localhost:3000";

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
    }
}
```

---

## Oefening 2 — Timeout simuleren

**Opgave:** Stel in Mockoon een latency in van 5000 ms. Configureer `HttpClient.Timeout = 2 seconden`. Verwacht een `TaskCanceledException`.

```csharp
[Fact]
public async Task GetShippingRateAsync_WhenRequestTimesOut_ThrowsException()
{
    // Arrange — Mockoon antwoordt pas na 5 seconden, timeout na 2
    HttpClient httpClient   = new HttpClient();
    httpClient.Timeout      = TimeSpan.FromSeconds(2);
    ShippingClient client   = new ShippingClient(httpClient, MockoonBaseUrl);

    // Act
    Func<Task> act = GetShippingRateAsync;

    async Task GetShippingRateAsync()
    {
        await client.GetShippingRateAsync(destination: "Antwerpen", weight: 1.0);
    }

    // Assert
    await act.Should().ThrowAsync<TaskCanceledException>();

    httpClient.Dispose();
}
```

**Antwoorden op de vragen:**
1. **Waarom timeouts testen?** Zonder timeout blokkeert de applicatie onbepaald als de externe service niet reageert — een enkel trage oproep kan de volledige service platleggen.
2. **In productie zonder timeout:** threads worden geblokkeerd; bij genoeg gelijktijdige requests raken alle beschikbare threads op → de service reageert niet meer voor andere gebruikers (denial of service door zichzelf).

---

## Oefening 4 — Vergelijking Moq versus Mockoon (analyse)

1. **Moq:** gebruik je wanneer je de logica van één klasse test in isolatie — je vervangt een interface door een nep-object in geheugen. **Mockoon:** gebruik je wanneer je de echte HTTP-aanroep wil testen — serialisatie, headers, URL, statuscodes.

2. **Moq test** of `ShippingClient` de juiste methode aanroept met de juiste parameters. **Mockoon test** of `ShippingClient` ook echt een correcte HTTP-request opbouwt en de response correct parseert.

3. **`baseUrl` via constructor:** maakt de klasse testbaar zonder code te wijzigen. Met een hardcoded URL zou je altijd de echte service aanroepen — dat is traag, afhankelijk en soms betalend.

4. **Welke test faalt eerst als de JSON-structuur wijzigt?** De integratietest met Mockoon — die test de echte deserialisatie van de JSON-response. Een Moq-test maakt de response zelf aan en raakt nooit de JSON-parsing.

5. **Integratietest als unit test:** ja, technisch mogelijk met Moq op `HttpMessageHandler`. Maar je verliest de garantie dat de HTTP-aanroep correct opgebouwd is — Mockoon test de volledige keten inclusief transport.