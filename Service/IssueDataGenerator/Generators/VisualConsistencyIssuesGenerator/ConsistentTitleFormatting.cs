using PowerBI_MCP.DTO;
using PowerBI_MCP.Handlers;

namespace PowerBI_MCP.Service.IssueDataGenerator.Generators.VisualConsistencyIssuesGenerator
{
    public class ConsistentTitleFormatting : IIssueDataGenerator
    {
        public SingleIssueRuleData GetData(IssueContext issueContext)
        {
            List<object> issueData = [];

            Dictionary<string, List<VisualSummary>> inconsistentTitleAlignmentDic = new();
            foreach (var vis in issueContext.ReportDocumentation?.VisualList ?? [])
            {
                try
                {
                    if (inconsistentTitleAlignmentDic.ContainsKey(vis.WithoutTitleFormatting["Alignment"]))
                    {
                        inconsistentTitleAlignmentDic[vis.WithoutTitleFormatting["Alignment"]].Add(vis);
                    }
                    else
                    {
                        inconsistentTitleAlignmentDic.Add(vis.WithoutTitleFormatting["Alignment"], [vis]);
                    }
                }
                catch (Exception ex)
                {
                    GlobalHandler.WriteCrashLog(ex.ToString());
                }
            }
            if (inconsistentTitleAlignmentDic.Count > 1)
            {
                foreach (var alignment in inconsistentTitleAlignmentDic)
                {
                    try
                    {
                        List<VisualSummary> visList = alignment.Value;
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
                                { "Alignment", alignment.Key },
                                { "FontSize", "" },
                                { "FontFamily", "" },
                                { "FontColor", "" },
                                { "AffectedVisualCount", alignment.Value.Count },
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
            Dictionary<string, List<VisualSummary>> inconsistentTitleFontSizeDic = new();
            foreach (var vis in issueContext.ReportDocumentation?.VisualList ?? [])
            {
                try
                {
                    if (inconsistentTitleFontSizeDic.ContainsKey(vis.WithoutTitleFormatting["FontSize"]))
                    {
                        inconsistentTitleFontSizeDic[vis.WithoutTitleFormatting["FontSize"]].Add(vis);
                    }
                    else
                    {
                        inconsistentTitleFontSizeDic.Add(vis.WithoutTitleFormatting["FontSize"], [vis]);
                    }
                }
                catch (Exception ex)
                {
                    GlobalHandler.WriteCrashLog(ex.ToString());
                }
            }
            if (inconsistentTitleFontSizeDic.Count > 1)
            {
                foreach (var fontSize in inconsistentTitleFontSizeDic)
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
                                { "ReportName", issueContext.ReportDocumentation?.ReportUISettings.ReportName ?? "" },
                                { "FontSize", fontSize.Key },
                                { "Alignment", "" },
                                { "FontFamily", "" },
                                { "FontColor", "" },
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
            Dictionary<string, List<VisualSummary>> inconsistentTitleFontFamilyDic = new();
            foreach (var vis in issueContext.ReportDocumentation?.VisualList ?? [])
            {
                try
                {
                    if (inconsistentTitleFontFamilyDic.ContainsKey(vis.WithoutTitleFormatting["FontFamily"]))
                    {
                        inconsistentTitleFontFamilyDic[vis.WithoutTitleFormatting["FontFamily"]].Add(vis);
                    }
                    else
                    {
                        inconsistentTitleFontFamilyDic.Add(vis.WithoutTitleFormatting["FontFamily"], [vis]);
                    }
                }
                catch (Exception ex)
                {
                    GlobalHandler.WriteCrashLog(ex.ToString());
                }
            }
            if (inconsistentTitleFontFamilyDic.Count > 1)
            {
                foreach (var fontFamily in inconsistentTitleFontFamilyDic)
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
                                { "ReportName", issueContext.ReportDocumentation?.ReportUISettings.ReportName ?? "" },
                                { "FontFamily", fontFamily.Key },
                                { "Alignment", "" },
                                { "FontSize", "" },
                                { "FontColor", "" },
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
            Dictionary<string, List<VisualSummary>> inconsistentTitleFontColorDic = new();
            foreach (var vis in issueContext.ReportDocumentation?.VisualList ?? [])
            {
                try
                {
                    string? color = vis.WithoutTitleFormatting["FontColor"]?.ToString()?.Trim('\'')?.ToLower();
                    if (color == null)
                        continue;
                    if (inconsistentTitleFontColorDic.ContainsKey(color) == true)
                    {
                        inconsistentTitleFontColorDic[color].Add(vis);
                    }
                    else
                    {
                        inconsistentTitleFontColorDic.Add(color, [vis]);
                    }
                }
                catch (Exception ex)
                {
                    GlobalHandler.WriteCrashLog(ex.ToString());
                }
            }
            if (inconsistentTitleFontColorDic.Count > 1)
            {
                foreach (var fontColor in inconsistentTitleFontColorDic)
                {
                    try
                    {
                        List<VisualSummary> visList = fontColor.Value;
                        List<Dictionary<string, string>> visualDetailList = [];
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
                                { "FontColor", fontColor.Key },
                                { "Alignment", "" },
                                { "FontSize", "" },
                                { "FontFamily", "" },
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
            return new() { Issues = issueData, MaxIssuable = 0 }; 
        }
    }
}