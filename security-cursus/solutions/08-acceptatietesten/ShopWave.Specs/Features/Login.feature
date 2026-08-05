Feature: Inloggen bij ShopWave

  Scenario Outline: Inloggen met verschillende wachtwoorden
    Given er is een account voor "alice@shopwave.be" met wachtwoord "wachtwoord123"
    When de gebruiker inlogt met "alice@shopwave.be" en "<wachtwoord>"
    Then ontvangt de gebruiker de melding "<melding>"

    Examples:
      | wachtwoord     | melding                  |
      | wachtwoord123  | Voer uw 2FA-code in.     |
      | foutWachtwoord | Ongeldig wachtwoord.     |
      |                | Ongeldig wachtwoord.     |
