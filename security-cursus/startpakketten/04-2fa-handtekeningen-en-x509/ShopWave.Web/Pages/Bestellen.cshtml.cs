using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShopWave;
using ShopWave.Web.Infrastructure;

namespace ShopWave.Web.Pages
{
    // Rekent een bestelling af via CheckoutService en plaatst ze via OrderService,
    // allebei uit les 1. De pagina valideert alleen de invoer en toont het resultaat.
    public class BestellenModel : PageModel
    {
        private readonly DemoProductCatalog catalog;
        private readonly CheckoutService    checkoutService;
        private readonly OrderService       orderService;

        public BestellenModel(
            DemoProductCatalog catalog,
            CheckoutService    checkoutService,
            OrderService       orderService)
        {
            this.catalog         = catalog;
            this.checkoutService = checkoutService;
            this.orderService    = orderService;
        }

        [BindProperty]
        public int ProductId { get; set; } = 1;

        [BindProperty]
        public int Aantal { get; set; } = 1;

        [BindProperty]
        public int KortingPercent { get; set; } = 0;

        public List<DemoProduct> Producten { get; private set; } = new List<DemoProduct>();

        public string Melding { get; private set; } = string.Empty;

        public bool IsFout { get; private set; } = false;

        public double Totaal { get; private set; } = 0.0;

        public bool HeeftResultaat { get; private set; } = false;

        public void OnGet()
        {
            Producten = catalog.GetAll();
        }

        public void OnPost()
        {
            Producten = catalog.GetAll();

            DemoProduct product = catalog.GetById(ProductId);

            if (product == null)
            {
                Melding = "Onbekend product.";
                IsFout  = true;
            }
            else if (Aantal < 1)
            {
                Melding = "Aantal moet minstens 1 zijn.";
                IsFout  = true;
            }
            else if (KortingPercent < 0 || KortingPercent > 100)
            {
                Melding = "Kortingspercentage moet tussen 0 en 100 liggen.";
                IsFout  = true;
            }
            else
            {
                Totaal = checkoutService.CalculateFinalTotal(product.Price, Aantal, KortingPercent);

                if (Totaal <= 0)
                {
                    Melding = "Het bedrag moet groter zijn dan nul.";
                    IsFout  = true;
                }
                else
                {
                    Melding        = orderService.PlaceOrder(ProductId, Aantal, Totaal);
                    IsFout         = Melding != "Bestelling bevestigd";
                    HeeftResultaat = true;
                }
            }
        }
    }
}
