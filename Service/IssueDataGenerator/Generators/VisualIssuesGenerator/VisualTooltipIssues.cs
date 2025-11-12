using PowerBI_MCP.DTO;
using PowerBI_MCP.Handlers;

namespace PowerBI_MCP.Service.IssueDataGenerator.Generators.VisualIssuesGenerator
{
    public class VisualTooltipIssues : IIssueDataGenerator
    {
        public SingleIssueRuleData GetData(IssueContext issueContext)
        {
             
            return new() { Issues = issueContext.ReportDocumentation?.VisualList.Where(x => x.TooltipShow == false).Cast<object>().ToList() ?? [],
                MaxIssuable = issueContext.ReportDocumentation?.VisualList?.Count };

        }
    }
}