using PowerBI_MCP.DTO;

namespace PowerBI_MCP.Service.IssueDataGenerator.Generators.MeasuresIssuesGenerator
{
    public class GroupMeasureByFolder : IIssueDataGenerator
    {
        public SingleIssueRuleData GetData(IssueContext issueContext)
        {
            var measures = issueContext.ModelDocumentation.Measures;
            var measureNotInFolder = measures.Where(x => x.Folder == null).Cast<object>().ToList();
            return new SingleIssueRuleData()
            {
                Issues = measureNotInFolder ?? [],
                MaxIssuable = measures.Count
            };
        }
    }
}