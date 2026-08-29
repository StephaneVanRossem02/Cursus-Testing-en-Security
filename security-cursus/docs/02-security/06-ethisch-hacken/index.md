---
title: "Les 11: Ethisch Hacken"
sidebar_label: "Overzicht"
---

# Les 11: Ethisch Hacken

## ShopWave

Tien lessen lang heb je ShopWave gebouwd en beveiligd. Wachtwoorden worden gehasht met BCrypt. 2FA beschermt de loginflow. Orders worden digitaal ondertekend. De API draait op HTTPS. JWT bewaakt de endpoints. SQL Injection, XSS en misconfiguraties zijn gefixed. Maar hoe weet je of al die maatregelen ook echt werken?

**Ethisch hacken**, ook wel penetration testing of pentesting, is het gecontroleerd aanvallen van een systeem met toestemming van de eigenaar. Het doel is kwetsbaarheden vinden voor een echte aanvaller dat doet. In deze les draai je de rollen om: jij bent de aanvaller op je eigen systeem.

**Wettelijk kader.** Ethisch hacken is enkel legaal met expliciete toestemming van de eigenaar van het systeem. In België valt ongeautoriseerd hacken onder artikel 550bis van het Strafwetboek en de Europese NIS2-richtlijn. In deze les werk je uitsluitend op je eigen lokale ShopWave-omgeving. Gebruik deze technieken nooit op externe systemen zonder schriftelijke toestemming.

---

## Leerdoelen

Na deze les kan je:

1. uitleggen wat ethisch hacken is en waarom het wettelijk kader niet onderhandelbaar is
2. de vijf fasen van een professionele pentest benoemen en toepassen
3. JWT-tokens manipuleren en uitleggen waarom de aanval mislukt dankzij signature-validatie
4. een brute-force aanval simuleren en verifiëren dat rate limiting werkt
5. het verschil aantonen tussen een developer exception page in development en in productie
6. CORS-headers inspecteren via curl en de configuratie beoordelen
7. een `PentestReport`-klasse implementeren die bevindingen beheert met risicoclassificatie
8. een professioneel pentestreport schrijven met bevindingen, bewijs en aanbevelingen

---

## Navigatie

| Pagina | Inhoud |
|--------|--------|
| [Theorie](./theorie) | Pentesting-methodologie, JWT-aanvallen, tools, pentestreport, demo stap voor stap |
| [Oefeningen](./oefeningen) | 5 oefeningen: JWT-manipulatie, brute-force, informatielekkage, PentestReport-klasse, volledig rapport |
| [Oplossingen](./oplossingen) | Volledige oplossingen met toelichting |
