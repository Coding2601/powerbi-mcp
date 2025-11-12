using PowerBI_MCP.DTO;

namespace PowerBI_MCP.Service.IssueDataGenerator.Generators.DaxIssuesGenerator
{
    public class AvoidIsBlankCheck : IIssueDataGenerator
    {
        public SingleIssueRuleData GetData(IssueContext issueContext)
        {
            var measures = issueContext.ModelDocumentation.Measures;
            var measuresContainIsBlankCheck = measures.Where(m =>
                !string.IsNullOrWhiteSpace(m.Expression) &&
                m.Expression.Contains("ISBLANK", StringComparison.OrdinalIgnoreCase)).Cast<object>().ToList();
            return new SingleIssueRuleData()
            {
                Issues = measuresContainIsBlankCheck ?? [],
                MaxIssuable = measures.Count
            };
        }
    }
}