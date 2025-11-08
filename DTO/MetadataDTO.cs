using PowerBI_MCP.Handlers;
using Microsoft.AnalysisServices.Tabular;

namespace PowerBI_MCP.DTO
{
    /// <summary>
    /// Represents UI settings for a report, including theme, filters, and personalization options.
    /// </summary>
    public class ReportUISettingsModel
    {
        public string? Theme { get; set; }
        public string? ReportId { get; set; }
        public string? ReportName { get; set; }
        public List<Dictionary<string, string>>? ReportFilters { get; set; }
        public string? ReportFiltersString { get; set; }
        public bool IsFilterPaneExpanded { get; set; }
        public string? UIAnalysisSource { get; set; }
        public bool IsPersonalizationEnabled { get; set; }
    }

    /// <summary>
    /// Represents a measure in a report or model, including metadata and DAX expression.
    /// </summary>
    public class MeasureSummary
    {
        public string? ReportId { get; set; }
        public string? ReportName { get; set; }
        public string? TableName { get; set; }
        public string? MeasureName { get; set; }
        public string? DataType { get; set; }
        public string? Visible { get; set; }
        public string? Expression { get; set; }
        public FormattedExpressionAndDependency? FormattedExpressionAndDependency { get; set; }

        public string? Folder { get; set; }
        public string? Format { get; set; }
        public string? Description { get; set; }
        public string? HasError { get; set; }
        public string? Origin { get; set; }
        public bool? IsUsed { get; set; } = false;
        public bool IsUsedInUnused { get; set; } = false; 
    }

    /// <summary>
    /// Return type for page detail queries, including summary and disabled interactions.
    /// </summary>
    public class GetPageDetailReturnType
    {
        public PageSummary? PageSummary { get; set; } = new();
        public Dictionary<string, List<string>>? DisabledEditInteractions { get; set; } = new();
    }

    /// <summary>
    /// Represents summary information for a report page.
    /// </summary>
    public class PageSummary
    {
        public string? ReportId { get; set; }
        public string? ReportName { get; set; }
        public int PageIndex { get; set; }
        public string? PageName { get; set; }
        public string? PageId { get; set; }
        public bool IsHidden { get; set; }
        public string? Size { get; set; }
        public string? PageType { get; set; }
        public List<Dictionary<string, string>>? PageFilters { get; set; }
        public string? PageFiltersString { get; set; }
        public string? PageTypeFilter { get; set; }
        public string? TotalVisuals { get; set; }
        public string? TotalStatic { get; set; }
        public string? TotalCustom { get; set; }
        public string? TotalHidden { get; set; }
        public string? TotalSlicers { get; set; }
        public string? TotalGrids { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public bool IsPersonalizationEnabled { get; set; }
        public string? PageField { get; set; }
        public int? TotalBookmarks { get; set; }
    }

    /// <summary>
    /// Return type for visual detail queries, including formatting and grouping.
    /// </summary>
    public class GetVisualDetailsReturnType
    {
        public VisualSummary VisualSummary { get; set; } = new();
        public Dictionary<string, List<Dictionary<string, string>>> AllUsedFontFamilies { get; set; } = new();
        public bool IsGroup { get; set; } = false;
        public VisualGroup VisualGroup { get; set; } = new();
        public ReportUIFormattingModel ReportUIFormattingModel { get; set; } = new();
    }

    /// <summary>
    /// Represents UI formatting settings for a report, including slicers, tables, and static components.
    /// </summary>
    public class ReportUIFormattingModel
    {
        public List<SlicersModel> SlicersList { get; set; } = [];
        public List<TableOrMatrixModel> TableOrMatrixList { get; set; } = [];
        public List<StaticComponentsModel> StaticComponentsList { get; set; } = [];
    }

    /// <summary>
    /// Represents a group of visuals in a report.
    /// </summary>
    public class VisualGroup
    {
        public string? GroupId { get; set; }
        public string? GroupName { get; set; }
        public string? ParentGroupId { get; set; }
        public string? X { get; set; }
        public string? Y { get; set; }
        public string? Z { get; set; }
    }

    public class AlignmentSummary
    {
        public string PageName { get; set; } = "";
        public string Visual1 { get; set; } = "";
        public string Visual2 { get; set; } = "";
        public string VisualId { get; set; } = "";
        public string VisualInformation { get; set; } = "";
        public string SpacingViolation { get; set; } = "";
    }

    public enum VisualUsedFieldType
    {
        TABLE, COLUMN, MEASURE, FIELD_PARAMETER, HIERARCHY, UNKNOWN
    }

    public class FormattedExpressionAndDependency
    {
        public string FormatedExpression { get; set; } = string.Empty;
        public ExpressionDependency[] Dependency { get; set; } = [];
    }

    public enum DependencyType
    {
        Column,
        Measure,
        Table,
        Hierarchy,
        Unknown
    }

    public class ExpressionDependency
    {
        public DependencyType Type { get; set; } = DependencyType.Column;

        public string TableName { get; set; } = "";
        public string FieldName { get; set; } = "";
    }

    public class VisualUsedFields
    {
        public string UsedIn { get; set; } = "";
        public VisualUsedFieldType Type { get; set; } = VisualUsedFieldType.COLUMN;
        public string TableName { get; set; } = "";
        public string Hierarchy { get; set; } = "";
        public string FieldName { get; set; } = "";
        public FilterFieldLocation FilterFieldLocation { get; set; } = FilterFieldLocation.VISUAL;
        public FilterType FilterType { get; set; } = FilterType.CATEGORICAL;
        public bool IsFieldParameterField { get; set; } = false;
    }

    /// <summary>
    /// Represents a visual in a report, including metadata, formatting, and filters.
    /// </summary>
    public class VisualSummary
    {
        public string? ReportId { get; set; }
        public string? ReportName { get; set; }
        public string? PageId { get; set; }
        public string? PageName { get; set; }
        public string? VisualId { get; set; }
        public string? VisualTitle { get; set; }
        public string? VisualSubTitle { get; set; }
        public string? OgVisualType { get; set; }
        public string? VisualType { get; set; }
        public bool IsHidden { get; set; }
        public string? TabOrder { get; set; }
        public string? ParentGroupId { get; set; }
        public string? Fields { get; set; }
        public string? Columns { get; set; }
        public string? Measures { get; set; }
        public string? DaxQuery { get; set; }
        public List<Dictionary<string, string>>? VisualFilters { get; set; }
        public string? VisualFiltersString { get; set; }
        public string? AltText { get; set; }
        public bool TooltipShow { get; set; }
        public string? TooltipType { get; set; }
        public string? TooltipSection { get; set; }
        public bool IsConditionalFormatted { get; set; }
        public List<ConditionalFormattingSummary>? ConditionalFormattingSummary { get; set; }
        public string? ConditionalFormattingSummaryString { get; set; }
        public List<string>? InteractionDisabledWith { get; set; }
        public string? InteractionDisabledWithString { get; set; }
        public bool IsCustom { get; set; }
        public bool IsStaticTextBox { get; set; }
        public string? ButtonText { get; set; }
        public bool IsPersonalizationEnabled { get; set; }
        public string? X { get; set; }
        public string? Y { get; set; }
        public string? Z { get; set; }
        public string? Width { get; set; }
        public string? Height { get; set; }
        public List<VisualTextModel>? VisualTexts { get; set; }

        // Set of field parameters used in the visual.
        public HashSet<string> FieldParameter = [];

        // Mapping of measures to columns for the visual.
        public HashSet<Dictionary<string, string>> Measure_Column_Mapping { get; set; } = new();

        // Formatting dictionaries for various visual elements.
        public Dictionary<string, string> WithoutTitleFormatting { get; set; } = [];
        public Dictionary<string, string> DataLabelFormatting { get; set; } = [];
        public Dictionary<string, string> BackgroundFormatting { get; set; } = [];
        public Dictionary<string, string> BorderFormatting { get; set; } = [];
        public Dictionary<string, string> LegendFormatting { get; set; } = [];
        public Dictionary<string, string> TooltipFormatting { get; set; } = [];

        // Extracted color information for the visual.
        public Dictionary<string, List<Dictionary<string, string>>> ExtractedColors { get; set; } = new();

        public bool HasVisualCalculation { get; set; } = false;
        public string CalculationExpression { get; set; } = "";

        public HashSet<VisualUsedFields> UsedFields { get; set; } = new();
    }

    /// <summary>
    /// Represents a text element within a visual.
    /// </summary>
    public class VisualTextModel
    {
        public string? Text { get; set; }
        public string? Type { get; set; }
    }

    /// <summary>
    /// Represents a summary of conditional formatting applied to a visual.
    /// </summary>
    public class ConditionalFormattingSummary
    {
        public string? AppliedOn { get; set; }
        public List<ConditionalFormattingDetail>? ConditionalFormattingDetails { get; set; }
    }

    /// <summary>
    /// Represents details of a conditional formatting rule.
    /// </summary>
    public class ConditionalFormattingDetail
    {
        public string? ColumnName { get; set; }
        public string? FormatName { get; set; }
        public string? FormatStyle { get; set; }
        public string? DependedColumnName { get; set; }
        public string? Summarization { get; set; }
        public GradientFormattingExpression? gradientFormattingExpression { get; set; }
        public RuleFormattingExpression? ruleFormattingExpression { get; set; }
    }

    /// <summary>
    /// Represents a gradient formatting expression for conditional formatting.
    /// </summary>
    public class GradientFormattingExpression
    {
        public string? minValue { get; set; }
        public string? midValue { get; set; }
        public string? maxValue { get; set; }
        public string? minColor { get; set; }
        public string? midColor { get; set; }
        public string? maxColor { get; set; }
    }

    /// <summary>
    /// Represents a rule-based formatting expression for conditional formatting.
    /// </summary>
    public class RuleFormattingExpression
    {
        public List<Rulex>? rules { get; set; }
        public RuleFormattingExpression()
        {
            rules = [];
        }
    }

    /// <summary>
    /// Represents a single rule in a rule-based formatting expression.
    /// </summary>
    public class Rulex    
    {
        public string? condition { get; set; }
        public string? value { get; set; }
    }

    /// <summary>
    /// Represents summary data for a report.
    /// </summary>
    public class ReportSummary
    {
        public string? ReportId { get; set; }
        public string? ReportName { get; set; }
        public string? Field { get; set; }
        public string? Value { get; set; }
    }

    /// <summary>
    /// Represents a bookmark in a report.
    /// </summary>
    public class BookmarkModel
    {
        public string? ReportId { get; set; }
        public string ReportName { get; set; } = "";
        public int Index { get; set; }
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string PageId { get; set; } = "";
        public string PageName { get; set; } = "";
        public string AffectedVisuals { get; set; } = "";
    }


    public class TableSource
    { 
        public string Name { get; set; } = "";
        public string Location { get; set; } = "";
    }
    /// <summary>
    /// Represents metadata for a table in a model.
    /// </summary>
    public class TableSummary
    {
        public string? TableName { get; set; }
        public string? TableType { get; set; }
        public string? Visible { get; set; }
        public string? StorageMode { get; set; }
        public string? PowerQueryGroup { get; set; }
        public int? PowerQueryTransformation { get; set; }
        public string? PowerQuerySteps { get; set; }
        public FormattedExpressionAndDependency? FormattedExpressionAndDependency { get; set; }
        public HashSet<string>? Source { get; set; }
        public HashSet<string>? SourcePath { get; set; }
        public HashSet<string>? AppliedSteps { get; set; }
        public HashSet<string>? TableUsedInQuery { get; set; }
        public string? IncrementalRefreshSetUp { get; set; }
        public string? IsIncludedInRefresh { get; set; }
        public string? IsStandaloneTable { get; set; }
        public string? IsSecurityApplied { get; set; }
        public string? DataCategory { get; set; }
        public string? Description { get; set; }
        public int? Columns { get; set; }
        public int? CalculatedColumns { get; set; }
        public string? Partitions { get; set; }
        public int? Hierarchies { get; set; }
        public int? Measures { get; set; }
        public string? SourceType { get; set; }
        public string? SourceDetails { get; set; }
        public string? SourceTables { get; set; }
        public string? CalculationGroup { get; set; }
        public bool? IsCalculatedTable { get; set; }
        public string? CalculatedTableExpression { get; set; }
        public List<Dictionary<string, string>>? TableAppliedSteps { get; set; }
        public int? TableAppliedStepsCount { get; set; }

        public string? RefreshPolicyExpression { get; set; }
        public FormattedExpressionAndDependency? FormattedRefreshPolicyExpressionAndDependency { get; set; }

        public List<FormattedExpressionAndDependency>? CalculationGroups { get; set; } 

        public List<TableSource> Sources { get; set; } = [];

        public bool? IsUsed { get; set; } = false;
        public bool IsUsedInUnused { get; set; } = false;
    }

    /// <summary>
    /// Represents a relationship between tables in a model.
    /// </summary>
    public class RelationshipSummary
    {
        public string? RelationshipName { get; set; }
        public string? IsActive { get; set; }
        public ColumnSummary? FromColumn { get; set; }
        public ColumnSummary? ToColumn { get; set; }
        public string? FromMode { get; set; }
        public string? ToMode { get; set; }
        public string? FromCardinality { get; set; }
        public string? ToCardinality { get; set; }
        public string? RelationshipCardinality { get; set; }
        public string? CrossFilterDirection { get; set; }
        public string? RelationshipType { get; set; }
        public string? IsReferentialIntegrityEnabled { get; set; }
        public string? IsSecurityFilterEnabled { get; set; }
    }

    /// <summary>
    /// Represents a security role in a model.
    /// </summary>
    public class RolesSummary
    {
        public string? RoleName { get; set; }
        public string? TableName { get; set; }
        public string? RowLevelSecurityFilter { get; set; }
        public string? ObjectLevelSecurityFilter { get; set; }
        public FormattedExpressionAndDependency? FormattedExpressionAndDependency { get; set; }

    }

    public class SortByColumnSummary
    {
        public string? TableName { get; set; }
        public string? ColumnName { get; set; }
        public string? Visible { get; set; }
        public string? DataType { get; set; }

    }

    /// <summary>
    /// Represents metadata for a column in a table.
    /// </summary
    public class ColumnSummary
    {
        public string? TableName { get; set; }
        public string? ColumnName { get; set; }
        public string? DataType { get; set; }
        public string? ColumnType { get; set; }
        public string? Visible { get; set; }
        public string? Expression { get; set; }
        public FormattedExpressionAndDependency? FormattedExpressionAndDependency { get; set; }

        public string? Folder { get; set; }
        public string? Format { get; set; }
        public string? DataCategory { get; set; }
        public string? Description { get; set; } 
        public SortByColumnSummary? SortByColumn { get; set; }

        public string? Summarization { get; set; }
        public string? HasError { get; set; }
        public string? IsKeyColumn { get; set; }
        public string? IsUniqueColumn { get; set; }
        public string? IsInvolvedInRelationship { get; set; }
        public string? IsSecurityApplied { get; set; }
        public bool? IsUsed { get; set; } = false;
        public bool IsUsedInUnused { get; set; } = false;

    }

    public class HierarchyLevel
    {
        public string Name { get; set; } = "";
        public string ColumnName { get; set; } = "";
    }
    /// <summary>
    /// Represents metadata for hierarchy 
    /// </summary>
    public class HierarchySummary
    {
        public string TableName { get; set; } = "";
        public string Hierarchy { get; set; } = "";
        public List<HierarchyLevel> Levels { get; set; } = [];
    }

    /// <summary>
    /// Represents a summary field and value for a model.
    /// </summary>
    public class SemanticModelSummary
    {
        public string? Field { get; set; }
        public string? Value { get; set; }
    }

    /// <summary>
    /// Represents a parameter in a model.
    /// </summary>
    public class ParameterSummary
    {
        public string? ParameterName { get; set; }
        public string? DataType { get; set; }
        public string? IsRequired { get; set; }
        public string? Value { get; set; }
    }

    /// <summary>
    /// DTO for relationship-related data, including columns and tables in relationships.
    /// </summary>
    public class RelationshipDTO
    {
        public RelationshipDTO() { }

        public Dictionary<string, bool> relationshipColumns { get; set; } = new Dictionary<string, bool>();
        public List<RelationshipSummary> relationshipList { get; set; } = new List<RelationshipSummary>();
        public Dictionary<string, bool> tableInRelationship { get; set; } = new Dictionary<string, bool>();
    }

    /// <summary>
    /// DTO for security role-related data, including tables and columns.
    /// </summary>
    public class SecurityRoleDTO
    {
        public SecurityRoleDTO() { }

        public List<string> securityTables { get; set; } = new List<string>();
        public List<RolesSummary> rolesDataList { get; set; } = new List<RolesSummary>();
        public List<string> securityColumns { get; set; } = new List<string>();
    }

    /// <summary>
    /// DTO for table, column, and measure metadata in a model.
    /// </summary>
    public class TableColumnMeasureDTO
    {
        public TableColumnMeasureDTO() { }
        public int dateTableCounter { get; set; } = 0;
        public int refreshFlag { get; set; } = 0;
        public List<string> modelType { get; set; } = new List<string>();
        public List<TableSummary> tableDataList { get; set; } = new List<TableSummary>();
        public List<MeasureSummary> measureDataList { get; set; } = new List<MeasureSummary>();
        public List<ColumnSummary> columnDataList { get; set; } = new List<ColumnSummary>();
        public List<HierarchySummary> Hierarchies { get; set; } = [];
    }
}