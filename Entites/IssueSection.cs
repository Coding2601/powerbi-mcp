namespace PowerBI_MCP.Entities
{
    public class IssueSection
    {
        public int Id { get; set; }
        public string Parent { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty; 
        public string Description {get;set;} =string.Empty;
        public bool IsContainIssue {get;set;} = false;
        public DateTime CreatedTime { get; set; } = DateTime.Now;
        public DateTime UpdatedTime { get; set; } = DateTime.Now;
        public string ModifiedBy { get; set; } = string.Empty;
    }
}