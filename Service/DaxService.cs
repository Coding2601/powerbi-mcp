using PowerBI_MCP.Interfaces;
using PowerBI_MCP.Repositories;
using PowerBI_MCP.Handlers;
using Microsoft.AnalysisServices.Tabular;
using Microsoft.AnalysisServices.AdomdClient;
using System;
using System.Data;
using System.Security.Principal;
using Newtonsoft.Json;

namespace PowerBI_MCP.Service
{
    public class DaxService : IDaxService
    {
        private readonly DaxHandler _daxHandler;
        public DaxService(DaxHandler daxHandler)
        {
            _daxHandler = daxHandler;
        }
        public string RunDaxQuery(string query, string modelName)
        {
            return _daxHandler.ExecuteQuery(query, modelName, "DAX");
        }

        public string RunMdxQuery(string query, string modelName)
        {
            return _daxHandler.ExecuteQuery(query, modelName, "MDX");
        }

        // private string ExecuteQuery(string query, string modelName, string queryType)
        // {
        //     string serverName = ModelRepo.Instance.GetServerName(modelName);
        //     List<string> dbs = new List<string>();
        //     if (string.IsNullOrEmpty(serverName))
        //         throw new Exception($"Model '{modelName}' is not connected to any server.");

        //     using (var tomServer = new Microsoft.AnalysisServices.Tabular.Server())
        //     {
        //         tomServer.Connect($"DataSource={serverName};");

        //         foreach (Database db in tomServer.Databases)
        //         {
        //             Console.WriteLine($"Database found: {db.Name}");
        //             dbs.Add(db.Name.ToString());
        //         }
        //     }

        //     foreach (var dbName in dbs)
        //     {
        //         string connString = $"Data Source={serverName};Initial Catalog={dbName};Integrated Security=SSPI;";
        //         try
        //         {
        //             using (var conn = new AdomdConnection(connString))
        //             {
        //                 conn.Open();
        //                 using (var cmd = new AdomdCommand(query, conn))
        //                 using (var reader = cmd.ExecuteReader())
        //                 {
        //                     var rows = new List<Dictionary<string, object>>();
        //                     while (reader.Read())
        //                     {
        //                         var row = new Dictionary<string, object>();
        //                         for (int i = 0; i < reader.FieldCount; i++)
        //                         {
        //                             row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
        //                         }
        //                         rows.Add(row);
        //                     }

        //                     return JsonConvert.SerializeObject(rows, Formatting.Indented);
        //                 }
        //             }
        //         }
        //         catch (AdomdErrorResponseException ex)
        //         {
        //             throw new Exception($"{queryType} query failed due to SSAS/XMLA error: {ex.Message}");
        //         }
        //         catch (Exception ex)
        //         {
        //             throw new Exception($"{queryType} query failed: {ex.Message}");
        //         }
        //     }
        //     return "";
        // }
    }
}
