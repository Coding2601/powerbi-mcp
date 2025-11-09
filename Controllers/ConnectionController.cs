using Microsoft.AspNetCore.Mvc;
using PowerBI_MCP.DTO;
using PowerBI_MCP.Interfaces;
using ModelContextProtocol.Server;
using System.ComponentModel;

namespace PowerBI_MCP.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    [McpServerToolType] // ✅ Marks this class as a tool container for MCP
    public class ConnectionController : ControllerBase
    {
        private readonly IConnectionService _connectionService;

        public ConnectionController(IConnectionService connectionService)
        {
            _connectionService = connectionService;
        }

        // --------------------------------------
        // ✅ HTTP endpoint - Connect to report
        // --------------------------------------
        [HttpPost]
        public IActionResult ConnectReport([FromBody] ReportConnectionDTO args)
        {
            if (args == null)
                return BadRequest("Report name and its path are required.");
            else if (string.IsNullOrEmpty(args.ReportName) || string.IsNullOrEmpty(args.ReportPath))
                return BadRequest("Report name and its path cannot be empty.");

            try
            {
                _connectionService.ConnectReport(args.ReportName, args.ReportPath);
                return Ok("Report connection established successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while connecting to the report: {ex.Message}");
            }
        }

        // --------------------------------------
        // ✅ HTTP endpoint - Connect to SASS model
        // --------------------------------------
        [HttpPost]
        public IActionResult ConnectToSASSModel([FromBody] ModelConnectionDTO args)
        {
            if (args == null)
                return BadRequest("Model name and its path are required.");
            else if (string.IsNullOrEmpty(args.ModelName))
                return BadRequest("Model name cannot be empty.");

            try
            {
                var result = _connectionService.ConnectToSASSModel(args.ModelName);
                if (!result)
                    return StatusCode(500, "Failed to connect to the specified model.");

                return Ok(new
                {
                    success = result,
                    message = "Connected to SASS model successfully.",
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while connecting to the model: {ex.Message}");
            }
        }

        [McpServerTool]
        [Description("Connects to a Power BI report via MCP.")]
        public object Mcp_ConnectReport(
            [Description("Name of the Power BI report")] string reportName, 
            [Description("File system path to the report")] string reportPath)
        {
            try
            {
                _connectionService.ConnectReport(reportName, reportPath);
                return new
                {
                    success = true,
                    message = $"Successfully connected to report '{reportName}' at '{reportPath}'.",
                    reportName = reportName,
                    reportPath = reportPath
                };
            }
            catch (Exception ex)
            {
                return new
                {
                    success = false,
                    message = $"Error connecting to report: {ex.Message}",
                    error = ex.Message
                };
            }
        }

        [McpServerTool]
        [Description("Connects to a SASS model via MCP.")]
        public object Mcp_ConnectToSASSModel(
            [Description("Name of the SASS model to connect to")] string modelName)
        {
            try
            {
                var result = _connectionService.ConnectToSASSModel(modelName);
                return new
                {
                    success = result,
                    message = result
                        ? $"Successfully connected to SASS model '{modelName}'."
                        : $"Failed to connect to SASS model '{modelName}'.",
                    modelName = modelName
                };
            }
            catch (Exception ex)
            {
                return new
                {
                    success = false,
                    message = $"Error connecting to SASS model: {ex.Message}",
                    error = ex.Message
                };
            }
        }
    }
}
