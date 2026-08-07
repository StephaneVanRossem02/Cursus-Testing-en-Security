# Startpakket les 5: Integration Testing

Dit is je vertrekpunt voor de oefeningen van les 5. Alles wat je in de vorige
lessen gebouwd hebt staat er al in, samen met de code die je tijdens de theorie
van deze les opbouwt. Wat je in de oefeningen moet schrijven, staat er nog niet
in: daar vind je een skelet met lege methodes en de melding `// jouw code hier`.

> De oplossing van deze les is een aparte download. Kijk daar pas in nadat je het
> zelf geprobeerd hebt.

## Wat zit erin

| Project | Wat het is |
|---------|------------|
| `ShopWave` | De domeinklassen. Klaar tot en met les 4, plus de theorie van deze les. |
| `ShopWave.Tests` | De tests van de vorige lessen. Die horen groen te staan. |
| `ShopWave.Web` | De webshop. Krijg je kant en klaar, je hoeft geen Razor te kennen. |
| `ShopWave.ConsoleDemo` | Instappunt voor de stukjes "controleer je werk" uit de oefeningen. |

## Wat jij bouwt

1. `CheckoutService` integreren (de klasse staat klaar)
2. `DiscountCalculator` integreren
3. De volledige bestelflow testen
4. De callback-techniek toepassen op `OrderConfirmationService`
5. Reflectie

## Starten

```
dotnet build
dotnet test
dotnet run --project ShopWave.Web
```

De webshop draait dan op http://localhost:5000.

## De webshop

**Bestellen** rekent nu af vanuit je winkelmandje in plaats van per los product,
en toont een bevestigingscode. Die code komt uit `OrderConfirmationService`, de
klasse waarvoor jij in oefening 4 de callback-techniek gaat testen.
