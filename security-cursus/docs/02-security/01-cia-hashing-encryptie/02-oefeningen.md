---
title: "Les 2: Oefeningen - CIA, Hashing en Encryptie"
sidebar_label: "Oefeningen"
---

# Oefeningen: CIA, Hashing en Encryptie

Werk de oefeningen in volgorde. Elke oefening bouwt verder op de vorige. Kijk niet vooraf in de oplossingen.

Je werkt in de bestaande ShopWave-solution. Alle nieuwe klassen maak je aan in `ShopWave/Security/`.

---

<h3 class="opdracht-titel">Opdracht</h3>

## Oefening 1: AccountRepository bouwen

**Leerdoel:** je implementeert een inlogsysteem dat wachtwoorden correct hasht en brute-force-aanvallen afremt via lockout.

**Moeilijkheidsgraad:** basis

**Situatie:** ShopWave heeft een inlogsysteem nodig. Na drie opeenvolgende foute pogingen wordt een account geblokkeerd om brute-force-aanvallen te voorkomen.

**Wat je doet:**

Maak `ShopWave/Security/AccountRepository.cs` aan met twee methoden:

- `Register(string email, string password)`: maakt een nieuw account aan. Gebruik `CustomerAccount` intern voor wachtwoordopslag.
- `Login(string email, string password)`: controleert de inloggegevens en geeft een leesbare string terug.

`Login` geeft precies één van de volgende strings terug:

| Situatie | Returnwaarde |
|----------|-------------|
| Correct wachtwoord | `"Inloggen geslaagd."` |
| Fout wachtwoord | `"Inloggen mislukt."` |
| Drie opeenvolgende foute pogingen | `"Account geblokkeerd."` |
| E-mailadres bestaat niet | `"Gebruiker niet gevonden."` |

**Aanvullende regels:**

- De foutenteller wordt gereset na een succesvolle login.
- Een geblokkeerd account blijft geblokkeerd, ook als daarna het juiste wachtwoord ingevoerd wordt.
- Je hebt twee interne datastructuren nodig: één voor de accounts en één voor de foutentellers.

**Startcode:**

```csharp
namespace ShopWave.Security
{
    public class AccountRepository
    {
        private readonly Dictionary<string, CustomerAccount> _accounts;
        private readonly Dictionary<string, int>             _failedAttempts;
        private const int MaxAttempts = 3;

        public AccountRepository()
        {
            _accounts       = new Dictionary<string, CustomerAccount>();
            _failedAttempts = new Dictionary<string, int>();
        }

        public void Register(string email, string password)
        {
            // jouw code hier
        }

        public string Login(string email, string password)
        {
            // jouw code hier
            return string.Empty;
        }
    }
}
```

**Controleer je werk:** voeg tijdelijk toe aan `Program.cs`:

```csharp
AccountRepository repository = new AccountRepository();
repository.Register("alice@shopwave.be", "wachtwoord123");

Console.WriteLine(repository.Login("alice@shopwave.be", "fout"));
Console.WriteLine(repository.Login("alice@shopwave.be", "fout"));
Console.WriteLine(repository.Login("alice@shopwave.be", "fout"));
Console.WriteLine(repository.Login("alice@shopwave.be", "wachtwoord123"));
```

Verwacht resultaat:

```csharp
Inloggen mislukt.
Inloggen mislukt.
Account geblokkeerd.
Account geblokkeerd.
```

---

<h3 class="opdracht-titel">Opdracht</h3>

## Oefening 2: Wachtwoordsterkte afdwingen

**Leerdoel:** je begrijpt waarom zwakke wachtwoorden een beveiligingsrisico zijn en implementeert validatieregels die dat risico verkleinen.

**Moeilijkheidsgraad:** basis

**Situatie:** een klant van ShopWave kiest het wachtwoord `"123"`. BCrypt hasht dat correct, maar een aanvaller kan het in seconden raden. ShopWave wil minimale wachtwoordeisen afdwingen bij registratie.

**Wat je doet:**

Maak `ShopWave/Security/PasswordValidator.cs` aan:

```csharp
namespace ShopWave.Security
{
    public class PasswordValidator
    {
        public bool IsValid(string password)     { ... }
        public string GetErrorMessage(string password) { ... }
    }
}
```

Een wachtwoord is geldig als het aan alle vier de regels voldoet:

1. Minstens 8 tekens lang
2. Minstens één hoofdletter (`A`-`Z`)
3. Minstens één cijfer (`0`-`9`)
4. Minstens één speciaal teken uit de reeks: `!@#$%^&*`

`GetErrorMessage` geeft de eerste overtreden regel terug als leesbare foutmelding. Als het wachtwoord geldig is, geeft hij een lege string terug.

Pas daarna `AccountRepository.Register` aan zodat registratie mislukt als het wachtwoord ongeldig is. Voeg daarvoor een returnwaarde toe aan `Register`:

```csharp
public string Register(string email, string password)
```

| Situatie | Returnwaarde |
|----------|-------------|
| Registratie geslaagd | `"Registratie geslaagd."` |
| Wachtwoord ongeldig | De foutmelding van `PasswordValidator` |
| E-mailadres al in gebruik | `"E-mailadres al in gebruik."` |

**Controleer je werk:** voeg tijdelijk toe aan `Program.cs`:

```csharp
AccountRepository repository = new AccountRepository();

Console.WriteLine(repository.Register("alice@shopwave.be", "123"));
Console.WriteLine(repository.Register("alice@shopwave.be", "wachtwoord"));
Console.WriteLine(repository.Register("alice@shopwave.be", "Wachtwoord1!"));
Console.WriteLine(repository.Register("alice@shopwave.be", "Wachtwoord1!"));
```

Verwacht resultaat:

```csharp
Wachtwoord moet minstens 8 tekens lang zijn.
Wachtwoord moet minstens één hoofdletter bevatten.
Registratie geslaagd.
E-mailadres al in gebruik.
```

---

<h3 class="opdracht-titel">Opdracht</h3>

## Oefening 3: OrderEncryptor bouwen

**Leerdoel:** je implementeert een klasse die `AesEncryptor` omhult en gevoelige ordergegevens versleuteld opslaat.

**Moeilijkheidsgraad:** gemiddeld

**Situatie:** ShopWave wil gevoelige ordergegevens versleuteld bewaren in de database. `AesEncryptor` uit de demo werkt rechtstreeks met een sleutelarray. `OrderEncryptor` verbergt die sleutel intern zodat de rest van de applicatie er niet over hoeft na te denken.

**Wat je doet:**

Maak `ShopWave/Security/OrderEncryptor.cs` aan:

```csharp
namespace ShopWave.Security
{
    public class OrderEncryptor
    {
        public string EncryptOrderData(string orderData)     { ... }
        public string DecryptOrderData(string encryptedData) { ... }
    }
}
```

**Vereisten:**

- Definieer de AES-sleutel als `private const string` in de klasse. Gebruik `"ShopWaveOrderSleutel!!"` als waarde en pas die aan naar 32 bytes via `PadRight(32)`.
- Gebruik `AesEncryptor` intern. Maak die aan in de constructor.
- `EncryptOrderData` geeft de versleutelde string terug.
- `DecryptOrderData` geeft de originele string terug.

Maak daarna `ShopWave/Security/OrderRepository.cs` aan. `OrderRepository` bewaart orders als versleutelde strings en kan ze ophalen:

```csharp
namespace ShopWave.Security
{
    public class OrderRepository
    {
        private readonly Dictionary<string, string> _orders;
        private readonly OrderEncryptor             _encryptor;

        public OrderRepository()
        {
            _orders    = new Dictionary<string, string>();
            _encryptor = new OrderEncryptor();
        }

        public void SaveOrder(string orderId, string orderData) { ... }
        public string GetOrder(string orderId)                  { ... }
    }
}
```

`SaveOrder` versleutelt de orderdata voor opslag. `GetOrder` haalt de versleutelde data op en ontsleutelt die.

**Controleer je werk:** voeg tijdelijk toe aan `Program.cs`:

```csharp
OrderRepository orders = new OrderRepository();

orders.SaveOrder("ORD-001", "alice@shopwave.be | Laptop | 999.99 EUR");
orders.SaveOrder("ORD-002", "bob@shopwave.be   | Muis   |  29.99 EUR");

Console.WriteLine(orders.GetOrder("ORD-001"));
Console.WriteLine(orders.GetOrder("ORD-002"));
```

Verwacht resultaat:

```csharp
alice@shopwave.be | Laptop | 999.99 EUR
bob@shopwave.be   | Muis   |  29.99 EUR
```

---

<h3 class="opdracht-titel">Opdracht</h3>

## Oefening 4: Versleutelde klantnotities

**Leerdoel:** je past AES-encryptie toe in een realistisch scenario en denkt na over welke data versleuteld moet worden en waarom.

**Moeilijkheidsgraad:** uitdaging

**Situatie:** de klantenservice van ShopWave wil interne notities bijhouden per klant: bezorkinstructies, VIP-status, opmerkingen. Die notities bevatten gevoelige informatie en mogen niet leesbaar opgeslagen worden. Als de database gestolen wordt, mogen aanvallers die notities niet kunnen lezen.

**Wat je doet:**

Maak `ShopWave/Security/CustomerNotesService.cs` aan:

```csharp
namespace ShopWave.Security
{
    public class CustomerNotesService
    {
        public void AddNote(string email, string note)           { ... }
        public string GetNote(string email)                      { ... }
        public bool HasNote(string email)                        { ... }
        public void DeleteNote(string email)                     { ... }
    }
}
```

**Vereisten:**

- Notities worden intern versleuteld opgeslagen via `AesEncryptor`. De sleutel definieer je als `private const string`.
- `GetNote` ontsleutelt en geeft de leesbare notitie terug. Als er geen notitie bestaat, geeft hij een lege string terug.
- `HasNote` geeft `true` als er een (versleutelde) notitie bestaat voor dat e-mailadres.
- `DeleteNote` verwijdert de notitie.
- Als je de interne dictionary van `CustomerNotesService` zou inspecteren, zie je alleen versleutelde data, nooit de plain-text notities.

Voeg daarna een methode `ExportEncryptedNotes` toe die een overzicht geeft van alle opgeslagen notities in hun **versleutelde** vorm. Dat simuleert wat een aanvaller ziet als hij de database steelt.

```csharp
public Dictionary<string, string> ExportEncryptedNotes() { ... }
```

**Controleer je werk:** voeg tijdelijk toe aan `Program.cs`:

```csharp
CustomerNotesService notes = new CustomerNotesService();

notes.AddNote("alice@shopwave.be", "VIP-klant. Altijd prioriteit geven.");
notes.AddNote("bob@shopwave.be",   "Bezorging: achterdeur gebruiken.");

Console.WriteLine($"Alice heeft notitie: {notes.HasNote("alice@shopwave.be")}");
Console.WriteLine($"Notitie Alice: {notes.GetNote("alice@shopwave.be")}");

Console.WriteLine("\nWat een aanvaller ziet:");
foreach (var entry in notes.ExportEncryptedNotes())
{
    Console.WriteLine($"{entry.Key}: {entry.Value[..30]}...");
}
```

Verwacht resultaat:

```csharp
Alice heeft notitie: True
Notitie Alice: VIP-klant. Altijd prioriteit geven.

Wat een aanvaller ziet:
alice@shopwave.be: a3Fk9mNpQ2rBk3aLvM8sRt1w...
bob@shopwave.be: Xy7mNpQ2rBk3aLvM8sRt1wJc9...
```

---

<h3 class="opdracht-titel">Opdracht</h3>

## Oefening 5: CIA-analyse van ShopWave

**Leerdoel:** je past het CIA-model toe op concrete situaties en koppelt beveiligingsproblemen aan technische oplossingen.

**Moeilijkheidsgraad:** basis

Beantwoord de volgende vragen op papier of in een tekstbestand.

**Situatie A:** een aanvaller heeft toegang gekregen tot de database van ShopWave. Alle wachtwoorden zijn opgeslagen als plain text. Welke CIA-pijler is geschonden? Wat is de impact? Welke technische maatregel had dit voorkomen?

**Situatie B:** een aanvaller slaagt erin de prijs van een product te wijzigen van 999 EUR naar 1 EUR in de database, zonder dat iemand dit merkt. Welke CIA-pijler is geschonden? Wat is de impact voor ShopWave?

**Situatie C:** ShopWave wordt aangevallen met een DDoS-aanval. De webshop is vier uur lang niet bereikbaar tijdens een drukke promotieperiode. Welke CIA-pijler is geschonden? Welke maatregel kan ShopWave nemen om de impact te beperken?

**Situatie D:** een medewerker van ShopWave kan de klantnotities uit oefening 4 lezen door rechtstreeks in de database te kijken, omdat de notities als plain text opgeslagen zijn. Welke CIA-pijler is hier in het geding? Wat lost oefening 4 precies op?
