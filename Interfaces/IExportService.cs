using System.Data;
using PowerBI_MCP.Entities;
using PowerBI_MCP.Models;

namespace PowerBI_MCP.Interfaces
{
    public interface IExportService
    {
        /// <summary>
        /// Exports selected Power BI report and semantic model sections to Excel,
        /// generates multiple DataSets, and compresses them into a ZIP.
        /// </summary>
        Task ExportToExcel(ReportModel report, DatasetModel model, List<string> exportAreas);

        /// <summary>
        /// Generates documentation data tables for a report using the report cache.
        /// </summary>
        void GenerateReportDocumentationDataset(string cacheKey, DataTable docDatasetTable, DataSet docDataSet);

        /// <summary>
        /// Generates documentation data tables for a semantic model using the model cache.
        /// </summary>
        void GenerateModelDocumentationDataset(string cacheKey, DataTable docDatasetTable, DataSet docDataSet);
    }
}