using PowerBI_MCP.DTO;
using PowerBI_MCP.Handlers;

namespace PowerBI_MCP.Service.IssueDataGenerator.Generators.TablesIssuesGenerator
{
    public class HideFactTables : IIssueDataGenerator
    {
        public SingleIssueRuleData GetData(IssueContext issueContext)
        {
            var tables = issueContext.ModelDocumentation.Tables; 
            return new SingleIssueRuleData()
            {
                Issues = ComplianceHandler.GetFactTables(issueContext.ModelDocumentation ?? new(), issueContext.MeasureDetails?? new()),
                MaxIssuable = tables.Count
            };
        }
    }
}