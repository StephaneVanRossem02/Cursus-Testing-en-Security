# Startpakket les 1: Unit Testing en Mocking

Dit is je vertrekpunt voor de oefeningen van les 1. Alles wat je in de vorige
lessen gebouwd hebt staat er al in, samen met de code die je tijdens de theorie
van deze les opbouwt. Wat je in de oefeningen moet schrijven, staat er nog niet
in: daar vind je een skelet met lege methodes en de melding `// jouw code hier`.

> De oplossing van deze les is een aparte download. Kijk daar pas in nadat je het
> zelf geprobeerd hebt.

## Wat zit erin

| Project | Wat het is |
|---------|------------|
| `ShopWave` | De domeinklassen. Klaar tot en met les 0, plus de theorie van deze les. |
| `ShopWave.Tests` | Nog leeg. Hier schrijf jij je tests. |
| `ShopWave.Web` | De webshop. Krijg je kant en klaar, je hoeft geen Razor te kennen. |
| `ShopWave.ConsoleDemo` | Instappunt voor de stukjes "controleer je werk" uit de oefeningen. |

## Wat jij bouwt

1. `DiscountCalculator` testen
2. `OrderService` testen zonder voorraadcontrole
3. `OrderService` testen met voorraadcontrole
4. `CheckoutService` testen

## Starten

```
dotnet build
dotnet test
dotnet run --project ShopWave.Web
```

De webshop draait dan op http://localhost:5000.

## De webshop

De webshop start met drie pagina's: de startpagina, **Producten** en **Bestellen**.
Bestellen roept `OrderService` en `CheckoutService` aan. Zolang jij die klassen
niet getest hebt, weet je niet of ze kloppen; de pagina toont gewoon wat ze doen.
