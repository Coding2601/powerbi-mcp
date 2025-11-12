using PowerBI_MCP.DTO;
using PowerBI_MCP.Handlers;
using System.Text.Json;

namespace PowerBI_MCP.Service.IssueDataGenerator.Generators.VisualAlignmentIssuesGenerator
{
    public class RightSideBarMargin : IVisualAlignmentIssueGenerator
    {
        private List<ReportDocumentation> associatedReportsUIDoc = new List<ReportDocumentation>();
        private string rightMargin = "20";        
        void IVisualAlignmentIssueGenerator.SetAssociatedReportsUIDoc(List<ReportDocumentation> reportUIDocs)
        {
            this.associatedReportsUIDoc = reportUIDocs;
        }
        void IVisualAlignmentIssueGenerator.SetVisualSpacing(string? spacing)
        {
            if (string.IsNullOrWhiteSpace(spacing))
            {
                return;
            }
            
            try
            {
                if (spacing == null)
                {
                    return;
                }
                
                var spacingObject = JsonSerializer.Deserialize<JsonElement>(spacing);
                
                if (spacingObject.TryGetProperty("right", out var rightValue))
                {
                    this.rightMargin = rightValue.ValueKind == JsonValueKind.String
                        ? rightValue.GetString() ?? this.rightMargin
                        : rightValue.ToString();
                }
            }
            catch (Exception ex)
            {
                GlobalHandler.WriteCrashLog($"Unexpected error in SetVisualSpacing with spacing '{spacing}': {ex.Message}");
            }
        }
        public SingleIssueRuleData GetData(IssueContext issueContext)
        {
            try
            {
                var reportDoc = issueContext.ReportDocumentation;
                if (reportDoc?.VisualList == null || reportDoc?.PageSummary == null)
                {
                    return new SingleIssueRuleData();
                }

                var marginIssues = new List<dynamic>();
                var visualList = reportDoc.VisualList;
                var allPages = reportDoc.PageSummary;

                double rightMarginThreshold = double.Parse(this.rightMargin);
                double marginTolerance = 100;

                var pageVisualDict = new Dictionary<string, List<VisualSummary>>();
                foreach (var visual in visualList)
                {
                    try
                    {
                        if (visual.PageId == null) continue;

                        if (pageVisualDict.ContainsKey(visual.PageId))
                            pageVisualDict[visual.PageId].Add(visual);
                        else
                            pageVisualDict[visual.PageId] = new List<VisualSummary> { visual };
                    }
                    catch (Exception ex)
                    {
                        GlobalHandler.WriteCrashLog(ex.ToString());
                    }
                }

                foreach (var page in pageVisualDict)
                {
                    try
                    {
                        var pageData = allPages.FirstOrDefault(x => x.PageId == page.Key);
                        string? reportName = pageData?.ReportName;
                        string? pageName = pageData?.PageName;
                        double pageWidth = pageData?.Width ?? 0;
                        double pageHeight = pageData?.Height ?? 0;
                        var visuals = page.Value.Where(v => v.IsHidden != true).ToList();

                        if (pageWidth == 0 || pageHeight == 0) continue;

                        if (visuals.Count == 1)
                        {
                            var v = visuals[0];
                            double vWidth = double.Parse(v.Width ?? "0");
                            double vHeight = double.Parse(v.Height ?? "0");
                            if (Math.Abs(vWidth - pageWidth) < 0.01 && Math.Abs(vHeight - pageHeight) < 0.01)
                                continue;
                        }

                        int visualIndex = 0;
                        foreach (var visual in visuals)
                        {
                            try
                            {
                                visualIndex++;
                                double x = double.Parse(visual.X ?? "0");
                                double y = double.Parse(visual.Y ?? "0");
                                double width = double.Parse(visual.Width ?? "0");
                                double height = double.Parse(visual.Height ?? "0");

                                double marginLeft = x;
                                double marginTop = y;
                                double marginRight = pageWidth - (x + width);
                                double marginBottom = pageHeight - (y + height);

                                bool isTouchingLeftBorder = Math.Abs(marginLeft) <= marginTolerance;
                                bool isTouchingTopBorder = Math.Abs(marginTop) <= marginTolerance;
                                bool isTouchingRightBorder = Math.Abs(marginRight) <= marginTolerance;
                                bool isTouchingBottomBorder = Math.Abs(marginBottom) <= marginTolerance;

                                bool isBorderAdjacent = isTouchingLeftBorder || isTouchingTopBorder || isTouchingRightBorder || isTouchingBottomBorder;

                                if (isBorderAdjacent)
                                {
                                    var problematicMargins = new List<string>();
                                    var borderViolations = new List<string>();

                                    if (marginRight <= marginTolerance && (marginRight < rightMarginThreshold || marginRight > rightMarginThreshold))
                                    {
                                        problematicMargins.Add($"Right: {Math.Abs(marginRight - rightMarginThreshold):0.00}px");
                                        borderViolations.Add("Right");
                                    }

                                    if (problematicMargins.Count != 0)
                                    {
                                        string borderPosition = string.Join(", ", borderViolations);
                                        string allSpacingDetails = $"Left: {marginLeft:0.00}px; Top: {marginTop:0.00}px; Right: {marginRight:0.00}px; Bottom: {marginBottom:0.00}px";

                                        marginIssues.Add(new Dictionary<string, object>
                                        {
                                            ["PageName"] = pageName ?? string.Empty,
                                            ["VisualId"] = visual.VisualId ?? string.Empty,
                                            ["visualInformation"] = $"{visualIndex}) Visual ID: {visual.VisualId}, Visual Type: {visual.VisualType}, Visual Title: {(visual.VisualTitle != null ? $"{visual.VisualTitle}" : "")}",
                                            ["spacingViolation"] = string.Join("; ", problematicMargins),
                                            ["actualSpacing"] = allSpacingDetails,
                                            ["error"] = $"Visual has border margin issues: {borderPosition}"
                                        });
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

                return new SingleIssueRuleData
                {
                    Issues = marginIssues.Cast<object>().ToList(),
                    MaxIssuable = visualList.Count
                };
            }
            catch (Exception ex)
            {
                GlobalHandler.WriteCrashLog(ex.ToString());
                return new SingleIssueRuleData();
            }
        }
    }
}