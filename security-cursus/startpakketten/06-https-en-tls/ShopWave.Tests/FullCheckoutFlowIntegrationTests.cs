using FluentAssertions;
using Moq;
using ShopWave;

namespace ShopWave.Tests
{
    public class FullCheckoutFlowIntegrationTests
    {
        [Fact]
        public void PlaceOrder_WithValidCouponZOMER10_ProcessesReducedAmount()
        {
            // Arrange
            Mock<IPaymentGateway> mockGateway = new Mock<IPaymentGateway>();
            mockGateway.Setup(g => g.ProcessPayment(It.IsAny<double>())).Returns(true);

            Mock<IStockService> mockStock = new Mock<IStockService>();
            mockStock.Setup(s => s.IsInStock(It.IsAny<int>(), It.IsAny<int>())).Returns(true);

            CouponService      couponService      = new CouponService();
            DiscountCalculator discountCalculator = new DiscountCalculator();
            OrderService       orderService       = new OrderService(
                mockGateway.Object,
                mockStock.Object,
                couponService);

            CartService cartService = new CartService(couponService, discountCalculator);
            cartService.AddItem("Laptop", 100.0);
            cartService.ApplyCoupon("ZOMER10");

            // Act
            string result = orderService.PlaceOrder(1, 1, cartService.Total);

            // Assert
            result.Should().Be("Bestelling bevestigd");
            mockGateway.Verify(g => g.ProcessPayment(90.0), Times.Once);
        }

        [Fact]
        public void PlaceOrder_WithInvalidCoupon_ProcessesFullAmount()
        {
            // Arrange
            Mock<IPaymentGateway> mockGateway = new Mock<IPaymentGateway>();
            mockGateway.Setup(g => g.ProcessPayment(It.IsAny<double>())).Returns(true);

            Mock<IStockService> mockStock = new Mock<IStockService>();
            mockStock.Setup(s => s.IsInStock(It.IsAny<int>(), It.IsAny<int>())).Returns(true);

            CouponService      couponService      = new CouponService();
            DiscountCalculator discountCalculator = new DiscountCalculator();
            OrderService       orderService       = new OrderService(
                mockGateway.Object,
                mockStock.Object,
                couponService);

            CartService cartService = new CartService(couponService, discountCalculator);
            cartService.AddItem("Laptop", 100.0);
            cartService.ApplyCoupon("ONGELDIG");

            // Act
            string result = orderService.PlaceOrder(1, 1, cartService.Total);

            // Assert
            result.Should().Be("Bestelling bevestigd");
            mockGateway.Verify(g => g.ProcessPayment(100.0), Times.Once);
        }

        [Fact]
        public void PlaceOrder_MultipleItemsNoCoupon_ProcessesCorrectTotal()
        {
            // Arrange
            Mock<IPaymentGateway> mockGateway = new Mock<IPaymentGateway>();
            mockGateway.Setup(g => g.ProcessPayment(It.IsAny<double>())).Returns(true);

            Mock<IStockService> mockStock = new Mock<IStockService>();
            mockStock.Setup(s => s.IsInStock(It.IsAny<int>(), It.IsAny<int>())).Returns(true);

            CouponService      couponService      = new CouponService();
            DiscountCalculator discountCalculator = new DiscountCalculator();
            OrderService       orderService       = new OrderService(
                mockGateway.Object,
                mockStock.Object,
                couponService);

            CartService cartService = new CartService(couponService, discountCalculator);
            cartService.AddItem("Laptop", 80.0);
            cartService.AddItem("Muis",   20.0);

            // Act
            string result = orderService.PlaceOrder(1, 1, cartService.Total);

            // Assert
            result.Should().Be("Bestelling bevestigd");
            mockGateway.Verify(g => g.ProcessPayment(100.0), Times.Once);
        }

        [Fact]
        public void PlaceOrder_WhenNotInStock_ReturnsNietBeschikbaar()
        {
            // Arrange
            Mock<IPaymentGateway> mockGateway = new Mock<IPaymentGateway>();

            Mock<IStockService> mockStock = new Mock<IStockService>();
            mockStock.Setup(s => s.IsInStock(It.IsAny<int>(), It.IsAny<int>())).Returns(false);

            CouponService      couponService      = new CouponService();
            DiscountCalculator discountCalculator = new DiscountCalculator();
            OrderService       orderService       = new OrderService(
                mockGateway.Object,
                mockStock.Object,
                couponService);

            CartService cartService = new CartService(couponService, discountCalculator);
            cartService.AddItem("Laptop", 100.0);

            // Act
            string result = orderService.PlaceOrder(1, 1, cartService.Total);

            // Assert
            result.Should().Be("Product niet beschikbaar");
            mockGateway.Verify(g => g.ProcessPayment(It.IsAny<double>()), Times.Never);
        }
    }
}
