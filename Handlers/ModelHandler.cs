using PowerBI_MCP.DTO;
using System.Data;
using System.IO;
using Microsoft.AnalysisServices.Tabular;
using PowerBI_MCP.Utils;
using System.Text.RegularExpressions;

namespace PowerBI_MCP.Handlers
{
    /// <summary>
    /// Handler for extracting and transforming model metadata, relationships, roles, tables, columns, and measures.
    /// </summary>
    public class ModelHandler
    {
        /// <summary>
        /// Holds relationship metadata for the model.
        /// </summary>
        public class RelationshipData
        {
            public RelationshipData() { }

            /// <summary>Dictionary of columns involved in relationships.</summary>
            public Dictionary<string, bool> relationshipColumns { get; set; } = new Dictionary<string, bool>();
            /// <summary>List of relationship data models.</summary>
            public List<RelationshipSummary> relationshipList { get; set; } = new List<RelationshipSummary>();
            /// <summary>Dictionary of tables involved in relationships.</summary>
            public Dictionary<string, bool> tableInRelationship { get; set; } = new Dictionary<string, bool>();
        }

        /// <summary>
        /// Holds security role metadata for the model.
        /// </summary>
        public class SecurityRoleData
        {
            public SecurityRoleData() { }

            /// <summary>List of tables with security applied.</summary>
            public List<string> securityTables { get; set; } = new List<string>();
            /// <summary>List of role data models.</summary>
            public List<RolesSummary> rolesDataList { get; set; } = new List<RolesSummary>();
            /// <summary>List of columns with security applied.</summary>
            public List<string> securityColumns { get; set; } = new List<string>();
        }

        /// <summary>
        /// Holds table, column, and measure metadata for the model.
        /// </summary>
        public class TableColumnMeasureData
        {
            public TableColumnMeasureData() { }

            /// <summary>Counter for local date tables.</summary>
            public int dateTableCounter { get; set; } = 0;
            /// <summary>Flag for incremental refresh configuration.</summary>
            public int refreshFlag { get; set; } = 0;
            /// <summary>List of model storage modes.</summary>
            public List<string> modelType { get; set; } = new List<string>();
            /// <summary>List of table data models.</summary>
            public List<TableSummary> tableDataList { get; set; } = new List<TableSummary>();
            /// <summary>List of measure data models.</summary>
            public List<MeasureSummary> measureDataList { get; set; } = new List<MeasureSummary>();
            /// <summary>List of column data models.</summary>
            public List<ColumnSummary> columnDataList { get; set; } = new List<ColumnSummary>();
        }

        /// <summary>
        /// Extracts relationship metadata from a RelationshipCollection.
        /// </summary>
        /// <param name="relations">Collection of relationships.</param>
        /// <returns>RelationshipDTO with relationship details.</returns>
        public static RelationshipDTO GetRelationshipData(RelationshipCollection relations)
        {
            RelationshipDTO relationshipData = new RelationshipDTO();

            foreach (SingleColumnRelationship relation in relations)
            {
                try
                {
                    if (relation == null)
                        continue;

                    // Get source and target tables for the relationship.
                    Table FromTable = relation.FromTable;
                    Table ToTable = relation.ToTable;
                    string fromMode = FromTable.RefreshPolicy != null ? FromTable.RefreshPolicy.Mode.ToString() : FromTable.Partitions[0].Mode.ToString();
                    string toMode = ToTable.RefreshPolicy != null ? ToTable.RefreshPolicy.Mode.ToString() : ToTable.Partitions[0].Mode.ToString();

                    // Build relationship data model.
                    RelationshipSummary relData = new RelationshipSummary()
                    {
                        RelationshipName = relation.Name,
                        IsActive = relation.IsActive.ToString(),
                        FromColumn = new ColumnSummary(){
                            TableName = relation.FromTable.Name,
                            ColumnName = relation.FromColumn.Name,
                            DataType = relation.FromColumn.DataType.ToString(),
                            Visible = relation.FromColumn.IsHidden ? "No" : "Yes",
                        },
                        ToColumn = new ColumnSummary(){
                            TableName = relation.ToTable.Name,
                            ColumnName = relation.ToColumn.Name,
                            DataType = relation.ToColumn.DataType.ToString(),
                            Visible = relation.ToColumn.IsHidden ? "No" : "Yes",
                        },
                        FromMode = fromMode,
                        ToMode = toMode,
                        FromCardinality = relation.FromCardinality.ToString(),
                        ToCardinality = relation.ToCardinality.ToString(),
                        RelationshipCardinality = relation.FromCardinality + " - to - " + relation.ToCardinality,
                        CrossFilterDirection = relation.CrossFilteringBehavior.ToString().Contains("One") == true ? "Single" : "Both",
                        RelationshipType = (fromMode == "Import" && toMode == "DirectQuery") || (fromMode == "DirectQuery" && toMode == "Import") ? "Weak" : "Strong",
                        IsReferentialIntegrityEnabled = relation.RelyOnReferentialIntegrity == true ? "Yes" : "No",
                        IsSecurityFilterEnabled = relation.SecurityFilteringBehavior.ToString(),
                    };

                    // Track columns and tables involved in relationships.
                    if (relationshipData.relationshipColumns.ContainsKey(relation.FromTable.Name + '.' + relation.FromColumn.Name) == false)
                        relationshipData.relationshipColumns.Add(relation.FromTable.Name + '.' + relation.FromColumn.Name, true);
                    if (relationshipData.relationshipColumns.ContainsKey(relation.ToTable.Name + '.' + relation.ToColumn.Name) == false)
                        relationshipData.relationshipColumns.Add(relation.ToTable.Name + '.' + relation.ToColumn.Name, true);

                    if (relationshipData.tableInRelationship.ContainsKey(relation.ToTable.Name) == false)
                        relationshipData.tableInRelationship.Add(relation.ToTable.Name, true);
                    if (relationshipData.tableInRelationship.ContainsKey(relation.FromTable.Name) == false)
                        relationshipData.tableInRelationship.Add(relation.FromTable.Name, true);

                    relationshipData.relationshipList.Add(relData);
                }
                catch (System.Exception ex)
                {
                    GlobalHandler.WriteCrashLog(ex.ToString());
                }
            }
            return relationshipData;
        }

        /// <summary>
        /// Extracts summary model info from the database and model.
        /// </summary>
        /// <param name="database">Tabular database object.</param>
        /// <param name="databaseModel">Tabular model object.</param>
        /// <param name="tableColumnMeasureData">Table/column/measure metadata.</param>
        /// <returns>List of model data summary objects.</returns>
        public static List<SemanticModelSummary> GetModelInfoDataModel(Database database, Model databaseModel, TableColumnMeasureDTO tableColumnMeasureData)
        {
            List<SemanticModelSummary> modelDataList =
            [
                new SemanticModelSummary() { Field = "Database Name", Value = database?.Name != null ? database?.Name.ToString() : "Unknown" },
                new SemanticModelSummary() { Field = "Compatibility", Value = database?.CompatibilityLevel != null ? database?.CompatibilityLevel.ToString() : "Unknown" },
                new SemanticModelSummary() { Field = "Estimated Size", Value = (database?.EstimatedSize / (double)1048576)?.ToString("0.00") + "MB" },
                new SemanticModelSummary() { Field = "Model Type", Value = tableColumnMeasureData.modelType.Count == 1 ? tableColumnMeasureData.modelType[0] : "Mixed" },
                new SemanticModelSummary()
                {
                    Field = "Tables",
                    Value =
                        "Total: "
                        + tableColumnMeasureData.tableDataList.Count.ToString()
                        + " (Power Query - "
                        + tableColumnMeasureData.tableDataList.Where(x => x.TableType == "M Query").Count().ToString()
                        + ", Calculated -  "
                        + tableColumnMeasureData.tableDataList.Where(x => x.TableType == "Calculated").Count().ToString()
                        + ", Field Parameter -  "
                        + tableColumnMeasureData.tableDataList.Where(x => x.TableType == "Field Parameter").Count().ToString()
                        + ", Calculation Group -  "
                        + tableColumnMeasureData.tableDataList.Where(x => x.TableType == "Calculation Group").Count().ToString()
                        + ")",
                },
                new SemanticModelSummary()
                {
                    Field = "Columns",
                    Value =
                        "Total: "
                        + tableColumnMeasureData.columnDataList.Count.ToString()
                        + " (Power Query - "
                        + (tableColumnMeasureData.columnDataList.Count - tableColumnMeasureData.columnDataList.Where(x => x.ColumnType != null && x.ColumnType == "Calculated").Count()).ToString()
                        + ", Calculated -  "
                        + tableColumnMeasureData.columnDataList.Where(x => x.ColumnType != null && x.ColumnType == "Calculated").Count().ToString()
                        + ")",
                },
                new SemanticModelSummary() { Field = "Measures", Value = tableColumnMeasureData.measureDataList.Count.ToString() },
                new SemanticModelSummary() { Field = "Is Security Enabled", Value = databaseModel.Roles.Count > 0 ? "Yes" : "No" },
                new SemanticModelSummary() { Field = "Is Incremental Refresh Configured", Value = tableColumnMeasureData.refreshFlag == 1 ? "Yes" : "No" },
                new SemanticModelSummary() { Field = "Is Local Datetime Enabled", Value = tableColumnMeasureData.dateTableCounter > 0 ? "Yes" : "No" },
                new SemanticModelSummary() { Field = "Are Perspective Created", Value = databaseModel.Perspectives.Count > 0 ? "Yes" : "No" },
                new SemanticModelSummary() { Field = "Mode", Value = database?.ReadWriteMode != null ? database?.ReadWriteMode.ToString() : "Unknown" },
                new SemanticModelSummary() { Field = "Max Parallelism Per Query", Value = databaseModel.MaxParallelismPerQuery.ToString() },
                new SemanticModelSummary() { Field = "Max Parallelism Per Refresh", Value = databaseModel.MaxParallelismPerRefresh.ToString() },
                new SemanticModelSummary() { Field = "Max Connection", Value = databaseModel.DataSourceDefaultMaxConnections.ToString() },
            ];

            return modelDataList;
        }

        /// <summary>
        /// Extracts security roles and permissions from the model.
        /// </summary>
        /// <param name="roles">Collection of model roles.</param>
        /// <returns>SecurityRoleDTO with security role details.</returns>
        public static SecurityRoleDTO GetRolesData(ModelRoleCollection roles)
        {
            SecurityRoleDTO securityRoleData = new();

            foreach (ModelRole role in roles)
            {
                try
                {
                    foreach (TablePermission tp in role.TablePermissions)
                    {
                        try
                        {
                            string OLSProp = "";
                            if (tp.MetadataPermission == MetadataPermission.None)
                            {
                                OLSProp = "No Access";
                            }
                            else if (tp.MetadataPermission == MetadataPermission.Default)
                            {
                                ColumnPermissionCollection columnPermissions = tp.ColumnPermissions;
                                foreach (ColumnPermission col in columnPermissions)
                                {
                                    try
                                    {
                                        OLSProp = String.Concat(OLSProp, col.Name);
                                        securityRoleData.securityColumns.Add(tp?.Name.Trim() + "." + col.Name.Trim());
                                    }
                                    catch (Exception ex)
                                    {
                                        GlobalHandler.WriteCrashLog(ex.ToString());
                                    }
                                }
                                OLSProp = OLSProp == "" ? "" : "Restricted Access. No access on following columns: " + OLSProp;
                            }
                            if (tp?.Name?.Trim() != null)
                                securityRoleData.securityTables.Add(tp?.Name?.Trim() ?? "");
                            securityRoleData.rolesDataList.Add(
                                new RolesSummary()
                                {
                                    RoleName = role?.Name,
                                    TableName = tp?.Name.Trim(),
                                    RowLevelSecurityFilter = tp?.FilterExpression,
                                    ObjectLevelSecurityFilter = OLSProp,
                                }
                            );
                        }
                        catch (System.Exception ex)
                        {
                            GlobalHandler.WriteCrashLog(ex.ToString());
                        }
                    }
                }
                catch (Exception ex)
                {
                    GlobalHandler.WriteCrashLog(ex.ToString());
                }
            }
            return securityRoleData;
        }

        /// <summary>
        /// Extracts measure metadata from a MeasureCollection.
        /// </summary>
        /// <param name="measures">Collection of measures.</param>
        /// <param name="measureDataList">List to populate with measure data.</param>
        public static void GetMeasuresDataModel(MeasureCollection measures, List<MeasureSummary> measureDataList)
        {
            foreach (Measure meas in measures)
            {
                try
                {
                    measureDataList.Add(
                        new MeasureSummary()
                        {
                            TableName = meas?.Table.Name,
                            MeasureName = meas?.Name,
                            DataType = meas?.DataType.ToString(),
                            Visible = meas?.IsHidden == true ? "No" : "Yes",
                            Expression = meas?.Expression,
                            Folder = meas?.DisplayFolder,
                            Format = meas?.FormatString,
                            Description = meas?.Description,
                            // BaseFields = "",
                            HasError = meas?.ErrorMessage == "" ? "No" : "Yes (" + meas?.ErrorMessage + ")",
                            Origin = "Semantic Model",
                        }
                    );
                }
                catch (Exception ex)
                {
                    GlobalHandler.WriteCrashLog(ex.ToString());
                }
            }
        }

        /// <summary>
        /// Extracts column metadata from a ColumnCollection.
        /// </summary>
        /// <param name="columns">Collection of columns.</param>
        /// <param name="columnDataList">List to populate with column data.</param>
        /// <param name="relationshipColumns">Dictionary of columns involved in relationships.</param>
        /// <param name="securityColumns">List of columns with security applied.</param>
        public static void GetColumnDataModel(ColumnCollection columns, List<ColumnSummary> columnDataList, Dictionary<string, bool> relationshipColumns, List<string> securityColumns)
        {
            foreach (Column col in columns)
            {
                try
                {
                    if (col.Type == ColumnType.Calculated)
                    {
                        // Handle calculated columns.
                        CalculatedColumn calculatedColumn = (CalculatedColumn)col;
                        columnDataList.Add(
                            new ColumnSummary()
                            {
                                TableName = calculatedColumn.Table.Name,
                                ColumnName = calculatedColumn.Name,
                                DataType = calculatedColumn.DataType.ToString(),
                                ColumnType = calculatedColumn.Type.ToString(),
                                Visible = calculatedColumn.IsHidden ? "No" : "Yes",
                                Expression = calculatedColumn.Expression,
                                Folder = calculatedColumn.DisplayFolder.ToString(),
                                Format = calculatedColumn.FormatString.ToString(),
                                DataCategory = calculatedColumn.DataCategory,
                                Description = calculatedColumn.Description,
                                SortByColumn = new()
                                {
                                    TableName =calculatedColumn.SortByColumn?.Table?.Name ?? "" ,
                                    ColumnName= calculatedColumn.SortByColumn?.Name ?? "" ,
                                    Visible = calculatedColumn.SortByColumn?.IsHidden == true ? "No" : "Yes" ,
                                    DataType =calculatedColumn.SortByColumn?.DataType.ToString() ?? "" ,
                                },
                                Summarization = calculatedColumn.SummarizeBy.ToString(),
                                HasError = calculatedColumn.ErrorMessage == "" ? "No" : "Yes (" + calculatedColumn.ErrorMessage.ToString() + ")",
                                IsKeyColumn =
                                    (calculatedColumn.IsKey == true || calculatedColumn.Name.ToLower().Contains("key") == true || calculatedColumn.Name.ToLower().Contains("id") == true)
                                        ? "Yes"
                                        : "No",
                                IsUniqueColumn = calculatedColumn.IsUnique == true ? "Yes" : "No",
                                IsInvolvedInRelationship = relationshipColumns.ContainsKey(calculatedColumn.Table.Name + '.' + calculatedColumn.Name) ? "Yes" : "No",
                                IsSecurityApplied = securityColumns.Contains(calculatedColumn.Table.Name + '.' + calculatedColumn.Name) == true ? "Yes" : "No",
                            }
                        );
                    }
                    else
                    {
                        // Handle regular columns.
                        if (col.Type.ToString() == "RowNumber")
                            continue;

                        columnDataList.Add(
                            new ColumnSummary()
                            {
                                TableName = col.Table.Name,
                                ColumnName = col.Name,
                                DataType = col.DataType.ToString(),
                                ColumnType = col.Type.ToString(),
                                Visible = col.IsHidden ? "No" : "Yes",
                                Expression = "NA",
                                Folder = col.DisplayFolder.ToString(),
                                Format = col.FormatString.ToString(),
                                DataCategory = col.DataCategory,
                                Description = col.Description,
                                SortByColumn = new()
                                {
                                    TableName = col.SortByColumn?.Table?.Name ?? "",
                                    ColumnName = col.SortByColumn?.Name ?? "",
                                    Visible = col.SortByColumn?.IsHidden == true ? "No" : "Yes",
                                    DataType = col.SortByColumn?.DataType.ToString() ?? "",
                                },
                                Summarization = col.SummarizeBy.ToString(),
                                HasError = col.ErrorMessage == "" ? "No" : "Yes (" + col.ErrorMessage.ToString() + ")",
                                IsKeyColumn = (col.IsKey == true || col.Name.ToLower().Contains("key") == true || col.Name.ToLower().Contains("id") == true) ? "Yes" : "No",
                                IsUniqueColumn = col.IsUnique == true ? "Yes" : "No",
                                IsInvolvedInRelationship = relationshipColumns.ContainsKey(col.Table.Name + '.' + col.Name) ? "Yes" : "No",
                                IsSecurityApplied = securityColumns.Contains(col.Table.Name + '.' + col.Name) == true ? "Yes" : "No",
                            }
                        );
                    }
                }
                catch (System.Exception ex)
                {
                    GlobalHandler.WriteCrashLog(ex.ToString());
                }
            }
        }

        /// <summary>
        /// Extracts a substring between two values.
        /// </summary>
        /// <param name="sourceValue">Source string.</param>
        /// <param name="startValue">Start delimiter.</param>
        /// <param name="endValue">End delimiter.</param>
        /// <returns>Substring between start and end values, or empty string if not found.</returns>
        private static string getBetween(string sourceValue, string startValue, string endValue)
        {
            if (sourceValue.Contains(startValue) && sourceValue.Contains(endValue))
            {
                int startIndex,
                    endIndex;
                startIndex = sourceValue.IndexOf(startValue, 0) + startValue.Length;
                endIndex = sourceValue.IndexOf(endValue, startIndex);
                return sourceValue.Substring(startIndex, endIndex - startIndex);
            }
            return "";
        }

        /// <summary>
        /// Extracts parameter metadata from NamedExpressionCollection.
        /// </summary>
        /// <param name="namedExpressions">Collection of named expressions.</param>
        /// <returns>List of parameter data models.</returns>
        public static List<ParameterSummary> GetParameterDataModel(NamedExpressionCollection namedExpressions)
        {
            List<ParameterSummary> parameterDataList = new();

            foreach (NamedExpression exp in namedExpressions)
            {
                try
                {
                    var endIndex = exp.Expression.ToString().IndexOf("meta", 0);
                    var paraValue = endIndex > -1 ? exp.Expression.ToString().Substring(0, endIndex - 0) : exp.Expression.ToString();
                    parameterDataList.Add(
                        new ParameterSummary()
                        {
                            ParameterName = exp.Name,
                            DataType = getBetween(exp.Expression.ToString(), "Type=\"", "\","),
                            IsRequired = exp.Expression.Contains("IsParameterQueryRequired=true") ? "Yes" : "No",
                            Value = paraValue,
                        }
                    );
                }
                catch (Exception ex)
                {
                    GlobalHandler.WriteCrashLog(ex.ToString());
                }
            }
            return parameterDataList;
        }

        public static void GetHierarchyDataModel(HierarchyCollection hierarchies, List<HierarchySummary> hierarchySummaries)
        {
            foreach(var h in hierarchies)
            {
                List<HierarchyLevel> levels = [];
                foreach(var l in h.Levels) levels.Add(new() {Name = l.Name, ColumnName = l.Column.Name });
                
                hierarchySummaries.Add(new() { TableName = h.Table.Name, Hierarchy = h.Name, Levels = levels });
            }
        }
        /// <summary>
        /// Extracts table, column, and measure metadata from the model.
        /// </summary>
        /// <param name="tables">Collection of tables.</param>
        /// <param name="model">Tabular model object.</param>
        /// <param name="tableInRelationship">Dictionary of tables in relationships.</param>
        /// <param name="relationshipColumns">Dictionary of columns in relationships.</param>
        /// <param name="securityColumns">List of columns with security applied.</param>
        /// <param name="securityTables">List of tables with security applied.</param>
        /// <returns>TableColumnMeasureDTO with extracted metadata.</returns>
        public static TableColumnMeasureDTO GetTableColumnMeasureData(
            TableCollection tables,
            Model model,
            Dictionary<string, bool> tableInRelationship,
            Dictionary<string, bool> relationshipColumns,
            List<string> securityColumns,
            List<string> securityTables
        )
        {
            TableColumnMeasureDTO tableColumnMeasureData = new TableColumnMeasureDTO();

            foreach (Table tbl in tables)
            {
                try
                {
                    // Count local date tables.
                    if (tbl.Name.StartsWith("LocalDateTable") && tbl.IsHidden == true)
                    {
                        tableColumnMeasureData.dateTableCounter++;
                    }

                    // Extract measures and columns for the table.
                    GetMeasuresDataModel(tbl.Measures, tableColumnMeasureData.measureDataList);
                    GetColumnDataModel(tbl.Columns, tableColumnMeasureData.columnDataList, relationshipColumns, securityColumns);
                    GetHierarchyDataModel(tbl.Hierarchies, tableColumnMeasureData.Hierarchies);
                    string IncrementalGranularity = "",
                        IncrementalPeriods = "",
                        IncrementalPeriodsOffset = "",
                        RollingWindowGranularity = "",
                        RollingWindowPeriods = "", PollingExpression = "";
                    string mode = tbl.RefreshPolicy != null ? tbl.RefreshPolicy.Mode.ToString() : tbl.Partitions[0].Mode.ToString();

                    if (!tableColumnMeasureData.modelType.Contains(mode))
                    {
                        tableColumnMeasureData.modelType.Add(mode);
                    }

                    // Extract incremental refresh settings if present.
                    if (tbl.RefreshPolicy != null)
                    {
                        BasicRefreshPolicy rp = (BasicRefreshPolicy)tbl.RefreshPolicy;
                        IncrementalGranularity = rp.IncrementalGranularity.ToString();
                        IncrementalPeriods = rp.IncrementalPeriods.ToString();
                        IncrementalPeriodsOffset = rp.IncrementalPeriodsOffset.ToString();
                        RollingWindowGranularity = rp.RollingWindowGranularity.ToString();
                        RollingWindowPeriods = rp.RollingWindowPeriods.ToString();
                        tableColumnMeasureData.refreshFlag = 1;
                        PollingExpression = rp.PollingExpression != null ? rp.PollingExpression : "";
                    }

                    bool isCalculatedTable = false;
                    string calculatedTableExpression = "";
                    List<Dictionary<string, string>> tableAppliedSteps = [];
                    string powerQueryStepsString = "";
                    string powerQueryGroup = "";
                    string powerQueryMode = "";
                    string tableType = "";

                    List<TableSource> tableSources = [];
                    HashSet<string> source = [];
                    HashSet<string> sourceDetails = [];
                    HashSet<string> appliedSteps = [];
                    HashSet<string> tableUsedInQuery = [];
                    foreach (Partition prt in tbl.Partitions)
                    {
                        string regexForSource = @"Source\d*\s*=\s*(\w+)\b";
                        if (prt == null)
                            continue;
                        try
                        {
                            string sourceQuery = "";
                            if (prt.SourceType == PartitionSourceType.M)
                            {
                                // Handle M Query partition.
                                MPartitionSource mPartition = (MPartitionSource)prt.Source;
                                sourceQuery = mPartition.Expression;
                                tableType = "M Query";

                                // Get tables used in the query.
                                string tableNamePattern = @"\b(?:FROM|JOIN|from|join)\s+([^\s(]+)\s*(?=\(|\b)";
                                MatchCollection tableMatches = Regex.Matches(sourceQuery, tableNamePattern, RegexOptions.IgnoreCase);
                                foreach (Match match in tableMatches)
                                {
                                    try
                                    {
                                        string currentTableName = match.Groups[1].Value;
                                        tableUsedInQuery.Add(currentTableName);
                                    }
                                    catch (Exception ex)
                                    {
                                        GlobalHandler.WriteCrashLog(ex.ToString());
                                    }
                                }

                                // Get M query steps.
                                List<object> queryDetails = GlobalHandler.getPartitionSourceQueryDetails(sourceQuery, model, prt);
                                tableAppliedSteps = (List<Dictionary<string, string>>)queryDetails[0];
                                // foreach (var step in tableAppliedSteps)
                                // {
                                //     powerQueryStepsString += i.ToString() + " ) " + step["stepName"] + " - " + step["method"] + "\n";
                                //     i++;
                                // }
                                powerQueryStepsString = sourceQuery;
                            }
                            else if (prt.SourceType == PartitionSourceType.Calculated)
                            {
                                // Handle calculated table partition.
                                if (ModelDependencyHandler.CheckIsFieldParameter(tbl))
                                {
                                    tableType = "Field Parameter";
                                }
                                else
                                {
                                    tableType = "Calculated";
                                }
                                CalculatedPartitionSource calculatedPartition = (CalculatedPartitionSource)prt.Source;
                                powerQueryStepsString = calculatedPartition.Expression;
                                sourceQuery = calculatedPartition.Expression;
                                calculatedTableExpression = calculatedPartition.Expression;
                                isCalculatedTable = true;

                                source.Add("Calculated");
                                tableSources.Add(new TableSource() { Name = "Calculated", Location = "" });
                            }
                            else if (prt.SourceType == PartitionSourceType.Entity)
                            {
                                // Handle entity partition.
                                regexForSource = @"database\d*\s*=\s*(\w+)\b";
                                tableType = "Entity";
                                sourceQuery = ((EntityPartitionSource)prt.Source).ExpressionSource.Expression;
                                powerQueryStepsString = sourceQuery;
                                tableUsedInQuery.Add(tbl.SourceLineageTag);
                            }
                            powerQueryMode = prt.Mode.ToString() ?? "";
                            powerQueryGroup = prt.QueryGroup == null ? "" : prt.QueryGroup.ToString() ?? "";

                            Match matchForSource = Regex.Match(sourceQuery, regexForSource, RegexOptions.IgnoreCase);
                            var acceptableValues = new[] { "teradata", "oracle", "csv", "sharepoint", "odbc", "synapse", "excel", "json" };

                            if (sourceQuery == "")
                            {
                                source.Add("Power BI Dataset");
                                tableSources.Add(new TableSource() { Name = "Power BI Dataset", Location = "" });

                            }
                            else
                            {
                                // Console.WriteLine("Count of matchForSource.Groups.Count : " + tbl.Name + " " + matchForSource.Groups.Count);
                                for (int idx = 1; idx < matchForSource.Groups.Count; idx++)
                                {
                                    try
                                    {
                                        string patternToGetSourceLocation = @"""(.*?)"""; // Get Path of source For eg. Get Link of sharepoint, excel, csv
                                        Regex regexToGetSourceLocation = new Regex(patternToGetSourceLocation);
                                        Match getSourceLocation = regexToGetSourceLocation.Match(sourceQuery);

                                        if (getSourceLocation.Success)
                                        {
                                            if (acceptableValues.Contains(matchForSource.Groups[1].Value.ToLower()))
                                            {
                                                source.Add(matchForSource.Groups[1].Value);
                                                sourceDetails.Add(getSourceLocation.Groups[1].Value);
                                                tableSources.Add(new TableSource() { Name = matchForSource.Groups[1].Value, Location = getSourceLocation.Groups[1].Value });

                                            }
                                            else if (
                                                matchForSource.Groups[1].Value.Equals("sql", StringComparison.CurrentCultureIgnoreCase)
                                                || matchForSource.Groups[1].Value.Equals("datawarehouse", StringComparison.CurrentCultureIgnoreCase)
                                            )
                                            {
                                                // Handle SQL and Datawarehouse sources.
                                                string patternToGetSQLSourceName = @"sql\.databases?\b\(([^,]+),\s*([^,]+)\)";
                                                MatchCollection SQLSourceNameCollection = Regex.Matches(sourceQuery.ToLower(), patternToGetSQLSourceName, RegexOptions.IgnoreCase);

                                                foreach (Match SQLSourceName in SQLSourceNameCollection)
                                                {
                                                    try
                                                    {
                                                        string currentServerName = SQLSourceName.Groups.Count > 1 ? SQLSourceName.Groups[1].Value.ToLower().Trim() : "";
                                                        string currentDBName = SQLSourceName.Groups.Count > 2 ? SQLSourceName.Groups[2].Value.ToLower().Trim() : "";

                                                        if (
                                                            currentServerName.StartsWith('\"') && currentServerName.EndsWith('\"')
                                                            || currentServerName.StartsWith('\'') && currentServerName.EndsWith('\'')
                                                        )
                                                        {
                                                            // If it is, prefix the console output with "Comma : "
                                                            string serverNameToDisplay = "";
                                                            if (currentServerName.Contains("synapse", StringComparison.CurrentCultureIgnoreCase))
                                                            {
                                                                serverNameToDisplay = "Synapse";
                                                            }
                                                            else if (currentServerName.Contains("datawarehouse", StringComparison.CurrentCultureIgnoreCase))
                                                            {
                                                                serverNameToDisplay = "Fabric SQL Endpoint";
                                                            }
                                                            else
                                                            {
                                                                serverNameToDisplay = "SQL";
                                                            }
                                                            source.Add(serverNameToDisplay);
                                                            sourceDetails.Add("Server: " + currentServerName + " Database: " + currentDBName);

                                                            tableSources.Add(new TableSource()
                                                            {
                                                                Name = serverNameToDisplay,
                                                                Location = "Server: " + currentServerName + " Database: " + currentDBName
                                                            });

                                                        }
                                                        else
                                                        {
                                                            // Resolve parameterized server/database names.
                                                            List<ParameterSummary> parameterList = GetParameterDataModel(model.Expressions);

                                                            foreach (ParameterSummary paramModel in parameterList)
                                                            {
                                                                try
                                                                {
                                                                    if (paramModel.ParameterName?.ToLower() == currentServerName?.ToLower())
                                                                    {
                                                                        currentServerName = paramModel.Value ?? "";
                                                                    }

                                                                    if (paramModel.ParameterName?.ToLower() == currentDBName?.ToLower())
                                                                    {
                                                                        currentDBName = paramModel.Value ?? "";
                                                                    }
                                                                }
                                                                catch (Exception ex)
                                                                {
                                                                    GlobalHandler.WriteCrashLog(ex.ToString());
                                                                }
                                                            }

                                                            string serverNameToDisplay = "";
                                                            if (currentServerName != null && currentServerName.ToLower().Contains("synapse"))
                                                            {
                                                                serverNameToDisplay = "Synapse";
                                                            }
                                                            else
                                                            {
                                                                serverNameToDisplay = "SQL";
                                                            }
                                                            source.Add(serverNameToDisplay);
                                                            sourceDetails.Add("Server: " + currentServerName + " Database: " + currentDBName);

                                                            tableSources.Add(new TableSource() { Name = serverNameToDisplay, Location = "Server: " + currentServerName + " Database: " + currentDBName });

                                                        }
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        GlobalHandler.WriteCrashLog(ex.ToString());
                                                    }
                                                }
                                            }
                                            else if (sourceQuery.ToLower().Contains("binary.fromtext"))
                                            {
                                                source.Add("Binary");
                                                sourceDetails.Add(matchForSource.Groups[1].Value);
                                                tableSources.Add(new TableSource() { Name = "Binary", Location = matchForSource.Groups[1].Value });

                                            }
                                            else
                                            {
                                                source.Add("Others");
                                                tableSources.Add(new TableSource() { Name = "Others", Location = "" });

                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        GlobalHandler.WriteCrashLog(ex.ToString());
                                    }
                                }
                                if (source.Count == 0)
                                {
                                    source.Add("Others");
                                    tableSources.Add(new TableSource() { Name = "Others", Location = "" });

                                }
                            }

                            // Extract navigation table names if present.
                            string regexNavigationTableName = @"Schema\s*=\s*""([^""]+)""\s*,\s*Item\s*=\s*""([^""]+)""";
                            Match patternMatch = Regex.Match(sourceQuery, regexNavigationTableName);
                            if (patternMatch.Success)
                            {
                                // Extract the schema and item names
                                string schema = patternMatch.Groups[1].Value;
                                string item = patternMatch.Groups[2].Value;

                                // Concatenate schema and item with a period in between
                                string result = $"{schema}.{item}";

                                // Output the result
                                tableUsedInQuery.Add(result);
                            }

                            // Get tables name used in queries
                            string regexExpressionToGetAppliedResult = @"#(""[\w\s]+""\s*)=\s*Table\.(\w+)\(";
                            MatchCollection appliedStepsMatch = Regex.Matches(sourceQuery, regexExpressionToGetAppliedResult, RegexOptions.IgnoreCase);

                            foreach (Match match in appliedStepsMatch)
                            {
                                try
                                {
                                    string stepName = match.Groups[1].Value.Trim('\"');
                                    string methodName = match.Groups[2].Value;

                                    // Console.WriteLine($"Step name: {stepName}, Method name: {methodName}");
                                    appliedSteps.Add(match.Groups[2].Value);
                                }
                                catch (Exception ex)
                                {
                                    GlobalHandler.WriteCrashLog(ex.ToString());
                                }
                            }
                        }
                        catch (System.Exception ex)
                        {
                            GlobalHandler.WriteCrashLog(ex.ToString());
                        }
                    }

                    List<FormattedExpressionAndDependency> calcGroupDetails = new();
                    if (tbl.CalculationGroup != null)
                    {
                        tableType = "Calculation Group";
                        foreach (var calcItem in tbl.CalculationGroup.CalculationItems)
                        {
                            calcGroupDetails.Add(new() { FormatedExpression = calcItem.Expression.Trim() });
                        }
                    }

                    // Add table metadata to the result list.
                    tableColumnMeasureData.tableDataList.Add(
                        new TableSummary()
                        {
                            TableName = tbl.Name,
                            StorageMode = powerQueryMode,
                            TableType = tableType,
                            Visible = tbl.IsHidden == true ? "No" : "Yes",
                            SourceType = mode,
                            SourceDetails = mode,
                            SourceTables = mode,
                            PowerQueryGroup = powerQueryGroup,
                            PowerQuerySteps = powerQueryStepsString,
                            PowerQueryTransformation = tableAppliedSteps.Count,
                            IncrementalRefreshSetUp =
                                tbl.RefreshPolicy != null
                                    ? "Yes"
                                        + "\n(Incremental Refresh Settings: Refresh Granularity: "
                                        + IncrementalGranularity
                                        + ", Refresh Period: "
                                        + IncrementalPeriods
                                        + ", Refresh Offset: "
                                        + IncrementalPeriodsOffset
                                        + "\nArchive Settings: Rolling Window Granularity: "
                                        + RollingWindowGranularity
                                        + ", Rolling Window Period: "
                                        + RollingWindowPeriods
                                        + ")"
                                    : "No",
                            IsIncludedInRefresh = tbl.ExcludeFromModelRefresh ? "No" : "Yes",
                            IsStandaloneTable = tableInRelationship.ContainsKey(tbl.Name) ? "No" : "Yes",
                            IsSecurityApplied = securityTables.Contains(tbl.Name) ? "Yes" : "No",
                            DataCategory = tbl.DataCategory,
                            Description = tbl.Description,
                            Partitions =
                                tbl.Partitions.Count
                                + " (Import - "
                                + tbl.Partitions.ToList().Where((x) => x.Mode == ModeType.Import).ToList().Count
                                + ", DQ  - "
                                + tbl.Partitions.ToList().Where((x) => x.Mode == ModeType.DirectQuery).ToList().Count
                                + ", Dual - "
                                + tbl.Partitions.ToList().Where((x) => x.Mode == ModeType.Dual).ToList().Count
                                + ", DirectLake - "
                                + tbl.Partitions.ToList().Where((x) => x.Mode == ModeType.DirectLake).ToList().Count
                                + ")",
                            Columns = tbl.Columns.Count,
                            CalculatedColumns = tbl.Columns.ToList().Where((x) => x.Type.ToString() == "Calculated").ToList().Count,
                            Hierarchies = tbl.Hierarchies.Count,
                            Measures = tbl.Measures.Count,
                            CalculationGroup = tbl.CalculationGroup != null ? "Yes" : "No",
                            IsCalculatedTable = isCalculatedTable,
                            CalculatedTableExpression = calculatedTableExpression,
                            TableAppliedSteps = tableAppliedSteps,
                            TableAppliedStepsCount = tableAppliedSteps.Count,
                            Source = source,
                            TableUsedInQuery = tableUsedInQuery,
                            AppliedSteps = appliedSteps,
                            SourcePath = sourceDetails,
                            RefreshPolicyExpression = PollingExpression,
                            CalculationGroups = calcGroupDetails,
                            Sources = tableSources,

                        }
                    );
                }
                catch (System.Exception ex)
                {
                    GlobalHandler.WriteCrashLog(ex.ToString());
                }
            }
            return tableColumnMeasureData;
        }

        public static void CreateTableDataSet(List<TableSummary> tables, DataSet ds)
        {
            DataTable tableData = ds.Tables.Add("D_Tables");
            tableData.Columns.AddRange(
                [
                    new System.Data.DataColumn("Table Name", typeof(string)),
                    new System.Data.DataColumn("Table Type", typeof(string)),
                    new System.Data.DataColumn("Visible", typeof(string)),
                    new System.Data.DataColumn("Storage Mode", typeof(string)),
                    new System.Data.DataColumn("Power Query Group", typeof(string)),
                    new System.Data.DataColumn("Power Query Transformation", typeof(string)),
                    new System.Data.DataColumn("Power Query Steps", typeof(string)),
                    new System.Data.DataColumn("incremental Refresh Set Up", typeof(string)),
                    new System.Data.DataColumn("Is Included In Refresh", typeof(string)),
                    new System.Data.DataColumn("Is Standalone Table", typeof(string)),
                    new System.Data.DataColumn("Is Security Applied", typeof(string)),
                    new System.Data.DataColumn("Data Category", typeof(string)),
                    new System.Data.DataColumn("Description", typeof(string)),
                    new System.Data.DataColumn("Columns", typeof(string)),
                    new System.Data.DataColumn("Partitions", typeof(string)),
                    new System.Data.DataColumn("Hierarchies", typeof(string)),
                    new System.Data.DataColumn("Measures", typeof(string)),
                ]
            );
            foreach (TableSummary tbl in tables)
            {
                try
                {
                    tableData.Rows.Add(
                        tbl.TableName,
                        tbl.TableType,
                        tbl.Visible,
                        tbl.StorageMode,
                        tbl.PowerQueryGroup,
                        GlobalHandler.ExcelCellValueFormatting(tbl.PowerQueryTransformation.ToString() ?? ""),
                        GlobalHandler.ExcelCellValueFormatting(tbl.PowerQuerySteps ?? ""),
                        GlobalHandler.ExcelCellValueFormatting(tbl.IncrementalRefreshSetUp ?? ""),
                        GlobalHandler.ExcelCellValueFormatting(tbl.IsIncludedInRefresh ?? ""),
                        GlobalHandler.ExcelCellValueFormatting(tbl.IsStandaloneTable ?? ""),
                        GlobalHandler.ExcelCellValueFormatting(tbl.IsSecurityApplied ?? ""),
                        GlobalHandler.ExcelCellValueFormatting(tbl.DataCategory ?? ""),
                        GlobalHandler.ExcelCellValueFormatting(tbl.Description ?? ""),
                        GlobalHandler.ExcelCellValueFormatting(tbl.Columns + " (Power Query - " + (tbl.Columns - tbl.CalculatedColumns) + ", Calculated - " + tbl.CalculatedColumns + ")"),
                        tbl.Partitions,
                        tbl.Hierarchies.ToString(),
                        tbl.Measures.ToString()
                    );
                }
                catch (Exception ex)
                {
                    GlobalHandler.WriteCrashLog(ex.ToString());
                }
            }
        }

        public static void CreateParameterDataSet(List<ParameterSummary> parameters, DataSet ds)
        {
            DataTable paraData = ds.Tables.Add("D_Parameters");
            paraData.Columns.AddRange(
                [
                    new System.Data.DataColumn("Name", typeof(string)),
                    new System.Data.DataColumn("Data Type", typeof(string)),
                    new System.Data.DataColumn("Is Required", typeof(string)),
                    new System.Data.DataColumn("Value", typeof(string)),
                ]
            );

            foreach (ParameterSummary parameter in parameters)
            {
                try
                {
                    paraData.Rows.Add(
                        GlobalHandler.ExcelCellValueFormatting(parameter.ParameterName ?? ""),
                        GlobalHandler.ExcelCellValueFormatting(parameter.DataType ?? ""),
                        GlobalHandler.ExcelCellValueFormatting(parameter.IsRequired ?? ""),
                        GlobalHandler.ExcelCellValueFormatting(parameter.Value ?? "")
                    );
                }
                catch (Exception ex)
                {
                    GlobalHandler.WriteCrashLog(ex.ToString());
                }
            }
        }

        public static void CreateModelSummaryDataSet(List<SemanticModelSummary> modelSummaries, DataSet ds)
        {
            DataTable modelData = ds.Tables.Add("D_Model Summary");
            modelData.Columns.AddRange([new System.Data.DataColumn("Field", typeof(string)), new System.Data.DataColumn("Value", typeof(string))]);

            foreach (SemanticModelSummary modelSummary in modelSummaries)
            {
                try
                {
                    modelData.Rows.Add(modelSummary.Field, modelSummary.Value);
                }
                catch (Exception ex)
                {
                    GlobalHandler.WriteCrashLog(ex.ToString());
                }
            }
        }

        public static void CreateMeasuresDataSet(List<MeasureSummary> measures, DataSet ds)
        {
            DataTable measureData = ds.Tables.Add("D_Measures");
            measureData.Columns.AddRange(
                [
                    new System.Data.DataColumn("Report Name", typeof(string)),
                    new System.Data.DataColumn("Table Name", typeof(string)),
                    new System.Data.DataColumn("Measure Name", typeof(string)),
                    new System.Data.DataColumn("Data Type", typeof(string)),
                    new System.Data.DataColumn("Visible", typeof(string)),
                    new System.Data.DataColumn("Expression", typeof(string)),
                    new System.Data.DataColumn("Folder", typeof(string)),
                    new System.Data.DataColumn("Format String", typeof(string)),
                    new System.Data.DataColumn("Description", typeof(string)),
                    // new  System.Data.DataColumn("Base Fields",typeof(string)),
                    new System.Data.DataColumn("Has Error", typeof(string)),
                    new System.Data.DataColumn("Origin", typeof(string)),
                ]
            );
            foreach (var meas in measures)
            {
                try
                {
                    measureData.Rows.Add(
                        GlobalHandler.ExcelCellValueFormatting(meas.ReportName ?? ""),
                        GlobalHandler.ExcelCellValueFormatting(meas.TableName ?? ""),
                        GlobalHandler.ExcelCellValueFormatting(meas.MeasureName ?? ""),
                        GlobalHandler.ExcelCellValueFormatting(meas.DataType ?? ""),
                        GlobalHandler.ExcelCellValueFormatting(meas.Visible ?? ""),
                        GlobalHandler.ExcelCellValueFormatting(meas.Expression ?? ""),
                        GlobalHandler.ExcelCellValueFormatting(meas.Folder ?? ""),
                        GlobalHandler.ExcelCellValueFormatting(meas.Format ?? ""),
                        GlobalHandler.ExcelCellValueFormatting(meas.Description ?? ""),
                        // GlobalHandler.ExcelCellValueFormatting(meas.BaseFields ?? ""),
                        GlobalHandler.ExcelCellValueFormatting(meas.HasError ?? ""),
                        GlobalHandler.ExcelCellValueFormatting(meas.Origin ?? "")
                    );
                }
                catch (Exception ex)
                {
                    GlobalHandler.WriteCrashLog(ex.ToString());
                }
            }
        }

        public static void CreateColumnDataSet(List<ColumnSummary> columns, DataSet ds)
        {
            DataTable columnData = ds.Tables.Add("D_Columns");
            columnData.Columns.AddRange(
                new System.Data.DataColumn[17]
                {
                    new System.Data.DataColumn("Table Name", typeof(string)),
                    new System.Data.DataColumn("Column Name", typeof(string)),
                    new System.Data.DataColumn("Data Type", typeof(string)),
                    new System.Data.DataColumn("Column Type", typeof(string)),
                    new System.Data.DataColumn("Visible", typeof(string)),
                    new System.Data.DataColumn("Expression", typeof(string)),
                    new System.Data.DataColumn("Folder", typeof(string)),
                    new System.Data.DataColumn("Format String", typeof(string)),
                    new System.Data.DataColumn("Data Category", typeof(string)),
                    new System.Data.DataColumn("Description", typeof(string)),
                    new System.Data.DataColumn("Sort By", typeof(string)),
                    new System.Data.DataColumn("Summarize By", typeof(string)),
                    new System.Data.DataColumn("Has Error", typeof(string)),
                    new System.Data.DataColumn("Is Key Column", typeof(string)),
                    new System.Data.DataColumn("Is Unique Column", typeof(string)),
                    new System.Data.DataColumn("Is Involved in Relationship", typeof(string)),
                    new System.Data.DataColumn("Is Security Applied", typeof(string)),
                }
            );

            foreach (var column in columns)
            {
                try
                {
                    columnData.Rows.Add(
                        GlobalHandler.ExcelCellValueFormatting(column.TableName ?? ""),
                        GlobalHandler.ExcelCellValueFormatting(column.ColumnName ?? ""),
                        GlobalHandler.ExcelCellValueFormatting(column.DataType ?? ""),
                        GlobalHandler.ExcelCellValueFormatting(column.ColumnType ?? ""),
                        GlobalHandler.ExcelCellValueFormatting(column.Visible ?? ""),
                        GlobalHandler.ExcelCellValueFormatting(column.Expression ?? ""),
                        GlobalHandler.ExcelCellValueFormatting(column.Folder ?? ""),
                        GlobalHandler.ExcelCellValueFormatting(column.Format ?? ""),
                        GlobalHandler.ExcelCellValueFormatting(column.DataCategory ?? ""),
                        GlobalHandler.ExcelCellValueFormatting(column.Description ?? ""),
                        GlobalHandler.ExcelCellValueFormatting(column.SortByColumn != null ? column.SortByColumn.ColumnName ?? "" : ""),
                        GlobalHandler.ExcelCellValueFormatting(column.Summarization ?? ""),
                        GlobalHandler.ExcelCellValueFormatting(column.HasError ?? ""),
                        GlobalHandler.ExcelCellValueFormatting(column.IsKeyColumn ?? ""),
                        GlobalHandler.ExcelCellValueFormatting(column.IsUniqueColumn ?? ""),
                        GlobalHandler.ExcelCellValueFormatting(column.IsInvolvedInRelationship ?? ""),
                        GlobalHandler.ExcelCellValueFormatting(column.IsSecurityApplied ?? "")
                    );
                }
                catch (Exception ex)
                {
                    GlobalHandler.WriteCrashLog(ex.ToString());
                }
            }
        }

        public static void CreateRelationshipDataSet(List<RelationshipSummary> relationshipList, DataSet ds)
        {
            DataTable relationData = ds.Tables.Add("D_Relationships");
            relationData.Columns.AddRange(
                [
                    new System.Data.DataColumn("Relationship Name", typeof(string)),
                    new System.Data.DataColumn("IsActive", typeof(string)),
                    new System.Data.DataColumn("From Field", typeof(string)),
                    new System.Data.DataColumn("To Field", typeof(string)),
                    new System.Data.DataColumn("Relationship Cardinality", typeof(string)),
                    new System.Data.DataColumn("Cross Filtering Behavior", typeof(string)),
                    new System.Data.DataColumn("Relationship Type", typeof(string)),
                    new System.Data.DataColumn("Is Referential Integrity Enabled", typeof(string)),
                    new System.Data.DataColumn("Is Security Filter Enabled", typeof(string)),
                ]
            );

            foreach (var relData in relationshipList)
            {
                try
                {
                    relationData.Rows.Add(
                        GlobalHandler.ExcelCellValueFormatting(relData.RelationshipName ?? ""),
                        GlobalHandler.ExcelCellValueFormatting(relData.IsActive ?? ""),
                        GlobalHandler.ExcelCellValueFormatting(relData.FromColumn.ColumnName ?? ""),
                        GlobalHandler.ExcelCellValueFormatting(relData.ToColumn.ColumnName ?? ""),
                        GlobalHandler.ExcelCellValueFormatting(relData.RelationshipCardinality ?? ""),
                        GlobalHandler.ExcelCellValueFormatting(relData.CrossFilterDirection ?? ""),
                        GlobalHandler.ExcelCellValueFormatting(relData.RelationshipType ?? ""),
                        GlobalHandler.ExcelCellValueFormatting(relData.IsReferentialIntegrityEnabled ?? ""),
                        GlobalHandler.ExcelCellValueFormatting(relData.IsSecurityFilterEnabled ?? "")
                    );
                }
                catch (Exception ex)
                {
                    GlobalHandler.WriteCrashLog(ex.ToString());
                }
            }
        }
        
        public static void CreateRoleDataSet(List<RolesSummary> roles, DataSet ds)
        {
            DataTable rolesData = ds.Tables.Add("D_Security Roles");
            rolesData.Columns.AddRange(
                [
                    new System.Data.DataColumn("Role Name", typeof(string)),
                    new System.Data.DataColumn("Table Name", typeof(string)),
                    new System.Data.DataColumn("Row Level Security Filter", typeof(string)),
                    new System.Data.DataColumn("Object Level Security Filter", typeof(string)),
                ]
            );

            foreach (var role in roles)
            {
                try
                {
                    rolesData.Rows.Add(
                        GlobalHandler.ExcelCellValueFormatting(role.RoleName ?? ""),
                        GlobalHandler.ExcelCellValueFormatting(role.TableName ?? ""),
                        GlobalHandler.ExcelCellValueFormatting(role.RowLevelSecurityFilter ?? ""),
                        GlobalHandler.ExcelCellValueFormatting(role.ObjectLevelSecurityFilter ?? "")
                    );
                }
                catch (Exception ex)
                {
                    GlobalHandler.WriteCrashLog(ex.ToString());
                }
            }
        }
    };
}