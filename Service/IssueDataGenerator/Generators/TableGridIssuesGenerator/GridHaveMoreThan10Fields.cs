using PowerBI_MCP.DTO;
using PowerBI_MCP.Handlers;

namespace PowerBI_MCP.Service.IssueDataGenerator.Generators.TableGridIssuesGenerator
{
    public class GridHaveMoreThan10Fields : IIssueDataGenerator
    {
        public SingleIssueRuleData GetData(IssueContext issueContext)
        {
            var tempIssueData = issueContext.ReportDocumentation?.ReportUIFormatting.TableOrMatrixList.Where(x => x.ColumnCount > 20);

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

            return new() { Issues = issueData, MaxIssuable = issueContext.ReportDocumentation?.ReportUIFormatting.TableOrMatrixList?.Count };
        }
    }
}