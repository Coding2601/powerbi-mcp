using PowerBI_MCP.DTO;
using PowerBI_MCP.Handlers;

namespace PowerBI_MCP.Service.IssueDataGenerator.Generators.VisualLoadingIssuesGenerator
{
    public class VisualLoadingIssue : IIssueDataGenerator
    {
        public SingleIssueRuleData GetData(IssueContext issueContext)
        {
            var issues = new List<object>();
            return new() { Issues =   [], MaxIssuable = issueContext.ReportDocumentation?.VisualList?.Count };
        }
    }
}