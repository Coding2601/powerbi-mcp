using PowerBI_MCP.DTO;
using PowerBI_MCP.Handlers;

namespace PowerBI_MCP.Service.IssueDataGenerator.Generators.VisualConsistencyIssuesGenerator
{
    public class ConsistentDataLabelFormatting : IIssueDataGenerator
    {
        public SingleIssueRuleData GetData(IssueContext issueContext)
        {
            List<object> issueData = [];
            Dictionary<string, List<VisualSummary>> inconsistentLabelFontSizeDic = new();
            foreach (var vis in  issueContext.ReportDocumentation?.VisualList ?? [])
            {
                try
                {
                    if (inconsistentLabelFontSizeDic.ContainsKey(vis.DataLabelFormatting["FontSize"]))
                    {
                        inconsistentLabelFontSizeDic[vis.DataLabelFormatting["FontSize"]].Add(vis);
                    }
                    else
                    {
                        inconsistentLabelFontSizeDic.Add(vis.DataLabelFormatting["FontSize"], [vis]);
                    }
                }
                catch (Exception ex)
                {
                    GlobalHandler.WriteCrashLog(ex.ToString());
                }
            }
            if (inconsistentLabelFontSizeDic.Count > 1)
            {
                foreach (var fontSize in inconsistentLabelFontSizeDic)
                {
                    try
                    {
                        List<VisualSummary> visList = fontSize.Value;
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
                                { "ReportName",  issueContext.ReportDocumentation?.ReportUISettings.ReportName ?? "" },
                                { "FontSize", fontSize.Key },
                                { "FontFamily", "" },
                                { "FontColor", "" },
                                { "BackgroundColor", "" },
                                { "BackgroundTransparency", "" },
                                { "Visibility", "" },
                                { "Orientation", "" },
                                { "AffectedVisualCount", fontSize.Value.Count },
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
            Dictionary<string, List<VisualSummary>> inconsistentLabelFontFamilyDic = new();
            foreach (var vis in  issueContext.ReportDocumentation?.VisualList ?? [])
            {
                try
                {
                    if (inconsistentLabelFontFamilyDic.ContainsKey(vis.DataLabelFormatting["FontFamily"]))
                    {
                        inconsistentLabelFontFamilyDic[vis.DataLabelFormatting["FontFamily"]].Add(vis);
                    }
                    else
                    {
                        inconsistentLabelFontFamilyDic.Add(vis.DataLabelFormatting["FontFamily"], [vis]);
                    }
                }
                catch (Exception ex)
                {
                    GlobalHandler.WriteCrashLog(ex.ToString());
                }
            }
            if (inconsistentLabelFontFamilyDic.Count > 1)
            {
                foreach (var fontFamily in inconsistentLabelFontFamilyDic)
                {
                    try
                    {
                        List<VisualSummary> visList = fontFamily.Value;
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
                                { "ReportName",  issueContext.ReportDocumentation?.ReportUISettings.ReportName ?? "" },
                                { "FontFamily", fontFamily.Key },
                                { "FontSize", "" },
                                { "FontColor", "" },
                                { "BackgroundColor", "" },
                                { "BackgroundTransparency", "" },
                                { "Visibility", "" },
                                { "Orientation", "" },
                                { "AffectedVisualCount", fontFamily.Value.Count },
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
            Dictionary<string, List<VisualSummary>> inconsistentLabelFontColorDic = new();
            foreach (var vis in  issueContext.ReportDocumentation?.VisualList ?? [])
            {
                try
                {
                    string? color = vis.DataLabelFormatting["FontColor"]?.ToString()?.Trim('\'')?.ToLower();
                    if (color == null) continue;
                    if (inconsistentLabelFontColorDic.ContainsKey(color) == true)
                    {
                        inconsistentLabelFontColorDic[color].Add(vis);
                    }
                    else
                    {
                        inconsistentLabelFontColorDic.Add(color, [vis]);
                    }
                }
                catch (Exception ex)
                {
                    GlobalHandler.WriteCrashLog(ex.ToString());
                }
            }

            if (inconsistentLabelFontColorDic.Count > 1)
            {
                foreach (var fontColor in inconsistentLabelFontColorDic)
                {
                    try
                    {
                        List<VisualSummary> visList = fontColor.Value;
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
                                { "ReportName",  issueContext.ReportDocumentation?.ReportUISettings.ReportName ?? "" },
                                { "FontColor", fontColor.Key },
                                { "FontSize", "" },
                                { "BackgroundColor", "" },
                                { "BackgroundTransparency", "" },
                                { "Visibility", "" },
                                { "Orientation", "" },
                                { "AffectedVisualCount", fontColor.Value.Count },
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
            Dictionary<string, List<VisualSummary>> inconsistentLabelBackgroundColorDic = new();
            foreach (var vis in  issueContext.ReportDocumentation?.VisualList ?? [])
            {
                try
                {
                    string? bgColor = vis.DataLabelFormatting["BackgroundColor"]?.ToString()?.Trim('\'')?.ToLower();
                    if (bgColor == null) continue;
                    if (inconsistentLabelBackgroundColorDic.ContainsKey(bgColor) == true)
                    {
                        inconsistentLabelBackgroundColorDic[bgColor].Add(vis);
                    }
                    else
                    {
                        inconsistentLabelBackgroundColorDic.Add(bgColor, [vis]);
                    }
                }
                catch (Exception ex)
                {
                    GlobalHandler.WriteCrashLog(ex.ToString());
                }
            }
            if (inconsistentLabelBackgroundColorDic.Count > 1)
            {
                foreach (var backgroundColor in inconsistentLabelBackgroundColorDic)
                {
                    try
                    {
                        List<VisualSummary> visList = backgroundColor.Value;
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
                                { "ReportName",  issueContext.ReportDocumentation?.ReportUISettings.ReportName ?? "" },
                                { "BackgroundColor", backgroundColor.Key },
                                { "FontSize", "" },
                                { "FontFamily", "" },
                                { "FontColor", "" },
                                { "BackgroundTransparency", "" },
                                { "Visibility", "" },
                                { "Orientation", "" },
                                { "AffectedVisualCount", backgroundColor.Value.Count },
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
            Dictionary<string, List<VisualSummary>> inconsistentLabelBackgroundTransparencyDic = new();
            foreach (var vis in  issueContext.ReportDocumentation?.VisualList ?? [])
            {
                try
                {
                    if (inconsistentLabelBackgroundTransparencyDic.ContainsKey(vis.DataLabelFormatting["BackgroundTransparency"]))
                    {
                        inconsistentLabelBackgroundTransparencyDic[vis.DataLabelFormatting["BackgroundTransparency"]].Add(vis);
                    }
                    else
                    {
                        inconsistentLabelBackgroundTransparencyDic.Add(vis.DataLabelFormatting["BackgroundTransparency"], [vis]);
                    }
                }
                catch (Exception ex)
                {
                    GlobalHandler.WriteCrashLog(ex.ToString());
                }
            }
            if (inconsistentLabelBackgroundTransparencyDic.Count > 1)
            {
                foreach (var backgroundTransparency in inconsistentLabelBackgroundTransparencyDic)
                {
                    try
                    {
                        List<VisualSummary> visList = backgroundTransparency.Value;
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
                                { "ReportName",  issueContext.ReportDocumentation?.ReportUISettings.ReportName ?? "" },
                                { "BackgroundTransparency", backgroundTransparency.Key },
                                { "FontSize", "" },
                                { "FontFamily", "" },
                                { "FontColor", "" },
                                { "BackgroundColor", "" },
                                { "Visibility", "" },
                                { "Orientation", "" },
                                { "AffectedVisualCount", backgroundTransparency.Value.Count },
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
            Dictionary<string, List<VisualSummary>> inconsistentLabelVisibilityDic = new();
            foreach (var vis in  issueContext.ReportDocumentation?.VisualList ?? [])
            {
                try
                {
                    if (inconsistentLabelVisibilityDic.ContainsKey(vis.DataLabelFormatting["Visibility"]))
                    {
                        inconsistentLabelVisibilityDic[vis.DataLabelFormatting["Visibility"]].Add(vis);
                    }
                    else
                    {
                        inconsistentLabelVisibilityDic.Add(vis.DataLabelFormatting["Visibility"], [vis]);
                    }
                }
                catch (Exception ex)
                {
                    GlobalHandler.WriteCrashLog(ex.ToString());
                }
            }
            if (inconsistentLabelVisibilityDic.Count > 1)
            {
                foreach (var Visibility in inconsistentLabelVisibilityDic)
                {
                    try
                    {
                        List<VisualSummary> visList = Visibility.Value;
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
                                { "ReportName",  issueContext.ReportDocumentation?.ReportUISettings.ReportName ?? "" },
                                { "Visibility", Visibility.Key },
                                { "FontSize", "" },
                                { "FontFamily", "" },
                                { "FontColor", "" },
                                { "BackgroundColor", "" },
                                { "BackgroundTransparency", "" },
                                { "Orientation", "" },
                                { "AffectedVisualCount", Visibility.Value.Count },
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
            Dictionary<string, List<VisualSummary>> inconsistentLabelOrientationDic = new();
            foreach (var vis in  issueContext.ReportDocumentation?.VisualList ?? [])
            {
                try
                {
                    if (inconsistentLabelOrientationDic.ContainsKey(vis.DataLabelFormatting["Orientation"]))
                    {
                        inconsistentLabelOrientationDic[vis.DataLabelFormatting["Orientation"]].Add(vis);
                    }
                    else
                    {
                        inconsistentLabelOrientationDic.Add(vis.DataLabelFormatting["Orientation"], [vis]);
                    }
                }
                catch (Exception ex)
                {
                    GlobalHandler.WriteCrashLog(ex.ToString());
                }
            }

            if (inconsistentLabelOrientationDic.Count > 1)
            {
                foreach (var orientation in inconsistentLabelOrientationDic)
                {
                    try
                    {
                        List<VisualSummary> visList = orientation.Value;
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
                                { "ReportName",  issueContext.ReportDocumentation?.ReportUISettings.ReportName ?? "" },
                                { "Orientation", orientation.Key },
                                { "FontSize", "" },
                                { "FontFamily", "" },
                                { "FontColor", "" },
                                { "BackgroundColor", "" },
                                { "BackgroundTransparency", "" },
                                { "Visibility", "" },
                                { "AffectedVisualCount", orientation.Value.Count },
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