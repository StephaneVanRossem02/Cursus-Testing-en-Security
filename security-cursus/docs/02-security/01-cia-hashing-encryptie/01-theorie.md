---
title: "Les 2: Security — CIA-model, Hashing en Encryptie — Theorie"
sidebar_label: "Theorie"
---

# Les 2: Security — CIA-model, Hashing en Encryptie — Theorie

## Het CIA-model

Het CIA-model is het basiskader voor informatiebeveiliging. Elke beveiligingsbeslissing is terug te brengen tot één of meerdere pijlers:

| Pijler           | Doel                                    | Voorbeeld in ShopWave                             |
|------------------|-----------------------------------------|---------------------------------------------------|
| Confidentiality  | Data afschermen voor onbevoegden        | Versleutelde wachtwoorden in database             |
| Integrity        | Data correct en ongewijzigd houden      | Digitale handtekening op bestelling               |
| Availability     | Systemen bereikbaar houden              | DDoS-bescherming, failover, monitoring            |

Een veilig systeem houdt alle drie pijlers in balans.

---

## Hashing

Een hashfunctie zet een invoer van willekeurige lengte om naar een vaste uitvoer (de **hash** of **digest**).

Eigenschappen van een goede hashfunctie:
- **Deterministisch** — zelfde invoer geeft altijd zelfde hash
- **Eénrichtingsverkeer** — origineel niet terug te berekenen uit de hash
- **Kleine wijziging, grote hash-wijziging** — één karakter anders → volledig andere hash
- **Botsingsresistent** — twee invoerwaarden mogen niet dezelfde hash geven

> ⚠️ **Gebruik SHA-256 nooit voor wachtwoorden** — het is te snel. Op een moderne GPU worden miljarden SHA-256-hashes per seconde berekend, wat brute force triviaal maakt.

### BCrypt voor wachtwoorden

Gebruik **BCrypt** (`BCrypt.Net-Next` NuGet-pakket) — dit algoritme is bewust traag gemaakt.

BCrypt berekent automatisch een willekeurige **salt** en bewaart die samen met de hash in één string. Je slaat salt en hash in één veld op — geen aparte salt-kolom nodig.

```csharp
string hash = BCrypt.Net.BCrypt.HashPassword(password);         // hashen
bool ok     = BCrypt.Net.BCrypt.Verify(password, storedHash);   // verifiëren
```

---

## Salt

Een **salt** is een willekeurige waarde die toegevoegd wordt aan het wachtwoord vóór het hashen. Dit voorkomt **rainbow-table-aanvallen** (vooraf berekende hash-tabellen).

BCrypt doet dit volledig automatisch. Twee gebruikers met hetzelfde wachtwoord krijgen een volledig andere hash, omdat elke BCrypt-aanroep een nieuwe willekeurige salt genereert.

---

## Encryptie

Encryptie is **tweerichtingsverkeer**: met de juiste sleutel kan je de originele waarde teruglezen.

### Symmetrische encryptie (AES)

Dezelfde sleutel voor versleutelen én ontsleutelen. Snel — industriestandaard voor data-at-rest en data-in-transit na de TLS-handshake.

### Asymmetrische encryptie (RSA)

Publieke sleutel voor versleutelen, private sleutel voor ontsleutelen. Trager — gebruikt in HTTPS, digitale handtekeningen en certificaten.

### IV — Initialization Vector

Gebruik altijd een **willekeurige IV** per encryptieoperatie (`aes.GenerateIV()`). Een vaste IV betekent dat dezelfde plaintext altijd dezelfde ciphertext oplevert — dat lekt informatie. De IV is geen geheim; je slaat hem samen met de ciphertext op (bv. als prefix).

---

## Hashing versus Encryptie

| Eigenschap      | Hashing                          | Encryptie                            |
|-----------------|----------------------------------|--------------------------------------|
| Richting        | Eénrichtingsverkeer              | Tweerichtingsverkeer                 |
| Terugkeerbaar?  | Nee                              | Ja, met de juiste sleutel            |
| Gebruik         | Wachtwoorden, integriteitscontrole | Data die je later moet lezen       |
| Voorbeeld       | Wachtwoord opslaan               | Creditcardnummer opslaan             |

**Vuistregel:** gebruik hashing wanneer je de originele waarde nooit hoeft te lezen. Gebruik encryptie wanneer je de originele waarde later wél nodig hebt.