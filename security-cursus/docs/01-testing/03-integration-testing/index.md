---
title: "Les 5: Integration Testing"
sidebar_label: "Overzicht"
---

# Les 5: Integration Testing

In les 1 testten we klassen in isolatie met mocks. In les 3 bouwden we klassen stap voor stap via TDD. In deze les stellen we de vraag die beide lessen openlaten: werken die klassen ook correct samen?

We bouwen verder op de ShopWave-klassen uit les 1 en 3. `OrderService`, `CartService` en `CouponService` worden voor het eerst samen getest, zonder mocks voor eigen code.

## Leerdoelen

Na deze les kan je:

- uitleggen wat een integration test is en hoe die verschilt van een unit test
- de testpiramide beschrijven en situeren welk testtype je wanneer gebruikt
- een integration test schrijven in C# met xUnit waarbij echte klassen samenwerken
- de callback-techniek toepassen om niet-deterministische waarden op te vangen in een test
- beredeneren wanneer je een unit test schrijft en wanneer een integration test

## Wat heb je nodig?

**Installeer dit voor je begint:**
- De ShopWave-klassen uit les 1 en les 3 (`OrderService`, `CartService`, `CouponService`, `DiscountCalculator`)
- NuGet-pakketten: `xunit`, `Moq`, `FluentAssertions`

## Opbouw van deze les

| Pagina | Wat staat er? |
|--------|--------------|
| [Theorie](theorie) | Uitleg van integration testing met een volledige demo |
| [Oefeningen](oefeningen) | Zelf klassen samen testen zonder mocks |
| [Oplossingen](oplossingen) | Volledige uitwerking met toelichting |

**Werkwijze:** lees eerst de theorie inclusief de demo volledig door. Werk daarna de oefeningen zonder naar de oplossingen te kijken.
