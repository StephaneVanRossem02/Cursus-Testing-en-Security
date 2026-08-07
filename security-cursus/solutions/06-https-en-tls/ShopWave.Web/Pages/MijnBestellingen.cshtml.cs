using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShopWave.Security;

namespace ShopWave.Web.Pages
{
    // Toont wat AES-encryptie uit les 2 doet met ordergegevens. OrderRepository slaat
    // alleen versleutelde tekst op; OrderEncryptor laat zien hoe die tekst eruitziet.
    public class MijnBestellingenModel : PageModel
    {
        private readonly OrderRepository orderRepository;
        private readonly OrderEncryptor  orderEncryptor;

        public MijnBestellingenModel(OrderRepository orderRepository, OrderEncryptor orderEncryptor)
        {
            this.orderRepository = orderRepository;
            this.orderEncryptor  = orderEncryptor;
        }

        [BindProperty]
        public string OrderId { get; set; } = "ORD-2024-00042";

        [BindProperty]
        public string OrderData { get; set; } = "alice@shopwave.be | Laptop | 999.99 EUR";

        public string Versleuteld { get; private set; } = string.Empty;

        public string Ontsleuteld { get; private set; } = string.Empty;

        public string Melding { get; private set; } = string.Empty;

        public bool IsFout { get; private set; } = false;

        public void OnGet()
        {
        }

        public void OnPost()
        {
            if (OrderId == string.Empty || OrderData == string.Empty)
            {
                Melding = "Ordernummer en ordergegevens zijn verplicht.";
                IsFout  = true;
            }
            else
            {
                orderRepository.SaveOrder(OrderId, OrderData);

                Versleuteld = orderEncryptor.EncryptOrderData(OrderData);
                Ontsleuteld = orderRepository.GetOrder(OrderId);
                Melding     = "Bestelling versleuteld opgeslagen.";
            }
        }
    }
}
