---
title: "Les 11: Ethisch Hacken — Pentest op ShopWave"
sidebar_label: "Ethisch Hacken"
---

# Les 11: Ethisch Hacken — Pentest op ShopWave

## ShopWave

Tien lessen lang hebben we ShopWave gebouwd en beveiligd. We hebben wachtwoorden gehasht met BCrypt, 2FA toegevoegd, orders digitaal ondertekend, de API beveiligd met JWT, SQL injection en XSS voorkomen en kwetsbare packages opgespoord. Maar hoe weet je of al die maatregelen ook echt werken?

**Ethisch hacken** — ook wel *penetration testing* of *pentesting* — is het gecontroleerd aanvallen van een systeem met toestemming van de eigenaar, met als doel kwetsbaarheden te vinden vóór een echte aanvaller dat doet. In deze les draaien we de rollen om: jij bent de aanvaller. Jij probeert ShopWave te breken.

> ⚠️ **Wettelijk kader — dit is niet onderhandelbaar.** Ethisch hacken is alleen legaal als je expliciete toestemming hebt van de eigenaar van het systeem. In België valt ongeautoriseerd hacken onder artikel 550bis van het Strafwetboek (computervredebreuk) en de Europese NIS2-richtlijn. In deze les werken we **uitsluitend op onze eigen lokale ShopWave-omgeving**. Gebruik deze technieken nooit op externe systemen zonder schriftelijke toestemming.

---

## Theorie

### 1. Wat is ethisch hacken?

Ethisch hacken is het systematisch testen van een systeem op kwetsbaarheden, vanuit het perspectief van een aanvaller. Het verschil met kwaadaardige hacking is toestemming en intentie: een ethisch hacker rapporteert zijn bevindingen aan de eigenaar zodat ze opgelost kunnen worden.

**De drie rollen in security:**

| Rol | Naam | Wat ze doen |
|-----|------|-------------|
| Aanvaller | Red Team | Probeert het systeem te compromitteren |
| Verdediger | Blue Team | Bouwt en bewaakt de beveiliging |
| Beide | Purple Team | Werken samen: aanvaller deelt technieken, verdediger past maatregelen aan |

In deze les speel jij de rol van Red Team op je eigen systeem.

---

### 2. De pentesting-methodologie

Een professionele pentest volgt een gestructureerde aanpak:

1. **Verkenning (Reconnaissance)** — informatie verzamelen over het doelwit
2. **Scannen en enumereren** — poorten, endpoints en kwetsbaarheden in kaart brengen
3. **Exploitatie** — kwetsbaarheden misbruiken
4. **Post-exploitatie** — begrijpen wat een aanvaller met toegang kan doen
5. **Rapporteren** — bevindingen documenteren met risicoclassificatie en aanbevelingen

We volgen deze stappen op ShopWave.

---

### 3. Tools

**Burp Suite Community Edition** — een intercepting proxy die HTTP-verkeer onderschept, inspecteert en aanpast. Je stuurt requests via Burp, die ze onderschept en toont voordat ze de server bereiken.

Download via: `https://portswigger.net/burp/communitydownload`

**OWASP ZAP (Zed Attack Proxy)** — een open-source scanner die automatisch kwetsbaarheden opspoort in webapplicaties. Ideaal voor een eerste, brede scan.

Download via: `https://www.zaproxy.org/download/`

**curl** — commandoregelgereedschap voor HTTP-requests. Ingebouwd in Windows 11, macOS en Linux.

**jwt.io** — een online tool om JWT-tokens te decoderen en te inspecteren. Gebruik de lokale versie of de website (nooit echte productietokens online plakken).

---

### 4. Aanvalsoppervlak van ShopWave

Alles wat we gebouwd hebben, is een potentieel aanvalsoppervlak:

| Component | Risico |
|-----------|--------|
| `/login`-endpoint | Brute force, credential stuffing |
| JWT-tokens | Manipulatie van claims, algoritmewissel naar `none` |
| `/zoek`-endpoint | SQL injection (we hebben dit gefixed, maar we verifiëren) |
| Foutmeldingen | Informatielekkage bij verkeerde configuratie |
| NuGet-packages | Kwetsbare dependencies (A06) |
| CORS-headers | Te permissief geconfigureerde origins |

---

## Demo

We voeren stap voor stap een gestructureerde pentest uit op de lokale ShopWave API.

### Stap 1 — Verkenning: endpoints in kaart brengen

Start de ShopWave API (`dotnet run` in het API-project). De API draait op `https://localhost:5001`.

Gebruik **curl** om te kijken welke endpoints beschikbaar zijn:

```bash
curl -k https://localhost:5001/
curl -k https://localhost:5001/health
curl -k https://localhost:5001/swagger
```

De `-k` vlag slaat certificaatvalidatie over voor self-signed certificaten — acceptabel in development, nooit in productie.

Noteer alle endpoints die je vindt. Dit zijn je aanvalspunten.

---

### Stap 2 — JWT-token inspecteren en manipuleren

#### Stap 2a — Token ophalen

Log in via curl en sla het token op:

```bash
curl -k -X POST https://localhost:5001/login \
  -H "Content-Type: application/json" \
  -d "{\"email\":\"alice@shopwave.be\",\"wachtwoord\":\"wachtwoord123\"}"
```

Je krijgt een response zoals:

```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJhbGljZUBzaG9wd2F2ZS5iZSIsInJvbGUiOiJ1c2VyIiwiZXhwIjoxNzE2ODE2MDAwfQ.abc123"
}
```

#### Stap 2b — Token decoderen

Plak het token in **jwt.io** (lokale versie) of splits het handmatig:

```
Header:  eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9
Payload: eyJzdWIiOiJhbGljZUBzaG9wd2F2ZS5iZSIsInJvbGUiOiJ1c2VyIiwiZXhwIjoxNzE2ODE2MDAwfQ
```

Decode de payload (Base64):

```json
{
  "sub": "alice@shopwave.be",
  "role": "user",
  "exp": 1716816000
}
```

#### Stap 2c — Rolmanipulatie proberen

Verander de `role`-claim van `user` naar `admin` in de payload, hercodeer naar Base64 en stuur het aangepaste token:

```bash
# Aangepast token met role: admin (handmatig geconstrueerd)
curl -k https://localhost:5001/admin \
  -H "Authorization: Bearer <aangepast_token>"
```

**Verwacht resultaat:** `401 Unauthorized` — de signature klopt niet meer omdat de geheime sleutel ontbreekt. Dit bevestigt dat JWT-signature-validatie correct werkt.

Noteer dit als een **geslaagde beveiligingstest** in je rapport.

#### Stap 2d — Algoritmeaanval `alg: none`

Een klassieke JWT-aanval is het veranderen van het algoritme naar `none`. Sommige slecht geconfigureerde bibliotheken accepteren tokens zonder signature:

```json
{
  "alg": "none",
  "typ": "JWT"
}
```

Test of de ShopWave API dit accepteert:

```bash
# Token met alg:none, geen signature
curl -k https://localhost:5001/orders \
  -H "Authorization: Bearer eyJhbGciOiJub25lIiwidHlwIjoiSldUIn0.eyJzdWIiOiJhZG1pbkBzaG9wd2F2ZS5iZSIsInJvbGUiOiJhZG1pbiJ9."
```

**Verwacht resultaat:** `401 Unauthorized` — .NET's `JwtBearerAuthentication` weigert tokens met `alg: none` standaard. Noteer als **geslaagde beveiligingstest**.

---

### Stap 3 — Brute force logintest

We testen of rate limiting werkt door vijf foute loginpogingen te sturen:

```bash
for i in 1 2 3 4 5 6; do
  echo "Poging $i:"
  curl -k -s -o /dev/null -w "%{http_code}" \
    -X POST https://localhost:5001/login \
    -H "Content-Type: application/json" \
    -d "{\"email\":\"alice@shopwave.be\",\"wachtwoord\":\"fout$i\"}"
  echo ""
done
```

**Verwacht resultaat:**
- Pogingen 1-5: `401 Unauthorized`
- Poging 6: `429 Too Many Requests` (als rate limiting geconfigureerd is)

Als poging 6 ook `401` geeft, is rate limiting **niet** geactiveerd — dit is een bevinding om te rapporteren.

---

### Stap 4 — SQL Injection verifiëren (defensieve test)

We hebben SQL injection al gefixed in Les 9 met parameterized queries. Nu verifiëren we dat de fix effectief is:

```bash
# SQL injection-payload als zoekopdracht
curl -k "https://localhost:5001/producten?naam=' OR '1'='1"
curl -k "https://localhost:5001/producten?naam='; DROP TABLE products --"
```

**Verwacht resultaat:** lege resultatenlijst of `400 Bad Request` — geen SQL-fout, geen gelekte data. De parameterized query behandelt de input als een letterlijke string, niet als SQL-code.

Noteer als **geslaagde beveiligingstest** (fix werkt).

---

### Stap 5 — Informatielekkage testen

Test of foutmeldingen interne informatie lekken:

```bash
# Ongeldige route
curl -k https://localhost:5001/bestaaniet

# Ongeldige input
curl -k -X POST https://localhost:5001/login \
  -H "Content-Type: application/json" \
  -d "GEEN_GELDIGE_JSON"
```

**Verwacht resultaat in productie:** generieke foutmelding zonder stacktrace of interne paden.

**Verwacht resultaat in development:** volledige stacktrace — dit is normaal voor development maar nooit acceptabel in productie.

---

### Stap 6 — OWASP ZAP automatische scan

Open **OWASP ZAP** en voer een **Automated Scan** uit op `https://localhost:5001`.

ZAP stuurt honderden automatische aanvalsverzoeken en rapporteert kwetsbaarheden per risiconivenau:

| Risico | Betekenis |
|--------|-----------|
| High | Kritieke kwetsbaarheid — onmiddellijk oplossen |
| Medium | Significante kwetsbaarheid — oplossen vóór go-live |
| Low | Verbetering aanbevolen |
| Informational | Geen kwetsbaarheid maar nuttige informatie |

Bekijk het rapport en noteer alle bevindingen van Medium of hoger.

---

### Stap 7 — Burp Suite: request interceptie

1. Open **Burp Suite Community Edition**
2. Ga naar **Proxy** → **Intercept is on**
3. Configureer je browser of curl om de Burp-proxy te gebruiken (standaard: `127.0.0.1:8080`)
4. Stuur een loginrequest

Burp onderschept het request. Je kan nu:
- De request-body aanpassen vóór hij de server bereikt
- Headers toevoegen of wijzigen
- Het request opnieuw sturen met aanpassingen (**Repeater**)

Probeer in de **Repeater** het wachtwoord aan te passen en observeer de serverresponse.

---

## Oefeningen

### Oefening 1 — Pentest: CORS-headers inspecteren

Stuur een request met een `Origin`-header en inspecteer de response:

```bash
curl -k -H "Origin: https://kwaadaardige-site.com" \
  https://localhost:5001/orders
```

Bekijk de response-headers:
- Is `Access-Control-Allow-Origin` aanwezig?
- Staat er `*` (wildcard) of een specifieke origin?

Rapporteer je bevinding.

---

### Oefening 2 — Brute force met Burp Intruder

Gebruik **Burp Suite → Intruder** om automatisch een lijst van wachtwoorden te testen op het `/login`-endpoint.

1. Onderschep een loginrequest in Burp
2. Stuur naar **Intruder** (Ctrl+I)
3. Markeer de wachtwoordwaarde als payload-positie
4. Laad een kleine woordenlijst (bv. `top10passwords.txt`)
5. Start de aanval

Observeer:
- Welke HTTP-statuscodes krijg je terug?
- Wordt de aanval vertraagd na meerdere pogingen?
- Wanneer wordt het account geblokkeerd?

---

### Oefening 3 — Pentestreport schrijven

Schrijf een professioneel pentestreport voor ShopWave. Gebruik de volgende structuur:

**1. Samenvatting**
- Datum en scope van de test
- Getest systeem: ShopWave API v1.0 (lokaal)
- Testmethode: black box / grey box

**2. Bevindingen**

Gebruik voor elke bevinding de volgende indeling:

| Veld | Waarde |
|------|--------|
| ID | FINDING-01 |
| Titel | Rate limiting ontbreekt op /login |
| Risico | Medium |
| CVSS-score | 5.3 |
| Beschrijving | Onbeperkte loginpogingen zijn mogelijk |
| Bewijs | HTTP-responsecode 401 bij poging 6 in plaats van 429 |
| Aanbeveling | Activeer `AddRateLimiter` met `FixedWindowLimiter` |
| Status | Open / Gesloten |

**3. Risicomatrix**

Classificeer alle bevindingen op een 2×2 matrix (Waarschijnlijkheid × Impact):

```
           | Lage impact | Hoge impact
-----------|-------------|------------
Waarschijnlijk | Medium   | High
Onwaarschijnlijk | Low   | Medium
```

**4. Conclusie**
- Totaal aantal bevindingen per risiconivenau
- Algeheel beveiligingsoordeel
- Prioriteit voor herstel

---

### Oefening 4 — Kwetsbare packages opsporen

Voer in de ShopWave-solution het volgende commando uit:

```
dotnet list package --vulnerable
```

Noteer:
- Zijn er kwetsbare packages?
- Wat is de CVE-identifier?
- Wat is de minimale versie die de kwetsbaarheid oplost?

Update kwetsbare packages en voer het commando opnieuw uit om te bevestigen dat alle kwetsbaarheden opgelost zijn.

---

## Samenvatting

| Concept | Wat je moet onthouden |
|---------|-----------------------|
| Ethisch hacken | Gecontroleerd aanvallen met toestemming — nooit zonder |
| Wettelijk kader | Art. 550bis Strafwetboek (BE) + NIS2 — ongeautoriseerd hacken is strafbaar |
| Pentesting-fases | Verkenning → Scannen → Exploitatie → Post-exploitatie → Rapportage |
| JWT-manipulatie | Signature-validatie beschermt claims — `alg: none`-aanval geblokkeerd door .NET |
| Brute force | Rate limiting en account lockout zijn de verdediging |
| SQL injection verificatie | Parameterized queries beschermen — test altijd of de fix ook echt werkt |
| Informatielekkage | Stacktraces en interne paden mogen nooit in productie verschijnen |
| OWASP ZAP | Automatische scanner voor breed overzicht van kwetsbaarheden |
| Burp Suite | Intercepting proxy voor gerichte, handmatige tests |
| Pentestreport | Bevindingen documenteren met risicoclassificatie en aanbevelingen |
| `dotnet list package --vulnerable` | Opsporen van NuGet-packages met bekende CVEs (OWASP A06) |

---

## Volgende les

Les 12 is **Herhaling en Globalisatie**. We maken de cirkel rond: een volledig overzicht van de ShopWave-beveiligingsarchitectuur, een herhaling van alle teststrategieën, en een blik op hoe security en testing geïntegreerd worden in een professionele DevSecOps-pipeline.