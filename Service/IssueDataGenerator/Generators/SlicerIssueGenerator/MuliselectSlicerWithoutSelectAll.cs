using PowerBI_MCP.DTO;
using PowerBI_MCP.Handlers;

namespace PowerBI_MCP.Service.IssueDataGenerator.Generators.SlicerIssueGenerator
{
    public class MuliselectSlicerWithoutSelectAll : IIssueDataGenerator
    {
        public SingleIssueRuleData GetData(IssueContext issueContext)
        {
            var tempIssueData = issueContext.ReportDocumentation?.ReportUIFormatting.SlicersList.Where(x => x.IsSearchAllInMultiSelectDisabled == true);

            List<object> issueData = [];
            foreach (var vis in tempIssueData ?? [])
            {
                try
                {
                    if (vis.VisualId != null && issueContext.ReportDocumentation?.VisualDataDictionary.ContainsKey(vis.VisualId) == true)
                        issueData.Add(issueContext.ReportDocumentation.VisualDataDictionary[vis.VisualId]);
                }
                catch (Exception ex)
                {
                    GlobalHandler.WriteCrashLog(ex.ToString());
                }
            }

            return new() { Issues = issueData, MaxIssuable = issueContext.ReportDocumentation?.ReportUIFormatting.SlicersList?.Count };
        }
    }
}