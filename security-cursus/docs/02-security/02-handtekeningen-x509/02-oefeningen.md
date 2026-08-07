---
title: "Les 4: Oefeningen - 2FA, Handtekeningen en X.509"
sidebar_label: "Oefeningen"
---

# Oefeningen: 2FA, Handtekeningen en X.509

Werk de oefeningen in volgorde. Elke oefening bouwt verder op de vorige. Kijk niet vooraf in de oplossingen.

Je werkt verder in de bestaande ShopWave-solution. Nieuwe klassen maak je aan in `ShopWave/Security/`.

---

## Startpakket downloaden

[Download het startpakket van les 4](/downloads/shopwave-start-04-2fa-handtekeningen-en-x509.zip) (ZIP)

Hierin staat alles wat je in de vorige lessen gebouwd hebt, samen met de code die je
tijdens de theorie van deze les opbouwt. Wat je in de oefeningen zelf moet schrijven,
staat erin als skelet met de melding `// jouw code hier`.

De webshop zit erbij. Je hoeft geen Razor te kennen: start hem met
`dotnet run --project ShopWave.Web` en open http://localhost:5000. Zo zie je meteen wat je code doet.

---

<h3 class="opdracht-titel">Opdracht</h3>

## Oefening 1: Wachtwoordreset via 2FA

**Leerdoel:** je past het 2FA-principe toe in een nieuw scenario en ziet hoe dezelfde techniek hergebruikt wordt.

**Moeilijkheidsgraad:** basis

**Situatie:** een klant van ShopWave is zijn wachtwoord vergeten. ShopWave wil een veilige resetprocedure: de klant vraagt een resetcode aan, ontvangt die via e-mail (gesimuleerd via een callback), en kan daarna een nieuw wachtwoord instellen.

**Wat je doet:**

Maak `ShopWave/Security/PasswordResetService.cs` aan. Deze klasse heeft drie methoden:

- `RequestReset(string email, Action<string, string> onCodeSent)`: genereert een willekeurige 6-cijferige code voor dit e-mailadres en roept de callback aan met het e-mailadres en de code. De code vervalt na 15 minuten.
- `VerifyCode(string email, string code)`: controleert of de code geldig en niet verlopen is. Geeft `true` of `false` terug. De code wordt na verificatie verwijderd (eenmalig bruikbaar).
- `ResetPassword(string email, string code, string newPassword, AccountRepository accounts)`: verifieert de code en, als die geldig is, wijzigt het wachtwoord in `AccountRepository`. Geeft een leesbare string terug: `"Wachtwoord gewijzigd."` of `"Ongeldige of verlopen code."`.

**Vereisten:**

- Gebruik `RandomNumberGenerator.GetInt32` voor de code, net als in `TwoFactorService`.
- De callback heeft dezelfde signatuur als in `AccountRepository`: `Action<string, string>`.
- Hergebruik `PendingCode` voor het opslaan van de code met vervaltijd.
- `AccountRepository` heeft nog geen `ChangePassword`-methode. Voeg die zelf toe.

**Startcode:**

```csharp
using System.Security.Cryptography;

namespace ShopWave.Security
{
    public class PasswordResetService
    {
        private readonly Dictionary<string, PendingCode> pendingResets;

        public PasswordResetService()
        {
            pendingResets = new Dictionary<string, PendingCode>();
        }

        public void RequestReset(string email, Action<string, string> onCodeSent)
        {
            // jouw code hier
        }

        public bool VerifyCode(string email, string code)
        {
            // jouw code hier
            return false;
        }

        public string ResetPassword(
            string            email,
            string            code,
            string            newPassword,
            AccountRepository accounts)
        {
            // jouw code hier
            return string.Empty;
        }
    }
}
```

**Controleer je werk:** voeg tijdelijk toe aan `Program.cs`:

```csharp
PasswordResetService resetService = new PasswordResetService();

resetService.RequestReset(
    "alice@shopwave.be",
    onCodeSent: (email, code) => Console.WriteLine($"[RESET] Code voor {email}: {code}"));

Console.Write("Voer de resetcode in: ");
string resetCode = Console.ReadLine() ?? string.Empty;

TwoFactorService  twoFactor  = new TwoFactorService();
AccountRepository repository = new AccountRepository(twoFactor);
repository.Register("alice@shopwave.be", "oudWachtwoord");

string result = resetService.ResetPassword(
    "alice@shopwave.be", resetCode, "nieuwWachtwoord123", repository);

Console.WriteLine(result);
```

Verwacht resultaat:

```csharp
[RESET] Code voor alice@shopwave.be: 739201
Voer de resetcode in: 739201
Wachtwoord gewijzigd.
```

---

<h3 class="opdracht-titel">Opdracht</h3>

## Oefening 2: Maximaal aantal 2FA-pogingen

**Leerdoel:** je breidt een bestaande beveiligingsklasse uit met een extra beveiligingsregel.

**Moeilijkheidsgraad:** gemiddeld

**Situatie:** een aanvaller die het wachtwoord van een klant kent, kan onbeperkt 2FA-codes proberen. ShopWave wil dat na 3 foute pogingen de 2FA-code geblokkeerd wordt. De klant moet dan opnieuw inloggen om een nieuwe code te ontvangen.

**Wat je doet:**

Breid `TwoFactorService` uit met pogingentelling:

- Voeg een `Dictionary<string, int>` toe die per e-mailadres bijhoudt hoeveel foute pogingen er zijn.
- In `VerifyCode`: verhoog de teller bij een foute code. Als de teller 3 of meer bereikt, verwijder dan de code en reset de teller. Verdere pogingen voor dat e-mailadres geven altijd `false`, ook als de code ooit correct was.
- Bij een geslaagde verificatie: reset de teller voor dat e-mailadres.
- Voeg een methode `GetRemainingAttempts(string email)` toe die teruggeeft hoeveel pogingen de klant nog heeft. Geeft 3 terug als er nog geen foute poging was.

**Vereisten:**

- Pas alleen `TwoFactorService` aan. Andere klassen mogen niet gewijzigd worden.
- Na het blokkeren geeft `GenerateCode` wel weer een nieuwe code, maar de pogingenteller start dan opnieuw op nul.

**Controleer je werk:** voeg tijdelijk toe aan `Program.cs`:

```csharp
TwoFactorService service = new TwoFactorService();
string code = service.GenerateCode("alice@shopwave.be");

Console.WriteLine($"Pogingen resterend: {service.GetRemainingAttempts("alice@shopwave.be")}");
service.VerifyCode("alice@shopwave.be", "000000");
Console.WriteLine($"Pogingen resterend: {service.GetRemainingAttempts("alice@shopwave.be")}");
service.VerifyCode("alice@shopwave.be", "000000");
Console.WriteLine($"Pogingen resterend: {service.GetRemainingAttempts("alice@shopwave.be")}");
service.VerifyCode("alice@shopwave.be", "000000");
Console.WriteLine($"Pogingen resterend: {service.GetRemainingAttempts("alice@shopwave.be")}");

bool result = service.VerifyCode("alice@shopwave.be", code);
Console.WriteLine($"Geldig na blokkering: {result}");
```

Verwacht resultaat:

```csharp
Pogingen resterend: 3
Pogingen resterend: 2
Pogingen resterend: 1
Pogingen resterend: 0
Geldig na blokkering: False
```

---

<h3 class="opdracht-titel">Opdracht</h3>

## Oefening 3: Factuur ondertekenen

**Leerdoel:** je past digitale handtekeningen toe in een nieuw scenario en ziet dat het principe identiek is, ongeacht het type document.

**Moeilijkheidsgraad:** gemiddeld

**Situatie:** ShopWave verstuurt ook facturen aan klanten. Die facturen moeten digitaal ondertekend worden zodat een klant kan bewijzen dat een factuur echt van ShopWave komt en onderweg niet gewijzigd is.

**Wat je doet:**

Maak `ShopWave/Security/InvoiceSigner.cs` aan. Deze klasse werkt hetzelfde als `OrderSigner` maar voor facturen.

Een factuurstring heeft het formaat:

```csharp
FACT-{nummer} | {datum} | {klant} | {bedrag} EUR
```

Voorbeeld: `"FACT-2024-0042 | 2024-11-15 | alice@shopwave.be | 1249.99 EUR"`

De klasse heeft twee methoden:

- `Sign(string invoiceData)`: ondertekent de factuurstring en geeft de handtekening terug als Base64-string.
- `Verify(string invoiceData, string signature)`: verifieert of de handtekening overeenkomt met de factuurstring. Geeft `true` of `false` terug.

Maak ook `ShopWave/Security/InvoiceSignerFactory.cs` aan. Deze statische klasse heeft één methode:

- `Create()`: maakt een self-signed certificaat aan met `CertificateHelper` en geeft een nieuwe `InvoiceSigner` terug.

**Vereisten:**

- `InvoiceSigner` krijgt het certificaat via de constructor, net als `OrderSigner`.
- `InvoiceSigner` mag de code van `OrderSigner` niet kopiëren. Gebruik overerving of extraheer de gemeenschappelijke logica.

**Tip:** als je de handtekening manueel verandert (ook maar één teken), moet `Verify` `false` teruggeven.

**Controleer je werk:** voeg tijdelijk toe aan `Program.cs`:

```csharp
InvoiceSigner signer = InvoiceSignerFactory.Create();

string invoice   = "FACT-2024-0042 | 2024-11-15 | alice@shopwave.be | 1249.99 EUR";
string signature = signer.Sign(invoice);

Console.WriteLine($"Factuur:      {invoice}");
Console.WriteLine($"Geldig:       {signer.Verify(invoice, signature)}");

string gemanipuleerd = invoice.Replace("1249.99", "0.01");
Console.WriteLine($"Gemanipuleerd: {signer.Verify(gemanipuleerd, signature)}");
```

Verwacht resultaat:

```csharp
Factuur:      FACT-2024-0042 | 2024-11-15 | alice@shopwave.be | 1249.99 EUR
Geldig:       True
Gemanipuleerd: False
```

---

<h3 class="opdracht-titel">Opdracht</h3>

## Oefening 4: Versleuteld én ondertekend document

**Leerdoel:** je combineert encryptie (confidentiality) en handtekeningen (integrity) in één stroom en begrijpt in welke volgorde dat moet.

**Moeilijkheidsgraad:** uitdaging

**Situatie:** ShopWave wil dat gevoelige orderdata zowel verborgen als integer is. Versleuteling verbergt de inhoud (confidentiality). Een handtekening bewijst dat de versleutelde data niet gewijzigd is (integrity). Beide samen geven de maximale bescherming.

**Wat je doet:**

Maak `ShopWave/Security/SecureOrderDocument.cs` aan met twee methoden:

- `Protect(string orderData)`: versleutelt de orderdata met `AesEncryptor` en ondertekent daarna de **versleutelde** data met `OrderSigner`. Geeft een object terug met twee strings: `EncryptedData` en `Signature`.
- `Unprotect(string encryptedData, string signature)`: verifieert eerst de handtekening van de versleutelde data. Als die geldig is, ontsleutelt en geeft de originele orderdata terug. Als de handtekening niet klopt, gooit hij een `InvalidOperationException` met de boodschap `"Handtekening ongeldig. Data mogelijk gemanipuleerd."`.

Maak ook `ShopWave/Security/ProtectedOrder.cs` aan:

```csharp
namespace ShopWave.Security
{
    public class ProtectedOrder
    {
        public string EncryptedData { get; }
        public string Signature     { get; }

        public ProtectedOrder(string encryptedData, string signature)
        {
            EncryptedData = encryptedData;
            Signature     = signature;
        }
    }
}
```

**Vereisten:**

- `SecureOrderDocument` maakt intern een `AesEncryptor` aan met een vaste sleutel (gebruik `"ShopWaveGeheimeSleutel!!"` als string, PadRight(32)).
- `SecureOrderDocument` maakt intern een `OrderSigner` aan via `CertificateHelper`.
- De handtekening wordt altijd gezet op de **versleutelde** data, niet op de plain-text data. Zo detecteer je ook manipulaties van de ciphertext.

**Controleer je werk:** voeg tijdelijk toe aan `Program.cs`:

```csharp
SecureOrderDocument doc = new SecureOrderDocument();

string orderData      = "ORD-001 | alice@shopwave.be | Laptop | 999.99 EUR";
ProtectedOrder order  = doc.Protect(orderData);

Console.WriteLine($"Versleuteld: {order.EncryptedData[..40]}...");

string restored = doc.Unprotect(order.EncryptedData, order.Signature);
Console.WriteLine($"Hersteld:    {hersteld}");

try
{
    doc.Unprotect("gemanipuleerdeData", order.Signature);
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"Fout: {ex.Message}");
}
```

Verwacht resultaat:

```csharp
Versleuteld: a3Fk9mNpQ2rBk3aLvM8sRt1wJc9dFe6h...
Hersteld:    ORD-001 | alice@shopwave.be | Laptop | 999.99 EUR
Fout: Handtekening ongeldig. Data mogelijk gemanipuleerd.
```

---

<h3 class="opdracht-titel">Opdracht</h3>

## Oefening 5: CIA-koppeling

**Leerdoel:** je verbindt de technische keuzes uit de vorige oefeningen met het CIA-model.

**Moeilijkheidsgraad:** basis

Beantwoord de volgende vragen op papier of in een tekstbestand.

1. In oefening 4 onderteken je de **versleutelde** data in plaats van de originele data. Waarom is die volgorde belangrijk? Wat zou er fout gaan als je eerst ondertekende en daarna versleutelde?

2. `PasswordResetService` uit oefening 1 gebruikt een code die na 15 minuten verloopt. Welke beveiligingsdoelstelling rechtvaardigt die beperkte geldigheid? Wat zou er kunnen misgaan met een code die nooit verloopt?

3. Welke CIA-pijler beschermt `AesEncryptor` en welke beschermt `OrderSigner`? Kan een systeem dat alleen `AesEncryptor` gebruikt, ooit bewijzen dat een orderbevestiging niet gemanipuleerd is? Leg uit.

4. ShopWave overweegt het wachtwoord van een klant te bewaren als een AES-encryptie in plaats van als een BCrypt-hash, zodat een medewerker het wachtwoord kan opvragen als de klant het vergeten is. Leg uit waarom dit een beveiligingsfout is. Welke CIA-pijler is hier in het geding?

---

## Controleer je werk in de webshop

Start de webshop met `dotnet run --project ShopWave.Web` en open http://localhost:5000. Zo zie je je eigen code draaien in plaats van alleen een groene testbalk.

| Wat je doet | Wat je ziet als je code klopt |
|-------------|-------------------------------|
| Log in met een geldig wachtwoord | Je komt in stap 2 en de 2FA-code verschijnt op het scherm |
| Voer een foute code in | `Ongeldige 2FA-code.` uit jouw `VerifyTwoFactor` |
| Vraag een nieuwe code en voer die correct in | `Inloggen geslaagd.` |
| Gebruik diezelfde code nog eens | Afgewezen: een code werkt maar één keer |
| Vraag een reset aan bij **Wachtwoord vergeten** | De resetcode verschijnt, en na invoeren `Wachtwoord gewijzigd.` |
| Ga naar **Orderbevestiging** en wijzig één teken in de tekst | De handtekening wordt ongeldig: `OrderSigner.Verify()` geeft false |

Onder elk resultaat staat uit welke klasse het komt. Zie je iets anders dan hierboven, dan weet je meteen welke methode je moet nakijken.
