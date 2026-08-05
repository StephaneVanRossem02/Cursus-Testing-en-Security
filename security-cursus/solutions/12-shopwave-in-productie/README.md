# Les 12: ShopWave in Productie

Cumulatief oplossingsproject en tevens het eindpunt van de cursus. Bevat alle code van les 1 t.e.m. les 11, plus de productieconfiguratie en afsluitende artefacten van deze les.

## Wat is nieuw in deze les

In `ShopWave.Api`:

- `Program.cs` - productieklaar gemaakt: Swagger enkel in development (`app.Environment.IsDevelopment()`), `UseHttpsRedirection`, en CORS-origins die dynamisch uit de configuratie gelezen worden (`Cors:AllowedOrigins`).
- `appsettings.json` en `appsettings.Production.json` - configuratie-overrides per omgeving (logniveaus en CORS-origins). Bevatten geen secrets.

In `ShopWave/Security/`:

- `SecurityChecklist` en `ChecklistItem` - deployment-checklist die items per categorie en status beheert (oefening 2).
- `CiaPijlerAnalyse` en `CiaPillar` - CIA-pijleranalyse met voorbeelden per pijler (oefening 3).
- `SecretsAudit` - eenvoudige scanner voor hardcoded secrets in codelijnen (oefening 4).

Demo-code uit de theorie (`ShopWave/Demos/`):

- `SecurityChecklistDemo` - doorloopt de volledige ShopWave-deploymentchecklist en print het rapport (theorie stap 8h).

Er zijn geen nieuwe unit tests of acceptatietests (oefening 5 is een eindreflectie). De testsuite blijft `ShopWave.Tests` (56 geslaagd + 5 overgeslagen Mockoon-tests) en `ShopWave.Specs` (11 geslaagd).

## Cumulatieve wijzigingen

- Swagger (`Swashbuckle.AspNetCore`) toegevoegd en enkel geactiveerd in development.
- CORS gebruikt nu de origins uit `appsettings` in plaats van een hardcoded lijst (les 9). In development valt de configuratie terug op `appsettings.json` (`https://shopwave.be`).
- `UseHttpsRedirection` toegevoegd aan de pipeline.
- Het `/crash`-endpoint en de omgevingsafhankelijke foutafhandeling bestaan al sinds les 9 en zijn ongewijzigd.

### Bron-opmerking

De klasse `SecurityChecklist` gebruikt een nullable-annotatie (`ChecklistItem?`, letterlijk uit de bron) terwijl het `ShopWave`-project met `<Nullable>disable</Nullable>` werkt (de codestijl van de cursus). Dat geeft een onschuldige waarschuwing (`CS8632`), geen fout. De build en tests blijven groen.

## NuGet-pakketten

- `ShopWave.Api`: extra `Swashbuckle.AspNetCore` (naast `Microsoft.AspNetCore.Authentication.JwtBearer`, `System.IdentityModel.Tokens.Jwt`).
- Overige projecten: ongewijzigd.

## Bouwen en testen

```bash
dotnet build
dotnet test
```

Groen: 56 geslaagd + 5 overgeslagen (`ShopWave.Tests`), 11 geslaagd (`ShopWave.Specs`).

Draaien in productiemodus:

```powershell
$env:ASPNETCORE_ENVIRONMENT = "Production"
$env:JWT_SECRET_KEY = "ShopWaveGeheimeSleutel2024!!XYZ#"
dotnet run --project ShopWave.Api
```

## Waarschuwing

Dit is een startpunt en referentie. Bekijk het pas nadat je de oefeningen zelf geprobeerd hebt.
