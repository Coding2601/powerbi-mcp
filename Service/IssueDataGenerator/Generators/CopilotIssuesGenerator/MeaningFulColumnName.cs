using PowerBI_MCP.DTO;
using PowerBI_MCP.Handlers;
using System.Text.RegularExpressions;

namespace PowerBI_MCP.Service.IssueDataGenerator.Generators.CopilotIssuesGenerator
{
    public class MeaningFulColumnName : IIssueDataGenerator
    {
        private bool LooksMeaningful(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            if (Regex.IsMatch(name, @"^(Table|Sheet|Column|Measure)\s*\d+$", RegexOptions.IgnoreCase))
                return false;

            if (name.Length < 3)
                return false;

            if (Regex.IsMatch(name, @"^(tbl_|calc_|temp_|copy|backup)", RegexOptions.IgnoreCase))
                return false;

            string? correctedWhole = ComplianceHandler.GetCorrectSpelledString(name);
            if (correctedWhole != null && !correctedWhole.Equals(name, StringComparison.Ordinal))
                return false;

            return true;
        }
        public SingleIssueRuleData GetData(IssueContext issueContext)
        {
            var issues = new List<object>();

            foreach (var col in issueContext.ModelDocumentation?.Columns ?? [])
            {
                if (!LooksMeaningful(col.ColumnName ?? ""))
                {
                    issues.Add(new Dictionary<string, object>
                    {
                        ["TableName"] = col.TableName ?? "",
                        ["ColumnName"] = col.ColumnName ?? "",

                    });
                }
            }

            return new() { Issues = issues ?? [], MaxIssuable = issueContext.ModelDocumentation?.Columns?.Count };
        }
    }
}