using System.Transactions;

namespace PowerBI_MCP.DTO
{
    public class ErrorDTO : Exception
    {
        public string? CustomErrorToShowUsers { get; set; }

        public ErrorDTO(string? customErrorToShowUsers = null)
            : base(customErrorToShowUsers) // Pass message to Exception base
        { 
            CustomErrorToShowUsers = customErrorToShowUsers;
        }
    }
}
