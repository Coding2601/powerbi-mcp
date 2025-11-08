using PowerBI_MCP.Interfaces;
using PowerBI_MCP.Handlers;
using PowerBI_MCP.DTO;
using PowerBI_MCP.Models;
using PowerBI_MCP.Utils;
using Microsoft.AnalysisServices.Tabular;

namespace PowerBI_MCP.Service
{
    public class ConnectionService : IConnectionService
    {
        private readonly ConnectionHandler _connectionHandler;
        private readonly FileHandler _fileHandler = new FileHandler();

        public ConnectionService(ConnectionHandler connectionHandler, FileHandler fileHandler)
        {
            _connectionHandler = connectionHandler;
            _fileHandler = fileHandler;
        }

        public bool ConnectReport(string reportName, string reportPath)
        {
            try
            {
                string fullpath = Path.Combine(reportPath, reportName);

                if (GlobalHandler.IsArtifactAvailable(reportName, fullpath))
                    return true;

                if (!_fileHandler.CheckFileExists(fullpath))
                    throw new Exception("REPORT_FILE_NOT_FOUND");

                string reportId = "";
                while (!GlobalHandler.CheckArtifactExistsById(reportId) && reportId == "")
                    reportId = Guid.NewGuid().ToString();

                string extractionPath = Path.Combine(AppConfig.duplicateReportZipDirectory, reportId);
                Directory.CreateDirectory(extractionPath);

                _fileHandler.ExtractZipFile(fullpath, extractionPath);

                bool isPBIR = false;
                if (_connectionHandler.CheckValidPBIR(fullpath))
                    isPBIR = true;

                ArtifactModel.Reports.Add(new ReportModel
                {
                    ReportId = reportId,
                    ReportName = reportName,
                    ReportPath = fullpath,
                    ReportType = isPBIR ? ReportType.PBIR : ReportType.PBIX
                });
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        public bool ConnectToSASSModel(string modelName)
        {
            try
            {
                string server = _connectionHandler.GetServer(modelName);

                if(GlobalHandler.IsArtifactAvailable(server, ""))
                    return true;

                Microsoft.AnalysisServices.Server tabularServer = new();
                tabularServer.Connect(server);
                string? db;
                try
                {
                    db = tabularServer.Databases[0].ID;
                }
                catch (Exception ex)
                {
                    GlobalHandler.WriteCrashLog("Model not found or " + ex.ToString(), "Model Not Found");
                    throw new Exception("LIVE_CONNECTED_MODEL_NOT_FOUND");
                }
                if (db == null)
                    throw new Exception("LIVE_CONNECTED_MODEL_NOT_FOUND");
                
                string ConnectionString = "DataSource=" + server;
                Server server1 = new();
                server1.Connect(ConnectionString);
                Database database = server1.Databases.GetByName(db);

                string modelId = "";
                while (!GlobalHandler.CheckArtifactExistsById(modelId) && modelId == "")
                    modelId = Guid.NewGuid().ToString();
                ArtifactModel.Datasets.Add(new DatasetModel
                {
                    DatasetId = modelId,
                    DatasetName = modelName,
                    ConnectionType = ConnectionType.SASS,
                    ServerName = server,
                    DbName = db,
                    dbStatic = database
                });
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }
    }
}