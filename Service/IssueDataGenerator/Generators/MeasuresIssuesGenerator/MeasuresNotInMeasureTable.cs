using PowerBI_MCP.DTO;
using PowerBI_MCP.Handlers;

namespace PowerBI_MCP.Service.IssueDataGenerator.Generators.MeasuresIssuesGenerator
{
    public class MeasuresNotInMeasureTable : IIssueDataGenerator
    {
        public SingleIssueRuleData GetData(IssueContext issueContext)
        {
            var measures = issueContext.ModelDocumentation.Measures;
            var measuresNotInMeasureTable = ComplianceHandler.GetMeasureNotInMeasureTables(issueContext.ModelDocumentation ?? new());
            return new SingleIssueRuleData()
            {
                Issues = measuresNotInMeasureTable ?? [],
                MaxIssuable = measures.Count
            };
        }
    }
}