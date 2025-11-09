using Microsoft.AspNetCore.Mvc;
using PowerBI_MCP.DTO;
using PowerBI_MCP.Interfaces;
using ModelContextProtocol.Server;
using System.ComponentModel;

namespace PowerBI_MCP.MCP
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    [McpServerToolType]
    public class ConnectionController
    {
        private readonly IConnectionService _connectionService;

        public ConnectionController(IConnectionService connectionService)
        {
            _connectionService = connectionService;
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