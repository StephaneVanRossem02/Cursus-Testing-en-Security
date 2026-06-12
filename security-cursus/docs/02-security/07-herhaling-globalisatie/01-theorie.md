---
title: "Les 12: Herhaling en Globalisatie — Theorie"
sidebar_label: "Theorie"
---

# Les 12: Herhaling en Globalisatie — Theorie

## De ShopWave-beveiligingsarchitectuur

ShopWave implementeert **defense in depth** — meerdere beveiligingslagen zodat een aanvaller die door één laag breekt, nog steeds geblokkeerd wordt door de volgende:

| Laag            | Wat beschermt het?       | Technologie                                      |
|-----------------|--------------------------|--------------------------------------------------|
| Datalaag        | Data-at-rest             | BCrypt (wachtwoorden), AES-256 met willekeurige IV |
| Transportlaag   | Data-in-transit          | HTTPS/TLS 1.3, Kestrel met X.509-certificaat    |
| Applicatielaag  | Toegangscontrole         | JWT Bearer, 2FA, account lockout, rate limiting  |
| Codelaag        | Kwetsbaarheden in code   | Parameterized queries, input validatie, CORS     |

---

## OWASP Top 10 — status ShopWave na 12 lessen

| #   | Kwetsbaarheid                  | Status in ShopWave                              |
|-----|--------------------------------|-------------------------------------------------|
| A01 | Broken Access Control          | ✔ JWT + rolgebaseerde autorisatie               |
| A02 | Cryptographic Failures         | ✔ BCrypt, AES-256, TLS 1.3                      |
| A03 | Injection                      | ✔ Parameterized queries / foreach-filtering, input validatie |
| A04 | Insecure Design                | ✔ Lockout, 2FA, defense in depth                |
| A05 | Security Misconfiguration      | ✔ User Secrets, omgevingsconfiguratie, CORS     |
| A06 | Vulnerable Components          | ✔ `dotnet list package --vulnerable`            |
| A07 | Auth & Session Failures        | ✔ BCrypt, 2FA, JWT met vervaldatum              |
| A08 | Software & Data Integrity      | ✔ Digitale handtekeningen op orders             |
| A09 | Logging & Monitoring Failures  | ⚠ Niet expliciet behandeld in de cursus         |
| A10 | SSRF                           | ⚠ Niet behandeld in de cursus                  |

---

## Teststrategieën — samenvatting

| Strategie              | Kern                                         | Wanneer                             |
|------------------------|----------------------------------------------|-------------------------------------|
| Unit testing           | Geïsoleerde logica met mocks                 | Altijd — bij elke klasse            |
| TDD                    | Test eerst, implementeer daarna              | Nieuwe functionaliteit              |
| Integration testing    | Samenwerking echte klassen                   | Na unit tests                       |
| BDD / Acceptatietesten | Gedrag in begrijpelijke taal                 | Wanneer requirements als scenario's |
| Penetration testing    | Beveiliging als aanvaller testen             | Vóór elke release                   |

---

## Beveiligingsprincipes — samenvatting

| Principe            | Kern                                                              |
|---------------------|-------------------------------------------------------------------|
| CIA-model           | Confidentiality, Integrity, Availability — elk besluit hieraan toetsen |
| Defense in depth    | Meerdere lagen — als één faalt, houdt de volgende stand           |
| Least privilege     | Geef alleen de rechten die nodig zijn                             |
| Fail securely       | Bij fout: weiger toegang, log, geef geen interne details          |
| Never trust input   | Valideer altijd server-side, ongeacht client-side validatie       |
| Secrets management  | Nooit hardcoded — User Secrets, omgevingsvariabelen, Key Vault    |

---

## DevSecOps

DevSecOps integreert beveiligingstests in het CI/CD-proces bij elke commit:

```
Commit → Build → Test → Security Scan → Package → Deploy
                  ↑           ↑
           Unit tests     SAST: dotnet list --vulnerable
           Integration    DAST: OWASP ZAP
           Acceptatie
```

- **SAST** (Static Application Security Testing): analyseert broncode zonder uitvoer — snel, geïntegreerd in build
- **DAST** (Dynamic Application Security Testing): test de draaiende applicatie van buitenaf — vindt runtime-kwetsbaarheden

---

## SecurityChecklist en CiaPijlerAnalyse

De `SecurityChecklist`-klasse houdt per beveiligingsitem de implementatiestatus bij:

- `ChecklistStatus.Geimplementeerd` — volledig aanwezig
- `ChecklistStatus.Gedeeltelijk` — aanwezig maar onvolledig
- `ChecklistStatus.NietGeimplementeerd` — ontbreekt — risico!

De `CiaPijlerAnalyse`-klasse groepeert concrete voorbeelden per CIA-pijler en geeft een overzichtelijk rapport van de beveiligingsstatus van de volledige applicatie.