using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShopWave;
using ShopWave.Web.Infrastructure;

namespace ShopWave.Web.Pages
{
    // Het winkelmandje in zijn eenvoudigste vorm: artikels toevoegen en het totaal
    // tonen. Meer kan deze pagina niet, want meer heeft CartService nog niet.
    // Zodra jij ApplyCoupon, RemoveItem en Clear geschreven hebt, staat de volledige
    // pagina in de oplossing van deze les.
    public class WinkelmandjeModel : PageModel
    {
        private readonly DemoProductCatalog catalog;
        private readonly CartService        cartService;
        private readonly DemoCartView       cartView;

        public WinkelmandjeModel(
            DemoProductCatalog catalog,
            CartService        cartService,
            DemoCartView       cartView)
        {
            this.catalog     = catalog;
            this.cartService = cartService;
            this.cartView    = cartView;
        }

        [BindProperty]
        public int ProductId { get; set; } = 1;

        [BindProperty]
        public int Aantal { get; set; } = 1;

        public List<DemoProduct>  Producten { get; private set; } = new List<DemoProduct>();

        public List<DemoCartLine> Regels { get; private set; } = new List<DemoCartLine>();

        public double Totaal { get; private set; } = 0.0;

        public string Melding { get; private set; } = string.Empty;

        public bool IsFout { get; private set; } = false;

        public void OnGet()
        {
            Laden();
        }

        public IActionResult OnPostToevoegen()
        {
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
            else
            {
                cartService.AddItem(product.Name, product.Price, Aantal);
                cartView.Add(product.Name, product.Price, Aantal);
                Melding = $"{product.Name} toegevoegd aan je mandje.";
            }

            Laden();

            return Page();
        }

        private void Laden()
        {
            Producten = catalog.GetAll();
            Regels    = cartView.GetLines();
            Totaal    = cartService.Total;
        }
    }
}
