using PowerBI_MCP.Utils;
using PowerBI_MCP.Handlers;
using PowerBI_MCP.Models;

namespace PowerBI_MCP.Repositories
{
    public sealed class ArtifactRepo
    {
        private static readonly Lazy<ArtifactRepo> _instance = new Lazy<ArtifactRepo>(() => new ArtifactRepo());
        public static ArtifactRepo Instance => _instance.Value;

        internal int SaveReportArtifact(string reportId, string reportName, string reportPath, string docPath, bool isPBIR)
        {
            if (!SQLHandler.CheckTableExists(Constants.REPORT_TABLE_NAME))
            {
                string createTableQuery = $@"
                    CREATE TABLE {Constants.REPORT_TABLE_NAME} (
                        ReportId TEXT PRIMARY KEY,
                        ReportName TEXT NOT NULL,
                        ReportPath TEXT NOT NULL,
                        DocPath TEXT NOT NULL,
                        IsPBIR INTEGER NOT NULL
                    );";

                SQLHandler.RunCreateQuery(createTableQuery);
            }
            string insertQuery = $@"
                INSERT OR REPLACE INTO {Constants.REPORT_TABLE_NAME} (ReportId, ReportName, ReportPath, DocPath, IsPBIR)
                VALUES ('{reportId}', '{reportName}', '{reportPath}', '{docPath}' ,{(isPBIR ? 1 : 0)});";
            return SQLHandler.RunInsertQuery(insertQuery);
        }
        internal int SaveModelArtifact(string modelId, string docPath, string modelName, ConnectionType connectionType, string serverName)
        {
            if (!SQLHandler.CheckTableExists(Constants.MODEL_TABLE_NAME))
            {
                string createTableQuery = $@"
                    CREATE TABLE {Constants.MODEL_TABLE_NAME} (
                        ModelId TEXT PRIMARY KEY,
                        ModelName TEXT NOT NULL,
                        ConnectionType TEXT NOT NULL,
                        ServerName TEXT NOT NULL,
                        DocPath TEXT NOT NULL
                    );";

                SQLHandler.RunCreateQuery(createTableQuery);
            }
            string insertQuery = $@"
                INSERT OR REPLACE INTO {Constants.MODEL_TABLE_NAME} (ModelId, DocPath, ModelName, ConnectionType, ServerName)
                VALUES ('{modelId}', '{docPath}', '{modelName}', '{connectionType}', '{serverName}');";
            return SQLHandler.RunInsertQuery(insertQuery);
        }
        internal bool CheckReportArtifactExists(string reportName)
        {
            if (!SQLHandler.CheckTableExists(Constants.REPORT_TABLE_NAME))
                return false;
            string selectQuery = $@"
                SELECT COUNT(*) FROM {Constants.REPORT_TABLE_NAME}
                WHERE ReportName = '{reportName}';";

            int count = SQLHandler.RunCountQuery(selectQuery);
            return count > 0;
        }
        internal bool CheckModelArtifactExists(string modelName)
        {
            if (!SQLHandler.CheckTableExists(Constants.MODEL_TABLE_NAME))
                return false;
            string selectQuery = $@"
                SELECT COUNT(*) FROM {Constants.MODEL_TABLE_NAME}
                WHERE ModelName = '{modelName}';";

            int count = SQLHandler.RunCountQuery(selectQuery);
            return count > 0;
        }
        internal bool CheckArtifactExistsById(string artifactId, bool isReport)
        {
            if (!SQLHandler.CheckTableExists(isReport ? Constants.REPORT_TABLE_NAME : Constants.MODEL_TABLE_NAME))
                return false;
            string tableName = isReport ? Constants.REPORT_TABLE_NAME : Constants.MODEL_TABLE_NAME;
            string idColumn = isReport ? "ReportId" : "ModelId";

            string selectQuery = $@"
                SELECT COUNT(*) FROM {tableName}
                WHERE {idColumn} = '{artifactId}';";

            int count = SQLHandler.RunCountQuery(selectQuery);
            return count > 0;
        }
    }
}