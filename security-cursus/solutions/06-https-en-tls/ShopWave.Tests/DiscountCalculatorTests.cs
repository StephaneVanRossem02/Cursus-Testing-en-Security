using FluentAssertions;
using ShopWave;

namespace ShopWave.Tests
{
    public class DiscountCalculatorTests
    {
        [Fact]
        public void ApplyDiscount_WithZeroPercent_ReturnsOriginalPrice()
        {
            DiscountCalculator calculator = new DiscountCalculator();
            double result = calculator.ApplyDiscount(100.0, 0);
            result.Should().Be(100.0);
        }

        [Fact]
        public void ApplyDiscount_With25Percent_ReturnsCorrectPrice()
        {
            DiscountCalculator calculator = new DiscountCalculator();
            double result = calculator.ApplyDiscount(80.0, 25);
            result.Should().Be(60.0);
        }

        [Fact]
        public void ApplyDiscount_With100Percent_ReturnsZero()
        {
            DiscountCalculator calculator = new DiscountCalculator();
            double result = calculator.ApplyDiscount(100.0, 100);
            result.Should().Be(0.0);
        }

        [Fact]
        public void ApplyDiscount_WithNegativePercent_ThrowsArgumentException()
        {
            DiscountCalculator calculator = new DiscountCalculator();
            Action act = () => calculator.ApplyDiscount(100.0, -1);
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void ApplyDiscount_WithPercentOver100_ThrowsArgumentException()
        {
            DiscountCalculator calculator = new DiscountCalculator();
            Action act = () => calculator.ApplyDiscount(100.0, 101);
            act.Should().Throw<ArgumentException>();
        }

        [Theory]
        [InlineData(100.0,   0, 100.0)]
        [InlineData(100.0,  10,  90.0)]
        [InlineData(100.0,  50,  50.0)]
        [InlineData(100.0,  75,  25.0)]
        [InlineData(100.0, 100,   0.0)]
        public void ApplyDiscount_WithValidPercents_ReturnsCorrectPrice(
            double originalPrice, int discountPercent, double expected)
        {
            DiscountCalculator calculator = new DiscountCalculator();
            double result = calculator.ApplyDiscount(originalPrice, discountPercent);
            result.Should().Be(expected);
        }
    }
}
