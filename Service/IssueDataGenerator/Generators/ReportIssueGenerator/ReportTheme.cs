using PowerBI_MCP.DTO; 

namespace PowerBI_MCP.Service.IssueDataGenerator.Generators.ReportIssueGenerator
{
    public class ReportTheme : IIssueDataGenerator
    {
        public SingleIssueRuleData GetData(IssueContext issueContext)
        {
            List<object> issueData = [];
            if (issueContext.ReportDocumentation?.ReportUISettings.Theme == null)
                issueData.Add(new Dictionary<string, object>() { { "ReportName", issueContext.ReportDocumentation?.ReportUISettings.ReportName ?? "" }, { "Theme", "No custom theme available" } });
         
            return new() { Issues = issueData ?? [], MaxIssuable =1 };
        }
    }
}