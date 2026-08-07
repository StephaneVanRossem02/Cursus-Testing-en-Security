#!/usr/bin/env bash
#
# Schrijft de README van elk startpakket. Wordt aangeroepen door
# build-startpakketten.sh, maar kan ook los draaien.
#
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
OUT="$ROOT/startpakketten"

titel() {
  case "$1" in
    01-*) echo "Unit Testing en Mocking" ;;
    02-*) echo "CIA, Hashing en Encryptie" ;;
    03-*) echo "Test Driven Development" ;;
    04-*) echo "2FA, Handtekeningen en X.509" ;;
    05-*) echo "Integration Testing" ;;
    06-*) echo "HTTPS en TLS" ;;
    07-*) echo "JWT en OAuth2" ;;
    08-*) echo "Acceptatietesten" ;;
    09-*) echo "Secure Coding (OWASP)" ;;
    10-*) echo "Integration Testing met Mockoon" ;;
    11-*) echo "Ethisch Hacken" ;;
    12-*) echo "ShopWave in Productie" ;;
  esac
}

adres() {
  case "$1" in
    0[1-5]-*) echo "http://localhost:5000" ;;
    *)        echo "https://localhost:5443" ;;
  esac
}

webshop() {
  case "$1" in
    01-*) cat <<'EOF'
De webshop start met drie pagina's: de startpagina, **Producten** en **Bestellen**.
Bestellen roept `OrderService` en `CheckoutService` aan. Zolang jij die klassen
niet getest hebt, weet je niet of ze kloppen; de pagina toont gewoon wat ze doen.
EOF
;;
    02-*) cat <<'EOF'
Nieuw deze les: **Registreren**, **Inloggen** en **Mijn bestellingen**. Die drie
pagina's hangen volledig aan de klassen die jij in de oefeningen schrijft. Zolang
`AccountRepository` en `PasswordValidator` leeg zijn, doen de formulieren niets.
Vul ze in, herstart de webshop, en je ziet je eigen wachtwoordregels en de lockout
na drie foute pogingen in actie.
EOF
;;
    03-*) cat <<'EOF'
Nieuw deze les: **Winkelmandje**, in eenvoudige vorm. Je kan er artikels aan
toevoegen en het totaal zien. Meer kan de pagina niet, want meer heeft
`CartService` nog niet. De volledige pagina met coupons, verwijderen en legen
staat in de oplossing van deze les.
EOF
;;
    04-*) cat <<'EOF'
Het **Winkelmandje** is nu volledig: coupons, artikels verwijderen en mandje legen.
Nieuw deze les: **Inloggen** verloopt in twee stappen met een 2FA-code,
**Wachtwoord vergeten** gebruikt jouw `PasswordResetService`, en
**Orderbevestiging** laat een handtekening zien die breekt zodra je de tekst
aanpast.
EOF
;;
    05-*) cat <<'EOF'
**Bestellen** rekent nu af vanuit je winkelmandje in plaats van per los product,
en toont een bevestigingscode. Die code komt uit `OrderConfirmationService`, de
klasse waarvoor jij in oefening 4 de callback-techniek gaat testen.
EOF
;;
    06-*) cat <<'EOF'
De webshop draait vanaf deze les op HTTPS met een self-signed certificaat. Je
browser toont een waarschuwing: dat hoort zo, klik door. Het slotje en de
certificaatgegevens zijn precies waar deze les over gaat.
EOF
;;
    07-*) cat <<'EOF'
Nieuw deze les: **Token**. Die pagina maakt een JWT aan en toont de drie delen
apart. Je ziet dat de payload leesbaar is zonder de sleutel: een token is
ondertekend, niet versleuteld.
EOF
;;
    08-*) cat <<'EOF'
Geen nieuwe pagina's deze les. De webshop blijft draaien zoals in les 7. Je werkt
in `ShopWave.Specs`, waar je de flows die je in de webshop ziet, vastlegt als
Gherkin-scenario's.
EOF
;;
    09-*) cat <<'EOF'
Nieuw deze les: **Zoeken**. Die pagina heeft twee zoekknoppen naast elkaar, een
veilige en een naïeve. Met de naïeve zoek je op `@shopwave.be` en krijg je alle
orders te zien, ook die van de beheerder. Dat is het lek waar oefening 1 over gaat.
EOF
;;
    10-*) cat <<'EOF'
Geen nieuwe pagina's deze les. Je werkt tegen een mockserver in
`ShopWave.Tests`. De webshop blijft draaien zoals in les 9.
EOF
;;
    11-*) cat <<'EOF'
Nieuw deze les: **Pentestrapport**. Die pagina toont de bevindingen uit jouw
`PentestReport`. Zolang je `AddFinding` leeg is, blijft de lijst leeg.
EOF
;;
    12-*) cat <<'EOF'
Geen nieuwe pagina's deze les. Je neemt de volledige webshop door met de
checklist die je in de oefeningen bouwt.
EOF
;;
  esac
}

opdrachten() {
  case "$1" in
    01-*) cat <<'EOF'
1. `DiscountCalculator` testen
2. `OrderService` testen zonder voorraadcontrole
3. `OrderService` testen met voorraadcontrole
4. `CheckoutService` testen
EOF
;;
    02-*) cat <<'EOF'
1. `AccountRepository` bouwen (skelet staat klaar)
2. Wachtwoordsterkte afdwingen met `PasswordValidator` (skelet staat klaar)
3. `OrderEncryptor` en `OrderRepository` bouwen (skeletten staan klaar)
4. Versleutelde klantnotities: `CustomerNotesService` maak je zelf aan
5. CIA-analyse van ShopWave
EOF
;;
    03-*) cat <<'EOF'
1. `CartService` via TDD (skelet met `AddItem` en `Total` staat klaar)
2. `CartService` uitbreiden met couponondersteuning
3. `OrderService` uitbreiden via TDD
4. Reflectie
EOF
;;
    04-*) cat <<'EOF'
1. Wachtwoordreset via 2FA (skelet van `PasswordResetService` staat klaar)
2. `TwoFactorService` uitbreiden met een pogingenteller
3. `InvoiceSigner` maken zonder code te kopiëren uit `OrderSigner`
4. Versleuteld en ondertekend document
5. CIA-koppeling
EOF
;;
    05-*) cat <<'EOF'
1. `CheckoutService` integreren (de klasse staat klaar)
2. `DiscountCalculator` integreren
3. De volledige bestelflow testen
4. De callback-techniek toepassen op `OrderConfirmationService`
5. Reflectie
EOF
;;
    06-*) cat <<'EOF'
1. De ShopWave API op HTTPS zetten (skelet staat klaar in `ShopWave.Api/Program.cs`)
2. De TLS-handshake simuleren met AES
3. HTTP en HTTPS naast elkaar vergelijken
4. Security headers en HSTS
5. Een echt certificaat analyseren
EOF
;;
    07-*) cat <<'EOF'
1. Het `/me`-endpoint uitbreiden
2. Admin-rol en rolgebaseerde toegang
3. Tokenvervaltijd valideren
4. `TokenBlacklist` implementeren
5. JWT en OAuth 2.0 koppelen aan CIA
EOF
;;
    08-*) cat <<'EOF'
1. Scenario Outline voor de loginflow
2. Lockout-feature
3. Registratie-feature
4. 2FA-flow als Scenario Outline
5. Reflectie
EOF
;;
    09-*) cat <<'EOF'
1. SQL Injection op productnaam
2. Invoervalidatie op login en verify
3. Rate limiting op het login-endpoint
4. CORS correct configureren (skelet van `CorsValidator` staat klaar)
5. OWASP-analyse van een incident
EOF
;;
    10-*) cat <<'EOF'
1. Een tweede bestemming testen via `[Theory]`
2. Foutscenario's uitbreiden
3. Timeout en latency
4. WireMock.Net
5. Reflectie
EOF
;;
    11-*) cat <<'EOF'
1. JWT-manipulatie in C#
2. De `alg:none`-aanval simuleren
3. Informatielekkage in development en productie
4. `PentestReport` implementeren (skelet staat klaar)
5. Een volledig pentestrapport schrijven
EOF
;;
    12-*) cat <<'EOF'
1. Productieomgeving configureren
2. `SecurityChecklist` implementeren (skelet staat klaar)
3. `CiaPijlerAnalyse` implementeren
4. Secrets audit
5. Eindreflectie
EOF
;;
  esac
}

i=0
for d in "$OUT"/*/; do
  les="$(basename "$d")"
  i=$((i + 1))

  {
    echo "# Startpakket les $i: $(titel "$les")"
    echo
    echo "Dit is je vertrekpunt voor de oefeningen van les $i. Alles wat je in de vorige"
    echo "lessen gebouwd hebt staat er al in, samen met de code die je tijdens de theorie"
    echo "van deze les opbouwt. Wat je in de oefeningen moet schrijven, staat er nog niet"
    echo "in: daar vind je een skelet met lege methodes en de melding \`// jouw code hier\`."
    echo
    echo "> De oplossing van deze les is een aparte download. Kijk daar pas in nadat je het"
    echo "> zelf geprobeerd hebt."
    echo
    echo "## Wat zit erin"
    echo
    echo "| Project | Wat het is |"
    echo "|---------|------------|"
    echo "| \`ShopWave\` | De domeinklassen. Klaar tot en met les $((i - 1)), plus de theorie van deze les. |"
    if [ "$i" -eq 1 ]; then
      echo "| \`ShopWave.Tests\` | Nog leeg. Hier schrijf jij je tests. |"
    else
      echo "| \`ShopWave.Tests\` | De tests van de vorige lessen. Die horen groen te staan. |"
    fi
    echo "| \`ShopWave.Web\` | De webshop. Krijg je kant en klaar, je hoeft geen Razor te kennen. |"
    echo "| \`ShopWave.ConsoleDemo\` | Instappunt voor de stukjes \"controleer je werk\" uit de oefeningen. |"
    if [ -d "$d/ShopWave.Api" ]; then
      echo "| \`ShopWave.Api\` | De API waar de security-oefeningen op werken. |"
    fi
    if [ -d "$d/ShopWave.Specs" ]; then
      echo "| \`ShopWave.Specs\` | De acceptatietests in Gherkin. |"
    fi
    echo
    echo "## Wat jij bouwt"
    echo
    opdrachten "$les"
    echo
    echo "## Starten"
    echo
    echo '```'
    echo "dotnet build"
    echo "dotnet test"
    echo "dotnet run --project ShopWave.Web"
    echo '```'
    echo
    echo "De webshop draait dan op $(adres "$les")."
    echo
    echo "## De webshop"
    echo
    webshop "$les"
  } > "$d/README.md"

  echo "README klaar: $les"
done
