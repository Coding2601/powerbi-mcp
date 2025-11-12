using PowerBI_MCP.DTO;
using PowerBI_MCP.Handlers;
using System.Text.RegularExpressions;

namespace PowerBI_MCP.Service.IssueDataGenerator.Generators.CopilotIssuesGenerator
{
    public class MeasureNameVsLogic : IIssueDataGenerator
    {
        private static List<string> ExtractColumnTokens(string dax)
        {
            var matches = Regex.Matches(dax, @"\[(.*?)\]");
            return matches.Cast<Match>()
                          .Select(m => m.Groups[1].Value)
                          .Where(s => !string.IsNullOrWhiteSpace(s))
                          .Distinct(StringComparer.OrdinalIgnoreCase)
                          .ToList();
        }
        public SingleIssueRuleData GetData(IssueContext issueContext)
        {
           var issues = new List<object>();

            foreach (var m in issueContext.ModelDocumentation?.Measures ?? [])
            {
                var dax = m.Expression ?? "";
                var name = m.MeasureName ?? "";
                var table = m.TableName ?? "";
                var referencedFields = ExtractColumnTokens(dax);
                bool hasAnyFieldInName = referencedFields.Any(field =>
                    name.IndexOf(field, StringComparison.OrdinalIgnoreCase) >= 0);

                if (!hasAnyFieldInName && referencedFields.Count > 0)
                {
                    issues.Add(new Dictionary<string, object>
                    {

                        ["TableName"] = table,
                        ["MeasureName"] = name,
                        ["Expression"] = dax,
                        ["ReferencedFields"] = string.Join(", ", referencedFields),
                    });

                }
            }
            return new() { Issues = issues ?? [], MaxIssuable = issueContext.ModelDocumentation?.Measures?.Count };
        }
    }
}