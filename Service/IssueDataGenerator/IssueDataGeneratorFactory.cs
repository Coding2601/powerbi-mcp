using PowerBI_MCP.DTO;
using PowerBI_MCP.Service.IssueDataGenerator;

namespace PowerBI_MCP.Service.IssueDataGenerator
{
    public class IssueDataGeneratorFactory
    {

        private IssueDataGeneratorFactory() { }
        private static IssueDataGeneratorFactory? _instance;
        public static IssueDataGeneratorFactory Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new IssueDataGeneratorFactory();
                }
                return _instance;
            }
        }

        public IArtifactGroupIssueGenerator CreateArtifactGroupIssueGenerator(string type)
        {
            switch (type)
            {
                case IssueRuleFunctionMapper.COLUMNS_UNUSED:
                    return new PowerBI_MCP.Service.IssueDataGenerator.Generators.ColumnsIssuesGenerator.RemovedUnusedColumns();
                case IssueRuleFunctionMapper.MEASURES_UNUSED:
                    return new Generators.MeasuresIssuesGenerator.RemoveUnusedMeasures();
                default:
                    throw new NotImplementedException($"Artifact group issue generator for type '{type}' is not implemented.");
            }
        }

        public IVisualAlignmentIssueGenerator CreateVisualAlignmentIssueGenerator(string type)
        {
            switch (type)
            {
                // case IssueRuleFunctionMapper.SIDEBAR_MARGIN:
                //     return new Generators.VisualAlignmentIssuesGenerator.SideBarMargin();
                case IssueRuleFunctionMapper.VERTICAL_ALIGNMENT:
                    return new Generators.VisualAlignmentIssuesGenerator.VerticalAlignment();
                case IssueRuleFunctionMapper.HORIZONTAL_ALIGNMENT:
                    return new Generators.VisualAlignmentIssuesGenerator.HorizontalAlignment();
                // case IssueRuleFunctionMapper.VISUAL_SPACING:
                //     return new Generators.VisualAlignmentIssuesGenerator.VisualSpacing();
                case IssueRuleFunctionMapper.VERTICAL_VISUAL_SPACING:
                    return new Generators.VisualAlignmentIssuesGenerator.VerticalVisualSpacing();
                case IssueRuleFunctionMapper.HORIZONTAL_VISUAL_SPACING:
                    return new Generators.VisualAlignmentIssuesGenerator.HorizontalVisualSpacing();
                case IssueRuleFunctionMapper.TOP_SIDEBAR_MARGIN:
                    return new Generators.VisualAlignmentIssuesGenerator.TopSideBarMargin();
                case IssueRuleFunctionMapper.LEFT_SIDEBAR_MARGIN:
                    return new Generators.VisualAlignmentIssuesGenerator.LeftSideBarMargin();
                case IssueRuleFunctionMapper.BOTTOM_SIDEBAR_MARGIN:
                    return new Generators.VisualAlignmentIssuesGenerator.BottomSideBarMargin();
                case IssueRuleFunctionMapper.RIGHT_SIDEBAR_MARGIN:
                    return new Generators.VisualAlignmentIssuesGenerator.RightSideBarMargin();
                default:
                    throw new NotImplementedException($"Visual alignment issue generator for type '{type}' is not implemented.");
            }
        }

        public IIssueDataGenerator CreateIssueDataGenerator(string type)
        {
            switch (type)
            {
                // tables
                case IssueRuleFunctionMapper.TABLE_LOCAL_DATE_TIME:
                    return new Generators.TablesIssuesGenerator.RemoveAutoDateTimeTables();

                case IssueRuleFunctionMapper.TABLE_CALCULATED:
                    return new Generators.TablesIssuesGenerator.AvoidCalcTables();

                case IssueRuleFunctionMapper.TABLE_HIGH_POWER_QUERY_TRANSFORMATIONS:
                    return new Generators.TablesIssuesGenerator.ReducePowerQueriesTransformations();

                case IssueRuleFunctionMapper.TABLE_FACT_NOT_HIDDEN:
                    return new Generators.TablesIssuesGenerator.HideFactTables();

                // columns
                case IssueRuleFunctionMapper.COLUMN_SYNTAX_ERROR:
                    return new Generators.ColumnsIssuesGenerator.RemoveSyntaxErrors();

                case IssueRuleFunctionMapper.AVOID_CALCULATED_COLUMN:
                    return new Generators.ColumnsIssuesGenerator.RemoveSyntaxErrors();

                case IssueRuleFunctionMapper.COLUMN_SUMMARIZE_BY_KEY_ID_COL:
                    return new Generators.ColumnsIssuesGenerator.SummarizeKeyField();

                case IssueRuleFunctionMapper.COLUMN_DATETIME_DATA_TYPE:
                    return new Generators.ColumnsIssuesGenerator.DatetimeColumnWithMinOrSecPrecision();

                case IssueRuleFunctionMapper.COLUMN_MISSING_DESCRIPTION:
                    return new Generators.ColumnsIssuesGenerator.AddDescription();

                case IssueRuleFunctionMapper.COLUMN_HIDE:
                    return new Generators.ColumnsIssuesGenerator.HideInternalColumns();

                case IssueRuleFunctionMapper.COLUMN_DUPLICATE:
                    return new Generators.ColumnsIssuesGenerator.RemoveDuplicateColumns();

                case IssueRuleFunctionMapper.COLUMNS_UNUSED:
                    return new Generators.ColumnsIssuesGenerator.RemovedUnusedColumns();

                case IssueRuleFunctionMapper.FORMATTING_NO_SEPARATOR_COLUMN:
                    return new Generators.ColumnsIssuesGenerator.ColumnsWithoutCommaSeparator();


                // measures
                case IssueRuleFunctionMapper.MEASURE_WITHOUT_MEASURE_TABLE:
                    return new Generators.MeasuresIssuesGenerator.MeasuresNotInMeasureTable();

                case IssueRuleFunctionMapper.MEASURE_SYNTAX_ERROR:
                    return new Generators.MeasuresIssuesGenerator.ResolveSyntaxErrors();

                case IssueRuleFunctionMapper.MEASURE_MISSING_FOLDER:
                    return new Generators.MeasuresIssuesGenerator.GroupMeasureByFolder();

                case IssueRuleFunctionMapper.MEASURE_MISSING_DESCRIPTION:
                    return new Generators.MeasuresIssuesGenerator.AddDescription();

                case IssueRuleFunctionMapper.MEASURE_DUPLICATE:
                    return new Generators.MeasuresIssuesGenerator.RemoveDuplicateMeasure();

                case IssueRuleFunctionMapper.MEASURES_UNUSED:
                    return new Generators.MeasuresIssuesGenerator.RemoveUnusedMeasures();

                case IssueRuleFunctionMapper.FORMATTING_NO_SEPARATOR_MEASURE:
                    return new Generators.MeasuresIssuesGenerator.MeasuresWithoutCommaSeparator();


                // DAX
                case IssueRuleFunctionMapper.DAX_NON_RECOMMENDED_FUNCTION:
                    return new Generators.DaxIssuesGenerator.NonRecommendedDAXFunc();

                case IssueRuleFunctionMapper.DAX_DIVIDE_DEFAULT:
                    return new Generators.DaxIssuesGenerator.AvoidStaticInDivide();

                case IssueRuleFunctionMapper.DAX_DIVIDE_TO_STATIC:
                    return new Generators.DaxIssuesGenerator.AvoidBlankToStaticConversion();

                case IssueRuleFunctionMapper.DAX_ISBLANK_CHECK:
                    return new Generators.DaxIssuesGenerator.AvoidIsBlankCheck();

                case IssueRuleFunctionMapper.DAX_USED_SUMMARIZE_OR_GROUPBY_FUNC:
                    return new Generators.DaxIssuesGenerator.AvoidSummarizeOrGroupByFunc();

                case IssueRuleFunctionMapper.DAX_REPEATED_EXPRESSIONS:
                    return new Generators.DaxIssuesGenerator.AvoidSameDaxExpression();

                case IssueRuleFunctionMapper.DAX_SUGGEST_VARIABLE:
                    return new Generators.DaxIssuesGenerator.UseVariables();

                //relationship
                case IssueRuleFunctionMapper.RELATIONSHIP_MANY_TO_MANY_CARDINALITY:
                    return new Generators.RelationShipIssuesGenerator.ManyToManyRelationships();

                case IssueRuleFunctionMapper.RELATIONSHIP_BIDIRECTIONAL_FILTERING:
                    return new Generators.RelationShipIssuesGenerator.BiDirectionalRelationships();

                case IssueRuleFunctionMapper.RELATIONSHIP_USE_NON_NUMERIC_FIELD:
                    return new Generators.RelationShipIssuesGenerator.NonNumericFieldUsedInRelationships();

                case IssueRuleFunctionMapper.RELATIONSHIP_ONE_TO_ONE:
                    return new Generators.RelationShipIssuesGenerator.OneToOneRelationships();

                // DQ and relation security
                case IssueRuleFunctionMapper.DQ_WEAK_RELATIONSHIP:
                    return new Generators.DqAndSecurityIssuesGenerator.DQWeakRelationships();

                case IssueRuleFunctionMapper.DQ_DISABLE_REFERENTIAL_INTEGRITY:
                    return new Generators.DqAndSecurityIssuesGenerator.DQDisableReferentialIntegrity();

                case IssueRuleFunctionMapper.SECURITY_NO_SECURITY_RELATIONS:
                    return new Generators.DqAndSecurityIssuesGenerator.NoSecurityRelations();

                case IssueRuleFunctionMapper.RLS_ON_MANY_SIDE_OF_REL:
                    return new Generators.DqAndSecurityIssuesGenerator.RLSOnManySideOfRel();


                // Spell check
                case IssueRuleFunctionMapper.SPELL_TABLE_NAME:
                    return new Generators.SpellCheckIssuesGenerator.TableNameSpellCheck();

                case IssueRuleFunctionMapper.SPELL_COLUMN_NAME:
                    return new Generators.SpellCheckIssuesGenerator.ColumnNameSpellCheck();

                case IssueRuleFunctionMapper.SPELL_MEASURE_NAME:
                    return new Generators.SpellCheckIssuesGenerator.MeasureNameSpellCheck();

                case IssueRuleFunctionMapper.SPELL_PAGE_NAME:
                    return new Generators.SpellCheckIssuesGenerator.PageNameSpellCheck();

                case IssueRuleFunctionMapper.SPELL_VISUAL_TITLE:
                    return new Generators.SpellCheckIssuesGenerator.VisualTitleSpellCheck();

                case IssueRuleFunctionMapper.SPELL_VISUAL_TEXT:
                    return new Generators.SpellCheckIssuesGenerator.VisualTextSpellCheck();

                // Report
                case IssueRuleFunctionMapper.REPORT_THEME:
                    return new Generators.ReportIssueGenerator.ReportTheme();

                case IssueRuleFunctionMapper.FORMATTING_INCONSISTENT_DECIMAL:
                    return new Generators.ReportIssueGenerator.FormattingInconsistentDecimal();

                case IssueRuleFunctionMapper.FORMATTING_INCONSISTENT_DATETIME:
                    return new Generators.ReportIssueGenerator.FormattingInconsistentDateTime();

                // Page
                case IssueRuleFunctionMapper.PAGE_INCONSISTENT_SIZE:
                    return new Generators.PageIssueGenerator.InconsistentPageSize();

                case IssueRuleFunctionMapper.PAGE_STATIC_MORE_THAN_10:
                    return new Generators.PageIssueGenerator.PageWithMoreThan10Visual();

                case IssueRuleFunctionMapper.PAGE_VISUAL_MORE_THAN_10:
                    return new Generators.PageIssueGenerator.PageWithMoreThan10Static();


                // visual
                case IssueRuleFunctionMapper.VISUAL_ALT_TEXT_ISSUE:
                    return new Generators.VisualIssuesGenerator.VisualAltTextIssues();
                case IssueRuleFunctionMapper.VISUAL_TABORDER_ISSUE:
                    return new Generators.VisualIssuesGenerator.VisualTabOrderIssues();
                case IssueRuleFunctionMapper.VISUAL_TITLE_ISSUES:
                    return new Generators.VisualIssuesGenerator.VisualTitleIssues();
                case IssueRuleFunctionMapper.VISUAL_SUBTITLE_ISSUES:
                    return new Generators.VisualIssuesGenerator.VisualSubTitleIssue();
                case IssueRuleFunctionMapper.VISUAL_TOOLTIP_ISSUES:
                    return new Generators.VisualIssuesGenerator.VisualTooltipIssues();
                case IssueRuleFunctionMapper.VISUAL_DATA_LABEL_ISSUES:
                    return new Generators.VisualIssuesGenerator.VisualDataLabelIssues();

                // visual Formatting
                case IssueRuleFunctionMapper.VISUAL_TITLE_FORMATTING:
                    return new Generators.VisualConsistencyIssuesGenerator.ConsistentTitleFormatting();
                case IssueRuleFunctionMapper.VISUAL_DATA_BACKGROUND_FORMATTING:
                    return new Generators.VisualConsistencyIssuesGenerator.ConsistentBackgroundFormatting();
                case IssueRuleFunctionMapper.VISUAL_BORDER_FORMATTING:
                    return new Generators.VisualConsistencyIssuesGenerator.ConsistentBorderFormatting();
                case IssueRuleFunctionMapper.VISUAL_DATA_LABEL_FORMATTING:
                    return new Generators.VisualConsistencyIssuesGenerator.ConsistentDataLabelFormatting();

                // Title Formatting
                case IssueRuleFunctionMapper.VISUAL_TITLE_ALIGNMENT:
                    return new Generators.VisualFormattingIssuesGenerator.TitleAlignmentFormatting();

                // Data Label Formatting
                case IssueRuleFunctionMapper.VISUAL_DATALABEL_FONT_SIZE:
                    return new Generators.DataLabelFormattingIssuesGenerator.DataLabelFontSizeFormatting();
                case IssueRuleFunctionMapper.VISUAL_DATALABEL_FONT_FAMILY:
                    return new Generators.DataLabelFormattingIssuesGenerator.DataLabelFontFamilyFormatting();
                case IssueRuleFunctionMapper.VISUAL_DATALABEL_FONT_COLOR:
                    return new Generators.DataLabelFormattingIssuesGenerator.DataLabelFontColorFormatting();
                case IssueRuleFunctionMapper.VISUAL_DATALABEL_ORIENTATION:
                    return new Generators.DataLabelFormattingIssuesGenerator.DataLabelOrientationFormatting();
                case IssueRuleFunctionMapper.VISUAL_DATALABEL_BACKGROUND_COLOR:
                    return new Generators.DataLabelFormattingIssuesGenerator.DataLabelBackgroundColorFormatting();

                // Border Formatting
                case IssueRuleFunctionMapper.VISUAL_BORDER_VISIBILITY:
                    return new Generators.BorderFormattingIssuesGenerator.BorderVisibilityFormatting();
                case IssueRuleFunctionMapper.VISUAL_BORDER_COLOR:
                    return new Generators.BorderFormattingIssuesGenerator.BorderColorFormatting();
                case IssueRuleFunctionMapper.VISUAL_BORDER_RADIUS:
                    return new Generators.BorderFormattingIssuesGenerator.BorderRadiusFormatting();
                case IssueRuleFunctionMapper.VISUAL_BORDER_THICKNESS:
                    return new Generators.BorderFormattingIssuesGenerator.BorderThicknessFormatting();

                // Background Formatting
                case IssueRuleFunctionMapper.VISUAL_BACKGROUND_COLOR:
                    return new Generators.BackgroundFormattingIssuesGenerator.BackgroundColorFormatting();
                case IssueRuleFunctionMapper.VISUAL_BACKGROUND_TRANSPARENCY:
                    return new Generators.BackgroundFormattingIssuesGenerator.BackgroundTransparencyFormatting();

                // Visual Title Formatting
                case IssueRuleFunctionMapper.VISUAL_TITLE_FONT_FAMILY_FORMATTING:
                    return new Generators.VisualFormattingIssuesGenerator.TitleFontFamilyFormatting();
                case IssueRuleFunctionMapper.VISUAL_TITLE_FONT_COLOR_FORMATTING:
                    return new Generators.VisualFormattingIssuesGenerator.TitleFontColorFormatting();
                case IssueRuleFunctionMapper.VISUAL_TITLE_FONT_SIZE_FORMATTING:
                    return new Generators.VisualFormattingIssuesGenerator.TitleFontSizeFormatting();
                case IssueRuleFunctionMapper.VISUAL_TITLE_BACKGROUND_COLOR_FORMATTING:
                    return new Generators.VisualFormattingIssuesGenerator.TitleBackgroundColorFormatting();

                // static visual and button
                case IssueRuleFunctionMapper.BUTTONS_WITHOUT_ACTION:
                    return new Generators.ButtonsIssuesGenerator.ButtonWithoutAction();

                case IssueRuleFunctionMapper.BUTTONS_WITHOUT_TOOLTIP:
                    return new Generators.ButtonsIssuesGenerator.ButtonWithoutTooltip();

                case IssueRuleFunctionMapper.STATIC_VISIBLE_HEADER_ICONS:
                    return new Generators.ButtonsIssuesGenerator.VisibleHeaderIconInStaticVis();

                // Slicer
                case IssueRuleFunctionMapper.SLICER_WITHOUT_SEARCH_OPTION:
                    return new Generators.SlicerIssueGenerator.SlicerWithoutSearch();

                case IssueRuleFunctionMapper.SLICER_WITHOUT_SELECT_ALL_OPTION:
                    return new Generators.SlicerIssueGenerator.MuliselectSlicerWithoutSelectAll();

                case IssueRuleFunctionMapper.SLICER_WITHOUT_HEADERS:
                    return new Generators.SlicerIssueGenerator.SlicerWithoutHeader();

                // table and matrix visual
                case IssueRuleFunctionMapper.HEAVY_GRIDS:
                    return new Generators.TableGridIssuesGenerator.GridHaveMoreThan10Fields();

                case IssueRuleFunctionMapper.GRID_WITHOUT_GRAND_TOTAL:
                    return new Generators.TableGridIssuesGenerator.GrandTotalDisabled();

                case IssueRuleFunctionMapper.FULLY_EXPANDED_MATRIX:
                    return new Generators.TableGridIssuesGenerator.FullyExpandedMatrix();

                // Accessibility related generators
                case IssueRuleFunctionMapper.COLOR_NOT_VALID_COLORBLIND:
                    return new Generators.AccessibilityIssuesGenerator.ColorNotDistinguishableForColorBlind();
                case IssueRuleFunctionMapper.COLOR_NOT_VALID_BACKGROUND_CONTRAST:
                    return new Generators.AccessibilityIssuesGenerator.BackgroundContrast();
                case IssueRuleFunctionMapper.VISUAL_TEXT_SPACING:
                    return new Generators.AccessibilityIssuesGenerator.VisualTextSpacing();
                case IssueRuleFunctionMapper.LINK_PURPOSE:
                    return new Generators.AccessibilityIssuesGenerator.LinkPurpose();
                case IssueRuleFunctionMapper.COLOR_NOT_VALID_COLOR_PALETTE:
                    return new Generators.AccessibilityIssuesGenerator.ColorPalette();


                // Copilot related generators
                case IssueRuleFunctionMapper.AI_PREP_SIMPLIFY_SCHEMA:
                    return new Generators.CopilotIssuesGenerator.AIPrepSimplifyDataSchema();
                case IssueRuleFunctionMapper.AI_PREP_VERIFIED_ANSWERS:
                    return new Generators.CopilotIssuesGenerator.AIPrepVerifiedAnswers();
                case IssueRuleFunctionMapper.AI_PREP_COMMON_PROMPT:
                    return new Generators.CopilotIssuesGenerator.AIPrepCommonPrompt();
                case IssueRuleFunctionMapper.MISSING_TABLE_DESCRIPTION:
                    return new Generators.CopilotIssuesGenerator.AddDescriptionInTable();
                case IssueRuleFunctionMapper.TABLE_SYNONYMNS:
                    return new Generators.CopilotIssuesGenerator.MissingTableSynonyms();
                case IssueRuleFunctionMapper.COLUMN_SYNONYMS:
                    return new Generators.CopilotIssuesGenerator.MissingColumnsSynonyms();
                case IssueRuleFunctionMapper.MEASURE_SYNONYMNS:
                    return new Generators.CopilotIssuesGenerator.MissingMeasuresSynonyms();
                case IssueRuleFunctionMapper.AVOID_PAGE_LEVEL_FILTER:
                    return new Generators.CopilotIssuesGenerator.AvoidPageLevelFilter();
                case IssueRuleFunctionMapper.AVOID_VISUAL_LEVEL_FILTER:
                    return new Generators.CopilotIssuesGenerator.AvoidVisualLevelFilter();
                case IssueRuleFunctionMapper.AVOID_BOOKMARKS:
                    return new Generators.CopilotIssuesGenerator.AvoidBookmarks();
                case IssueRuleFunctionMapper.NON_MEANINGFUL_TABLE_NAME:
                    return new Generators.CopilotIssuesGenerator.MeaningFulTableName();
                case IssueRuleFunctionMapper.NON_MEANINGFUL_COLUMN_NAME:
                    return new Generators.CopilotIssuesGenerator.MeaningFulColumnName();
                case IssueRuleFunctionMapper.NON_MEANINGFUL_MEASURE_NAME:
                    return new Generators.CopilotIssuesGenerator.MeaningFulMeasureName();
                case IssueRuleFunctionMapper.MEASURE_NAME_VS_LOGIC:
                    return new Generators.CopilotIssuesGenerator.MeasureNameVsLogic();
                case IssueRuleFunctionMapper.MISSING_DIMENSION_HIERARCHIES:
                    return new Generators.CopilotIssuesGenerator.MissingDimensionHierarchies();

                // Visual Alignment rules
                case IssueRuleFunctionMapper.VERTICAL_ALIGNMENT:
                    return new Generators.VisualAlignmentIssuesGenerator.VerticalAlignment();
                case IssueRuleFunctionMapper.HORIZONTAL_ALIGNMENT:
                    return new Generators.VisualAlignmentIssuesGenerator.HorizontalAlignment();
                // case IssueRuleFunctionMapper.VISUAL_SPACING:
                //     return new Generators.VisualAlignmentIssuesGenerator.VisualSpacing();
                // case IssueRuleFunctionMapper.SIDEBAR_MARGIN:
                //     return new Generators.VisualAlignmentIssuesGenerator.SideBarMargin();
                case IssueRuleFunctionMapper.VERTICAL_VISUAL_SPACING:
                    return new Generators.VisualAlignmentIssuesGenerator.VerticalVisualSpacing();
                case IssueRuleFunctionMapper.HORIZONTAL_VISUAL_SPACING:
                    return new Generators.VisualAlignmentIssuesGenerator.HorizontalVisualSpacing();
                case IssueRuleFunctionMapper.TOP_SIDEBAR_MARGIN:
                    return new Generators.VisualAlignmentIssuesGenerator.TopSideBarMargin();
                case IssueRuleFunctionMapper.LEFT_SIDEBAR_MARGIN:
                    return new Generators.VisualAlignmentIssuesGenerator.LeftSideBarMargin();
                case IssueRuleFunctionMapper.BOTTOM_SIDEBAR_MARGIN:
                    return new Generators.VisualAlignmentIssuesGenerator.BottomSideBarMargin();
                case IssueRuleFunctionMapper.RIGHT_SIDEBAR_MARGIN:
                    return new Generators.VisualAlignmentIssuesGenerator.RightSideBarMargin();


                // visual loading 
                case IssueRuleFunctionMapper.VISUAL_WITH_HIGH_SE_QUERIES:
                    return new Generators.VisualLoadingIssuesGenerator.VisualLoadingIssue();
                case IssueRuleFunctionMapper.VISUAL_WITH_HIGH_LOAD_TIME:
                    return new Generators.VisualLoadingIssuesGenerator.VisualLoadingIssue();
                case IssueRuleFunctionMapper.VISUAL_WITH_HIGH_CPU_TIME:
                    return new Generators.VisualLoadingIssuesGenerator.VisualLoadingIssue();
                case IssueRuleFunctionMapper.PAGE_WITH_HIGH_SE_QUERIES:
                    return new Generators.VisualLoadingIssuesGenerator.VisualLoadingIssue();
                case IssueRuleFunctionMapper.PAGE_WITH_HIGH_LOAD_TIME:
                    return new Generators.VisualLoadingIssuesGenerator.VisualLoadingIssue();


                default:
                    throw new NotImplementedException($"Issue data generator for type '{type}' is not implemented.");
            }
        }
    }
}