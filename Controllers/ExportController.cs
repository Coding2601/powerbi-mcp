using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PowerBI_MCP.DTO;
using PowerBI_MCP.Interfaces;
using System.Security.Claims;
using PowerBI_MCP.Models;
using PowerBI_MCP.Handlers;

namespace PowerBI_MCP.Controllers
{
    [Route("/api/v1/[controller]/[action]")]
    [Authorize]
    public class ExportController : ControllerBase
    {
        private readonly IExportService _exportService;

        public ExportController(IExportService exportService)
        {
            _exportService = exportService;
        }

        [HttpPost]
        public async Task<IActionResult> ExportToExcel([FromBody] ExportRequestDTO arguments)
        {
            if (arguments == null)
            {
                return BadRequest("Please provide valid export details.");
            }
            if (arguments.ReportName == "" || arguments.SemanticModelName == "")
            {
                return BadRequest("Please provide valid report and semantic model names.");
            }
            if (arguments.ExportAreas == null || arguments.ExportAreas.Count == 0)
            {
                return BadRequest("Please select at least one export area. (Insights/Alignment/Documentation/Unused)");
            }
            List<ReportModel> reports = GlobalHandler.GetReportByName(arguments.ReportName);
            List<DatasetModel> datasets = GlobalHandler.GetModelByName(arguments.SemanticModelName);
            if (reports.Count == 0)
            {
                return BadRequest("No report found with the given name.");
            }
            if (datasets.Count == 0)
            {
                return BadRequest("No semantic model found with the given name.");
            }
            if (datasets.Count > 1)
            {
                string datasetInfos = string.Join("; ", datasets.Select(d => 
                    $"Server: {d.ServerName}, Database: {d.DbName}, ConnectionType: {d.ConnectionType}"));
                
                throw new Exception($"There are more than one semantic model with the name '{arguments.SemanticModelName}'. Models: {datasetInfos}");
            }
            if (reports.Count > 1)
            {
                string reportPaths = string.Join(", ", reports.Select(r => r.ReportPath));
                throw new Exception($"There are more than one report with the name '{arguments.ReportName}'. Report paths: {reportPaths}");
            }
            try
            {
                string userEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? "";
                await _exportService.ExportToExcel(reports[0], datasets[0], arguments.ExportAreas);
                return Ok();
            }
            catch (ErrorDTO err)
            {
                return CommonController.HandleException(err, User.FindFirst(ClaimTypes.Email)?.Value ?? "", err.Message, "Export");
            }
            catch (Exception ex)
            {
                return CommonController.HandleException(ex, User.FindFirst(ClaimTypes.Email)?.Value ?? "", "An error occurred while exporting data.", "Export");
            }
        }
    }
}