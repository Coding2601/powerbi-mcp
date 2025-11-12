using PowerBI_MCP.DTO;

namespace PowerBI_MCP.Service.IssueDataGenerator.Generators.TablesIssuesGenerator
{
    public class AvoidCalcTables : IIssueDataGenerator
    {
        public SingleIssueRuleData GetData(IssueContext issueContext)
        {
            var tables = issueContext.ModelDocumentation.Tables;
            var calcTables = tables.Where(x => x.IsCalculatedTable == true).Cast<object>().ToList();
            return new SingleIssueRuleData()
            {
                Issues = calcTables ?? [],
                MaxIssuable = tables.Count
            };
        }
    }
}