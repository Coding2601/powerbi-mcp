using Microsoft.AspNetCore.Mvc;
using PowerBI_MCP.DTO;
using PowerBI_MCP.Handlers;
using PowerBI_MCP.Interfaces;

namespace PowerBI_MCP.Controllers
{
    [ApiController]
    [Route("/api/[controller]/[action]")]
    public class DocumentationController : ControllerBase
    {
        private readonly IDocumentationService _docService;
        public DocumentationController(IDocumentationService docService)
        {
            _docService = docService;
        }
        [HttpGet]
        public IActionResult GetReportDocumentation([FromQuery] string reportName)
        {
            if (string.IsNullOrEmpty(reportName))
                return BadRequest("Please input report name");
            try
            {
                var reportDoc = _docService.GetReportDoc(reportName);
                return Ok(reportDoc);
            }
            catch (Exception ex)
            {
                GlobalHandler.WriteCrashLog(ex.ToString());
                return StatusCode(500, $"error while fetching report documentaiton {ex.ToString()}");
            }
        }

        [HttpGet]
        public IActionResult GetModelDocumentation([FromQuery] string modelName)
        {
            if (string.IsNullOrEmpty(modelName))
                return BadRequest("Please input model name");
            try
            {
                var reportDoc = _docService.GetModelDoc(modelName);
                return Ok(reportDoc);
            }
            catch (Exception ex)
            {
                GlobalHandler.WriteCrashLog(ex.ToString());
                return StatusCode(500, $"error while fetching report documentaiton {ex.ToString()}");
            }
        }
    }
}