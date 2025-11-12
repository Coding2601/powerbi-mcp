using PowerBI_MCP.DTO;
using PowerBI_MCP.Handlers;

namespace PowerBI_MCP.Service.IssueDataGenerator.Generators.SpellCheckIssuesGenerator
{
    public class TableNameSpellCheck : IIssueDataGenerator
    {
        public SingleIssueRuleData GetData(IssueContext issueContext)
        {
            List<object> issueData = [];
            foreach (var table in issueContext.ModelDocumentation?.Tables ?? [])
            {
                try
                {
                    var text = table.TableName;
                    string? correctText = ComplianceHandler.GetCorrectSpelledString(text);
                    if (text != null && correctText != null && text != correctText)
                    {
                        object newSpellError = new Dictionary<string, object>()
                        {
                            { "TableName", text },
                            { "Visible", table.Visible != null ? table.Visible : false },
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
            return new() { Issues = issueData ?? [], MaxIssuable = issueContext.ModelDocumentation?.Tables?.Count };
        }
    }
}