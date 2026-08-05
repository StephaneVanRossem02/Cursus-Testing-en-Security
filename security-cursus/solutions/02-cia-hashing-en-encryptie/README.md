# Les 2: CIA, Hashing en Encryptie

Cumulatief oplossingsproject. Bevat alle code van les 1 ongewijzigd, plus de beveiligingslaag van deze les.

## Wat is nieuw in deze les

Alle nieuwe klassen staan in `ShopWave/Security/` (namespace `ShopWave.Security`, exact zoals de bron).

Demo-code uit de theorie (`ShopWave/Security/Demos/`):

- `UserRepository` - het probleem: plain-text wachtwoorden.
- `PasswordHasher` - wachtwoorden hashen en verifieren met BCrypt.
- `CustomerAccount` - account dat enkel de BCrypt-hash bewaart, nooit het plain-text wachtwoord.
- `AesEncryptor` - symmetrische AES-encryptie met willekeurige IV per operatie.

Oefening-oplossingen (`ShopWave/Security/`):

- `AccountRepository` - registratie en login met blokkering na 3 foute pogingen.
- `PasswordValidator` - dwingt wachtwoordsterkte af (lengte, hoofdletter, cijfer, speciaal teken).
- `OrderEncryptor` en `OrderRepository` - ordergegevens versleuteld opslaan.
- `CustomerNotesService` - klantnotities versleuteld opslaan.

Er zijn in deze les geen nieuwe unit tests: de testsuite van les 1 (16 tests) blijft ongewijzigd en groen.

### Cumulatieve wijzigingen

- `AccountRepository.Register` staat in de uitgebreide versie uit oplossing 2 (geeft een `string` terug en valideert het wachtwoord). De eenvoudige `void`-versie uit oplossing 1 is daarmee vervangen, zoals de oplossing zelf voorschrijft.

### Opmerking over de AesEncryptor-constructors

`AesEncryptor` heeft twee constructors: een die een sleutel als `string` aanneemt (en die zelf aanvult of afkapt tot 32 bytes) en een die een kant-en-klare `byte[]` aanneemt. Die tweede heb je nodig zodra de sleutel niet uit leesbare tekst bestaat, zoals de willekeurige sessiesleutel in les 6. `OrderEncryptor` en `CustomerNotesService` gebruiken de string-constructor.

Dit was oorspronkelijk een inconsistentie in de cursusbron (de theorie toonde alleen de string-constructor terwijl de oplossingen een byte-array meegaven). Die is intussen in de cursus zelf rechtgezet.

## NuGet-pakketten

In `ShopWave`:

- `BCrypt.Net-Next`

In `ShopWave.Tests` (ongewijzigd t.o.v. les 1):

- `Microsoft.NET.Test.Sdk`, `xunit`, `xunit.runner.visualstudio`, `Moq`, `FluentAssertions`

## Bouwen en testen

```bash
dotnet build
dotnet test
```

Beide horen groen te zijn (16 tests slagen).

## Waarschuwing

Dit is een startpunt en referentie. Bekijk het pas nadat je de oefeningen zelf geprobeerd hebt.
