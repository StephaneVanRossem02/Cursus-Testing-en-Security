# Startpakket les 7: JWT en OAuth2

Dit is je vertrekpunt voor de oefeningen van les 7. Alles wat je in de vorige
lessen gebouwd hebt staat er al in, samen met de code die je tijdens de theorie
van deze les opbouwt. Wat je in de oefeningen moet schrijven, staat er nog niet
in: daar vind je een skelet met lege methodes en de melding `// jouw code hier`.

> De oplossing van deze les is een aparte download. Kijk daar pas in nadat je het
> zelf geprobeerd hebt.

## Wat zit erin

| Project | Wat het is |
|---------|------------|
| `ShopWave` | De domeinklassen. Klaar tot en met les 6, plus de theorie van deze les. |
| `ShopWave.Tests` | De tests van de vorige lessen. Die horen groen te staan. |
| `ShopWave.Web` | De webshop. Krijg je kant en klaar, je hoeft geen Razor te kennen. |
| `ShopWave.ConsoleDemo` | Instappunt voor de stukjes "controleer je werk" uit de oefeningen. |
| `ShopWave.Api` | De API waar de security-oefeningen op werken. |

## Wat jij bouwt

1. Het `/me`-endpoint uitbreiden
2. Admin-rol en rolgebaseerde toegang
3. Tokenvervaltijd valideren
4. `TokenBlacklist` implementeren
5. JWT en OAuth 2.0 koppelen aan CIA

## Starten

```
dotnet build
dotnet test
dotnet run --project ShopWave.Web
```

De webshop draait dan op https://localhost:5443.

## De webshop

Nieuw deze les: **Token**. Die pagina maakt een JWT aan en toont de drie delen
apart. Je ziet dat de payload leesbaar is zonder de sleutel: een token is
ondertekend, niet versleuteld.
