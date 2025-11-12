using PowerBI_MCP.DTO;
using PowerBI_MCP.Handlers;

namespace PowerBI_MCP.Service.IssueDataGenerator.Generators.DaxIssuesGenerator
{
    public class AvoidSameDaxExpression : IIssueDataGenerator
    {
        public SingleIssueRuleData GetData(IssueContext issueContext)
        {
            var measures = issueContext.ModelDocumentation.Measures;
            var measureWithRepeatedExpression = ComplianceHandler.GetSeparateExpressions(issueContext.MeasureDetails ?? new());
            return new SingleIssueRuleData()
            {
                Issues = measureWithRepeatedExpression ?? [],
                MaxIssuable = measures.Count
            };
        }
    }
}