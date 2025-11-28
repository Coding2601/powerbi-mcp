using PowerBI_MCP.Utils;
using Microsoft.Data.Sqlite;
using Newtonsoft.Json.Linq;

namespace PowerBI_MCP.Handlers
{
    public static class SQLHandler
    {
        private static readonly string _connectionString = AppConfig.ConnectionString;

        public static int RunCountQuery(string query)
        {
            try
            {
                using var connection = new SqliteConnection(_connectionString);
                connection.Open();

                using var command = new SqliteCommand(query, connection);
                var result = command.ExecuteScalar();

                if (result != null && int.TryParse(result.ToString(), out int count))
                {
                    return count;
                }
                else
                {
                    return 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed Query:\n" + query);
                GlobalHandler.WriteCrashLog(
                    "Failed Query:\n" + query + "\n" + ex.ToString(),
                    null,
                    "SQLite Count Function Failure"
                );
                return -1;
            }
        }
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
        public static bool CheckTableExists(string tableName)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();

                string sql = "SELECT name FROM sqlite_master WHERE type='table' AND name=@tableName;";

                using (var command = new SqliteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@tableName", tableName);

                    var result = command.ExecuteScalar();

                    if (result != null)
                    {
                        Console.WriteLine($"✅ Table '{tableName}' exists.");
                        return true;
                    }
                    else
                    {
                        Console.WriteLine($"❌ Table '{tableName}' does not exist.");
                        return false;
                    }
                }
            }
        }
        public static int RunCreateQuery(string query)
        {
            try
            {
                using var connection = new SqliteConnection(_connectionString);
                connection.Open();

                using var command = new SqliteCommand(query, connection);
                int rowsAffected = command.ExecuteNonQuery();

                return rowsAffected;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed Query:\n" + query);
                GlobalHandler.WriteCrashLog(
                    "Failed Query:\n" + query + "\n" + ex.ToString(),
                    null,
                    "SQLite Create Function Failure"
                );
                return -1;
            }
        }
        public static int RunInsertQuery(string query)
        {
            try
            {
                using var connection = new SqliteConnection(_connectionString);
                connection.Open();

                using var command = new SqliteCommand(query, connection);
                int rowsAffected = command.ExecuteNonQuery();

                return rowsAffected;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed Query:\n" + query);
                GlobalHandler.WriteCrashLog(
                    "Failed Query:\n" + query + "\n" + ex.ToString(),
                    null,
                    "SQLite Insert Function Failure"
                );
                return -1;
            }
        }
    }
}
