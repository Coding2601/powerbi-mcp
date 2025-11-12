using PowerBI_MCP.DTO;
using PowerBI_MCP.Handlers;

namespace PowerBI_MCP.Service.IssueDataGenerator.Generators.VisualConsistencyIssuesGenerator
{
    public class ConsistentBackgroundFormatting : IIssueDataGenerator
    {
        public SingleIssueRuleData GetData(IssueContext issueContext)
        {
            List<object> issueData = [];
            Dictionary<string, List<VisualSummary>> inconsistentBackgroundColorDic = new();
            foreach (var vis in issueContext.ReportDocumentation?.VisualList ?? [])
            {
                try
                {
                    string? color = vis.BackgroundFormatting["Color"]?.ToString()?.Trim('\'')?.ToLower();
                    if (color == null) continue;

                    if (inconsistentBackgroundColorDic.ContainsKey(color) == true)
                    {
                        inconsistentBackgroundColorDic[color].Add(vis);
                    }
                    else
                    {
                        inconsistentBackgroundColorDic.Add(color, [vis]);
                    }
                }
                catch (Exception ex)
                {
                    GlobalHandler.WriteCrashLog(ex.ToString());
                }
            }
            if (inconsistentBackgroundColorDic.Count > 1)
            {
                foreach (var backgroundColor in inconsistentBackgroundColorDic)
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
                                { "ReportName", issueContext.ReportDocumentation?.ReportUISettings.ReportName ?? "" },
                                { "Color", backgroundColor.Key },
                                { "Transparency", "" },
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
            Dictionary<string, List<VisualSummary>> inconsistentBackgroundTransparencyDic = new();
            foreach (var vis in issueContext.ReportDocumentation?.VisualList ?? [])
            {
                try
                {
                    if (inconsistentBackgroundTransparencyDic.ContainsKey(vis.BackgroundFormatting["Transparency"]))
                    {
                        inconsistentBackgroundTransparencyDic[vis.BackgroundFormatting["Transparency"]].Add(vis);
                    }
                    else
                    {
                        inconsistentBackgroundTransparencyDic.Add(vis.BackgroundFormatting["Transparency"], [vis]);
                    }
                }
                catch (Exception ex)
                {
                    GlobalHandler.WriteCrashLog(ex.ToString());
                }
            }
            if (inconsistentBackgroundTransparencyDic.Count > 1)
            {
                foreach (var backgroundTransparency in inconsistentBackgroundTransparencyDic)
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
                                    + (vis.VisualTitle != null && vis.VisualTitle.Trim() != "" ? ",  " + vis.VisualTitle : "")
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
                                { "Transparency", backgroundTransparency.Key },
                                { "Color", "" },
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

            return new() { Issues = issueData, MaxIssuable = 0 };
        }
    }
}