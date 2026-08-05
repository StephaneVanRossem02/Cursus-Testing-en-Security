# Les 8: Acceptatietesten

Cumulatief oplossingsproject. Bevat alle code van les 1 t.e.m. les 7, plus de acceptatietests (BDD/Gherkin met Reqnroll) van deze les.

## Wat is nieuw in deze les

Een vierde project: **`ShopWave.Specs`**, een Reqnroll-acceptatietestproject (xUnit).

- `LoginContext` - gedeelde toestand tussen step definitions (Reqnroll DI).
- `StepDefinitions/CommonSteps.cs` - de gedeelde `Given`-stap (account aanmaken, met callback voor de 2FA-code).
- `StepDefinitions/LoginSteps.cs`, `LockoutSteps.cs`, `RegistratieSteps.cs`, `TwoFactorSteps.cs` - step definitions.
- `Features/Login.feature`, `Lockout.feature`, `Registratie.feature`, `TwoFactor.feature` - de Gherkin-scenario's.

`ShopWave.Specs` bevat 11 scenario's (inclusief Scenario Outlines) die allemaal slagen. De bestaande `ShopWave.Tests` (54 tests) blijven ongewijzigd en groen.

## Aansluiting op de security-track

De acceptatiescenario's testen de `AccountRepository` en `TwoFactorService` die je in les 4 gebouwd hebt. De meldingen die de scenario's verwachten (`"Voer uw 2FA-code in."`, `"Ongeldig wachtwoord."`, `"Ongeldige 2FA-code."`, `"Account bestaat al."`) en de `string`-returnwaarde van `Register` komen exact overeen met wat les 4 aanleert. De callback-constructor van `TwoFactorService` zorgt dat `CommonSteps` de gegenereerde 2FA-code kan opvangen.

Dit was oorspronkelijk een inconsistentie in de cursusbron: de testing-track (les 8) verwachtte een andere `AccountRepository` dan de security-track (les 4) aanleerde, waardoor de acceptatietests niet compileerden. Die is intussen in les 4 van de cursus zelf rechtgezet.

## NuGet-pakketten

- `ShopWave.Specs`: `Reqnroll.xUnit`, `Microsoft.NET.Test.Sdk`, `xunit`, `xunit.runner.visualstudio`.
- Overige projecten: ongewijzigd t.o.v. les 7.

## Bouwen en testen

```bash
dotnet build
dotnet test
```

Beide horen groen te zijn: `ShopWave.Tests` (54) en `ShopWave.Specs` (11 acceptatiescenario's).

## Waarschuwing

Dit is een startpunt en referentie. Bekijk het pas nadat je de oefeningen zelf geprobeerd hebt.
