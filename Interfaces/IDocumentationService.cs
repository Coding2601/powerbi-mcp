using PowerBI_MCP.Models;
using PowerBI_MCP.DTO;

namespace PowerBI_MCP.Interfaces
{
    public interface IDocumentationService
    {
        public ReportDocumentation? GetReportDoc(string reportName);
        public ModelDocumentation? GetModelDoc(string modelName);
    }
}