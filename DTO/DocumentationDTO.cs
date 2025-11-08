namespace PowerBI_MCP.DTO
{
    public class ReportDocumentation : ICloneable
    {
        public string WorkspaceId { get; set; } = "";

        public string ReportId { get; set; } = "";

        /// <summary>
        /// Gets or sets the name of the report theme.
        /// </summary>
        public string ThemeName { get; set; } = "";

        /// <summary>
        /// Gets or sets the theme JSON string.
        /// </summary>
        public string Theme { get; set; } = "{}";

        /// <summary>
        /// Gets or sets the list of measures in the report.
        /// </summary>
        public List<MeasureSummary> Measures { get; set; } = [];

        /// <summary>
        /// Gets or sets the list of visuals in the report.
        /// </summary>
        public List<VisualSummary> VisualList { get; set; } = [];

        /// <summary>
        /// Gets or sets the UI settings for the report.
        /// </summary>
        public ReportUISettingsModel ReportUISettings { get; set; } = new();

        /// <summary>
        /// Gets or sets the dictionary of visual groups.
        /// </summary>
        public Dictionary<string, VisualGroup> visualGroups { get; set; } = [];

        /// <summary>
        /// Gets or sets all used font families in the report.
        /// </summary>
        public Dictionary<string, List<Dictionary<string, string>>> AllUsedFontFamilies { get; set; } = [];

        /// <summary>
        /// Gets or sets the dictionary mapping visual IDs to their data.
        /// </summary>
        public Dictionary<string, VisualSummary> VisualDataDictionary { get; set; } = [];

        /// <summary>
        /// Gets or sets the dictionary of columns used in the report.
        /// </summary>
        public Dictionary<string, bool> ColumnDictionary { get; set; } = [];

        /// <summary>
        /// Gets or sets the dictionary of measures used in the report.
        /// </summary>
        public Dictionary<string, bool> MeasureDictionary { get; set; } = [];

        /// <summary>
        /// Gets or sets the summary data for the report.
        /// </summary>
        public List<ReportSummary> ReportSummary { get; set; } = [];

        /// <summary>
        /// Gets or sets the UI formatting settings for the report.
        /// </summary>
        public ReportUIFormattingModel ReportUIFormatting { get; set; } = new();

        /// <summary>
        /// Gets or sets the summary for each page in the report.
        /// </summary>
        public List<PageSummary> PageSummary { get; set; } = [];

        /// <summary>
        /// Gets or sets the dictionary mapping page names to their summary data.
        /// </summary>
        public Dictionary<string, PageSummary> PageDictionary { get; set; } = [];

        /// <summary>
        /// Gets or sets the list of bookmarks in the report.
        /// </summary>
        public List<BookmarkModel> BookmarkList { get; set; } = [];

        public ReportDefinitionFile[] ReportDefinitionFiles { get; set; } = [];
        public string DefinitionPbirBase64 { get; set; } = "";
        public string PlatformBase64 { get; set; } = "";

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
    public class ReportDefinitionFile
    {
        public string RelativePath { get; set; } = "";
        public string Content { get; set; } = ""; // actual JSON text
    }
    public class ModelDocumentation : ICloneable
    {
        public string WorkspaceId { get; set; } = "";
        public string ModelId { get; set; } = "";

        /// <summary>
        /// Gets or sets the summary information for the model.
        /// </summary>
        public List<SemanticModelSummary> Summary { get; set; } = [];

        /// <summary>
        /// Gets or sets the list of parameters in the model.
        /// </summary>
        public List<ParameterSummary> Parameters { get; set; } = [];

        /// <summary>
        /// Gets or sets the list of tables in the model.
        /// </summary>
        public List<TableSummary> Tables { get; set; } = [];

        /// <summary>
        /// Gets or sets the list of measures in the model.
        /// </summary>
        public List<MeasureSummary> Measures { get; set; } = [];

        /// <summary>
        /// Gets or sets the list of relationships in the model.
        /// </summary>
        public List<RelationshipSummary> Relationships { get; set; } = [];

        /// <summary>
        /// Gets or sets the list of security roles in the model.
        /// </summary>
        public List<RolesSummary> SecurityRoles { get; set; } = [];

        /// <summary>
        /// Gets or sets the list of columns in the model.
        /// </summary>
        public List<ColumnSummary> Columns { get; set; } = [];

        public List<HierarchySummary> Hierarchies { get; set; } = [];

        /// <summary>
        /// Gets or sets the dictionary of tables involved in relationships.
        /// </summary>
        public Dictionary<string, bool> TablesInRelationship { get; set; } = new();

        /// <summary>
        /// Gets or sets the dictionary of columns involved in relationships.
        /// </summary>
        public Dictionary<string, bool> ColumnsInRelationship { get; set; } = new();

        /// <summary>
        /// Stores the original model.bim JSON string used to build this documentation.
        /// </summary>
        public string OriginalModelBimJson { get; set; } = "";

        /// <summary>
        /// Stores the definition.pbism file content as base64 encoded string.
        /// </summary>
        public string DefinitionPbismBase64 { get; set; } = "";

        /// <summary>
        /// Stores the .platform file content as base64 encoded string.
        /// </summary>
        public string PlatformBase64 { get; set; } = "";

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}