using FluentAssertions;
using Moq;
using ShopWave;

namespace ShopWave.Tests
{
    // Demo uit de theorie: de bestelflow als integration test. De constructoraanroep van
    // CartService is aangepast aan de definitieve versie met DiscountCalculator. Zie README.
    public class CheckoutFlowIntegrationTests
    {
        [Fact]
        public void CheckoutFlow_WithValidCoupon_ProcessesCorrectAmount()
        {
            // Arrange
            Mock<IPaymentGateway> mockGateway = new Mock<IPaymentGateway>();
            mockGateway.Setup(g => g.ProcessPayment(It.IsAny<double>())).Returns(true);

            Mock<IStockService> mockStock = new Mock<IStockService>();
            mockStock.Setup(s => s.IsInStock(It.IsAny<int>(), It.IsAny<int>())).Returns(true);

            CouponService couponService = new CouponService();
            OrderService  orderService  = new OrderService(
                mockGateway.Object,
                mockStock.Object,
                couponService);

            CartService cartService = new CartService(couponService, new DiscountCalculator());
            cartService.AddItem("Laptop", 100.0);
            cartService.ApplyCoupon("ZOMER10");

            // Act
            string result = orderService.PlaceOrder(1, 1, cartService.Total);

            // Assert
            result.Should().Be("Bestelling bevestigd");
            mockGateway.Verify(
                g => g.ProcessPayment(90.0),
                Times.Once,
                "de betaling moet het bedrag na korting bevatten");
        }

        [Fact]
        public void CheckoutFlow_WithInvalidCoupon_ProcessesFullAmount()
        {
            // Arrange
            Mock<IPaymentGateway> mockGateway = new Mock<IPaymentGateway>();
            mockGateway.Setup(g => g.ProcessPayment(It.IsAny<double>())).Returns(true);

            Mock<IStockService> mockStock = new Mock<IStockService>();
            mockStock.Setup(s => s.IsInStock(It.IsAny<int>(), It.IsAny<int>())).Returns(true);

            CouponService couponService = new CouponService();
            OrderService  orderService  = new OrderService(
                mockGateway.Object,
                mockStock.Object,
                couponService);

            CartService cartService = new CartService(couponService, new DiscountCalculator());
            cartService.AddItem("Laptop", 100.0);
            cartService.ApplyCoupon("ONGELDIG");

            // Act
            string result = orderService.PlaceOrder(1, 1, cartService.Total);

            // Assert
            result.Should().Be("Bestelling bevestigd");
            mockGateway.Verify(
                g => g.ProcessPayment(100.0),
                Times.Once,
                "een ongeldige coupon mag het bedrag niet verlagen");
        }
    }
}
