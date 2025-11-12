using PowerBI_MCP.DTO;
using PowerBI_MCP.Handlers;

namespace PowerBI_MCP.Service.IssueDataGenerator.Generators.VisualIssuesGenerator
{
    public class VisualAltTextIssues : IIssueDataGenerator
    {
        public SingleIssueRuleData GetData(IssueContext issueContext)
        {
            List<object> altTextIssuesData = [];
            var visuals = issueContext.ReportDocumentation?.VisualList ?? [];

            foreach (var visual in visuals)
            {
                try
                {
                    string? visualType = visual.OgVisualType?.ToLower();
                    string? altText = visual.AltText;
                    string? error = null;

                    // Case 1: Shapes or Images should NOT have ALT Text
                    if (visualType == "shape" || visualType == "image")
                    {
                        if (!string.IsNullOrEmpty(altText))
                            error = "Images and Shapes should not have ALT Text.";
                    }
                    else
                    {
                        // Case 2: Missing ALT Text
                        if (string.IsNullOrEmpty(altText))
                            error = "ALT Text is missing.";
                        // Case 3: ALT Text too large
                        else if (altText.Length > 150)
                            error = "Visuals should not have ALT Text larger than 150 characters.";
                    }

                    if (error != null)
                    {
                        Dictionary<string, object> errorDict = [];
                        errorDict["reportId"] = visual.ReportId ?? "";
                        errorDict["reportName"] = visual.ReportName ?? "";
                        errorDict["pageId"] = visual.PageId ?? "";
                        errorDict["pageName"] = visual.PageName ?? "";
                        errorDict["visualId"] = visual.VisualId ?? "";
                        errorDict["visualType"] = visual.VisualType ?? "";
                        errorDict["visualTitle"] = visual.VisualTitle ?? "";
                        errorDict.Add("error", error);
                        altTextIssuesData.Add(errorDict);
                    }
                }
                catch (Exception ex)
                {
                    GlobalHandler.WriteCrashLog(ex.ToString());
                }
            }
             
            return new() { Issues = altTextIssuesData ?? [], MaxIssuable = issueContext.ReportDocumentation?.VisualList?.Count };
        }
    }
}