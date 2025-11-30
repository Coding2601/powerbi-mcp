using System.Xml;
using Microsoft.AnalysisServices.Tabular;
using Microsoft.AnalysisServices.AdomdClient;
using System;
using System.Data;
using System.Security.Principal;
using Newtonsoft.Json;
using PowerBI_MCP.Repositories;

namespace PowerBI_MCP.Handlers
{
    public class DaxHandler
    {
        public string SaveDaxQueryToFile(string queryName, string query)
        {
            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string saveDirectory = Path.Combine(documentsPath, "PowerBI-MCP", "SavedQueries");
            
            if (!Directory.Exists(saveDirectory))
                Directory.CreateDirectory(saveDirectory);

            string sanitizedQueryName = string.Join("_", queryName.Split(Path.GetInvalidFileNameChars()));
            string filePath = Path.Combine(saveDirectory, $"{sanitizedQueryName}.dax");

            File.WriteAllText(filePath, query);
            return $"Query saved successfully at: {filePath}";
        }
        public string ExecuteQuery(string query, string modelName, string queryType)
        {
            string serverName = ModelRepo.Instance.GetServerName(modelName);
            List<string> dbs = new List<string>();
            if (string.IsNullOrEmpty(serverName))
                throw new Exception($"Model '{modelName}' is not connected to any server.");

            using (var tomServer = new Microsoft.AnalysisServices.Tabular.Server())
            {
                tomServer.Connect($"DataSource={serverName};");

                foreach (Database db in tomServer.Databases)
                {
                    Console.WriteLine($"Database found: {db.Name}");
                    dbs.Add(db.Name.ToString());
                }
            }

            foreach (var dbName in dbs)
            {
                string connString = $"Data Source={serverName};Initial Catalog={dbName};Integrated Security=SSPI;";
                try
                {
                    using (AdomdConnection conn = new AdomdConnection(connString))
                    {
                        conn.Open();
                        var rows = new List<Dictionary<string, object>>();
                        using (AdomdCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = query;
                            using (AdomdDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    var row = new Dictionary<string, object>();
                                    for (int i = 0; i < reader.FieldCount; i++)
                                    {
                                        row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                                    }
                                    rows.Add(row);
                                }
                            }
                        }                        
                        return JsonConvert.SerializeObject(rows, Newtonsoft.Json.Formatting.Indented);
                    }
                }
                catch (AdomdErrorResponseException ex)
                {
                    throw new Exception($"{queryType} query failed due to SSAS/XMLA error: {ex.Message}");
                }
                catch (Exception ex)
                {
                    throw new Exception($"{queryType} query failed: {ex.Message}");
                }
            }
            return "";
        }
    }
}