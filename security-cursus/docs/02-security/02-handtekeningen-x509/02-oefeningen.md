---
title: "Les 4: Security — 2FA, Digitale Handtekeningen en X.509 — Oefeningen"
sidebar_label: "Oefeningen"
---

# Les 4: Security — 2FA, Digitale Handtekeningen en X.509 — Oefeningen

> **Code-afspraken:** geen top-level statements · altijd `{}` · max één `return` · geen `break`/`continue` · geen underscore-prefix op parameters · geen geneste klassen · geen ternary/null-conditional · geen tuples · `double` i.p.v. `decimal` · identifiers Engels · tekst Nederlands

---

## Oefening 1 — TwoFactorService testen

**Opgave:** Test: code niet leeg, correcte code geaccepteerd, foute code geweigerd, onbekend e-mailadres geweigerd, code kan maar één keer gebruikt worden, verlopen code geweigerd (`validitySeconds: 0` + `Thread.Sleep(10)`).

```csharp
// TwoFactorServiceTests.cs
using FluentAssertions;
using ShopWave.Security;

namespace ShopWave.Tests
{
    public class TwoFactorServiceTests
    {
        [Fact]
        public void GenerateCode_ForValidEmail_ReturnsNonEmptyCode()
        {
            // Arrange
            TwoFactorService service = new TwoFactorService();

            // Act
            string code = service.GenerateCode("alice@shopwave.be");

            // Assert
            code.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void VerifyCode_WithCorrectCode_ReturnsTrue()
        {
            // Arrange
            TwoFactorService service = new TwoFactorService();
            string code = service.GenerateCode("alice@shopwave.be");

            // Act
            bool result = service.VerifyCode("alice@shopwave.be", code);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void VerifyCode_WithWrongCode_ReturnsFalse()
        {
            // Arrange
            TwoFactorService service = new TwoFactorService();
            service.GenerateCode("alice@shopwave.be");

            // Act
            bool result = service.VerifyCode("alice@shopwave.be", "000000");

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void VerifyCode_ForUnknownEmail_ReturnsFalse()
        {
            // Arrange
            TwoFactorService service = new TwoFactorService();

            // Act
            bool result = service.VerifyCode("onbekend@shopwave.be", "123456");

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void VerifyCode_UsedCodeVerifiedAgain_ReturnsFalse()
        {
            // Arrange
            TwoFactorService service = new TwoFactorService();
            string code = service.GenerateCode("alice@shopwave.be");
            service.VerifyCode("alice@shopwave.be", code); // eerste gebruik

            // Act
            bool result = service.VerifyCode("alice@shopwave.be", code); // tweede gebruik

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void VerifyCode_ExpiredCode_ReturnsFalse()
        {
            // Arrange
            TwoFactorService service = new TwoFactorService(validitySeconds: 0);
            string code = service.GenerateCode("alice@shopwave.be");
            System.Threading.Thread.Sleep(10);

            // Act
            bool result = service.VerifyCode("alice@shopwave.be", code);

            // Assert
            result.Should().BeFalse();
        }
    }
}
```

---

## Oefening 3 — OrderSigner testen

**Opgave:** Test: handtekening van originele data is geldig, van gemanipuleerde data ongeldig, handtekening met certificaat A is ongeldig bij certificaat B.

```csharp
// OrderSignerTests.cs
using FluentAssertions;
using ShopWave.Security;
using System.Security.Cryptography.X509Certificates;

namespace ShopWave.Tests
{
    public class OrderSignerTests
    {
        private readonly X509Certificate2 certificate;
        private readonly OrderSigner      signer;

        public OrderSignerTests()
        {
            this.certificate = CertificateHelper.CreateSelfSignedCertificate("ShopWave");
            this.signer      = new OrderSigner(this.certificate);
        }

        [Fact]
        public void Verify_OriginalOrderData_ReturnsTrue()
        {
            // Arrange
            string orderData = "ORD-001 | Alice | Laptop | 999.99 EUR";
            string signature = this.signer.Sign(orderData);

            // Act
            bool result = this.signer.Verify(orderData, signature);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void Verify_ManipulatedOrderData_ReturnsFalse()
        {
            // Arrange
            string originalData    = "ORD-001 | Alice | Laptop | 999.99 EUR";
            string manipulatedData = "ORD-001 | Alice | Laptop | 1.00 EUR";
            string signature       = this.signer.Sign(originalData);

            // Act
            bool result = this.signer.Verify(manipulatedData, signature);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void Verify_WithDifferentCertificate_ReturnsFalse()
        {
            // Arrange
            X509Certificate2 otherCert  = CertificateHelper.CreateSelfSignedCertificate("AndereServer");
            OrderSigner      otherSigner = new OrderSigner(otherCert);

            string orderData  = "ORD-001 | Alice | Laptop | 999.99 EUR";
            string signature  = this.signer.Sign(orderData);

            // Act
            bool result = otherSigner.Verify(orderData, signature);

            // Assert
            result.Should().BeFalse();
        }
    }
}
```

---

## Oefening 4 — SecureOrderService

**Opgave:** `SecureOrderService.CreateSignedOrder` bouwt een orderstring op (formaat: `"{email} | {product} | {prijs:0.00} EUR"`) en ondertekent die. `ValidateOrder` verifieert de handtekening. Gebruik `CultureInfo.InvariantCulture` voor de prijsformattering.

**OrderResult.cs**

```csharp
namespace ShopWave.Security
{
    public class OrderResult
    {
        public string OrderData { get; set; }
        public string Signature { get; set; }

        public OrderResult(string orderData, string signature)
        {
            this.OrderData = orderData;
            this.Signature = signature;
        }
    }
}
```

**SecureOrderService.cs**

```csharp
using System.Globalization;
using System.Security.Cryptography.X509Certificates;

namespace ShopWave.Security
{
    public class SecureOrderService
    {
        private readonly OrderSigner signer;

        public SecureOrderService()
        {
            X509Certificate2 certificate = CertificateHelper.CreateSelfSignedCertificate("ShopWave");
            this.signer = new OrderSigner(certificate);
        }

        public OrderResult CreateSignedOrder(string customerEmail, string productName, double price)
        {
            string orderData = string.Format(
                CultureInfo.InvariantCulture,
                "{0} | {1} | {2:0.00} EUR",
                customerEmail,
                productName,
                price);

            string signature = this.signer.Sign(orderData);

            return new OrderResult(orderData, signature);
        }

        public bool ValidateOrder(string orderData, string signature)
        {
            return this.signer.Verify(orderData, signature);
        }
    }
}
```

---

## Oefening 5 — CIA-koppeling (schriftelijk)

Beantwoord per technologie welke CIA-pijler beschermd wordt en waarom:

1. **2FA** → Confidentiality — beschermt toegang, ook bij gestolen wachtwoord
2. **Digitale handtekening** → Integrity — manipulatie van orderdata onmiddellijk detecteerbaar
3. **AES-encryptie** → Confidentiality — orderdata onleesbaar zonder de sleutel
4. **Prijsmanipulatie gedetecteerd via handtekening** → Integrity — de server ziet dat de handtekening niet meer klopt met de gewijzigde data