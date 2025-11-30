using ModelContextProtocol.Server;
using PowerBI_MCP.DTO;
using PowerBI_MCP.Interfaces;
using PowerBI_MCP.Models;
using PowerBI_MCP.Handlers;
using System.ComponentModel;
using Microsoft.AspNetCore.Mvc;

namespace PowerBI_MCP.MCP
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    [McpServerToolType]
    public class ExportController
    {
        private readonly IExportService _exportService;

        public ExportController(IExportService exportService)
        {
            _exportService = exportService;
        }

        [McpServerTool]
        [Description(
            "Exports Power BI report insights, alignment, documentation, and unused fields to Excel. " +
            "User provides the report name, semantic model name, and selected export areas. " +
            "The tool validates the report/model and generates an Excel export automatically."
        )]
        public async Task<object> Mcp_ExportToExcel(
            [Description("Name of the Power BI report to export data from")] string reportName,
            [Description("Name of the semantic model associated with the report")] string semanticModelName,
            [Description("List of export areas (Insights / Alignment / Documentation / Unused)")] List<string> exportAreas)
        {
            if (string.IsNullOrWhiteSpace(reportName) || string.IsNullOrWhiteSpace(semanticModelName))
            {
                return new
                {
                    success = false,
                    message = "Invalid report or semantic model name."
                };
            }

            if (exportAreas == null || exportAreas.Count == 0)
            {
                return new
                {
                    success = false,
                    message = "At least one export area must be selected (Insights / Alignment / Documentation / Unused)."
                };
            }

            List<ReportModel> reports = GlobalHandler.GetReportByName(reportName);
            List<DatasetModel> datasets = GlobalHandler.GetModelByName(semanticModelName);

            if (reports.Count == 0)
            {
                return new
                {
                    success = false,
                    message = $"No report found with the name '{reportName}'."
                };
            }

            if (datasets.Count == 0)
            {
                return new
                {
                    success = false,
                    message = $"No semantic model found with the name '{semanticModelName}'."
                };
            }

            if (datasets.Count > 1)
            {
                string datasetInfos = string.Join("; ", datasets.Select(d =>
                    $"Server: {d.ServerName}, Database: {d.DbName}, ConnectionType: {d.ConnectionType}"));

                return new
                {
                    success = false,
                    message = $"More than one semantic model found with the name '{semanticModelName}'.",
                    models = datasetInfos
                };
            }

            if (reports.Count > 1)
            {
                string reportPaths = string.Join(", ", reports.Select(r => r.ReportPath));
                return new
                {
                    success = false,
                    message = $"More than one report found with the name '{reportName}'.",
                    reports = reportPaths
                };
            }

            try
            {
                await _exportService.ExportToExcel(reports[0], datasets[0], exportAreas);

                return new
                {
                    success = true,
                    message = $"Export completed successfully for report '{reportName}' and model '{semanticModelName}'.",
                    report = reportName,
                    model = semanticModelName,
                    exportAreas = exportAreas
                };
            }
            catch (ErrorDTO err)
            {
                return new
                {
                    success = false,
                    message = err.Message,
                    error = err
                };
            }
            catch (Exception ex)
            {
                return new
                {
                    success = false,
                    message = "An error occurred while exporting to Excel.",
                    error = ex.Message
                };
            }
        }
    }
}
