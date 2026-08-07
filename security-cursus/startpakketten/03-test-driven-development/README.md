# Startpakket les 3: Test Driven Development

Dit is je vertrekpunt voor de oefeningen van les 3. Alles wat je in de vorige
lessen gebouwd hebt staat er al in, samen met de code die je tijdens de theorie
van deze les opbouwt. Wat je in de oefeningen moet schrijven, staat er nog niet
in: daar vind je een skelet met lege methodes en de melding `// jouw code hier`.

> De oplossing van deze les is een aparte download. Kijk daar pas in nadat je het
> zelf geprobeerd hebt.

## Wat zit erin

| Project | Wat het is |
|---------|------------|
| `ShopWave` | De domeinklassen. Klaar tot en met les 2, plus de theorie van deze les. |
| `ShopWave.Tests` | De tests van de vorige lessen. Die horen groen te staan. |
| `ShopWave.Web` | De webshop. Krijg je kant en klaar, je hoeft geen Razor te kennen. |
| `ShopWave.ConsoleDemo` | Instappunt voor de stukjes "controleer je werk" uit de oefeningen. |

## Wat jij bouwt

1. `CartService` via TDD (skelet met `AddItem` en `Total` staat klaar)
2. `CartService` uitbreiden met couponondersteuning
3. `OrderService` uitbreiden via TDD
4. Reflectie

## Starten

```
dotnet build
dotnet test
dotnet run --project ShopWave.Web
```

De webshop draait dan op http://localhost:5000.

## De webshop

Nieuw deze les: **Winkelmandje**, in eenvoudige vorm. Je kan er artikels aan
toevoegen en het totaal zien. Meer kan de pagina niet, want meer heeft
`CartService` nog niet. De volledige pagina met coupons, verwijderen en legen
staat in de oplossing van deze les.
