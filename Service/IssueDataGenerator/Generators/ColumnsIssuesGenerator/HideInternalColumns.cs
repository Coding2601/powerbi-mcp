using PowerBI_MCP.DTO;
using PowerBI_MCP.Handlers;

namespace PowerBI_MCP.Service.IssueDataGenerator.Generators.ColumnsIssuesGenerator
{
    public class HideInternalColumns : IIssueDataGenerator
    {
        public SingleIssueRuleData GetData(IssueContext issueContext)
        {
            var columns = issueContext.ModelDocumentation.Columns;
            var colToBeHidden = ComplianceHandler.GetColumnsToBeHidden(issueContext.ModelDocumentation ?? new());
            return new SingleIssueRuleData()
            {
                Issues = colToBeHidden ?? [],
                MaxIssuable = columns.Count
            };
        }
    }
}