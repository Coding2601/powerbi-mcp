using PowerBI_MCP.DTO;
using PowerBI_MCP.Handlers;

namespace PowerBI_MCP.Service.IssueDataGenerator.Generators.VisualIssuesGenerator
{
    public class VisualTitleIssues : IIssueDataGenerator
    {
        public SingleIssueRuleData GetData(IssueContext issueContext)
        {
            var issues = new List<object>();
            var visualList = issueContext.ReportDocumentation?.VisualList ?? [];

            foreach (var visual in visualList)
            {
                try
                {
                    var visualType = visual.OgVisualType?.ToLower() ?? string.Empty;
                    var hasTitle = !string.IsNullOrEmpty(visual.VisualTitle);
                    var isShapeOrImage = visualType == "shape" || visualType == "image";
                    var isCard = visualType.Contains("card");
                    var isText = visualType.Contains("text");

                    // Case 1: Shape/Image with title (should not have title)
                    if (isShapeOrImage && hasTitle)
                    {
                        Dictionary<string, object> errorDict = [];
                        errorDict["reportId"] = visual.ReportId ?? "";
                        errorDict["reportName"] = visual.ReportName ?? "";
                        errorDict["pageId"] = visual.PageId ?? "";
                        errorDict["pageName"] = visual.PageName ?? "";
                        errorDict["visualId"] = visual.VisualId ?? "";
                        errorDict["visualType"] = visual.VisualType ?? "";
                        errorDict["visualTitle"] = visual.VisualTitle ?? "";
                        errorDict["error"] = "Shape or image should not have title.";
                        issues.Add(errorDict);
                    }
                    // Case 2: Other visual types that should have title but don't
                    else if (!isShapeOrImage && !isCard && !isText && !hasTitle)
                    {
                        Dictionary<string, object> errorDict = [];
                        errorDict["reportId"] = visual.ReportId ?? "";
                        errorDict["reportName"] = visual.ReportName ?? "";
                        errorDict["pageId"] = visual.PageId ?? "";
                        errorDict["pageName"] = visual.PageName ?? "";
                        errorDict["visualId"] = visual.VisualId ?? "";
                        errorDict["visualType"] = visual.VisualType ?? "";
                        errorDict["visualTitle"] = visual.VisualTitle ?? "";
                        errorDict["error"] = "Title is missing.";
                        issues.Add(errorDict);
                    }
                }
                catch (Exception ex)
                {
                   GlobalHandler.WriteCrashLog(ex.ToString());
                }
            }

            return new() { Issues = issues ?? [], MaxIssuable = issueContext.ReportDocumentation?.VisualList?.Count };
        }
    }
}