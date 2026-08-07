namespace ShopWave
{
    public class CartService
    {
        private readonly Dictionary<string, CartItem> items;
        private readonly ICouponService               couponService;
        private readonly DiscountCalculator           discountCalculator;
        private          int                           couponDiscount;

        public CartService(ICouponService couponService, DiscountCalculator discountCalculator)
        {
            items              = new Dictionary<string, CartItem>();
            this.couponService      = couponService;
            this.discountCalculator = discountCalculator;
            couponDiscount     = 0;
        }

        public double Total
        {
            get
            {
                double subtotal = 0;

                foreach (CartItem item in items.Values)
                {
                    subtotal += item.Price * item.Quantity;
                }

                return couponDiscount > 0
                    ? discountCalculator.Apply(subtotal, couponDiscount)
                    : subtotal;
            }
        }

        public void AddItem(string name, double price, int quantity = 1)
        {
            if (quantity < 0)
            {
                throw new ArgumentException("Aantal mag niet negatief zijn.", nameof(quantity));
            }

            if (items.ContainsKey(name))
            {
                items[name].Quantity += quantity;
            }
            else
            {
                items[name] = new CartItem(name, price, quantity);
            }
        }

        public void ApplyCoupon(string code)
        {
            if (couponService.IsValid(code))
            {
                couponDiscount = couponService.GetDiscount(code);
                couponService.MarkAsUsed(code);
            }
        }

        public void RemoveItem(string name)
        {
            if (items.ContainsKey(name))
            {
                items.Remove(name);
            }
        }

        public void Clear()
        {
            items.Clear();
            couponDiscount = 0;
        }
    }
}
