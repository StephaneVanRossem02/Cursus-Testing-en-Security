namespace ShopWave.Security
{
    public class ChecklistItem
    {
        public string Category    { get; set; } = "";
        public string Description { get; set; } = "";
        public string Status      { get; set; } = "NotImplemented";
        public string Notes       { get; set; } = "";
    }

    public class SecurityChecklist
    {
        private readonly List<ChecklistItem> items;

        public SecurityChecklist()
        {
            items = new List<ChecklistItem>();
        }

        public void AddItem(string category, string description)
        {
            items.Add(new ChecklistItem
            {
                Category    = category,
                Description = description
            });
        }

        public void SetStatus(string description, string status, string notes)
        {
            ChecklistItem? item = items.FirstOrDefault(
                i => i.Description == description);

            if (item != null)
            {
                item.Status = status;
                item.Notes  = notes;
            }
        }

        public List<ChecklistItem> GetByStatus(string status)
        {
            return items
                .Where(i => i.Status == status)
                .ToList();
        }

        public List<ChecklistItem> GetByCategory(string category)
        {
            return items
                .Where(i => i.Category == category)
                .ToList();
        }

        public bool IsFullyImplemented()
        {
            return items.All(i => i.Status == "Implemented");
        }

        public void PrintReport()
        {
            Console.WriteLine("=== ShopWave Security Checklist ===");

            List<string> categories = items
                .Select(i => i.Category)
                .Distinct()
                .ToList();

            foreach (string category in categories)
            {
                Console.WriteLine($"\n[{category}]");

                List<ChecklistItem> categoryItems = GetByCategory(category);

                foreach (ChecklistItem item in categoryItems)
                {
                    string indicator = item.Status switch
                    {
                        "Implemented"    => "[OK]",
                        "Partial"        => "[!!]",
                        _                => "[ ]"
                    };

                    Console.WriteLine($"  {indicator} {item.Description}");

                    if (!string.IsNullOrWhiteSpace(item.Notes))
                    {
                        Console.WriteLine($"       {item.Notes}");
                    }
                }
            }

            int implemented    = GetByStatus("Implemented").Count;
            int partial        = GetByStatus("Partial").Count;
            int notImplemented = GetByStatus("NotImplemented").Count;
            int total          = items.Count;

            Console.WriteLine($"\nGeimplementeerd: {implemented}/{total}   " +
                              $"Gedeeltelijk: {partial}/{total}   " +
                              $"Niet geimplementeerd: {notImplemented}/{total}");
        }
    }
}
