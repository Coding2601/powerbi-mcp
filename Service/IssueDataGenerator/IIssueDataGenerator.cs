using PowerBI_MCP.DTO;

namespace PowerBI_MCP.Service.IssueDataGenerator
{
    public interface IIssueDataGenerator{
        public SingleIssueRuleData GetData(IssueContext issueContext);
    }    
}