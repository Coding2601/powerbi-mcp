using PowerBI_MCP.DTO;
using PowerBI_MCP.Entities;
using PowerBI_MCP.Handlers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data.SQLite;
using PowerBI_MCP.Utils;

namespace PowerBI_MCP.Repositories
{
    public sealed class IssueMetadataRepo
    {
        private static readonly Lazy<IssueMetadataRepo> _instance = new Lazy<IssueMetadataRepo>(() => new IssueMetadataRepo());
        public static IssueMetadataRepo Instance => _instance.Value;

        internal List<IssueSection> GetIssueSection()
        {
            string query = @$"SELECT * from {Constants.INSIGHT_TABLE_NAME};";
            JArray? issuesMetadataJson = SQLHandler.RunReadQuery(query);
            if (issuesMetadataJson == null) return null;
            return JsonConvert.DeserializeObject<List<IssueSection>>(issuesMetadataJson?.ToString() ?? "[]") ?? [];
        }
        
        internal bool Check()
        {
                string connectionString = AppConfig.ConnectionString;
            Console.WriteLine("Connection String: " + connectionString);    
            using (var connection = new SQLiteConnection(connectionString))
            {
                    try
                    {
                        connection.Open();

                        if (connection.State == System.Data.ConnectionState.Open)
                        {
                            Console.WriteLine("Connection is open and working!");
                            return true;
                        }
                        else
                        {
                            Console.WriteLine("Connection failed or is closed.");
                            return false;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error: " + ex.Message);
                        return false;
                    }
            }
        }

        internal List<IssueRulesMetadata> GetAllIssuesMetadata()
        {
            string query = $"SELECT * FROM {Constants.INSIGHT_TABLE_NAME}";
            JArray issuesMetadataJson = SQLHandler.RunReadQuery(query) ?? [];
            return JsonConvert.DeserializeObject<List<IssueRulesMetadata>>(issuesMetadataJson.ToString()) ?? [];
        }

        internal IssueRulesMetadata? GetIssuesMetadataById(int id)
        {
            string query = @$"SELECT * FROM {Constants.INSIGHT_TABLE_NAME} where id = {id}";
            JArray? issuesMetadataJson = SQLHandler.RunReadQuery(query);
            if(issuesMetadataJson==null) return null;
            return JsonConvert.DeserializeObject<IssueRulesMetadata>(issuesMetadataJson?.First()?.ToString() ??"{}");
        }

        internal List<IssueRulesMetadata> GetUnusedFieldRelatedIssueMetadata()
        {
            string query = @$"SELECT * from  {Constants.INSIGHT_TABLE_NAME} WHERE dataGeneratorFunction In ('{IssueRuleFunctionMapper.COLUMNS_UNUSED}','{IssueRuleFunctionMapper.MEASURES_UNUSED}')";
            JArray issuesMetadataJson = SQLHandler.RunReadQuery(query) ?? [];
            return JsonConvert.DeserializeObject<List<IssueRulesMetadata>>(issuesMetadataJson.ToString() ?? "[]") ?? [];
        }

        internal List<IssueRulesMetadata> GetVisualAlignmentRelatedIssueMetadata()
        {
            string query = @$"SELECT * from {Constants.INSIGHT_TABLE_NAME} WHERE dataGeneratorFunction In ('{IssueRuleFunctionMapper.VERTICAL_VISUAL_SPACING}','{IssueRuleFunctionMapper.HORIZONTAL_VISUAL_SPACING}', '{IssueRuleFunctionMapper.VERTICAL_ALIGNMENT}','{IssueRuleFunctionMapper.HORIZONTAL_ALIGNMENT}', '{IssueRuleFunctionMapper.TOP_SIDEBAR_MARGIN}', '{IssueRuleFunctionMapper.BOTTOM_SIDEBAR_MARGIN}', '{IssueRuleFunctionMapper.LEFT_SIDEBAR_MARGIN}', '{IssueRuleFunctionMapper.RIGHT_SIDEBAR_MARGIN}')";
            JArray issuesMetadataJson = SQLHandler.RunReadQuery(query) ?? [];
            return JsonConvert.DeserializeObject<List<IssueRulesMetadata>>(issuesMetadataJson.ToString() ?? "[]") ?? [];
        }
    }
}