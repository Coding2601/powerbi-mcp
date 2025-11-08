using PowerBI_MCP.DTO;
using Microsoft.AnalysisServices.Tabular;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

namespace PowerBI_MCP.Handlers
{
    /// <summary>
    /// Provides utility methods for detecting unused or special objects in the tabular model.
    /// </summary>
    public class ModelDependencyHandler
    {
        public const string TABLE_FEILD_Separator = "__|__TABLE_FEILD_Separator__|__";
        /// <summary>
        /// The name of the extended property used to identify field parameters.
        /// </summary>
        const string FP_EXTENDED_PROPERTY_NAME = "ParameterMetadata";

        /// <summary>
        /// The expected value of the extended property for field parameters (with whitespace removed).
        /// </summary>
        const string FP_EXTENDED_PROPERTY_VALUE = "{\"version\":3,\"kind\":2}";

        /// <summary>
        /// Checks if a given table is a Field Parameter table by inspecting its columns' extended properties.
        /// </summary>
        /// <param name="table">The table to check.</param>
        /// <returns>True if the table is a Field Parameter table; otherwise, false.</returns>

        public readonly Dictionary<string, Dictionary<string, string>>? MeasureDetails;
        public readonly Dictionary<string, Dictionary<string, string>>? calColumnDetails;
        private readonly List<TableSummary>? tables;
        private readonly ModelDocumentation modelDocumentation;
        public ModelDependencyHandler(ModelDocumentation modelDoc)
        {
            this.modelDocumentation = modelDoc;
            MeasureDetails = [];
            calColumnDetails = [];
            foreach (var meas in modelDoc.Measures ?? [])
            {
                try
                {
                    if (meas.MeasureName != null && meas.TableName != null && meas.Expression != null && MeasureDetails.ContainsKey(meas.MeasureName) == false)
                        MeasureDetails.Add(
                            meas.MeasureName,
                            new()
                            {
                                    { "table", meas.TableName },
                                    { "expression", meas.Expression.Trim() },
                                    { "reportId", meas.ReportId ?? "report id" },
                                    { "reportName", meas.ReportName ?? "report name" },
                                    { "visible", meas.Visible ?? "" },
                                    { "folder", meas.Folder ?? "" },
                                    { "description", meas.Description ?? "" },
                                    { "formatting", meas.Format ?? "" },
                                    { "Origin", meas.Origin ?? "" },
                                    { "IsHidden", meas.Visible ?? "" },
                            }
                        );
                }
                catch (Exception ex)
                {
                    GlobalHandler.WriteCrashLog(ex.ToString());
                }
            }

            tables = modelDocumentation.Tables ?? new List<TableSummary>();

            if (tables != null)
            {
                foreach (var column in modelDocumentation.Columns ?? [])
                {
                    try
                    {
                        string columnName = column.ColumnName ?? "";
                        if (column.ColumnType == ColumnType.Calculated.ToString())
                        {
                            calColumnDetails.Add(
                                columnName,
                                new()
                                {
                                        { "expression", column.Expression?.Trim() ??"" },
                                        { "table", column.TableName ??"" },
                                        { "isHidden", column.Visible=="No" ? "Yes" : "No" },
                                        { "dataType", column.DataType?.ToString() ??"" },
                                        { "description", column.Description?.ToString()  ??""},
                                        { "formatting", column.Format ??"" },
                                        { "displayFolder", column.Folder?.ToString()  ??""},
                                        { "isKey", column.IsKeyColumn   ??"" },
                                }
                            );
                        }
                    }
                    catch (Exception ex)
                    {
                        GlobalHandler.WriteCrashLog(ex.ToString());
                    }
                }

            }
        }

        public static bool CheckIsFieldParameter(Table table)
        {
            bool isFieldParameter = false;
            try
            {
                // Iterate through each column in the table
                foreach (var column in table.Columns)
                {
                    try
                    { 
                        foreach (ExtendedProperty exPara in column.ExtendedProperties)
                        {
                            if (exPara.Name == FP_EXTENDED_PROPERTY_NAME)
                            {
                                JObject valueJson = JObject.Parse(((JsonExtendedProperty)exPara).Value); // Moved inside braces
                                if ((valueJson?["kind"]?.ToString() == "2" || valueJson?["Kind"]?.ToString() == "2" )
                                    && (valueJson?["version"]?.ToString() == "3" || valueJson?["Version"]?.ToString() == "3"))
                                   isFieldParameter = true;
                            }
                        }

                        // Check if the column has exactly one group-by column (Field Parameter pattern)
                        if (column.RelatedColumnDetails?.GroupByColumns?.Count() == 1)
                        {
                            foreach (var groupByColumn in column.RelatedColumnDetails.GroupByColumns)
                            {
                                try
                                {
                                    // Inspect extended properties of the grouping column
                                    foreach (var extendedProperty in groupByColumn.GroupingColumn.ExtendedProperties)
                                    {
                                        try
                                        {
                                            // Check for the specific extended property name and value
                                            if (
                                                (extendedProperty.Name == FP_EXTENDED_PROPERTY_NAME)
                                                && (
                                                    ((JsonExtendedProperty)extendedProperty)
                                                        .Value.Replace(" ", string.Empty)
                                                        .Replace("\r\n", string.Empty)
                                                        .Replace("\n", string.Empty)
                                                        .Replace("\r", string.Empty) == FP_EXTENDED_PROPERTY_VALUE
                                                )
                                            )
                                            {
                                                isFieldParameter = true;
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            // Log and continue if an error occurs while checking extended properties
                                            GlobalHandler.WriteCrashLog(ex.ToString());
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    // Log and continue if an error occurs while iterating group-by columns
                                    GlobalHandler.WriteCrashLog(ex.ToString());
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // Log and continue if an error occurs while iterating columns
                        GlobalHandler.WriteCrashLog(ex.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                // Log and continue if an error occurs at the table level
                GlobalHandler.WriteCrashLog(ex.ToString());
            }
            return isFieldParameter;
        }

        public static FormattedExpressionAndDependency FormatExpressions(string expression, string expressionTable, List<TableSummary> tables, List<ColumnSummary> columns, List<MeasureSummary> measures)
        {
            string formattedExpression = expression;
            HashSet<ExpressionDependency> dependency = [];

            foreach (var table in tables)
            {
                var tableName = table.TableName ?? "";
                var escapedTableName = Regex.Escape(table.TableName ?? "");
                string regexToFindOnlyTable = @"\b(?:'?" + escapedTableName + @"'?)\b(?!['\s]*\[)(?!\s*[\[""\]])";
                //string regexToFindOnlyTable = @"\b(?:'?" + escapedTableName + @"'?)\b(?!['\s]*\[)";

                //string regexToFindOnlyTable2 = @"\b'?" + escapedTableName + @"'?\b(?!\s*[\[""\]])";

                if (Regex.IsMatch(formattedExpression, regexToFindOnlyTable, RegexOptions.IgnoreCase))
                {
                    formattedExpression = Regex.Replace(formattedExpression, regexToFindOnlyTable, "'" + tableName + "'", RegexOptions.IgnoreCase);
                    dependency.Add(new() { Type = DependencyType.Table, FieldName = "", TableName = tableName });
                }
            }

            foreach (var col in columns)
            {
                var tableName = col.TableName ?? "";
                var colName = col.ColumnName ?? "";
                var escapedTableName = Regex.Escape(col.TableName ?? "");
                var escapedColName = Regex.Escape(col.ColumnName ?? "");
                var regexPatternNoQuotes = @"\s*" + escapedTableName + @"\s*\[" + escapedColName + @"\]\s*";
                var regexPatternSpaceBetween = @"\s*'" + escapedTableName + @"'\s+\[" + escapedColName + @"\]\s*";
                var regexPatternNoTableNames = @"(?<!\s*'" + @"\s*)\s*\[" + escapedColName + @"\]\s*";


                if (formattedExpression.IndexOf("[" + colName + "]", StringComparison.OrdinalIgnoreCase) > -1)
                {
                    if (formattedExpression.IndexOf("'" + tableName + "'[" + colName + "]", StringComparison.OrdinalIgnoreCase) > -1)
                    {
                        dependency.Add(new() { Type = DependencyType.Column, FieldName = colName, TableName = tableName });
                    }
                    else if (Regex.IsMatch(formattedExpression, regexPatternNoQuotes, RegexOptions.IgnoreCase) ||
                     Regex.IsMatch(formattedExpression, regexPatternSpaceBetween, RegexOptions.IgnoreCase))
                    {
                        formattedExpression = Regex.Replace(formattedExpression,
                                        regexPatternNoQuotes,
                                        "'" + tableName + "'[" + colName + "]",
                                        RegexOptions.IgnoreCase);

                        formattedExpression = Regex.Replace(formattedExpression,
                                            regexPatternSpaceBetween,
                                            "'" + tableName + "'[" + colName + "]",
                                            RegexOptions.IgnoreCase);
                        dependency.Add(new() { Type = DependencyType.Column, FieldName = colName, TableName = tableName });
                    }

                    else if (Regex.IsMatch(formattedExpression, regexPatternNoTableNames, RegexOptions.IgnoreCase) &&
                        formattedExpression.IndexOf(tableName, StringComparison.OrdinalIgnoreCase) == -1)
                    {
                        formattedExpression = Regex.Replace(formattedExpression,
                                        regexPatternNoTableNames,
                                        "'" + expressionTable + "'[" + colName + "]",
                                        RegexOptions.IgnoreCase);
                        dependency.Add(new() { Type = DependencyType.Column, FieldName = colName, TableName = expressionTable });
                    }

                }
            }

            foreach (var meas in measures)
            {
                if (formattedExpression.IndexOf("[" + meas.MeasureName + "]", StringComparison.OrdinalIgnoreCase) > -1)
                {
                    dependency.Add(new() { Type = DependencyType.Measure, FieldName = meas.MeasureName ?? "", TableName = meas.TableName ?? "" });
                }
            }

            return new() { FormatedExpression = formattedExpression, Dependency = dependency.ToArray() };
        }

        // public Dictionary<string, Dictionary<string, bool>> CreateColumnDependencies(ModelDocumentation modelDoc)
        // {
        //     Dictionary<string, Dictionary<string, bool>> columnDependency = [];
        //     Dictionary<string, Dictionary<string, HashSet<string>>> columnDependent = [];


        //     if (modelDoc.Tables != null && columnDependency != null)
        //     {
        //         // Checking and Adding Sort by columns dependency
        //         foreach (var column in modelDoc.Columns)
        //         {
        //             try
        //             {
        //                 if (column.SortByColumn != null)
        //                 {
        //                     columnDependency[column.TableName + TABLE_FEILD_Separator + column.SortByColumn]["isUsedInSortByColumn"] = true;
        //                     columnDependent[column.TableName + TABLE_FEILD_Separator + column.SortByColumn]["dependentSortByColumns"].Add(column.TableName + TABLE_FEILD_Separator + column.ColumnName);
        //                 }
        //             }
        //             catch (Exception ex)
        //             {
        //                 GlobalHandler.WriteCrashLog(ex.ToString());
        //             }
        //         }

        //         // Checking and Adding Hierarchy columns dependency
        //         //foreach (var hierarchy in table.Hierarchies)
        //         //{
        //         //    try
        //         //    {
        //         //        foreach (var column in hierarchy.Levels)
        //         //        {
        //         //            try
        //         //            {
        //         //                columnDependency.objectDependency[table.Name + "." + column.Column.Name]["isUsedInHierarchy"] = true;
        //         //                columnDependency.objectLineage[table.Name + "." + column.Column.Name]["dependentHierarchies"].Add(hierarchy.Name);
        //         //            }
        //         //            catch (Exception ex)
        //         //            {
        //         //                GlobalHelper.WriteCrashLog(ex.ToString());
        //         //            }
        //         //        }
        //         //    }
        //         //    catch (Exception ex)
        //         //    {
        //         //        GlobalHelper.WriteCrashLog(ex.ToString());
        //         //    }
        //         //}

        //     }

        //     // Checking and Adding Relationship columns dependency
        //     //if (modelDoc.Relationships != null && columnDependency != null)
        //     //    foreach (var relationship in modelDoc.Relationships)
        //     //    {
        //     //        try
        //     //        {
        //     //            columnDependency[relationship.FromColumn?.TableName + TABLE_FEILD_Separator + relationship.FromColumn?.TableName]["isUsedInRelationship"] = true;
        //     //            columnDependency[relationship.ToColumn?.TableName + TABLE_FEILD_Separator + relationship.ToColumn?.TableName]["isUsedInRelationship"] = true;


        //     //            columnDependent[relationship.FromColumn?.TableName + TABLE_FEILD_Separator + relationship.FromColumn?.ColumnName]["dependentRelationships"]
        //     //                .Add(relationship.ToColumn?.TableName+ TABLE_FEILD_Separator + relationship.ToColumn?.ColumnName);
        //     //            columnDependent[relationship.ToColumn?.TableName + TABLE_FEILD_Separator + relationship.ToColumn?.ColumnName]["dependentRelationships"]
        //     //                .Add(relationship.FromColumn?.TableName + TABLE_FEILD_Separator + relationship.FromColumn?.ColumnName);

        //     //        }
        //     //        catch (Exception ex)
        //     //        {
        //     //            GlobalHandler.WriteCrashLog(ex.ToString());
        //     //        }
        //     //    }

        //     //// Check the column used in measure and add to the dependency column

        //     //    foreach (string columnUsed in columnDependency.objectDependency.Keys)
        //     //    {
        //     //        try
        //     //        {
        //     //            string[] name = new string[2];

        //     //            if (TableColumnDotMapping != null && TableColumnDotMapping.ContainsKey(columnUsed))
        //     //            {
        //     //                name[0] = TableColumnDotMapping[columnUsed]["table"];
        //     //                name[1] = TableColumnDotMapping[columnUsed]["column"];
        //     //            }
        //     //            else
        //     //            {
        //     //                name = columnUsed.Contains('.') ? columnUsed.Split('.') : [columnUsed];
        //     //            }
        //     //            if (MeasureDetails != null)
        //     //                foreach (KeyValuePair<string, Dictionary<string, string>> keyValuePairInner in MeasureDetails)
        //     //                {
        //     //                    try
        //     //                    {
        //     //                        if (name.Length >= 2 && expression.IndexOf("'" + name[0] + "'[" + name[1] + "]", StringComparison.OrdinalIgnoreCase) > -1)
        //     //                        {
        //     //                            columnDependency.objectDependency[columnUsed]["isUsedInMeasure"] = true;
        //     //                            columnDependency.objectLineage[columnUsed]["dependentMeasures"].Add(keyValuePairInner.Key);
        //     //                        }
        //     //                    }
        //     //                    catch (Exception ex)
        //     //                    {
        //     //                        GlobalHelper.WriteCrashLog(ex.ToString());
        //     //                    }
        //     //                }

        //     //            // Check for column used in calculation groups
        //     //            if (calcGroupDetails != null)
        //     //                foreach (KeyValuePair<string, Dictionary<string, string>> keyValuePairInner in calcGroupDetails)
        //     //                {
        //     //                    try
        //     //                    {
        //     //                        if (name.Length >= 2 && expression.IndexOf("'" + name[0] + "'[" + name[1] + "]", StringComparison.OrdinalIgnoreCase) > -1)
        //     //                        {
        //     //                            columnDependency.objectDependency[columnUsed]["isUsedInCalculationGroup"] = true;
        //     //                            columnDependency.objectLineage[columnUsed]["dependentCalculationGroups"].Add(keyValuePairInner.Key);
        //     //                        }
        //     //                    }
        //     //                    catch (Exception ex)
        //     //                    {
        //     //                        GlobalHelper.WriteCrashLog(ex.ToString());
        //     //                    }
        //     //                }

        //     //            //Check for columns used in Roles
        //     //            if (rolesDetails != null)
        //     //                foreach (KeyValuePair<string, Dictionary<string, string>> keyValuePairInner in rolesDetails)
        //     //                {
        //     //                    try
        //     //                    {
        //     //                        var roleName = string.Empty;
        //     //                        if (TableRoleDotMapping != null && TableRoleDotMapping.ContainsKey(keyValuePairInner.Key))
        //     //                        {
        //     //                            roleName = TableRoleDotMapping[keyValuePairInner.Key]["role"];
        //     //                        }
        //     //                        else
        //     //                        {
        //     //                            roleName = keyValuePairInner.Key.Contains('.') ? keyValuePairInner.Key.Split(".")[1] : keyValuePairInner.Key;
        //     //                        }
        //     //                        if (name.Length >= 2 && expression.IndexOf("'" + name[0] + "'[" + name[1] + "]", StringComparison.OrdinalIgnoreCase) > -1)
        //     //                        {
        //     //                            columnDependency.objectDependency[columnUsed]["isUsedInRoles"] = true;
        //     //                            columnDependency.objectLineage[columnUsed]["dependentRoles"].Add(roleName);
        //     //                        }
        //     //                    }
        //     //                    catch (Exception ex)
        //     //                    {
        //     //                        GlobalHelper.WriteCrashLog(ex.ToString());
        //     //                    }
        //     //                }

        //     //            //Check for columns used in Incremental Refresh
        //     //            if (IRDetails != null)
        //     //                foreach (KeyValuePair<string, string> keyValuePairInner in IRDetails)
        //     //                {
        //     //                    try
        //     //                    {
        //     //                        if (
        //     //                            name.Length >= 2 && (keyValuePairInner.Value.IndexOf("\"" + name[0] + "\"[" + name[1] + "]", StringComparison.OrdinalIgnoreCase) > -1)
        //     //                            || (keyValuePairInner.Value.IndexOf("" + name[0] + "[" + name[1] + "]", StringComparison.OrdinalIgnoreCase) > -1)
        //     //                        )
        //     //                        {
        //     //                            columnDependency.objectDependency[columnUsed]["isUsedInIncrementalRefresh"] = true;
        //     //                        }
        //     //                    }
        //     //                    catch (Exception ex)
        //     //                    {
        //     //                        GlobalHelper.WriteCrashLog(ex.ToString());
        //     //                    }
        //     //                }

        //     //            //check the column used in calculated column and add to the dependency column
        //     //            if (calColumnDetails != null)
        //     //                foreach (KeyValuePair<string, Dictionary<string, string>> keyValuePairInner in calColumnDetails)
        //     //                {
        //     //                    try
        //     //                    {
        //     //                        if (name.Length >= 2 && expression.IndexOf("'" + name[0] + "'[" + name[1] + "]", StringComparison.OrdinalIgnoreCase) > -1)
        //     //                        {
        //     //                            columnDependency.objectDependency[columnUsed]["isUsedInCalculatedColumn"] = true;
        //     //                            columnDependency.objectLineage[columnUsed]["dependentCalculatedColumns"].Add(keyValuePairInner.Key);
        //     //                        }
        //     //                    }
        //     //                    catch (Exception ex)
        //     //                    {
        //     //                        GlobalHelper.WriteCrashLog(ex.ToString());
        //     //                    }
        //     //                }

        //     //            if (CalculatedTables != null)
        //     //            {
        //     //                foreach (KeyValuePair<string, Dictionary<string, string>> keyValuePairInner in CalculatedTables)
        //     //                {
        //     //                    try
        //     //                    {
        //     //                        if (name.Length >= 2 && expression.IndexOf("'" + name[0] + "'[" + name[1] + "]", StringComparison.OrdinalIgnoreCase) > -1)
        //     //                        {
        //     //                            if (name[0] != keyValuePairInner.Key)
        //     //                            {
        //     //                                columnDependency.objectDependency[columnUsed]["isUsedInCalculatedTable"] = true;
        //     //                                columnDependency.objectLineage[columnUsed]["dependentCalculatedTables"].Add(keyValuePairInner.Key);
        //     //                            }
        //     //                        }
        //     //                    }
        //     //                    catch (Exception ex)
        //     //                    {
        //     //                        GlobalHelper.WriteCrashLog(ex.ToString());
        //     //                    }
        //     //                }
        //     //            }
        //     //            if (FieldParameters != null)
        //     //            {
        //     //                foreach (KeyValuePair<string, Dictionary<string, string>> keyValuePairInner in FieldParameters)
        //     //                {
        //     //                    try
        //     //                    {
        //     //                        if (name.Length >= 2 && expression.IndexOf("'" + name[0] + "'[" + name[1] + "]", StringComparison.OrdinalIgnoreCase) > -1)
        //     //                        {
        //     //                            if (name[0] != keyValuePairInner.Key)
        //     //                            {
        //     //                                columnDependency.objectDependency[columnUsed]["isUsedInFieldParameter"] = true;
        //     //                                columnDependency.objectLineage[columnUsed]["dependentFieldParameters"].Add(keyValuePairInner.Key);
        //     //                            }
        //     //                        }
        //     //                    }
        //     //                    catch (Exception ex)
        //     //                    {
        //     //                        GlobalHelper.WriteCrashLog(ex.ToString());
        //     //                    }
        //     //                }
        //     //            }
        //     //        }
        //     //        catch (Exception ex)
        //     //        {
        //     //            GlobalHelper.WriteCrashLog(ex.ToString());
        //     //        }
        //     //    }

        //     return columnDependency;

        // }

    }
}
