using Microsoft.AspNetCore.Mvc;
using PowerBI_MCP.DTO;
using PowerBI_MCP.Interfaces;

namespace PowerBI_MCP.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class ConnectionController : ControllerBase
    {
        private readonly IConnectionService _connectionService;
        public ConnectionController(IConnectionService connectionService)
        {
           this._connectionService = connectionService;
        }
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
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while connecting to the report: {ex.Message}");
            }
        } 

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
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while connecting to the model: {ex.Message}");
            }
        } 
    }
}