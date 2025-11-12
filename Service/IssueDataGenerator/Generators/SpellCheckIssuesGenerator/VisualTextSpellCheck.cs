using PowerBI_MCP.DTO;
using PowerBI_MCP.Handlers;

namespace PowerBI_MCP.Service.IssueDataGenerator.Generators.SpellCheckIssuesGenerator
{
    public class VisualTextSpellCheck : IIssueDataGenerator
    {
        public SingleIssueRuleData GetData(IssueContext issueContext)
        {
            List<object> issueData = [];
            int totalIssuable = 0;
            foreach (var vis in issueContext.ReportDocumentation?.VisualList ?? [])
            {
                try
                {
                    var texts = vis.VisualTexts;
                    if (texts == null || texts.Count == 0)
                        continue;
                    totalIssuable += texts.Count;

                    foreach (var textModel in texts)
                    {
                        try
                        {
                            string? text = textModel.Text;
                            string? correctText = ComplianceHandler.GetCorrectSpelledString(text);
                            if (text != null && correctText != null && text != correctText)
                            {
                                object newSpellError = new Dictionary<string, object>()
                                {
                                    { "ReportId", vis.ReportId ?? "" ?? "" },
                                    { "ReportName", vis.ReportName ?? "" ?? "" },
                                    { "PageName", vis.PageName ?? "" ?? "" },
                                    { "VisualId", vis.VisualId ?? "" ?? "" },
                                    { "VisualTitle", vis.VisualTitle ?? "" ?? "" },
                                    { "VisualType", vis.VisualType ?? "" ?? "" },
                                    { "Visible", vis.IsHidden == false ? "Yes" : "No" },
                                    { "IncorrectTextType", textModel.Type ?? "" },
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