# Startpakket les 12: ShopWave in Productie

Dit is je vertrekpunt voor de oefeningen van les 12. Alles wat je in de vorige
lessen gebouwd hebt staat er al in, samen met de code die je tijdens de theorie
van deze les opbouwt. Wat je in de oefeningen moet schrijven, staat er nog niet
in: daar vind je een skelet met lege methodes en de melding `// jouw code hier`.

> De oplossing van deze les is een aparte download. Kijk daar pas in nadat je het
> zelf geprobeerd hebt.

## Wat zit erin

| Project | Wat het is |
|---------|------------|
| `ShopWave` | De domeinklassen. Klaar tot en met les 11, plus de theorie van deze les. |
| `ShopWave.Tests` | De tests van de vorige lessen. Die horen groen te staan. |
| `ShopWave.Web` | De webshop. Krijg je kant en klaar, je hoeft geen Razor te kennen. |
| `ShopWave.ConsoleDemo` | Instappunt voor de stukjes "controleer je werk" uit de oefeningen. |
| `ShopWave.Api` | De API waar de security-oefeningen op werken. |
| `ShopWave.Specs` | De acceptatietests in Gherkin. |

## Wat jij bouwt

1. Productieomgeving configureren
2. `SecurityChecklist` implementeren (skelet staat klaar)
3. `CiaPijlerAnalyse` implementeren
4. Secrets audit
5. Eindreflectie

## Starten

```
dotnet build
dotnet test
dotnet run --project ShopWave.Web
```

De webshop draait dan op https://localhost:5443.

## De webshop

Geen nieuwe pagina's deze les. Je neemt de volledige webshop door met de
checklist die je in de oefeningen bouwt.
