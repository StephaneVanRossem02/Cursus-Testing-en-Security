---
title: "Les 2: Oplossingen - CIA, Hashing en Encryptie"
sidebar_label: "Oplossingen"
---

# Oplossingen: CIA, Hashing en Encryptie

**Bekijk dit pas nadat je de oefeningen zelf geprobeerd hebt.** Lees de toelichting ook als je het juist had.

---

## Oplossing 1: AccountRepository bouwen

```csharp
namespace ShopWave.Security
{
    public class AccountRepository
    {
        private readonly Dictionary<string, CustomerAccount> accounts;
        private readonly Dictionary<string, int>             failedAttempts;
        private const int MaxAttempts = 3;

        public AccountRepository()
        {
            accounts       = new Dictionary<string, CustomerAccount>();
            failedAttempts = new Dictionary<string, int>();
        }

        public void Register(string email, string password)
        {
            CustomerAccount account = new CustomerAccount(email, password);
            accounts[email]        = account;
            failedAttempts[email]  = 0;
        }

        public string Login(string email, string password)
        {
            string result;

            if (!accounts.ContainsKey(email))
            {
                result = "Gebruiker niet gevonden.";
            }
            else if (failedAttempts[email] >= MaxAttempts)
            {
                result = "Account geblokkeerd.";
            }
            else
            {
                bool correct = accounts[email].VerifyPassword(password);

                if (correct)
                {
                    failedAttempts[email] = 0;
                    result = "Inloggen geslaagd.";
                }
                else
                {
                    failedAttempts[email]++;

                    if (failedAttempts[email] >= MaxAttempts)
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

### Toelichting

De volgorde van de `if`-controles in `Login` is bewust. Eerst controleer je of het account bestaat. Daarna controleer je of het geblokkeerd is. Die tweede controle staat vóór `VerifyPassword`. Dat is belangrijk: BCrypt-verificatie is opzettelijk traag (100+ milliseconden). Als je eerst `VerifyPassword` aanroept en daarna de blokkering controleert, laat je voor elk geblokkeerd account een trage berekening uitvoeren. Een aanvaller kan dat misbruiken om de server te belasten.

`failedAttempts[email]++` telt de foute poging op. Direct daarna controleer je of het maximum bereikt is. Zo geeft de derde foute poging al de melding `"Account geblokkeerd."` in plaats van `"Inloggen mislukt."`.

**Veelgemaakte fout:** studenten resetten de teller niet na een succesvolle login. Daarna tellen eerdere foute pogingen mee. Na twee foute pogingen en één correcte poging zou de volgende foute poging dan het account blokkeren. Dat is niet de bedoeling.

---

## Oplossing 2: Wachtwoordsterkte afdwingen

### PasswordValidator.cs

```csharp
namespace ShopWave.Security
{
    public class PasswordValidator
    {
        private const string SpecialCharacters = "!@#$%^&*";

        public bool IsValid(string password)
        {
            return GetErrorMessage(password) == string.Empty;
        }

        public string GetErrorMessage(string password)
        {
            string result = string.Empty;

            bool hasUppercase = false;
            bool hasDigit     = false;
            bool hasSpecial   = false;

            foreach (char c in password)
            {
                if (char.IsUpper(c))
                {
                    hasUppercase = true;
                }

                if (char.IsDigit(c))
                {
                    hasDigit = true;
                }

                if (SpecialCharacters.Contains(c))
                {
                    hasSpecial = true;
                }
            }

            if (password.Length < 8)
            {
                result = "Wachtwoord moet minstens 8 tekens lang zijn.";
            }
            else if (!hasUppercase)
            {
                result = "Wachtwoord moet minstens één hoofdletter bevatten.";
            }
            else if (!hasDigit)
            {
                result = "Wachtwoord moet minstens één cijfer bevatten.";
            }
            else if (!hasSpecial)
            {
                result = "Wachtwoord moet minstens één speciaal teken bevatten (!@#$%^&*).";
            }

            return result;
        }
    }
}
```

### AccountRepository uitbreiden

Pas `Register` aan zodat hij een `string` teruggeeft:

```csharp
private readonly PasswordValidator validator;

public AccountRepository()
{
    accounts       = new Dictionary<string, CustomerAccount>();
    failedAttempts = new Dictionary<string, int>();
    validator      = new PasswordValidator();
}

public string Register(string email, string password)
{
    string result;

    if (accounts.ContainsKey(email))
    {
        result = "Account bestaat al.";
    }
    else
    {
        string error = validator.GetErrorMessage(password);

        if (error != string.Empty)
        {
            result = error;
        }
        else
        {
            CustomerAccount account = new CustomerAccount(email, password);
            accounts[email]        = account;
            failedAttempts[email]  = 0;

            result = "Registratie geslaagd.";
        }
    }

    return result;
}
```

### Toelichting

`IsValid` roept intern `GetErrorMessage` aan en controleert of de foutmelding leeg is. Zo heb je nooit twee implementaties van dezelfde logica.

De volgorde van controles in `Register` is bewust: eerst controleer je of het e-mailadres al bestaat, daarna het wachtwoord. Als je het omgekeerd doet, geef je een aanvaller informatie: hij ziet aan de foutmelding dat het e-mailadres al bestaat, ook al heeft hij een zwak wachtwoord ingegeven.

De methode gebruikt één lus die alle drie de eigenschappen tegelijk opzoekt, en daarna één `if`-`else if`-keten die de eerste ontbrekende eigenschap in een `result`-variabele zet. Zo heeft de methode maar één `return` op het einde. Dat is de stijl die we in heel ShopWave aanhouden: één uitgang per methode, geen `break` en geen tussentijdse `return`.

**Veelgemaakte fout:** studenten schrijven drie aparte lussen, één per eigenschap. Dat werkt, maar je loopt dan drie keer over hetzelfde wachtwoord. Met drie booleans in één lus doe je hetzelfde werk in één doorloop.

---

## Oplossing 3: OrderEncryptor bouwen

### OrderEncryptor.cs

```csharp
namespace ShopWave.Security
{
    public class OrderEncryptor
    {
        private const  string       KeyString = "ShopWaveOrderSleutel!!";
        private readonly AesEncryptor aes;

        public OrderEncryptor()
        {
            aes = new AesEncryptor(KeyString);
        }

        public string EncryptOrderData(string orderData)
        {
            return aes.Encrypt(orderData);
        }

        public string DecryptOrderData(string encryptedData)
        {
            return aes.Decrypt(encryptedData);
        }
    }
}
```

### OrderRepository.cs

```csharp
namespace ShopWave.Security
{
    public class OrderRepository
    {
        private readonly Dictionary<string, string> orders;
        private readonly OrderEncryptor             encryptor;

        public OrderRepository()
        {
            orders    = new Dictionary<string, string>();
            encryptor = new OrderEncryptor();
        }

        public void SaveOrder(string orderId, string orderData)
        {
            orders[orderId] = encryptor.EncryptOrderData(orderData);
        }

        public string GetOrder(string orderId)
        {
            string result;

            if (!orders.ContainsKey(orderId))
            {
                result = string.Empty;
            }
            else
            {
                result = encryptor.DecryptOrderData(orders[orderId]);
            }

            return result;
        }
    }
}
```

### Toelichting

`OrderEncryptor` is een wrapper rond `AesEncryptor`. De sleutel is verborgen in de klasse: de aanroeper hoeft niet te weten welke sleutel gebruikt wordt of hoe AES werkt.

`OrderRepository` slaat de versleutelde string op in de dictionary. Als je de dictionary inspecteert, zie je alleen versleutelde data. Dat simuleert een database: een aanvaller die de database steelt, ziet geen leesbare orderdata.

**Veelgemaakte fout:** studenten maken een nieuwe `AesEncryptor` aan in `EncryptOrderData` en een andere in `DecryptOrderData`. Omdat beide encryptors dezelfde sleutel krijgen, werkt dat nog steeds. Maar het is zuiverder om de encryptor éénmalig aan te maken in de constructor en hem intern te hergebruiken.

De sleutel geef je als gewone string mee. `AesEncryptor` vult die zelf aan of kapt hem af tot exact 32 bytes (zie de constructor in de theorie). Je hoeft dus zelf geen `PadRight` of `Encoding.UTF8.GetBytes` te schrijven.

---

## Oplossing 4: Versleutelde klantnotities

```csharp
namespace ShopWave.Security
{
    public class CustomerNotesService
    {
        private const  string        KeyString = "ShopWaveNotitiesSleutel!";
        private readonly AesEncryptor  aes;
        private readonly Dictionary<string, string> encryptedNotes;

        public CustomerNotesService()
        {
            aes            = new AesEncryptor(KeyString);
            encryptedNotes = new Dictionary<string, string>();
        }

        public void AddNote(string email, string note)
        {
            encryptedNotes[email] = aes.Encrypt(note);
        }

        public string GetNote(string email)
        {
            string result;

            if (!encryptedNotes.ContainsKey(email))
            {
                result = string.Empty;
            }
            else
            {
                result = aes.Decrypt(encryptedNotes[email]);
            }

            return result;
        }

        public bool HasNote(string email)
        {
            return encryptedNotes.ContainsKey(email);
        }

        public void DeleteNote(string email)
        {
            encryptedNotes.Remove(email);
        }

        public Dictionary<string, string> ExportEncryptedNotes()
        {
            return new Dictionary<string, string>(encryptedNotes);
        }
    }
}
```

### Toelichting

`encryptedNotes` slaat nooit plain-text op. `AddNote` versleutelt onmiddellijk bij opslag. `GetNote` ontsleutelt bij opvragen. Een medewerker die rechtstreeks in de interne dictionary kijkt, ziet altijd versleutelde data.

`ExportEncryptedNotes` geeft een kopie terug van de dictionary, niet de dictionary zelf. Zo kan de aanroeper de interne toestand niet wijzigen door de dictionary aan te passen.

`HasNote` controleert alleen of er een sleutel bestaat voor dat e-mailadres, zonder te ontsleutelen. Dat is sneller en veiliger: je vermijdt een ontsleuteling als je alleen wil weten of er een notitie is.

**Veelgemaakte fout:** studenten slaan de notitie plain-text op en versleutelen alleen in `ExportEncryptedNotes`. Daardoor is de data intern leesbaar, en beschermt de klasse alleen de export, niet de interne opslag.

---

## Oplossing 5: CIA-analyse van ShopWave

**Situatie A: plain-text wachtwoorden**

CIA-pijler: **Confidentiality**. Wachtwoorden zijn vertrouwelijke gegevens. Als ze als plain text opgeslagen zijn en de database gestolen wordt, heeft een aanvaller direct toegang tot alle accounts. De technische maatregel is BCrypt-hashing: een aanvaller ziet dan alleen onleesbare hashes, en BCrypt is opzettelijk traag zodat brute force tijdrovend is.

**Situatie B: prijs gewijzigd in de database**

CIA-pijler: **Integrity**. De data in de database is gewijzigd zonder dat iemand het merkt. De impact voor ShopWave is financieel: producten worden verkocht voor 1 EUR in plaats van 999 EUR. Een technische maatregel is een digitale handtekening op prijswijzigingen, of strikte toegangscontrole op de database zodat niet elke medewerker prijzen kan wijzigen.

**Situatie C: DDoS-aanval**

CIA-pijler: **Availability**. De webshop is niet beschikbaar voor klanten. De financiële impact is verlies van omzet tijdens de promotieperiode. Mogelijke maatregelen: DDoS-bescherming via een CDN (CloudFlare), rate limiting, of extra servercapaciteit die automatisch opschaalt bij piekbelasting.

**Situatie D: klantnotities leesbaar**

CIA-pijler: **Confidentiality**. Interne notities bevatten gevoelige informatie over klanten. Als een medewerker die rechtstreeks in de database kan lezen, is er geen scheiding tussen de applicatielaag en de datalaag. Oefening 4 lost dit op door notities versleuteld op te slaan via `AesEncryptor`. Een medewerker die de database inspecteert, ziet alleen ciphertext, nooit de leesbare notities.

---

## Dit project downloaden

[Download het volledige ShopWave-project van les 2](/downloads/shopwave-02-cia-hashing-en-encryptie.zip) (ZIP)

Bevat alle code tot en met deze les, klaar om te openen in Visual Studio. Bouwen en testen doe je met `dotnet build` en `dotnet test`. In de `README.md` staat wat er nieuw is en hoeveel tests er horen te slagen.

Alle lessen samen vind je op [Oplossingen downloaden](../../oplossingen-downloaden.md).
