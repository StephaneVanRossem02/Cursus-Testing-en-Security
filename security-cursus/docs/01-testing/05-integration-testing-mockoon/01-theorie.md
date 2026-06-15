---
title: "Les 10: Theorie - Integration Testing (Mockoon)"
sidebar_label: "Theorie"
---

# Theorie: Integration Testing (Mockoon)

## 1. Waarom is dit nodig?

In les 5 schreven we integration tests voor de bestelflow van ShopWave. We gebruikten echte klassen: `CartService`, `CouponService` en `OrderService`. Alleen de betaalgateway was een mock, omdat dat een externe dienst is.

Maar "externe dienst" betekent in les 5 nog iets abstracts: een interface die we mocken. In de praktijk roept ShopWave externe diensten aan via HTTP. Dat verandert het probleem fundamenteel.

Stel dat `ShippingClient` het leveringstarief ophaalt bij een externe verzendservice:

```csharp
public async Task<ShippingResponse> GetShippingRateAsync(string destination, double weight)
{
    string url = $"https://verzendservice.be/api/tarief?bestemming={destination}&gewicht={weight}";
    HttpResponseMessage response = await _httpClient.GetAsync(url);
    string json = await response.Content.ReadAsStringAsync();
    return JsonSerializer.Deserialize<ShippingResponse>(json)!;
}
```

Wat gaat er mis als je deze code wil testen?

**Probleem 1: je kan `HttpClient` niet mocken met Moq.** `HttpClient` is een concrete klasse, geen interface. Je kan wel `HttpMessageHandler` mocken, maar dat leidt tot ingewikkelde en fragiele testcode. Bovendien test je dan niet de echte HTTP-call.

**Probleem 2: de externe service is niet betrouwbaar.** Ze kan traag zijn, offline zijn, of kosten aanrekenen per aanroep. In een CI-omgeving draait je testpipeline soms tientallen keren per dag. Elke testrun die de echte verzendservice aanroept, is een risico.

**Probleem 3: je kan specifieke foutscenarios niet forceren.** Hoe test je dat je code correct reageert op een HTTP 500-fout? Of op een timeout? De echte service gooit die fouten niet op bestelling.

De oplossing is een **mock server**: een lokale HTTP-server die je zelf configureert. Wat Moq doet voor een interface in geheugen, doet een mock server voor een externe HTTP-dienst.

**Mini-controle:** waarom is `HttpMessageHandler` mocken geen goede oplossing? Je test dan niet of de URL correct opgebouwd is, of de headers kloppen, of de JSON-parsing werkt. Je vervangt de kern van het probleem door een nep-versie.

---

## 2. Wat is een mock server?

Een **mock server** is een lokale HTTP-server die jij configureert: "als je een GET-request krijgt op `/api/tarief`, geef dan dit JSON-antwoord terug."

```csharp
Moq:         IPaymentGateway (interface)  →  nep-object in geheugen
Mock server: https://verzendservice.be   →  nep-server op localhost
```

De `ShippingClient` maakt een echte HTTP-call. In productie gaat die call naar de echte verzendservice. In tests gaat die call naar de mock server op je eigen machine.

Dit geeft je het beste van beide werelden:

- Je test de **echte `ShippingClient`-code**, inclusief URL-opbouw, HTTP-call en JSON-parsing
- Je bent **niet afhankelijk van de externe service**: die hoeft niet online te zijn
- Je kan **specifieke scenario's forceren**: een HTTP 500, een timeout, een leeg antwoord

**Mini-controle:** wat is het verschil tussen een mock server en een Moq-mock? Een Moq-mock vervangt een C#-interface in het geheugen van je testproces. Een mock server simuleert een volledige HTTP-server via een echte TCP-verbinding op localhost. De `ShippingClient` merkt het verschil niet.

---

## 3. Mockoon en WireMock.Net

Er zijn twee tools die we in deze les gebruiken.

**Mockoon** is een visuele tool met een grafische interface. Je maakt routes aan via knoppen en invoervelden, zonder code te schrijven. Ideaal om snel iets te proberen en om de concepten te leren.

**WireMock.Net** is een NuGet-pakket. De mock server draait in hetzelfde proces als je tests. Je configureert de routes in C#-code. Geen aparte applicatie nodig.

| Eigenschap | Mockoon | WireMock.Net |
|-----------|---------|-------------|
| Interface | Visueel (GUI) | Code (C#) |
| Opstarten | Handmatig, apart proces | Automatisch, in testproces |
| Geschikt voor | Demo's, handmatig verkennen | Geautomatiseerde tests, CI/CD |
| Installatie | Aparte applicatie downloaden | NuGet-pakket |

We starten met Mockoon om het principe te begrijpen. In oefening 4 schrijf je dezelfde tests opnieuw met WireMock.Net.

**Mini-controle:** waarom is WireMock.Net beter geschikt voor een CI/CD-pipeline dan Mockoon? WireMock.Net start en stopt automatisch als onderdeel van het testproces. Mockoon moet handmatig opgestart worden door een persoon.

---

## 4. Testbaar ontwerp: baseUrl via de constructor

Voordat we iets testen, moeten we de `ShippingClient` testbaar maken. De sleutel is de basis-URL niet hardcoded in de klasse te zetten.

Maak `ShopWave/ShippingResponse.cs` aan:

```csharp
namespace ShopWave
{
    public class ShippingResponse
    {
        public string Bestemming { get; set; } = string.Empty;
        public double Gewicht    { get; set; }
        public double Tarief     { get; set; }
        public string Vervoerder { get; set; } = string.Empty;
    }
}
```

Maak `ShopWave/ShippingClient.cs` aan:

```csharp
using System.Net.Http;
using System.Text.Json;

namespace ShopWave
{
    public class ShippingClient
    {
        private readonly HttpClient _httpClient;
        private readonly string     _baseUrl;

        public ShippingClient(HttpClient httpClient, string baseUrl)
        {
            _httpClient = httpClient;
            _baseUrl    = baseUrl;
        }

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

De `baseUrl` komt via de constructor. Dat is een bewuste ontwerpbeslissing:

- In productie: `new ShippingClient(httpClient, "https://verzendservice.be")`
- In tests: `new ShippingClient(httpClient, "http://localhost:3001")`

Dezelfde code, ander eindpunt. Geen enkele wijziging in de klasse zelf.

`EnsureSuccessStatusCode()` gooit een `HttpRequestException` als de HTTP-statuscode geen 2xx is. Zonder die aanroep zou de code stilletjes doorgaan met een lege of foutieve response en later crashen met een cryptische null-referentiefout.

`PropertyNameCaseInsensitive = true` zorgt dat het JSON-veld `"tarief"` correct gemapt wordt op de C#-property `Tarief`, ongeacht of de API kleine letters of hoofdletters gebruikt.

**Mini-controle:** wat zou er fout gaan als de `baseUrl` hardcoded stond in `ShippingClient`? Je kan de URL niet vervangen in tests. Elke testrun roept de echte externe service aan. Dat is traag, afhankelijk en soms betalend.

---

## 5. Async testen: exceptions en timeouts

Tests voor async methoden schrijf je met `async Task` in plaats van `void`. Je wacht op het resultaat met `await`.

Als je een exception verwacht van een async methode, gebruik je een `Func<Task>`:

```csharp
Func<Task> act = async () =>
{
    await client.GetShippingRateAsync(destination: "FOUT", weight: 1.0);
};

await act.Should().ThrowAsync<HttpRequestException>();
```

Je kan niet schrijven `act.Should().Throw<HttpRequestException>()` voor async methoden. De synchrone `Throw` vangt alleen synchrone exceptions. Gebruik altijd `ThrowAsync` voor async code.

**Timeouts testen** doe je door de `Timeout`-property van `HttpClient` in te stellen en de mock server langzamer te laten antwoorden dan de timeout:

```csharp
HttpClient httpClient = new HttpClient();
httpClient.Timeout    = TimeSpan.FromSeconds(2);
```

Als de mock server pas na 5 seconden antwoordt maar de timeout 2 seconden is, gooit `HttpClient` een `TaskCanceledException`. Die test je met:

```csharp
await act.Should().ThrowAsync<TaskCanceledException>();
```

Timeouts zijn belangrijk om te testen. Zonder timeout blokkeert de applicatie onbepaald als de externe service niet reageert. Bij genoeg gelijktijdige requests raken alle beschikbare threads op en reageert de service niet meer voor andere gebruikers.

**Mini-controle:** wat is het verschil tussen `ThrowAsync<HttpRequestException>` en `ThrowAsync<TaskCanceledException>`? `HttpRequestException` gooit `EnsureSuccessStatusCode` bij een 4xx of 5xx response. `TaskCanceledException` gooit `HttpClient` als de timeout verstrijkt voordat de server antwoordt.

---

## 6. Security: waarom mock servers ook voor beveiliging belangrijk zijn

Mock servers zijn niet alleen handig voor snelheid of beschikbaarheid. Ze spelen ook een rol in beveiligingstests.

**Onbetrouwbare externe responses.** Als ShopWave de respons van een externe API vertrouwt zonder validatie, is dat een aanvalsvector. Wat als de externe service gehackt is en kwaadaardige data teruggeeft? Een mock server laat je testen wat je code doet met een onverwacht antwoord: een negatief tarief, een lege string, een getal waar tekst verwacht wordt.

**Authenticatiefouten simuleren.** Externe API's gebruiken vaak API-sleutels of OAuth-tokens. Wat doet je code als de externe service een 401 Unauthorized teruggeeft? Of een 403 Forbidden? Met een mock server kan je die scenario's op bestelling simuleren.

**Datalekkage voorkomen.** Stel dat je applicatie gevoelige gegevens (klantnaam, adres) meestuurt in de URL of headers naar een externe dienst. Met een mock server kan je de ontvangen request inspecteren en verifiëren dat geen ongewenste data meegestuurd wordt.

```csharp
// WireMock.Net: verifieer dat de request geen gevoelige data bevat
server.FindLogEntries(
    Request.Create().WithPath("/api/verzending"))
    .Should().AllSatisfy(entry =>
        entry.RequestMessage.Url.Should().NotContain("wachtwoord"));
```

---

## 7. Demo: ShippingClient stap voor stap

We bouwen de volledige flow in zes stappen. Mockoon draait als mock server. We schrijven een echte integration test die de URL-opbouw, HTTP-call en JSON-parsing verifieert.

---

### Stap 1: Mockoon installeren en opstarten

Download Mockoon via [mockoon.com](https://mockoon.com/#download) en installeer het.

Open Mockoon. Je ziet een lege omgeving. Maak een nieuwe mock environment aan:

1. Klik op het `+`-icoontje naast "Mock local server"
2. Geef de naam `ShopWave Verzendservice`
3. Stel poort `3001` in

Voeg een route toe:

1. Klik op `Add route`
2. Stel in:
   - **Method:** GET
   - **Path:** `/api/verzending`
   - **Status:** 200
3. Plak in het **Body**-veld:

```json
{
  "bestemming": "Antwerpen",
  "gewicht": 2.5,
  "tarief": 6.99,
  "vervoerder": "DHL"
}
```

Klik op de groene **play**-knop. Mockoon draait nu op poort 3001.

Wat je ziet: de play-knop wordt groen en toont "Running". Navigeer in je browser naar `http://localhost:3001/api/verzending`. Je krijgt het JSON-antwoord dat je net configureerde.

---

### Stap 2: ShippingResponse en ShippingClient aanmaken

Maak de twee bestanden aan zoals beschreven in sectie 4. Bouw de solution.

Wat je ziet:

```csharp
Build succeeded.
```

Geen tests nog, maar de klassen compileren. De `baseUrl` in de constructor is de sleutel: in de volgende stap geven we de Mockoon-URL mee.

---

### Stap 3: eerste integration test schrijven

Maak `ShopWave.Tests/ShippingClientIntegrationTests.cs` aan:

```csharp
using FluentAssertions;
using System.Net.Http;
using ShopWave;

namespace ShopWave.Tests
{
    public class ShippingClientIntegrationTests
    {
        private const string MockoonBaseUrl = "http://localhost:3001";
    }
}
```

De URL van Mockoon als constante. Als je Mockoon op een andere poort draait, pas je alleen die constante aan.

Voeg de eerste test toe:

```csharp
        [Fact]
        public async Task GetShippingRateAsync_WithValidRequest_ReturnsTarief()
        {
            // Arrange
            HttpClient     httpClient = new HttpClient();
            ShippingClient client     = new ShippingClient(httpClient, MockoonBaseUrl);

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

Voer de test uit. Zorg dat Mockoon draait.

Wat je ziet:

```csharp
✓ GetShippingRateAsync_WithValidRequest_ReturnsTarief
```

Dit is een echte HTTP-call. `ShippingClient` bouwt de URL op, doet een `GetAsync`, parseert de JSON en geeft het resultaat terug. Alles echt, niets gemockt in geheugen.

---

### Stap 4: foutscenario testen

Voeg in Mockoon een tweede route toe:

- **Method:** GET
- **Path:** `/api/verzending`
- Klik op **Rules** en voeg een regel toe: query parameter `bestemming` is gelijk aan `FOUT`
- **Status:** 500
- **Body:** leeg

Voeg de tweede test toe:

```csharp
        [Fact]
        public async Task GetShippingRateAsync_WhenServerReturns500_ThrowsHttpRequestException()
        {
            // Arrange
            HttpClient     httpClient = new HttpClient();
            ShippingClient client     = new ShippingClient(httpClient, MockoonBaseUrl);

            // Act
            Func<Task> act = async () =>
            {
                await client.GetShippingRateAsync(destination: "FOUT", weight: 1.0);
            };

            // Assert
            await act.Should().ThrowAsync<HttpRequestException>();

            httpClient.Dispose();
        }
```

Voer alle tests uit.

Wat je ziet:

```csharp
✓ GetShippingRateAsync_WithValidRequest_ReturnsTarief
✓ GetShippingRateAsync_WhenServerReturns500_ThrowsHttpRequestException
```

`EnsureSuccessStatusCode()` in `ShippingClient` doet het werk: bij een 500-response gooit die methode een `HttpRequestException`. De test vangt die exception op.

---

### Stap 5: timeout testen

Stel in Mockoon een latency in van 5000 milliseconden op de bestaande route voor Antwerpen. Klik op de route, zoek het veld **Latency (ms)** en vul `5000` in.

Voeg de derde test toe:

```csharp
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

Voer de test uit.

Wat je ziet:

```csharp
✓ GetShippingRateAsync_WhenRequestTimesOut_ThrowsTaskCanceledException
```

Mockoon antwoordt pas na 5 seconden. De `HttpClient` wacht maximaal 2 seconden. Na 2 seconden annuleert hij de request en gooit een `TaskCanceledException`. Verwijder daarna de latency in Mockoon (zet terug op 0), anders falen de andere tests.

---

### Stap 6: alle tests samen

Voer alle tests uit.

Wat je ziet:

```csharp
✓ GetShippingRateAsync_WithValidRequest_ReturnsTarief
✓ GetShippingRateAsync_WhenServerReturns500_ThrowsHttpRequestException
✓ GetShippingRateAsync_WhenRequestTimesOut_ThrowsTaskCanceledException
```

Drie integration tests. Alle drie testen de echte `ShippingClient`-code inclusief HTTP-call. Geen enkel nep-object in het geheugen van het testproces.

Als Mockoon niet draait, falen de tests met een `HttpRequestException`. Dat is correct: een integration test die afhankelijk is van een externe service, moet falen als die service niet beschikbaar is.

---

## 8. Samenvatting

| Concept | Wat je moet onthouden |
|--------|-----------------------|
| Mock server | Lokale HTTP-server die vaste antwoorden geeft; vervangt een externe service tijdens tests |
| Mockoon | Visuele tool voor routes en responses; handmatig opstarten |
| WireMock.Net | NuGet-pakket; draait in testproces; geschikt voor CI/CD |
| `baseUrl` via constructor | Maakt de klasse testbaar zonder code te wijzigen |
| `EnsureSuccessStatusCode` | Gooit `HttpRequestException` bij 4xx of 5xx; voorkomt stille fouten |
| `ThrowAsync` | Gebruik je voor async exceptions in FluentAssertions |
| `TaskCanceledException` | Gooit `HttpClient` als de timeout verstrijkt |
| Wanneer mock server? | Als je de echte HTTP-call wil testen: URL-opbouw, serialisatie, foutcodes |
| Wanneer Moq? | Als je de logica van één klasse wil testen in isolatie |
