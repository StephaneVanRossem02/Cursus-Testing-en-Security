using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShopWave;
using ShopWave.Web.Infrastructure;

namespace ShopWave.Web.Pages
{
    // Toont de producten en laat zien wat DiscountCalculator uit les 1 met de prijs doet.
    // De pagina bevat zelf geen rekenlogica; die zit in de domeinklasse.
    public class ProductenModel : PageModel
    {
        private readonly DemoProductCatalog catalog;
        private readonly DiscountCalculator discountCalculator;
        private readonly IStockService      stockService;

        public ProductenModel(
            DemoProductCatalog catalog,
            DiscountCalculator discountCalculator,
            IStockService      stockService)
        {
            this.catalog            = catalog;
            this.discountCalculator = discountCalculator;
            this.stockService       = stockService;
        }

        [BindProperty(SupportsGet = true)]
        public int KortingPercent { get; set; } = 10;

        public List<DemoProduct> Producten { get; private set; } = new List<DemoProduct>();

        public string Foutmelding { get; private set; } = string.Empty;

        public void OnGet()
        {
            Producten = catalog.GetAll();

            if (KortingPercent < 0 || KortingPercent > 100)
            {
                Foutmelding    = "Kortingspercentage moet tussen 0 en 100 liggen.";
                KortingPercent = 0;
            }
        }

        public double PrijsNaKorting(DemoProduct product)
        {
            double result = discountCalculator.ApplyDiscount(product.Price, KortingPercent);

            return result;
        }

        public int Voorraad(DemoProduct product)
        {
            int result = 0;

            if (stockService is DemoStockService demoStock)
            {
                result = demoStock.GetQuantity(product.Id);
            }

            return result;
        }
    }
}
