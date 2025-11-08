namespace PowerBI_MCP.DTO
{
    /// <summary>
    /// Slicer visual, including columns, measures, and UI options.
    /// </summary>
    public class SlicersModel
    {
        /// <summary>Visual ID of the slicer.</summary>
        public string? VisualId { get; set; }

        /// <summary>Columns used in the slicer.</summary>
        public string? Columns { get; set; }

        /// <summary>Measures used in the slicer.</summary>
        public string? Measures { get; set; }

        /// <summary>True if search is disabled.</summary>
        public bool? IsSearchDisabled { get; set; }

        /// <summary>True if "search all in multi-select" is disabled.</summary>
        public bool? IsSearchAllInMultiSelectDisabled { get; set; }

        /// <summary>True if the slicer header is disabled.</summary>
        public bool? IsHeaderDisabled { get; set; }
    }

    /// <summary>
    /// Table or matrix visual, including column and total settings.
    /// </summary>
    public class TableOrMatrixModel
    {
        /// <summary>Visual ID.</summary>
        public string? VisualId { get; set; }

        /// <summary>Visual type (table or matrix).</summary>
        public string? VisualType { get; set; }

        /// <summary>Number of columns.</summary>
        public int? ColumnCount { get; set; }

        /// <summary>True if grand total is disabled.</summary>
        public bool? IsGrandTotalDisabled { get; set; }

        /// <summary>True if row grand total is disabled.</summary>
        public bool? IsRowGrandTotalDisabled { get; set; }

        /// <summary>True if column grand total is disabled.</summary>
        public bool? IsColumnGrandTotalDisabled { get; set; }

        /// <summary>True if fully expanded.</summary>
        public bool? IsFullyExpanded { get; set; }
    }

    /// <summary>
    /// Static component (e.g., button or icon) with UI options.
    /// </summary>
    public class StaticComponentsModel
    {
        /// <summary>Visual ID.</summary>
        public string? VisualId { get; set; }

        /// <summary>Type of static component.</summary>
        public string? VisualType { get; set; }

        /// <summary>Button text if applicable.</summary>
        public string? ButtonText { get; set; }

        /// <summary>True if button action is disabled.</summary>
        public bool? IsButtonActionDisabled { get; set; }

        /// <summary>True if button tooltip is disabled.</summary>
        public bool? IsButtonToolTipDisabled { get; set; }

        /// <summary>True if header icon is enabled.</summary>
        public bool? IsHeaderIconEnabled { get; set; }
    }
}