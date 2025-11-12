using PowerBI_MCP.DTO;

namespace PowerBI_MCP.Service.IssueDataGenerator.Generators.DaxIssuesGenerator
{
    public class NonRecommendedDAXFunc : IIssueDataGenerator
    {
        public SingleIssueRuleData GetData(IssueContext issueContext)
        {
            var measures = issueContext.ModelDocumentation.Measures;
            var measureWithNonReccomandedFunctions = measures.Where(m =>
                !string.IsNullOrWhiteSpace(m.Expression) && (m.Expression.Contains("HASONEVALUE(", StringComparison.OrdinalIgnoreCase) ||
                    m.Expression.Contains("IFERROR(", StringComparison.OrdinalIgnoreCase) ||
                    m.Expression.Contains("ISERROR(", StringComparison.OrdinalIgnoreCase))).Cast<object>().ToList();
            return new SingleIssueRuleData()
            {
                Issues = measureWithNonReccomandedFunctions ?? [],
                MaxIssuable = measures.Count
            };
        }
    }
}