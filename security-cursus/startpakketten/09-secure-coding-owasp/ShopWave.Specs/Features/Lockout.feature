Feature: Account lockout bij ShopWave

  Scenario: Account vergrendeld na drie foute pogingen
    Given er is een account voor "bob@shopwave.be" met wachtwoord "veiligPw"
    When de gebruiker drie keer inlogt met een fout wachtwoord
    Then is het account van "bob@shopwave.be" geblokkeerd

  Scenario: Na blokkering werkt ook het correcte wachtwoord niet meer
    Given er is een account voor "bob@shopwave.be" met wachtwoord "veiligPw"
    When de gebruiker drie keer inlogt met een fout wachtwoord
    And de gebruiker inlogt met het correcte wachtwoord "veiligPw"
    Then ontvangt de gebruiker de melding "Account geblokkeerd."
