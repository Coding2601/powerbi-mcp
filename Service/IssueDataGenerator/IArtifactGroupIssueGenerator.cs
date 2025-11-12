using PowerBI_MCP.DTO;

namespace PowerBI_MCP.Service.IssueDataGenerator
{
    public interface IArtifactGroupIssueGenerator : IIssueDataGenerator
    {
        public void SetAssociatedReportsUIDoc(List<ReportDocumentation> reportUIDocs);

    }
}
