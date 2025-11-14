using PowerBI_MCP.Utils;
using PowerBI_MCP.Handlers;
using PowerBI_MCP.Models;

namespace PowerBI_MCP.Repositories
{
    public sealed class ArtifactRepo
    {
        private static readonly Lazy<ArtifactRepo> _instance = new Lazy<ArtifactRepo>(() => new ArtifactRepo());
        public static ArtifactRepo Instance => _instance.Value;

        internal int SaveReportArtifact(string reportId, string reportName, string reportPath, bool isPBIR)
        {
            if (!SQLHandler.CheckTableExists(Constants.REPORT_TABLE_NAME))
            {
                string createTableQuery = $@"
                    CREATE TABLE {Constants.REPORT_TABLE_NAME} (
                        ReportId TEXT PRIMARY KEY,
                        ReportName TEXT NOT NULL,
                        ReportPath TEXT NOT NULL,
                        IsPBIR INTEGER NOT NULL
                    );";

                SQLHandler.RunCreateQuery(createTableQuery);
            }
            string insertQuery = $@"
                INSERT OR REPLACE INTO {Constants.REPORT_TABLE_NAME} (ReportId, ReportName, ReportPath, IsPBIR)
                VALUES ('{reportId}', '{reportName}', '{reportPath}', {(isPBIR ? 1 : 0)});";
            return SQLHandler.RunInsertQuery(insertQuery);
        }
        internal int SaveModelArtifact(string modelId, string modelName, ConnectionType connectionType, string serverName)
        {
            if (!SQLHandler.CheckTableExists(Constants.MODEL_TABLE_NAME))
            {
                string createTableQuery = $@"
                    CREATE TABLE {Constants.MODEL_TABLE_NAME} (
                        ModelId TEXT PRIMARY KEY,
                        ModelName TEXT NOT NULL,
                        ConnectionType TEXT NOT NULL,
                        ServerName TEXT NOT NULL
                    );";

                SQLHandler.RunCreateQuery(createTableQuery);
            }
            string insertQuery = $@"
                INSERT OR REPLACE INTO {Constants.MODEL_TABLE_NAME} (ModelId, ModelName, ConnectionType, ServerName)
                VALUES ('{modelId}', '{modelName}', '{connectionType}', '{serverName}');";
            return SQLHandler.RunInsertQuery(insertQuery);
        }
    }
}