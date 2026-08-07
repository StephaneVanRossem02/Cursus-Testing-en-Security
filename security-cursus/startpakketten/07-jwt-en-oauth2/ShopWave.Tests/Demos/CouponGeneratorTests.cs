using FluentAssertions;
using ShopWave;

namespace ShopWave.Tests
{
    // Demo uit de theorie: de callback-techniek met CouponGenerator.
    public class CouponGeneratorTests
    {
        [Fact]
        public void GenerateCode_ProducesValidCode()
        {
            string capturedCode = string.Empty;

            CouponGenerator generator = new CouponGenerator(
                onCodeGenerated: code => { capturedCode = code; });

            string result = generator.GenerateCode();

            capturedCode.Should().NotBeNullOrEmpty();
            capturedCode.Should().Be(result);
        }
    }
}
