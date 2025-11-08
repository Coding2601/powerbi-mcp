namespace PowerBI_MCP.DTO
{
    public enum ReportType
    {
        PBIX,
        PBIR,
        UNKNOWN
    }
    public enum ModelType
    {
        SASS,
        BIM,
        TMDL
    }
    public class ReportConnectionDTO
    {
        public string ReportName { get; set; } = string.Empty;
        public string ReportPath { get; set; } = string.Empty;
    }

    public class ModelConnectionDTO
    {
        public string ModelName { get; set; } = string.Empty;
    }
}