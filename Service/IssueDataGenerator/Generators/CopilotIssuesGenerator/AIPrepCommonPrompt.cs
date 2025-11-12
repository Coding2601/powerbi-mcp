using PowerBI_MCP.DTO;
using PowerBI_MCP.Handlers;
using Microsoft.AnalysisServices.Tabular;
using Newtonsoft.Json.Linq; 

namespace PowerBI_MCP.Service.IssueDataGenerator.Generators.CopilotIssuesGenerator
{
    public class AIPrepCommonPrompt : IIssueDataGenerator
    {
        public SingleIssueRuleData GetData(IssueContext issueContext)
        {
            var issues = new List<object>();
            bool hasCustomInstructions = false; 
            Database database = GlobalHandler.ConvertBimJsonToDatabase(issueContext.ModelDocumentation.OriginalModelBimJson);
            
            var model = database.Model;
            
            foreach (var cultureToken in model.Cultures)
            {
                var content = cultureToken.LinguisticMetadata.Content;
                if (string.IsNullOrEmpty(content))
                    continue;
                JObject cultureContentJson = JObject.Parse(content);
                if (cultureContentJson != null && cultureContentJson["CustomInstructions"] != null && cultureContentJson["CustomInstructions"]?.ToString() != "")
                    hasCustomInstructions = true;
            }

            if (!hasCustomInstructions) issues.Add(new Dictionary<string, object> { { "Issue", "No AI instructions have been added in 'Prep data for AI' section." } });

            return new() { Issues = issues ?? [], MaxIssuable = 1 };
        }
    }
}