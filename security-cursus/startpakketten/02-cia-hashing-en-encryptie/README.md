# Startpakket les 2: CIA, Hashing en Encryptie

Dit is je vertrekpunt voor de oefeningen van les 2. Alles wat je in de vorige
lessen gebouwd hebt staat er al in, samen met de code die je tijdens de theorie
van deze les opbouwt. Wat je in de oefeningen moet schrijven, staat er nog niet
in: daar vind je een skelet met lege methodes en de melding `// jouw code hier`.

> De oplossing van deze les is een aparte download. Kijk daar pas in nadat je het
> zelf geprobeerd hebt.

## Wat zit erin

| Project | Wat het is |
|---------|------------|
| `ShopWave` | De domeinklassen. Klaar tot en met les 1, plus de theorie van deze les. |
| `ShopWave.Tests` | De tests van de vorige lessen. Die horen groen te staan. |
| `ShopWave.Web` | De webshop. Krijg je kant en klaar, je hoeft geen Razor te kennen. |
| `ShopWave.ConsoleDemo` | Instappunt voor de stukjes "controleer je werk" uit de oefeningen. |

## Wat jij bouwt

1. `AccountRepository` bouwen (skelet staat klaar)
2. Wachtwoordsterkte afdwingen met `PasswordValidator` (skelet staat klaar)
3. `OrderEncryptor` en `OrderRepository` bouwen (skeletten staan klaar)
4. Versleutelde klantnotities: `CustomerNotesService` maak je zelf aan
5. CIA-analyse van ShopWave

## Starten

```
dotnet build
dotnet test
dotnet run --project ShopWave.Web
```

De webshop draait dan op http://localhost:5000.

## De webshop

Nieuw deze les: **Registreren**, **Inloggen** en **Mijn bestellingen**. Die drie
pagina's hangen volledig aan de klassen die jij in de oefeningen schrijft. Zolang
`AccountRepository` en `PasswordValidator` leeg zijn, doen de formulieren niets.
Vul ze in, herstart de webshop, en je ziet je eigen wachtwoordregels en de lockout
na drie foute pogingen in actie.
