using System.Security.Claims;
using System.Threading.Tasks;
using PowerBI_MCP.DTO;
using PowerBI_MCP.Entities;
using PowerBI_MCP.Service;
using PowerBI_MCP.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PowerBI_MCP.Controllers
{
    [Route("/api/[controller]/[action]")]
    [Authorize]
    public class IssueController : ControllerBase
    {
        private readonly IIssueService _issueService;
        
        public IssueController(IIssueService issueService)
        {
            _issueService = issueService;
        }

        [HttpGet]
        public IActionResult GetIssuesSection()
        {
            try
            {
                var data = _issueService.GetIssueSection();

                if (data == null)
                {
                    return NotFound("No data found.");
                }
                return Ok(data);
            }
            catch (ErrorDTO err)
            {
                return CommonController.HandleException(err, User.FindFirst(ClaimTypes.Email)?.Value ?? "", err.Message, "Issue");
            }
            catch (Exception ex)
            {
                return CommonController.HandleException(ex, User.FindFirst(ClaimTypes.Email)?.Value ?? "", "An error occurred while fetching Issue data.", "Issue");
            }

        }

        [HttpGet]
        public IActionResult GetIssuesData([FromQuery] string artifactName, [FromQuery] string artifactType)
        {
            if (string.IsNullOrEmpty(artifactName) || string.IsNullOrEmpty(artifactType))
            {
                return BadRequest("artifactName and artifactType are required.");
            }
            try
            {
                // string userEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? "";
                var data = _issueService.GetData(artifactName, artifactType, "userEmail");

                if (data == null)
                {
                    return NotFound("No data found.");
                }
                return Ok(data);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return CommonController.HandleException(ex, User.FindFirst(ClaimTypes.Email)?.Value ?? "", "An error occurred while fetching Issue data.", "Issue");
            }
        }

        // [HttpGet]
        // public async Task<IActionResult> GetUnusedDetails([FromQuery] int executionId)
        // {
        //     if (executionId < 0) return BadRequest("Please provide a valid executionId.");

        //     try
        //     {
        //         string userEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? "";
        //         var data = await _issueService.GetUnusedFieldData(executionId, userEmail);

        //         if (data == null)
        //         {
        //             return NotFound("No data found.");
        //         }
        //         return Ok(data);
        //     }
        //     catch (ErrorDTO err)
        //     {
        //         return CommonController.HandleException(err, User.FindFirst(ClaimTypes.Email)?.Value ?? "", err.Message, "Issue");
        //     }
        //     catch (Exception ex)
        //     {
        //         return CommonController.HandleException(ex, User.FindFirst(ClaimTypes.Email)?.Value ?? "", "An error occurred while fetching Issue data.", "Issue");
        //     }
        // }

        [HttpGet]
        public IActionResult GetAlignmentIssues([FromQuery] string reportName, [FromQuery] string? spacing)
        {
            if (string.IsNullOrEmpty(reportName))
            {
                return BadRequest("Please provide valid reportName and spacing.");
            }
            try
            {
                // string userEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? "";
                var data =  _issueService.GetAlignmentIssues(reportName, spacing, "userEmail");

                if (data == null)
                {
                    return NotFound("No data found.");
                }
                return Ok(data);
            }
            catch (ErrorDTO err)
            {
                return CommonController.HandleException(err, User.FindFirst(ClaimTypes.Email)?.Value ?? "", err.Message, "Issue");
            }
            catch (Exception ex)
            {
                return CommonController.HandleException(ex, User.FindFirst(ClaimTypes.Email)?.Value ?? "", "An error occurred while fetching Issue data.", "Issue");
            }
        }

        // [HttpPost]
        // public IActionResult CheckFixedStatus([FromBody] CheckFixedStatusReqModel request)
        // {
        //     if (request == null || string.IsNullOrEmpty(request.WorkspaceId) || string.IsNullOrEmpty(request.ArtifactId) || request.IssueId <= 0)
        //     {
        //         return BadRequest("Please provide valid workspaceId, artifactId, and issueId.");
        //     }

        //     try
        //     {
        //         string userEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? "";
        //         var response = _issueService.CheckFixedStatus(request, userEmail);
        //         return Ok(response);
        //     }
        //     catch (ErrorDTO err)
        //     {
        //         return CommonController.HandleException(err, User.FindFirst(ClaimTypes.Email)?.Value ?? "", err.Message, "Issue");
        //     }
        //     catch (Exception ex)
        //     {
        //         return CommonController.HandleException(ex, User.FindFirst(ClaimTypes.Email)?.Value ?? "", "An error occurred while checking fixed status.", "Issue");
        //     }
        // }

        // [HttpPost]
        // public async Task<IActionResult> AddToIgnoreList([FromBody] AddToIgnoreListReqModel request)
        // {
        //     if (request == null || request.addToIgnoreLists == null)
        //     {
        //         return BadRequest("Please provide list of ignore rows");
        //     }
        //     try
        //     {
        //         await _issueService.AddToIgnoreList(request.addToIgnoreLists);
        //         return Ok();
        //     }
        //     catch (Exception ex)
        //     {
        //         return CommonController.HandleException(ex, User.FindFirst(ClaimTypes.Email)?.Value ?? "", "An error occurred while adding issues to ignore list.", "Issue");
        //     }
        // }

        // [HttpGet]
        // public IActionResult GetIgnoreItems([FromQuery] int insightId)
        // {
        //     try
        //     {
        //         var res = _issueService.GetIgnoreItems(insightId);
        //         return Ok(res);
        //     }
        //     catch (Exception ex)
        //     {
        //         return CommonController.HandleException(ex, User.FindFirst(ClaimTypes.Email)?.Value ?? "", "An error occurred while adding issues to ignore list.", "Issue");
        //     }
        // }

        // [HttpDelete]
        // public async Task<IActionResult> DeleteFromIgnoreList([FromQuery] List<int> request)
        // {
        //     if (request == null || request.Count == 0)
        //     {
        //         return BadRequest("Please provide data");
        //     }
        //     try
        //     {
        //         await _issueService.DeleteFromIgnoreList(request);
        //         return Ok();
        //     }
        //     catch (Exception ex)
        //     {
        //         return CommonController.HandleException(ex, User.FindFirst(ClaimTypes.Email)?.Value ?? "", "An error occurred while adding issues to ignore list.", "Issue");
        //     }
        // }
    }
}