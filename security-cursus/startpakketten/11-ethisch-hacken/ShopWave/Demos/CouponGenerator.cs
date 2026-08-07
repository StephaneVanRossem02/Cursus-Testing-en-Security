namespace ShopWave
{
    // Demo uit de theorie (vereenvoudigd voorbeeld van de callback-techniek).
    public class CouponGenerator
    {
        private readonly Action<string> onCodeGenerated;

        // Constructor voor productie
        public CouponGenerator()
        {
            onCodeGenerated = null;
        }

        // Constructor voor integration testing
        public CouponGenerator(Action<string> onCodeGenerated)
        {
            this.onCodeGenerated = onCodeGenerated;
        }

        public string GenerateCode()
        {
            string code = Guid.NewGuid().ToString("N")[..8].ToUpper();

            if (onCodeGenerated != null)
            {
                onCodeGenerated(code);
            }

            return code;
        }
    }
}
