---
title: "Les 5: Integration Testing — Theorie"
sidebar_label: "Theorie"
---

# Les 5: Integration Testing — Theorie

## Wat unit tests niet testen

Unit tests testen klassen in isolatie via mocks. Ze testen niet of klassen correct **samenwerken**. Mogelijke problemen die unit tests missen:

- Klasse A retourneert `null` in een scenario dat de mock nooit nabootst
- De volgorde van aanroepen tussen twee klassen is verkeerd
- Constructorinitializatie die afhankelijk is van een andere klasse

---

## Wat is een integration test?

Een integration test test de **samenwerking van twee of meer echte klassen**, zonder mocks voor eigen code. Externe diensten (databases, API's) mogen nog steeds gemockt worden.

| Kenmerk             | Unit test                | Integration test                  |
|---------------------|--------------------------|-----------------------------------|
| Eigen klassen       | Gemockt                  | Echt                              |
| Externe diensten    | Gemockt                  | Gemockt (mock server)             |
| Snelheid            | Snel                     | Trager                            |
| Foutdetectie        | Logica in één klasse     | Samenwerking tussen klassen       |
| Teststijl           | White box                | Black box                         |

---

## De testpiramide

```
      /\
     /  \   E2E — weinig, traag, fragiel
    /----\
   /      \  Integration — matig, medium snelheid
  /--------\
 /          \ Unit — veel, snel, goedkoop
/____________\
```

| Niveau      | Snelheid  | Scope                | Aantal |
|-------------|-----------|----------------------|--------|
| Unit        | Zeer snel | Één klasse           | Veel   |
| Integration | Trager    | Meerdere klassen     | Matig  |
| E2E         | Traag     | Volledige applicatie | Weinig |

---

## Callback-patroon voor 2FA-codes in tests

De `TwoFactorService` accepteert een `Action<string, string>`-callback om de gegenereerde code door te sturen. In tests onderschep je die code via een instantiemethode (geen lambda):

```csharp
public class LoginFlowIntegrationTests
{
    private string capturedCode = string.Empty;

    private void OnCodeGenerated(string email, string code)
    {
        this.capturedCode = code;
    }

    [Fact]
    public void Login_CorrectPassword_ThenVerifyCode_ReturnsGeslaagd()
    {
        TwoFactorService service = new TwoFactorService(
            validitySeconds: 60,
            onCodeGenerated: this.OnCodeGenerated);   // methode-referentie, geen lambda

        AccountRepository repo = new AccountRepository(service);
        repo.Register("alice@shopwave.be", "wachtwoord123");

        string loginResult  = repo.Login("alice@shopwave.be", "wachtwoord123");
        string verifyResult = repo.VerifyTwoFactor("alice@shopwave.be", this.capturedCode);

        loginResult.Should().Be("2FA vereist.");
        verifyResult.Should().Be("Inloggen geslaagd.");
    }
}
```

---

## Wanneer unit vs. integration?

| Scenario                                     | Gebruik           |
|----------------------------------------------|-------------------|
| Logica van één klasse testen                 | Unit test         |
| Randgevallen en uitzonderingen               | Unit test         |
| Volledige use case van begin tot einde       | Integration test  |
| Samenwerking van meerdere klassen            | Integration test  |
| Robuust na refactoring van interne structuur | Integration test  |