---
title: "Les 5: Integration Testing — Oefeningen"
sidebar_label: "Oefeningen"
---

# Les 5: Integration Testing — Oefeningen

> **Code-afspraken:** geen top-level statements · altijd `{}` · max één `return` · geen `break`/`continue` · geen underscore-prefix op parameters · geen geneste klassen · geen ternary/null-conditional · geen tuples · `double` i.p.v. `decimal` · identifiers Engels · tekst Nederlands

---

## Oefening 1 — Volledige loginflow als integration test

**Opgave:** Test de volledige loginflow met **echte klassen** (geen mocks voor eigen code). Gebruik de `onCodeGenerated`-callback als methodereferentie om de 2FA-code op te vangen.

```csharp
// LoginFlowIntegrationTests.cs
using FluentAssertions;
using ShopWave.Security;

namespace ShopWave.Tests
{
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
            // Arrange
            TwoFactorService twoFactorService = new TwoFactorService(
                validitySeconds: 60,
                onCodeGenerated: this.OnCodeGenerated);

            AccountRepository repository = new AccountRepository(twoFactorService);
            repository.Register("alice@shopwave.be", "wachtwoord123");

            // Act
            string loginResult  = repository.Login("alice@shopwave.be", "wachtwoord123");
            string verifyResult = repository.VerifyTwoFactor("alice@shopwave.be", this.capturedCode);

            // Assert
            loginResult.Should().Be("2FA vereist.");
            verifyResult.Should().Be("Inloggen geslaagd.");
        }

        [Fact]
        public void Login_SameCodeUsedTwice_SecondVerifyFails()
        {
            // Arrange
            TwoFactorService twoFactorService = new TwoFactorService(
                validitySeconds: 60,
                onCodeGenerated: this.OnCodeGenerated);

            AccountRepository repository = new AccountRepository(twoFactorService);
            repository.Register("alice@shopwave.be", "wachtwoord123");
            repository.Login("alice@shopwave.be", "wachtwoord123");

            // Act
            string firstVerify  = repository.VerifyTwoFactor("alice@shopwave.be", this.capturedCode);
            string secondVerify = repository.VerifyTwoFactor("alice@shopwave.be", this.capturedCode);

            // Assert
            firstVerify.Should().Be("Inloggen geslaagd.");
            secondVerify.Should().NotBe("Inloggen geslaagd.");
        }

        [Fact]
        public void Login_ThreeWrongPasswords_BlocksAccount()
        {
            // Arrange
            TwoFactorService  twoFactorService = new TwoFactorService();
            AccountRepository repository       = new AccountRepository(twoFactorService);
            repository.Register("alice@shopwave.be", "wachtwoord123");

            // Act
            repository.Login("alice@shopwave.be", "fout1");
            repository.Login("alice@shopwave.be", "fout2");
            string thirdResult  = repository.Login("alice@shopwave.be", "fout3");
            string afterLockout = repository.Login("alice@shopwave.be", "wachtwoord123");

            // Assert
            thirdResult.Should().Be("Account geblokkeerd.");
            afterLockout.Should().Be("Account geblokkeerd.");
        }
    }
}
```

---

## Oefening 2 — Verlopen 2FA-code

**Opgave:** Integration test die verifieert dat een verlopen 2FA-code de checkout tegenhoudt. Gebruik `validitySeconds: 0` en `Thread.Sleep(10)`.

```csharp
[Fact]
public void VerifyTwoFactor_WithExpiredCode_Fails()
{
    // Arrange
    TwoFactorService twoFactorService = new TwoFactorService(
        validitySeconds: 0,
        onCodeGenerated: this.OnCodeGenerated);

    AccountRepository repository = new AccountRepository(twoFactorService);
    repository.Register("alice@shopwave.be", "wachtwoord123");
    repository.Login("alice@shopwave.be", "wachtwoord123");

    System.Threading.Thread.Sleep(10);

    // Act
    string result = repository.VerifyTwoFactor("alice@shopwave.be", this.capturedCode);

    // Assert
    result.Should().NotBe("Inloggen geslaagd.");
}
```

---

## Oefening 3 — Handtekeningintegriteit na manipulatie

**Opgave:** Integration test: handtekening geldig op originele orderdata, ongeldig na prijsmanipulatie. Voeg CIA-commentaar toe bij elke assert.

```csharp
// OrderIntegrityIntegrationTests.cs
using FluentAssertions;
using ShopWave.Security;

namespace ShopWave.Tests
{
    public class OrderIntegrityIntegrationTests
    {
        [Fact]
        public void CreateSignedOrder_AfterManipulation_SignatureIsInvalid()
        {
            // Arrange — CIA: Integrity — handtekening beschermt orderdata
            SecureOrderService service = new SecureOrderService();

            // Act
            OrderResult order = service.CreateSignedOrder(
                "alice@shopwave.be", "Laptop", 999.99);

            // Assert — originele data is geldig
            bool originalValid = service.ValidateOrder(order.OrderData, order.Signature);
            originalValid.Should().BeTrue(
                "de handtekening van de originele orderdata moet geldig zijn");

            // Simuleer prijsmanipulatie door een aanvaller
            string manipulatedData = order.OrderData.Replace("999.99", "1.00");

            // Assert — gemanipuleerde data is ongeldig (CIA: Integrity)
            bool manipulatedValid = service.ValidateOrder(manipulatedData, order.Signature);
            manipulatedValid.Should().BeFalse(
                "een aangetaste handtekening moet gedetecteerd worden");
        }
    }
}
```

---

## Oefening 5 — Meerdere accounts tegelijk

**Opgave:** Integration test voor twee accounts door de volledige flow. Verifieer dat de handtekeningen van Alice en Bob niet uitwisselbaar zijn.

```csharp
[Fact]
public void TwoAccounts_SignaturesAreNotInterchangeable()
{
    // Arrange
    string aliceCode = string.Empty;
    string bobCode   = string.Empty;

    TwoFactorService aliceTfs = new TwoFactorService(
        validitySeconds: 60,
        onCodeGenerated: (email, code) => { aliceCode = code; });

    TwoFactorService bobTfs = new TwoFactorService(
        validitySeconds: 60,
        onCodeGenerated: (email, code) => { bobCode = code; });

    AccountRepository aliceRepo = new AccountRepository(aliceTfs);
    AccountRepository bobRepo   = new AccountRepository(bobTfs);

    aliceRepo.Register("alice@shopwave.be", "alicePw");
    bobRepo.Register("bob@shopwave.be", "bobPw");

    aliceRepo.Login("alice@shopwave.be", "alicePw");
    bobRepo.Login("bob@shopwave.be", "bobPw");

    aliceRepo.VerifyTwoFactor("alice@shopwave.be", aliceCode);
    bobRepo.VerifyTwoFactor("bob@shopwave.be", bobCode);

    SecureOrderService service = new SecureOrderService();

    // Act
    OrderResult aliceOrder = service.CreateSignedOrder("alice@shopwave.be", "Laptop", 999.99);
    OrderResult bobOrder   = service.CreateSignedOrder("bob@shopwave.be",   "Tablet",  499.99);

    // Assert — handtekening van Alice is ongeldig voor de orderdata van Bob
    bool aliceSigOnBobData = service.ValidateOrder(bobOrder.OrderData, aliceOrder.Signature);
    aliceSigOnBobData.Should().BeFalse(
        "handtekeningen zijn niet uitwisselbaar tussen accounts");
}
```