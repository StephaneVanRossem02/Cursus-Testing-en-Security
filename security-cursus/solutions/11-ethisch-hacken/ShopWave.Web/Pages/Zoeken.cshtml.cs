using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShopWave.Web.Infrastructure;

namespace ShopWave.Web.Pages
{
    // Les 9: zoeken op bestellingen. De pagina valideert eerst de invoer en zoekt
    // daarna op de veilige manier. Ter vergelijking tonen we ook wat een naieve
    // zoekopdracht zou teruggeven.
    public class ZoekenModel : PageModel
    {
        private readonly DemoOrderDatabase database;

        public ZoekenModel(DemoOrderDatabase database)
        {
            this.database = database;
        }

        [BindProperty]
        public string Zoekterm { get; set; } = string.Empty;

        public List<string> VeiligResultaat { get; private set; } = new List<string>();

        public List<string> OnveiligResultaat { get; private set; } = new List<string>();

        public string Melding { get; private set; } = string.Empty;

        public bool IsFout { get; private set; } = false;

        public bool HeeftGezocht { get; private set; } = false;

        public int TotaalAantal { get; private set; } = 0;

        public void OnGet()
        {
            TotaalAantal = database.AantalOrders();
        }

        public IActionResult OnPost()
        {
            TotaalAantal = database.AantalOrders();

            if (string.IsNullOrWhiteSpace(Zoekterm))
            {
                Melding = "Zoekterm is verplicht.";
                IsFout  = true;
            }
            else if (Zoekterm.Length > 100)
            {
                Melding = "Zoekterm mag maximaal 100 tekens bevatten.";
                IsFout  = true;
            }
            else if (!Zoekterm.Contains("@"))
            {
                Melding = "Zoek op een e-mailadres, dus met een apenstaartje erin.";
                IsFout  = true;
            }
            else
            {
                VeiligResultaat   = database.ZoekOpEmail(Zoekterm);
                OnveiligResultaat = database.ZoekOnveilig(Zoekterm);
                HeeftGezocht      = true;

                if (VeiligResultaat.Count == 0)
                {
                    Melding = "Geen bestellingen gevonden.";
                }
                else
                {
                    Melding = $"{VeiligResultaat.Count} bestelling(en) gevonden.";
                }
            }

            return Page();
        }
    }
}
