using Microsoft.AnalysisServices.Tabular;
using PowerBI_MCP.DTO;

namespace PowerBI_MCP.Models
{
    public enum ConnectionType
    {
        SASS,
        BIM,
        TMDL,
        UNKNOWN
    }

    public class ArtifactModel
    {
        public static List<ReportModel> Reports { get; set; } = new List<ReportModel>();
        public static List<DatasetModel> Datasets { get; set; } = new List<DatasetModel>();
    }

    public class ReportModel
    {
        public string ReportId { get; set; } = string.Empty;
        public string ReportName { get; set; } = string.Empty;
        public string ReportPath { get; set; } = string.Empty;
        public ReportType ReportType { get; set; } = ReportType.UNKNOWN;
    }

    public class DatasetModel
    {
        public string DatasetId { get; set; } = string.Empty;
        public string DatasetName { get; set; } = string.Empty;
        public ConnectionType ConnectionType { get; set; } = ConnectionType.UNKNOWN;
        public string ServerName { get; set; } = string.Empty;
        public string DbName { get; set; } = string.Empty;
        public Database dbStatic { get; set; } = new Database();
    }
}