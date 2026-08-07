using FluentAssertions;
using Moq;
using ShopWave;

namespace ShopWave.Tests
{
    public class CheckoutServiceTests
    {
        [Fact]
        public void CalculateFinalTotal_WithNoDiscountAndShipping_ReturnsTotalPlusShipping()
        {
            Mock<IShippingService> mockShipping = new Mock<IShippingService>();
            mockShipping.Setup(s => s.GetShippingCost(It.IsAny<double>())).Returns(5.0);
            CheckoutService service = new CheckoutService(mockShipping.Object);

            double result = service.CalculateFinalTotal(10.0, 3, 0);

            result.Should().Be(35.0);
        }

        [Fact]
        public void CalculateFinalTotal_CallsGetShippingCostExactlyOnce()
        {
            Mock<IShippingService> mockShipping = new Mock<IShippingService>();
            mockShipping.Setup(s => s.GetShippingCost(It.IsAny<double>())).Returns(0.0);
            CheckoutService service = new CheckoutService(mockShipping.Object);

            service.CalculateFinalTotal(10.0, 1, 0);

            mockShipping.Verify(s => s.GetShippingCost(It.IsAny<double>()), Times.Once);
        }
    }
}
