using FluentAssertions;
using ShopWave;

namespace ShopWave.Tests
{
    // Demo uit de theorie: CouponService volledig via TDD opgebouwd (concrete klasse).
    public class CouponServiceTests
    {
        [Fact]
        public void IsValid_WithValidCouponCode_ReturnsTrue()
        {
            // Arrange
            CouponService service = new CouponService();

            // Act
            bool result = service.IsValid("ZOMER10");

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void IsValid_WithUnknownCouponCode_ReturnsFalse()
        {
            // Arrange
            CouponService service = new CouponService();

            // Act
            bool result = service.IsValid("BESTAANIET");

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void GetDiscount_WithValidCouponCode_ReturnsCorrectDiscount()
        {
            // Arrange
            CouponService service = new CouponService();

            // Act
            int discount = service.GetDiscount("ZOMER10");

            // Assert
            discount.Should().Be(10);
        }

        [Fact]
        public void IsValid_AfterCouponIsUsed_ReturnsFalse()
        {
            // Arrange
            CouponService service = new CouponService();
            service.MarkAsUsed("ZOMER10");

            // Act
            bool result = service.IsValid("ZOMER10");

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void IsValid_BeforeCouponIsUsed_ReturnsTrue()
        {
            // Arrange
            CouponService service = new CouponService();

            // Act
            bool result = service.IsValid("ZOMER10");

            // Assert
            result.Should().BeTrue();
        }
    }
}
