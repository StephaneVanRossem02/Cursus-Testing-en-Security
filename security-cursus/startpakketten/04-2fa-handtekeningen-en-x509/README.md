# Startpakket les 4: 2FA, Handtekeningen en X.509

Dit is je vertrekpunt voor de oefeningen van les 4. Alles wat je in de vorige
lessen gebouwd hebt staat er al in, samen met de code die je tijdens de theorie
van deze les opbouwt. Wat je in de oefeningen moet schrijven, staat er nog niet
in: daar vind je een skelet met lege methodes en de melding `// jouw code hier`.

> De oplossing van deze les is een aparte download. Kijk daar pas in nadat je het
> zelf geprobeerd hebt.

## Wat zit erin

| Project | Wat het is |
|---------|------------|
| `ShopWave` | De domeinklassen. Klaar tot en met les 3, plus de theorie van deze les. |
| `ShopWave.Tests` | De tests van de vorige lessen. Die horen groen te staan. |
| `ShopWave.Web` | De webshop. Krijg je kant en klaar, je hoeft geen Razor te kennen. |
| `ShopWave.ConsoleDemo` | Instappunt voor de stukjes "controleer je werk" uit de oefeningen. |

## Wat jij bouwt

1. Wachtwoordreset via 2FA (skelet van `PasswordResetService` staat klaar)
2. `TwoFactorService` uitbreiden met een pogingenteller
3. `InvoiceSigner` maken zonder code te kopiëren uit `OrderSigner`
4. Versleuteld en ondertekend document
5. CIA-koppeling

## Starten

```
dotnet build
dotnet test
dotnet run --project ShopWave.Web
```

De webshop draait dan op http://localhost:5000.

## De webshop

Het **Winkelmandje** is nu volledig: coupons, artikels verwijderen en mandje legen.
Nieuw deze les: **Inloggen** verloopt in twee stappen met een 2FA-code,
**Wachtwoord vergeten** gebruikt jouw `PasswordResetService`, en
**Orderbevestiging** laat een handtekening zien die breekt zodra je de tekst
aanpast.
