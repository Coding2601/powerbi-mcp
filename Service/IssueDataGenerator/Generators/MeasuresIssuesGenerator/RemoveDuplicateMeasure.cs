using PowerBI_MCP.DTO;
using PowerBI_MCP.Handlers;

namespace PowerBI_MCP.Service.IssueDataGenerator.Generators.MeasuresIssuesGenerator
{
    public class RemoveDuplicateMeasure : IIssueDataGenerator
    {
        public SingleIssueRuleData GetData(IssueContext issueContext)
        {
            var measures = issueContext.ModelDocumentation.Measures;
            var duplicateMeasures = ComplianceHandler.GetDuplicateMeasures(issueContext.MeasureDetails ?? new());
            return new SingleIssueRuleData()
            {
                Issues = duplicateMeasures ?? [],
                MaxIssuable = measures.Count
            };
        }
    }
}