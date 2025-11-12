using PowerBI_MCP.DTO;

namespace PowerBI_MCP.Service.IssueDataGenerator.Generators.RelationShipIssuesGenerator
{
    public class BiDirectionalRelationships : IIssueDataGenerator
    {
        public SingleIssueRuleData GetData(IssueContext issueContext)
        {
            var rels = issueContext.ModelDocumentation.Relationships;
            var biDirectionalRels = rels.Where(x => x.CrossFilterDirection == "Both").Cast<object>().ToList();
            return new SingleIssueRuleData()
            {
                Issues = biDirectionalRels ?? [],
                MaxIssuable = rels.Count
            };
        }
    }
}