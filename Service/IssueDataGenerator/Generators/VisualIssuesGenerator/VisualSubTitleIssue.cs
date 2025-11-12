using PowerBI_MCP.DTO;
using PowerBI_MCP.Handlers;

namespace PowerBI_MCP.Service.IssueDataGenerator.Generators.VisualIssuesGenerator
{
    public class VisualSubTitleIssue : IIssueDataGenerator
    {
        public SingleIssueRuleData GetData(IssueContext issueContext)
        {
            var issues = new List<object>();

            foreach (var visual in issueContext.ReportDocumentation?.VisualList ?? [])
            {
                if (string.IsNullOrWhiteSpace(visual.VisualSubTitle))
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

            return new() { Issues = issues ?? [], MaxIssuable = issueContext.ReportDocumentation?.VisualList?.Count  };
        }
    }
}