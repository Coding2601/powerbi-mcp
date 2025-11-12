using PowerBI_MCP.DTO;
using PowerBI_MCP.Handlers;
using System.Text.Json;

namespace PowerBI_MCP.Service.IssueDataGenerator.Generators.VisualAlignmentIssuesGenerator
{
    public class HorizontalAlignment : IVisualAlignmentIssueGenerator
    {
        private List<ReportDocumentation> associatedReportsUIDoc = new List<ReportDocumentation>();
        private string gap = "5";
        void IVisualAlignmentIssueGenerator.SetAssociatedReportsUIDoc(List<ReportDocumentation> reportUIDocs)
        {
            this.associatedReportsUIDoc = reportUIDocs;
        }
        void IVisualAlignmentIssueGenerator.SetVisualSpacing(string? gap)
        {
            if (string.IsNullOrWhiteSpace(gap))
            {
                return;
            }
            try
            {
                if (gap == null)
                {
                    return;
                }

                var spacingObject = JsonSerializer.Deserialize<JsonElement>(gap);
                if (spacingObject.TryGetProperty("gap", out var gapValue))
                {
                    this.gap = gapValue.ValueKind == JsonValueKind.String
                        ? gapValue.GetString() ?? this.gap
                        : gapValue.ToString();
                }
            }
            catch (Exception ex)
            {
                GlobalHandler.WriteCrashLog(ex.ToString());
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

                var alignmentIssues = new List<object>();
                var visualList = reportDoc.VisualList;
                var allPages = reportDoc.PageSummary;
                int threshold = int.Parse(this.gap);

                var pageVisualDict = new Dictionary<string, List<VisualSummary>>();
                foreach (var visual in visualList)
                {
                    try
                    {
                        if (visual.PageId == null) continue;

                        if (pageVisualDict.ContainsKey(visual.PageId))
                            pageVisualDict[visual.PageId].Add(visual);
                        else
                            pageVisualDict[visual.PageId] = [visual];
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
                        var visuals = page.Value;

                        if (visuals.Count < 2) continue;

                        var comparedPairs = new HashSet<string>();
                        int hidden1 = 0;
                        int i = 0;

                        foreach (var vis1 in visuals)
                        {
                            try
                            {
                                i++;
                                if (vis1.IsHidden == true)
                                {
                                    hidden1++;
                                    continue;
                                }

                                int hidden2 = 0;
                                int j = 0;
                                foreach (var vis2 in visuals)
                                {
                                    try
                                    {
                                        j++;
                                        if (vis2 == null || vis1 == vis2 || vis2.IsHidden == true)
                                        {
                                            if (vis2?.IsHidden == true) hidden2++;
                                            continue;
                                        }

                                        string pairId = string.Compare(vis1.VisualId, vis2.VisualId, StringComparison.Ordinal) < 0
                                            ? $"{vis1.VisualId}_{vis2.VisualId}"
                                            : $"{vis2.VisualId}_{vis1.VisualId}";

                                        if (comparedPairs.Contains(pairId)) continue;
                                        comparedPairs.Add(pairId);

                                        double xDist = Math.Abs(double.Parse(vis1.X ?? "0") - double.Parse(vis2.X ?? "0"));
                                        double xRightDist = Math.Abs(
                                            double.Parse(vis1.X ?? "0") + double.Parse(vis1.Width ?? "0") -
                                            double.Parse(vis2.X ?? "0") - double.Parse(vis2.Width ?? "0")
                                        );

                                        bool hasLeftIssue = xDist <= threshold && xDist > 1;
                                        bool hasRightIssue = xRightDist <= threshold && xRightDist > 1;

                                        if (hasLeftIssue || hasRightIssue)
                                        {
                                            string spacingViolation = "";
                                            string error = "";

                                            if (hasLeftIssue && hasRightIssue)
                                            {
                                                spacingViolation = $"Left: {xDist:0.00}px; Right: {xRightDist:0.00}px";
                                                error = $"Horizontal alignment issue in: Left, Right";
                                            }
                                            else if (hasLeftIssue)
                                            {
                                                spacingViolation = $"Left: {xDist:0.00}px";
                                                error = $"Horizontal alignment issue in: Left";
                                            }
                                            else
                                            {
                                                spacingViolation = $"Right: {xRightDist:0.00}px";
                                                error = $"Horizontal alignment issue in: Right";
                                            }

                                            alignmentIssues.Add(new Dictionary<string, object>
                                            {
                                                // ReportName = reportName,
                                                ["PageName"] = pageName ?? string.Empty,
                                                // PageId = page.Key,
                                                // VisualId = vis1.VisualId,
                                                ["visual1"] = $"{i - hidden1}) Visual ID: {vis1.VisualId}, Visual Type: {vis1.VisualType}, Visual Title: {(vis1.VisualTitle != null ? $"{vis1.VisualTitle}" : "")}",
                                                ["visual2"] = $"{j - hidden2}) Visual ID: {vis2.VisualId}, Visual Type: {vis2.VisualType}, Visual Title: {(vis2.VisualTitle != null ? $"{vis2.VisualTitle}" : "")}",
                                                ["spacingViolation"] = spacingViolation,
                                                ["actualSpacing"] = $"X1: {vis1.X}, Width1: {vis1.Width}, X2: {vis2.X}, Width2: {vis2.Width}",
                                                ["error"] = error
                                            });
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
                    }
                    catch (Exception ex)
                    {
                        GlobalHandler.WriteCrashLog(ex.ToString());
                    }
                }

                return new SingleIssueRuleData
                {
                    Issues = alignmentIssues,
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