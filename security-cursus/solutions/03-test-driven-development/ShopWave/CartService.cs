namespace ShopWave
{
    public class CartService
    {
        private readonly Dictionary<string, CartItem> items;
        private readonly ICouponService               couponService;
        private          double                        couponDiscount;

        public CartService(ICouponService couponService)
        {
            items          = new Dictionary<string, CartItem>();
            this.couponService  = couponService;
            couponDiscount = 0;
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

                return subtotal * (1 - couponDiscount / 100.0);
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
        }
    }
}
