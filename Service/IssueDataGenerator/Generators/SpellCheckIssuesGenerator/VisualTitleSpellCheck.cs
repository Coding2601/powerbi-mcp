using PowerBI_MCP.DTO;
using PowerBI_MCP.Handlers;

namespace PowerBI_MCP.Service.IssueDataGenerator.Generators.SpellCheckIssuesGenerator
{
    public class VisualTitleSpellCheck : IIssueDataGenerator
    {
        public SingleIssueRuleData GetData(IssueContext issueContext)
        {
            List<object> issueData = [];
            foreach (var vis in issueContext.ReportDocumentation?.VisualList?? [])
            {
                try
                {
                    var text = vis.VisualTitle;
                    string? correctText = ComplianceHandler.GetCorrectSpelledString(text);
                    if (text != null && correctText != null && text != correctText)
                    {
                        object newSpellError = new Dictionary<string, object>()
                        {
                            { "ReportId", vis.ReportId ?? "" ?? "" },
                            { "ReportName", vis.ReportName ?? "" ?? "" },
                            { "PageName", vis.PageName ?? "" ?? "" },
                            { "VisualTitle", vis.VisualTitle ?? "" ?? "" },
                            { "VisualId", vis.VisualId ?? "" ?? "" },
                            { "VisualType", vis.VisualType ?? "" ?? "" },
                            { "Visible", vis.IsHidden == false ? "Yes" : "No" },
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
            return new() { Issues = issueData ?? [], MaxIssuable = issueContext.ReportDocumentation?.VisualList?.Count };
        }
    }
}