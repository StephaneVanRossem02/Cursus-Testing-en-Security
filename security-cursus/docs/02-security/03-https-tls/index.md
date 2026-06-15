---
title: "Les 6: HTTPS en TLS"
sidebar_label: "Overzicht"
---

# Les 6: HTTPS en TLS

In les 2 leerde je wachtwoorden hashen met BCrypt en data versleutelen met AES. In les 4 voegde je 2FA toe en leerde je orders digitaal ondertekenen. ShopWave heeft een solide beveiligingsbasis.

Maar er is nog een groot gat. Al die beveiliging beschermt de data die je opslaat. Het beschermt niet de data die onderweg is. Op dit moment stuurt ShopWave wachtwoorden, tokens en orderdata als leesbare tekst over het netwerk. Iedereen die het netwerk kan afluisteren, open wifi, een internetprovider, een aanvaller op hetzelfde netwerk, kan alles meelezen en zelfs aanpassen.

In deze les voegen we HTTPS toe aan ShopWave. We begrijpen precies wat er technisch achter de schermen gebeurt en zien hoe de concepten uit les 2 en 4 samenkomen in het TLS-protocol.

## Leerdoelen

Na deze les kan je:

- uitleggen wat een aanvaller ziet als hij onbeveiligd HTTP-verkeer onderschept
- de drie garanties van HTTPS koppelen aan de juiste CIA-pijler
- de stappen van de TLS-handshake in de juiste volgorde zetten en per stap uitleggen waarvoor RSA en AES gebruikt worden
- het verschil uitleggen tussen een self-signed certificaat en een certificaat van een Certificate Authority
- een ASP.NET Core Minimal API configureren om op HTTPS te draaien
- de TLS-handshake simuleren in C# met `RSA.Encrypt`, `RSA.Decrypt` en `AesEncryptor`
- uitleggen wat forward secrecy is en waarom TLS 1.3 de voorkeur heeft

## Wat heb je nodig?

- Visual Studio 2022 (Community of hoger)
- .NET 8 SDK
- De ShopWave-solution uit les 2 en 4 met de klassen `AesEncryptor`, `CertificateHelper` en `OrderSigner`

Geen extra NuGet-pakketten nodig voor de kern van deze les. De demo werkt met ingebouwde .NET-klassen en de klassen die je al gebouwd hebt.

## Opbouw van deze les

| Pagina | Wat staat er? |
|--------|--------------|
| [Theorie](theorie) | Uitleg van alle concepten met voorbeelden |
| [Oefeningen](oefeningen) | Zelf aan de slag met ShopWave |
| [Oplossingen](oplossingen) | Volledige uitwerking met toelichting |

**Werkwijze:** lees eerst de theorie door. Werk daarna de oefeningen zonder naar de oplossingen te kijken. Controleer achteraf je werk en lees de toelichting, ook als je het juist had.
