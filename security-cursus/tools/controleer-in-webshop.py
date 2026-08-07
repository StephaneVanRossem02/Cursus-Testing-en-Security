"""
Zet onderaan elke oefeningenpagina een blok "Controleer je werk in de webshop".

Het blok noemt per les een paar concrete handelingen en wat er dan op het scherm
hoort te staan. De webshop toont onder elk resultaat uit welke klasse het komt,
dus een student ziet niet alleen dat het werkt maar ook dat het door zijn eigen
code gegaan is.

Draai je het script een tweede keer, dan gebeurt er niets.
"""

import pathlib
import sys

ROOT = pathlib.Path(__file__).resolve().parent.parent
DOCS = ROOT / "docs"

KOP = "## Controleer je werk in de webshop"

LESSEN = [
    ("01-testing/01-unit-testing-mocking", 1),
    ("02-security/01-cia-hashing-encryptie", 2),
    ("01-testing/02-tdd", 3),
    ("02-security/02-handtekeningen-x509", 4),
    ("01-testing/03-integration-testing", 5),
    ("02-security/03-https-tls", 6),
    ("02-security/04-jwt-oauth2", 7),
    ("01-testing/04-acceptatietesten", 8),
    ("02-security/05-secure-coding-owasp", 9),
    ("01-testing/05-integration-testing-mockoon", 10),
    ("02-security/06-ethisch-hacken", 11),
    ("02-security/07-herhaling-globalisatie", 12),
]

TABELLEN = {
    1: [
        ("Ga naar **Producten**",
         "De vijf producten met hun voorraad. De Webcam staat op 0."),
        ("Bestel 2 Laptops met 10 procent korting",
         "`Bestelling bevestigd` en een totaal van **1799,98 EUR**"),
        ("Bestel 1 Webcam",
         "`Product niet beschikbaar`, want `IStockService` geeft 0 terug"),
        ("Bestel 1 Laptop zonder korting",
         "`Bestelling bevestigd` en een totaal van **999,99 EUR**"),
    ],
    2: [
        ("Registreer met wachtwoord `123`",
         "`Wachtwoord moet minstens 8 tekens lang zijn.` uit jouw `PasswordValidator`"),
        ("Registreer met wachtwoord `wachtwoord`",
         "`Wachtwoord moet minstens één hoofdletter bevatten.`"),
        ("Registreer met wachtwoord `Wachtwoord1!`",
         "`Registratie geslaagd.`"),
        ("Registreer datzelfde adres nog eens",
         "`Account bestaat al.`"),
        ("Log drie keer na elkaar fout in",
         "Bij de derde poging `Account geblokkeerd.` uit jouw lockout"),
        ("Sla een order op bij **Mijn bestellingen** en sla hem nog eens op",
         "Twee keer een andere versleutelde tekst, want elke keer een nieuwe IV"),
    ],
    # De mandjespagina van les 3 kan alleen toevoegen en optellen: CartService
    # heeft op dat moment nog geen ApplyCoupon, RemoveItem of Clear.
    3: [
        ("Voeg een Laptop toe aan je mandje",
         "Eén regel in de tabel en een totaal van **999,99 EUR**"),
        ("Voeg er een Muis bij",
         "Twee regels en een totaal van **1029,98 EUR**"),
        ("Voeg nog een Laptop toe",
         "Geen derde regel maar aantal 2, en een totaal van **2029,97 EUR**"),
    ],
    # Les 4 krijgt de volledige mandjespagina, dus daar kan de coupon wel getoond
    # worden. Zie de tabel van les 4 hieronder.
    4: [
        ("Log in met een geldig wachtwoord",
         "Je komt in stap 2 en de 2FA-code verschijnt op het scherm"),
        ("Voer een foute code in",
         "`Ongeldige 2FA-code.` uit jouw `VerifyTwoFactor`"),
        ("Vraag een nieuwe code en voer die correct in",
         "`Inloggen geslaagd.`"),
        ("Gebruik diezelfde code nog eens",
         "Afgewezen: een code werkt maar één keer"),
        ("Vraag een reset aan bij **Wachtwoord vergeten**",
         "De resetcode verschijnt, en na invoeren `Wachtwoord gewijzigd.`"),
        ("Ga naar **Orderbevestiging** en wijzig één teken in de tekst",
         "De handtekening wordt ongeldig: `OrderSigner.Verify()` geeft false"),
    ],
    5: [
        ("Vul je mandje en ga naar **Bestellen**",
         "Het bedrag komt uit `CartService.Total`, niet meer uit een los product"),
        ("Reken een leeg mandje af",
         "`Mandje is leeg` uit jouw `CheckoutService.Checkout()`"),
        ("Reken een gevuld mandje af",
         "`Betaling geslaagd` en een bevestigingscode in de vorm `ORD-000001-3CED`"),
        ("Bestel nog eens",
         "Een andere bevestigingscode: `OrderConfirmationService` genereert elke keer een nieuwe"),
    ],
    6: [
        ("Start de webshop en open `https://localhost:5443`",
         "Een certificaatwaarschuwing. Die hoort er te zijn bij een self-signed certificaat."),
        ("Bekijk het certificaat in je browser",
         "Subject en issuer zijn allebei `CN=ShopWave`: het certificaat tekent zichzelf"),
        ("Open de netwerktab van je browser en bekijk de response headers",
         "`X-Content-Type-Options`, `X-Frame-Options` en `Strict-Transport-Security`"),
        ("Probeer `http://localhost:5443`",
         "Dat werkt niet: de poort spreekt alleen TLS"),
    ],
    7: [
        ("Ga naar **Token** en maak een token aan voor rol `user`",
         "De drie delen apart: header, payload en signature"),
        ("Bekijk de payload",
         "Je e-mailadres en je rol zijn gewoon leesbaar, zonder sleutel"),
        ("Maak een token aan voor rol `admin`",
         "Dezelfde header, een andere payload, een andere signature"),
        ("Plak het token in jwt.io",
         "Dezelfde claims. Een token is ondertekend, niet versleuteld."),
    ],
    8: [
        ("Loop de loginflow in de webshop door: registreren, inloggen, 2FA",
         "Precies de stappen die je in je `.feature`-bestanden beschrijft"),
        ("Log drie keer fout in",
         "De lockout die je in `Lockout.feature` vastlegt"),
        ("Vergelijk je scenario's met wat je op het scherm ziet",
         "Elke `Given`, `When` en `Then` heeft een tegenhanger in de webshop"),
    ],
    9: [
        ("Ga naar **Zoeken** en zoek veilig op `alice@shopwave.be`",
         "Alleen de orders van Alice"),
        ("Zoek naïef op `alice@shopwave.be`",
         "Hetzelfde resultaat. Zo lijkt de naïeve versie in orde."),
        ("Zoek naïef op `@shopwave.be`",
         "**Alle** orders, ook die van de beheerder. Dat is het lek."),
        ("Zoek veilig op `@shopwave.be`",
         "Niets, want er is geen account met dat adres"),
    ],
    10: [
        ("Start je mockserver en draai je tests",
         "De tests praten met de mock, niet met een echte koeriersdienst"),
        ("Zet de mockserver uit en draai opnieuw",
         "De Mockoon-tests falen, de WireMock-tests blijven groen: die starten hun eigen server"),
    ],
    11: [
        ("Ga naar **Token**, kopieer de payload en wijzig de rol naar `admin`",
         "Het token wordt afgewezen: de signature klopt niet meer"),
        ("Ga naar **Pentestrapport**",
         "De bevindingen die jouw `PentestReport` teruggeeft"),
        ("Voeg een bevinding met risico `High` toe",
         "Ze verschijnt in de lijst en in de telling per risiconiveau"),
    ],
    12: [
        ("Loop de volledige webshop door met je checklist ernaast",
         "Voor elk item kan je aanwijzen waar het in de code zit"),
        ("Controleer of er nog geheimen in `appsettings.json` staan",
         "Wat je `SecretsAudit` vindt, moet overeenkomen met wat er echt staat"),
        ("Draai `IsFullyImplemented()`",
         "Zolang er items op `NotImplemented` staan, geeft die false terug"),
    ],
}


def blok(nummer):
    adres = "http://localhost:5000" if nummer <= 5 else "https://localhost:5443"

    regels = [
        "",
        "---",
        "",
        KOP,
        "",
        "Start de webshop met `dotnet run --project ShopWave.Web` en open "
        f"{adres}. Zo zie je je eigen code draaien in plaats van alleen een groene testbalk.",
        "",
        "| Wat je doet | Wat je ziet als je code klopt |",
        "|-------------|-------------------------------|",
    ]

    for handeling, verwacht in TABELLEN[nummer]:
        regels.append(f"| {handeling} | {verwacht} |")

    regels += [
        "",
        "Onder elk resultaat staat uit welke klasse het komt. Zie je iets anders dan "
        "hierboven, dan weet je meteen welke methode je moet nakijken.",
        "",
    ]

    return "\n".join(regels)


def main():
    for map_, nummer in LESSEN:
        pad = DOCS / map_ / "02-oefeningen.md"
        inhoud = pad.read_text(encoding="utf-8")

        if KOP in inhoud:
            print(f"overgeslagen (staat er al): les {nummer}")
            continue

        pad.write_text(inhoud.rstrip() + "\n" + blok(nummer), encoding="utf-8")
        print(f"blok toegevoegd: les {nummer}")

    return 0


if __name__ == "__main__":
    sys.exit(main())
