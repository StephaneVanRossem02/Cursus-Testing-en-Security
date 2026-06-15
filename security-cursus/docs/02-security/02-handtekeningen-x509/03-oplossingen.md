---
title: "Les 4: Oplossingen - 2FA, Handtekeningen en X.509"
sidebar_label: "Oplossingen"
---

# Oplossingen: 2FA, Handtekeningen en X.509

**Bekijk dit pas nadat je de oefeningen zelf geprobeerd hebt.** Lees de toelichting ook als je het juist had.

---

## Oplossing 1: Wachtwoordreset via 2FA

### ChangePassword toevoegen aan AccountRepository

```csharp
public bool ChangePassword(string email, string newPassword)
{
    if (!_accounts.ContainsKey(email))
    {
        return false;
    }

    _accounts[email] = new CustomerAccount(email, newPassword);
    return true;
}
```

### PasswordResetService.cs

```csharp
using System.Security.Cryptography;

namespace ShopWave.Security
{
    public class PasswordResetService
    {
        private readonly Dictionary<string, PendingCode> _pendingResets;

        public PasswordResetService()
        {
            _pendingResets = new Dictionary<string, PendingCode>();
        }

        public void RequestReset(string email, Action<string, string> onCodeSent)
        {
            string   code      = RandomNumberGenerator.GetInt32(100000, 999999).ToString();
            DateTime expiresAt = DateTime.UtcNow.AddMinutes(15);

            _pendingResets[email] = new PendingCode(code, expiresAt);

            onCodeSent(email, code);
        }

        public bool VerifyCode(string email, string code)
        {
            bool isValid = false;

            if (_pendingResets.ContainsKey(email))
            {
                PendingCode pending = _pendingResets[email];

                if (DateTime.UtcNow <= pending.ExpiresAt && pending.Code == code)
                {
                    isValid = true;
                }

                _pendingResets.Remove(email);
            }

            return isValid;
        }

        public string ResetPassword(
            string            email,
            string            code,
            string            newPassword,
            AccountRepository accounts)
        {
            if (VerifyCode(email, code))
            {
                accounts.ChangePassword(email, newPassword);
                return "Wachtwoord gewijzigd.";
            }

            return "Ongeldige of verlopen code.";
        }
    }
}
```

### Toelichting

`VerifyCode` werkt identiek aan `TwoFactorService.VerifyCode`: de code wordt altijd verwijderd na de eerste verificatiepoging, ongeacht of die gelukt is. Zo kan een aanvaller die een resetcode onderschepte, die code niet opnieuw gebruiken nadat de eigenaar hem al geprobeerd heeft.

`ChangePassword` maakt een nieuw `CustomerAccount`-object aan met het nieuwe wachtwoord. `CustomerAccount` hasht het wachtwoord in de constructor via BCrypt. Je hoeft het nieuwe wachtwoord dus niet zelf te hashen.

**Veelgemaakte fout:** studenten roepen `VerifyCode` twee keer aan: één keer om te controleren en een tweede keer binnen `ChangePassword`. De tweede aanroep retourneert altijd `false` omdat de code al verwijderd is na de eerste aanroep.

---

## Oplossing 2: Maximaal aantal 2FA-pogingen

Aanpassingen in `TwoFactorService`:

```csharp
using System.Security.Cryptography;

namespace ShopWave.Security
{
    public class TwoFactorService
    {
        private readonly Dictionary<string, PendingCode> _pendingCodes;
        private readonly Dictionary<string, int>         _failedAttempts;
        private readonly int                             _validitySeconds;

        public TwoFactorService(int validitySeconds = 30)
        {
            _pendingCodes   = new Dictionary<string, PendingCode>();
            _failedAttempts = new Dictionary<string, int>();
            _validitySeconds = validitySeconds;
        }

        public string GenerateCode(string email)
        {
            string   code      = RandomNumberGenerator.GetInt32(100000, 999999).ToString();
            DateTime expiresAt = DateTime.UtcNow.AddSeconds(_validitySeconds);

            _pendingCodes[email]   = new PendingCode(code, expiresAt);
            _failedAttempts[email] = 0;

            return code;
        }

        public bool VerifyCode(string email, string code)
        {
            bool isValid = false;

            if (_pendingCodes.ContainsKey(email))
            {
                PendingCode pending = _pendingCodes[email];

                if (DateTime.UtcNow <= pending.ExpiresAt && pending.Code == code)
                {
                    isValid                = true;
                    _failedAttempts[email] = 0;
                    _pendingCodes.Remove(email);
                }
                else
                {
                    _failedAttempts[email] = _failedAttempts.ContainsKey(email)
                        ? _failedAttempts[email] + 1
                        : 1;

                    if (_failedAttempts[email] >= 3)
                    {
                        _pendingCodes.Remove(email);
                    }
                }
            }

            return isValid;
        }

        public int GetRemainingAttempts(string email)
        {
            if (!_failedAttempts.ContainsKey(email))
            {
                return 3;
            }

            int remaining = 3 - _failedAttempts[email];
            return remaining < 0 ? 0 : remaining;
        }
    }
}
```

### Toelichting

`GenerateCode` reset de teller op nul bij het aanmaken van een nieuwe code. Zo start elke nieuwe loginpoging met een schone lei.

In `VerifyCode` staat `_pendingCodes.Remove(email)` nu **binnen** de `if`-blokken in plaats van altijd buiten. Bij een geslaagde verificatie wordt de code direct verwijderd. Bij een mislukte verificatie na 3 pogingen wordt de code ook verwijderd. Bij minder dan 3 mislukte pogingen blijft de code staan zodat de klant nog een kans heeft.

**Veelgemaakte fout:** studenten laten `_pendingCodes.Remove(email)` buiten alle `if`-blokken staan (zoals in de originele versie). Daardoor wordt de code na elke verificatiepoging verwijderd. De pogingenteller heeft dan geen effect: er is nooit een code om te blokkeren.

---

## Oplossing 3: Factuur ondertekenen

De gemeenschappelijke logica van `OrderSigner` en `InvoiceSigner` is identiek. Extraheer die in een basisklasse zodat je de code maar één keer schrijft.

### DocumentSigner.cs (basisklasse)

```csharp
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace ShopWave.Security
{
    public abstract class DocumentSigner
    {
        private readonly X509Certificate2 _certificate;

        protected DocumentSigner(X509Certificate2 certificate)
        {
            _certificate = certificate;
        }

        public string Sign(string data)
        {
            RSA privateKey = _certificate.GetRSAPrivateKey()!;

            byte[] dataBytes      = Encoding.UTF8.GetBytes(data);
            byte[] signatureBytes = privateKey.SignData(
                dataBytes,
                HashAlgorithmName.SHA256,
                RSASignaturePadding.Pkcs1);

            return Convert.ToBase64String(signatureBytes);
        }

        public bool Verify(string data, string signature)
        {
            RSA publicKey = _certificate.GetRSAPublicKey()!;

            byte[] dataBytes      = Encoding.UTF8.GetBytes(data);
            byte[] signatureBytes = Convert.FromBase64String(signature);

            return publicKey.VerifyData(
                dataBytes,
                signatureBytes,
                HashAlgorithmName.SHA256,
                RSASignaturePadding.Pkcs1);
        }
    }
}
```

### InvoiceSigner.cs

```csharp
using System.Security.Cryptography.X509Certificates;

namespace ShopWave.Security
{
    public class InvoiceSigner : DocumentSigner
    {
        public InvoiceSigner(X509Certificate2 certificate) : base(certificate)
        {
        }
    }
}
```

### InvoiceSignerFactory.cs

```csharp
namespace ShopWave.Security
{
    public static class InvoiceSignerFactory
    {
        public static InvoiceSigner Create()
        {
            return new InvoiceSigner(
                CertificateHelper.CreateSelfSignedCertificate("ShopWave"));
        }
    }
}
```

### OrderSigner.cs aanpassen

Nu `DocumentSigner` bestaat, pas je ook `OrderSigner` aan zodat die de basisklasse gebruikt:

```csharp
using System.Security.Cryptography.X509Certificates;

namespace ShopWave.Security
{
    public class OrderSigner : DocumentSigner
    {
        public OrderSigner(X509Certificate2 certificate) : base(certificate)
        {
        }
    }
}
```

### Toelichting

`abstract` op de basisklasse betekent dat je nooit een `DocumentSigner` direct aanmaakt, alleen subklassen zoals `OrderSigner` of `InvoiceSigner`. Als je later het hash-algoritme wil wijzigen van SHA256 naar SHA512, pas je het maar op één plek aan.

**Veelgemaakte fout:** studenten kopiëren de volledige code van `OrderSigner` naar `InvoiceSigner`. Dat werkt functioneel, maar maakt onderhoud moeilijk.

---

## Oplossing 4: Versleuteld én ondertekend document

### ProtectedOrder.cs

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

### SecureOrderDocument.cs

```csharp
using System.Text;

namespace ShopWave.Security
{
    public class SecureOrderDocument
    {
        private readonly AesEncryptor _encryptor;
        private readonly OrderSigner  _signer;

        public SecureOrderDocument()
        {
            byte[] key = Encoding.UTF8.GetBytes(
                "ShopWaveGeheimeSleutel!!".PadRight(32));

            _encryptor = new AesEncryptor(key);
            _signer    = new OrderSigner(
                CertificateHelper.CreateSelfSignedCertificate("ShopWave"));
        }

        public ProtectedOrder Protect(string orderData)
        {
            string encryptedData = _encryptor.Encrypt(orderData);
            string signature     = _signer.Sign(encryptedData);

            return new ProtectedOrder(encryptedData, signature);
        }

        public string Unprotect(string encryptedData, string signature)
        {
            bool signatureValid = _signer.Verify(encryptedData, signature);

            if (!signatureValid)
            {
                throw new InvalidOperationException(
                    "Handtekening ongeldig. Data mogelijk gemanipuleerd.");
            }

            return _encryptor.Decrypt(encryptedData);
        }
    }
}
```

### Toelichting

De volgorde in `Protect` is cruciaal: **eerst versleutelen, daarna ondertekenen**. De handtekening beschermt zo de versleutelde data. Als een aanvaller ook maar één byte van de ciphertext wijzigt, detecteert `Verify` dat.

In `Unprotect` controleer je de handtekening altijd als eerste stap. Nooit eerst ontsleutelen en dan controleren. Als de handtekening ongeldig is, mag je de data niet vertrouwen.

**Veelgemaakte fout:** studenten ondertekenen de plain-text orderdata in plaats van de versleutelde data. Daardoor beschermt de handtekening niet de ciphertext, en kan een aanvaller de versleutelde bytes manipuleren zonder dat `Verify` dat detecteert.

---

## Oplossing 5: CIA-koppeling

**Vraag 1: volgorde in oefening 4**

Als je eerst ondertekent en daarna versleutelt, beschermt de handtekening de plain-text. Een aanvaller kan de ciphertext byte per byte wijzigen zonder dat de handtekeningcontrole dat detecteert, want die controle kijkt naar de plain-text die niet gewijzigd is. Door eerst te versleutelen en daarna te ondertekenen, beschermt de handtekening de ciphertext. Elke wijziging in de ciphertext maakt de handtekening ongeldig.

**Vraag 2: vervaltijd van de resetcode**

Een code die niet verloopt, blijft geldig totdat hij gebruikt wordt. Als een aanvaller de resetcode onderschept via een gelekte e-mail, heeft hij onbeperkt de tijd om hem te gebruiken. Met een vervaltijd van 15 minuten verkleint het aanvalsvenster drastisch. De beveiligingsdoelstelling is het beperken van de impact van een gecompromitteerde code.

**Vraag 3: AesEncryptor versus OrderSigner**

`AesEncryptor` beschermt **Confidentiality**: de data is onleesbaar voor iedereen zonder de sleutel.

`OrderSigner` beschermt **Integrity**: de data blijft leesbaar, maar elke wijziging is detecteerbaar.

Een systeem dat alleen `AesEncryptor` gebruikt, kan niet bewijzen dat de orderbevestiging ongewijzigd is. AES-encryptie detecteert geen manipulaties van de ciphertext. Een aanvaller kan bytes in de ciphertext wijzigen, wat na ontsleuteling leidt tot corrupte of andere data, maar het systeem weet niet of dat door een aanval of een transmissiefout komt.

**Vraag 4: wachtwoord bewaren als AES-encryptie**

Als je wachtwoorden versleutelt met AES, bestaat er ergens een sleutel waarmee je ze kan ontsleutelen. Als die sleutel gestolen wordt, zijn alle wachtwoorden direct leesbaar. BCrypt heeft geen sleutel: de hash is niet omkeerbaar. Zelfs als een aanvaller de volledige database inclusief BCrypt-hashes steelt, kan hij wachtwoorden niet herleiden.

De CIA-pijler is **Confidentiality**: wachtwoorden zijn persoonlijke gegevens die vertrouwelijk moeten blijven. Een medewerker die wachtwoorden kan opvragen via een decryptieoptie, is zelf een beveiligingsrisico. De juiste oplossing is een resetflow: het bestaande wachtwoord wordt nooit opgevraagd, de klant stelt zelf een nieuw wachtwoord in.
