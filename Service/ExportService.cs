using System.Data;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO.Compression;
using PowerBI_MCP.DTO;
using PowerBI_MCP.Handlers;
using PowerBI_MCP.Service;
using PowerBI_MCP.Interfaces;
using PowerBI_MCP.Utils.Cache;
// using PowerBI_MCP.Services.ImpactAnalysis;
using PowerBI_MCP.Entities;
using PowerBI_MCP.Models;

namespace PowerBI_MCP.Service
{    
    public class ExportService : IExportService
    {
        private readonly IssueService _issueService = new();
        // private readonly ImpactAnalysisService _impactAnalysisService = new();

        public async Task ExportToExcel(ReportModel report, DatasetModel model, List<string> exportAreas)
        {
            string basePath = Path.Combine("C:\\Users\\Ravi Mishra\\OneDrive\\Documents\\powerbi-mcp", $"{SanitizeName(model.DatasetName)}_Excel");
            string zipPath = $"{basePath}.zip";
            Directory.CreateDirectory(basePath);

            try
            {
                // foreach (var report in reports)
                // {
                    string reportFolder = Path.Combine(basePath, $"{SanitizeName(report.ReportName.Replace(".", "_"))}_Report");
                    Directory.CreateDirectory(reportFolder);

                    foreach (string area in exportAreas)
                    {
                        List<DataSet> datasets = new();
                        switch (area.ToLower())
                        {
                            case "documentation":
                                var docTab = new DataSet();
                                var docTable = docTab.Tables.Add("Documentation Summary");
                                docTable.Columns.AddRange(new DataColumn[]
                                {
                                    new DataColumn("Field", typeof(string)),
                                    new DataColumn("Count", typeof(int)),
                                    new DataColumn("Details Table", typeof(string))
                                });
                                var docDataSet = new DataSet();
                                string reportCacheId = report.ReportId;
                                GenerateReportDocumentationDataset(reportCacheId, docTable, docDataSet);
                                datasets.Add(docTab);
                                datasets.Add(docDataSet);
                                SaveExcel(reportFolder, "Documentation", datasets, exportAreas);
                                break;

                            case "alignment":
                                var alignTab = new DataSet();
                                var alignTable = alignTab.Tables.Add("Alignment Summary");
                                alignTable.Columns.AddRange(new DataColumn[]
                                {
                                    new DataColumn("Field", typeof(string)),
                                    new DataColumn("Issue Count", typeof(int)),
                                    new DataColumn("Total Count", typeof(int)),
                                    new DataColumn("Details Table", typeof(string))
                                });
                                var alignDataSet = new DataSet();
                                var issues = (List<IssueRulesMetadataDTO>)_issueService.GetAlignmentIssues(report.ReportName, null, "");
                                GenerateAlignmentDataset(issues, alignDataSet, alignTable);
                                datasets.Add(alignTab);
                                datasets.Add(alignDataSet);
                                SaveExcel(reportFolder, "Alignment", datasets, exportAreas);
                                break;

                            case "insights":
                                var insightTab = new DataSet();
                                var insightTable = insightTab.Tables.Add("Insights Summary");
                                insightTable.Columns.AddRange(new DataColumn[]
                                {
                                    new DataColumn("Parent", typeof(string)),
                                    new DataColumn("Rule", typeof(string)),
                                    new DataColumn("Description", typeof(string)),
                                    new DataColumn("Issue Count", typeof(int)),
                                    new DataColumn("Total Count", typeof(int)),
                                    new DataColumn("Details Table", typeof(string))
                                });
                                var insightData = new DataSet();
                                var reportInsights = _issueService.GetData(report.ReportName, "Report", "");
                                // GenerateInsightsDataset(reportInsights.IssueRules, insightData, insightTable, reportInsights.IssueRuleColumns);
                                datasets.Add(insightTab);
                                datasets.Add(insightData);
                                SaveExcel(reportFolder, "Insights", datasets, exportAreas);
                                break;

                            // case "lineage":
                            //     var lineageTab = new DataSet();
                            //     var lineageTable = lineageTab.Tables.Add("Lineage Fields");
                            //     lineageTable.Columns.AddRange(new DataColumn[]
                            //     {
                            //         new DataColumn("Object Name", typeof(string)),
                            //         new DataColumn("Object Type", typeof(string)),
                            //         new DataColumn("Parent Names", typeof(string))
                            //     });
                            //     var lineageData = _impactAnalysisService.GetReportImpactAnalysis(new ReportUIDetailsDTO
                            //     {
                            //         WorkspaceId = report.WorkspaceId,
                            //         ReportId = report.ReportId,
                            //         SemanticModelWorkspaceId = report.SemanticModelWorkspaceId,
                            //         SemanticModelId = report.SemanticModelId
                            //     }, userEmail);
                            //     GenerateLineageDataset(lineageData.Items, lineageTable);
                            //     datasets.Add(lineageTab);
                            //     SaveExcel(reportFolder, "Lineage", datasets, exportAreas);
                            //     break;
                        }
                    }
                // }

                // foreach (var model in models)
                // {
                    string modelFolder = Path.Combine(basePath, $"{SanitizeName(model.DatasetName)}_Model");
                    Directory.CreateDirectory(modelFolder);

                    foreach (string area in exportAreas)
                    {
                        List<DataSet> datasets = new();

                        switch (area.ToLower())
                        {
                            case "documentation":
                                var docTab = new DataSet();
                                var docTable = docTab.Tables.Add("Documentation Summary");
                                docTable.Columns.AddRange(new DataColumn[]
                                {
                                    new DataColumn("Field", typeof(string)),
                                    new DataColumn("Count", typeof(int)),
                                    new DataColumn("Details Table", typeof(string))
                                });
                                var docDataSet = new DataSet();
                                string modelCacheId = model.DatasetId;
                                GenerateModelDocumentationDataset(modelCacheId, docTable, docDataSet);
                                datasets.Add(docTab);
                                datasets.Add(docDataSet);
                                SaveExcel(modelFolder, "Documentation", datasets, exportAreas);
                                break;

                            case "insights":
                                var insightTab = new DataSet();
                                var insightTable = insightTab.Tables.Add("Insights Summary");
                                insightTable.Columns.AddRange(new DataColumn[]
                                {
                                    new DataColumn("Parent", typeof(string)),
                                    new DataColumn("Rule", typeof(string)),
                                    new DataColumn("Description", typeof(string)),
                                    new DataColumn("Issue Count", typeof(int)),
                                    new DataColumn("Total Count", typeof(int)),
                                    new DataColumn("Details Table", typeof(string))
                                });
                                var insightData = new DataSet();
                                var modelInsights = _issueService.GetData(model.DatasetName, "Semantic Model", "");
                                // GenerateInsightsDataset(modelInsights.IssueRules, insightData, insightTable, modelInsights.IssueRuleColumns);
                                datasets.Add(insightTab);
                                datasets.Add(insightData);
                                SaveExcel(modelFolder, "Insights", datasets, exportAreas);
                                break;

                            case "unused fields":
                            {
                                var unusedTab = new DataSet();
                                var unusedTable = unusedTab.Tables.Add("Unused Fields");
                                unusedTable.Columns.AddRange(new DataColumn[]
                                {
                                    new DataColumn("Table Name", typeof(string)),
                                    new DataColumn("Object Type", typeof(string)),
                                    new DataColumn("Object Name", typeof(string))
                                });

                                // Timeout token (max 20 seconds)
                                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(20));

                                var unusedTask = Task.Run(async () =>
                                {
                                    var unusedObj = await _issueService.GetUnusedFieldData(
                                        model.DatasetName, 
                                        report.ReportName
                                    );

                                    return (unusedObj as dynamic).result;
                                }, cts.Token);

                                Dictionary<string, object> unusedData;

                                try
                                {
                                    unusedData = await unusedTask;
                                }
                                catch (OperationCanceledException)
                                {
                                    // Instead of freezing forever, write partial results
                                    unusedTable.Rows.Add("TIMEOUT", "TIMEOUT", "TIMEOUT retrieving unused fields");
                                    datasets.Add(unusedTab);
                                    SaveExcel(modelFolder, "Unused Fields", datasets, exportAreas);
                                    break;
                                }

                                // STREAM rows instead of buffering ALL in-memory
                                foreach (var entry in (unusedData as dynamic).result)
                                {
                                    foreach (var field in entry.Value)
                                    {
                                        unusedTable.Rows.Add(
                                            entry.Key,
                                            field.ObjectType.ToString(),
                                            field.ObjectName.ToString()
                                        );
                                    }
                                }

                                datasets.Add(unusedTab);
                                SaveExcel(modelFolder, "Unused Fields", datasets, exportAreas);
                                break;
                            }
                            // case "lineage":
                            //     var lineageTab = new DataSet();
                            //     var lineageTable = lineageTab.Tables.Add("Lineage Fields");
                            //     lineageTable.Columns.AddRange(new DataColumn[]
                            //     {
                            //         new DataColumn("Object Name", typeof(string)),
                            //         new DataColumn("Object Type", typeof(string)),
                            //         new DataColumn("Parent Names", typeof(string))
                            //     });
                            //     var lineageData = await _impactAnalysisService.GetModelImpactAnalysis(executionId, userEmail);
                            //     if (lineageData.result.ContainsKey(model.SemanticModelId))
                            //     {
                            //         GenerateLineageDataset(lineageData.result[model.SemanticModelId].Items, lineageTable);
                            //         datasets.Add(lineageTab);
                            //         SaveExcel(modelFolder, "Lineage", datasets, exportAreas);
                            //     }
                            //     else
                            //     {
                            //         Console.WriteLine($"No lineage data found for SemanticModelId: {model.SemanticModelId}");
                            //     }
                            //     break;
                        }
                    }
                // }

                if (File.Exists(zipPath)) File.Delete(zipPath);
                ZipFile.CreateFromDirectory(basePath, zipPath);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
                GlobalHandler.WriteCrashLog(ex.ToString());
            }
            finally
            {
                if (Directory.Exists(basePath))
                    Directory.Delete(basePath, true);
            }
        }

        public static void GenerateInsightsDataset(List<IssueRulesMetadataDTO> issueRules, DataSet insightsDataSet, DataTable insightsDatasetTable, List<IssueRuleDataColumns> issueRuleColumns)
        {
            foreach (var issueRule in issueRules)
            {
                if (issueRule.IssueCount > 0)
                {
                    string tableName = issueRule.Name.Replace("/", "").Replace("'", "");
                    string detailsTableName = $"I_{tableName.Replace(" ", "_")}";
                    if (insightsDataSet.Tables.Contains(detailsTableName))
                    {
                        int duplicateIndex = 1;
                        string newTableName = $"{detailsTableName}_{duplicateIndex}";
                        while (insightsDataSet.Tables.Contains(newTableName))
                        {
                            duplicateIndex++;
                            newTableName = $"{detailsTableName}_{duplicateIndex}";
                        }
                        detailsTableName = newTableName;
                    }
                    DataTable insightsDataTable = insightsDataSet.Tables.Add(detailsTableName);
                    int index = 0;
                    foreach (var data in issueRule.Data)
                    {
                        try
                        {
                            if (data == null)
                                continue;

                            if (data is IDictionary<string, object> dictionary) // if data is dictionary
                            {
                                if (index == 0)
                                    foreach (var val in dictionary) // create columns from dictionary keys
                                    {
                                        try
                                        {
                                            insightsDataTable.Columns.Add(val.Key, typeof(string));
                                        }
                                        catch (Exception ex)
                                        {
                                            GlobalHandler.WriteCrashLog(ex.ToString());
                                        }
                                    }

                                List<string> tableRow = [];
                                foreach (var val in dictionary) // add data in rows from dictionary values
                                {
                                    try
                                    {
                                        string value = "";
                                        if (val.Value is string)
                                            value = val.Value.ToString() ?? "";
                                        else if (val.Value is bool)
                                            value = val.Value.ToString() == true.ToString() ? "Yes" : "No";
                                        else if (val.Value is DateTime)
                                            value = ((DateTime)val.Value).ToString("MM/dd/yyyy hh:mm tt");
                                        else if (val.Value is double || val.Value is float || val.Value is decimal)
                                            value = ((double)val.Value).ToString("0.00");
                                        else if (val.Value is int || val.Value is long || val.Value is short)
                                            value = ((int)val.Value).ToString();

                                        try
                                        {
                                            if (val.Value is JArray jArray)
                                            {
                                                value = JsonConvert.SerializeObject(jArray, Formatting.Indented);
                                            }
                                            else if (val.Value is JObject jObject)
                                            {
                                                value = JsonConvert.SerializeObject(jObject, Formatting.Indented);
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            value = val.Value.ToString() ?? "";
                                            Console.WriteLine(ex.ToString());
                                        }

                                        tableRow.Add(value);
                                    }
                                    catch (Exception ex)
                                    {
                                        GlobalHandler.WriteCrashLog(ex.ToString());
                                    }
                                }

                                // Check if tableRow has enough elements
                                if (tableRow.Count >= insightsDataTable.Columns.Count)
                                    insightsDataTable.Rows.Add([.. tableRow[..insightsDataTable.Columns.Count]]);
                                else
                                {
                                    // Handle the case where there are not enough elements, e.g., log an error or add an empty row
                                    var paddedRow = new List<string>(tableRow);
                                    while (paddedRow.Count < insightsDataTable.Columns.Count)
                                    {
                                        paddedRow.Add(""); // Add empty strings to pad the row
                                    }
                                    insightsDataTable.Rows.Add([.. paddedRow]);
                                }
                            }
                            else if (data is IDictionary<string, string> stringDictionary)
                            {
                                if (index == 0)
                                    foreach (var val in stringDictionary)
                                    {
                                        try
                                        {
                                            insightsDataTable.Columns.Add(val.Key, typeof(string));
                                        }
                                        catch (Exception ex)
                                        {
                                            GlobalHandler.WriteCrashLog(ex.ToString());
                                        }
                                    }

                                List<string> tableRow = [];
                                foreach (var val in stringDictionary)
                                {
                                    try
                                    {
                                        string value = "";
                                        if (val.Value is string)
                                            value = val.Value.ToString() ?? "";
                                        tableRow.Add(value);
                                    }
                                    catch (Exception ex)
                                    {
                                        GlobalHandler.WriteCrashLog(ex.ToString());
                                    }
                                }

                                if (tableRow.Count >= insightsDataTable.Columns.Count)
                                    insightsDataTable.Rows.Add([.. tableRow[..insightsDataTable.Columns.Count]]);
                                else
                                {
                                    var paddedRow = new List<string>(tableRow);
                                    while (paddedRow.Count < insightsDataTable.Columns.Count)
                                    {
                                        paddedRow.Add("");
                                    }
                                    insightsDataTable.Rows.Add([.. paddedRow]);
                                }
                            }
                            else
                            {
                                var dict = new Dictionary<string, object>();
                                foreach (PropertyInfo prop in data.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
                                {
                                    string propName = prop.Name;
                                    if (!issueRuleColumns.Exists(col => col.ColumnName == propName))
                                        continue;
                                    dict[propName] = prop.GetValue(data) ?? "";
                                }
                                if (index == 0)
                                    foreach (var val in dict)
                                    {
                                        try
                                        {
                                            insightsDataTable.Columns.Add(val.Key, typeof(string));
                                        }
                                        catch (Exception ex)
                                        {
                                            GlobalHandler.WriteCrashLog(ex.ToString());
                                        }
                                    }

                                List<string> tableRow = [];
                                foreach (var val in dict)
                                {
                                    try
                                    {
                                        string value = "";
                                        if (val.Value is string)
                                            value = val.Value.ToString() ?? "";
                                        else if (val.Value is bool)
                                            value = val.Value.ToString() == true.ToString() ? "Yes" : "No";
                                        else if (val.Value is DateTime)
                                            value = ((DateTime)val.Value).ToString("MM/dd/yyyy hh:mm tt");
                                        else if (val.Value is double || val.Value is float || val.Value is decimal)
                                            value = ((double)val.Value).ToString("0.00");
                                        else if (val.Value is int || val.Value is long || val.Value is short)
                                            value = ((int)val.Value).ToString();

                                        try
                                        {
                                            if (val.Value is JArray jArray)
                                            {
                                                value = JsonConvert.SerializeObject(jArray, Formatting.Indented);
                                            }
                                            else if (val.Value is JObject jObject)
                                            {
                                                value = JsonConvert.SerializeObject(jObject, Formatting.Indented);
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            value = val.Value.ToString() ?? "";
                                            Console.WriteLine(ex.ToString());
                                        }

                                        tableRow.Add(value);
                                    }
                                    catch (Exception ex)
                                    {
                                        GlobalHandler.WriteCrashLog(ex.ToString());
                                    }
                                }

                                if (tableRow.Count >= insightsDataTable.Columns.Count)
                                    insightsDataTable.Rows.Add([.. tableRow[..insightsDataTable.Columns.Count]]);
                                else
                                {
                                    var paddedRow = new List<string>(tableRow);
                                    while (paddedRow.Count < insightsDataTable.Columns.Count)
                                    {
                                        paddedRow.Add("");
                                    }
                                    insightsDataTable.Rows.Add([.. paddedRow]);
                                }
                            }
                            index++;
                        }
                        catch (Exception ex)
                        {
                            GlobalHandler.WriteCrashLog(ex.ToString());
                        }
                    }
                    insightsDatasetTable.Rows.Add(
                        issueRule.IssueGroup,
                        issueRule.Name,
                        issueRule.Description,
                        issueRule.IssueCount,
                        issueRule.MaxIssuable,
                        detailsTableName
                    );
                }
            }
        }

        public static void GenerateUnusedDataset(DataTable unusedObjectsTable, List<IssueRulesMetadataDTO> result)
        {
            foreach (var issueRule in result)
            {
                foreach (var data in issueRule.Data)
                {
                    try
                    {
                        if (data == null)
                            continue;

                        if (data is ColumnSummary columnSummary)
                        {
                            string tableName = columnSummary.TableName ?? "";
                            string objectType = "Column";
                            string objectName = columnSummary.ColumnName ?? "";

                            unusedObjectsTable.Rows.Add(tableName, objectType, objectName);
                        }
                        else if (data is MeasureSummary measureSummary)
                        {
                            string tableName = measureSummary.TableName ?? "";
                            string objectType = "Measure";
                            string objectName = measureSummary.MeasureName ?? "";

                            unusedObjectsTable.Rows.Add(tableName, objectType, objectName);
                        }
                        else
                        {
                            Console.WriteLine("Unexpected data format in unused dataset generation." + " " + data.GetType().ToString());
                        }
                    }
                    catch (Exception ex)
                    {
                        GlobalHandler.WriteCrashLog(ex.ToString());
                    }
                }
            }
        }
    
        public static void GenerateAlignmentDataset(List<IssueRulesMetadataDTO> issues, DataSet alignmentDataSet, DataTable alignmentDatasetTable)
        {
            if (issues != null)
            {
                ReportUIHandler.CreateAlignmentDataSet(issues[0].Data, alignmentDataSet, "Vertical Alignment");
                ReportUIHandler.CreateAlignmentDataSet(issues[1].Data, alignmentDataSet, "Horizontal Alignment");
                ReportUIHandler.CreateSpacingDataSet(issues[2].Data, alignmentDataSet, "Vertical Spacing");
                ReportUIHandler.CreateSpacingDataSet(issues[3].Data, alignmentDataSet, "Horizontal Spacing");
                ReportUIHandler.CreateMarginDataSet(issues[4].Data, alignmentDataSet, "Top Margin");
                ReportUIHandler.CreateMarginDataSet(issues[5].Data, alignmentDataSet, "Left Margin");
                ReportUIHandler.CreateMarginDataSet(issues[6].Data, alignmentDataSet, "Bottom Margin");
                ReportUIHandler.CreateMarginDataSet(issues[7].Data, alignmentDataSet, "Right Margin");

                alignmentDatasetTable.Rows.Add("Vertical Alignment", issues[0].IssueCount, issues[0].MaxIssuable, "A_Vertical Alignment");
                alignmentDatasetTable.Rows.Add("Horizontal Alignment", issues[1].IssueCount, issues[1].MaxIssuable, "A_Horizontal Alignment");
                alignmentDatasetTable.Rows.Add("Vertical Spacing", issues[2].IssueCount, issues[2].MaxIssuable, "A_Vertical Spacing");
                alignmentDatasetTable.Rows.Add("Horizontal Spacing", issues[3].IssueCount, issues[3].MaxIssuable, "A_Horizontal Spacing");
                alignmentDatasetTable.Rows.Add("Top Margin", issues[4].IssueCount, issues[4].MaxIssuable, "A_Top Margin");
                alignmentDatasetTable.Rows.Add("Bottom Margin", issues[5].IssueCount, issues[5].MaxIssuable, "A_Bottom Margin");
                alignmentDatasetTable.Rows.Add("Left Margin", issues[6].IssueCount, issues[6].MaxIssuable, "A_Left Margin");
                alignmentDatasetTable.Rows.Add("Right Margin", issues[7].IssueCount, issues[7].MaxIssuable, "A_Right Margin");
            }
        }

        // public static void GenerateLineageDataset(List<ImpactAnalysisItemDTO> items, DataTable lineageTable)
        // {
        //     foreach (var item in items)
        //     {
        //         try
        //         {
        //             HashSet<string> parentIds = new();
        //             foreach (var parent in item.ParentIds)
        //             {
        //                 string[] parts = parent.Split(new[] { "__|__Impact_Analysis_String_Join_Separator__|__" }, StringSplitOptions.None);
        //                 parentIds.Add(parts[parts.Length - 1].ToString());
        //             }
        //             // string[] ids = item.Id.Split(new[] { "__|__Impact_Analysis_String_Join_Separator__|__" }, StringSplitOptions.None);
        //             string parentId = string.Join(", ", parentIds);
        //             lineageTable.Rows.Add(
        //                 item.Name,
        //                 item.Type,
        //                 JsonConvert.SerializeObject(parentId, Formatting.Indented)
        //             );
        //         }
        //         catch (Exception ex)
        //         {
        //             GlobalHandler.WriteCrashLog(ex.ToString());
        //         }
        //     }
        // }

        public void GenerateReportDocumentationDataset(string cacheKey, DataTable docDatasetTable, DataSet docDataSet)
        {
            ReportDocumentation? reportDoc = ReportCache.Get(cacheKey);
            if (reportDoc != null)
            {
                ReportUIHandler.CreateReportSummaryDataSet(reportDoc.ReportSummary, docDataSet);
                ReportUIHandler.CreatePageDataSet(reportDoc.PageSummary, docDataSet);
                ReportUIHandler.CreateVisualDataSet(reportDoc.VisualList, docDataSet);
                ReportUIHandler.CreateBookmarkDataSet(reportDoc.BookmarkList, docDataSet);

                docDatasetTable.Rows.Add("Report Summary", reportDoc.ReportSummary.Count, "D_Report Summary");
                docDatasetTable.Rows.Add("Page Summary", reportDoc.PageSummary.Count, "D_Page Summary");
                docDatasetTable.Rows.Add("Visual Summary", reportDoc.VisualList.Count, "D_Visual Summary");
                docDatasetTable.Rows.Add("Bookmarks present in Report", reportDoc.BookmarkList.Count, "D_Bookmarks present in Report");
            }
        }
        
        public void GenerateModelDocumentationDataset(string cacheKey, DataTable docDatasetTable, DataSet docDataSet)
        {
            ModelDocumentation? modelDoc = ModelCache.Get(cacheKey);
            List<MeasureSummary> reportAllMeasures = modelDoc?.Measures ?? [];
            if (modelDoc != null)
            {
                ModelHandler.CreateModelSummaryDataSet(modelDoc.Summary ?? [], docDataSet);
                ModelHandler.CreateParameterDataSet(modelDoc.Parameters ?? [], docDataSet);
                ModelHandler.CreateTableDataSet(modelDoc.Tables ?? [], docDataSet);
                ModelHandler.CreateColumnDataSet(modelDoc.Columns ?? [], docDataSet);
                ModelHandler.CreateMeasuresDataSet(reportAllMeasures, docDataSet);
                ModelHandler.CreateRelationshipDataSet(modelDoc.Relationships ?? [], docDataSet);
                ModelHandler.CreateRoleDataSet(modelDoc.SecurityRoles ?? [], docDataSet);

                if (modelDoc.Summary != null)
                    docDatasetTable.Rows.Add("Model Summary", modelDoc.Summary.Count, "D_Model Summary");
                if (modelDoc.Parameters != null)
                    docDatasetTable.Rows.Add("Parameters", modelDoc.Parameters.Count, "D_Parameters");
                if (modelDoc.Tables != null)
                    docDatasetTable.Rows.Add("Tables", modelDoc.Tables.Count, "D_Tables");
                if (modelDoc.Columns != null)
                    docDatasetTable.Rows.Add("Columns", modelDoc.Columns.Count, "D_Columns");
                if (modelDoc.Measures != null)
                    docDatasetTable.Rows.Add("Measures", reportAllMeasures.Count, "D_Measures");
                if (modelDoc.Relationships != null)
                    docDatasetTable.Rows.Add("Relationships", modelDoc.Relationships.Count, "D_Relationships");
                if (modelDoc.SecurityRoles != null)
                    _ = docDatasetTable.Rows.Add("Security Roles", modelDoc.SecurityRoles.Count, "D_Security Roles");
            }
        }
   
        private void SaveExcel(string folderPath, string fileBaseName, List<DataSet> datasets, List<string> exportAreas)
        {
            string fileName = $"{fileBaseName}.xlsx";
            string filePath = Path.Combine(folderPath, fileName);
            GlobalHandler.CreateExcelExportFiles(filePath, datasets, exportAreas);
        }

        private string SanitizeName(string name)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
                name = name.Replace(c, '_');
            return name;
        }
    }
}