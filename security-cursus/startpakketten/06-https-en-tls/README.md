# Startpakket les 6: HTTPS en TLS

Dit is je vertrekpunt voor de oefeningen van les 6. Alles wat je in de vorige
lessen gebouwd hebt staat er al in, samen met de code die je tijdens de theorie
van deze les opbouwt. Wat je in de oefeningen moet schrijven, staat er nog niet
in: daar vind je een skelet met lege methodes en de melding `// jouw code hier`.

> De oplossing van deze les is een aparte download. Kijk daar pas in nadat je het
> zelf geprobeerd hebt.

## Wat zit erin

| Project | Wat het is |
|---------|------------|
| `ShopWave` | De domeinklassen. Klaar tot en met les 5, plus de theorie van deze les. |
| `ShopWave.Tests` | De tests van de vorige lessen. Die horen groen te staan. |
| `ShopWave.Web` | De webshop. Krijg je kant en klaar, je hoeft geen Razor te kennen. |
| `ShopWave.ConsoleDemo` | Instappunt voor de stukjes "controleer je werk" uit de oefeningen. |
| `ShopWave.Api` | De API waar de security-oefeningen op werken. |

## Wat jij bouwt

1. De ShopWave API op HTTPS zetten (skelet staat klaar in `ShopWave.Api/Program.cs`)
2. De TLS-handshake simuleren met AES
3. HTTP en HTTPS naast elkaar vergelijken
4. Security headers en HSTS
5. Een echt certificaat analyseren

## Starten

```
dotnet build
dotnet test
dotnet run --project ShopWave.Web
```

De webshop draait dan op https://localhost:5443.

## De webshop

De webshop draait vanaf deze les op HTTPS met een self-signed certificaat. Je
browser toont een waarschuwing: dat hoort zo, klik door. Het slotje en de
certificaatgegevens zijn precies waar deze les over gaat.
