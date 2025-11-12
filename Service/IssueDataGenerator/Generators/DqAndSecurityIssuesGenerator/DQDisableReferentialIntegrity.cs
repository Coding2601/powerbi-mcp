using PowerBI_MCP.DTO;

namespace PowerBI_MCP.Service.IssueDataGenerator.Generators.DqAndSecurityIssuesGenerator
{
    public class DQDisableReferentialIntegrity : IIssueDataGenerator
    {
        public SingleIssueRuleData GetData(IssueContext issueContext)
        {
            var rels = issueContext.ModelDocumentation.Relationships;
            var disabledReferentialIntegrety = rels.Where(rel =>
            {
                bool IsDual(string? mode) => string.Equals(mode?.Trim(), "Dual", StringComparison.OrdinalIgnoreCase);
                bool IsDirectQuery(string? mode) => string.Equals(mode?.Trim(), "DirectQuery", StringComparison.OrdinalIgnoreCase);
                return (IsDual(rel.FromMode) && IsDirectQuery(rel.ToMode)) || (IsDirectQuery(rel.FromMode) && IsDual(rel.ToMode)) ||  (IsDirectQuery(rel.FromMode) && IsDirectQuery(rel.ToMode));
            }).Cast<object>().ToList();
            return new SingleIssueRuleData()
            {
                Issues = disabledReferentialIntegrety ?? [],
                MaxIssuable = rels.Count
            };
        }
    }
}