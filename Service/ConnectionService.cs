using PowerBI_MCP.Interfaces;
using PowerBI_MCP.Handlers;
using PowerBI_MCP.DTO;
using PowerBI_MCP.Models;
using PowerBI_MCP.Utils;
using Microsoft.AnalysisServices.Tabular;
using System;
using System.IO;
using Newtonsoft.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace PowerBI_MCP.Service
{
    public class ConnectionService : IConnectionService
    {
        private readonly ConnectionHandler _connectionHandler;
        private readonly FileHandler _fileHandler;
        private readonly DocumentationHandler _documentationHandler;

        public ConnectionService(
            ConnectionHandler connectionHandler,
            FileHandler fileHandler,
            DocumentationHandler documentationHandler)
        {
            _connectionHandler = connectionHandler;
            _fileHandler = fileHandler;
            _documentationHandler = documentationHandler;
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
                if (_connectionHandler.CheckValidPBIR(extractionPath))
                    isPBIR = true;

                ArtifactModel.Reports.Add(new ReportModel
                {
                    ReportId = reportId,
                    ReportName = reportName,
                    ReportPath = extractionPath,
                    ReportType = isPBIR ? ReportType.PBIR : ReportType.PBIX
                });
                ReportDocumentation reportDoc = _documentationHandler.GenerateReportDocumentation(
                                                    isPBIR, extractionPath, reportId
                                                );
                string filePath = Path.Combine(AppConfig.duplicateReportZipDirectory, reportId + ".json");

                // Serialize object to JSON (with indented formatting)
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };

                string json = System.Text.Json.JsonSerializer.Serialize(reportDoc, options);

                // Save JSON to file asynchronously
                File.WriteAllTextAsync(filePath, json);

                Console.WriteLine($"JSON saved to: {filePath}");
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
                ModelDocumentation modelDoc = _documentationHandler.GenerateModelDocumentation(database);
                string extractionPath = Path.Combine(AppConfig.duplicateReportZipDirectory, modelId);
                Directory.CreateDirectory(extractionPath);
                string filePath = Path.Combine(extractionPath, modelId + ".json");

                // Serialize object to JSON (with indented formatting)
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };

                string json = System.Text.Json.JsonSerializer.Serialize(modelDoc, options);

                // Save JSON to file asynchronously
                File.WriteAllTextAsync(filePath, json);

                Console.WriteLine($"JSON saved to: {filePath}");
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