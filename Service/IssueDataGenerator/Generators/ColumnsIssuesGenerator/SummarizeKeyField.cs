using PowerBI_MCP.DTO;

namespace PowerBI_MCP.Service.IssueDataGenerator.Generators.ColumnsIssuesGenerator
{
    public class SummarizeKeyField : IIssueDataGenerator
    {
        public SingleIssueRuleData GetData(IssueContext issueContext)
        {
            var columns = issueContext.ModelDocumentation.Columns;
            var keyColSummarize = columns.Where(x => (x.IsKeyColumn=="Yes" || x.IsUniqueColumn == "Yes") &&  x.Summarization != "None").Cast<object>().ToList();
            return new SingleIssueRuleData()
            {
                Issues = keyColSummarize ?? [],
                MaxIssuable = columns.Count
            };
        }
    }
}