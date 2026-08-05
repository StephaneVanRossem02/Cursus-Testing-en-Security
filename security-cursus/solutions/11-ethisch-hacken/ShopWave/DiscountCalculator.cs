namespace ShopWave
{
    public class DiscountCalculator
    {
        // Uit les 1. Behouden zodat de bestaande DiscountCalculatorTests van les 1 blijven werken.
        public double ApplyDiscount(double originalPrice, int discountPercent)
        {
            double result;

            if (discountPercent < 0 || discountPercent > 100)
            {
                throw new ArgumentException(
                    "Kortingspercentage moet tussen 0 en 100 liggen.",
                    nameof(discountPercent));
            }

            result = originalPrice * (1 - discountPercent / 100.0);

            return result;
        }

        // Toegevoegd in les 5 (oplossing 2). CartService gebruikt deze methode.
        public double Apply(double amount, int discountPercent)
        {
            if (discountPercent < 0 || discountPercent > 100)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(discountPercent),
                    "Kortingspercentage moet tussen 0 en 100 liggen.");
            }

            return amount * (1 - discountPercent / 100.0);
        }
    }
}
