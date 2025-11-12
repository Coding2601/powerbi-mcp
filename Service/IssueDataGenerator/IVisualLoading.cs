namespace PowerBI_MCP.Service
{
    public interface IVisualLoading
    {
        Dictionary<string, string> StartTrace(string xmlaEndpoint, string dbName, string accessToken, string traceName);
        void StopTrace(string traceName);
    }
}