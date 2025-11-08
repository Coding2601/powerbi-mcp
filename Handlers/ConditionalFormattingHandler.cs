using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PowerBI_MCP.DTO;
using PowerBI_MCP.Handlers;

namespace PowerBI_MCP.Handlers
{
    /// <summary>
    /// Handles extraction and parsing of conditional formatting details from visual configuration JSON.
    /// </summary>
    public class ConditionalFormattingHandler
    {
        /// <summary>
        /// Extracts a list of conditional formatting summaries from the provided visual config.
        /// </summary>
        /// <param name="config">The JSON token representing the visual configuration.</param>
        /// <returns>List of <see cref="ConditionalFormattingSummary"/> objects.</returns>
        public List<ConditionalFormattingSummary> GetConditionalFormattingSummary(JToken? config)
        {
            List<ConditionalFormattingSummary> conditionalFormattingSummaries = new List<ConditionalFormattingSummary>();
            if (config == null)
                return conditionalFormattingSummaries;

            // Check if the config contains singleVisual objects.
            if (config["singleVisual"]?["objects"] != null)
            {
                JObject configObj = JObject.Parse((config["singleVisual"]?["objects"] ?? new JObject() { }).ToString());
                foreach (var obj in configObj.Properties())
                {
                    try
                    {
                        // Create a summary for each formatting object.
                        ConditionalFormattingSummary conditionalFormattingSummary = new ConditionalFormattingSummary
                        {
                            AppliedOn = GetAppliedOn(obj.Name.ToString(), config["singleVisual"]?["visualType"]?.ToString()),
                            ConditionalFormattingDetails = new List<ConditionalFormattingDetail>(),
                        };
                        // Parse each formatting element.
                        foreach (JToken ele in JToken.Parse(obj.Value.ToString()))
                        {
                            try
                            {
                                JToken? properties = ele["properties"];
                                // Extract various conditional formatting details.
                                if (properties != null && properties["backColor"] != null)
                                    getConditionalFormattingDetails("Background Color", properties["backColor"]?["solid"]?["color"]?["expr"], ele, ref conditionalFormattingSummary);
                                if (properties != null && properties["fontColor"] != null)
                                    getConditionalFormattingDetails("Font Color", properties["fontColor"]?["solid"]?["color"]?["expr"], ele, ref conditionalFormattingSummary);
                                if (properties != null && properties["icon"] != null)
                                    getConditionalFormattingDetails("Icon", properties["icon"]?["value"]?["expr"], ele, ref conditionalFormattingSummary);
                                if (properties != null && properties["webURL"] != null)
                                    getConditionalFormattingDetails("WebURL", properties["webURL"]?["expr"], ele, ref conditionalFormattingSummary);
                                if (properties != null && properties["labelColor"] != null)
                                    getConditionalFormattingDetails("Label Color", properties["labelColor"]?["solid"]?["color"]?["expr"], ele, ref conditionalFormattingSummary);
                                if (properties != null && properties["titleColor"] != null)
                                    getConditionalFormattingDetails("Title Color", properties["titleColor"]?["solid"]?["color"]?["expr"], ele, ref conditionalFormattingSummary);
                                if (properties != null && properties["gridlineColor"] != null)
                                    getConditionalFormattingDetails("Gridline Color", properties["gridlineColor"]?["solid"]?["color"]?["expr"], ele, ref conditionalFormattingSummary);
                                if (properties != null && properties["start"] != null)
                                    getConditionalFormattingDetails("Range (Start)", properties["start"]?["expr"], ele, ref conditionalFormattingSummary);
                                if (properties != null && properties["end"] != null)
                                    getConditionalFormattingDetails("Range (End)", properties["end"]?["expr"], ele, ref conditionalFormattingSummary);
                                if (properties != null && properties["detailColor"] != null)
                                    getConditionalFormattingDetails("Detail Color", properties["detailColor"]?["solid"]?["color"]?["expr"], ele, ref conditionalFormattingSummary);
                                if (properties != null && properties["color"] != null)
                                    getConditionalFormattingDetails("Value Color", properties["color"]?["solid"]?["color"]?["expr"], ele, ref conditionalFormattingSummary);
                                if (properties != null && properties["backgroundColor"] != null)
                                    getConditionalFormattingDetails("Background Color", properties["backgroundColor"]?["solid"]?["color"]?["expr"], ele, ref conditionalFormattingSummary);
                                if (properties != null && properties["fill"] != null)
                                    getConditionalFormattingDetails("Color", properties["fill"]?["solid"]?["color"]?["expr"], ele, ref conditionalFormattingSummary);
                            }
                            catch (Exception ex)
                            {
                                // Log and continue on error.
                                GlobalHandler.WriteCrashLog(ex.ToString());
                            }
                        }
                        // Add summary if details exist.
                        if (conditionalFormattingSummary.ConditionalFormattingDetails?.Count > 0)
                            conditionalFormattingSummaries.Add(conditionalFormattingSummary);
                    }
                    catch (Exception ex)
                    {
                        GlobalHandler.WriteCrashLog(ex.ToString());
                    }
                }
            }
            return conditionalFormattingSummaries;
        }

        /// <summary>
        /// Determines the axis or element the formatting is applied on, based on key and visual type.
        /// </summary>
        /// <param name="key">Formatting key.</param>
        /// <param name="visualType">Type of the visual.</param>
        /// <returns>Axis or element name.</returns>
        public string GetAppliedOn(string key, string? visualType)
        {
            if (key == "valueAxis" && (visualType == "barChart" || visualType == "clusteredBarChart" || visualType == "hundredPercentStackedBarChart"))
                return "X-Axis";
            else if (key == "categoryAxis" && (visualType == "barChart" || visualType == "clusteredBarChart" || visualType == "hundredPercentStackedBarChart"))
                return "Y-Axis";
            else if (key == "valueAxis")
                return "Y-Axis";
            else if (key == "categoryAxis")
                return "X-Axis";
            else if (key == "totals")
                return "Total Labels";
            else if (key == "labels")
                return "Data Labels";
            else if (key == "legend")
                return "Legend";
            else if (key == "smallMultiplesLayout")
                return "Small Multiples";
            else if (key == "subheader")
                return "Small Multiples";
            else if (key == "dataPoint")
                return "Data Point";
            else
                return key;
        }

        /// <summary>
        /// Extracts and adds conditional formatting details for a specific format type.
        /// </summary>
        /// <param name="formatName">Name of the format (e.g., "Background Color").</param>
        /// <param name="expr">JSON token representing the formatting expression.</param>
        /// <param name="ele">JSON token for the formatting element.</param>
        /// <param name="conditionalFormattingSummary">Reference to the summary to update.</param>
        public void getConditionalFormattingDetails(string formatName, JToken? expr, JToken? ele, ref ConditionalFormattingSummary conditionalFormattingSummary)
        {
            if (expr == null)
                return;
            ConditionalFormattingDetail conditionalFormattingDetail = new ConditionalFormattingDetail
            {
                ColumnName = ele?["selector"]?["metadata"] != null ? ele?["selector"]?["metadata"]?.ToString() : null,
                FormatName = formatName,
            };
            // Check for gradient, rule, or field value formatting.
            if (expr["FillRule"] != null)
                GetGradientFormattingDetails(expr["FillRule"], ref conditionalFormattingDetail, ref conditionalFormattingSummary);
            else if (expr["Conditional"] != null)
                GetRuleFormattingDetails(expr["Conditional"], ref conditionalFormattingDetail, ref conditionalFormattingSummary);
            else if (expr["Aggregation"] != null)
                GetFieldValueFormattingDetails(JsonConvert.SerializeObject(expr["Aggregation"]), ref conditionalFormattingDetail, ref conditionalFormattingSummary);
        }

        /// <summary>
        /// Extracts gradient formatting details and adds them to the summary.
        /// </summary>
        public void GetGradientFormattingDetails(JToken? gradientJsonObj, ref ConditionalFormattingDetail conditionalFormattingDetail, ref ConditionalFormattingSummary conditionalFormattingSummary)
        {
            if (gradientJsonObj == null)
                return;
            conditionalFormattingDetail.FormatStyle = "Gradient";
            if (gradientJsonObj["Input"] != null)
                GetDependedColumnNameAndSummarization(JsonConvert.SerializeObject(gradientJsonObj["Input"]), ref conditionalFormattingDetail);

            conditionalFormattingDetail.gradientFormattingExpression = new GradientFormattingExpression();
            // Extract min, mid, max colors and values.
            if (gradientJsonObj["FillRule"]?["linearGradient2"]?["min"]?["color"]?["Literal"]?["Value"] != null)
                conditionalFormattingDetail.gradientFormattingExpression.minColor = gradientJsonObj["FillRule"]?["linearGradient2"]?["min"]?["color"]?["Literal"]?["Value"]?.ToString();
            if (gradientJsonObj["FillRule"]?["linearGradient2"]?["max"]?["color"]?["Literal"]?["Value"] != null)
                conditionalFormattingDetail.gradientFormattingExpression.maxColor = gradientJsonObj["FillRule"]?["linearGradient2"]?["max"]?["color"]?["Literal"]?["Value"]?.ToString();
            if (gradientJsonObj["FillRule"]?["linearGradient2"]?["mid"]?["color"]?["Literal"]?["Value"] != null)
                conditionalFormattingDetail.gradientFormattingExpression.midColor = gradientJsonObj["FillRule"]?["linearGradient2"]?["mid"]?["color"]?["Literal"]?["Value"]?.ToString();
            if (gradientJsonObj["FillRule"]?["linearGradient2"]?["min"]?["value"]?["Literal"]?["Value"] != null)
                conditionalFormattingDetail.gradientFormattingExpression.minValue = gradientJsonObj["FillRule"]?["linearGradient2"]?["min"]?["value"]?["Literal"]?["Value"]?.ToString();
            if (gradientJsonObj["FillRule"]?["linearGradient2"]?["max"]?["value"]?["Literal"]?["Value"] != null)
                conditionalFormattingDetail.gradientFormattingExpression.maxValue = gradientJsonObj["FillRule"]?["linearGradient2"]?["max"]?["value"]?["Literal"]?["Value"]?.ToString();
            if (gradientJsonObj["FillRule"]?["linearGradient2"]?["mid"]?["value"]?["Literal"]?["Value"] != null)
                conditionalFormattingDetail.gradientFormattingExpression.midValue = gradientJsonObj["FillRule"]?["linearGradient2"]?["mid"]?["value"]?["Literal"]?["Value"]?.ToString();

            conditionalFormattingSummary.ConditionalFormattingDetails?.Add(conditionalFormattingDetail);
        }

        /// <summary>
        /// Extracts depended column name and summarization from a JSON string.
        /// </summary>
        public void GetDependedColumnNameAndSummarization(string dependencyDetailJson, ref ConditionalFormattingDetail conditionalFormattingDetail, bool isTextCol = false)
        {
            if (dependencyDetailJson == null)
                return;
            dynamic? dependencyDetailObj = JsonConvert.DeserializeObject<dynamic>(dependencyDetailJson);
            if (dependencyDetailObj?.Aggregation != null)
            {
                conditionalFormattingDetail.DependedColumnName = dependencyDetailObj.Aggregation.Expression.Column.Property;
                conditionalFormattingDetail.Summarization = GetSummarization((int)dependencyDetailObj.Aggregation.Function, isTextCol);
            }
            else if (dependencyDetailObj?.Measure != null)
            {
                conditionalFormattingDetail.DependedColumnName = dependencyDetailObj.Measure.Property;
                conditionalFormattingDetail.Summarization = null;
            }
        }

        /// <summary>
        /// Returns the summarization operation name based on its ID.
        /// </summary>
        public string GetSummarization(int OperationID, bool isTextCol = false)
        {
            string[] summarizationOperations = { "Sum", "Average", "Count (Distinct)", "Minimum", "Maximum", "Count", "Median", "Standard deviation", "Variance" };
            if (isTextCol && OperationID == 3)
                return "First";
            if (isTextCol && OperationID == 4)
                return "Last";
            if (OperationID >= 0 && OperationID <= 8)
                return summarizationOperations[OperationID];
            return "";
        }

        /// <summary>
        /// Extracts rule-based formatting details and adds them to the summary.
        /// </summary>
        public void GetRuleFormattingDetails(JToken? ruleJsonObj, ref ConditionalFormattingDetail conditionalFormattingDetail, ref ConditionalFormattingSummary conditionalFormattingSummary)
        {
            if (ruleJsonObj == null)
                return;
            conditionalFormattingDetail.FormatStyle = "Rules";
            // Extract depended column and summarization for various rule structures.
            if (ruleJsonObj["Cases"]?[0]?["Condition"]?["And"] != null)
                GetDependedColumnNameAndSummarization(
                    JsonConvert.SerializeObject(ruleJsonObj["Cases"]?[0]?["Condition"]?["And"]?["Left"]?["Comparison"]?["Left"]),
                    ref conditionalFormattingDetail,
                    false
                );
            else if (ruleJsonObj["Cases"]?[0]?["Condition"]?["Not"] != null && ruleJsonObj["Cases"]?[0]?["Condition"]?["Not"]?["Expression"]?["Comparison"] != null)
                GetDependedColumnNameAndSummarization(
                    JsonConvert.SerializeObject(ruleJsonObj["Cases"]?[0]?["Condition"]?["Not"]?["Expression"]?["Comparison"]?["Left"]),
                    ref conditionalFormattingDetail,
                    true
                );
            else if (ruleJsonObj["Cases"]?[0]?["Condition"]?["Not"] != null && ruleJsonObj["Cases"]?[0]?["Condition"]?["Not"]?["Expression"]?["StartsWith"] != null)
                GetDependedColumnNameAndSummarization(
                    JsonConvert.SerializeObject(ruleJsonObj["Cases"]?[0]?["Condition"]?["Not"]?["Expression"]?["StartsWith"]?["Left"]),
                    ref conditionalFormattingDetail,
                    true
                );
            else if (ruleJsonObj["Cases"]?[0]?["Condition"]?["Not"] != null && ruleJsonObj["Cases"]?[0]?["Condition"]?["Not"]?["Expression"]?["Contains"] != null)
                GetDependedColumnNameAndSummarization(
                    JsonConvert.SerializeObject(ruleJsonObj["Cases"]?[0]?["Condition"]?["Not"]?["Expression"]?["Contains"]?["Left"]),
                    ref conditionalFormattingDetail,
                    true
                );
            else if (ruleJsonObj["Cases"]?[0]?["Condition"]?["Comparison"] != null)
                GetDependedColumnNameAndSummarization(JsonConvert.SerializeObject(ruleJsonObj["Cases"]?[0]?["Condition"]?["Comparison"]?["Left"]), ref conditionalFormattingDetail, true);
            else if (ruleJsonObj["Cases"]?[0]?["Condition"]?["StartsWith"] != null)
                GetDependedColumnNameAndSummarization(JsonConvert.SerializeObject(ruleJsonObj["Cases"]?[0]?["Condition"]?["StartsWith"]?["Left"]), ref conditionalFormattingDetail, true);
            else if (ruleJsonObj["Cases"]?[0]?["Condition"]?["Contains"] != null)
                GetDependedColumnNameAndSummarization(JsonConvert.SerializeObject(ruleJsonObj["Cases"]?[0]?["Condition"]?["Contains"]?["Left"]), ref conditionalFormattingDetail, true);

            conditionalFormattingDetail.ruleFormattingExpression = new RuleFormattingExpression();
            // Parse each rule case.
            foreach (JToken Case in ruleJsonObj["Cases"] ?? new JArray() { })
            {
                try
                {
                    Rulex rule = new Rulex();
                    // Handle AND condition.
                    if (Case["Condition"]?["And"] != null)
                    {
                        var ComparisonLeftKindValue = Case["Condition"]?["And"]?["Left"]?["Comparison"]?["ComparisonKind"];
                        var ComparisonRightKindValue = Case["Condition"]?["And"]?["Right"]?["Comparison"]?["ComparisonKind"];
                        if (ComparisonLeftKindValue != null && ComparisonRightKindValue != null)
                            rule.condition =
                                $"{conditionalFormattingDetail.DependedColumnName} is {getCompOpr((int)ComparisonLeftKindValue)} {getCompVal(Case["Condition"]?["And"]?["Left"]?["Comparison"]?["Right"])} And {conditionalFormattingDetail.DependedColumnName} is {getCompOpr((int)ComparisonRightKindValue)} {getCompVal(Case["Condition"]?["And"]?["Right"]?["Comparison"]?["Right"])}";
                    }
                    // Handle NOT/Comparison.
                    else if (Case["Condition"]?["Not"] != null && Case["Condition"]?["Not"]?["Expression"]?["Comparison"] != null)
                    {
                        string text = getCompVal(Case?["Condition"]?["Not"]?["Expression"]?["Comparison"]?["Right"]);
                        if (text == "null")
                            rule.condition = $"{conditionalFormattingDetail.DependedColumnName} is not blank";
                        else if (text == "''")
                            rule.condition = $"{conditionalFormattingDetail.DependedColumnName} is not empty";
                        else
                            rule.condition = $"{conditionalFormattingDetail.DependedColumnName} is not {text}";
                    }
                    // Handle NOT/StartsWith.
                    else if (Case["Condition"]?["Not"] != null && Case["Condition"]?["Not"]?["Expression"]?["StartsWith"] != null)
                    {
                        rule.condition = $"{conditionalFormattingDetail.DependedColumnName} does not starts with {getCompVal(Case?["Condition"]?["Not"]?["Expression"]?["StartsWith"]?["Right"])}";
                    }
                    // Handle NOT/Contains.
                    else if (Case["Condition"]?["Not"] != null && Case["Condition"]?["Not"]?["Expression"]?["Contains"] != null)
                    {
                        rule.condition = $"{conditionalFormattingDetail.DependedColumnName} does not contains {getCompVal(Case["Condition"]?["Not"]?["Expression"]?["Contains"]?["Right"])}";
                    }
                    // Handle direct comparison.
                    else if (Case["Condition"]?["Comparison"] != null)
                    {
                        string text = getCompVal(Case["Condition"]?["Comparison"]?["Right"]);
                        if (text == "null")
                            rule.condition = $"{conditionalFormattingDetail.DependedColumnName} is blank";
                        else if (text == "''")
                            rule.condition = $"{conditionalFormattingDetail.DependedColumnName} is empty";
                        else
                            rule.condition = $"{conditionalFormattingDetail.DependedColumnName} is {text}";
                    }
                    // Handle StartsWith.
                    else if (Case["Condition"]?["StartsWith"] != null)
                    {
                        rule.condition = $"{conditionalFormattingDetail.DependedColumnName} starts with {getCompVal(Case["Condition"]?["StartsWith"]?["Right"])}";
                    }
                    // Handle Contains.
                    else if (Case["Condition"]?["Contains"] != null)
                    {
                        rule.condition = $"{conditionalFormattingDetail.DependedColumnName} contains {getCompVal(Case["Condition"]?["Contains"]?["Right"])}";
                    }
                    rule.value = Case?["Value"]?["Literal"]?["Value"]?.ToString();
                    conditionalFormattingDetail.ruleFormattingExpression.rules?.Add(rule);
                }
                catch (Exception ex)
                {
                    GlobalHandler.WriteCrashLog(ex.ToString());
                }
            }

            conditionalFormattingSummary.ConditionalFormattingDetails?.Add(conditionalFormattingDetail);
        }

        /// <summary>
        /// Returns the comparison operator string for a given operator ID.
        /// </summary>
        public string getCompOpr(int operatorID)
        {
            if (operatorID == 1)
                return ">";
            else if (operatorID == 2)
                return ">=";
            else if (operatorID == 3)
                return "<";
            else if (operatorID == 4)
                return "<=";
            return "";
        }

        /// <summary>
        /// Extracts the comparison value from a JSON token.
        /// </summary>
        public string getCompVal(JToken? valToken)
        {
            if (valToken == null)
                return "";
            if (valToken["RangePercent"] != null)
                return valToken?["RangePercent"]?["Percent"]?.ToString() ?? "";
            else if (valToken["Literal"] != null)
                return valToken?["Literal"]?["Value"]?.ToString() ?? "";
            return "";
        }

        /// <summary>
        /// Extracts field value formatting details and adds them to the summary.
        /// </summary>
        public void GetFieldValueFormattingDetails(string fieldValueJson, ref ConditionalFormattingDetail conditionalFormattingDetail, ref ConditionalFormattingSummary conditionalFormattingSummary)
        {
            dynamic? fieldValueJsonObj = JsonConvert.DeserializeObject<dynamic>(fieldValueJson);
            conditionalFormattingDetail.FormatStyle = "Field Value";
            conditionalFormattingDetail.DependedColumnName = fieldValueJsonObj?.Expression?.Column?.Property;
            conditionalFormattingDetail.Summarization = GetSummarization((int)fieldValueJsonObj?.Function, true);

            conditionalFormattingSummary.ConditionalFormattingDetails?.Add(conditionalFormattingDetail);
        }
    }
}