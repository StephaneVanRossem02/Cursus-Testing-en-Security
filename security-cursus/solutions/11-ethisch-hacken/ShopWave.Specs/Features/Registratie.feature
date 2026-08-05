Feature: Registratie bij ShopWave

  Scenario: Registratie van een nieuw account
    Given er bestaat nog geen account voor "david@shopwave.be"
    When de gebruiker zich registreert met e-mailadres "david@shopwave.be" en wachtwoord "veiligPw99"
    Then is het account aangemaakt

  Scenario: Registratie van een bestaand account
    Given er is al een account voor "david@shopwave.be"
    When de gebruiker zich opnieuw registreert met hetzelfde e-mailadres "david@shopwave.be"
    Then ontvangt de gebruiker de registratiefout "Account bestaat al."
