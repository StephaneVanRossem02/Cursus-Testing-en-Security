#!/usr/bin/env bash
#
# Bouwt per les een startpakket op uit de oplossingen.
#
#   startpakket les N = oplossing van les N-1
#                     + alles wat de theorie van les N toevoegt
#                     + startcode-skeletten voor de klassen die de oefeningen van
#                       les N vragen maar die de webshop nodig heeft om te compileren
#                     + de volledige ShopWave.Web van les N
#
# De theoriebestanden komen letterlijk uit de oplossing. De skeletten staan in
# tools/skeletons/<les>/ en zijn de klasseschets uit de oefeningenpagina met een
# compileerbare body in plaats van { ... }.
#
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
SOL="$ROOT/solutions"
SKEL="$ROOT/tools/skeletons"
OUT="$ROOT/startpakketten"

LESSEN=(
  01-unit-testing-en-mocking
  02-cia-hashing-en-encryptie
  03-test-driven-development
  04-2fa-handtekeningen-en-x509
  05-integration-testing
  06-https-en-tls
  07-jwt-en-oauth2
  08-acceptatietesten
  09-secure-coding-owasp
  10-integration-testing-mockoon
  11-ethisch-hacken
  12-shopwave-in-productie
)

# Bestanden uit ShopWave/ die de THEORIE van die les aanbrengt en die dus in het
# startpakket horen. Alles onder een map Demos/ wordt sowieso meegenomen.
theorie_bestanden() {
  case "$1" in
    03-test-driven-development)   echo "CartItem.cs ICouponService.cs" ;;
    04-2fa-handtekeningen-en-x509) echo "Security/CertificateHelper.cs Security/PendingCode.cs Security/AccountRepository.cs" ;;
    05-integration-testing)       echo "OrderConfirmationService.cs CheckoutService.cs" ;;
    07-jwt-en-oauth2)             echo "Security/JwtTokenService.cs" ;;
    10-integration-testing-mockoon) echo "ShippingClient.cs ShippingResponse.cs" ;;
    *)                            echo "" ;;
  esac
}

# Bestanden uit de vorige les die deze les vervangt en die dus niet meer mogen
# blijven staan. Les 5 geeft een nieuwe CheckoutService met een andere vorm; de
# oude klasse en haar tests uit les 1 kunnen daar niet naast bestaan.
te_verwijderen() {
  case "$1" in
    05-integration-testing) echo "ShopWave.Tests/CheckoutServiceTests.cs" ;;
    *)                      echo "" ;;
  esac
}

EXCL=(--exclude=./bin --exclude=./obj --exclude='*/bin' --exclude='*/obj'
      --exclude=.vs --exclude='*.user' --exclude='*.suo' --exclude='*.feature.cs')

kopieer() {
  mkdir -p "$2"
  tar -C "$1" "${EXCL[@]}" -cf - . | tar -C "$2" -xf -
}

for i in "${!LESSEN[@]}"; do
  les="${LESSEN[$i]}"
  nummer=$((i + 1))
  doel="$OUT/$les"

  rm -rf "$doel"
  mkdir -p "$doel"

  if [ "$i" -gt 0 ]; then
    vorige="${LESSEN[$((i - 1))]}"

    # 1. Alles van de vorige les: dat heeft de student al staan.
    for project in ShopWave ShopWave.Tests ShopWave.Api ShopWave.Specs ShopWave.ConsoleDemo; do
      if [ -d "$SOL/$vorige/$project" ]; then
        kopieer "$SOL/$vorige/$project/" "$doel/$project/"
      fi
    done

    # 2. Wat de theorie van deze les toevoegt aan ShopWave.
    (cd "$SOL/$les/ShopWave" && find . -path '*/Demos/*' -name '*.cs' \
        -not -path './bin/*' -not -path './obj/*' -print0) |
      while IFS= read -r -d '' f; do
        mkdir -p "$doel/ShopWave/$(dirname "$f")"
        cp "$SOL/$les/ShopWave/$f" "$doel/ShopWave/$f"
      done

    for f in $(theorie_bestanden "$les"); do
      mkdir -p "$doel/ShopWave/$(dirname "$f")"
      cp "$SOL/$les/ShopWave/$f" "$doel/ShopWave/$f"
    done
  else
    # Les 1 geeft alle domeinklassen kant en klaar: je schrijft er de tests bij.
    kopieer "$SOL/$les/ShopWave/" "$doel/ShopWave/"
    mkdir -p "$doel/ShopWave.Tests"
    kopieer "$SOL/$les/ShopWave.ConsoleDemo/" "$doel/ShopWave.ConsoleDemo/"
  fi

  # 3. De webshop van deze les, volledig. Die krijgt de student cadeau.
  kopieer "$SOL/$les/ShopWave.Web/" "$doel/ShopWave.Web/"

  # 4. Projectbestanden en solution van deze les: de pakketverwijzingen die de
  #    oefeningen van deze les nodig hebben, moeten er al in staan.
  cp "$SOL/$les"/*.sln "$doel/"
  (cd "$SOL/$les" && find . -name '*.csproj' -not -path './*/bin/*' -not -path './*/obj/*' -print0) |
    while IFS= read -r -d '' f; do
      mkdir -p "$doel/$(dirname "$f")"
      cp "$SOL/$les/$f" "$doel/$f"
    done

  # 5. De testprojecten bevatten de tests die de student zelf gaat schrijven.
  #    Alleen de tests van vorige lessen blijven staan; die horen groen te zijn.
  if [ -d "$SKEL/$les" ]; then
    kopieer "$SKEL/$les/" "$doel/"
  fi

  for f in $(te_verwijderen "$les"); do
    rm -f "$doel/$f"
  done

  # 6. Bouwrommel die tijdens het kopieren is meegekomen, weg.
  find "$doel" -type d \( -name bin -o -name obj -o -name .vs -o -name .idea \) \
       -prune -exec rm -rf {} + 2>/dev/null || true
  find "$doel" -type f \( -name '*.user' -o -name '*.suo' -o -name '*.feature.cs' \) \
       -delete 2>/dev/null || true

  echo "startpakket klaar: $les"
done

bash "$ROOT/tools/readmes-startpakketten.sh"
