using PowerBI_MCP.DTO;

namespace PowerBI_MCP.Service.IssueDataGenerator.Generators.DaxIssuesGenerator
{
    public class AvoidBlankToStaticConversion : IIssueDataGenerator
    {
        public SingleIssueRuleData GetData(IssueContext issueContext)
        {
            var measures = issueContext.ModelDocumentation.Measures;
            var meausreConvertingBlankToStatic = measures.Where(m =>
                !string.IsNullOrWhiteSpace(m.Expression) &&
                m.Expression.Contains("IF(ISBLANK(", StringComparison.OrdinalIgnoreCase)).Cast<object>().ToList();
            return new SingleIssueRuleData()
            {
                Issues = meausreConvertingBlankToStatic ?? [],
                MaxIssuable = measures.Count
            };
        }
    }
}