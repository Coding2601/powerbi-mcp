using PowerBI_MCP.DTO;
using PowerBI_MCP.Handlers;

namespace PowerBI_MCP.Service.IssueDataGenerator.Generators.CopilotIssuesGenerator
{
    public class AddDescriptionInTable : IIssueDataGenerator
    {
        public SingleIssueRuleData GetData(IssueContext issueContext)
        { 
            var issues = new List<object>();

            foreach (var tbl in issueContext.ModelDocumentation?.Tables ?? [])
            {
                if (string.IsNullOrWhiteSpace(tbl.Description))
                {
                    issues.Add(new Dictionary<string, object>
                    {
                        ["TableName"] = tbl.TableName ?? string.Empty,
                    });
                }
            }
            return new() { Issues = issues ?? [], MaxIssuable = issueContext.ModelDocumentation?.Tables?.Count };
        }
    }
}