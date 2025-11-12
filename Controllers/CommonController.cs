using Microsoft.AspNetCore.Mvc;
using PowerBI_MCP.Handlers;


namespace PowerBI_MCP.Controllers
{
    public class CommonController : ControllerBase
    {
        public static IActionResult HandleException(Exception ex, string userEmail, string messageForUser = "An error occurred while processing your request.", string? exceptionCategory = null)
        {
            Console.WriteLine(ex.ToString());
            GlobalHandler.WriteCrashLog(ex.ToString(), userEmail, exceptionCategory);
            return new ObjectResult(new { message = messageForUser, error = ex.ToString() }) { StatusCode = 500 };
        }
    }

}