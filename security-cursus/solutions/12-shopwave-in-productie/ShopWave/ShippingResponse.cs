namespace ShopWave
{
    public class ShippingResponse
    {
        public string Bestemming { get; set; } = string.Empty;
        public double Gewicht    { get; set; }
        public double Tarief     { get; set; }
        public string Vervoerder { get; set; } = string.Empty;
    }
}
