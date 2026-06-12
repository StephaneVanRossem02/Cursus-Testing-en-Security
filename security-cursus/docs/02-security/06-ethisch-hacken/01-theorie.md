---
title: "Les 11: Ethisch Hacken — Pentest op ShopWave — Theorie"
sidebar_label: "Theorie"
---

# Les 11: Ethisch Hacken — Pentest op ShopWave — Theorie

## Wat is ethisch hacken?

**Ethisch hacken** (penetration testing / pentesting) is het gecontroleerd aanvallen van een systeem **met toestemming van de eigenaar**, met als doel kwetsbaarheden te vinden vóór een echte aanvaller dat doet.

> **Wettelijk kader:** ongeautoriseerd hacken is in België strafbaar onder artikel 550bis van het Strafwetboek en valt ook onder de Europese NIS2-richtlijn. Ethisch hacken vereist altijd schriftelijke toestemming.

---

## De drie rollen in security

| Rol              | Naam        | Wat ze doen                                                 |
|------------------|-------------|-------------------------------------------------------------|
| Aanvaller        | Red Team    | Probeert het systeem te compromitteren                      |
| Verdediger       | Blue Team   | Bouwt en bewaakt de beveiliging                             |
| Beide            | Purple Team | Werken samen — aanvaller deelt technieken, verdediger past maatregelen aan |

---

## De pentesting-methodologie

1. **Verkenning** — informatie verzamelen over het doelwit (endpoints, technologieën, versies)
2. **Scannen en enumereren** — poorten, endpoints en kwetsbaarheden in kaart brengen
3. **Exploitatie** — kwetsbaarheden misbruiken
4. **Post-exploitatie** — begrijpen wat een aanvaller met toegang kan doen
5. **Rapporteren** — bevindingen documenteren met risicoclassificatie en aanbevelingen

---

## Tools

| Tool                        | Gebruik                                                    |
|-----------------------------|------------------------------------------------------------|
| **Burp Suite Community**    | Intercepting proxy — onderschept en past HTTP-verkeer aan  |
| **OWASP ZAP**               | Automatische scanner voor webkwetsbaarheden                |
| **curl**                    | Commandoregel HTTP-tool — verzoeken sturen en headers inspecteren |
| **jwt.io**                  | JWT decoderen en inspecteren (gebruik nooit productietokens!) |

---

## JWT-aanvallen

### Rolmanipulatie

De aanvaller past de payload aan: `"role": "user"` → `"role": "admin"`. Daarna berekent hij **geen nieuwe geldige signature**.

De server valideert de signature → de gewijzigde payload matcht niet meer met de signature → het token wordt geweigerd.

**Conclusie:** signature-validatie beschermt tegen rolmanipulatie.

### alg:none aanval

De aanvaller wijzigt de header: `"alg": "HS256"` → `"alg": "none"` en laat de signature weg.

.NET weigert tokens met `alg: none` standaard — dit kan niet worden uitgeschakeld zonder expliciete configuratie.

**Conclusie:** de standaard .NET JWT-middleware is correct geconfigureerd.

---

## Pentestreport

Een professioneel pentestreport bevat per bevinding:

| Veld          | Inhoud                                                    |
|---------------|-----------------------------------------------------------|
| ID            | Unieke identifier (bv. FINDING-01)                        |
| Titel         | Korte beschrijving van de kwetsbaarheid                   |
| Risico        | Informational / Low / Medium / High / Critical            |
| CVSS-score    | Numerieke risicoscore (0.0–10.0)                          |
| Beschrijving  | Wat is het probleem en hoe werkt het?                     |
| Bewijs        | HTTP-request/response, screenshot, log-uitvoer            |
| Aanbeveling   | Concrete technische maatregel                             |
| Status        | Open / Gesloten                                           |

---

## Risicomatrix

```
              | Lage impact | Hoge impact
Waarschijnlijk  |   Medium    |    High
Onwaarschijnlijk|    Low      |   Medium
```

Het risico wordt bepaald door de combinatie van **waarschijnlijkheid** (hoe makkelijk te misbruiken?) en **impact** (hoe groot is de schade?).

---

## DevSecOps — security in het CI/CD-proces

```
Commit → Build → Test → Security Scan → Package → Deploy
                  ↑          ↑
           Unit tests    SAST + dotnet list --vulnerable
           Integration   DAST: OWASP ZAP
           Acceptatie
```

- **SAST** (Static Application Security Testing): analyseert broncode zonder de applicatie uit te voeren
- **DAST** (Dynamic Application Security Testing): test de draaiende applicatie van buitenaf