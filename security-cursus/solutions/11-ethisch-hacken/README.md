# Les 11: Ethisch Hacken

Cumulatief oplossingsproject. Bevat alle code van les 1 t.e.m. les 10, plus de ethisch-hacken-artefacten van deze les.

## Wat is nieuw in deze les

Deze les is grotendeels conceptueel en werkt met CLI-tools (curl, verkenning). De compileerbare C#-code:

In `ShopWave/Security/`:

- `Finding` en `PentestReport` - klassen om pentestbevindingen te registreren, filteren op risico of status, en samen te vatten (oefening 4).

Demo-code uit theorie en oplossingen (`ShopWave/Demos/`):

- `JwtAttackDemos` - twee aanvalssimulaties tegen de API: JWT-rolmanipulatie (oefening 1) en de `alg:none`-aanval (oefening 2). Beide horen te mislukken tegen de correct geconfigureerde `ShopWave.Api`.
- `PentestReportDemo` - stelt een volledig pentestreport op met vijf bevindingen en print de open bevindingen (oefening 5).

Er zijn in deze les geen nieuwe unit tests of acceptatietests (oefening 3 en 5 zijn analyse-/documentatieoefeningen). De testsuite blijft `ShopWave.Tests` (56 geslaagd + 5 overgeslagen) en `ShopWave.Specs` (11 geslaagd).

## Cumulatieve wijzigingen

- Geen wijzigingen aan bestaande klassen. Het `/crash`-endpoint dat oefening 3 vermeldt bestaat al sinds les 9 (met een gedetailleerdere foutmelding) en is ongewijzigd gelaten.
- De curl-/CLI-oefeningen (verkenning, headers inspecteren) zijn geen C#-code en staan niet in het project; ze staan in de theorie- en oefeningenpagina's.

## NuGet-pakketten

Ongewijzigd t.o.v. les 10.

## Bouwen en testen

```bash
dotnet build
dotnet test
```

Groen: 56 geslaagd + 5 overgeslagen (`ShopWave.Tests`), 11 geslaagd (`ShopWave.Specs`).

## Waarschuwing

Dit is een startpunt en referentie. Bekijk het pas nadat je de oefeningen zelf geprobeerd hebt. De aanvalstechnieken in deze les gebruik je uitsluitend op je eigen lokale ShopWave-omgeving.
