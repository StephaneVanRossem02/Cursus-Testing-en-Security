---
title: "Les 2: Security — CIA-model, hashing en encryptie"
sidebar_label: "CIA, Hashing & Encryptie"
---

# Les 2: Security — CIA-model, hashing en encryptie

## ShopWave

We werken verder in de **ShopWave**-solution die je in Les 1 hebt aangemaakt. ShopWave slaat gegevens op van duizenden klanten: e-mailadressen, wachtwoorden, bestelgeschiedenissen en betalingsgegevens. Al die gegevens moeten beschermd worden. In deze les leggen we het fundament voor alle beveiligingsbeslissingen die je als ontwikkelaar neemt.

Open de bestaande solution `ShopWave` in Visual Studio. We voegen een nieuwe map `Security` toe aan het project `ShopWave`.

---

## Theorie

### 1. Het CIA-model

Het **CIA-model** is het basiskader voor informatiebeveiliging. Elke beveiligingsbeslissing die je neemt als ontwikkelaar is terug te brengen tot één of meerdere van de drie pijlers. CIA staat voor:

- **Confidentiality** — vertrouwelijkheid
- **Integrity** — integriteit
- **Availability** — beschikbaarheid

Dit model wordt gebruikt in alle sectoren: overheid, zorgsector, banken, e-commerce. Voor ShopWave betekent het concreet:

| Pijler | Vraag die je stelt | Voorbeeld in ShopWave |
|--------|--------------------|-----------------------|
| Confidentiality | Is de informatie afgeschermd voor onbevoegden? | Klantgegevens mogen niet leesbaar zijn als de database gestolen wordt |
| Integrity | Is de informatie correct en ongewijzigd? | Een bestelbedrag mag niet aangepast zijn door een aanvaller |
| Availability | Is de informatie beschikbaar wanneer nodig? | De webshop moet bereikbaar zijn, ook tijdens piekdrukte |

Een veilig systeem houdt alle drie pijlers in balans. Een systeem dat alleen focust op vertrouwelijkheid maar beschikbaarheid verwaarloost, is even gevaarlijk als een systeem dat niets beveiligt.

---

#### 1.1 Confidentiality (Vertrouwelijkheid)

Doel: voorkomen dat onbevoegden toegang krijgen tot gevoelige informatie.

**Hoe bereik je dit?**

- **Encryptie** — gegevens worden versleuteld zodat ze onleesbaar zijn zonder de juiste sleutel. Dit geldt zowel bij verzending (HTTPS) als bij opslag (versleutelde database).
- **Authenticatie en autorisatie** — authenticatie bepaalt *wie je bent*, autorisatie bepaalt *wat je mag doen*. Een klant bij ShopWave kan zijn eigen bestellingen zien, maar niet die van andere klanten.
- **VPN** — een beveiligde tunnel tussen gebruiker en netwerk, waardoor buitenstaanders niet kunnen meekijken.

**Bekende beveiligingsfouten:**

- *Sony PlayStation Network (2011)*: 77 miljoen accounts gelekt, inclusief wachtwoorden en creditcardgegevens, door gebrekkige encryptie.
- *E-mail CC in plaats van BCC*: een menselijke fout waarbij alle e-mailadressen zichtbaar zijn voor alle ontvangers.
- *USB-sticks in de onderwijssector*: studentenlijsten met persoonsgegevens onversleuteld op verloren USB-sticks.

---

#### 1.2 Integrity (Integriteit)

Doel: zorgen dat data correct, volledig en ongewijzigd blijft.

**Hoe bereik je dit?**

- **Hashing** — een hash is een unieke digitale vingerafdruk van een stuk data. Elke wijziging, hoe klein ook, levert een volledig andere hash op. Je kan hiermee controleren of data ongewijzigd is.
- **Digitale handtekeningen** — bewijzen dat de inhoud afkomstig is van de juiste afzender en onderweg niet gewijzigd is.
- **Version control (Git)** — houdt bij wie welke wijzigingen deed en wanneer.

**Bekende beveiligingsfouten:**

- *Manipulatie van bankgegevens*: hackers passen rekeningnummers aan in e-mailfacturen. Het geld belandt bij de aanvaller.
- *Zorgsector*: een fout in een patiëntendossier kan leiden tot verkeerde medicatie. Integriteit staat hier letterlijk gelijk aan mensenlevens.

---

#### 1.3 Availability (Beschikbaarheid)

Doel: zorgen dat systemen en data bereikbaar zijn voor bevoegde gebruikers wanneer ze dat nodig hebben.

**Hoe bereik je dit?**

- **DDoS-bescherming** — bescherming tegen aanvallen waarbij servers overspoeld worden met verkeer.
- **Failover-systemen** — als een server uitvalt, neemt een andere automatisch het werk over.
- **Monitoring** — systemen continu bewaken zodat storingen vroeg gedetecteerd worden.

**Bekende beveiligingsfouten:**

- *GitHub DDoS-aanval (2018)*: één van de grootste aanvallen ooit — GitHub tijdelijk offline.
- *Facebook outage (2021)*: een fout in de netwerkconfiguratie legde Facebook, Instagram en WhatsApp wereldwijd urenlang plat.
- *Ransomware zonder back-up*: bedrijven die geen werkende back-up hadden, verloren alle data.

---

### 2. Hashing

#### Wat is hashing?

Een **hashfunctie** zet een invoerwaarde van willekeurige lengte om naar een vaste uitvoer: de **hash** of **digest**. Dat resultaat heeft altijd dezelfde lengte, ongeacht hoe lang de invoer is.

Eigenschappen van een goede hashfunctie:

- **Deterministisch** — dezelfde invoer geeft altijd dezelfde hash
- **Eénrichtingsverkeer** — je kan de originele invoer niet afleiden uit de hash
- **Kleine invoerwijziging, grote hash-wijziging** — één karakter anders in de invoer geeft een volledig andere hash
- **Botsingsresistent** — twee verschillende invoerwaarden mogen niet dezelfde hash opleveren

**Voorbeeld met SHA-256:**

| Invoer | SHA-256 hash (verkort) |
|--------|-----------------------|
| `hallo` | `d3751d33f9cd5049...` |
| `Hallo` | `185f8db329...` |
| `hallo ` (met spatie) | `volledig andere hash` |

Eén hoofdletter verschil geeft een volledig andere hash. Dit maakt hashing ideaal om te detecteren of data gewijzigd is.

#### Hashing voor wachtwoorden

Hashing lost een concreet probleem op: hoe sla je een wachtwoord veilig op?

**Probleem — plain-text opslag:**

```csharp
// FOUT — nooit doen
string opgeslagenWachtwoord = "mijnGeheimWachtwoord123";
```

Als de database gestolen wordt, zijn alle wachtwoorden onmiddellijk leesbaar.

**Oplossing — hash het wachtwoord bij registratie, vergelijk hashes bij login:**

```
Bij registratie  →  hash(wachtwoord)          →  sla de hash op in de database
Bij login        →  hash(ingegeven wachtwoord) →  vergelijk met de opgeslagen hash
```

#### Welke hashfunctie gebruik je voor wachtwoorden?

> ⚠️ **Belangrijke regel: gebruik SHA-256 NOOIT voor wachtwoorden.**

SHA-256 is een algemene hashfunctie die ontworpen is om zo **snel** mogelijk te zijn. Op een moderne GPU worden miljarden SHA-256-hashes per seconde berekend. Dat is ideaal voor integriteitscontroles, maar **gevaarlijk voor wachtwoorden**: een aanvaller kan in korte tijd alle mogelijke wachtwoorden uitproberen (brute-force).

Voor wachtwoorden gebruik je een **specifieke wachtwoordhashfunctie** die bewust **traag** is gemaakt:

| Algoritme | NuGet package | Aanbevolen? |
|-----------|---------------|-------------|
| **BCrypt** | `BCrypt.Net-Next` | ✔ Ja — eenvoudig, bewezen, standaard |
| **PBKDF2** | Ingebouwd in .NET (`Rfc2898DeriveBytes`) | ✔ Ja |
| **Argon2** | `Konscious.Security.Cryptography` | ✔ Ja — modernste optie |
| SHA-256 | Ingebouwd | ✖ Nee — te snel voor wachtwoorden |

**BCrypt** is de meest gebruikte keuze in .NET-projecten. BCrypt berekent automatisch een salt en bewaart die samen met de hash in één string — je hoeft de salt niet apart bij te houden.

---

### 3. Encryptie

#### Wat is encryptie?

**Encryptie** zet leesbare data (plaintext) om naar onleesbare data (ciphertext) met behulp van een sleutel. Met de juiste sleutel kan je de data terug ontsleutelen.

#### Symmetrische encryptie

Bij **symmetrische encryptie** gebruik je dezelfde sleutel om te versleutelen én te ontsleutelen.

```
Plaintext  +  sleutel  →  Ciphertext
Ciphertext +  sleutel  →  Plaintext
```

Voordeel: snel en eenvoudig te implementeren.
Nadeel: beide partijen moeten dezelfde sleutel kennen. Hoe deel je die sleutel veilig?

In .NET gebruik je **AES** (Advanced Encryption Standard) voor symmetrische encryptie. AES is de industriestandaard en wordt wereldwijd toegepast.

#### Asymmetrische encryptie

Bij **asymmetrische encryptie** gebruik je een sleutelpaar: een publieke sleutel om te versleutelen, een private sleutel om te ontsleutelen.

```
Plaintext  +  publieke sleutel →  Ciphertext
Ciphertext +  private sleutel  →  Plaintext
```

Voordeel: je hoeft de private sleutel nooit te delen.
Nadeel: trager dan symmetrische encryptie.

Asymmetrische encryptie wordt gebruikt in HTTPS, digitale handtekeningen en certificaten. Dit behandelen we in Les 5.

---

### 4. Hashing versus encryptie

Dit is een veelgemaakte verwarring. Het verschil is fundamenteel:

| | Hashing | Encryptie |
|--|---------|-----------|
| Richting | Eénrichtingsverkeer | Tweerichtingsverkeer |
| Terugkeerbaar? | Nee | Ja, met de juiste sleutel |
| Gebruik | Wachtwoorden, integriteitscontrole | Vertrouwelijke data die je later moet lezen |
| Voorbeeld | Wachtwoord opslaan | Creditcardnummer opslaan |

Vuistregel: gebruik **hashing** wanneer je de originele waarde nooit hoeft te lezen. Gebruik **encryptie** wanneer je de originele waarde later wél nodig hebt.

---

### 5. Salt bij hashing

Hashing alleen is niet voldoende voor wachtwoorden. Een aanvaller met een **rainbow table** — een vooraf berekende lijst van hashes voor veelgebruikte wachtwoorden — kan snel achterhalen welk wachtwoord bij een hash hoort.

De oplossing is een **salt**: een willekeurige waarde die je toevoegt aan het wachtwoord vóór je het hasht.

```
hash("wachtwoord")         →  altijd dezelfde hash
hash("wachtwoord" + salt)  →  elke keer een unieke hash
```

Elke gebruiker krijgt een eigen, willekeurig gegenereerde salt. Je slaat de salt op naast de hash in de database. Zo zijn twee gebruikers met hetzelfde wachtwoord niet meer herkenbaar aan hun hash.

> 💡 **BCrypt doet dit automatisch.** Wanneer je `BCrypt.HashPassword(password)` aanroept, wordt er intern een willekeurige salt gegenereerd en ingebakken in de hash-string die wordt teruggegeven. Je hoeft de salt dus niet apart bij te houden — `BCrypt.Verify(password, hash)` leest de salt gewoon terug uit de hash.

---

## Demo

We bouwen verder op de ShopWave-solution van Les 1. We voegen een map `Security` toe aan het project `ShopWave` en implementeren stap voor stap correcte wachtwoordbeveiliging en encryptie.

### Stap 1 — Map aanmaken in het bestaande project

Rechtermuisklik op het project `ShopWave` in Solution Explorer → **Add** → **New Folder** → noem de map `Security`.

Alle klassen die we in deze les maken, komen in die map terecht.

---

### Stap 2 — Het probleem demonstreren: plain-text wachtwoorden

Maak in de map `Security` een nieuw bestand `UserRepository.cs` aan:

```csharp
namespace ShopWave.Security
{
    public class UserRepository
    {
        // Simulatie: wachtwoorden opgeslagen als plain text
        // Dit is FOUT en nooit acceptabel in een echte applicatie
        private readonly Dictionary<string, string> _users = new Dictionary<string, string>
        {
            { "alice@shopwave.be", "mijnWachtwoord123" },
            { "bob@shopwave.be",   "qwerty" }
        };

        public bool Login(string email, string password)
        {
            bool result = false;

            if (_users.ContainsKey(email))
            {
                result = _users[email] == password;
            }

            return result;
        }
    }
}
```

Voeg tijdelijk toe aan `Program.cs` van `ShopWave`:

```csharp
using ShopWave.Security;

UserRepository repository = new UserRepository();

Console.Write("E-mail: ");
string email = Console.ReadLine() ?? string.Empty;

Console.Write("Wachtwoord: ");
string password = Console.ReadLine() ?? string.Empty;

bool success = repository.Login(email, password);

if (success)
{
    Console.WriteLine("Inloggen geslaagd.");
}
else
{
    Console.WriteLine("Inloggen mislukt.");
}
```

Dit werkt, maar als de database uitlekt, ziet een aanvaller alle wachtwoorden onmiddellijk. We gaan dit stap voor stap verbeteren.

---

### Stap 3 — PasswordHasher aanmaken

Installeer eerst het NuGet-pakket `BCrypt.Net-Next` via de Package Manager Console:

```
Install-Package BCrypt.Net-Next
```

Of via de .NET CLI:

```
dotnet add package BCrypt.Net-Next
```

Maak in de map `Security` een nieuw bestand `PasswordHasher.cs` aan:

```csharp
namespace ShopWave.Security
{
    // BCrypt.Net-Next NuGet-pakket vereist
    // BCrypt berekent automatisch een willekeurige salt en bewaart die
    // samen met de hash in één string — je hoeft de salt NIET apart op te slaan.
    public class PasswordHasher
    {
        public string Hash(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public bool Verify(string password, string storedHash)
        {
            return BCrypt.Net.BCrypt.Verify(password, storedHash);
        }
    }
}
```

Demonstreer het verschil in `Program.cs`:

```csharp
using ShopWave.Security;

PasswordHasher hasher = new PasswordHasher();

string password = "mijnWachtwoord123";
string hash1    = hasher.Hash(password);
string hash2    = hasher.Hash(password);
string hash3    = hasher.Hash("MijnWachtwoord123"); // hoofdletter M

Console.WriteLine($"Hash 1: {hash1}");
Console.WriteLine($"Hash 2: {hash2}");
Console.WriteLine($"Hash 3: {hash3}");
Console.WriteLine();
Console.WriteLine($"Hash 1 en 2 zijn gelijk: {hash1 == hash2}");    // false — BCrypt genereert elke keer een nieuwe salt
Console.WriteLine($"Hash 1 en 3 zijn gelijk: {hash1 == hash3}");    // false
Console.WriteLine($"Verificatie met hash 1:  {hasher.Verify(password, hash1)}"); // true
```

Observeer: hetzelfde wachtwoord geeft elke keer een **andere** hash, omdat BCrypt intern een willekeurige salt genereert. Toch werkt `Verify` correct, want BCrypt leest de salt terug uit de hash-string zelf. Eén hoofdletter verschil geeft uiteraard ook een andere hash.

---

### Stap 4 — Twee gebruikers, hetzelfde wachtwoord

Demonstreer in `Program.cs` waarom BCrypt automatisch rainbow-table-aanvallen blokkeert:

```csharp
using ShopWave.Security;

PasswordHasher hasher = new PasswordHasher();

// Twee gebruikers kiezen hetzelfde wachtwoord
string passwordAlice = "mijnWachtwoord123";
string passwordBob   = "mijnWachtwoord123";

string hashAlice = hasher.Hash(passwordAlice);
string hashBob   = hasher.Hash(passwordBob);

Console.WriteLine("Zelfde wachtwoord, twee BCrypt-hashes:");
Console.WriteLine($"Alice: {hashAlice}");
Console.WriteLine($"Bob:   {hashBob}");
Console.WriteLine($"Gelijk: {hashAlice == hashBob}");             // false — elke BCrypt-hash heeft eigen salt
Console.WriteLine();
Console.WriteLine($"Alice geverifieerd: {hasher.Verify(passwordAlice, hashAlice)}"); // true
Console.WriteLine($"Bob geverifieerd:   {hasher.Verify(passwordBob,   hashBob)}");   // true
```

Zelfs als Alice en Bob hetzelfde wachtwoord kiezen, krijgen ze een volledig andere hash. Een aanvaller die de database steelt, kan niet zien dat twee accounts hetzelfde wachtwoord gebruiken. BCrypt doet dit volledig automatisch — je hoeft geen salt apart te genereren of op te slaan.

---

### Stap 5 — CustomerAccount aanmaken

Maak in de map `Security` een nieuw bestand `CustomerAccount.cs` aan. Dit is het object dat een klant van ShopWave voorstelt, met correct beveiligde wachtwoordopslag:

```csharp
namespace ShopWave.Security
{
    public class CustomerAccount
    {
        public string Email        { get; private set; }
        public string PasswordHash { get; private set; }
        // Geen Salt-property nodig — BCrypt bewaart de salt intern in de PasswordHash-string

        private readonly PasswordHasher _hasher;

        public CustomerAccount(string email, string password)
        {
            _hasher = new PasswordHasher();

            Email        = email;
            PasswordHash = _hasher.Hash(password);
        }

        public bool VerifyPassword(string password)
        {
            return _hasher.Verify(password, PasswordHash);
        }
    }
}
```

Merk op: het plain-text wachtwoord wordt nergens opgeslagen. Alleen de BCrypt-hash wordt bewaard — de salt zit al ingebakken in die hash-string, dus je hoeft hem niet apart op te slaan.

---

### Stap 6 — AesEncryptor aanmaken

ShopWave wil gevoelige gegevens zoals orderreferenties versleuteld opslaan. Maak in de map `Security` een nieuw bestand `AesEncryptor.cs` aan:

```csharp
using System.Security.Cryptography;
using System.Text;

namespace ShopWave.Security
{
    public class AesEncryptor
    {
        private readonly byte[] _key;

        public AesEncryptor(string key)
        {
            // Key moet exact 32 bytes zijn voor AES-256
            string paddedKey = key.PadRight(32).Substring(0, 32);
            _key = Encoding.UTF8.GetBytes(paddedKey);
        }

        public string Encrypt(string plainText)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = _key;
                aes.GenerateIV(); // willekeurige IV per encryptie — NOOIT een vaste IV gebruiken

                ICryptoTransform encryptor = aes.CreateEncryptor();
                byte[] inputBytes     = Encoding.UTF8.GetBytes(plainText);
                byte[] encryptedBytes = encryptor.TransformFinalBlock(inputBytes, 0, inputBytes.Length);

                // IV (16 bytes) vooraan aan de ciphertext toevoegen zodat Decrypt hem terug kan lezen
                byte[] result = new byte[aes.IV.Length + encryptedBytes.Length];
                Array.Copy(aes.IV,        0, result, 0,             aes.IV.Length);
                Array.Copy(encryptedBytes, 0, result, aes.IV.Length, encryptedBytes.Length);

                return Convert.ToBase64String(result);
            }
        }

        public string Decrypt(string cipherText)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = _key;

                byte[] inputBytes = Convert.FromBase64String(cipherText);

                // IV terugextraheren uit de eerste 16 bytes
                byte[] iv             = new byte[16];
                byte[] encryptedBytes = new byte[inputBytes.Length - 16];
                Array.Copy(inputBytes, 0,  iv,             0, 16);
                Array.Copy(inputBytes, 16, encryptedBytes, 0, encryptedBytes.Length);

                aes.IV = iv;

                ICryptoTransform decryptor = aes.CreateDecryptor();
                byte[] decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);

                return Encoding.UTF8.GetString(decryptedBytes);
            }
        }
    }
}
```

> ⚠️ **Waarom een willekeurige IV?** Een vaste IV betekent dat dezelfde plaintext altijd dezelfde ciphertext oplevert. Dat lekt informatie: een aanvaller die twee identieke versleutelde waarden ziet, weet dat de originele data ook identiek was. Met een willekeurige IV per encryptie is dit niet mogelijk. De IV is geen geheim — hij wordt gewoon meegeopgeslagen bij de ciphertext.

Demonstreer in `Program.cs`:

```csharp
using ShopWave.Security;

AesEncryptor encryptor = new AesEncryptor(key: "ShopWaveGeheimeSleutel!");

string orderReference = "ORD-2024-00042";
string encrypted1     = encryptor.Encrypt(orderReference);
string encrypted2     = encryptor.Encrypt(orderReference);
string decrypted      = encryptor.Decrypt(encrypted1);

Console.WriteLine($"Origineel:     {orderReference}");
Console.WriteLine($"Versleuteld 1: {encrypted1}");
Console.WriteLine($"Versleuteld 2: {encrypted2}");
Console.WriteLine($"Gelijk:        {encrypted1 == encrypted2}"); // false — willekeurige IV
Console.WriteLine($"Ontsleuteld:   {decrypted}");
```

Observeer: dezelfde orderreferentie geeft elke keer een andere ciphertext, maar ontsleutelen geeft altijd het origineel terug.

---

## Oefeningen

Werk de oefeningen in volgorde. Elke oefening bouwt voort op de vorige en op de klassen uit de demo. Alle nieuwe klassen maak je aan in de map `Security` van het bestaande project `ShopWave`.

---

### Oefening 1 — CustomerAccount testen

Schrijf in `ShopWave.Tests` een nieuwe testklasse `CustomerAccountTests.cs`.

Test de volgende scenario's:

1. Na aanmaken van een `CustomerAccount` is `PasswordHash` niet leeg
2. Een BCrypt-hash begint met `$2a$` of `$2b$` (het BCrypt-formaat)
3. `VerifyPassword` geeft `true` terug bij het correcte wachtwoord
4. `VerifyPassword` geeft `false` terug bij een fout wachtwoord
5. Twee `CustomerAccount`-objecten met hetzelfde wachtwoord hebben een **verschillende** `PasswordHash` (BCrypt genereert elke keer een nieuwe salt)

Gebruik FluentAssertions voor alle assertions.

---

### Oefening 2 — AccountRepository

Maak een klasse `AccountRepository.cs` aan in de map `Security`:

```csharp
public class AccountRepository
{
    public void Register(string email, string password) { ... }
    public string Login(string email, string password)  { ... }
}
```

`Login` retourneert:
- `"Inloggen geslaagd."` bij de correcte combinatie
- `"Inloggen mislukt."` bij een fout wachtwoord
- `"Account geblokkeerd."` na 3 opeenvolgende mislukte pogingen voor hetzelfde account
- `"Gebruiker niet gevonden."` als het e-mailadres niet bestaat

Het aantal mislukte pogingen wordt gereset na een succesvolle login.

---

### Oefening 3 — AccountRepository testen

Schrijf in `ShopWave.Tests` een testklasse `AccountRepositoryTests.cs`.

Test minstens de volgende scenario's:

1. Registreer een gebruiker en log daarna in met het juiste wachtwoord → `"Inloggen geslaagd."`
2. Log in met een fout wachtwoord → `"Inloggen mislukt."`
3. Log in met een onbestaand e-mailadres → `"Gebruiker niet gevonden."`
4. Drie opeenvolgende foute pogingen → vierde poging geeft `"Account geblokkeerd."`, ook met het juiste wachtwoord
5. Na een succesvolle login wordt de teller gereset (een nieuwe foute poging telt opnieuw als poging 1)

---

### Oefening 4 — OrderEncryptor

Maak een klasse `OrderEncryptor.cs` aan in de map `Security`. Deze klasse gebruikt `AesEncryptor` intern en biedt een eenvoudigere interface specifiek voor ordergegevens:

```csharp
public class OrderEncryptor
{
    public string EncryptOrderData(string orderData)    { ... }
    public string DecryptOrderData(string encryptedData) { ... }
}
```

Definieer de key als private constante in de klasse zelf (geen hardcoded waarden uit de demo overnemen, verzin je eigen). De IV wordt automatisch willekeurig gegenereerd per encryptie — je hoeft hem niet als constante te definiëren.

Schrijf ook een testklasse `OrderEncryptorTests.cs` in `ShopWave.Tests` met minstens de volgende tests:

1. Versleutelde data is niet gelijk aan de originele string
2. Ontsleutelen van versleutelde data geeft de originele string terug
3. Twee keer versleutelen van dezelfde string geeft **verschillende** ciphertexts (door de willekeurige IV), maar ontsleutelen van elk resultaat geeft telkens de originele string terug

---

### Oefening 5 — CIA-analyse van ShopWave

Dit is een analyseopdracht. Bepaal voor elke situatie:
- Welke CIA-pijler(s) worden geschonden?
- Wat is de concrete impact voor ShopWave?
- Welke technische maatregel lost het probleem op?

**Situatie A:**
Een aanvaller heeft toegang gekregen tot de database van ShopWave. Alle wachtwoorden zijn opgeslagen als plain text.

**Situatie B:**
Een aanvaller slaagt erin de prijs van een product te wijzigen van 999 euro naar 1 euro in de database, zonder dat iemand dit merkt.

**Situatie C:**
ShopWave wordt aangevallen met een DDoS-aanval. De webshop is vier uur lang niet bereikbaar tijdens een drukke promotieperiode.

**Situatie D:**
Een medewerker stuurt per ongeluk de volledige klantenlijst inclusief adressen en telefoonnummers via e-mail naar een verkeerde ontvanger.

Noteer je analyse per situatie.

---

## Modeloplossing

> De modeloplossing is beschikbaar na het indienen van de labo-opdracht via Digitap.

---

### Modeloplossing Oefening 2 — AccountRepository

```csharp
namespace ShopWave.Security
{
    public class AccountRepository
    {
        private readonly Dictionary<string, CustomerAccount> _accounts;
        private readonly Dictionary<string, int> _failedAttempts;
        private const int MaxAttempts = 3;

        public AccountRepository()
        {
            _accounts       = new Dictionary<string, CustomerAccount>();
            _failedAttempts = new Dictionary<string, int>();
        }

        public void Register(string email, string password)
        {
            CustomerAccount account = new CustomerAccount(email, password);
            _accounts[email]        = account;
            _failedAttempts[email]  = 0;
        }

        public string Login(string email, string password)
        {
            string result;

            if (!_accounts.ContainsKey(email))
            {
                result = "Gebruiker niet gevonden.";
            }
            else if (_failedAttempts[email] >= MaxAttempts)
            {
                result = "Account geblokkeerd.";
            }
            else
            {
                bool correct = _accounts[email].VerifyPassword(password);

                if (correct)
                {
                    _failedAttempts[email] = 0;
                    result = "Inloggen geslaagd.";
                }
                else
                {
                    _failedAttempts[email]++;

                    if (_failedAttempts[email] >= MaxAttempts)
                    {
                        result = "Account geblokkeerd.";
                    }
                    else
                    {
                        result = "Inloggen mislukt.";
                    }
                }
            }

            return result;
        }
    }
}
```

---

### Modeloplossing Oefening 3 — AccountRepositoryTests

```csharp
using FluentAssertions;
using ShopWave.Security;

namespace ShopWave.Tests
{
    public class AccountRepositoryTests
    {
        [Fact]
        public void Login_WithCorrectPassword_ReturnsGeslaagd()
        {
            // Arrange
            AccountRepository repository = new AccountRepository();
            repository.Register("alice@shopwave.be", "wachtwoord123");

            // Act
            string result = repository.Login("alice@shopwave.be", "wachtwoord123");

            // Assert
            result.Should().Be("Inloggen geslaagd.");
        }

        [Fact]
        public void Login_WithWrongPassword_ReturnsMislukt()
        {
            // Arrange
            AccountRepository repository = new AccountRepository();
            repository.Register("alice@shopwave.be", "wachtwoord123");

            // Act
            string result = repository.Login("alice@shopwave.be", "foutWachtwoord");

            // Assert
            result.Should().Be("Inloggen mislukt.");
        }

        [Fact]
        public void Login_WithUnknownEmail_ReturnsNietGevonden()
        {
            // Arrange
            AccountRepository repository = new AccountRepository();

            // Act
            string result = repository.Login("onbekend@shopwave.be", "wachtwoord123");

            // Assert
            result.Should().Be("Gebruiker niet gevonden.");
        }

        [Fact]
        public void Login_AfterThreeFailedAttempts_ReturnsGeblokkeerd()
        {
            // Arrange
            AccountRepository repository = new AccountRepository();
            repository.Register("alice@shopwave.be", "wachtwoord123");

            repository.Login("alice@shopwave.be", "fout");
            repository.Login("alice@shopwave.be", "fout");
            repository.Login("alice@shopwave.be", "fout");

            // Act — vierde poging, ook met juist wachtwoord
            string result = repository.Login("alice@shopwave.be", "wachtwoord123");

            // Assert
            result.Should().Be("Account geblokkeerd.");
        }

        [Fact]
        public void Login_AfterSuccessfulLogin_ResetsFailedAttempts()
        {
            // Arrange
            AccountRepository repository = new AccountRepository();
            repository.Register("alice@shopwave.be", "wachtwoord123");

            repository.Login("alice@shopwave.be", "fout");
            repository.Login("alice@shopwave.be", "fout");
            repository.Login("alice@shopwave.be", "wachtwoord123"); // reset

            // Act — foute poging na reset moet als poging 1 tellen
            string result = repository.Login("alice@shopwave.be", "fout");

            // Assert
            result.Should().Be("Inloggen mislukt.");
        }
    }
}
```

---

## Samenvatting

| Concept | Wat je moet onthouden |
|---------|-----------------------|
| CIA-model | Confidentiality, Integrity, Availability — de drie pijlers van informatiebeveiliging |
| Confidentiality | Data afschermen via encryptie, authenticatie en autorisatie |
| Integrity | Data correct houden via hashing en digitale handtekeningen |
| Availability | Systemen bereikbaar houden via failover, monitoring en DDoS-bescherming |
| Hashing | Eénrichtingsverkeer — gebruik voor wachtwoorden en integriteitscontrole |
| BCrypt | Gebruik dit voor wachtwoorden in .NET — bewust traag, salt ingebakken in de hash-string |
| Salt | Willekeurige toevoeging aan het wachtwoord vóór het hashen — BCrypt doet dit automatisch |
| Encryptie | Tweerichtingsverkeer — gebruik voor data die je later moet kunnen lezen |
| AES | Symmetrische encryptie — dezelfde sleutel voor versleutelen en ontsleutelen |
| IV (Initialization Vector) | Willekeurige waarde per encryptie — altijd `GenerateIV()` gebruiken, nooit een vaste IV |
| Hashing vs encryptie | Wachtwoorden: hashing (BCrypt). Gevoelige data die je moet kunnen lezen: encryptie (AES) |

---

## Volgende les

In Les 3 gaan we dieper in op **Testing: TDD — Test Driven Development**. We bouwen nieuwe functionaliteit voor ShopWave volledig test-first: eerst de test schrijven, dan pas de implementatie.