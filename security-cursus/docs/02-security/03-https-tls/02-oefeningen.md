---
title: "Les 6: Security — HTTPS, TLS en Certificaten — Oefeningen"
sidebar_label: "Oefeningen"
---

# Les 6: Security — HTTPS, TLS en Certificaten — Oefeningen

> **Code-afspraken:** geen top-level statements · altijd `{}` · max één `return` · geen `break`/`continue` · geen underscore-prefix op parameters · geen geneste klassen · geen ternary/null-conditional · geen tuples · `double` i.p.v. `decimal` · identifiers Engels · tekst Nederlands

---

## Oefening 1 — ShopWave API uitbreiden

**Opgave:** Breid `ShopWave.Api/Program.cs` uit met drie endpoints: `POST /register`, `POST /login` en `POST /verify`. De API blijft actief op `https://localhost:5001`.

```csharp
// Fragment in ShopWave.Api/Program.cs
app.MapPost("/register", HandleRegister);
app.MapPost("/login",    HandleLogin);
app.MapPost("/verify",   HandleVerify);

IResult HandleRegister(RegisterRequest request)
{
    accountRepository.Register(request.Email, request.Password);
    return Results.Ok(new { bericht = "Registratie geslaagd." });
}

IResult HandleLogin(LoginRequest request)
{
    string result = accountRepository.Login(request.Email, request.Password);
    return Results.Ok(new { status = result });
}

IResult HandleVerify(VerifyRequest request)
{
    string result = accountRepository.VerifyTwoFactor(request.Email, request.Code);

    if (result != "Inloggen geslaagd.")
    {
        return Results.Unauthorized();
    }

    return Results.Ok(new { bericht = result });
}
```

---

## Oefening 3 — Handshake simuleren met AES

**Opgave:** Simuleer stap voor stap de TLS-handshake: RSA-sleutelpaar voor de server, willekeurige AES-sleutel voor de client, AES-sleutel versleuteld via RSA, dan het eigenlijke bericht versleuteld via AES.

```csharp
// In ShopWave/Program.cs — methode DemoTlsHandshake()
using ShopWave.Security;
using System.Security.Cryptography;
using System.Text;

namespace ShopWave
{
    public class Program
    {
        public static void Main(string[] args)
        {
            DemoTlsHandshake();
        }

        private static void DemoTlsHandshake()
        {
            // Stap 1: server genereert RSA-sleutelpaar (2048 bit)
            RSA serverRsa = RSA.Create(2048);

            // Stap 2: client genereert willekeurige AES-sleutel (32 bytes = 256 bit)
            byte[] rawKey = new byte[32];
            RandomNumberGenerator.Fill(rawKey);
            string aesKey = Convert.ToBase64String(rawKey).Substring(0, 32);

            // Stap 3: client versleutelt AES-sleutel met RSA public key van server
            byte[] encryptedKey = serverRsa.Encrypt(
                Encoding.UTF8.GetBytes(aesKey),
                RSAEncryptionPadding.OaepSHA256);

            // Stap 4: server ontsleutelt AES-sleutel met zijn private key
            string decryptedKey = Encoding.UTF8.GetString(
                serverRsa.Decrypt(encryptedKey, RSAEncryptionPadding.OaepSHA256));

            // Stap 5: client versleutelt bericht met AES
            string originalMessage  = "alice@shopwave.be | Laptop | 999.99 EUR";
            AesEncryptor encryptor  = new AesEncryptor();
            string encryptedMessage = encryptor.Encrypt(originalMessage, aesKey);

            // Stap 6: server ontsleutelt bericht met de ontvangen AES-sleutel
            AesEncryptor serverEncryptor = new AesEncryptor();
            string decryptedMessage      = serverEncryptor.Decrypt(encryptedMessage, decryptedKey);

            // Stap 7: resultaat
            Console.WriteLine($"Origineel:    {originalMessage}");
            Console.WriteLine($"Versleuteld:  {encryptedMessage}");
            Console.WriteLine($"Ontsleuteld:  {decryptedMessage}");
            Console.WriteLine($"Overeenkomst: {originalMessage == decryptedMessage}");

            serverRsa.Dispose();
        }
    }
}
```

**Antwoorden op de analysevragen:**
- **Waarom is het veilig om de AES-sleutel via RSA te versturen?** Alleen de server heeft de private RSA-sleutel; niemand anders kan de AES-sleutel ontsleutelen.
- **Wat zou er mislopen als de AES-sleutel in plain text verstuurd werd?** Elke tussenpersoon (man-in-the-middle) kan de sleutel onderscheppen en alle verdere berichten ontsleutelen.

---

## Oefening 4 — Handtekening in de TLS-context

**Opgave:** Maak een RSA-sleutelpaar aan, onderteken een serverbericht, verifieer de handtekening, pas het bericht aan en verifieer opnieuw (verwacht: ongeldig).

```csharp
private static void DemoServerHandtekening()
{
    RSA    rsa     = RSA.Create(2048);
    string bericht = "ShopWave server actief op " + DateTime.UtcNow.ToString("O");

    byte[] data      = System.Text.Encoding.UTF8.GetBytes(bericht);
    byte[] signature = rsa.SignData(data, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

    bool geldig = rsa.VerifyData(data, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
    Console.WriteLine($"Handtekening geldig:          {geldig}");

    // Manipulatie: één letter anders
    string aangepastBericht = bericht.Replace("ShopWave", "HackWave");
    byte[] aangepastData    = System.Text.Encoding.UTF8.GetBytes(aangepastBericht);

    bool geldigNaManipulatie = rsa.VerifyData(
        aangepastData, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
    Console.WriteLine($"Handtekening geldig na aanpassing: {geldigNaManipulatie}");

    rsa.Dispose();
}
```