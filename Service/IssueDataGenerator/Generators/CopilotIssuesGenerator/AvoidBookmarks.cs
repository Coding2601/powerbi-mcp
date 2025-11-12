using PowerBI_MCP.DTO;
using PowerBI_MCP.Handlers;

namespace PowerBI_MCP.Service.IssueDataGenerator.Generators.CopilotIssuesGenerator
{
    public class AvoidBookmarks : IIssueDataGenerator
    {
        public SingleIssueRuleData GetData(IssueContext issueContext)
        {
            var issues = new List<object>();

            foreach (var bm in issueContext.ReportDocumentation?.BookmarkList ?? Enumerable.Empty<BookmarkModel>())
            {
                issues.Add(new Dictionary<string, object>
                {
                    ["ReportId"] = bm.ReportId ?? string.Empty,
                    ["ReportName"] = bm.ReportName ?? string.Empty,
                    ["PageId"] = bm.PageId ?? string.Empty,
                    ["PageName"] = bm.PageName ?? string.Empty,
                    ["BookmarkId"] = bm.Id ?? string.Empty,
                    ["BookmarkName"] = bm.Name ?? string.Empty
                });
            }
            return new() { Issues = issues ?? [], MaxIssuable = issues?.Count };
        }
    }
}