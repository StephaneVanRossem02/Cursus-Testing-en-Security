using FluentAssertions;
using Moq;
using ShopWave;

namespace ShopWave.Tests
{
    // Aangepast t.o.v. de bron: de definitieve CartService (oplossing 2) vereist een
    // ICouponService in de constructor. Deze basistests uit oplossing 1 gebruikten de
    // parameterloze constructor. De constructoraanroep is daarom aangepast met een mock;
    // er wordt geen coupon toegepast, dus de totalen blijven identiek. Zie README.
    public class CartServiceTests
    {
        [Fact]
        public void Total_EmptyCart_ReturnsZero()
        {
            Mock<ICouponService> mockCoupon = new Mock<ICouponService>();
            CartService cart = new CartService(mockCoupon.Object);
            double result    = cart.Total;
            result.Should().Be(0.0);
        }

        [Fact]
        public void AddItem_SingleItem_UpdatesTotal()
        {
            Mock<ICouponService> mockCoupon = new Mock<ICouponService>();
            CartService cart = new CartService(mockCoupon.Object);
            cart.AddItem("Laptop", 999.99);
            cart.Total.Should().Be(999.99);
        }

        [Fact]
        public void AddItem_MultipleItems_ReturnsCombinedTotal()
        {
            Mock<ICouponService> mockCoupon = new Mock<ICouponService>();
            CartService cart = new CartService(mockCoupon.Object);
            cart.AddItem("Laptop", 999.99);
            cart.AddItem("Muis",    29.99);
            cart.Total.Should().Be(1029.98);
        }

        [Fact]
        public void AddItem_WithQuantity_MultipliesPriceByQuantity()
        {
            Mock<ICouponService> mockCoupon = new Mock<ICouponService>();
            CartService cart = new CartService(mockCoupon.Object);
            cart.AddItem("Pen", 2.50, 4);
            cart.Total.Should().Be(10.0);
        }

        [Fact]
        public void RemoveItem_ExistingItem_UpdatesTotal()
        {
            Mock<ICouponService> mockCoupon = new Mock<ICouponService>();
            CartService cart = new CartService(mockCoupon.Object);
            cart.AddItem("Laptop", 999.99);
            cart.AddItem("Muis",    29.99);
            cart.RemoveItem("Muis");
            cart.Total.Should().Be(999.99);
        }

        [Fact]
        public void AddItem_WithNegativeQuantity_ThrowsArgumentException()
        {
            Mock<ICouponService> mockCoupon = new Mock<ICouponService>();
            CartService cart = new CartService(mockCoupon.Object);
            Action act = () => cart.AddItem("Laptop", 999.99, -1);
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void Clear_NonEmptyCart_ResetsTotal()
        {
            Mock<ICouponService> mockCoupon = new Mock<ICouponService>();
            CartService cart = new CartService(mockCoupon.Object);
            cart.AddItem("Laptop", 999.99);
            cart.Clear();
            cart.Total.Should().Be(0.0);
        }
    }
}
