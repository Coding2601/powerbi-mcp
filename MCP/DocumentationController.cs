using System.ComponentModel;
using Microsoft.AspNetCore.Mvc;
using ModelContextProtocol.Server;
using PowerBI_MCP.DTO;
using PowerBI_MCP.Handlers;
using PowerBI_MCP.Interfaces;

namespace PowerBI_MCP.MCP
{
    /// <summary>
    /// Controller responsible for generating documentation for Power BI reports and semantic models.
    /// Provides endpoints to fetch structured documentation for given report or model names.
    /// </summary>
    [ApiController]
    [Route("api/[controller]/[action]")]
    [McpServerToolType]
    public class DocumentationController : ControllerBase
    {
        private readonly IDocumentationService _docService;

        public DocumentationController(IDocumentationService docService)
        {
            _docService = docService;
        }

        /// <summary>
        /// Retrieves the documentation of a specific Power BI report.
        /// </summary>
        /// <param name="reportName">The exact name of the Power BI report for which documentation is required.</param>
        /// <returns>
        /// A JSON object representing the report documentation if found.  
        /// Returns HTTP 400 if the report name is missing.  
        /// Returns HTTP 500 if an internal error occurs.
        /// </returns>
        [HttpGet]
        [McpServerTool]
        [Description("Fetches detailed documentation for a specific Power BI report by name.")]
        public IActionResult GetReportDocumentation([FromQuery, Description("Exact name of the Power BI report.")] string reportName)
        {
            if (string.IsNullOrEmpty(reportName))
                return BadRequest("Please input report name.");

            try
            {
                var reportDoc = _docService.GetReportDoc(reportName);
                return Ok(reportDoc);
            }
            catch (Exception ex)
            {
                GlobalHandler.WriteCrashLog(ex.ToString());
                return StatusCode(500, $"Error while fetching report documentation: {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieves the documentation of a specific Power BI semantic model (dataset).
        /// </summary>
        /// <param name="modelName">The exact name of the semantic model (dataset) for which documentation is required.</param>
        /// <returns>
        /// A JSON object representing the model documentation if found.  
        /// Returns HTTP 400 if the model name is missing.  
        /// Returns HTTP 500 if an internal error occurs.
        /// </returns>
        [HttpGet]
        [McpServerTool]
        [Description("Fetches detailed documentation for a specific Power BI semantic model (dataset) by name.")]
        public IActionResult GetModelDocumentation([FromQuery, Description("Exact name of the Power BI semantic model (dataset).")] string modelName)
        {
            if (string.IsNullOrEmpty(modelName))
                return BadRequest("Please input model name.");

            try
            {
                var modelDoc = _docService.GetModelDoc(modelName);
                return Ok(modelDoc);
            }
            catch (Exception ex)
            {
                GlobalHandler.WriteCrashLog(ex.ToString());
                return StatusCode(500, $"Error while fetching model documentation: {ex.Message}");
            }
        }
    }
}
