Feature: Twee-factor authenticatie bij ShopWave

  Scenario: Succesvol inloggen met correcte 2FA-code
    Given er is een account voor "charlie@shopwave.be" met wachtwoord "pw123"
    When de gebruiker inlogt met het correcte wachtwoord voor "charlie@shopwave.be"
    And de gebruiker voert de correcte 2FA-code in voor "charlie@shopwave.be"
    Then is de gebruiker "charlie@shopwave.be" ingelogd

  Scenario: Inloggen met foute 2FA-code
    Given er is een account voor "charlie@shopwave.be" met wachtwoord "pw123"
    When de gebruiker inlogt met het correcte wachtwoord voor "charlie@shopwave.be"
    And de gebruiker voert een foute 2FA-code in voor "charlie@shopwave.be"
    Then ontvangt de gebruiker de melding "Ongeldige 2FA-code."

  Scenario Outline: 2FA-verificatie met verschillende codes
    Given er is een account voor "charlie@shopwave.be" met wachtwoord "pw123"
    When de gebruiker inlogt met het correcte wachtwoord voor "charlie@shopwave.be"
    And de gebruiker voert de 2FA-code "<type>" in voor "charlie@shopwave.be"
    Then ontvangt de gebruiker het resultaat "<resultaat>"

    Examples:
      | type    | resultaat            |
      | correct | Inloggen geslaagd.   |
      | fout    | Ongeldige 2FA-code.  |
