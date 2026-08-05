---
title: "Les 12: ShopWave in Productie"
sidebar_label: "Overzicht"
---

# Les 12: ShopWave in Productie

## ShopWave

Elf lessen lang heb je ShopWave gebouwd en beveiligd. Wachtwoorden worden gehasht met BCrypt. 2FA beschermt de loginflow. Orders worden digitaal ondertekend. De API draait op HTTPS. JWT bewaakt de endpoints. SQL Injection, XSS en misconfiguraties zijn gefixed. Vorige les heb je als ethisch hacker je eigen beveiliging aangevallen en gedocumenteerd.

Er is nog een vraag onbeantwoord: **hoe zet je dit in productie?**

Development en productie zijn twee verschillende werelden. Op je laptop werk je met een self-signed certificaat, een hardcoded testsleutel en de Developer Exception Page aan. Geen van die drie mag ooit op een productieserver terechtkomen. In deze les leer je wat er verandert als ShopWave van je laptop naar een echte server gaat, en hoe je die overgang veilig maakt.

---

## Leerdoelen

Na deze les kan je:

1. het verschil uitleggen tussen een development-omgeving en een productie-omgeving voor een ASP.NET Core API
2. secrets beheren via omgevingsvariabelen op een server en via Azure Key Vault
3. HTTPS configureren met een productiecertificaat via Let's Encrypt
4. uitleggen waarom een self-signed certificaat niet geschikt is voor productie
5. een `appsettings.Production.json` correct opzetten voor ShopWave
6. de OWASP Top 10-status van ShopWave beschrijven na elf lessen
7. een `SecurityChecklist`-klasse implementeren die de beveiligingsstatus bijhoudt
8. een `CiaPijlerAnalyse`-klasse implementeren die de CIA-pijlers documenteert

---

## Navigatie

| Pagina | Inhoud |
|--------|--------|
| [Theorie](./theorie) | Development vs productie, secrets, HTTPS, checklist, OWASP-status, DevSecOps, demo stap voor stap |
| [Oefeningen](./oefeningen) | 5 oefeningen: omgevingsvariabelen, SecurityChecklist, CiaPijlerAnalyse, productieconfiguratie, eindreflectie |
| [Oplossingen](./oplossingen) | Volledige oplossingen met toelichting |
