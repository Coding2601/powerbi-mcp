namespace PowerBI_MCP.DTO
{
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