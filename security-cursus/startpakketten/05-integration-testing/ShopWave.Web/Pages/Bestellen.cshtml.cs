using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShopWave;
using ShopWave.Web.Infrastructure;

namespace ShopWave.Web.Pages
{
    // Vanaf les 5 rekent CheckoutService het volledige winkelmandje af in plaats van
    // een los product. OrderConfirmationService maakt daarna de bevestigingscode.
    public class BestellenModel : PageModel
    {
        private readonly CartService              cartService;
        private readonly CheckoutService          checkoutService;
        private readonly OrderConfirmationService confirmationService;
        private readonly DemoCartView             cartView;

        public BestellenModel(
            CartService              cartService,
            CheckoutService          checkoutService,
            OrderConfirmationService confirmationService,
            DemoCartView             cartView)
        {
            this.cartService         = cartService;
            this.checkoutService     = checkoutService;
            this.confirmationService = confirmationService;
            this.cartView            = cartView;
        }

        public double Totaal { get; private set; } = 0.0;

        public List<DemoCartLine> Regels { get; private set; } = new List<DemoCartLine>();

        public string Melding { get; private set; } = string.Empty;

        public bool IsFout { get; private set; } = false;

        public string Bevestigingscode { get; private set; } = string.Empty;

        public void OnGet()
        {
            Laden();
        }

        public IActionResult OnPost()
        {
            Melding = checkoutService.Checkout();
            IsFout  = Melding != "Betaling geslaagd";

            if (!IsFout)
            {
                Bevestigingscode = confirmationService.GenerateConfirmationCode(1);

                // Na een geslaagde betaling is het mandje leeg.
                cartService.Clear();
                cartView.Clear();
            }

            Laden();

            return Page();
        }

        private void Laden()
        {
            Regels = cartView.GetLines();
            Totaal = cartService.Total;
        }
    }
}
