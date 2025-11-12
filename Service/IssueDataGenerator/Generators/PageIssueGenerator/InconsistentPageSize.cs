using PowerBI_MCP.DTO;
using PowerBI_MCP.Handlers;

namespace PowerBI_MCP.Service.IssueDataGenerator.Generators.PageIssueGenerator
{
    public class InconsistentPageSize : IIssueDataGenerator
    {
        public SingleIssueRuleData GetData(IssueContext issueContext)
        {
            List<object> issueData = [];
            HashSet<string> pageSizes = new HashSet<string>();
            foreach (var page in issueContext.ReportDocumentation?.PageSummary ?? [])
            {
                try
                {
                    if (page.PageType != "Tooltip Page")
                    {
                        if (page.Size != null)
                            pageSizes.Add(page.Size);
                        issueData.Add(page);
                    }
                }
                catch (Exception ex)
                {
                    GlobalHandler.WriteCrashLog(ex.ToString());
                }
            }
            return new() { Issues = pageSizes.Count > 1 ? issueData : [], MaxIssuable = issueContext.ReportDocumentation?.PageSummary?.Count };
        }
    }
}