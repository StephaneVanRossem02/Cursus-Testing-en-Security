using FluentAssertions;
using Moq;
using ShopWave;

namespace ShopWave.Tests
{
    public class CartServiceCouponTests
    {
        [Fact]
        public void ApplyCoupon_WithValidCoupon_ReducesTotal()
        {
            Mock<ICouponService> mockCoupon = new Mock<ICouponService>();
            mockCoupon.Setup(c => c.IsValid("ZOMER10")).Returns(true);
            mockCoupon.Setup(c => c.GetDiscount("ZOMER10")).Returns(10);

            CartService cart = new CartService(mockCoupon.Object);
            cart.AddItem("Laptop", 100.0);
            cart.ApplyCoupon("ZOMER10");

            cart.Total.Should().Be(90.0);
        }

        [Fact]
        public void ApplyCoupon_WithInvalidCoupon_DoesNotChangeTotal()
        {
            Mock<ICouponService> mockCoupon = new Mock<ICouponService>();
            mockCoupon.Setup(c => c.IsValid("ONGELDIG")).Returns(false);

            CartService cart = new CartService(mockCoupon.Object);
            cart.AddItem("Laptop", 100.0);
            cart.ApplyCoupon("ONGELDIG");

            cart.Total.Should().Be(100.0);
        }

        [Fact]
        public void ApplyCoupon_WithValidCoupon_CallsMarkAsUsedOnce()
        {
            Mock<ICouponService> mockCoupon = new Mock<ICouponService>();
            mockCoupon.Setup(c => c.IsValid("ZOMER10")).Returns(true);
            mockCoupon.Setup(c => c.GetDiscount("ZOMER10")).Returns(10);

            CartService cart = new CartService(mockCoupon.Object);
            cart.AddItem("Laptop", 100.0);
            cart.ApplyCoupon("ZOMER10");

            mockCoupon.Verify(c => c.MarkAsUsed("ZOMER10"), Times.Once);
        }

        [Fact]
        public void ApplyCoupon_WithInvalidCoupon_NeverCallsMarkAsUsed()
        {
            Mock<ICouponService> mockCoupon = new Mock<ICouponService>();
            mockCoupon.Setup(c => c.IsValid("ONGELDIG")).Returns(false);

            CartService cart = new CartService(mockCoupon.Object);
            cart.AddItem("Laptop", 100.0);
            cart.ApplyCoupon("ONGELDIG");

            mockCoupon.Verify(c => c.MarkAsUsed(It.IsAny<string>()), Times.Never);
        }
    }
}
