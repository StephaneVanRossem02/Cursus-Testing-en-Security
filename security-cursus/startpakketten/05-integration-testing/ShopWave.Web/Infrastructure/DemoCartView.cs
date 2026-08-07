namespace ShopWave.Web.Infrastructure
{
    // DEMO-INFRASTRUCTUUR, ALLEEN VOOR WEERGAVE. CartService houdt zijn artikelen privé
    // en geeft naar buiten enkel Total. Om het mandje op het scherm te kunnen tonen,
    // houdt deze klasse dezelfde toevoegingen bij. CartService blijft de bron van
    // waarheid voor het totaal; deze lijst dient enkel om regels te tonen.
    public class DemoCartView
    {
        private readonly Dictionary<string, DemoCartLine> lines;

        public DemoCartView()
        {
            lines = new Dictionary<string, DemoCartLine>();
        }

        public void Add(string name, double price, int quantity)
        {
            if (lines.ContainsKey(name))
            {
                lines[name].Quantity += quantity;
            }
            else
            {
                lines[name] = new DemoCartLine(name, price, quantity);
            }
        }

        public void Remove(string name)
        {
            lines.Remove(name);
        }

        public void Clear()
        {
            lines.Clear();
        }

        public List<DemoCartLine> GetLines()
        {
            return lines.Values.ToList();
        }
    }

    public class DemoCartLine
    {
        public string Name     { get; set; }
        public double Price    { get; set; }
        public int    Quantity { get; set; }

        public DemoCartLine(string name, double price, int quantity)
        {
            Name     = name;
            Price    = price;
            Quantity = quantity;
        }
    }
}
