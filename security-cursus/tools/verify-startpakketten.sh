#!/usr/bin/env bash
#
# Controleert elk startpakket: bouwt het en draait de tests van de vorige lessen.
# Die horen groen te staan. De tests van de les zelf schrijft de student nog.
#
set -uo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

for d in "$ROOT"/startpakketten/*/; do
  les="$(basename "$d")"
  printf '%-32s ' "$les"

  build=$(cd "$d" && dotnet build -v q --nologo 2>&1)
  if echo "$build" | grep -qE "error CS"; then
    echo "BUILD GEFAALD"
    echo "$build" | grep -oE "error CS[0-9]+: [^[]*" | sort -u | head -3 | sed 's/^/      /'
    continue
  fi

  aantal=$(find "$d/ShopWave.Tests" -name '*.cs' -not -path '*/obj/*' -not -path '*/bin/*' 2>/dev/null | wc -l)
  if [ "$aantal" -eq 0 ]; then
    echo "build ok, nog geen tests"
    continue
  fi

  test=$(cd "$d" && dotnet test --nologo -v q --no-build 2>&1)
  regel=$(echo "$test" | grep -oE "Failed: +[0-9]+, Passed: +[0-9]+" | head -1)
  if echo "$test" | grep -q "Passed!"; then
    echo "build ok, tests groen  ($regel)"
  else
    echo "TESTS ROOD  ($regel)"
    echo "$test" | grep -E "^\s+(Failed|Error)" | head -3 | sed 's/^/      /'
  fi
done
