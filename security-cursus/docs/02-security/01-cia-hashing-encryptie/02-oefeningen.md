---
title: "Les 2: Security — CIA-model, Hashing en Encryptie — Oefeningen"
sidebar_label: "Oefeningen"
---

# Les 2: Security — CIA-model, Hashing en Encryptie — Oefeningen

> **Code-afspraken:** geen top-level statements · altijd `{}` · max één `return` · geen `break`/`continue` · geen underscore-prefix op parameters · geen geneste klassen · geen ternary/null-conditional · geen tuples · `double` i.p.v. `decimal` · identifiers Engels · tekst Nederlands

---

## Oefening 1 — CustomerAccount testen

**Opgave:** Schrijf `CustomerAccountTests.cs` met vijf tests: hash niet leeg, hash begint met BCrypt-prefix, verificatie slaagt met juist wachtwoord, faalt met fout wachtwoord, twee accounts met zelfde wachtwoord hebben andere hash.

```csharp
// CustomerAccountTests.cs
using FluentAssertions;
using ShopWave.Security;

namespace ShopWave.Tests
{
    public class CustomerAccountTests
    {
        [Fact]
        public void CustomerAccount_AfterCreation_PasswordHashIsNotEmpty()
        {
            // Arrange & Act
            CustomerAccount account = new CustomerAccount("alice@shopwave.be", "wachtwoord123");

            // Assert
            account.PasswordHash.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void CustomerAccount_PasswordHash_StartsWithBCryptPrefix()
        {
            // Arrange & Act
            CustomerAccount account = new CustomerAccount("alice@shopwave.be", "wachtwoord123");

            // Assert
            bool startsCorrect = account.PasswordHash.StartsWith("$2a$")
                              || account.PasswordHash.StartsWith("$2b$");
            startsCorrect.Should().BeTrue();
        }

        [Fact]
        public void VerifyPassword_WithCorrectPassword_ReturnsTrue()
        {
            // Arrange
            CustomerAccount account = new CustomerAccount("alice@shopwave.be", "wachtwoord123");

            // Act
            bool result = account.VerifyPassword("wachtwoord123");

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void VerifyPassword_WithWrongPassword_ReturnsFalse()
        {
            // Arrange
            CustomerAccount account = new CustomerAccount("alice@shopwave.be", "wachtwoord123");

            // Act
            bool result = account.VerifyPassword("foutWachtwoord");

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void TwoAccountsWithSamePassword_HaveDifferentHashes()
        {
            // Arrange
            CustomerAccount alice = new CustomerAccount("alice@shopwave.be", "wachtwoord123");
            CustomerAccount bob   = new CustomerAccount("bob@shopwave.be",   "wachtwoord123");

            // Assert
            alice.PasswordHash.Should().NotBe(bob.PasswordHash);
        }
    }
}
```

---

## Oefening 2 & 3 — AccountRepository met lockout

**Opgave:** `AccountRepository` met `Register` en `Login`. Login retourneert `"Inloggen geslaagd."`, `"Inloggen mislukt."`, `"Account geblokkeerd."` (na 3 mislukte pogingen), of `"Gebruiker niet gevonden."`. Schrijf ook de bijbehorende tests.

**AccountRepository.cs**

```csharp
namespace ShopWave.Security
{
    public class AccountRepository
    {
        private readonly Dictionary<string, CustomerAccount> accounts;
        private readonly Dictionary<string, int>             failedAttempts;
        private const int MaxAttempts = 3;

        public AccountRepository()
        {
            this.accounts       = new Dictionary<string, CustomerAccount>();
            this.failedAttempts = new Dictionary<string, int>();
        }

        public void Register(string email, string password)
        {
            CustomerAccount account    = new CustomerAccount(email, password);
            this.accounts[email]       = account;
            this.failedAttempts[email] = 0;
        }

        public string Login(string email, string password)
        {
            string result;

            if (!this.accounts.ContainsKey(email))
            {
                result = "Gebruiker niet gevonden.";
            }
            else if (this.failedAttempts[email] >= MaxAttempts)
            {
                result = "Account geblokkeerd.";
            }
            else
            {
                bool correct = this.accounts[email].VerifyPassword(password);

                if (correct)
                {
                    this.failedAttempts[email] = 0;
                    result = "Inloggen geslaagd.";
                }
                else
                {
                    this.failedAttempts[email]++;

                    if (this.failedAttempts[email] >= MaxAttempts)
                    {
                        result = "Account geblokkeerd.";
                    }
                    else
                    {
                        result = "Inloggen mislukt.";
                    }
                }
            }

            return result;
        }
    }
}
```

**AccountRepositoryTests.cs**

```csharp
using FluentAssertions;
using ShopWave.Security;

namespace ShopWave.Tests
{
    public class AccountRepositoryTests
    {
        [Fact]
        public void Login_WithCorrectPassword_ReturnsGeslaagd()
        {
            // Arrange
            AccountRepository repository = new AccountRepository();
            repository.Register("alice@shopwave.be", "wachtwoord123");

            // Act
            string result = repository.Login("alice@shopwave.be", "wachtwoord123");

            // Assert
            result.Should().Be("Inloggen geslaagd.");
        }

        [Fact]
        public void Login_WithWrongPassword_ReturnsMislukt()
        {
            // Arrange
            AccountRepository repository = new AccountRepository();
            repository.Register("alice@shopwave.be", "wachtwoord123");

            // Act
            string result = repository.Login("alice@shopwave.be", "foutWachtwoord");

            // Assert
            result.Should().Be("Inloggen mislukt.");
        }

        [Fact]
        public void Login_WithUnknownEmail_ReturnsNietGevonden()
        {
            // Arrange
            AccountRepository repository = new AccountRepository();

            // Act
            string result = repository.Login("onbekend@shopwave.be", "wachtwoord");

            // Assert
            result.Should().Be("Gebruiker niet gevonden.");
        }

        [Fact]
        public void Login_AfterThreeWrongAttempts_ReturnsGeblokkeerd()
        {
            // Arrange
            AccountRepository repository = new AccountRepository();
            repository.Register("alice@shopwave.be", "wachtwoord123");
            repository.Login("alice@shopwave.be", "fout1");
            repository.Login("alice@shopwave.be", "fout2");
            repository.Login("alice@shopwave.be", "fout3");

            // Act
            string result = repository.Login("alice@shopwave.be", "wachtwoord123");

            // Assert
            result.Should().Be("Account geblokkeerd.");
        }

        [Fact]
        public void Login_AfterSuccessfulLogin_ResetsFailedCounter()
        {
            // Arrange
            AccountRepository repository = new AccountRepository();
            repository.Register("alice@shopwave.be", "wachtwoord123");
            repository.Login("alice@shopwave.be", "fout1");
            repository.Login("alice@shopwave.be", "wachtwoord123"); // reset

            // Act
            string result = repository.Login("alice@shopwave.be", "fout2");

            // Assert
            result.Should().Be("Inloggen mislukt.");
        }
    }
}
```

---

## Oefening 4 — OrderEncryptor

**Opgave:** `OrderEncryptor` omhult `AesEncryptor` met een interne sleutel. Drie tests: versleuteld ≠ origineel, ontsleuteld = origineel, twee versleutelingen van zelfde tekst geven andere ciphertexts (willekeurige IV).

**OrderEncryptor.cs**

```csharp
namespace ShopWave.Security
{
    public class OrderEncryptor
    {
        private const string Key = "ShopWaveSecretKey12345678901234"; // 32 tekens

        public string EncryptOrderData(string orderData)
        {
            AesEncryptor encryptor = new AesEncryptor();
            return encryptor.Encrypt(orderData, Key);
        }

        public string DecryptOrderData(string encryptedData)
        {
            AesEncryptor encryptor = new AesEncryptor();
            return encryptor.Decrypt(encryptedData, Key);
        }
    }
}
```

**OrderEncryptorTests.cs**

```csharp
using FluentAssertions;
using ShopWave.Security;

namespace ShopWave.Tests
{
    public class OrderEncryptorTests
    {
        [Fact]
        public void EncryptOrderData_ReturnsValueDifferentFromOriginal()
        {
            OrderEncryptor encryptor = new OrderEncryptor();
            string original  = "alice@shopwave.be | Laptop | 999.99 EUR";
            string encrypted = encryptor.EncryptOrderData(original);
            encrypted.Should().NotBe(original);
        }

        [Fact]
        public void DecryptOrderData_ReturnsOriginalString()
        {
            OrderEncryptor encryptor = new OrderEncryptor();
            string original  = "alice@shopwave.be | Laptop | 999.99 EUR";
            string encrypted = encryptor.EncryptOrderData(original);
            string decrypted = encryptor.DecryptOrderData(encrypted);
            decrypted.Should().Be(original);
        }

        [Fact]
        public void EncryptOrderData_TwiceSameInput_GivesDifferentCiphertexts()
        {
            OrderEncryptor encryptor  = new OrderEncryptor();
            string original           = "alice@shopwave.be | Laptop | 999.99 EUR";
            string encrypted1         = encryptor.EncryptOrderData(original);
            string encrypted2         = encryptor.EncryptOrderData(original);

            // Willekeurige IV geeft elke keer andere ciphertext
            encrypted1.Should().NotBe(encrypted2);

            // Maar beide kunnen ontsleuteld worden naar het origineel
            encryptor.DecryptOrderData(encrypted1).Should().Be(original);
            encryptor.DecryptOrderData(encrypted2).Should().Be(original);
        }
    }
}
```