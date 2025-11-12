using PowerBI_MCP.DTO;
using PowerBI_MCP.Handlers;

namespace PowerBI_MCP.Service.IssueDataGenerator.Generators.SpellCheckIssuesGenerator
{
    public class PageNameSpellCheck : IIssueDataGenerator
    {
        public SingleIssueRuleData GetData(IssueContext issueContext)
        {
            List<object> issueData = [];
            foreach (var page in issueContext.ReportDocumentation?.PageSummary ?? [])
            {
                try
                {
                    var text = page.PageName;
                    string? correctText = ComplianceHandler.GetCorrectSpelledString(text);
                    if (text != null && correctText != null && text != correctText)
                    {
                        object newSpellError = new Dictionary<string, object>()
                        {
                            { "ReportId", page.ReportId ?? "" },
                            { "ReportName", page.ReportName ?? "" },
                            { "PageId", page.PageId ?? "" },
                            { "PageName", text },
                            { "Visible", page.IsHidden == false ? "Yes" : "No" },
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
            return new() { Issues = issueData ?? [], MaxIssuable = issueContext.ReportDocumentation?.PageSummary?.Count };
        }
    }
}