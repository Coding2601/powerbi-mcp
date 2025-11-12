using PowerBI_MCP.DTO;
using PowerBI_MCP.Handlers;

namespace PowerBI_MCP.Service.IssueDataGenerator.Generators.CopilotIssuesGenerator
{
    public class MissingDimensionHierarchies : IIssueDataGenerator
    {
        public SingleIssueRuleData GetData(IssueContext issueContext)
        {
            var issues = new List<object>();
            bool IsTableHidden(TableSummary t)
            {
                var vis = t.Visible?.Trim().ToLowerInvariant();
                return string.IsNullOrEmpty(vis)
                       || vis == "no"
                       || vis == "false";
            }
            var oneSide = new HashSet<string>(
                (issueContext.ModelDocumentation?.Relationships?
                    .Where(r =>
                           (r.RelationshipCardinality?.Equals("OneToMany", StringComparison.OrdinalIgnoreCase) ?? false) ||
                           ((r.FromCardinality?.Equals("One", StringComparison.OrdinalIgnoreCase) ?? false) &&
                            (r.ToCardinality?.Equals("Many", StringComparison.OrdinalIgnoreCase) ?? false)))
                    .Select(r => r.FromColumn?.ColumnName?.Split('.')[0])
                    .Where(t => !string.IsNullOrWhiteSpace(t))
                    .Cast<string>() ?? Enumerable.Empty<string>()),
                StringComparer.OrdinalIgnoreCase);

            /* 2️⃣  Scan every table */
            foreach (var tbl in issueContext.ModelDocumentation?.Tables ?? [])
            {
                string tName = tbl.TableName ?? "";
                string lowerName = tName.ToLowerInvariant();

                bool isDimension =
                    oneSide.Contains(tName)
                    || lowerName.Contains("dim") || lowerName.Contains("calendar")
                    || lowerName.Contains("date")
                    || ((IsTableHidden(tbl) && (tbl.Columns ?? 0) == 1)
                        || (tbl.SourceType?.Equals("autodatetime", StringComparison.OrdinalIgnoreCase) ?? false)
                        || (tbl.SourceDetails?.Contains("AutoDateTime", StringComparison.OrdinalIgnoreCase) ?? false))
                    || (tbl.SourceType?.Equals("timeseries", StringComparison.OrdinalIgnoreCase) ?? false);

                if (!isDimension) continue;
                if (tbl.Hierarchies != null && tbl.Hierarchies > 0) continue;

                issues.Add(new Dictionary<string, object>
                {

                    ["TableName"] = tName,
                });
            }

            return new() { Issues = issues ?? [], MaxIssuable = issueContext.ModelDocumentation?.Tables?.Count };
        }
    }
}