using PowerBI_MCP.DTO;
using PowerBI_MCP.Handlers;

namespace PowerBI_MCP.Service.IssueDataGenerator.Generators.VisualIssuesGenerator
{
    public class VisualTabOrderIssues : IIssueDataGenerator
    {
        public SingleIssueRuleData GetData(IssueContext issueContext)
        {
            var issueList = new List<object>();
            var visuals = issueContext.ReportDocumentation?.VisualList ?? [];
            var pageWiseVisuals = new Dictionary<string, List<VisualSummary>>();

            // First, handle visuals without tab order
            var visualsWithoutTabOrder = visuals.Where(x => x.OgVisualType?.ToLower() is not ("shape" or "image" or "textbox") && x.TabOrder == null).ToList() ?? [];

            foreach (var visual in visualsWithoutTabOrder)
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
                    errorDict["error"] = "Taborder is missing.";
                    issueList.Add(errorDict);
                }
                catch (Exception ex)
                {
                    GlobalHandler.WriteCrashLog(ex.ToString());
                }
            }

            // Group visuals by page
            foreach (var vis in visuals ?? [])
            {
                try
                {
                    if (vis.PageId == null)
                        continue;
                    if (!pageWiseVisuals.ContainsKey(vis.PageId))
                        pageWiseVisuals[vis.PageId] = [];
                    pageWiseVisuals[vis.PageId].Add(vis);
                }
                catch (Exception ex)
                {
                    GlobalHandler.WriteCrashLog(ex.ToString());
                }
            }

            // Process each page
            foreach (var page in pageWiseVisuals)
            {
                try
                {
                    // Separate shapes/images from other visuals
                    var shapesAndImages = page.Value
                        .Where(v => (v.OgVisualType?.ToLower() is "shape" or "image" or "textbox")
                                && !string.IsNullOrEmpty(v.TabOrder))
                        .ToList();

                    var otherVisuals = page.Value
                        .Where(v => v.OgVisualType?.ToLower() is not ("shape" or "image" or "textbox"))
                        .ToList();

                    // Sort other visuals by Z-index (desc), Y-axis, X-axis
                    var sortedVisuals = otherVisuals
                        .OrderBy(v => float.Parse(v.Y ?? "0"))
                        .ThenBy(v => float.Parse(v.X ?? "0"))
                        .ThenByDescending(v => float.Parse(v.Z ?? "0"))
                        .ToList();

                    // Check regular visuals tab order
                    var orderedByTabOrder = otherVisuals
                        .Where(v => !string.IsNullOrEmpty(v.TabOrder))
                        .OrderBy(v => int.Parse(v.TabOrder))
                        .ToList();

                    // Check if tab order follows visual flow (left-to-right, top-to-bottom)
                    for (int i = 1; i < orderedByTabOrder.Count; i++)
                    {
                        try
                        {
                            var prevVisual = orderedByTabOrder[i - 1];
                            var currentVisual = orderedByTabOrder[i];

                            float prevY = float.Parse(prevVisual.Y ?? "0");
                            float prevX = float.Parse(prevVisual.X ?? "0");
                            float currentY = float.Parse(currentVisual.Y ?? "0");
                            float currentX = float.Parse(currentVisual.X ?? "0");

                            // Calculate vertical and horizontal positions
                            bool isBelow = currentY > prevY + 5; // 5px tolerance for same row
                            bool isSameRow = Math.Abs(currentY - prevY) <= 5;
                            bool isRight = currentX > prevX;

                            // If current visual is above or to the left of previous visual in tab order
                            if ((!isSameRow && currentY < prevY) || (isSameRow && currentX < prevX))
                            {
                                Dictionary<string, object> errorDict = [];
                                errorDict["reportId"] = currentVisual.ReportId ?? "";
                                errorDict["reportName"] = currentVisual.ReportName ?? "";
                                errorDict["pageId"] = currentVisual.PageId ?? "";
                                errorDict["pageName"] = currentVisual.PageName ?? "";
                                errorDict["visualId"] = currentVisual.VisualId ?? "";
                                errorDict["visualType"] = currentVisual.VisualType ?? "";
                                errorDict["visualTitle"] = currentVisual.VisualTitle ?? "";
                                errorDict["error"] = $"Tab order {i + 1} is before tab order {i} but appears after it in the visual flow";
                                issueList.Add(errorDict);
                            }
                        }
                        catch (Exception ex)
                        {
                            GlobalHandler.WriteCrashLog(ex.ToString());
                        }
                    }

                    // Check shapes and images - they should either have no tab order or be after all regular visuals
                    int maxRegularOrder = sortedVisuals.Count > 0 ?
                        int.Parse(sortedVisuals.Last().TabOrder ?? "0") : 0;

                    foreach (var visual in shapesAndImages)
                    {
                        try
                        {
                            if (!string.IsNullOrEmpty(visual.TabOrder))
                            {
                                int currentOrder = int.Parse(visual.TabOrder);
                                if (currentOrder <= maxRegularOrder)
                                {
                                    Dictionary<string, object> errorDict = [];
                                    errorDict["reportId"] = visual.ReportId ?? "";
                                    errorDict["reportName"] = visual.ReportName ?? "";
                                    errorDict["pageId"] = visual.PageId ?? "";
                                    errorDict["pageName"] = visual.PageName ?? "";
                                    errorDict["visualId"] = visual.VisualId ?? "";
                                    errorDict["visualType"] = visual.VisualType ?? "";
                                    errorDict["visualTitle"] = visual.VisualTitle ?? "";
                                    errorDict["suggestedTabOrder"] = maxRegularOrder + 1;
                                    errorDict["error"] = visual.OgVisualType?.ToLower() == "textbox"
                                        ? $"Textbox tab order should be greater than {maxRegularOrder} for better accessibility"
                                        : $"{visual.OgVisualType} tab order should be greater than {maxRegularOrder} or not set at all";
                                    issueList.Add(errorDict);
                                }
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

            return new() { Issues = issueList ?? [], MaxIssuable = issueContext.ReportDocumentation?.VisualList?.Count };
        }
    }
}