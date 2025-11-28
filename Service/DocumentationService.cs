using PowerBI_MCP.DTO;
using PowerBI_MCP.Interfaces;
using PowerBI_MCP.Models;
using PowerBI_MCP.Handlers;
using PowerBI_MCP.Utils;
using PowerBI_MCP.Utils.Cache;

namespace PowerBI_MCP.Service
{
    public class DocumentationService : IDocumentationService
    {
        private readonly DocumentationHandler _docHandler;
        public DocumentationService(DocumentationHandler docHandler)
        {
            _docHandler = docHandler;
        }
        public ReportDocumentation? GetReportDoc(string reportName)
        {
            List<ReportModel> reports = GlobalHandler.GetReportByName(reportName);
            if (reports.Count == 0)
                throw new Exception($"report not found, please connect this report");
            if (reports.Count > 1)
            {
                string reportPaths = string.Join(", ", reports.Select(r => r.ReportPath));
                throw new Exception($"There already exists a report with the name '{reportName}");
            }
            ReportModel report = reports[0];
            if (ReportCache.Get(report.ReportId) != null)
                return ReportCache.Get(report.ReportId);
            bool isPBIR = report.ReportType == ReportType.PBIR;
            string extractionPath = Path.Combine(AppConfig.duplicateReportZipDirectory, report.ReportId);
            return _docHandler.GenerateReportDocumentation(
                isPBIR,
                extractionPath,
                report.ReportId
            );
        }

        public ModelDocumentation? GetModelDoc(string modelName)
        {
            List<DatasetModel> datasets = GlobalHandler.GetModelByName(modelName);
            if (datasets.Count == 0)
                throw new Exception($"semantic model not found, please connect this semantic model");
            if (datasets.Count > 1)
            {
                throw new Exception($"There already exists a semantic model with the name '{modelName}'");
            }
            DatasetModel dataset = datasets[0];
            if (ModelCache.Get(dataset.DatasetId) != null)
                return ModelCache.Get(dataset.DatasetId);
            return _docHandler.GenerateModelDocumentation(dataset.dbStatic);
        }
    }
}