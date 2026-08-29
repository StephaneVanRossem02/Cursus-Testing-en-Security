---
title: "Les 10: Integration Testing (Mockoon)"
sidebar_label: "Overzicht"
---

# Les 10: Integration Testing (Mockoon)

In les 5 testte je de samenwerking tussen eigen klassen: `CartService`, `CouponService` en `OrderService` werkten samen zonder mocks. Dat werkte goed zolang alle code in je eigen project stond.

ShopWave roept in productie ook externe diensten aan via HTTP: een verzendservice die het leveringstarief berekent, een betaalprovider die transacties verwerkt. Die diensten kan je niet aanroepen in elke testrun. Ze kunnen traag zijn, betalend, of gewoon niet beschikbaar in een CI-omgeving.

In deze les leer je hoe je dat probleem oplost met een **mock server**: een lokale HTTP-server die je zelf configureert om vaste antwoorden terug te geven.

## Leerdoelen

Na deze les kan je:

- uitleggen waarom `HttpClient` moeilijk te mocken is met Moq en wat een mock server oplost
- Mockoon configureren met routes, responses en latency
- `ShippingClient` testbaar maken door de basis-URL via de constructor mee te geven
- integration tests schrijven voor HTTP-clients met xUnit en FluentAssertions
- async exceptions en timeouts testen
- WireMock.Net gebruiken als in-process alternatief voor Mockoon

## Wat heb je nodig?

**Installeer dit voor je begint:**
- Visual Studio 2022 (Community of hoger)
- .NET 8 SDK
- Mockoon: download via [mockoon.com](https://mockoon.com/#download) (Windows, macOS of Linux)
- NuGet-pakket in het testproject: `WireMock.Net` (voor oefening 4)

De bestaande NuGet-pakketten `xunit`, `Moq` en `FluentAssertions` heb je al uit les 1.

## Opbouw van deze les

| Pagina | Wat staat er? |
|--------|--------------|
| [Theorie](theorie) | Uitleg van alle concepten met voorbeelden |
| [Oefeningen](oefeningen) | Zelf aan de slag met ShopWave |
| [Oplossingen](oplossingen) | Volledige uitwerking met toelichting |

**Werkwijze:** lees eerst de theorie door. Werk daarna de oefeningen zonder naar de oplossingen te kijken. Controleer achteraf je werk en lees de toelichting, ook als je het juist had.
