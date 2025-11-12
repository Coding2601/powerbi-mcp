
using PowerBI_MCP.DTO;

namespace PowerBI_MCP.Service.IssueDataGenerator.Generators.TablesIssuesGenerator
{

    public class RemoveAutoDateTimeTables : IIssueDataGenerator
    {
        public SingleIssueRuleData GetData(IssueContext issueContext)
        {
            var tables = issueContext.ModelDocumentation.Tables;
            var autoDateTimeTables = tables.Where(x => x.TableName != null && x.TableName.StartsWith("LocalDateTable")).Cast<object>().ToList();
            return new SingleIssueRuleData()
            {
                Issues =  autoDateTimeTables ?? [],
                MaxIssuable = tables.Count
            };
        }
    }
}