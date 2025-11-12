using PowerBI_MCP.DTO;
using PowerBI_MCP.Handlers;

namespace PowerBI_MCP.Service.IssueDataGenerator.Generators.DaxIssuesGenerator
{
    public class UseVariables : IIssueDataGenerator
    {
        public SingleIssueRuleData GetData(IssueContext issueContext)
        {
            var measures = issueContext.ModelDocumentation.Measures;
            var measureSouldUseVariables = ComplianceHandler.GetVariableSuggestion(issueContext.MeasureDetails ?? new());
            return new SingleIssueRuleData()
            {
                Issues = measureSouldUseVariables ?? [],
                MaxIssuable = measures.Count
            };
        }
    }
}