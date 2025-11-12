namespace PowerBI_MCP.Entities
{
    public static class IssueRuleSource
    {
        public const string SEMANTIC_MODEL = "SEMANTIC_MODEL";
        public const string REPORT = "REPORT";
        public const string SEMANTIC_MODEL_OR_REPORT = "SEMANTIC_MODEL_OR_REPORT";
        public const string SEMANTIC_MODEL_AND_REPORT = "SEMANTIC_MODEL_AND_REPORT";
        public const string SEMANTIC_MODEL_AND_ALL_REPORT = "SEMANTIC_MODEL_AND_ALL_REPORT";
        public static IEnumerable<string> All => new[] { SEMANTIC_MODEL, REPORT, SEMANTIC_MODEL_OR_REPORT, SEMANTIC_MODEL_AND_REPORT ,SEMANTIC_MODEL_AND_ALL_REPORT};
        public static object AsObject => new { SEMANTIC_MODEL = SEMANTIC_MODEL, REPORT = REPORT, SEMANTIC_MODEL_OR_REPORT = SEMANTIC_MODEL_OR_REPORT, SEMANTIC_MODEL_AND_REPORT = SEMANTIC_MODEL_AND_REPORT,SEMANTIC_MODEL_AND_ALL_REPORT=SEMANTIC_MODEL_AND_ALL_REPORT };
    }

    public static class IssueRuleCategories
    {
        public const string COMPLIANCE = "COMPLIANCE";
        public const string CPU = "CPU";
        public const string ALIGNMENT = "ALIGNMENT";
        public const string ACCESSIBILITY = "ACCESSIBILITY";
        public static IEnumerable<string> All => new[] { COMPLIANCE, CPU, ALIGNMENT, ACCESSIBILITY };
        public static object AsObject => new { COMPLIANCE = COMPLIANCE, CPU = CPU, ALIGNMENT = ALIGNMENT, ACCESSIBILITY = ACCESSIBILITY };
    }

    public static class IssueRuleType
    {
        public const string SYSTEM_RULE = "SYSTEM_RULE";
        public const string ADMIN_USER_RULE = "ADMIN_USER_RULE";
        public const string USER_RULES = "USER_RULES"; 
    }

    public class IssueRulesMetadata
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ResolutionSteps { get; set; } = string.Empty;
        public string ReferenceLinks { get; set; } = string.Empty;
        public bool Visible { get; set; }
        public bool Fixable { get; set; }
        public string SectionIds { get; set; } = string.Empty;
        public string Tags { get; set; } = string.Empty;
        public string IssueGroup { get; set; } = string.Empty;
        public string RuleType { get; set; } = IssueRuleType.SYSTEM_RULE;
        public string DataGeneratorFunction { get; set; } = string.Empty;
        public string ApplicableArtifactType { get; set; } = IssueRuleSource.SEMANTIC_MODEL;
        public string ConfigurationValue { get; set; } = "";
        public int IssueOrder { get; set;} = 0;
        public int IssueSubTypeOrder { get; set; } = 0;

        public string IssueBaseObject { get; set; } = "";
        public string CreatedBy { get; set; } = "system@example.com";
        public DateTime CreatedTime { get; set; } = DateTime.Now;
        public DateTime UpdatedTime { get; set; } = DateTime.Now;
        public string ModifiedBy { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }
}