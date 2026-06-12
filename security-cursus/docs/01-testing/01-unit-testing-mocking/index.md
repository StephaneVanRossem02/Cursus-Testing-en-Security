---
title: "Les 1: Unit Testing en Mocking"
sidebar_label: "Overzicht"
---

# Les 1: Unit Testing en Mocking

In deze cursus bouw je stap voor stap een volledige test-suite voor **ShopWave**, een fictieve webshop. Klanten kunnen er producten bekijken, bestellen en betalen. Elke les voeg je een nieuw stukje toe of test je een bestaand onderdeel grondiger.

Een bug die je vindt terwijl je aan het programmeren bent, kost je vijf minuten. Dezelfde bug die een klant vindt nadat de applicatie live staat, kost uren aan herstelwerk, reputatieschade en soms geld. Automatische tests zijn je eerste verdedigingslinie.

## Leerdoelen

Na deze les kan je:

- uitleggen wat een unit test is en waarom je er een schrijft
- een test structureren volgens het Arrange-Act-Assert patroon
- met **ZOMBIES** bepalen welke testgevallen je schrijft
- tests schrijven met **xUnit** en resultaten controleren met **FluentAssertions**
- een klasse testbaar maken via **Dependency Injection**
- een nep-afhankelijkheid bouwen met **Moq** en het gedrag ervan controleren met `Verify`

## Wat heb je nodig?

**Installeer dit voor je begint:**
- Visual Studio 2022 (Community of hoger)
- .NET 8 SDK
- NuGet-pakketten in het testproject: `xunit`, `Moq`, `FluentAssertions`

De NuGet-pakketten installeer je via **Tools > NuGet Package Manager > Manage NuGet Packages for Solution**.

## Opbouw van deze les

| Pagina | Wat staat er? |
|--------|--------------|
| [Theorie](theorie) | Uitleg van alle concepten met voorbeelden |
| [Oefeningen](oefeningen) | Zelf aan de slag met ShopWave |
| [Oplossingen](oplossingen) | Volledige uitwerking met toelichting |

**Werkwijze:** lees eerst de theorie door. Werk daarna de oefeningen zonder naar de oplossingen te kijken. Controleer achteraf je werk en lees de toelichting, ook als je het juist had.
