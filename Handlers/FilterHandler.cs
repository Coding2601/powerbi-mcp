using PowerBI_MCP.DTO;
using Newtonsoft.Json.Linq;

namespace PowerBI_MCP.Handlers
{
    /// <summary>
    /// Represents metadata for a filter field.
    /// </summary>
    public class FilterFieldData
    {
        public VisualUsedFieldType? FieldType { get; set; } = VisualUsedFieldType.COLUMN;
        public FilterFieldLocation? FieldLocation { get; set; } = FilterFieldLocation.VISUAL;
        public FilterType? FilterTypeName { get; set; } = FilterType.CATEGORICAL; 
        public string TableName { get; set; } = "";
        public string Hierarchy { get; set; } = "";
        public string FieldName { get; set; } = "";
    }

    /// <summary>
    /// Enum for filter field type (column or measure).
    /// </summary>
    public enum FilterFieldType
    {
        COLUMN,
        MEASURE,
    }

    /// <summary>
    /// Enum for filter type (categorical, advanced, top N).
    /// </summary>
    public enum FilterType
    {
        CATEGORICAL,
        ADVANCED,
        TOPN,
    }

    /// <summary>
    /// Enum for filter field location (visual, page, report).
    /// </summary>
    public enum FilterFieldLocation
    {
        VISUAL,
        PAGE,
        REPORT,
    }

    /// <summary>
    /// Holds simplified filter data, including filter list and field dictionaries.
    /// </summary>
    public class SimplifyFilterData
    {
        public List<Dictionary<string, string>> filterList { get; set; } = [];
        public List<FilterFieldData> FilterFieldList { get; set; } = [];
        public Dictionary<string, bool> columnDict { get; set; } = [];
        public Dictionary<string, bool> measureDict { get; set; } = [];
    }

    /// <summary>
    /// Handler for simplifying and extracting filter information from JSON filter definitions.
    /// </summary>
    public class FilterHandler
    {
        public static Dictionary<string, bool> columnDict = [];
        public static Dictionary<string, bool> measureDict = [];
        private static List<FilterFieldData> filterFieldList = [];

        /// <summary>
        /// Clears the column and measure dictionaries.
        /// </summary>
        public static void clearDict()
        {
            columnDict = [];
            measureDict = [];
            filterFieldList = [];
        }

        /// <summary>
        /// Gets the field name from a filter expression.
        /// </summary>
        public static string GetFieldName(JToken? expression, FilterFieldLocation filterFieldLocation, FilterType filterType, bool isPBIR = false)
        {
            string? fieldName = null;
            if (expression != null)
            {
                var temp = expression["Column"] ?? expression["Measure"];
                if (expression["Aggregation"] != null)
                {
                    var aggregationExpr = expression["Aggregation"];
                    temp = aggregationExpr?["Expression"]?["Column"];
                }
                var colOrMeasure = expression["Column"] != null || (expression["Aggregation"]?["Expression"]?["Column"] != null) ? "Column" : "Measure";
                if (temp != null)
                {
                    fieldName = temp?["Expression"]?["SourceRef"]?["Entity"]?.ToString() + "." + temp?["Property"] + "( " + colOrMeasure + " )";
                    string? colOrMeasureName = temp?["Expression"]?["SourceRef"]?["Entity"]?.ToString() + "." + temp?["Property"]?.ToString();
                    if (colOrMeasure != null && colOrMeasure == "Column")
                    {
                        if (!columnDict.ContainsKey(colOrMeasureName))
                            columnDict.Add(colOrMeasureName.Trim(), true);
                        filterFieldList.Add(
                            new FilterFieldData()
                            {
                                TableName = temp?["Expression"]?["SourceRef"]?["Entity"]?.ToString() ?? "",
                                FieldName = temp?["Property"]?.ToString() ?? "",
                                FieldType = VisualUsedFieldType.COLUMN,
                                FieldLocation = filterFieldLocation,
                                FilterTypeName = filterType,
                            }
                        );
                    }
                    else if (colOrMeasure != null && colOrMeasure == "Measure")
                    {
                        colOrMeasureName = temp?["Property"]?.ToString();
                        if (colOrMeasureName != null && !measureDict.ContainsKey(colOrMeasureName))
                            measureDict.Add(colOrMeasureName.Trim(), true);
                        filterFieldList.Add(
                            new FilterFieldData()
                            {
                                TableName = temp?["Expression"]?["SourceRef"]?["Entity"]?.ToString() ?? "",
                                FieldName = temp?["Property"]?.ToString() ?? "",
                                FieldType = VisualUsedFieldType.MEASURE, 
                                FieldLocation = filterFieldLocation,
                                FilterTypeName = filterType,
                            }
                        );
                    }
                }
                else if (expression["HierarchyLevel"] != null)
                {
                    temp = expression["HierarchyLevel"];
                    fieldName =
                        temp?["Expression"]?["Hierarchy"]?["Expression"]?["SourceRef"]?["Entity"]?.ToString()
                        + "."
                        + temp?["Expression"]?["Hierarchy"]?["Hierarchy"]?.ToString()
                        + "."
                        + temp?["Level"]?.ToString()
                        + "( "
                        + "Hierarchy"
                        + " )";

                    filterFieldList.Add(
                           new FilterFieldData()
                           {
                               TableName = temp?["Expression"]?["Hierarchy"]?["Expression"]?["SourceRef"]?["Entity"]?.ToString() ?? "",
                               Hierarchy = temp?["Expression"]?["Hierarchy"]?["Hierarchy"]?.ToString() ?? "",
                               FieldName = temp?["Level"]?.ToString() ?? "",
                               FieldType = VisualUsedFieldType.HIERARCHY,
                               FieldLocation = filterFieldLocation,
                               FilterTypeName = filterType,
                           }
                       );
                }
            }
            return fieldName ?? string.Empty;
        }

        // Mapping for TopN aggregation functions.
        private static readonly Dictionary<string, string> topNFunctionMap = new()
        {
            { "0", "Sum" },
            { "1", "Average" },
            { "2", "Distinct Count" },
            { "3", "Minimum" },
            { "4", "Maximum" },
            { "5", "Count" },
            { "6", "Median" },
            { "7", "Standard deviation" },
        };

        /// <summary>
        /// Simplifies a categorical filter from JSON.
        /// </summary>
        private static Dictionary<string, string> simplifyCategoricalFilter(JObject filter, FilterFieldLocation filterFieldLocation, bool isPBIR = false)
        {
            string fieldName = "";
            Dictionary<string, string> simpleFilter = new Dictionary<string, string>();
            try
            {
                fieldName = GetFieldName(isPBIR ? filter["field"] : filter["expression"], filterFieldLocation, FilterType.CATEGORICAL, isPBIR);
                if (fieldName != null)
                {
                    if (filter["filter"] == null)
                    {
                        simpleFilter.Add(fieldName, " all are selected");
                        return simpleFilter;
                    }

                    var condition = filter?["filter"]?["Where"]?[0]?["Condition"];
                    string valueArray = condition?["Not"] != null ? " not exists in [ " : " exists in [ ";
                    condition = condition?["Not"] != null ? condition["Not"]?["Expression"] : condition;
                    if (condition != null)
                    {
                        var values = condition?["In"]?["Values"];
                        if (values?.Count() == 0)
                            valueArray += " ]";
                        for (int i = 0; i < values?.Count(); i++)
                        {
                            string? val = values?[i]?[0]?["Literal"]?["Value"]?.ToString();
                            if (val != null && val.EndsWith("L"))
                                val = val.Substring(0, val.Length - 1);
                            if (i == values?.Count() - 1)
                                valueArray += val + " ]";
                            else
                                valueArray += val + ", ";
                        }
                        simpleFilter.Add(fieldName, valueArray);
                        return simpleFilter;
                    }
                }
            }
            catch (Exception ex)
            {
                simpleFilter.Add(fieldName, "");
                Console.WriteLine(ex.ToString());
            }
            return simpleFilter;
        }

        /// <summary>
        /// Builds a string for advanced filter comparison.
        /// </summary>
        private static string advanceComparisonString(JToken? conditionChild, bool isPBIR = false)
        {
            string ans = "";
            bool notFactor = false;
            JToken? comparison = null;

            if (conditionChild == null)
            {
                ans = " not a valid advanced filter";
                return ans;
            }
            if (conditionChild["Not"] != null)
            {
                notFactor = true;
                conditionChild = conditionChild?["Not"]?["Expression"];
            }

            if (conditionChild?["Comparison"] != null)
            {
                comparison = conditionChild["Comparison"];
            }

            if (comparison != null)
            {
                string? comparisonKind = comparison?["ComparisonKind"]?.ToString();
                string? value = comparison?["Right"]?["Literal"]?["Value"]?.ToString().Trim('\'');
                if (value != null && value.EndsWith("L"))
                    value = value.Substring(0, value.Length - 1);

                if (comparisonKind == "0")
                {
                    if (notFactor)
                    {
                        if (value == "null")
                            ans = " is not blank";
                        else if (value == "")
                            ans = " is not empty";
                        else
                            ans = " is not " + value;
                    }
                    else
                    {
                        if (value == "null")
                            ans = " is blank";
                        else if (value == "")
                            ans = " is empty";
                        else
                            ans = " is " + value;
                    }
                }
                else if (comparisonKind == "1")
                    ans = " is greater than " + value;
                else if (comparisonKind == "2")
                    ans = " is greater than or equal to " + value;
                else if (comparisonKind == "3")
                    ans = " is less than " + value;
                else if (comparisonKind == "4")
                    ans = " is less than or equal to " + value;
                else
                {
                    ans = " not a valid advanced filter";
                }
            }
            else if (conditionChild?["StartsWith"] != null)
            {
                var startsWithObj = conditionChild["StartsWith"];
                string? value = startsWithObj?["Right"]?["Literal"]?["Value"]?.ToString().Trim('\'');

                if (notFactor)
                    ans = " not starts with " + value;
                else
                    ans = " starts with " + value;
            }
            else if (conditionChild?["Contains"] != null)
            {
                var containsObj = conditionChild["Contains"];
                string? value = containsObj?["Right"]?["Literal"]?["Value"]?.ToString().Trim('\'');

                if (notFactor)
                    ans = " not starts with " + value;
                else
                    ans = " starts with " + value;
            }

            return ans;
        }

        /// <summary>
        /// Simplifies an advanced filter from JSON.
        /// </summary>
        private static Dictionary<string, string> simplifyAdvanceFilter(JObject filter, FilterFieldLocation filterFieldLocation, bool isPBIR = false)
        {
            string fieldName = "";
            Dictionary<string, string> simpleFilter = new Dictionary<string, string>();
            try
            {
                fieldName = GetFieldName(isPBIR ? filter["field"] : filter["expression"], filterFieldLocation, FilterType.ADVANCED, isPBIR);
                if (fieldName != null)
                {
                    if (filter["filter"] == null)
                    {
                        simpleFilter.Add(fieldName, " No filter selected");
                        return simpleFilter;
                    }
                    var condition = filter["filter"]?["Where"]?[0]?["Condition"];
                    string valuesString;

                    if (condition != null)
                    {
                        JToken? comparison;
                        JToken? secondComparison;
                        if (condition["And"] != null)
                        {
                            comparison = condition?["And"]?["Left"];
                            secondComparison = condition?["And"]?["Right"];
                            valuesString = advanceComparisonString(comparison) + " And" + advanceComparisonString(secondComparison);
                        }
                        else if (condition["Or"] != null)
                        {
                            comparison = condition?["Or"]?["Left"];
                            secondComparison = condition?["Or"]?["Right"];
                            valuesString = advanceComparisonString(comparison) + " Or" + advanceComparisonString(secondComparison);
                        }
                        else
                        {
                            comparison = condition;
                            valuesString = advanceComparisonString(comparison);
                        }
                        simpleFilter.Add(fieldName, valuesString);
                        return simpleFilter;
                    }
                }
            }
            catch (Exception ex)
            {
                simpleFilter.Add(fieldName, "");
                Console.WriteLine(ex.ToString());
            }
            return simpleFilter;
        }

        /// <summary>
        /// Simplifies a TopN filter from JSON.
        /// </summary>
        public static Dictionary<string, string>? simplifyTopNFilter(JObject filter, FilterFieldLocation filterFieldLocation, bool isPBIR = false)
        {
            string fieldName = "";
            Dictionary<string, string> simpleFilter = new Dictionary<string, string>();
            try
            {
                fieldName = GetFieldName(isPBIR ? filter["field"] : filter["expression"], filterFieldLocation, FilterType.TOPN, isPBIR);
                if (fieldName != null)
                {
                    var filterObj = filter["filter"];

                    string? top = null,
                        bottom = null;

                    if (filterObj != null)
                    {
                        Dictionary<string, string> entityMap = new Dictionary<string, string>();
                        var query = filterObj?["From"]?[0]?["Expression"]?["Subquery"]?["Query"];
                        var entities = query?["From"];
                        foreach (var entity in entities ?? new JArray())
                        {
                            if (entity != null && entity["Name"] != null && entity["Entity"] != null)
                                entityMap.Add((entity["Name"] ?? "").ToString(), (entity["Entity"] ?? "").ToString());
                        }
                        if (query != null && query["Top"] != null)
                            top = query["Top"]?.ToString();
                        else if (query != null && query["Bottom"] != null)
                            bottom = query["Bottom"]?.ToString();
                        else
                            return null;

                        var orderBy = query["OrderBy"];
                        string? orderByFunction = null;
                        if (orderBy != null)
                        {
                            var orderByExpression = orderBy[0]?["Expression"];
                            if (orderByExpression != null)
                            {
                                if (orderByExpression["Aggregation"] != null)
                                {
                                    orderByFunction = topNFunctionMap[orderByExpression?["Aggregation"]?["Function"]?.ToString() ?? string.Empty];
                                    orderByExpression = orderByExpression?["Aggregation"]?["Expression"];
                                }
                                var orderByColOrMeasure = orderByExpression?["Column"] != null ? orderByExpression["Column"] : orderByExpression?["Measure"];
                                var colOrMeasure = orderByExpression?["Column"] != null ? "Column" : "Measure";
                                string topNFilter =
                                    colOrMeasure == "Column"
                                        ? entityMap[orderByColOrMeasure?["Expression"]?["SourceRef"]?["Source"]?.ToString().Trim() ?? string.Empty]
                                            + "."
                                            + orderByColOrMeasure?["Property"]?.ToString().Trim()
                                        : orderByColOrMeasure?["Property"]?.ToString().Trim() ?? string.Empty;
                                string orderByField = topNFilter + "( " + colOrMeasure + " )";

                                if (colOrMeasure == "Column" && !columnDict.ContainsKey(topNFilter))
                                {
                                    columnDict.Add(topNFilter, true);
                                }
                                else if (colOrMeasure == "Measure" && !measureDict.ContainsKey(topNFilter))
                                {
                                    measureDict.Add(topNFilter, true);
                                }
                                string valuesString = (top != null ? "Top " + top : "Bottom " + bottom) + " Order By " + (orderByFunction != null ? orderByFunction + " of " : "") + orderByField;
                                simpleFilter.Add(fieldName, valuesString);
                                return simpleFilter;
                            }
                        }
                    }
                    else
                    {
                        simpleFilter.Add(fieldName, "No filter selected");
                    }
                }
            }
            catch (Exception)
            {
                simpleFilter.Add(fieldName, "");
            }
            return simpleFilter;
        }

        /// <summary>
        /// Simplifies a list of filters and returns structured filter data.
        /// </summary>
        public static SimplifyFilterData simplify(JArray filters, bool isPBIR = false, FilterFieldLocation filterFieldLocation = FilterFieldLocation.VISUAL)
        {
            filterFieldList = [];
            List<Dictionary<string, string>> filterList = [];

            foreach (JObject filter in filters.Cast<JObject>())
            {
                Dictionary<string, string> filterVal = [];
                string filterType = filter["type"]?.ToString() ?? "Categorical";

                switch (filterType)
                {
                    case "Categorical":
                        filterVal = simplifyCategoricalFilter(filter, filterFieldLocation, isPBIR);
                        if (filterVal != null)
                        {
                            filterList.Add(filterVal);
                        }
                        break;

                    case "Advanced":
                        filterVal = simplifyAdvanceFilter(filter, filterFieldLocation, isPBIR);
                        if (filterVal != null)
                        {
                            filterList.Add(filterVal);
                        }
                        break;

                    case "TopN":
                        filterVal = simplifyTopNFilter(filter, filterFieldLocation, isPBIR) ?? [];
                        if (filterVal != null)
                        {
                            filterList.Add(filterVal);
                        }
                        break;

                    default:
                        continue;
                }
            }

            return new()
            {
                FilterFieldList = filterFieldList,
                filterList = filterList,
                columnDict = columnDict,
                measureDict = measureDict,
            };
        }

        /// <summary>
        /// Converts a list of filter dictionaries to a string representation.
        /// </summary>
        public static string ConvertFilterListToStr(List<Dictionary<string, string>> filterList)
        {
            var reportFilterString = "";
            int i = 1;
            if (filterList != null && filterList.Count > 0)
                foreach (var filter in filterList)
                {
                    foreach (var kvp in filter)
                    {
                        string key = kvp.Key;
                        string value = kvp.Value;
                        reportFilterString += i.ToString() + ") " + key + "=>" + value + "  \n";
                        i++;
                    }
                }
            return reportFilterString;
        }
    }
}