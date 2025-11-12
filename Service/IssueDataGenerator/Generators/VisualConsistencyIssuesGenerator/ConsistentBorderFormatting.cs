using PowerBI_MCP.DTO;
using PowerBI_MCP.Handlers;

namespace PowerBI_MCP.Service.IssueDataGenerator.Generators.VisualConsistencyIssuesGenerator
{
    public class ConsistentBorderFormatting : IIssueDataGenerator
    {
        public SingleIssueRuleData GetData(IssueContext issueContext)
        {
            List<object> issueData = [];
            Dictionary<string, List<VisualSummary>> inconsistentBorderVisibilityDic = new();
            foreach (var vis in issueContext.ReportDocumentation?.VisualList ?? [])
            {
                try
                {
                    if (inconsistentBorderVisibilityDic.ContainsKey(vis.BorderFormatting["Visibility"]))
                    {
                        inconsistentBorderVisibilityDic[vis.BorderFormatting["Visibility"]].Add(vis);
                    }
                    else
                    {
                        inconsistentBorderVisibilityDic.Add(vis.BorderFormatting["Visibility"], [vis]);
                    }
                }
                catch (Exception ex)
                {
                    GlobalHandler.WriteCrashLog(ex.ToString());
                }
            }
            if (inconsistentBorderVisibilityDic.Count > 1)
            {
                foreach (var borderVisibility in inconsistentBorderVisibilityDic)
                {
                    try
                    {
                        List<VisualSummary> visList = borderVisibility.Value;
                        List<Dictionary<string, string>> visualDetailList = new();
                        string visualListStr = "";
                        int visIdx = 0;
                        foreach (var vis in visList)
                        {
                            try
                            {
                                visIdx++;
                                visualListStr +=
                                    visIdx.ToString()
                                    + ") Visual Type: "
                                    + vis.VisualType
                                    + ", Visual ID: "
                                    + vis.VisualId
                                    + (vis.VisualTitle != null && vis.VisualTitle.Trim() != "" ? ", Visual Title: " + vis.VisualTitle : "")
                                    + "  \n";
                                var visualList = new Dictionary<string, string>() { { "visualId", vis.VisualId ?? "" } };
                                visualDetailList.Add(visualList);
                            }
                            catch (Exception ex)
                            {
                                GlobalHandler.WriteCrashLog(ex.ToString());
                            }
                        }
                        issueData.Add(
                            new Dictionary<string, object>()
                            {
                                { "ReportName", issueContext.ReportDocumentation?.ReportUISettings.ReportName ?? "" },
                                { "Visibility", borderVisibility.Key },
                                { "Thickness", "" },
                                { "Color", "" },
                                { "Radius", "" },
                                { "AffectedVisualCount", borderVisibility.Value.Count },
                                { "AffectedVisuals", visualListStr },
                                { "AffectedVisualsDetail", visualDetailList },
                            }
                        );
                    }
                    catch (Exception ex)
                    {
                        GlobalHandler.WriteCrashLog(ex.ToString());
                    }
                }
            }
            Dictionary<string, List<VisualSummary>> inconsistentBorderThicknessDic = new();
            foreach (var vis in issueContext.ReportDocumentation?.VisualList ?? [])
            {
                try
                {
                    if (inconsistentBorderThicknessDic.ContainsKey(vis.BorderFormatting["Thickness"]))
                    {
                        inconsistentBorderThicknessDic[vis.BorderFormatting["Thickness"]].Add(vis);
                    }
                    else
                    {
                        inconsistentBorderThicknessDic.Add(vis.BorderFormatting["Thickness"], [vis]);
                    }
                }
                catch (Exception ex)
                {
                    GlobalHandler.WriteCrashLog(ex.ToString());
                }
            }
            if (inconsistentBorderThicknessDic.Count > 1)
            {
                foreach (var borderThickness in inconsistentBorderThicknessDic)
                {
                    try
                    {
                        List<VisualSummary> visList = borderThickness.Value;
                        List<Dictionary<string, string>> visualDetailList = new();
                        string visualListStr = "";
                        int visIdx = 0;
                        foreach (var vis in visList)
                        {
                            try
                            {
                                visIdx++;
                                visualListStr +=
                                    visIdx.ToString()
                                    + ") Visual Type: "
                                    + vis.VisualType
                                    + ", Visual ID: "
                                    + vis.VisualId
                                    + (vis.VisualTitle != null && vis.VisualTitle.Trim() != "" ? ", Visual Title: " + vis.VisualTitle : "")
                                    + "  \n";
                                var visualList = new Dictionary<string, string>() { { "visualId", vis.VisualId ?? "" } };
                                visualDetailList.Add(visualList);
                            }
                            catch (Exception ex)
                            {
                                GlobalHandler.WriteCrashLog(ex.ToString());
                            }
                        }
                        issueData.Add(
                            new Dictionary<string, object>()
                            {
                                { "ReportName", issueContext.ReportDocumentation?.ReportUISettings.ReportName ?? "" },
                                { "Thickness", borderThickness.Key },
                                { "Visibility", "" },
                                { "Color", "" },
                                { "Radius", "" },
                                { "AffectedVisualCount", borderThickness.Value.Count },
                                { "AffectedVisuals", visualListStr },
                                { "AffectedVisualsDetail", visualDetailList },
                            }
                        );
                    }
                    catch (Exception ex)
                    {
                        GlobalHandler.WriteCrashLog(ex.ToString());
                    }
                }
            }
            Dictionary<string, List<VisualSummary>> inconsistentBorderColorDic = [];

            foreach (var vis in issueContext.ReportDocumentation?.VisualList ?? [])
            {
                try
                {
                    string? color = vis.BorderFormatting["Color"]?.ToString()?.Trim('\'')?.ToLower();
                    if (color == null) continue;
                    if (inconsistentBorderColorDic.ContainsKey(color) == true)
                    {
                        inconsistentBorderColorDic[color].Add(vis);
                    }
                    else
                    {
                        inconsistentBorderColorDic.Add(color, [vis]);
                    }
                }
                catch (Exception ex)
                {
                    GlobalHandler.WriteCrashLog(ex.ToString());
                }
            }
            if (inconsistentBorderColorDic.Count > 1)
            {
                foreach (var borderColor in inconsistentBorderColorDic)
                {
                    try
                    {
                        List<VisualSummary> visList = borderColor.Value;
                        List<Dictionary<string, string>> visualDetailList = new();
                        string visualListStr = "";
                        int visIdx = 0;
                        foreach (var vis in visList)
                        {
                            try
                            {
                                visIdx++;
                                visualListStr +=
                                    visIdx.ToString()
                                    + ") Visual Type: "
                                    + vis.VisualType
                                    + ", Visual ID: "
                                    + vis.VisualId
                                    + (vis.VisualTitle != null && vis.VisualTitle.Trim() != "" ? ", Visual Title: " + vis.VisualTitle : "")
                                    + "  \n";
                                var visualList = new Dictionary<string, string>() { { "visualId", vis.VisualId ?? "" } };
                                visualDetailList.Add(visualList);
                            }
                            catch (Exception ex)
                            {
                                GlobalHandler.WriteCrashLog(ex.ToString());
                            }
                        }
                        issueData.Add(
                            new Dictionary<string, object>()
                            {
                                { "ReportName", issueContext.ReportDocumentation?.ReportUISettings.ReportName ?? "" },
                                { "Color", borderColor.Key },
                                { "Visibility", "" },
                                { "Thickness", "" },
                                { "Radius", "" },
                                { "AffectedVisualCount", borderColor.Value.Count },
                                { "AffectedVisuals", visualListStr },
                                { "AffectedVisualsDetail", visualDetailList },
                            }
                        );
                    }
                    catch (Exception ex)
                    {
                        GlobalHandler.WriteCrashLog(ex.ToString());
                    }
                }
            }
            Dictionary<string, List<VisualSummary>> inconsistentBorderRadiusDic = new();
            foreach (var vis in issueContext.ReportDocumentation?.VisualList ?? [])
            {
                try
                {
                    if (inconsistentBorderRadiusDic.ContainsKey(vis.BorderFormatting["Radius"]))
                    {
                        inconsistentBorderRadiusDic[vis.BorderFormatting["Radius"]].Add(vis);
                    }
                    else
                    {
                        inconsistentBorderRadiusDic.Add(vis.BorderFormatting["Radius"], [vis]);
                    }
                }
                catch (Exception ex)
                {
                    GlobalHandler.WriteCrashLog(ex.ToString());
                }
            }
            if (inconsistentBorderRadiusDic.Count > 1)
            {
                foreach (var borderRadius in inconsistentBorderRadiusDic)
                {
                    try
                    {
                        List<VisualSummary> visList = borderRadius.Value;
                        List<Dictionary<string, string>> visualDetailList = new();
                        string visualListStr = "";
                        int visIdx = 0;
                        foreach (var vis in visList)
                        {
                            try
                            {
                                visIdx++;
                                visualListStr +=
                                    visIdx.ToString()
                                    + ") Visual Type: "
                                    + vis.VisualType
                                    + ", Visual ID: "
                                    + vis.VisualId
                                    + (vis.VisualTitle != null && vis.VisualTitle.Trim() != "" ? ", Visual Title: " + vis.VisualTitle : "")
                                    + "  \n";
                                var visualList = new Dictionary<string, string>() { { "visualId", vis.VisualId ?? "" } };
                                visualDetailList.Add(visualList);
                            }
                            catch (Exception ex)
                            {
                                GlobalHandler.WriteCrashLog(ex.ToString());
                            }
                        }
                        issueData.Add(
                            new Dictionary<string, object>()
                            {
                                { "ReportName", issueContext.ReportDocumentation?.ReportUISettings.ReportName ?? "" },
                                { "Radius", borderRadius.Key },
                                { "Visibility", "" },
                                { "Thickness", "" },
                                { "Color", "" },
                                { "AffectedVisualCount", borderRadius.Value.Count },
                                { "AffectedVisuals", visualListStr },
                                { "AffectedVisualsDetail", visualDetailList },
                            }
                        );
                    }
                    catch (Exception ex)
                    {
                        GlobalHandler.WriteCrashLog(ex.ToString());
                    }
                }
            }
            return new() { Issues = issueData, MaxIssuable = 0 };
        }
    }
}