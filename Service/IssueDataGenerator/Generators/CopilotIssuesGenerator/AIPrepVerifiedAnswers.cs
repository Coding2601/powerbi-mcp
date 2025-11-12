using PowerBI_MCP.DTO;
using PowerBI_MCP.Handlers;
using Microsoft.AnalysisServices.Tabular;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PowerBI_MCP.Service.IssueDataGenerator.Generators.CopilotIssuesGenerator
{
    public class AIPrepVerifiedAnswers : IIssueDataGenerator
    {
        public SingleIssueRuleData GetData(IssueContext issueContext)
        {
            var issues = new List<object>();
            //string tmdlDbFilePath = Path.Combine(issueContext.ModelDocumentation, "definition", "database.tmdl");
            //if (File.Exists(tmdlDbFilePath) == false) return issues;
            //string tmdlVerifiedAnswersFolder = Path.Combine(ConnectionModel.ModelAnalysisSourceValue, "VerifiedAnswers");
            //bool hasVerifiedAnswers = false;

            //if (Directory.Exists(tmdlVerifiedAnswersFolder))
            //{
            //    foreach (var file in Directory.GetFiles(tmdlVerifiedAnswersFolder, "*", SearchOption.AllDirectories))
            //    {
            //        System.Console.WriteLine(file);
            //        if (file.EndsWith("definition.json"))
            //        {
            //            JObject json = JObject.Parse(File.ReadAllText(file));
            //            System.Console.WriteLine(JsonConvert.SerializeObject(json).Substring(0, 300));
            //            if (json != null && json["triggerPrompts"] != null && json["triggerPrompts"]?.Count() > 0) hasVerifiedAnswers = true;
            //        }
            //    }
            //}

            //if (!hasVerifiedAnswers) issues.Add(new Dictionary<string, object> { { "Issue", "No Verified answers have been added in 'Prep data for AI' section." } });


            return new() { 
                Issues = issues, 
                MaxIssuable = issues.Count
            };
        }
    }
}