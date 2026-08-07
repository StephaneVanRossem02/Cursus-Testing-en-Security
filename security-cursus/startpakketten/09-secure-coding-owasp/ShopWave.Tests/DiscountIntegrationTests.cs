using FluentAssertions;
using ShopWave;

namespace ShopWave.Tests
{
    public class DiscountIntegrationTests
    {
        [Fact]
        public void Total_WithValidCouponZOMER10_AppliesTenPercent()
        {
            // Arrange
            CouponService       couponService       = new CouponService();
            DiscountCalculator  discountCalculator  = new DiscountCalculator();
            CartService         cartService         = new CartService(couponService, discountCalculator);

            cartService.AddItem("Laptop", 100.0);
            cartService.ApplyCoupon("ZOMER10");

            // Act
            double result = cartService.Total;

            // Assert
            result.Should().BeApproximately(90.0, precision: 0.01);
        }

        [Fact]
        public void Total_WithInvalidCoupon_ReturnsFullAmount()
        {
            // Arrange
            CouponService       couponService       = new CouponService();
            DiscountCalculator  discountCalculator  = new DiscountCalculator();
            CartService         cartService         = new CartService(couponService, discountCalculator);

            cartService.AddItem("Laptop", 100.0);
            cartService.ApplyCoupon("ONGELDIG");

            // Act
            double result = cartService.Total;

            // Assert
            result.Should().BeApproximately(100.0, precision: 0.01);
        }

        [Fact]
        public void Total_WithCouponWELKOM20_AppliesTwentyPercent()
        {
            // Arrange
            CouponService       couponService       = new CouponService();
            DiscountCalculator  discountCalculator  = new DiscountCalculator();
            CartService         cartService         = new CartService(couponService, discountCalculator);

            cartService.AddItem("Artikel", 50.0);
            cartService.ApplyCoupon("WELKOM20");

            // Act
            double result = cartService.Total;

            // Assert
            result.Should().BeApproximately(40.0, precision: 0.01);
        }

        [Fact]
        public void Total_TwoItemsWithCouponTROUWE5_AppliesFivePercent()
        {
            // Arrange
            CouponService       couponService       = new CouponService();
            DiscountCalculator  discountCalculator  = new DiscountCalculator();
            CartService         cartService         = new CartService(couponService, discountCalculator);

            cartService.AddItem("ArtikelA", 100.0);
            cartService.AddItem("ArtikelB", 100.0);
            cartService.ApplyCoupon("TROUWE5");

            // Act
            double result = cartService.Total;

            // Assert
            result.Should().BeApproximately(190.0, precision: 0.01);
        }
    }
}
