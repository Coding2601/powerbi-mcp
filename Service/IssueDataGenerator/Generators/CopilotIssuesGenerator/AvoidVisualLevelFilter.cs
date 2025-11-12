using PowerBI_MCP.DTO;
using PowerBI_MCP.Handlers;

namespace PowerBI_MCP.Service.IssueDataGenerator.Generators.CopilotIssuesGenerator
{
    public class AvoidVisualLevelFilter : IIssueDataGenerator
    {
        public SingleIssueRuleData GetData(IssueContext issueContext)
        {
            var issues = new List<object>();

            foreach (var visual in issueContext.ReportDocumentation?.VisualList ?? [])
            {
                if (visual.VisualFilters != null && visual.VisualFilters.Count > 0)
                {
                    issues.Add(new Dictionary<string, object>
                    {
                        ["ReportId"] = visual.ReportId ?? string.Empty,
                        ["ReportName"] = visual.ReportName ?? string.Empty,
                        ["PageId"] = visual.PageId ?? string.Empty,
                        ["PageName"] = visual.PageName ?? string.Empty,
                        ["VisualId"] = visual.VisualId ?? string.Empty,
                        ["VisualType"] = visual.VisualType ?? string.Empty,
                    });
                }
            }

            return new() { Issues = issues ?? [], MaxIssuable = issueContext.ReportDocumentation?.VisualList?.Count };
        }
    }
}