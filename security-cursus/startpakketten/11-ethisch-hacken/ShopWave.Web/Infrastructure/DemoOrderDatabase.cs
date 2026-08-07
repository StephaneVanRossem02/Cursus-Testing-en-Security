namespace ShopWave.Web.Infrastructure
{
    // DEMO-INFRASTRUCTUUR. Dezelfde gesimuleerde ordertabel als in ShopWave.Api bij
    // les 9. Een lijst met regels in het formaat email|product|bedrag, zodat de
    // zoekpagina iets heeft om in te zoeken.
    public class DemoOrderDatabase
    {
        private readonly List<string> orders;

        public DemoOrderDatabase()
        {
            orders = new List<string>
            {
                "alice@shopwave.be|Laptop|999.99",
                "bob@shopwave.be|Muis|29.99",
                "alice@shopwave.be|Toetsenbord|79.99",
                "admin@shopwave.be|Server|4999.99"
            };
        }

        // VEILIG: vergelijkt alleen het e-mailveld, exact. De zoekterm kan de opbouw
        // van de zoekopdracht niet beinvloeden.
        public List<string> ZoekOpEmail(string email)
        {
            List<string> result = orders
                .Where(order => order.StartsWith(email + "|", StringComparison.OrdinalIgnoreCase))
                .ToList();

            return result;
        }

        // KWETSBAAR, ALLEEN TER VERGELIJKING. Zoekt of de term ergens in de regel
        // voorkomt. Zo werkt een query die de invoer aan elkaar plakt: de invoer bepaalt
        // mee wat er teruggegeven wordt.
        public List<string> ZoekOnveilig(string term)
        {
            List<string> result = orders
                .Where(order => order.Contains(term))
                .ToList();

            return result;
        }

        public int AantalOrders()
        {
            return orders.Count;
        }
    }
}
