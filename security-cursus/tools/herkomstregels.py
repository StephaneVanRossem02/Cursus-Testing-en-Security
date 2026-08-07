"""
Zet onder elk resultaat in de webshop een regel die zegt uit welke klasse dat
resultaat komt. Zo ziet een student zwart op wit dat wat hij op het scherm krijgt
door zijn eigen code gegaan is.

De regel komt alleen te staan waar dat ook klopt: onder de melding als die melding
letterlijk de returnwaarde van een domeinklasse is, en anders bij het totaal, het
token of de versleutelde tekst.

Draai je het script een tweede keer, dan gebeurt er niets: de regel staat er dan al.
"""

import pathlib
import re
import sys

ROOT = pathlib.Path(__file__).resolve().parent.parent

# De melding op deze pagina's is letterlijk wat een domeinklasse teruggeeft. De
# herkomstregel komt onder het hele meldingblok.
NA_MELDING = {
    "Registreren.cshtml":
        "Deze melding is letterlijk de returnwaarde van "
        "<code>AccountRepository.Register()</code>, de methode die jij schrijft.",
    "Inloggen.cshtml":
        "Deze melding is letterlijk de returnwaarde van jouw "
        "<code>AccountRepository</code>.",
    "WachtwoordVergeten.cshtml":
        "Deze melding komt uit jouw <code>PasswordResetService</code>.",
    "Orderbevestiging.cshtml":
        "Geldig of ongeldig wordt bepaald door <code>OrderSigner.Verify()</code>.",
    "Bestellen.cshtml":
        "Deze melding is de returnwaarde van jouw <code>CheckoutService</code>.",
}

# Op deze pagina's staat het bewijs ergens anders dan bij de melding.
NA_REGEL = {
    "Winkelmandje.cshtml": (
        re.compile(r"Totaal: @Model\.Totaal"),
        "Dit totaal komt uit <code>CartService.Total</code>. Staat er 0,00 terwijl "
        "je mandje gevuld is, dan is die property nog niet af.",
    ),
    "MijnBestellingen.cshtml": (
        re.compile(r"<h2>Wat er in de opslag staat</h2>"),
        "Wat hieronder staat, is door jouw <code>OrderRepository</code> en "
        "<code>OrderEncryptor</code> gegaan.",
    ),
    "Token.cshtml": (
        re.compile(r"<h2>Het token</h2>"),
        "Dit token is aangemaakt door <code>JwtTokenService.GenerateToken()</code>.",
    ),
}

MERK = 'class="herkomst"'


def regel(inspringing, tekst):
    return f'{inspringing}<div class="herkomst">{tekst}</div>'


def patch_na_melding(regels, tekst):
    """Zet de regel als laatste binnen @if (Model.Melding != string.Empty) { ... }.

    Binnen het blok, niet erna: is er geen melding, dan hoort er ook geen uitleg
    over de herkomst van die melding te staan.
    """
    for i, r in enumerate(regels):
        if "@if (Model.Melding != string.Empty)" in r:
            for j in range(i + 1, len(regels)):
                if regels[j].rstrip() == "}":
                    regels.insert(j, regel("    ", tekst))
                    return True
            return False
    return False


def patch_na_regel(regels, patroon, tekst):
    for i, r in enumerate(regels):
        if patroon.search(r):
            inspringing = r[: len(r) - len(r.lstrip())]
            regels.insert(i + 1, regel(inspringing, tekst))
            return True
    return False


def verwerk(pad):
    inhoud = pad.read_text(encoding="utf-8")
    if MERK in inhoud:
        return "stond er al"

    regels = inhoud.split("\n")
    naam = pad.name

    if naam in NA_MELDING:
        gelukt = patch_na_melding(regels, NA_MELDING[naam])
    elif naam in NA_REGEL:
        patroon, tekst = NA_REGEL[naam]
        gelukt = patch_na_regel(regels, patroon, tekst)
    else:
        return None

    if not gelukt:
        return "ANKER NIET GEVONDEN"

    pad.write_text("\n".join(regels), encoding="utf-8")
    return "bijgewerkt"


def main():
    mappen = [ROOT / "solutions", ROOT / "tools" / "skeletons"]
    telling = {}

    for map_ in mappen:
        for pad in sorted(map_.glob("*/ShopWave.Web/Pages/*.cshtml")):
            uitkomst = verwerk(pad)
            if uitkomst is None:
                continue
            telling[uitkomst] = telling.get(uitkomst, 0) + 1
            if uitkomst == "ANKER NIET GEVONDEN":
                print(f"  {uitkomst}: {pad.relative_to(ROOT)}")

    for sleutel, aantal in sorted(telling.items()):
        print(f"{sleutel}: {aantal}")

    return 1 if telling.get("ANKER NIET GEVONDEN") else 0


if __name__ == "__main__":
    sys.exit(main())
