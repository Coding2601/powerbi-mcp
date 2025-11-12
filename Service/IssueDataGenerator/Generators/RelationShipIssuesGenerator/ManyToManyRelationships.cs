using PowerBI_MCP.DTO;

namespace PowerBI_MCP.Service.IssueDataGenerator.Generators.RelationShipIssuesGenerator
{
    public class ManyToManyRelationships : IIssueDataGenerator
    {
        public SingleIssueRuleData GetData(IssueContext issueContext)
        {
            var rels = issueContext.ModelDocumentation.Relationships;
            var manyToManyRels = rels.Where(x => x.FromCardinality == "Many" && x.ToCardinality == "Many").Cast<object>().ToList();
            return new SingleIssueRuleData()
            {
                Issues = manyToManyRels ?? [],
                MaxIssuable = rels.Count
            };
        }
    }
}