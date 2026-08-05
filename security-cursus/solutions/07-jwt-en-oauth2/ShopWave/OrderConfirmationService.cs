namespace ShopWave
{
    public class OrderConfirmationService
    {
        private readonly Action<string> onConfirmationCodeGenerated;

        public OrderConfirmationService()
        {
            onConfirmationCodeGenerated = null;
        }

        public OrderConfirmationService(Action<string> onConfirmationCodeGenerated)
        {
            this.onConfirmationCodeGenerated = onConfirmationCodeGenerated;
        }

        public string GenerateConfirmationCode(int orderId)
        {
            string code = $"ORD-{orderId:D6}-{Guid.NewGuid().ToString("N")[..4].ToUpper()}";

            if (onConfirmationCodeGenerated != null)
            {
                onConfirmationCodeGenerated(code);
            }

            return code;
        }

        public bool ValidateCode(string code)
        {
            return code != null && code.StartsWith("ORD-") && code.Length >= 12;
        }
    }
}
