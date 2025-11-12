using PowerBI_MCP.DTO;
using PowerBI_MCP.Handlers;
using System.Text.Json;

namespace PowerBI_MCP.Service.IssueDataGenerator.Generators.VisualAlignmentIssuesGenerator
{
    public class HorizontalVisualSpacing : IVisualAlignmentIssueGenerator
    {
        private List<ReportDocumentation> associatedReportsUIDoc = new List<ReportDocumentation>();
        private string spacing = "10";
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
                if (spacingObject.TryGetProperty("spacing", out var spacingValue))
                {
                    this.spacing = spacingValue.ValueKind == JsonValueKind.String
                        ? spacingValue.GetString() ?? this.spacing
                        : spacingValue.ToString();
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

                var spacingIssues = new List<dynamic>();
                var visualList = reportDoc.VisualList;
                var allPages = reportDoc.PageSummary;
                double targetSpacing = double.Parse(this.spacing);

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
                        var visuals = page.Value.Where(v => v.IsHidden != true).ToList();

                        if (visuals.Count < 2) continue;

                        var comparedPairs = new HashSet<string>();

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

                                var nearestVisuals = new Dictionary<string, (double distance, string visualId)>
                                {
                                    ["left"] = (-1, ""),
                                    ["right"] = (-1, ""),
                                };

                                foreach (var otherVisual in visuals)
                                {
                                    if (otherVisual == visual || otherVisual.IsHidden == true) continue;

                                    string pairId = string.Compare(visual.VisualId, otherVisual.VisualId, StringComparison.Ordinal) < 0
                                        ? $"{visual.VisualId}_{otherVisual.VisualId}"
                                        : $"{otherVisual.VisualId}_{visual.VisualId}";

                                    if (comparedPairs.Contains(pairId)) continue;
                                    comparedPairs.Add(pairId);

                                    try
                                    {
                                        double otherX = double.Parse(otherVisual.X ?? "0");
                                        double otherY = double.Parse(otherVisual.Y ?? "0");
                                        double otherWidth = double.Parse(otherVisual.Width ?? "0");
                                        double otherHeight = double.Parse(otherVisual.Height ?? "0");

                                        if (otherX + otherWidth <= x)
                                        {
                                            if (!(otherY + otherHeight <= y || otherY >= y + height))
                                            {
                                                double leftDistance = x - (otherX + otherWidth);
                                                if (nearestVisuals["left"].distance == -1 || leftDistance < nearestVisuals["left"].distance)
                                                {
                                                    nearestVisuals["left"] = (leftDistance, otherVisual.VisualId ?? "");
                                                }
                                            }
                                        }

                                        if (otherX >= x + width)
                                        {
                                            if (!(otherY + otherHeight <= y || otherY >= y + height))
                                            {
                                                double rightDistance = otherX - (x + width);
                                                if (nearestVisuals["right"].distance == -1 || rightDistance < nearestVisuals["right"].distance)
                                                {
                                                    nearestVisuals["right"] = (rightDistance, otherVisual.VisualId ?? "");
                                                }
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        GlobalHandler.WriteCrashLog(ex.ToString());
                                    }
                                }

                                var spacingViolations = new List<string>();
                                var spacingDetails = new List<string>();
                                var violatedDirections = new List<string>();
                                var nearestVisualIds = new List<string>();

                                foreach (var direction in nearestVisuals.Keys)
                                {
                                    if (nearestVisuals[direction].distance != -1)
                                    {
                                        double distance = nearestVisuals[direction].distance;
                                        string directionName = direction.Substring(0, 1).ToUpper() + direction.Substring(1);

                                        spacingDetails.Add($"{directionName}: {distance:0.00}px");

                                        if (distance != targetSpacing)
                                        {
                                            spacingViolations.Add($"{directionName}: {Math.Abs(distance - targetSpacing):0.00}px");
                                            violatedDirections.Add(directionName);
                                            nearestVisualIds.Add(nearestVisuals[direction].visualId);
                                        }
                                    }
                                }

                                if (spacingViolations.Count != 0)
                                {
                                    string violationSummary = string.Join("; ", spacingViolations);
                                    string allSpacingDetails = string.Join("; ", spacingDetails);

                                    var dimensionNames = spacingViolations
                                        .Select(v => v.Split(':')[0].Trim())
                                        .ToList();
                                    string dimensionSummary = string.Join(", ", dimensionNames);

                                    var visual2Details = new List<string>();
                                    for (int i = 0; i < violatedDirections.Count; i++)
                                    {
                                        var nearestVisual = visuals.FirstOrDefault(v => v.VisualId == nearestVisualIds[i]);
                                        if (nearestVisual != null)
                                        {
                                            int nearestVisualIndex = visuals.IndexOf(nearestVisual) + 1;
                                            visual2Details.Add($"{nearestVisualIndex}) Visual ID: {nearestVisual.VisualId}, Visual Type: {nearestVisual.VisualType}, Visual Title: {(nearestVisual.VisualTitle != null ? $"{nearestVisual.VisualTitle}" : "")}");
                                        }
                                    }
                                    string visual2Summary = string.Join("; ", visual2Details);

                                    spacingIssues.Add(new Dictionary<string, object>
                                    {
                                        ["PageName"] = pageName ?? string.Empty,
                                        ["visual1"] = $"{visualIndex}) Visual ID: {visual.VisualId}, Visual Type: {visual.VisualType}, Visual Title: {(visual.VisualTitle != null ? $"{visual.VisualTitle}" : "")}\n",
                                        ["visual2"] = visual2Summary,
                                        ["spacingViolation"] = violationSummary,
                                        ["actualSpacing"] = allSpacingDetails,
                                        ["error"] = $"Visual have spacing issues: {dimensionSummary}"
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

                return new SingleIssueRuleData
                {
                    Issues = spacingIssues.Cast<object>().ToList(),
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