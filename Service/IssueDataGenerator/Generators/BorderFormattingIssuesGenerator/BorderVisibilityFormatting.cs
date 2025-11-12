using PowerBI_MCP.DTO;
using PowerBI_MCP.Handlers;

namespace PowerBI_MCP.Service.IssueDataGenerator.Generators.BorderFormattingIssuesGenerator
{
    public class BorderVisibilityFormatting : IIssueDataGenerator
    {
        public SingleIssueRuleData GetData(IssueContext issueContext)
        {
            List<object> issueData = [];
            Dictionary<string, List<VisualSummary>> inconsistentVis = new();
            foreach (var vis in issueContext.ReportDocumentation?.VisualList ?? [])
            {
                try
                {
                    if (inconsistentVis.ContainsKey(vis.BorderFormatting["Visibility"]))
                    {
                        inconsistentVis[vis.BorderFormatting["Visibility"]].Add(vis);
                    }
                    else
                    {
                        inconsistentVis.Add(vis.BorderFormatting["Visibility"], [vis]);
                    }
                }
                catch (Exception ex)
                {
                    GlobalHandler.WriteCrashLog(ex.ToString());
                }
            }
            if (inconsistentVis.Count > 1)
            {
                foreach (var property in inconsistentVis)
                {
                    try
                    {
                        List<VisualSummary> visList = property.Value;
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
                                { "Visibility", property.Key}, 
                                { "AffectedVisualCount", property.Value.Count },
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

            return new SingleIssueRuleData()
            {
                Issues = issueData,
                MaxIssuable = issueData.Count
            };
        }
    }
}