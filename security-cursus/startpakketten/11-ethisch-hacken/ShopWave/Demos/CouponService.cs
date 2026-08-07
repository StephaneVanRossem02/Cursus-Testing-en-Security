namespace ShopWave
{
    public class CouponService : ICouponService
    {
        private readonly List<Coupon> coupons;

        public CouponService()
        {
            coupons = new List<Coupon>
            {
                new Coupon("ZOMER10",  10),
                new Coupon("WELKOM20", 20),
                new Coupon("TROUWE5",   5)
            };
        }

        public bool IsValid(string code)
        {
            Coupon coupon = coupons.Find(c => c.Code == code);
            return coupon != null && !coupon.IsUsed;
        }

        public int GetDiscount(string code)
        {
            Coupon coupon = coupons.Find(c => c.Code == code);
            int discount  = 0;

            if (coupon != null)
            {
                discount = coupon.DiscountPercent;
            }

            return discount;
        }

        public void MarkAsUsed(string code)
        {
            Coupon coupon = coupons.Find(c => c.Code == code);

            if (coupon != null)
            {
                coupon.IsUsed = true;
            }
        }
    }
}
