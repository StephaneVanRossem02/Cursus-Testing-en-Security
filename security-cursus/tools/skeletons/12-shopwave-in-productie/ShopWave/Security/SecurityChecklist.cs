namespace ShopWave.Security
{
    // STARTCODE voor oefening 2 van les 12.
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
            // jouw code hier
        }

        public void AddItem(string category, string description)
        {
            // jouw code hier
        }

        public void SetStatus(string description, string status, string notes)
        {
            // jouw code hier
        }

        public List<ChecklistItem> GetByStatus(string status)
        {
            // jouw code hier

            return new List<ChecklistItem>();
        }

        public List<ChecklistItem> GetByCategory(string category)
        {
            // jouw code hier

            return new List<ChecklistItem>();
        }

        public bool IsFullyImplemented()
        {
            // jouw code hier

            return false;
        }

        public void PrintReport()
        {
            // jouw code hier
        }
    }
}
