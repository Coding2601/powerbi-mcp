using PowerBI_MCP.DTO;
using PowerBI_MCP.Handlers;

namespace PowerBI_MCP.Service.IssueDataGenerator.Generators.SpellCheckIssuesGenerator
{
    public class MeasureNameSpellCheck : IIssueDataGenerator
    {
        public SingleIssueRuleData GetData(IssueContext issueContext)
        {
            List<object> issueData = [];
            foreach (var meas in issueContext.ModelDocumentation?.Measures ?? [])
            {
                try
                {
                    var text = meas.MeasureName;
                    string? correctText = ComplianceHandler.GetCorrectSpelledString(text);
                    if (text != null && correctText != null && text != correctText)
                    {
                        object newSpellError = new Dictionary<string, object>()
                        {
                            { "Origin", meas.Origin ?? "" },
                            { "TableName", meas.TableName ?? "" },
                            { "MeasureName", meas.MeasureName ?? "" },
                            { "Visible", meas.Visible != null ? meas.Visible : false },
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

            return new() { Issues = issueData ?? [], MaxIssuable = issueContext.ModelDocumentation?.Measures?.Count };
        }
    }
}