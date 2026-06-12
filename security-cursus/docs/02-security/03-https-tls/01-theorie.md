---
title: "Les 6: Security — HTTPS, TLS en Certificaten — Theorie"
sidebar_label: "Theorie"
---

# Les 6: Security — HTTPS, TLS en Certificaten — Theorie

## HTTP versus HTTPS

HTTP verstuurt alle data als leesbare tekst. HTTPS voegt een beveiligingslaag toe via **TLS (Transport Layer Security)**:

| Garantie           | CIA-pijler      | Omschrijving                                         |
|--------------------|-----------------|------------------------------------------------------|
| Vertrouwelijkheid  | Confidentiality | Berichten zijn versleuteld — onleesbaar voor derden  |
| Integriteit        | Integrity       | Berichten kunnen niet ongemerkt gewijzigd worden     |
| Authenticatie      | —               | De client weet dat hij met de echte server praat     |

---

## De TLS-handshake

Vóór applicatiedata wordt uitgewisseld, voeren client en server de TLS-handshake uit:

1. **ClientHello** — client stuurt TLS-versie en een willekeurig getal
2. **ServerHello + certificaat** — server antwoordt met zijn X.509-certificaat (bevat de publieke sleutel)
3. **Certificaatvalidatie** — client controleert: CA vertrouwd? Certificaat niet verlopen? Domeinnaam correct?
4. **Sleuteluitwisseling** — client genereert een sessiesleutel en versleutelt die met de publieke sleutel van de server
5. **Gedeelde sleutel** — beide berekenen dezelfde gedeelde sessiesleutel
6. **Communicatie** — alle verdere data via snelle symmetrische encryptie (AES)

---

## Waarom twee soorten cryptografie?

| Methode       | Voordeel           | Nadeel         | Gebruik in TLS                    |
|---------------|--------------------|----------------|-----------------------------------|
| RSA (asymm.)  | Veilig sleuteluitwisseling | Traag  | Sessiesleutel uitwisselen         |
| AES (symm.)   | Snel               | Sleutel moet al gedeeld zijn | Alle verdere communicatie |

TLS combineert beide: RSA voor de sleuteluitwisseling, AES voor de communicatie.

---

## De trust chain

```
Root CA  (ingebouwd in besturingssysteem / browser)
  └── Intermediate CA  (ondertekend door Root CA)
       └── Servercertificaat  (ondertekend door Intermediate CA)
```

Een browser vertrouwt het servercertificaat als de volledige keten herleidbaar is tot een vertrouwde Root CA. Bij **self-signed certificaten** ontbreekt die keten — browsers tonen een waarschuwing.

---

## Kestrel configureren voor HTTPS in .NET

```csharp
builder.WebHost.ConfigureKestrel(ConfigureKestrel);

private static void ConfigureKestrel(KestrelServerOptions options)
{
    options.ListenLocalhost(5001, listenOptions =>
    {
        X509Certificate2 cert = CertificateHelper.CreateSelfSignedCertificate("localhost");
        listenOptions.UseHttps(cert);
    });
}
```

---

## TLS 1.2 versus TLS 1.3

| Versie   | Sleuteluitwisseling   | Forward Secrecy | Aanbevolen? |
|----------|-----------------------|-----------------|-------------|
| TLS 1.2  | RSA of Diffie-Hellman | Optioneel       | Acceptabel  |
| TLS 1.3  | Alleen Diffie-Hellman | Altijd          | Ja          |

**Forward secrecy**: ook als de private sleutel van de server later uitlekt, blijft oudere communicatie beschermd — elke sessie gebruikt een unieke sessiesleutel.

---

## Nuttige security headers

Headers die je toevoegt via middleware of NuGet (`NWebsec`):

- `X-Content-Type-Options: nosniff` — voorkomt MIME-sniffing
- `X-Frame-Options: DENY` — voorkomt clickjacking via iframes
- `Content-Security-Policy` — bepaalt welke bronnen de browser mag laden
- `Strict-Transport-Security` (HSTS) — browser weigert HTTP-verbindingen na eerste HTTPS-bezoek