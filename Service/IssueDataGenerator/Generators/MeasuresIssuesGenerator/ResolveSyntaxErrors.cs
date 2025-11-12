using PowerBI_MCP.DTO;

namespace PowerBI_MCP.Service.IssueDataGenerator.Generators.MeasuresIssuesGenerator
{
    public class ResolveSyntaxErrors : IIssueDataGenerator
    {
        public SingleIssueRuleData GetData(IssueContext issueContext)
        {
            var measures = issueContext.ModelDocumentation.Measures;
            var measuresWithError = measures.Where(x => x.HasError != "No").Cast<object>().ToList();
            return new SingleIssueRuleData()
            {
                Issues = measuresWithError ?? [],
                MaxIssuable = measures.Count
            };
        }
    }
}