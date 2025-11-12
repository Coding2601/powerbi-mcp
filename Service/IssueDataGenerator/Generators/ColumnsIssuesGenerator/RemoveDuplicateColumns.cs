using PowerBI_MCP.DTO;
using PowerBI_MCP.Handlers;
using Microsoft.AnalysisServices.Tabular;

namespace PowerBI_MCP.Service.IssueDataGenerator.Generators.ColumnsIssuesGenerator
{
    public class RemoveDuplicateColumns : IIssueDataGenerator
    {
        public SingleIssueRuleData GetData(IssueContext issueContext)
        {
            var columns = issueContext.ModelDocumentation.Columns;
            var duplicateCols = ComplianceHandler.GetDuplicateCalculatedColumns(issueContext.ModelDocumentation ?? new(), issueContext.CalculatedColumns?? new());
            return new SingleIssueRuleData()
            {
                Issues = duplicateCols ?? [],
                MaxIssuable = columns.Count
            };
        }
    }
}