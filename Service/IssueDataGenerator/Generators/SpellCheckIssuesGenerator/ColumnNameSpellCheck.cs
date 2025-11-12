using PowerBI_MCP.DTO;
using PowerBI_MCP.Handlers;

namespace PowerBI_MCP.Service.IssueDataGenerator.Generators.SpellCheckIssuesGenerator
{
    public class ColumnNameSpellCheck : IIssueDataGenerator
    {
        public SingleIssueRuleData GetData(IssueContext issueContext)
        {
            List<object> issueData = [];
            foreach (var col in issueContext.ModelDocumentation?.Columns ?? [])
            {
                try
                {
                    var text = col.ColumnName;
                    string? correctText = ComplianceHandler.GetCorrectSpelledString(text);
                    if (text != null && correctText != null && text != correctText)
                    {
                        object newSpellError = new Dictionary<string, object>()
                        {
                            { "TableName", col.TableName ?? "" },
                            { "ColumnName", col.ColumnName ?? "" },
                            { "Visible", col.Visible != null ? col.Visible : false },
                            { "IncorrectSpelling", text },
                            { "CorrectSpelling", correctText },
                        };
                        issueData.Add(newSpellError);
                    }
                }
                catch (Exception ex)
                {
                    GlobalHandler.WriteCrashLog(ex.ToString());
                }
            }

            return new() { Issues = issueData, MaxIssuable = issueContext.ModelDocumentation?.Columns?.Count };
        }
    }
}