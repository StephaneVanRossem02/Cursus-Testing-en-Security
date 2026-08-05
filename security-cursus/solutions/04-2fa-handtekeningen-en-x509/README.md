# Les 4: 2FA, Handtekeningen en X.509

Cumulatief oplossingsproject. Bevat alle code van les 1 t.e.m. les 3, plus de 2FA- en handtekeningenlaag van deze les. Alle nieuwe klassen staan in `ShopWave/Security/`.

## Wat is nieuw in deze les

Demo-code uit de theorie (in `ShopWave/Security/`):

- `PendingCode` - hulpklasse voor een 2FA-code met vervaltijd.
- `TwoFactorService` - genereert en verifieert 2FA-codes (met pogingenlimiet, zie hieronder).
- `CertificateHelper` - maakt een self-signed X.509-certificaat aan.
- `OrderSigner` - ondertekent en verifieert orderdata met RSA (via `DocumentSigner`).
- `AccountRepository` - herzien voor de loginflow met 2FA (zie cumulatieve wijzigingen).

Oefening-oplossingen (in `ShopWave/Security/`):

- `PasswordResetService` - wachtwoordreset via een tijdelijke code (oefening 1).
- `TwoFactorService` met pogingenlimiet en `GetRemainingAttempts` (oefening 2).
- `DocumentSigner` (abstracte basisklasse), `InvoiceSigner`, `InvoiceSignerFactory`, en `OrderSigner` als subklasse (oefening 3).
- `ProtectedOrder` en `SecureOrderDocument` - eerst versleutelen, dan ondertekenen (oefening 4).

Er zijn in deze les geen nieuwe unit tests. De testsuite blijft op 37 tests en groen.

## Cumulatieve wijzigingen (belangrijk)

Deze les herschrijft bestaande code. De aangepaste versies staan hier, exact zoals de bron ze toont.

- **`AccountRepository` is herzien.** De constructor vereist nu een `TwoFactorService` (plus een optionele callback `Action<string, string>`). `Register` geeft een `string` terug (`"Registratie geslaagd."` / `"Account bestaat al."`), en `Login` geeft `"Voer uw 2FA-code in."` terug bij een correct wachtwoord in plaats van meteen `"Inloggen geslaagd."`. Bij een fout wachtwoord krijg je `"Ongeldig wachtwoord."`, en `VerifyTwoFactor` geeft `"Ongeldige 2FA-code."` bij een foute code. Deze meldingen en signaturen zijn precies wat de acceptatietests van les 8 verwachten, zodat de testing- en securitytrack op elkaar aansluiten. Let op: de wachtwoordvalidatie die les 2 aan `Register` toevoegde, zit niet in deze versie. `PasswordValidator` blijft bestaan als aparte klasse.
- **`TwoFactorService`** staat in de uitgebreide versie uit oefening 2 (met pogingenteller en `GetRemainingAttempts`) en heeft een tweede constructor met een `onCodeGenerated`-callback. Die callback vult de gegenereerde 2FA-code in, wat les 8 nodig heeft om de code op te vangen in een acceptatietest.
- **`OrderSigner`** erft nu van de nieuwe abstracte basisklasse `DocumentSigner` (oefening 3). De standalone versie uit de theorie-demo is daarmee vervangen. `Sign` en `Verify` staan in de basisklasse.

## NuGet-pakketten

Ongewijzigd t.o.v. les 2. `ShopWave`: `BCrypt.Net-Next`. Digitale handtekeningen en X.509 gebruiken `System.Security.Cryptography` uit de .NET-runtime (geen extra pakket). `ShopWave.Tests`: `Microsoft.NET.Test.Sdk`, `xunit`, `xunit.runner.visualstudio`, `Moq`, `FluentAssertions`.

## Bouwen en testen

```bash
dotnet build
dotnet test
```

Beide horen groen te zijn (37 tests slagen, 1 waarschuwing uit de bron in `OrderService`).

## Waarschuwing

Dit is een startpunt en referentie. Bekijk het pas nadat je de oefeningen zelf geprobeerd hebt.
