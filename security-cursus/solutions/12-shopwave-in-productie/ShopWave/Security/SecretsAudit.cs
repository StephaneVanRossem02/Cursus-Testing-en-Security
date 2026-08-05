namespace ShopWave.Security
{
    public class SecretsAudit
    {
        private readonly List<string> secretKeywords;

        public SecretsAudit()
        {
            secretKeywords = new List<string>
            {
                "password", "secret", "key", "token", "connectionstring"
            };
        }

        public bool IsHardcoded(string codeLine)
        {
            string lowerLine = codeLine.ToLowerInvariant();

            bool containsKeyword = secretKeywords
                .Any(keyword => lowerLine.Contains(keyword));

            bool containsStringLiteral = codeLine.Contains("\"");

            bool usesEnvironmentVariable = lowerLine.Contains("getenvironmentvariable");

            bool usesConfiguration = lowerLine.Contains("configuration[") ||
                                     lowerLine.Contains("configuration.get");

            bool isComment = lowerLine.TrimStart().StartsWith("//");

            return containsKeyword
                && containsStringLiteral
                && !usesEnvironmentVariable
                && !usesConfiguration
                && !isComment;
        }

        public List<string> AuditLines(List<string> codeLines)
        {
            return codeLines
                .Where(line => IsHardcoded(line))
                .ToList();
        }

        public void PrintAuditReport(List<string> codeLines)
        {
            List<string> hardcodedLines = AuditLines(codeLines);

            Console.WriteLine("=== Secrets Audit ===");

            if (hardcodedLines.Count == 0)
            {
                Console.WriteLine("\nGeen hardcoded secrets gevonden.");
            }
            else
            {
                Console.WriteLine($"\nMogelijke hardcoded secrets gevonden: {hardcodedLines.Count}");
                Console.WriteLine();

                for (int index = 0; index < codeLines.Count; index++)
                {
                    if (IsHardcoded(codeLines[index]))
                    {
                        Console.WriteLine($"  Regel {index + 1}: {codeLines[index].Trim()}");
                    }
                }

                Console.WriteLine("\nAanbeveling: vervang hardcoded waarden door " +
                                  "Environment.GetEnvironmentVariable(...).");
            }
        }
    }
}
