using PowerBI_MCP.DTO;
using PowerBI_MCP.Handlers;

namespace PowerBI_MCP.Service.IssueDataGenerator.Generators.CopilotIssuesGenerator
{
    public class MissingMeasuresSynonyms : IIssueDataGenerator
    {
        public SingleIssueRuleData GetData(IssueContext issueContext)
        {
            List<object> issueData = [];
            
            return new() { Issues = issueData ?? [], MaxIssuable = issueContext.ModelDocumentation?.Measures?.Count };
        }
    }
}