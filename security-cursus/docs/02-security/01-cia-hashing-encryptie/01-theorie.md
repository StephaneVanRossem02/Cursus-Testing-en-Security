---
title: "Les 2: Theorie - CIA, Hashing en Encryptie"
sidebar_label: "Theorie"
---

# Theorie: CIA, Hashing en Encryptie

## 1. Waarom security?

In les 1 bouwde je unit tests om te controleren dat de code van ShopWave correct werkt. Maar "correct werkt" is niet hetzelfde als "veilig is".

Stel dat de database van ShopWave gestolen wordt. Als wachtwoorden als plain text opgeslagen zijn, heeft een aanvaller onmiddellijk toegang tot alle accounts. Niet alleen op ShopWave: de meeste mensen hergebruiken wachtwoorden. Één gelekt wachtwoord opent dus meerdere deuren.

Stel dat een aanvaller een bestelbedrag aanpast van 999 euro naar 1 euro. Als er geen integriteitscontrole is, merkt niemand dat. De klant betaalt 1 euro. ShopWave levert een laptop.

Stel dat ShopWave aangevallen wordt tijdens Black Friday. De webshop is vier uur lang niet bereikbaar. Duizenden klanten gaan naar de concurrent. Reputatieschade die maanden duurt.

Dit zijn geen extreem zeldzame situaties. Ze zijn allemaal al gebeurd bij grote bedrijven. Als ontwikkelaar kan je ze voorkomen door de juiste beveiligingsmaatregelen toe te passen.

**Mini-controle:** noem voor elk van de drie scenario's hierboven de concrete schade: voor de klant, voor ShopWave en voor de maatschappij.

---

## 2. Het CIA-model

Het **CIA-model** is het basiskader voor informatiebeveiliging. Elke beveiligingsbeslissing die je als ontwikkelaar neemt, is terug te brengen tot één of meerdere van de drie pijlers.

CIA staat voor:

- **Confidentiality** (vertrouwelijkheid): data is alleen toegankelijk voor wie er recht op heeft
- **Integrity** (integriteit): data is correct, volledig en ongewijzigd
- **Availability** (beschikbaarheid): systemen en data zijn bereikbaar wanneer nodig

**Hoe dit eruitziet voor ShopWave:**

| Pijler | Vraag die je stelt | Voorbeeld in ShopWave |
|--------|--------------------|-----------------------|
| Confidentiality | Is de informatie afgeschermd voor onbevoegden? | Wachtwoorden mogen niet leesbaar zijn als de database uitlekt |
| Integrity | Is de informatie correct en ongewijzigd? | Een bestelbedrag mag niet aangepast zijn door een aanvaller |
| Availability | Is de informatie beschikbaar wanneer nodig? | De webshop moet bereikbaar zijn, ook tijdens piekdrukte |

Een veilig systeem houdt alle drie pijlers in balans. Een systeem dat alleen focust op vertrouwelijkheid maar beschikbaarheid verwaarloost, is even gevaarlijk als een systeem dat niets beveiligt.

### Confidentiality in de praktijk

Je bereikt vertrouwelijkheid via encryptie, authenticatie en autorisatie.

**Encryptie** maakt data onleesbaar zonder de juiste sleutel. Dit geldt bij verzending (HTTPS) en bij opslag (versleutelde database).

**Authenticatie** bepaalt wie je bent. **Autorisatie** bepaalt wat je mag doen. Een klant bij ShopWave kan zijn eigen bestellingen zien, maar niet die van andere klanten.

Bekende fouten: de Sony PlayStation Network-aanval in 2011 legde 77 miljoen accounts bloot, inclusief wachtwoorden en creditcardgegevens, door gebrekkige encryptie. Een simpele e-mail met CC in plaats van BCC lekt alle e-mailadressen van alle ontvangers aan elkaar.

### Integrity in de praktijk

Je bereikt integriteit via hashing en digitale handtekeningen.

**Hashing** maakt een unieke digitale vingerafdruk van een stuk data. Elke wijziging, hoe klein ook, levert een volledig andere hash op. Je kan hiermee controleren of data ongewijzigd is.

**Digitale handtekeningen** bewijzen dat de inhoud afkomstig is van de juiste afzender en onderweg niet gewijzigd is.

Bekende fouten: hackers passen rekeningnummers aan in e-mailfacturen. Het geld belandt bij de aanvaller. In de zorgsector kan een fout in een patiëntendossier leiden tot verkeerde medicatie.

### Availability in de praktijk

Je bereikt beschikbaarheid via DDoS-bescherming, failover-systemen en monitoring.

**DDoS** (Distributed Denial of Service) is een aanval waarbij servers overspoeld worden met verkeer totdat ze crashen. Een failover-systeem neemt automatisch het werk over als een server uitvalt.

Bekende fouten: een configuratiefout legde Facebook, Instagram en WhatsApp in 2021 wereldwijd urenlang plat. Bedrijven zonder werkende back-up verloren bij ransomware-aanvallen alle data permanent.

**Mini-controle:** een medewerker stuurt de volledige klantenlijst per ongeluk naar een verkeerde ontvanger. Welke CIA-pijler(s) worden geschonden? Confidentiality: de data is toegankelijk voor iemand zonder recht. Mogelijk ook Integrity als de ontvanger de lijst kan aanpassen.

---

## 3. Hashing

### Wat is een hash?

Een **hashfunctie** zet een invoerwaarde van willekeurige lengte om naar een vaste uitvoer: de **hash** of **digest**. Die uitvoer heeft altijd dezelfde lengte, ongeacht hoe lang de invoer is.

Eigenschappen van een goede hashfunctie:

- **Deterministisch**: dezelfde invoer geeft altijd dezelfde hash
- **Eénrichtingsverkeer**: je kan de originele invoer niet afleiden uit de hash
- **Kleine wijziging, grote verandering**: één karakter anders in de invoer geeft een volledig andere hash
- **Botsingsresistent**: twee verschillende invoerwaarden mogen niet dezelfde hash opleveren

**Voorbeeld met SHA-256:**

| Invoer | Hash (verkort) |
|--------|---------------|
| `hallo` | `d3751d33...` |
| `Hallo` | `185f8db3...` |
| `hallo ` (met spatie) | `volledig andere hash` |

Eén hoofdletter verschil geeft een volledig andere hash. Dit maakt hashing ideaal om te detecteren of data gewijzigd is.

### Hashing voor wachtwoorden

Hashing lost een concreet probleem op: hoe sla je een wachtwoord veilig op?

**Het probleem:** wachtwoorden als plain text opslaan.

```csharp
// NOOIT DOEN
string opgeslagenWachtwoord = "mijnGeheimWachtwoord123";
```

Als de database gestolen wordt, zijn alle wachtwoorden onmiddellijk leesbaar.

**De oplossing:** hash het wachtwoord bij registratie en vergelijk hashes bij login.

```
Bij registratie:  hash(wachtwoord)           →  sla de hash op in de database
Bij login:        hash(ingegeven wachtwoord)  →  vergelijk met de opgeslagen hash
```

Je slaat het originele wachtwoord nooit op. Alleen de hash. En uit de hash kan niemand het wachtwoord terugberekenen.

**Mini-controle:** een gebruiker vraagt "kan je mijn wachtwoord opzoeken? Ik ben het vergeten." Als jouw applicatie correct ontworpen is, is het antwoord: nee. Waarom? Omdat je alleen de hash opgeslagen hebt. Die kan je niet terugzetten naar het origineel. De juiste oplossing is een nieuw wachtwoord instellen.

---

## 4. Salt en rainbow tables

Hashing alleen is niet voldoende voor wachtwoorden. Stel dat een aanvaller de database steelt en alle hashes ziet. Als twee gebruikers hetzelfde wachtwoord gekozen hebben, hebben ze ook dezelfde hash. Dat lekt informatie.

Bovendien bestaat er een techniek genaamd de **rainbow table**: een vooraf berekende lijst van hashes voor miljoenen veelgebruikte wachtwoorden. Een aanvaller zoekt de gestolen hash op in de tabel en kent onmiddellijk het wachtwoord.

De oplossing is een **salt**: een willekeurige waarde die je toevoegt aan het wachtwoord vóór het hashen.

```
hash("wachtwoord")              →  altijd dezelfde hash
hash("wachtwoord" + zout1234)   →  unieke hash voor deze gebruiker
hash("wachtwoord" + koffie789)  →  andere unieke hash voor een andere gebruiker
```

Elke gebruiker krijgt een eigen, willekeurig gegenereerde salt. Twee gebruikers met hetzelfde wachtwoord krijgen zo twee volledig verschillende hashes. Een rainbow table werkt niet meer, want die is opgebouwd zonder die specifieke salt.

Je slaat de salt op naast de hash in de database, zodat je bij login de hash opnieuw kan berekenen met dezelfde salt.

**Mini-controle:** waarom is een salt geen geheim? Als een aanvaller de database steelt, heeft hij ook de salts. Maar dat maakt de rainbow table-aanval nog steeds onmogelijk. Elke salt is uniek per gebruiker, dus de aanvaller moet voor elke gebruiker apart een nieuwe rainbow table bouwen. Dat is computationeel onhaalbaar.

---

## 5. Welke hashfunctie gebruik je voor wachtwoorden?

**SHA-256 is niet geschikt voor wachtwoorden.** SHA-256 is een algemene hashfunctie die ontworpen is om zo snel mogelijk te zijn. Op een moderne GPU worden miljarden SHA-256-hashes per seconde berekend. Dat is ideaal voor integriteitscontroles op grote bestanden, maar gevaarlijk voor wachtwoorden: een aanvaller kan in korte tijd alle mogelijke wachtwoorden uitproberen.

Voor wachtwoorden gebruik je een hashfunctie die bewust **traag** gemaakt is:

| Algoritme | NuGet-pakket | Aanbevolen |
|-----------|-------------|-----------|
| **BCrypt** | `BCrypt.Net-Next` | Ja, eenvoudig en bewezen |
| **PBKDF2** | Ingebouwd in .NET (`Rfc2898DeriveBytes`) | Ja |
| **Argon2** | `Konscious.Security.Cryptography` | Ja, modernste optie |
| SHA-256 | Ingebouwd | Nee, te snel voor wachtwoorden |

**BCrypt** is de meest gebruikte keuze in .NET-projecten. BCrypt heeft nog een extra voordeel: het berekent automatisch een willekeurige salt en bewaart die samen met de hash in één string. Je hoeft de salt niet apart op te slaan.

```csharp
// Hashen bij registratie
string hash = BCrypt.Net.BCrypt.HashPassword(password);

// Verifiëren bij login
bool ok = BCrypt.Net.BCrypt.Verify(password, storedHash);
```

De string die `HashPassword` teruggeeft, ziet er zo uit:

```
$2b$11$R9h/cIPz0gi.URNNX3kh2OPST9/PgBkqquzi.Ss7KIUgO2t0jWMUW
```

Die string bevat alles wat `Verify` later nodig heeft: het algoritme (`$2b$`), de kostfactor (`11`), de salt (de volgende 22 tekens) en de hash zelf. Alles in één veld in de database.

**Mini-controle:** je ziet in de database twee rijen voor gebruikers die allebei "wachtwoord123" gekozen hebben. Allebei hebben ze een BCrypt-hash. Zijn die hashes gelijk? Nee. BCrypt genereert voor elke aanroep van `HashPassword` een nieuwe willekeurige salt. Dezelfde invoer geeft elke keer een andere hash. Toch werkt `Verify` correct, want de salt zit ingebakken in de hash-string.

---

## 6. Encryptie

### Wat is encryptie?

**Encryptie** zet leesbare data (plaintext) om naar onleesbare data (ciphertext) met behulp van een sleutel. Met de juiste sleutel kan je de data terug ontsleutelen.

Dit is het fundamentele verschil met hashing: encryptie is **tweerichtingsverkeer**. Je kan de originele data terugkrijgen. Hashing is eénrichtingsverkeer. Je kan de originele data niet terugkrijgen.

**Vuistregel:**
- Gebruik **hashing** als je de originele waarde nooit hoeft te lezen. Voorbeeld: wachtwoorden.
- Gebruik **encryptie** als je de originele waarde later wél nodig hebt. Voorbeeld: een creditcardnummer dat je later moet tonen aan de klant.

| Eigenschap | Hashing | Encryptie |
|-----------|---------|-----------|
| Richting | Eénrichtingsverkeer | Tweerichtingsverkeer |
| Terugkeerbaar? | Nee | Ja, met de juiste sleutel |
| Gebruik | Wachtwoorden, integriteitscontrole | Vertrouwelijke data die je later moet lezen |

### Symmetrische encryptie

Bij **symmetrische encryptie** gebruik je dezelfde sleutel om te versleutelen én te ontsleutelen.

```
Plaintext  +  sleutel  →  Ciphertext
Ciphertext +  sleutel  →  Plaintext
```

Voordeel: snel en eenvoudig. Nadeel: beide partijen moeten dezelfde sleutel kennen. Hoe deel je die sleutel veilig?

In .NET gebruik je **AES** (Advanced Encryption Standard) voor symmetrische encryptie. AES is de wereldwijde industriestandaard.

### Asymmetrische encryptie

Bij **asymmetrische encryptie** gebruik je een sleutelpaar: een publieke sleutel om te versleutelen, een private sleutel om te ontsleutelen.

```
Plaintext  +  publieke sleutel  →  Ciphertext
Ciphertext +  private sleutel   →  Plaintext
```

Voordeel: je hoeft de private sleutel nooit te delen. Iedereen mag de publieke sleutel kennen. Nadeel: trager dan symmetrische encryptie.

Asymmetrische encryptie komt terug in les 4 bij HTTPS en digitale handtekeningen.

### IV: Initialization Vector

Bij AES is er nog één belangrijk concept: de **IV** (Initialization Vector). De IV is een willekeurige waarde die je toevoegt aan de encryptieoperatie.

Zonder IV geeft dezelfde plaintext altijd dezelfde ciphertext. Een aanvaller die twee identieke versleutelde waarden ziet in de database, weet dat de originele data ook identiek was. Dat lekt informatie.

Met een willekeurige IV geeft dezelfde plaintext elke keer een andere ciphertext. De IV is geen geheim. Je slaat hem samen met de ciphertext op, bijvoorbeeld als prefix. Bij het ontsleutelen lees je de IV terug uit die prefix.

```csharp
aes.GenerateIV(); // altijd aanroepen, nooit een vaste IV gebruiken
```

**Mini-controle:** je versleutelt het creditcardnummer van twee klanten die allebei dezelfde kaart gebruiken. Zonder IV zijn de twee versleutelde waarden in de database identiek. Wat kan een aanvaller hieruit afleiden? Dat beide klanten hetzelfde kaartnummer hebben. Met een willekeurige IV zijn de twee waarden volledig verschillend, ook al is de invoer identiek.

---

## 7. Demo: wachtwoordbeveiliging stap voor stap

We bouwen de beveiligingslaag van ShopWave in zes stappen. We werken in de bestaande ShopWave-solution uit les 1.

---

### Stap 1: map aanmaken

Rechtermuisklik op het project `ShopWave` in Solution Explorer. Kies **Add > New Folder**. Noem de map `Security`.

Alle klassen van deze les komen in die map.

Bouw de solution. Geen fouten.

---

### Stap 2: het probleem laten zien

Maak `ShopWave/Security/UserRepository.cs` aan:

```csharp
namespace ShopWave.Security
{
    public class UserRepository
    {
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

Dit werkt, maar als de database uitlekt, ziet een aanvaller alle wachtwoorden onmiddellijk. We lossen dit stap voor stap op.

Wat je ziet als je de solution bouwt:

```
Build succeeded.
```

---

### Stap 3: BCrypt installeren en PasswordHasher aanmaken

Installeer `BCrypt.Net-Next` via de Package Manager Console:

```
Install-Package BCrypt.Net-Next
```

Maak `ShopWave/Security/PasswordHasher.cs` aan:

```csharp
namespace ShopWave.Security
{
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

Voeg tijdelijk toe aan `Program.cs`:

```csharp
using ShopWave.Security;

PasswordHasher hasher = new PasswordHasher();

string password = "mijnWachtwoord123";
string hash1    = hasher.Hash(password);
string hash2    = hasher.Hash(password);

Console.WriteLine($"Hash 1: {hash1}");
Console.WriteLine($"Hash 2: {hash2}");
Console.WriteLine($"Gelijk: {hash1 == hash2}");
Console.WriteLine($"Verificatie: {hasher.Verify(password, hash1)}");
```

Voer uit.

Wat je ziet:

```
Hash 1: $2b$11$R9h/cIPz0gi.URNNX3kh2O...
Hash 2: $2b$11$Kq8OUx7A9bY3dRv2mNpZ1e...
Gelijk: False
Verificatie: True
```

Hetzelfde wachtwoord geeft elke keer een andere hash, omdat BCrypt intern een willekeurige salt genereert. Toch werkt `Verify` correct: BCrypt leest de salt terug uit de hash-string.

---

### Stap 4: twee gebruikers, hetzelfde wachtwoord

Voeg toe aan `Program.cs`:

```csharp
string hashAlice = hasher.Hash("mijnWachtwoord123");
string hashBob   = hasher.Hash("mijnWachtwoord123");

Console.WriteLine($"Alice: {hashAlice}");
Console.WriteLine($"Bob:   {hashBob}");
Console.WriteLine($"Gelijk: {hashAlice == hashBob}");
```

Wat je ziet:

```
Alice: $2b$11$...een waarde...
Bob:   $2b$11$...andere waarde...
Gelijk: False
```

Zelfs als Alice en Bob hetzelfde wachtwoord kiezen, krijgen ze een volledig andere hash. Een aanvaller die de database steelt, kan niet zien dat twee accounts hetzelfde wachtwoord gebruiken.

---

### Stap 5: CustomerAccount aanmaken

Maak `ShopWave/Security/CustomerAccount.cs` aan:

```csharp
namespace ShopWave.Security
{
    public class CustomerAccount
    {
        public string Email        { get; private set; }
        public string PasswordHash { get; private set; }

        private readonly PasswordHasher _hasher;

        public CustomerAccount(string email, string password)
        {
            _hasher      = new PasswordHasher();
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

Let op: het plain-text wachtwoord wordt nergens opgeslagen. Alleen de BCrypt-hash wordt bewaard. De salt zit ingebakken in die hash-string, dus je hebt geen aparte property nodig.

Bouw de solution.

Wat je ziet:

```
Build succeeded.
```

---

### Stap 6: AesEncryptor aanmaken

ShopWave wil gevoelige ordergegevens versleuteld opslaan. Maak `ShopWave/Security/AesEncryptor.cs` aan:

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
            string paddedKey = key.PadRight(32).Substring(0, 32);
            _key = Encoding.UTF8.GetBytes(paddedKey);
        }

        public string Encrypt(string plainText)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = _key;
                aes.GenerateIV();

                ICryptoTransform encryptor = aes.CreateEncryptor();
                byte[] inputBytes     = Encoding.UTF8.GetBytes(plainText);
                byte[] encryptedBytes = encryptor.TransformFinalBlock(
                    inputBytes, 0, inputBytes.Length);

                byte[] result = new byte[aes.IV.Length + encryptedBytes.Length];
                Array.Copy(aes.IV,         0, result, 0,             aes.IV.Length);
                Array.Copy(encryptedBytes, 0, result, aes.IV.Length, encryptedBytes.Length);

                return Convert.ToBase64String(result);
            }
        }

        public string Decrypt(string cipherText)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = _key;

                byte[] inputBytes     = Convert.FromBase64String(cipherText);
                byte[] iv             = new byte[16];
                byte[] encryptedBytes = new byte[inputBytes.Length - 16];

                Array.Copy(inputBytes, 0,  iv,             0, 16);
                Array.Copy(inputBytes, 16, encryptedBytes, 0, encryptedBytes.Length);

                aes.IV = iv;

                ICryptoTransform decryptor    = aes.CreateDecryptor();
                byte[]           decryptedBytes = decryptor.TransformFinalBlock(
                    encryptedBytes, 0, encryptedBytes.Length);

                return Encoding.UTF8.GetString(decryptedBytes);
            }
        }
    }
}
```

Voeg toe aan `Program.cs`:

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
Console.WriteLine($"Gelijk:        {encrypted1 == encrypted2}");
Console.WriteLine($"Ontsleuteld:   {decrypted}");
```

Voer uit.

Wat je ziet:

```
Origineel:     ORD-2024-00042
Versleuteld 1: Xy7mNpQ2r...
Versleuteld 2: Bk3aLvM8s...
Gelijk:        False
Ontsleuteld:   ORD-2024-00042
```

Dezelfde orderreferentie geeft elke keer een andere ciphertext, door de willekeurige IV. Ontsleutelen geeft altijd het origineel terug.

---

## 8. Samenvatting

| Concept | Wat je moet onthouden |
|--------|-----------------------|
| CIA-model | Confidentiality, Integrity, Availability: de drie pijlers van informatiebeveiliging |
| Confidentiality | Data afschermen via encryptie, authenticatie en autorisatie |
| Integrity | Data correct houden via hashing en digitale handtekeningen |
| Availability | Systemen bereikbaar houden via failover, monitoring en DDoS-bescherming |
| Hashing | Eénrichtingsverkeer: gebruik voor wachtwoorden en integriteitscontrole |
| Salt | Willekeurige toevoeging aan het wachtwoord vóór het hashen; voorkomt rainbow-table-aanvallen |
| BCrypt | Gebruik voor wachtwoorden in .NET: bewust traag, salt automatisch ingebakken in de hash-string |
| SHA-256 | Niet geschikt voor wachtwoorden: te snel, brute force wordt triviaal |
| Encryptie | Tweerichtingsverkeer: gebruik voor data die je later moet kunnen lezen |
| AES | Symmetrische encryptie: dezelfde sleutel voor versleutelen en ontsleutelen |
| IV | Willekeurige waarde per encryptieoperatie; gebruik altijd `GenerateIV()`, nooit een vaste IV |
| Hashing vs encryptie | Wachtwoorden: BCrypt. Gevoelige data die je moet kunnen lezen: AES. |
