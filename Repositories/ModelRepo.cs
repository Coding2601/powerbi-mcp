using PowerBI_MCP.Utils;
using PowerBI_MCP.Handlers;
using PowerBI_MCP.Models;

namespace PowerBI_MCP.Repositories
{
    public sealed class ModelRepo
    {
        private static readonly Lazy<ModelRepo> _instance = new Lazy<ModelRepo>(() => new ModelRepo());
        public static ModelRepo Instance => _instance.Value;
        internal string GetServerName(string ModelName)
        {
            if (!SQLHandler.CheckTableExists(Constants.MODEL_TABLE_NAME))
            {
                return string.Empty;
            }
            string selectQuery = $@"
                SELECT ServerName FROM {Constants.MODEL_TABLE_NAME}
                WHERE ModelName = '{ModelName}';";

            return SQLHandler.RunSelectSingleValueQuery(selectQuery);
        }
    }
}