using PowerBI_MCP.DTO;
using PowerBI_MCP.Interfaces;
using PowerBI_MCP.Service;
using Microsoft.AspNetCore.Mvc;

namespace PowerBI_MCP.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class QueryController : ControllerBase
    {
        private readonly IDaxService _daxService;
        public QueryController(IDaxService daxService)
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
    
        [HttpPost]
        public IActionResult SaveDaxQuery(SaveDaxDTO saveDaxRequest)
        {
            if (saveDaxRequest == null)
            {
                return BadRequest("Invalid request data.");
            }
            if (string.IsNullOrEmpty(saveDaxRequest.Query) || string.IsNullOrEmpty(saveDaxRequest.QueryName))
            {
                return BadRequest("Query and QueryName cannot be empty.");
            }
            var result = _daxService.SaveDaxQuery(saveDaxRequest.QueryName, saveDaxRequest.Query);
            return Ok(result);
        }
    }
}