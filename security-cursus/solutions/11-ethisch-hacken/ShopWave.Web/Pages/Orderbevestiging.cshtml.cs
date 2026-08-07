using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShopWave.Security;

namespace ShopWave.Web.Pages
{
    // Toont wat een digitale handtekening uit les 4 doet. OrderSigner ondertekent de
    // orderdata; wijzig je daarna ook maar een teken, dan klopt de handtekening niet meer.
    public class OrderbevestigingModel : PageModel
    {
        private readonly OrderSigner signer;

        public OrderbevestigingModel(OrderSigner signer)
        {
            this.signer = signer;
        }

        [BindProperty]
        public string Orderdata { get; set; } = "ORD-2024-00042 | alice@shopwave.be | Laptop | 999.99 EUR";

        [BindProperty]
        public string Handtekening { get; set; } = string.Empty;

        [BindProperty]
        public string TeControleren { get; set; } = string.Empty;

        public string Melding { get; private set; } = string.Empty;

        public bool IsFout { get; private set; } = false;

        public bool HeeftHandtekening { get; private set; } = false;

        public bool HeeftControle { get; private set; } = false;

        public bool IsGeldig { get; private set; } = false;

        public void OnGet()
        {
        }

        public IActionResult OnPostOndertekenen()
        {
            if (Orderdata == string.Empty)
            {
                Melding = "Orderdata is verplicht.";
                IsFout  = true;
            }
            else
            {
                Handtekening      = signer.Sign(Orderdata);
                TeControleren     = Orderdata;
                HeeftHandtekening = true;
                Melding           = "Orderbevestiging ondertekend.";
            }

            return Page();
        }

        public IActionResult OnPostControleren()
        {
            HeeftHandtekening = true;
            HeeftControle     = true;

            // Een handtekening is Base64. Komt er iets anders binnen, dan is dat geen
            // geldige handtekening. Dat vangen we hier af, want OrderSigner verwacht
            // geldige Base64 en zou anders een FormatException gooien.
            byte[] buffer = new byte[Handtekening.Length];
            bool   isBase64 = Convert.TryFromBase64String(Handtekening, buffer, out int geschreven);

            if (!isBase64)
            {
                Melding = "Handtekening ongeldig: dit is geen geldige handtekening.";
                IsFout  = true;
            }
            else
            {
                IsGeldig = signer.Verify(TeControleren, Handtekening);

                if (IsGeldig)
                {
                    Melding = "Handtekening geldig: de orderdata is ongewijzigd.";
                }
                else
                {
                    Melding = "Handtekening ongeldig: de orderdata is aangepast.";
                    IsFout  = true;
                }
            }

            return Page();
        }
    }
}
