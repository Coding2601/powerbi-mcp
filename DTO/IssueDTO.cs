using PowerBI_MCP.Entities;

namespace PowerBI_MCP.DTO
{

    public class IssueRulesMetadataDTO : IssueRulesMetadata
    {
        public List<object> Data { get; set; } = [];
        public int IssueCount { get; set; } = 0;
        public int MaxIssuable { get; set; } = 0;
        public object? Error { get; set; }
    }
    public enum ColorBlindnessType
    {
        Deuteranopia,
        Protanopia,
        Tritanopia,
    }
    public class IssueContext
    {
        public ReportDocumentation ReportDocumentation { get; set; }
        public ModelDocumentation ModelDocumentation { get; set; }
        public Dictionary<string, Dictionary<string, string>> MeasureDetails { get; set; }
        public Dictionary<string, Dictionary<string, string>> CalculatedColumns { get; set; }
    }
    public static class IssueRuleFunctionMapper
    {
        //tables
        public const string TABLE_LOCAL_DATE_TIME = "TABLE_LOCAL_DATE_TIME";
        public const string TABLE_CALCULATED = "TABLE_CALCULATED";
        public const string TABLE_HIGH_POWER_QUERY_TRANSFORMATIONS = "TABLE_HIGH_POWER_QUERY_TRANSFORMATIONS";
        public const string TABLE_FACT_NOT_HIDDEN = "TABLE_FACT_NOT_HIDDEN";

        // columns
        public const string COLUMN_SYNTAX_ERROR = "COLUMN_SYNTAX_ERROR";
        public const string AVOID_CALCULATED_COLUMN = "AVOID_CALCULATED_COLUMN";
        public const string COLUMN_SUMMARIZE_BY_KEY_ID_COL = "COLUMN_SUMMARIZE_BY_KEY_ID_COL";
        public const string COLUMN_DATETIME_DATA_TYPE = "COLUMN_DATETIME_DATA_TYPE";
        public const string COLUMN_MISSING_DESCRIPTION = "COLUMN_MISSING_DESCRIPTION";
        public const string COLUMN_HIDE = "COLUMN_HIDE";
        public const string COLUMN_DUPLICATE = "COLUMN_DUPLICATE";

        // measures
        public const string MEASURE_WITHOUT_MEASURE_TABLE = "MEASURE_WITHOUT_MEASURE_TABLE";
        public const string MEASURE_SYNTAX_ERROR = "MEASURE_SYNTAX_ERROR";
        public const string MEASURE_MISSING_FOLDER = "MEASURE_MISSING_FOLDER";
        public const string MEASURE_MISSING_DESCRIPTION = "MEASURE_MISSING_DESCRIPTION";
        public const string MEASURE_DUPLICATE = "MEASURE_DUPLICATE";

        // dax expression
        public const string DAX_NON_RECOMMENDED_FUNCTION = "DAX_NON_RECOMMENDED_FUNCTION";
        public const string DAX_DIVIDE_DEFAULT = "DAX_DIVIDE_DEFAULT";
        public const string DAX_DIVIDE_TO_STATIC = "DAX_DIVIDE_TO_STATIC";
        public const string DAX_ISBLANK_CHECK = "DAX_ISBLANK_CHECK";
        public const string DAX_USED_SUMMARIZE_OR_GROUPBY_FUNC = "DAX_USED_SUMMARIZE_OR_GROUPBY_FUNC";
        public const string DAX_REPEATED_EXPRESSIONS = "DAX_REPEATED_EXPRESSIONS";
        public const string DAX_SUGGEST_VARIABLE = "DAX_SUGGEST_VARIABLE";

        // relationship
        public const string RELATIONSHIP_MANY_TO_MANY_CARDINALITY = "RELATIONSHIP_MANY_TO_MANY_CARDINALITY";
        public const string RELATIONSHIP_BIDIRECTIONAL_FILTERING = "RELATIONSHIP_BIDIRECTIONAL_FILTERING";
        public const string RELATIONSHIP_ONE_TO_ONE = "RELATIONSHIP_ONE_TO_ONE";
        public const string RELATIONSHIP_USE_NON_NUMERIC_FIELD = "RELATIONSHIP_USE_NON_NUMERIC_FIELD";

        // DQ
        public const string DQ_WEAK_RELATIONSHIP = "DQ_WEAK_RELATIONSHIP";
        public const string DQ_DISABLE_REFERENTIAL_INTEGRITY = "DQ_DISABLE_REFERENTIAL_INTEGRITY";

        // formatting
        public const string FORMATTING_NO_SEPARATOR_MEASURE = "MEASURE_LACK_COMMA_FORMATTING";
        public const string FORMATTING_NO_SEPARATOR_COLUMN = "COLUMN_LACK_COMMA_FORMATTING";
        public const string FORMATTING_INCONSISTENT_DECIMAL = "FORMATTING_INCONSISTENT_DECIMAL";
        public const string FORMATTING_INCONSISTENT_DATETIME = "FORMATTING_INCONSISTENT_DATETIME";

        // security
        public const string SECURITY_NO_SECURITY_RELATIONS = "SECURITY_NO_SECURITY_RELATIONS";
        public const string RLS_ON_MANY_SIDE_OF_REL = "RLS_ON_MANY_SIDE_OF_REL";

        // spell check
        public const string SPELL_TABLE_NAME = "SPELL_TABLE_NAME";
        public const string SPELL_COLUMN_NAME = "SPELL_COLUMN_NAME";
        public const string SPELL_MEASURE_NAME = "SPELL_MEASURE_NAME";
        public const string REPORT_THEME = "REPORT_THEME";
        public const string REPORT_MULTIPLE_FONT_FAMILY = "REPORT_MULTIPLE_FONT_FAMILY";
        public const string PAGE_INCONSISTENT_SIZE = "PAGE_INCONSISTENT_SIZE";
        public const string PAGE_VISUAL_MORE_THAN_10 = "PAGE_VISUAL_MORE_THAN_10";
        public const string PAGE_STATIC_MORE_THAN_10 = "PAGE_STATIC_MORE_THAN_10";
        public const string VISUAL_WITHOUT_TOOLTIP = "VISUAL_WITHOUT_TOOLTIP";
        public const string VISUAL_WITHOUT_ALT_TEXT = "VISUAL_WITHOUT_ALT_TEXT";
        public const string VISUAL_WITHOUT_TAB_ORDER = "VISUAL_WITHOUT_TAB_ORDER";
        public const string VISUAL_WITHOUT_TITLES = "VISUAL_WITHOUT_TITLES";
        public const string BUTTONS_WITHOUT_ACTION = "BUTTONS_WITHOUT_ACTION";
        public const string BUTTONS_WITHOUT_TOOLTIP = "BUTTONS_WITHOUT_TOOLTIP";
        public const string STATIC_VISIBLE_HEADER_ICONS = "STATIC_VISIBLE_HEADER_ICONS";
        public const string SLICER_WITHOUT_SEARCH_OPTION = "SLICER_WITHOUT_SEARCH_OPTION";
        public const string SLICER_WITHOUT_SELECT_ALL_OPTION = "SLICER_WITHOUT_SELECT_ALL_OPTION";
        public const string SLICER_WITHOUT_HEADERS = "SLICER_WITHOUT_HEADERS";
        public const string HEAVY_GRIDS = "HEAVY_GRIDS";
        public const string GRID_WITHOUT_GRAND_TOTAL = "GRID_WITHOUT_GRAND_TOTAL";
        public const string FULLY_EXPANDED_MATRIX = "FULLY_EXPANDED_MATRIX";
        public const string SPELL_PAGE_NAME = "SPELL_PAGE_NAME";
        public const string SPELL_VISUAL_TITLE = "SPELL_VISUAL_TITLE";
        public const string SPELL_VISUAL_TEXT = "SPELL_VISUAL_TEXT";
        public const string COLUMNS_UNUSED = "COLUMNS_UNUSED";
        public const string MEASURES_UNUSED = "MEASURES_UNUSED";
        public const string VARIABLE_SUGGEST = "VARIABLE_SUGGEST";
        public const string VISUAL_TITLE_FORMATTING = "VISUAL_TITLE_FORMATTING";
        public const string VISUAL_DATA_LABEL_FORMATTING = "VISUAL_DATA_LABEL_FORMATTING";
        public const string VISUAL_DATA_BACKGROUND_FORMATTING = "VISUAL_DATA_BACKGROUND_FORMATTING";
        public const string VISUAL_BORDER_FORMATTING = "VISUAL_BORDER_FORMATTING";

        // Title Formatting Constants
        
        // Visual Title Formatting Constants
        public const string VISUAL_TITLE_FONT_FAMILY_FORMATTING = "VISUAL_TITLE_FONT_FAMILY_FORMATTING";
        public const string VISUAL_TITLE_FONT_COLOR_FORMATTING = "VISUAL_TITLE_FONT_COLOR_FORMATTING";
        public const string VISUAL_TITLE_FONT_SIZE_FORMATTING = "VISUAL_TITLE_FONT_SIZE_FORMATTING";
        public const string VISUAL_TITLE_BACKGROUND_COLOR_FORMATTING = "VISUAL_TITLE_BACKGROUND_COLOR_FORMATTING";
        public const string VISUAL_TITLE_ALIGNMENT = "VISUAL_TITLE_ALIGNMENT_FORMATTING";

        // Data Label Formatting Constants
        public const string VISUAL_DATALABEL_FONT_SIZE = "VISUAL_DATALABEL_FONT_SIZE_FORMATTING";
        public const string VISUAL_DATALABEL_FONT_FAMILY = "VISUAL_DATALABEL_FONT_FAMILY_FORMATTING";
        public const string VISUAL_DATALABEL_FONT_COLOR = "VISUAL_DATALABEL_FONT_COLOR_FORMATTING";
        public const string VISUAL_DATALABEL_ORIENTATION = "VISUAL_DATALABEL_ORIENTATION_FORMATTING";
        public const string VISUAL_DATALABEL_BACKGROUND_COLOR = "VISUAL_DATALABEL_BACKGROUND_COLOR_FORMATTING";

        // Border Formatting Constants
        public const string VISUAL_BORDER_VISIBILITY = "VISUAL_BORDER_VISIBILITY_FORMATTING";
        public const string VISUAL_BORDER_COLOR = "VISUAL_BORDER_COLOR_FORMATTING";
        public const string VISUAL_BORDER_RADIUS = "VISUAL_BORDER_RADIUS_FORMATTING";
        public const string VISUAL_BORDER_THICKNESS = "VISUAL_BORDER_THICKNESS_FORMATTING";

        // Background Formatting Constants
        public const string VISUAL_BACKGROUND_COLOR = "VISUAL_BACKGROUND_COLOR_FORMATTING";
        public const string VISUAL_BACKGROUND_TRANSPARENCY = "VISUAL_BACKGROUND_TRANSPARENCY_FORMATTING";
        
        // Visual Accessibility specific visuals
        public const string VISUAL_ALT_TEXT_ISSUE = "VISUAL_ALT_TEXT_ISSUE";
        public const string VISUAL_TABORDER_ISSUE = "VISUAL_TABORDER_ISSUE";
        public const string VISUAL_TITLE_ISSUES = "VISUAL_TITLE_ISSUES";
        public const string VISUAL_SUBTITLE_ISSUES = "VISUAL_SUBTITLE_ISSUES";
        public const string VISUAL_TOOLTIP_ISSUES = "VISUAL_TOOLTIP_ISSUES";
        public const string VISUAL_DATA_LABEL_ISSUES = "VISUAL_DATA_LABEL_ISSUES";

        // Accessibility specific rules
        public const string COLOR_NOT_VALID_COLORBLIND = "COLOR_NOT_VALID_COLORBLIND";
        public const string COLOR_NOT_VALID_BACKGROUND_CONTRAST = "COLOR_NOT_VALID_BACKGROUND_CONTRAST";
        public const string VISUAL_TEXT_SPACING = "VISUAL_TEXT_SPACING";
        public const string LINK_PURPOSE = "LINK_PURPOSE";
        public const string COLOR_NOT_VALID_COLOR_PALETTE = "COLOR_NOT_VALID_COLOR_PALETTE";

        // Copilot specific rules
        public const string AI_PREP_SIMPLIFY_SCHEMA = "AI_PREP_SIMPLIFY_SCHEMA";
        public const string AI_PREP_VERIFIED_ANSWERS = "AI_PREP_VERIFIED_ANSWERS";
        public const string AI_PREP_COMMON_PROMPT = "AI_PREP_COMMON_PROMPT";
        public const string MISSING_TABLE_DESCRIPTION = "MISSING_TABLE_DESCRIPTION";
        public const string TABLE_SYNONYMNS = "TABLE_SYNONYMNS";
        public const string COLUMN_SYNONYMS = "COLUMN_SYNONYMS";
        public const string MEASURE_SYNONYMNS = "MEASURE_SYNONYMNS";
        public const string AVOID_PAGE_LEVEL_FILTER = "AVOID_PAGE_LEVEL_FILTER";
        public const string AVOID_VISUAL_LEVEL_FILTER = "AVOID_VISUAL_LEVEL_FILTER";
        public const string AVOID_BOOKMARKS = "AVOID_BOOKMARKS";
        public const string NON_MEANINGFUL_TABLE_NAME = "NON_MEANINGFUL_TABLE_NAME";
        public const string NON_MEANINGFUL_COLUMN_NAME = "NON_MEANINGFUL_COLUMN_NAME";
        public const string NON_MEANINGFUL_MEASURE_NAME = "NON_MEANINGFUL_MEASURE_NAME";
        public const string MEASURE_NAME_VS_LOGIC = "MEASURE_NAME_VS_LOGIC";
        public const string MISSING_DIMENSION_HIERARCHIES = "MISSING_DIMENSION_HIERARCHIES";

        // Visual Aligment rules
        public const string VERTICAL_ALIGNMENT = "VERTICAL_ALIGNMENT";
        public const string HORIZONTAL_ALIGNMENT = "HORIZONTAL_ALIGNMENT";
        public const string VISUAL_SPACING = "VISUAL_SPACING";
        public const string SIDEBAR_MARGIN = "SIDEBAR_MARGIN";
        public const string VERTICAL_VISUAL_SPACING = "VERTICAL_VISUAL_SPACING";
        public const string HORIZONTAL_VISUAL_SPACING = "HORIZONTAL_VISUAL_SPACING";
        public const string TOP_SIDEBAR_MARGIN = "TOP_SIDEBAR_MARGIN";
        public const string LEFT_SIDEBAR_MARGIN = "LEFT_SIDEBAR_MARGIN";
        public const string BOTTOM_SIDEBAR_MARGIN = "BOTTOM_SIDEBAR_MARGIN";
        public const string RIGHT_SIDEBAR_MARGIN = "RIGHT_SIDEBAR_MARGIN";


        // visual loading ruels
        public const string VISUAL_WITH_HIGH_SE_QUERIES = "VISUAL_WITH_HIGH_SE_QUERIES";
        public const string VISUAL_WITH_HIGH_LOAD_TIME = "VISUAL_WITH_HIGH_LOAD_TIME";
        public const string VISUAL_WITH_HIGH_CPU_TIME = "VISUAL_WITH_HIGH_CPU_TIME";
        public const string PAGE_WITH_HIGH_SE_QUERIES = "PAGE_WITH_HIGH_SE_QUERIES";
        public const string PAGE_WITH_HIGH_LOAD_TIME = "PAGE_WITH_HIGH_LOAD_TIME";

    }
    public class SingleIssueRuleData
    {
        public List<object> Issues { get; set; } = [];
        public int? MaxIssuable { get; set; } = 0;
    }
    public class AllIssueRuleData
    {
        // public List<IssueRuleDataColumns> IssueRuleColumns { get; set; } = [];
        public List<IssueRulesMetadataDTO> IssueRules { get; set; } = [];
        public List<IssueRulesMetadataDTO> FailedIssueRules { get; set; } = [];
        public List<IssueRulesMetadataDTO> DisabledIssueRules { get; set; } = [];
    }
    public class AddToIgnoreList
    {
        public string WorkspaceId { get; set; } = "";
        public string ArtifactId { get; set; } = "";
        public int InsightId { get; set; }
        public string InsightRowKey { get; set; } = "";
        public string Comment { get; set; } = "";
    }
    public class AddToIgnoreListReqModel
    {
        public List<AddToIgnoreList> addToIgnoreLists { get; set; } = [];
    }
}