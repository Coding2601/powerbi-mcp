using PowerBI_MCP.DTO;

namespace PowerBI_MCP.Service.IssueDataGenerator.Generators.ColumnsIssuesGenerator
{
    public class AvoidCalculatedColumns : IIssueDataGenerator
    {
        public SingleIssueRuleData GetData(IssueContext issueContext)
        {
            var columns = issueContext.ModelDocumentation.Columns;
            var calcTables = columns.Where(x => x.ColumnType == "Calculated").Cast<object>().ToList();
            return new SingleIssueRuleData()
            {
                Issues = calcTables ?? [],
                MaxIssuable = columns.Count
            };
        }
    }
}