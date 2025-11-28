using System.Security.Claims;
using System.Threading.Tasks;
using PowerBI_MCP.DTO;
using PowerBI_MCP.Entities;
using PowerBI_MCP.Service;
using PowerBI_MCP.Controllers;
using PowerBI_MCP.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModelContextProtocol.Server;
using System.ComponentModel;

namespace PowerBI_MCP.MCP
{
    [Route("/api/[controller]/[action]")]
    [Authorize]
    [ApiController]
    [McpServerToolType]
    public class IssueController : ControllerBase
    {
        private readonly IIssueService _issueService;

        public IssueController(IIssueService issueService)
        {
            _issueService = issueService;
        }

        /// <summary>
        /// Fetch unused field details for a given semantic model and report.
        /// </summary>
        [HttpGet]
        [McpServerTool]
        [Description("Fetches unused field details for the given semantic model and report.")]
        public async Task<IActionResult> MCP_GetUnusedDetails(
            [FromQuery, Description("Name of the semantic model.")] string modelName, 
            [FromQuery, Description("Name of the report.")] string reportName)
        {
            if (string.IsNullOrEmpty(modelName)) return BadRequest("Please provide a valid model name.");
            if (string.IsNullOrEmpty(reportName)) return BadRequest("Please provide a valid report name.");

            try
            {
                Console.WriteLine("Fetching unused details for model: " + modelName + ", report: " + reportName);
                var data = await _issueService.GetUnusedFieldData(modelName, reportName);

                if (data == null) return NotFound("No data found.");

                return Ok(data);
            }
            catch (ErrorDTO err)
            {
                return CommonController.HandleException(err,
                    User.FindFirst(ClaimTypes.Email)?.Value ?? "",
                    err.Message,
                    "Issue");
            }
            catch (Exception ex)
            {
                return CommonController.HandleException(ex,
                    User.FindFirst(ClaimTypes.Email)?.Value ?? "",
                    "An error occurred while fetching Issue data.",
                    "Issue");
            }
        }

        /// <summary>
        /// Retrieves a list of all issue sections.
        /// </summary>
        // [HttpGet]
        // [McpServerTool]
        // [Description("Fetches all issue sections available for Power BI assets.")]
        // public IActionResult GetIssuesSection()
        // {
        //     try
        //     {
        //         var data = _issueService.GetIssueSection();
        //         if (data == null) return NotFound("No data found.");
        //         return Ok(data);
        //     }
        //     catch (ErrorDTO err)
        //     {
        //         return CommonController.HandleException(err,
        //             User.FindFirst(ClaimTypes.Email)?.Value ?? "",
        //             err.Message,
        //             "Issue");
        //     }
        //     catch (Exception ex)
        //     {
        //         return CommonController.HandleException(ex,
        //             User.FindFirst(ClaimTypes.Email)?.Value ?? "",
        //             "An error occurred while fetching Issue data.",
        //             "Issue");
        //     }
        // }

        /// <summary>
        /// Retrieves issue data for a specific artifact (model or report).
        /// </summary>
        [HttpGet]
        [McpServerTool]
        [Description("Fetches detailed issue data for the specified artifact name and type.")]
        public IActionResult MCP_GetIssuesData(
            [FromQuery, Description("Name of the artifact (report/model).")] string artifactName,
            [FromQuery, Description("Type of the artifact (Report/Model).")] string artifactType)
        {
            if (string.IsNullOrEmpty(artifactName) || string.IsNullOrEmpty(artifactType))
                return BadRequest("artifactName and artifactType are required.");

            try
            {
                var data = _issueService.GetData(artifactName, artifactType, "userEmail");
                if (data == null) return NotFound("No data found.");
                return Ok(data);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return CommonController.HandleException(ex,
                    User.FindFirst(ClaimTypes.Email)?.Value ?? "",
                    "An error occurred while fetching Issue data.",
                    "Issue");
            }
        }

        /// <summary>
        /// Retrieves layout alignment issues from a Power BI report.
        /// </summary>
        [HttpGet]
        [McpServerTool]
        [Description("Fetches layout alignment issues in a Power BI report.")]
        public IActionResult MCP_GetAlignmentIssues(
            [FromQuery, Description("Name of the Power BI report.")] string reportName,
            [FromQuery, Description("Optional spacing threshold for detecting alignment issues.")] string? spacing)
        {
            if (string.IsNullOrEmpty(reportName))
                return BadRequest("Please provide valid reportName and spacing.");

            try
            {
                var data = _issueService.GetAlignmentIssues(reportName, spacing, "userEmail");
                if (data == null) return NotFound("No data found.");
                return Ok(data);
            }
            catch (ErrorDTO err)
            {
                return CommonController.HandleException(err,
                    User.FindFirst(ClaimTypes.Email)?.Value ?? "",
                    err.Message,
                    "Issue");
            }
            catch (Exception ex)
            {
                return CommonController.HandleException(ex,
                    User.FindFirst(ClaimTypes.Email)?.Value ?? "",
                    "An error occurred while fetching Issue data.",
                    "Issue");
            }
        }
    }
}
