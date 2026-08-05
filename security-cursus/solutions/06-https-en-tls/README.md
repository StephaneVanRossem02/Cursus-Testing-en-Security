# Les 6: HTTPS en TLS

Cumulatief oplossingsproject. Bevat alle code van les 1 t.e.m. les 5, plus de HTTPS/TLS-laag van deze les.

## Wat is nieuw in deze les

Deze les voegt een derde project toe aan de solution: **`ShopWave.Api`**, een ASP.NET Core minimal API die op HTTPS draait.

- `ShopWave.Api/Program.cs` - Kestrel geconfigureerd met een self-signed certificaat (via `CertificateHelper` uit les 4), HSTS, security headers en enkele endpoints (`/`, `/certificaat`, `/onveilig/inlog`, `/veilig/certificaatinfo`, `/headers`). Dit is de samengevoegde eindversie van oefening 1, 3 en 4.

Demo-code uit de oplossingen (`ShopWave/Demos/`):

- `TlsHandshakeSimulation` - simuleert de TLS-handshake: RSA om een sessiesleutel uit te wisselen, daarna AES voor de communicatie (oefening 2). Zelfstandig uitvoerbaar.
- `HttpsComparisonDemo` - vergelijkt HTTP en HTTPS door de API te bevragen (oefening 3). Compileert altijd; draaien vereist dat `ShopWave.Api` actief is.

Er zijn in deze les geen nieuwe unit tests (oefening 5 is een analyseoefening). De testsuite blijft op 54 tests en groen.

## Cumulatieve wijzigingen

- Nieuw project `ShopWave.Api` (`Microsoft.NET.Sdk.Web`, net8.0), met een projectreferentie naar `ShopWave`. Toegevoegd aan `ShopWave.sln`.
- De API-endpoints uit oefening 1, 3 en 4 zijn samengevoegd in een coherente `Program.cs` (HSTS en security headers staan bewust vooraan in de middleware-pipeline).

### Broncorrectie (belangrijk)

In oefening 3 staat in de console-demo de regel `client.GetStringAsync(...).Result;` als losse instructie. Dat compileert niet in C# (fout `CS0201`: een property-toegang mag niet als instructie op zichzelf staan). Het resultaat wordt hier weggegooid met `_ = ...` zodat het project compileert. Duidelijk gemarkeerd in `HttpsComparisonDemo.cs`. Dit is een echte fout in de cursusbron die je mogelijk wil rechttrekken.

## NuGet-pakketten

- `ShopWave`: `BCrypt.Net-Next`.
- `ShopWave.Api`: gebruikt het ASP.NET Core framework (`Microsoft.NET.Sdk.Web`), geen extra NuGet.
- `ShopWave.Tests`: `Microsoft.NET.Test.Sdk`, `xunit`, `xunit.runner.visualstudio`, `Moq`, `FluentAssertions`.

## Bouwen en testen

```bash
dotnet build
dotnet test
```

Beide horen groen te zijn (54 tests slagen, 1 waarschuwing uit de bron in `OrderService`).

De API zelf start je met:

```bash
dotnet run --project ShopWave.Api
```

Daarna is `https://localhost:5001` bereikbaar (self-signed certificaat, dus je browser waarschuwt).

## Waarschuwing

Dit is een startpunt en referentie. Bekijk het pas nadat je de oefeningen zelf geprobeerd hebt.
