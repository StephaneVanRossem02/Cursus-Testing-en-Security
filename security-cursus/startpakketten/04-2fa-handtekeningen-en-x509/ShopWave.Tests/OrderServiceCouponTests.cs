using FluentAssertions;
using Moq;
using ShopWave;

namespace ShopWave.Tests
{
    public class OrderServiceCouponTests
    {
        private Mock<IPaymentGateway> mockGateway;
        private Mock<IStockService>   mockStock;
        private Mock<ICouponService>  mockCoupon;
        private OrderService          service;

        public OrderServiceCouponTests()
        {
            mockGateway = new Mock<IPaymentGateway>();
            mockStock   = new Mock<IStockService>();
            mockCoupon  = new Mock<ICouponService>();
            service     = new OrderService(mockGateway.Object, mockStock.Object, mockCoupon.Object);
        }

        [Fact]
        public void PlaceOrder_WithoutCoupon_ProcessesFullAmount()
        {
            mockStock.Setup(s => s.IsInStock(1, 1)).Returns(true);
            mockGateway.Setup(g => g.ProcessPayment(100.0)).Returns(true);

            string result = service.PlaceOrder(1, 1, 100.0);

            result.Should().Be("Bestelling bevestigd");
            mockGateway.Verify(g => g.ProcessPayment(100.0), Times.Once);
        }

        [Fact]
        public void PlaceOrder_WithValidCoupon_ProcessesReducedAmount()
        {
            mockStock.Setup(s => s.IsInStock(1, 1)).Returns(true);
            mockCoupon.Setup(c => c.IsValid("ZOMER10")).Returns(true);
            mockCoupon.Setup(c => c.GetDiscount("ZOMER10")).Returns(10);
            mockGateway.Setup(g => g.ProcessPayment(90.0)).Returns(true);

            string result = service.PlaceOrder(1, 1, 100.0, "ZOMER10");

            result.Should().Be("Bestelling bevestigd");
            mockGateway.Verify(g => g.ProcessPayment(90.0), Times.Once);
        }

        [Fact]
        public void PlaceOrder_WithUsedCoupon_ReturnsCouponGebruikt()
        {
            mockCoupon.Setup(c => c.IsValid("ZOMER10")).Returns(false);

            string result = service.PlaceOrder(1, 1, 100.0, "ZOMER10");

            result.Should().Be("Coupon reeds gebruikt.");
        }

        [Fact]
        public void PlaceOrder_WithUsedCoupon_NeverCallsProcessPayment()
        {
            mockCoupon.Setup(c => c.IsValid("ZOMER10")).Returns(false);

            service.PlaceOrder(1, 1, 100.0, "ZOMER10");

            mockGateway.Verify(g => g.ProcessPayment(It.IsAny<double>()), Times.Never);
        }

        [Fact]
        public void PlaceOrder_WhenNotInStock_ReturnsNietBeschikbaar()
        {
            mockStock.Setup(s => s.IsInStock(1, 1)).Returns(false);

            string result = service.PlaceOrder(1, 1, 100.0);

            result.Should().Be("Product niet beschikbaar");
        }
    }
}
