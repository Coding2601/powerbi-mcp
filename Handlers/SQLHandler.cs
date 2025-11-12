using PowerBI_MCP.Utils;
using Microsoft.Data.Sqlite;
using Newtonsoft.Json.Linq;

namespace PowerBI_MCP.Handlers
{
    public static class SQLHandler
    {
        private static readonly string _connectionString = AppConfig.ConnectionString;

        public static JArray? RunReadQuery(string query)
        {
            try
            {
                JArray result = new();

                using var connection = new SqliteConnection(_connectionString);
                connection.Open();

                using var command = new SqliteCommand(query, connection);
                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    JObject row = new();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        try
                        {
                            row[reader.GetName(i)] = JToken.FromObject(reader.GetValue(i));
                        }
                        catch (Exception ex)
                        {
                            GlobalHandler.WriteCrashLog(ex.ToString(), null, "SQLite Read Function Failure");
                        }
                    }
                    result.Add(row);
                }

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed Query:\n" + query);
                GlobalHandler.WriteCrashLog(
                    "Failed Query:\n" + query + "\n" + ex.ToString(),
                    null,
                    "SQLite Read Function Failure"
                );
                return null;
            }
        }
    }
}
