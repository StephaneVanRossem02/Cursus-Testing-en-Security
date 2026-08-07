namespace ShopWave
{
    public class Coupon
    {
        public string Code            { get; set; }
        public int    DiscountPercent { get; set; }
        public bool   IsUsed          { get; set; }

        public Coupon(string code, int discountPercent)
        {
            Code            = code;
            DiscountPercent = discountPercent;
            IsUsed          = false;
        }
    }
}
