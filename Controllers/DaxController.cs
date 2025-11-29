using PowerBI_MCP.DTO;
using PowerBI_MCP.Interfaces;
using PowerBI_MCP.Service;
using Microsoft.AspNetCore.Mvc;

namespace PowerBI_MCP.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class DaxController : ControllerBase
    {
        private readonly IDaxService _daxService;
        public DaxController(IDaxService daxService)
        {
            _daxService = daxService;
        }
        [HttpPost]
        public IActionResult RunDaxQuery(RunDaxDTO daxRequest)
        {
            if (daxRequest == null || string.IsNullOrEmpty(daxRequest.Query) || string.IsNullOrEmpty(daxRequest.ModelName))
            {
                return BadRequest("Invalid request data.");
            }
            var result = _daxService.RunDaxQuery(daxRequest.Query, daxRequest.ModelName);
            return Ok(result);
        }
        [HttpPost]
        public IActionResult RunMdxQuery(RunMdxDTO mdxRequest)
        {
            if (mdxRequest == null || string.IsNullOrEmpty(mdxRequest.Query) || string.IsNullOrEmpty(mdxRequest.ModelName))
            {
                return BadRequest("Invalid request data.");
            }
            var result = _daxService.RunMdxQuery(mdxRequest.Query, mdxRequest.ModelName);
            return Ok(result);
        }
    }
}