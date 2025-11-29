namespace PowerBI_MCP.DTO{
    public class RunDaxDTO
    {
        public string Query { get; set; } = "";
        public string ModelName { get; set; } = "";
    }
    public class RunMdxDTO
    {
        public string Query { get; set; } = "";
        public string ModelName { get; set; } = "";
    }
}