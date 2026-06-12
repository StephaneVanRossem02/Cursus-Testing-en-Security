---
title: "Les 4: Security — 2FA, Digitale Handtekeningen en X.509 — Theorie"
sidebar_label: "Theorie"
---

# Les 4: Security — 2FA, Digitale Handtekeningen en X.509 — Theorie

## Two-Factor Authentication (2FA)

Een wachtwoord is iets wat je **weet**. 2FA vereist twee onafhankelijke bewijzen uit twee verschillende categorieën:

| Factor          | Omschrijving | Voorbeeld                         |
|-----------------|-------------|-----------------------------------|
| Iets wat je weet | Kennis      | Wachtwoord, pincode               |
| Iets wat je hebt | Bezit       | Smartphone, hardware token        |
| Iets wat je bent | Biometrie   | Vingerafdruk, gezichtsherkenning  |

Zelfs als een aanvaller het wachtwoord kent, kan hij niet inloggen zonder ook de tweede factor te hebben.

---

## TOTP — Time-based One-Time Password

De meest gebruikte 2FA-methode:

1. Bij activatie deelt de server een geheime sleutel met de authenticator-app (via QR-code).
2. Elke 30 seconden berekent de app een 6-cijferige code op basis van de sleutel + huidige tijd (HMAC-SHA1).
3. Zowel app als server berekenen onafhankelijk dezelfde code.
4. Codes zijn 30 seconden geldig.

In ShopWave: de `TwoFactorService` simuleert TOTP — de gegenereerde code wordt via een `Action<string, string>`-callback doorgegeven zodat tests de code kunnen onderscheppen zonder de klasse te mocken.

---

## Digitale handtekeningen

Een digitale handtekening garandeert:
- **Integriteit** — het bericht is niet gewijzigd
- **Authenticiteit** — het bericht komt van de verwachte afzender

Werking met asymmetrische cryptografie:

1. De afzender berekent een hash van de data.
2. De afzender versleutelt die hash met zijn **private sleutel** → dit is de handtekening.
3. De ontvanger decrypteert de handtekening met de **publieke sleutel** van de afzender.
4. De ontvanger berekent ook de hash van de data.
5. Als beide hashes gelijk zijn → de data is authentiek en ongewijzigd.

In ShopWave: bestellingen worden ondertekend met RSA, zodat niemand het bestelbedrag achteraf kan wijzigen.

```csharp
// Ondertekenen
byte[] signature = rsa.SignData(data, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

// Verifiëren
bool valid = rsa.VerifyData(data, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
```

---

## X.509-certificaten

Een X.509-certificaat koppelt een **publieke sleutel** aan een **identiteit**. Het is uitgegeven en ondertekend door een **Certificate Authority (CA)**.

| Type certificaat   | Wie ondertekent het?   | Vertrouwen browser? | Gebruik            |
|--------------------|------------------------|---------------------|--------------------|
| Self-signed        | Jezelf                 | Nee — waarschuwing  | Development        |
| CA-certificaat     | Vertrouwde CA          | Ja                  | Productie          |

`CertificateHelper.CreateSelfSignedCertificate("naam")` maakt een self-signed certificaat aan in C#.

---

## CIA-koppeling

| Technologie          | CIA-pijler      | Reden                                         |
|----------------------|-----------------|-----------------------------------------------|
| 2FA                  | Confidentiality | Beschermt toegang — ook bij gestolen wachtwoord |
| Digitale handtekening | Integrity      | Manipulatie van orderdata onmiddellijk detecteerbaar |
| Encryptie (AES)      | Confidentiality | Data onleesbaar zonder de juiste sleutel      |