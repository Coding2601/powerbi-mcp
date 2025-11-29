namespace PowerBI_MCP.Interfaces
{
    public interface IDaxService
    {
        public string RunDaxQuery(string Query, string ModelName);
        public string RunMdxQuery(string Query, string ModelName);
    }
}