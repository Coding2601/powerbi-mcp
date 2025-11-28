namespace PowerBI_MCP.DTO
{
    public class ExportRequestDTO
    {
        public string ReportName { get; set; } = "";
        public string SemanticModelName { get; set; } = "";
        public List<string> ExportAreas { get; set; } = new();
    }
    public class ReportUIDetailsDTO { 
        public string SemanticModelWorkspaceId { get; set; } = string.Empty;
        public string SemanticModelId { get;set; } = string.Empty;
        public string WorkspaceId { get; set; } = "";
        public string ReportId { get; set; } = "";
    }

    public class SemanticModelDetailsDTO
    {
        public string WorkspaceId { get; set; } = "";
        public string SemanticModelId { get; set; } = "";
    }
    public class ReportDTO : ReportUIDetailsDTO
    {
        public string Name { get; set; } = "";
    }
    public class SemanticModelDTO : SemanticModelDetailsDTO
    {
        public string Name { get; set; } = "";
    }
}