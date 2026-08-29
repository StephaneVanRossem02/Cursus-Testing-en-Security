---
title: "Les 4: 2FA, Handtekeningen en X.509"
sidebar_label: "Overzicht"
---

# Les 4: 2FA, Handtekeningen en X.509

In les 2 beveiligde je wachtwoorden met BCrypt en gevoelige data met AES. Dat was een grote stap vooruit: als de database van ShopWave gestolen wordt, ziet een aanvaller geen plain-text wachtwoorden meer.

Maar een wachtwoord alleen is niet genoeg. Wachtwoorden lekken op tientallen manieren: via phishing, via een ander gehackt platform waar de klant hetzelfde wachtwoord gebruikt, of via een eenvoudige brute-force-aanval op een zwak wachtwoord. Als een aanvaller het wachtwoord heeft, heeft hij volledige toegang.

In deze les voeg je een tweede verificatielaag toe aan de loginflow van ShopWave. Daarna leer je hoe ShopWave de integriteit van orderbevestigingen kan garanderen: als een aanvaller het bestelbedrag probeert te wijzigen, detecteert het systeem dat onmiddellijk.

## Leerdoelen

Na deze les kan je:

- uitleggen waarom een wachtwoord alleen onvoldoende bescherming biedt en wat 2FA oplost
- de drie factoren van authenticatie benoemen met concrete voorbeelden
- uitleggen hoe TOTP werkt en waarom codes na 30 seconden verlopen
- het verschil uitleggen tussen encryptie en een digitale handtekening
- een digitale handtekening zetten en verifiëren met RSA in C#
- een self-signed X.509-certificaat aanmaken en gebruiken in tests
- de concepten uit deze les koppelen aan de juiste CIA-pijler

## Wat heb je nodig?

**Installeer dit voor je begint:**
- Visual Studio 2022 (Community of hoger)
- .NET 8 SDK

Alle benodigde klassen zijn ingebouwd in .NET via `System.Security.Cryptography`. Geen extra NuGet-pakketten nodig voor deze les.

Je werkt verder in de bestaande ShopWave-solution en de map `ShopWave/Security/` die je in les 2 aangemaakt hebt.

## Opbouw van deze les

| Pagina | Wat staat er? |
|--------|--------------|
| [Theorie](theorie) | Uitleg van alle concepten met voorbeelden |
| [Oefeningen](oefeningen) | Zelf aan de slag met ShopWave |
| [Oplossingen](oplossingen) | Volledige uitwerking met toelichting |

**Werkwijze:** lees eerst de theorie door. Werk daarna de oefeningen zonder naar de oplossingen te kijken. Controleer achteraf je werk en lees de toelichting, ook als je het juist had.
