using PowerBI_MCP.DTO;
using PowerBI_MCP.Handlers;

namespace PowerBI_MCP.Service.IssueDataGenerator.Generators.PageIssueGenerator
{
    public class PageWithMoreThan10Static : IIssueDataGenerator
    {
        public SingleIssueRuleData GetData(IssueContext issueContext)
        {
            return new()
            {
                Issues = issueContext.ReportDocumentation?.PageSummary.Where(x => x.TotalStatic != null && int.Parse(x.TotalStatic) > 10).Cast<object>().ToList() ?? [],
                MaxIssuable = issueContext.ReportDocumentation?.PageSummary?.Count,
            };
        }
    }
}