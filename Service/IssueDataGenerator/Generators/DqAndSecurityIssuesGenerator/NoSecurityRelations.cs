using PowerBI_MCP.DTO;

namespace PowerBI_MCP.Service.IssueDataGenerator.Generators.DqAndSecurityIssuesGenerator
{
    public class NoSecurityRelations : IIssueDataGenerator
    {
        public SingleIssueRuleData GetData(IssueContext issueContext)
        {
            var rels = issueContext.ModelDocumentation.Relationships;
            var noSecurityRelations = rels.Where(x =>
            {
                string? fromTable = x.FromColumn?.TableName;
                string? toTable = x.ToColumn?.TableName;
                return x.IsSecurityFilterEnabled == "OneDirection"
                    && x.CrossFilterDirection == "BothDirections"
                    && ((fromTable != null && issueContext.ModelDocumentation.TablesInRelationship.ContainsKey(fromTable)) || (toTable != null && issueContext.ModelDocumentation.TablesInRelationship.ContainsKey(toTable)));
            }).Cast<object>().ToList();
            return new SingleIssueRuleData()
            {
                Issues = noSecurityRelations ?? [],
                MaxIssuable = rels.Count
            };
        }
    }
}