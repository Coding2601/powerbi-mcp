using PowerBI_MCP.DTO;

namespace PowerBI_MCP.Service.IssueDataGenerator
{
    public interface IVisualAlignmentIssueGenerator : IIssueDataGenerator
    {
        public void SetAssociatedReportsUIDoc(List<ReportDocumentation> reportUIDocs);
        public void SetVisualSpacing(string? spacing);
    }
}
