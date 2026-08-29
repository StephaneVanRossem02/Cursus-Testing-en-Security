#!/usr/bin/env bash
#
# Zet bovenaan elke oefeningenpagina een blok met de link naar het startpakket.
# Het blok komt na de inleiding en voor de eerste oefening. Draai je het script
# een tweede keer, dan gebeurt er niets: het blok staat er dan al.
#
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
DOCS="$ROOT/docs"

# map|lesnummer|slug van het startpakket
LESSEN=(
  "01-testing/01-unit-testing-mocking|1|01-unit-testing-en-mocking"
  "02-security/01-cia-hashing-encryptie|2|02-cia-hashing-en-encryptie"
  "01-testing/02-tdd|3|03-test-driven-development"
  "02-security/02-handtekeningen-x509|4|04-2fa-handtekeningen-en-x509"
  "01-testing/03-integration-testing|5|05-integration-testing"
  "02-security/03-https-tls|6|06-https-en-tls"
  "02-security/04-jwt-oauth2|7|07-jwt-en-oauth2"
  "01-testing/04-acceptatietesten|8|08-acceptatietesten"
  "02-security/05-secure-coding-owasp|9|09-secure-coding-owasp"
  "01-testing/05-integration-testing-mockoon|10|10-integration-testing-mockoon"
  "02-security/06-ethisch-hacken|11|11-ethisch-hacken"
  "02-security/07-herhaling-globalisatie|12|12-shopwave-in-productie"
)

for regel in "${LESSEN[@]}"; do
  IFS='|' read -r map nummer slug <<< "$regel"
  bestand="$DOCS/$map/02-oefeningen.md"

  if grep -q "## Startpakket downloaden" "$bestand"; then
    echo "overgeslagen (staat er al): les $nummer"
    continue
  fi

  if [ "$nummer" -le 5 ]; then
    adres="http://localhost:5000"
  else
    adres="https://localhost:5443"
  fi

  blok=$(cat <<EOF
---

## Startpakket downloaden

[Download het startpakket van les $nummer](/downloads/shopwave-start-$slug.zip) (ZIP)

Hierin staat alles wat je in de vorige lessen gebouwd hebt, samen met de code die je
tijdens de theorie van deze les opbouwt. Wat je in de oefeningen zelf moet schrijven,
staat erin als skelet met de melding \`// jouw code hier\`.

De webshop zit erbij. Je hoeft geen Razor te kennen: start hem met
\`dotnet run --project ShopWave.Web\` en open $adres. Zo zie je meteen wat je code doet.
EOF
)

  # Het blok komt voor de eerste horizontale lijn na de frontmatter. De eerste
  # twee streepjeslijnen zijn de frontmatter zelf.
  awk -v blok="$blok" '
    /^---$/ { n++ }
    n == 3 && !gedaan { print blok; print ""; gedaan = 1 }
    { print }
  ' "$bestand" > "$bestand.tmp"

  mv "$bestand.tmp" "$bestand"
  echo "link toegevoegd: les $nummer"
done
