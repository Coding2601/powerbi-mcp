using PowerBI_MCP.DTO;

namespace PowerBI_MCP.Service.IssueDataGenerator.Generators.ColumnsIssuesGenerator
{
    public class DatetimeColumnWithMinOrSecPrecision : IIssueDataGenerator
    {
        public SingleIssueRuleData GetData(IssueContext issueContext)
        {
            var columns = issueContext.ModelDocumentation.Columns;
            // Filter columns with DataType is "date" (case-insensitive) and it's format string contains minutes or seconds level precision
            var dateTimeColumnsWithMinOrSecPricision = columns
                .Where(x => x.DataType != null && x.DataType.ToLower().Contains("date") && (x.Format?.Contains("mm") == true || x.Format?.Contains("nn") == true || x.Format?.Contains("ss") == true))
                .ToList();

            return new() { Issues = dateTimeColumnsWithMinOrSecPricision.Cast<object>().ToList(), MaxIssuable = columns.Count };
        }
    }
}