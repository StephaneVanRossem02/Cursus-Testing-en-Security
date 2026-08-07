# Startpakket les 9: Secure Coding (OWASP)

Dit is je vertrekpunt voor de oefeningen van les 9. Alles wat je in de vorige
lessen gebouwd hebt staat er al in, samen met de code die je tijdens de theorie
van deze les opbouwt. Wat je in de oefeningen moet schrijven, staat er nog niet
in: daar vind je een skelet met lege methodes en de melding `// jouw code hier`.

> De oplossing van deze les is een aparte download. Kijk daar pas in nadat je het
> zelf geprobeerd hebt.

## Wat zit erin

| Project | Wat het is |
|---------|------------|
| `ShopWave` | De domeinklassen. Klaar tot en met les 8, plus de theorie van deze les. |
| `ShopWave.Tests` | De tests van de vorige lessen. Die horen groen te staan. |
| `ShopWave.Web` | De webshop. Krijg je kant en klaar, je hoeft geen Razor te kennen. |
| `ShopWave.ConsoleDemo` | Instappunt voor de stukjes "controleer je werk" uit de oefeningen. |
| `ShopWave.Api` | De API waar de security-oefeningen op werken. |
| `ShopWave.Specs` | De acceptatietests in Gherkin. |

## Wat jij bouwt

1. SQL Injection op productnaam
2. Invoervalidatie op login en verify
3. Rate limiting op het login-endpoint
4. CORS correct configureren (skelet van `CorsValidator` staat klaar)
5. OWASP-analyse van een incident

## Starten

```
dotnet build
dotnet test
dotnet run --project ShopWave.Web
```

De webshop draait dan op https://localhost:5443.

## De webshop

Nieuw deze les: **Zoeken**. Die pagina heeft twee zoekknoppen naast elkaar, een
veilige en een naïeve. Met de naïeve zoek je op `@shopwave.be` en krijg je alle
orders te zien, ook die van de beheerder. Dat is het lek waar oefening 1 over gaat.
