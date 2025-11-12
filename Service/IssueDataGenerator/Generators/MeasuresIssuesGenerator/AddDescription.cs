using PowerBI_MCP.DTO;

namespace PowerBI_MCP.Service.IssueDataGenerator.Generators.MeasuresIssuesGenerator
{
    public class AddDescription : IIssueDataGenerator
    {
        public SingleIssueRuleData GetData(IssueContext issueContext)
        {
            var measures = issueContext.ModelDocumentation.Measures;
            var measuresWithoutDescription = measures.Where(x => x.Description == null || x.Description.Length == 0).Cast<object>().ToList();
            return new SingleIssueRuleData()
            {
                Issues = measuresWithoutDescription ?? [],
                MaxIssuable = measures.Count
            };
        }
    }
}