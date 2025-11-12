using PowerBI_MCP.DTO;
using PowerBI_MCP.Handlers;

namespace PowerBI_MCP.Service.IssueDataGenerator.Generators.CopilotIssuesGenerator
{
    public class AvoidPageLevelFilter : IIssueDataGenerator
    {
        public SingleIssueRuleData GetData(IssueContext issueContext)
        {
             var issues = new List<object>();

            foreach (var page in issueContext.ReportDocumentation.PageSummary ?? [])
            {
                if (page.PageFilters != null && page.PageFilters.Count > 0)
                {
                    issues.Add(new Dictionary<string, object>
                    {
                        ["ReportId"] = page.ReportId ?? string.Empty,
                        ["ReportName"] = page.ReportName ?? string.Empty,
                        ["PageId"] = page.PageId ?? string.Empty,
                        ["PageName"] = page.PageName ?? string.Empty,

                    });
                }
            }

            return new() { Issues = issues ?? [], MaxIssuable = issueContext.ReportDocumentation.PageSummary?.Count };
        }
    }
}