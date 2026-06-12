---
title: "Les 10: Integration Testing met HTTP-mocks (Mockoon) — Theorie"
sidebar_label: "Theorie"
---

# Les 10: Integration Testing met HTTP-mocks (Mockoon) — Theorie

## Het probleem: externe HTTP-diensten in tests

ShopWave roept externe API's aan (bv. verzendservice). Die diensten kunnen traag, onbetrouwbaar of betalend zijn. Je kan ze niet aanroepen in elke testrun — ze zijn ook niet altijd beschikbaar in een CI-omgeving.

---

## De oplossing: mock HTTP-servers

Een **mock HTTP-server** simuleert een externe API lokaal. Je configureert welke requests ze ontvangt en welke responses ze teruggeeft.

| Tool          | Type                  | Gebruik                                              |
|---------------|-----------------------|------------------------------------------------------|
| **Mockoon**   | GUI-tool              | Snel routes configureren; ideaal voor demo's en handmatige tests |
| **WireMock.Net** | NuGet-bibliotheek  | Programmatische HTTP-mocks in geautomatiseerde tests |

---

## ShippingClient — testbaar ontwerp

De `ShippingClient` ontvangt de basis-URL via de constructor:

```csharp
public class ShippingClient
{
    private readonly HttpClient httpClient;
    private readonly string     baseUrl;

    public ShippingClient(HttpClient httpClient, string baseUrl)
    {
        this.httpClient = httpClient;
        this.baseUrl    = baseUrl;
    }
}
```

- In productie: `"https://verzendservice.be"`
- In tests: `"http://localhost:3000"` (Mockoon)

Dit maakt de klasse testbaar zonder de URL hardcoded te hebben — een bewuste ontwerpbeslissing.

---

## EnsureSuccessStatusCode

`response.EnsureSuccessStatusCode()` gooit een `HttpRequestException` als de statuscode geen 2xx is. Dit vermijdt stille fouten waarbij je verder werkt met een mislukte response.

```csharp
HttpResponseMessage response = await this.httpClient.GetAsync(url);
response.EnsureSuccessStatusCode(); // gooit exception bij 4xx of 5xx
string json = await response.Content.ReadAsStringAsync();
```

---

## Async exception testen in xUnit + FluentAssertions

```csharp
Func<Task> act = GetShippingRateAsync;

async Task GetShippingRateAsync()
{
    await client.GetShippingRateAsync("Brussel", 5.0);
}

await act.Should().ThrowAsync<HttpRequestException>();
```

---

## Timeout testen

```csharp
HttpClient httpClient = new HttpClient();
httpClient.Timeout    = TimeSpan.FromSeconds(2);

// Mockoon-route met 5 seconden latency → TaskCanceledException na 2 seconden
Func<Task> act = GetShippingRateAsync;
await act.Should().ThrowAsync<TaskCanceledException>();
```

---

## Moq versus Mockoon

| Aspect                  | Moq                                           | Mockoon                                      |
|-------------------------|-----------------------------------------------|----------------------------------------------|
| Wat wordt gesimuleerd?  | C#-interfaces en abstracte klassen in geheugen | Volledige HTTP-server met echte TCP-verbinding |
| Waar draait het?        | In het testproces zelf                         | Als apart proces (localhost)                 |
| Test je...              | Logica in één klasse                           | De echte HTTP-aanroep (serialisatie, headers, URL) |
| Wanneer gebruiken?      | Unit tests                                     | Integration tests voor HTTP-clients          |
| Setup                   | Code                                           | GUI of JSON-configuratie                     |

---

## De testpiramide in een HTTP-context

```
         /\
        /  \   E2E — echte service in productie
       /----\
      /      \  Integration + Mockoon — nep-server, echte HTTP-call
     /--------\
    /          \ Unit + Moq — nep-interface in geheugen
   /____________\
```