namespace PowerBI_MCP.DTO
{
    public static class ArtifactTypes
    {
        public const string SEMANTIC_MODEL = "Semantic Model";
        public const string REPORT = "Report";
        public static IEnumerable<string> All => new[] { SEMANTIC_MODEL, REPORT };
        public static object AsObject => new { SEMANTIC_MODEL = SEMANTIC_MODEL, REPORT = REPORT };
    }
}