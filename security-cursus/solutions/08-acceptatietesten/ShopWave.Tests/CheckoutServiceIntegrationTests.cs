using FluentAssertions;
using Moq;
using ShopWave;

namespace ShopWave.Tests
{
    // Aangepast t.o.v. de bron: deze tests (oplossing 1) gebruikten de CartService met
    // constructor van een eerdere stap. De definitieve CartService (oplossing 2) vereist ook
    // een DiscountCalculator. De constructoraanroepen zijn daarom uitgebreid. Zie README.
    public class CheckoutServiceIntegrationTests
    {
        [Fact]
        public void Checkout_WithOneItem_ProcessesCorrectAmount()
        {
            // Arrange
            Mock<IPaymentGateway> mockGateway = new Mock<IPaymentGateway>();
            mockGateway.Setup(g => g.ProcessPayment(It.IsAny<double>())).Returns(true);

            CouponService  couponService  = new CouponService();
            CartService    cartService    = new CartService(couponService, new DiscountCalculator());
            CheckoutService checkoutService = new CheckoutService(cartService, mockGateway.Object);

            cartService.AddItem("Laptop", 100.0);

            // Act
            string result = checkoutService.Checkout();

            // Assert
            result.Should().Be("Betaling geslaagd");
            mockGateway.Verify(g => g.ProcessPayment(100.0), Times.Once);
        }

        [Fact]
        public void Checkout_EmptyCart_ReturnsMandjeLegeMelding()
        {
            // Arrange
            Mock<IPaymentGateway> mockGateway = new Mock<IPaymentGateway>();

            CouponService  couponService  = new CouponService();
            CartService    cartService    = new CartService(couponService, new DiscountCalculator());
            CheckoutService checkoutService = new CheckoutService(cartService, mockGateway.Object);

            // Act
            string result = checkoutService.Checkout();

            // Assert
            result.Should().Be("Mandje is leeg");
            mockGateway.Verify(g => g.ProcessPayment(It.IsAny<double>()), Times.Never);
        }

        [Fact]
        public void Checkout_WithValidCoupon_ProcessesReducedAmount()
        {
            // Arrange
            Mock<IPaymentGateway> mockGateway = new Mock<IPaymentGateway>();
            mockGateway.Setup(g => g.ProcessPayment(It.IsAny<double>())).Returns(true);

            CouponService  couponService  = new CouponService();
            CartService    cartService    = new CartService(couponService, new DiscountCalculator());
            CheckoutService checkoutService = new CheckoutService(cartService, mockGateway.Object);

            cartService.AddItem("Laptop", 100.0);
            cartService.ApplyCoupon("ZOMER10");

            // Act
            string result = checkoutService.Checkout();

            // Assert
            result.Should().Be("Betaling geslaagd");
            mockGateway.Verify(g => g.ProcessPayment(90.0), Times.Once);
        }

        [Fact]
        public void Checkout_WhenPaymentFails_ReturnsMislukt()
        {
            // Arrange
            Mock<IPaymentGateway> mockGateway = new Mock<IPaymentGateway>();
            mockGateway.Setup(g => g.ProcessPayment(It.IsAny<double>())).Returns(false);

            CouponService  couponService  = new CouponService();
            CartService    cartService    = new CartService(couponService, new DiscountCalculator());
            CheckoutService checkoutService = new CheckoutService(cartService, mockGateway.Object);

            cartService.AddItem("Laptop", 100.0);

            // Act
            string result = checkoutService.Checkout();

            // Assert
            result.Should().Be("Betaling mislukt");
        }
    }
}
