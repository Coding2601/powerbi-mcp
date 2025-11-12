using PowerBI_MCP.DTO;

namespace PowerBI_MCP.Service.IssueDataGenerator.Generators.DqAndSecurityIssuesGenerator
{
    public class DQWeakRelationships : IIssueDataGenerator
    {
        public SingleIssueRuleData GetData(IssueContext issueContext)
        {
            var rels = issueContext.ModelDocumentation.Relationships;
            var weakRelation = rels.Where(rel =>
            {
                bool IsImport(string? mode) => string.Equals(mode?.Trim(), "Import", StringComparison.OrdinalIgnoreCase);
                bool IsDirectQuery(string? mode) => string.Equals(mode?.Trim(), "DirectQuery", StringComparison.OrdinalIgnoreCase);
                return (IsImport(rel.FromMode) && IsDirectQuery(rel.ToMode)) || (IsDirectQuery(rel.FromMode) && IsImport(rel.ToMode));
            }).Cast<object>().ToList();
            return new SingleIssueRuleData()
            {
                Issues = weakRelation ?? [],
                MaxIssuable = rels.Count
            };
        }
    }
}