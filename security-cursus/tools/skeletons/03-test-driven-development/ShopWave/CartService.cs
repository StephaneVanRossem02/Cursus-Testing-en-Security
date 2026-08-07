namespace ShopWave
{
    // STARTCODE voor oefening 1 en 2 van les 3.
    //
    // Alleen AddItem en Total staan erin, en ze doen nog niets. Die twee heeft de
    // webshop nodig om te compileren. De rest van de klasse ontwerp je zelf via
    // Red-Green-Refactor: eerst de testlijst, dan een falende test, dan de code.
    //
    // De constructor met ICouponService staat er al in omdat de webshop de klasse
    // via Dependency Injection aanmaakt. In oefening 2 vul je ApplyCoupon aan.
    public class CartService
    {
        private readonly Dictionary<string, CartItem> items;
        private readonly ICouponService               couponService;

        public CartService(ICouponService couponService)
        {
            items              = new Dictionary<string, CartItem>();
            this.couponService = couponService;
        }

        public double Total
        {
            get
            {
                // jouw code hier

                return 0;
            }
        }

        public void AddItem(string name, double price, int quantity = 1)
        {
            // jouw code hier
        }
    }
}
