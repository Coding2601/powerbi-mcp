namespace PowerBI_MCP.Interfaces
{
    public interface IConnectionService
    {
        public bool ConnectReport(string reportName, string reportPath);
        public bool ConnectToSASSModel(string modelName);
    }
}