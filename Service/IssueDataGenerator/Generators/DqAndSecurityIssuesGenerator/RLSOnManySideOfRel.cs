using PowerBI_MCP.DTO;
using static PowerBI_MCP.Handlers.ModelHandler;

namespace PowerBI_MCP.Service.IssueDataGenerator.Generators.DqAndSecurityIssuesGenerator
{
    public class RLSOnManySideOfRel : IIssueDataGenerator
    {
        public SingleIssueRuleData GetData(IssueContext issueContext)
        {
            var rels = issueContext.ModelDocumentation.Relationships;
            var weakRelation = rels.Where(x =>
            {
                string? fromTable = x.FromColumn?.TableName;
                string? toTable = x.ToColumn?.TableName;
                if (x.FromCardinality == "Many" && fromTable != null && issueContext.ModelDocumentation.SecurityRoles.Any(role => role.TableName == fromTable))
                    return true;
                if (x.ToCardinality == "Many" && toTable != null && issueContext.ModelDocumentation.SecurityRoles.Any(role => role.TableName == toTable))
                    return true;

                return false;
            }).Cast<object>().ToList();
            return new SingleIssueRuleData()
            {
                Issues = weakRelation ?? [],
                MaxIssuable = rels.Count
            };
        }
    }
}