using PowerBI_MCP.DTO;
using PowerBI_MCP.Entities;

namespace PowerBI_MCP.Interfaces
{
    public interface IIssueService
    {
        public List<IssueSection> GetIssueSection();
        public object GetAlignmentIssues(string artifactName, string? spacing, string userEmail);
        public AllIssueRuleData GetData(string artifactName, string artifactType, string userEmail);
    }
}