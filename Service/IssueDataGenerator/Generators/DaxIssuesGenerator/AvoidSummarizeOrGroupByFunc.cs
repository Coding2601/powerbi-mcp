using PowerBI_MCP.DTO;

namespace PowerBI_MCP.Service.IssueDataGenerator.Generators.DaxIssuesGenerator
{
    public class AvoidSummarizeOrGroupByFunc : IIssueDataGenerator
    {
        public SingleIssueRuleData GetData(IssueContext issueContext)
        {
            var measures = issueContext.ModelDocumentation.Measures;
            var measuresWithSummarizeOrGroupBy = measures.Where(m =>
                !string.IsNullOrWhiteSpace(m.Expression) &&
                (m.Expression.Contains("SUMMARIZE(", StringComparison.OrdinalIgnoreCase) ||
                m.Expression.Contains("GROUPBY(", StringComparison.OrdinalIgnoreCase))).Cast<object>().ToList();
            return new SingleIssueRuleData()
            {
                Issues = measuresWithSummarizeOrGroupBy ?? [],
                MaxIssuable = measures.Count
            };
        }
    }
}