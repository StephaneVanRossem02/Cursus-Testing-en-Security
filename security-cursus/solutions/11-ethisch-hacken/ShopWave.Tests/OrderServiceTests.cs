using FluentAssertions;
using Moq;
using ShopWave;

namespace ShopWave.Tests
{
    // Aangepast t.o.v. les 1: OrderService kreeg in les 3 een derde constructor-parameter
    // (ICouponService). De constructoraanroepen zijn daarom uitgebreid met een mock. Er wordt
    // geen coupon gebruikt, dus het gedrag van deze tests blijft identiek. Zie README.
    public class OrderServiceTests
    {
        [Fact]
        public void PlaceOrder_WhenNotInStock_ReturnsNietBeschikbaar()
        {
            Mock<IPaymentGateway> mockGateway = new Mock<IPaymentGateway>();
            Mock<IStockService> mockStock = new Mock<IStockService>();
            Mock<ICouponService> mockCoupon = new Mock<ICouponService>();
            mockStock.Setup(s => s.IsInStock(1, 1)).Returns(false);
            OrderService service = new OrderService(mockGateway.Object, mockStock.Object, mockCoupon.Object);

            string result = service.PlaceOrder(1, 1, 50.0);

            result.Should().Be("Product niet beschikbaar");
        }

        [Fact]
        public void PlaceOrder_WhenNotInStock_NeverCallsProcessPayment()
        {
            Mock<IPaymentGateway> mockGateway = new Mock<IPaymentGateway>();
            Mock<IStockService> mockStock = new Mock<IStockService>();
            Mock<ICouponService> mockCoupon = new Mock<ICouponService>();
            mockStock.Setup(s => s.IsInStock(It.IsAny<int>(), It.IsAny<int>())).Returns(false);
            OrderService service = new OrderService(mockGateway.Object, mockStock.Object, mockCoupon.Object);

            service.PlaceOrder(1, 1, 50.0);

            mockGateway.Verify(g => g.ProcessPayment(It.IsAny<double>()), Times.Never);
        }

        [Fact]
        public void PlaceOrder_WhenInStockAndPaymentSucceeds_ReturnsBevestigd()
        {
            Mock<IPaymentGateway> mockGateway = new Mock<IPaymentGateway>();
            Mock<IStockService> mockStock = new Mock<IStockService>();
            Mock<ICouponService> mockCoupon = new Mock<ICouponService>();
            mockGateway.Setup(g => g.ProcessPayment(50.0)).Returns(true);
            mockStock.Setup(s => s.IsInStock(1, 1)).Returns(true);
            OrderService service = new OrderService(mockGateway.Object, mockStock.Object, mockCoupon.Object);

            string result = service.PlaceOrder(1, 1, 50.0);

            result.Should().Be("Bestelling bevestigd");
            mockGateway.Verify(g => g.ProcessPayment(50.0), Times.Once);
        }

        [Fact]
        public void PlaceOrder_WithInvalidAmount_NeverCallsIsInStock()
        {
            Mock<IPaymentGateway> mockGateway = new Mock<IPaymentGateway>();
            Mock<IStockService> mockStock = new Mock<IStockService>();
            Mock<ICouponService> mockCoupon = new Mock<ICouponService>();
            OrderService service = new OrderService(mockGateway.Object, mockStock.Object, mockCoupon.Object);

            Action act = () => service.PlaceOrder(1, 1, -10.0);

            act.Should().Throw<ArgumentException>();
            mockStock.Verify(s => s.IsInStock(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }
    }
}
