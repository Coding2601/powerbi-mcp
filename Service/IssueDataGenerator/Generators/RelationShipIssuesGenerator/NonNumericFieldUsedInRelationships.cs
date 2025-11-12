using PowerBI_MCP.DTO;
using PowerBI_MCP.Handlers;
using System.Text.RegularExpressions;

namespace PowerBI_MCP.Service.IssueDataGenerator.Generators.RelationShipIssuesGenerator
{
    public class NonNumericFieldUsedInRelationships : IIssueDataGenerator
    {
        public SingleIssueRuleData GetData(IssueContext issueContext)
        {
            var regexNumeric = new Regex(@"^(Decimal|Double|Int64|Int32)$", RegexOptions.IgnoreCase);
            var rels = issueContext.ModelDocumentation.Relationships;
            var manyToManyRels = rels.Where(x =>
            {
                ColumnSummary? fromCol = null, toCol = null;
                foreach (var col in issueContext.ModelDocumentation.Columns ?? [])
                {
                    try
                    {
                        if (col.TableName + "." + col.ColumnName == x.FromColumn?.TableName + "." + x.FromColumn?.ColumnName)
                            fromCol = col;
                        if (col.TableName + "." + col.ColumnName == x.ToColumn?.TableName + "." + x.ToColumn?.ColumnName)
                            toCol = col;
                    }
                    catch (Exception ex)
                    {
                        GlobalHandler.WriteCrashLog(ex.ToString());
                    }
                }
                if (fromCol != null && !regexNumeric.IsMatch(fromCol.DataType ?? ""))
                    return true;
                if (toCol != null && !regexNumeric.IsMatch(toCol.DataType ?? ""))
                    return true;
                return false;
            }
                ).Cast<object>().ToList();
            return new SingleIssueRuleData()
            {
                Issues = manyToManyRels ?? [],
                MaxIssuable = rels.Count
            };
        }
    }
}