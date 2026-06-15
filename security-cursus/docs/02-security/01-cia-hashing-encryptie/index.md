---
title: "Les 2: CIA, Hashing en Encryptie"
sidebar_label: "Overzicht"
---

# Les 2: CIA, Hashing en Encryptie

ShopWave bewaart gegevens van duizenden klanten: e-mailadressen, wachtwoorden, bestelgeschiedenissen en betalingsgegevens. Wat gebeurt er als de database gestolen wordt? Wat als een aanvaller een bestelbedrag aanpast? Wat als de webshop tijdens een drukke promotieperiode plat gaat?

Dit zijn geen hypothetische scenario's. Datalekken, fraude en aanvallen op webshops komen dagelijks voor. Als ontwikkelaar ben jij de eerste verdedigingslinie.

In deze les leer je de drie pijlers van informatiebeveiliging kennen en pas je ze direct toe op ShopWave. Je beveiligt wachtwoorden met BCrypt en versleutelt gevoelige ordergegevens met AES.

## Leerdoelen

Na deze les kan je:

- het CIA-model uitleggen en toepassen op concrete situaties in ShopWave
- het verschil uitleggen tussen hashing en encryptie, en de juiste keuze maken per situatie
- wachtwoorden correct opslaan met BCrypt (inclusief automatische salt)
- uitleggen waarom SHA-256 niet geschikt is voor wachtwoorden
- gevoelige data versleutelen en ontsleutelen met AES en een willekeurige IV
- unit tests schrijven voor beveiligingsklassen

## Wat heb je nodig?

**Installeer dit voor je begint:**
- Visual Studio 2022 (Community of hoger)
- .NET 8 SDK
- NuGet-pakket in het hoofdproject `ShopWave`: `BCrypt.Net-Next`

Installeer via **Tools > NuGet Package Manager > Manage NuGet Packages for Solution** en zoek op `BCrypt.Net-Next`. AES is ingebouwd in .NET via `System.Security.Cryptography`.

## Opbouw van deze les

| Pagina | Wat staat er? |
|--------|--------------|
| [Theorie](theorie) | Uitleg van alle concepten met voorbeelden |
| [Oefeningen](oefeningen) | Zelf aan de slag met ShopWave |
| [Oplossingen](oplossingen) | Volledige uitwerking met toelichting |

**Werkwijze:** lees eerst de theorie door. Werk daarna de oefeningen zonder naar de oplossingen te kijken. Controleer achteraf je werk en lees de toelichting, ook als je het juist had.
