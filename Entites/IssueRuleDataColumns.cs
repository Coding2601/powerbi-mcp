namespace PowerBI_MCP.Entities
{
    /// <summary>
    /// Represents a data column for an issue rule.
    /// </summary>
    public class IssueRuleDataColumns
    {
        public int Id { get; set; }
        public int IssueRuleId { get; set; }
        public string ColumnName { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public bool IsFixed { get; set; }
        public bool IsKey { get; set; }
        public bool IsFilterable { get; set; }
        public bool IsEditable { get; set; }
        public bool IsVisible { get; set; }
        public DateTime CreatedTime { get; set; } = DateTime.Now;
        public DateTime UpdatedTime { get; set; } = DateTime.Now;
        public string ModifiedBy { get; set; } = string.Empty;
        public bool IsActive { get; set; } = false;
    }
}