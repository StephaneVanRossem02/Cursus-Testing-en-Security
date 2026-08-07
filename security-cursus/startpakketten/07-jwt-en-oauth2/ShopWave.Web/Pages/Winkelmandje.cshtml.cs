using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShopWave;
using ShopWave.Web.Infrastructure;

namespace ShopWave.Web.Pages
{
    // Het winkelmandje uit les 3. CartService houdt de artikelen bij en rekent de
    // coupon door via CouponService. De pagina roept alleen die methoden aan.
    // DemoCartView houdt dezelfde regels bij om ze te kunnen tonen.
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

        [BindProperty]
        public string Couponcode { get; set; } = string.Empty;

        [BindProperty]
        public string ArtikelNaam { get; set; } = string.Empty;

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

        public IActionResult OnPostCoupon()
        {
            double voor = cartService.Total;

            cartService.ApplyCoupon(Couponcode);

            double na = cartService.Total;

            if (na < voor)
            {
                Melding = $"Coupon {Couponcode} toegepast.";
            }
            else
            {
                Melding = $"Coupon {Couponcode} is niet geldig of al gebruikt.";
                IsFout  = true;
            }

            Laden();

            return Page();
        }

        public IActionResult OnPostVerwijderen()
        {
            cartService.RemoveItem(ArtikelNaam);
            cartView.Remove(ArtikelNaam);
            Melding = $"{ArtikelNaam} verwijderd.";

            Laden();

            return Page();
        }

        public IActionResult OnPostLegen()
        {
            cartService.Clear();
            cartView.Clear();
            Melding = "Mandje geleegd.";

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
