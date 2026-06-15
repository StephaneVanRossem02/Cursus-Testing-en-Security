---
title: "Les 8: Secure Coding (OWASP)"
sidebar_label: "Overzicht"
---

# Les 8: Secure Coding (OWASP)

## ShopWave

ShopWave heeft op dit punt een solide beveiligingsfundament. Wachtwoorden worden gehasht met BCrypt. 2FA beschermt de loginflow. Orders worden digitaal ondertekend. De API draait op HTTPS. JWT bewaakt de endpoints.

Maar al die maatregelen beschermen de communicatie en de authenticatie. Ze beschermen niet tegen fouten in de code zelf.

Een aanvaller hoeft HTTPS niet te omzeilen als hij een invoerveld kan misbruiken om rechtstreeks de database te lezen. Hij hoeft geen JWT te stelen als een foutmelding de volledige serverstructuur blootgeeft. Beveiliging begint niet bij de netwerklaag. Het begint bij elke regel code die je schrijft.

De **OWASP Top 10** is een lijst van de meest voorkomende en gevaarlijkste kwetsbaarheden in webapplicaties, samengesteld door een internationale gemeenschap van beveiligingsexperts. In deze les behandelen we de meest kritieke kwetsbaarheden voor ShopWave in detail.

---

## OWASP Top 10 (2021)

| Nr | Naam | Kernprobleem | ShopWave-voorbeeld |
|----|------|--------------|--------------------|
| A01 | Broken Access Control | Gebruikers voeren acties uit buiten hun rechten | Klant raadpleegt bestellingen van andere klant |
| A02 | Cryptographic Failures | Gevoelige data onvoldoende versleuteld of gehasht | Wachtwoorden als plain text |
| A03 | Injection | Onbetrouwbare data uitgevoerd als code | SQL Injection in productzoekfunctie |
| A04 | Insecure Design | Architectuur mist beveiliging van bij het begin | Geen lockout na herhaalde mislukte logins |
| A05 | Security Misconfiguration | Standaardinstellingen, debug-info in productie | Stack trace zichtbaar in foutmelding |
| A06 | Vulnerable Components | Verouderde bibliotheken met bekende kwetsbaarheden | NuGet-pakket met gekende CVE |
| A07 | Auth and Session Failures | Zwakke authenticatie, onveilig sessiebeheer | JWT zonder vervaldatum |
| A08 | Software and Data Integrity | Onbetrouwbare updates of deserialisatie | NuGet-pakket vervangen door malafide versie |
| A09 | Logging and Monitoring Failures | Aanvallen worden niet gedetecteerd | Geen logging van mislukte loginpogingen |
| A10 | SSRF | Server vraagt externe URL op gestuurd door gebruiker | API haalt URL op die aanvaller meestuurt |

In deze les diepen we A03 (Injection), A05 (Misconfiguration), input validatie en CORS verder uit.

---

## Leerdoelen

Na deze les kan je:

1. uitleggen wat SQL Injection is en hoe een aanvaller het uitvoert
2. een kwetsbaar zoekendpoint omzetten naar een veilige implementatie met parameterized queries
3. uitleggen wat XSS is en waarom output encoding de oplossing is
4. de Developer Exception Page omgevingsafhankelijk configureren
5. server-side input validatie toevoegen aan API-endpoints
6. CORS correct configureren zodat enkel toegestane origins verzoeken mogen sturen
7. rate limiting instellen op het login-endpoint om brute-force aanvallen te blokkeren
8. kwetsbare NuGet-packages opsporen via `dotnet list package --vulnerable`

---

## Navigatie

| Pagina | Inhoud |
|--------|--------|
| [Theorie](./theorie) | SQL Injection, XSS, misconfiguratie, input validatie, CORS, rate limiting, demo stap voor stap |
| [Oefeningen](./oefeningen) | 5 implementatieoefeningen op basis van ShopWave |
| [Oplossingen](./oplossingen) | Volledige oplossingen met toelichting |
