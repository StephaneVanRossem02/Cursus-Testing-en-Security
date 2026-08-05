namespace ShopWave.Security
{
    public class CiaPillar
    {
        public string Name { get; }
        private readonly List<string> examples;

        public CiaPillar(string name)
        {
            Name      = name;
            examples = new List<string>();
        }

        public void AddExample(string example)
        {
            examples.Add(example);
        }

        public IReadOnlyList<string> Examples => examples;
    }

    public class CiaPijlerAnalyse
    {
        public CiaPillar Confidentiality { get; }
        public CiaPillar Integrity       { get; }
        public CiaPillar Availability    { get; }

        public CiaPijlerAnalyse()
        {
            Confidentiality = new CiaPillar("Confidentiality");
            Integrity       = new CiaPillar("Integrity");
            Availability    = new CiaPillar("Availability");
        }

        public void PrintAnalysis()
        {
            Console.WriteLine("=== CIA-pijleranalyse ShopWave ===");

            PrintPillar(Confidentiality);
            PrintPillar(Integrity);
            PrintPillar(Availability);
        }

        private void PrintPillar(CiaPillar pillar)
        {
            Console.WriteLine($"\n{pillar.Name} ({pillar.Examples.Count} voorbeelden)");

            foreach (string example in pillar.Examples)
            {
                Console.WriteLine($"  - {example}");
            }
        }
    }
}
