using FluentAssertions;
using ShopWave;

namespace ShopWave.Tests
{
    public class OrderConfirmationServiceIntegrationTests
    {
        [Fact]
        public void GenerateConfirmationCode_ForOrderId1_StartsWithORD()
        {
            // Arrange
            string capturedCode = string.Empty;

            OrderConfirmationService service = new OrderConfirmationService(
                onConfirmationCodeGenerated: code => { capturedCode = code; });

            // Act
            string result = service.GenerateConfirmationCode(1);

            // Assert
            result.Should().StartWith("ORD-");
            capturedCode.Should().StartWith("ORD-");
        }

        [Fact]
        public void GenerateConfirmationCode_CallbackReceivesSameValueAsReturn()
        {
            // Arrange
            string capturedCode = string.Empty;

            OrderConfirmationService service = new OrderConfirmationService(
                onConfirmationCodeGenerated: code => { capturedCode = code; });

            // Act
            string result = service.GenerateConfirmationCode(42);

            // Assert
            capturedCode.Should().Be(result);
        }

        [Fact]
        public void GenerateConfirmationCode_Result_PassesValidation()
        {
            // Arrange
            OrderConfirmationService service = new OrderConfirmationService();

            // Act
            string code = service.GenerateConfirmationCode(1);
            bool   isValid = service.ValidateCode(code);

            // Assert
            isValid.Should().BeTrue();
        }

        [Fact]
        public void GenerateConfirmationCode_TwoCalls_ProduceUniqueCode()
        {
            // Arrange
            OrderConfirmationService service = new OrderConfirmationService();

            // Act
            string code1 = service.GenerateConfirmationCode(1);
            string code2 = service.GenerateConfirmationCode(1);

            // Assert
            code1.Should().NotBe(code2);
        }
    }
}
