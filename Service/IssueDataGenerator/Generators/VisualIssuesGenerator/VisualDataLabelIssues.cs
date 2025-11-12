using PowerBI_MCP.DTO;
using PowerBI_MCP.Handlers;

namespace PowerBI_MCP.Service.IssueDataGenerator.Generators.VisualIssuesGenerator
{
    public class VisualDataLabelIssues : IIssueDataGenerator
    {
        public SingleIssueRuleData GetData(IssueContext issueContext)
        {
           var visWithDataLabel = issueContext.ReportDocumentation?.VisualList.Where(x =>
            {
                if (x.OgVisualType?.ToLower() != "shape" && x.OgVisualType?.ToLower() != "image")
                {
                    if (x.DataLabelFormatting != null && x.DataLabelFormatting.TryGetValue("Visibility", out var visibility) && visibility.ToLower() == "false")
                        return true;
                }
                return false;
            });

            List<object> visWithDataLabels = [];
            foreach (var visual in visWithDataLabel ?? [])
            {
                try
                {
                    Dictionary<string, object> errorDict = [];
                    errorDict["reportId"] = visual.ReportId ?? "";
                    errorDict["reportName"] = visual.ReportName ?? "";
                    errorDict["pageId"] = visual.PageId ?? "";
                    errorDict["pageName"] = visual.PageName ?? "";
                    errorDict["visualId"] = visual.VisualId ?? "";
                    errorDict["visualType"] = visual.VisualType ?? "";
                    errorDict["visualTitle"] = visual.VisualTitle ?? "";
                    errorDict.Add("error", "Data label is disabled.");
                    visWithDataLabels.Add(errorDict);
                }
                catch (Exception ex)
                {
                    GlobalHandler.WriteCrashLog(ex.ToString());
                }
            }
            return new() { Issues = visWithDataLabels, MaxIssuable = issueContext.ReportDocumentation?.VisualList?.Count };
        }
    }
}