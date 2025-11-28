using PowerBI_MCP.Entities;
using PowerBI_MCP.DTO;
using PowerBI_MCP.Handlers;
using PowerBI_MCP.Repositories;
using PowerBI_MCP.Utils.Cache;
using Newtonsoft.Json;
using PowerBI_MCP.Interfaces;
using PowerBI_MCP.Service.IssueDataGenerator;
using PowerBI_MCP.Models;
using PowerBI_MCP.Utils;
using System.Text.Json;

namespace PowerBI_MCP.Service
{
    public class IssueService : IIssueService
    {
        private Dictionary<string, Dictionary<string, string>>? measureDetails;
        private Dictionary<string, Dictionary<string, string>>? calculatedColumns;
        public ModelDocumentation? modelDocumentation;
        public ReportDocumentation? reportDocumentation;

        public List<IssueSection> GetIssueSection()
        {
            return IssueMetadataRepo.Instance.GetIssueSection();
        }

        public async Task<object> GetUnusedFieldData(string modelName, string reportName)
        {
            List<DatasetModel> models = GlobalHandler.GetModelByName(modelName);
            List<ReportModel> reports = GlobalHandler.GetReportByName(reportName);
            if (reports.Count == 0) throw new ErrorDTO("No report found with the name " + reportName);
            if (models.Count == 0) throw new ErrorDTO("No model found with the name " + modelName);
            if (models.Count > 1) throw new ErrorDTO("Multiple models found with the name " + modelName + ". Please provide a unique model name.");
            if (reports.Count > 1) throw new ErrorDTO("Multiple reports found with the name " + reportName + ". Please provide a unique report name.");
            DatasetModel dataset = models[0];
            ReportModel report = reports[0];
            Dictionary<string, List<ReportModel>> datasetsToReports = new();
            datasetsToReports.Add(dataset.DatasetId, new List<ReportModel> { report });

            Console.WriteLine("Dataset ID: " + dataset.DatasetId);
            Console.WriteLine("Report ID: " + report.ReportId);

            // foreach (var artifact in execution.ExecutionArtifacts)
            // {
            //     if (artifact.ArtifactType == ArtifactTypes.REPORT && artifact.ParentId != null)
            //     {
            //         if(datasetsToReports.ContainsKey(artifact.ParentId) == false) datasetsToReports.Add(artifact.ParentId, []);
            //         datasetsToReports[artifact.ParentId].Add(artifact);
            //     }
            //     else if (artifact.ArtifactType == ArtifactTypes.SEMANTIC_MODEL)
            //     {
            //         if (!datasetsToReports.ContainsKey(artifact.ArtifactId))
            //             datasetsToReports.Add(artifact.ArtifactId, []);
            //     }
            // }

            List<IssueRulesMetadata> issueRulesMetadata = IssueMetadataRepo.Instance.GetUnusedFieldRelatedIssueMetadata();
            Dictionary<string, List<IssueRulesMetadataDTO>> result = new();
            Dictionary<string, ModelDocumentation> modelDocs = new();
            foreach (var datasetId in datasetsToReports.Keys)
            {
                if (dataset == null) continue;
                string modelDocCacheId = datasetId;
                ModelDocumentation? modelDoc = ModelCache.Get(modelDocCacheId);
                if (modelDoc == null)
                    throw new ErrorDTO("No Documentation found for Semantic model " + dataset.DatasetId);

                var associatedReports = datasetsToReports[datasetId];
                List<ReportDocumentation> reportUIDocs = new();
                foreach (var associatedReport in associatedReports)
                {
                    ReportDocumentation? reportDoc = ReportCache.Get(associatedReport.ReportId);
                    if (reportDoc != null) reportUIDocs.Add(reportDoc);
                }

                modelDoc = UnusedHandler.Handle(modelDoc, reportUIDocs);
                ModelCache.Set(modelDocCacheId, modelDoc);
                
                List<IssueRulesMetadataDTO> issues = new();

                foreach (var issueMetadata in issueRulesMetadata)
                {
                    IssueRulesMetadataDTO dto = JsonConvert.DeserializeObject<IssueRulesMetadataDTO>(JsonConvert.SerializeObject(issueMetadata)) ?? new();

                    IArtifactGroupIssueGenerator issueGenerator = IssueDataGeneratorFactory.Instance.CreateArtifactGroupIssueGenerator(issueMetadata.DataGeneratorFunction);
                    //issueGenerator.SetAssociatedReportsUIDoc(reportUIDocs);

                    IssueContext issueContent = new()
                    {
                        ModelDocumentation = modelDoc ?? new()
                    };
                    SingleIssueRuleData? singleIssueRuleData = issueGenerator.GetData(issueContent);

                    dto.Data = singleIssueRuleData?.Issues ?? [];
                    dto.IssueCount = (singleIssueRuleData?.Issues ?? []).Count;
                    dto.MaxIssuable = singleIssueRuleData?.MaxIssuable ?? 0;
                    issues.Add(dto);
                }
                result.Add(datasetId, issues);
                var modelDocWithTableColAndMeas = new ModelDocumentation()
                {
                    Tables = modelDoc?.Tables ?? [],
                    Columns  = modelDoc?.Columns ?? [],
                    Measures= modelDoc?.Measures ?? []
                }; 
                modelDocs.Add(datasetId, modelDocWithTableColAndMeas);
            }
            return new { result, modelDocs };
        }
        
        public object GetAlignmentIssues(string reportName, string? spacing, string userEmail)
        {
            string workspaceId = "", artifactId = "";
            List<ReportModel> reports = GlobalHandler.GetReportByName(reportName);
            if (reports.Count == 0)
                throw new Exception($"report not found, please connect this report");
            if (reports.Count > 1)
            {
                string reportPaths = string.Join(", ", reports.Select(r => r.ReportPath));
                throw new Exception($"There are more than one report with the name '{reportName}'. Report paths: {reportPaths}");
            }
            artifactId = reports[0].ReportId;
            ReportDocumentation? reportDoc = ReportCache.Get(GlobalHandler.GetArtifactCacheKey(userEmail, workspaceId.ToString(), artifactId));
            if (reportDoc == null) throw new ErrorDTO("No Documentation found for Report " + artifactId);
            List<IssueRulesMetadata> issueRulesMetadata = IssueMetadataRepo.Instance.GetVisualAlignmentRelatedIssueMetadata();
            List<IssueRulesMetadataDTO> issues = new();

            foreach (var issueMetadata in issueRulesMetadata)
            {
                IssueRulesMetadataDTO dto = JsonConvert.DeserializeObject<IssueRulesMetadataDTO>(JsonConvert.SerializeObject(issueMetadata)) ?? new();

                IVisualAlignmentIssueGenerator issueGenerator = IssueDataGeneratorFactory.Instance.CreateVisualAlignmentIssueGenerator(issueMetadata.DataGeneratorFunction);

                IssueContext issueContent = new()
                {
                    ModelDocumentation = modelDocumentation ?? new(),
                    ReportDocumentation = reportDoc,
                    MeasureDetails = measureDetails ?? new(),
                    CalculatedColumns = calculatedColumns ?? new()
                };
                issueGenerator.SetAssociatedReportsUIDoc(new List<ReportDocumentation> { reportDoc });
                issueGenerator.SetVisualSpacing(spacing);
                SingleIssueRuleData? singleIssueRuleData = issueGenerator.GetData(issueContent);

                dto.Data = singleIssueRuleData?.Issues ?? [];
                dto.IssueCount = (singleIssueRuleData?.Issues ?? []).Count;
                dto.MaxIssuable = singleIssueRuleData?.MaxIssuable ?? 0;
                issues.Add(dto);
            }
            string extractionPath = Path.Combine(AppConfig.duplicateReportZipDirectory);
            Directory.CreateDirectory(extractionPath);
            string filePath = Path.Combine(extractionPath, "alignment.json");
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = System.Text.Json.JsonSerializer.Serialize(issues, options);
            File.WriteAllTextAsync(filePath, json);
            Console.WriteLine($"JSON saved to: {filePath}");
            return issues;
        }

        public AllIssueRuleData GetData(string artifactName, string artifactType, string userEmail)
        {
            string artifactId = "";
            if (artifactType == ArtifactTypes.SEMANTIC_MODEL)
            {
                List<DatasetModel> datasets = GlobalHandler.GetModelByName(artifactName);
                if (datasets.Count == 0)
                    throw new Exception($"semantic model not found, please connect this semantic model");
                if (datasets.Count > 1)
                {
                    string datasetInfos = string.Join("; ", datasets.Select(d =>
                        $"Server: {d.ServerName}, Database: {d.DbName}, ConnectionType: {d.ConnectionType}"));

                    throw new Exception($"There are more than one semantic model with the name '{artifactName}'. Models: {datasetInfos}");
                }
                artifactId = datasets[0].DatasetId;
            }
            else
            {
                List<ReportModel> reports = GlobalHandler.GetReportByName(artifactName);
                if (reports.Count == 0)
                    throw new Exception($"report not found, please connect this report");
                if (reports.Count > 1)
                {
                    string reportPaths = string.Join(", ", reports.Select(r => r.ReportPath));
                    throw new Exception($"There are more than one report with the name '{artifactName}'. Report paths: {reportPaths}");
                }
                artifactId = reports[0].ReportId;
            }
            string key = GlobalHandler.GetArtifactCacheKey(userEmail, "", artifactId);
            
            if (artifactType == ArtifactTypes.SEMANTIC_MODEL)
            {
                modelDocumentation = ModelCache.Get(key);
                Console.WriteLine($"  Model Documentation from cache: {(modelDocumentation != null ? "FOUND" : "NOT FOUND")}");
                if (modelDocumentation != null)
                {
                    Console.WriteLine($"  Model Documentation Tables Count: {modelDocumentation.Tables?.Count ?? 0}");
                    if (modelDocumentation.Tables != null)
                    {
                        Console.WriteLine("  Model Documentation Table Names:");
                        foreach (var table in modelDocumentation.Tables)
                        {
                            Console.WriteLine($"    - {table.TableName}");
                            if (table.TableName != null && table.TableName.Contains("Users", StringComparison.OrdinalIgnoreCase))
                            {
                                Console.WriteLine($"      ⚠️⚠️⚠️ FOUND 'Users' TABLE IN MODELDOC FOR ISSUE GENERATION: {table.TableName} ⚠️⚠️⚠️");
                            }
                        }
                    }
                }
            }
            else
            {
                reportDocumentation = ReportCache.Get(key);
            }
            ModelDependencyHandler modelDependencyHandler = new(modelDocumentation ?? new ModelDocumentation());
            measureDetails = modelDependencyHandler.MeasureDetails ?? [];
            calculatedColumns = modelDependencyHandler.calColumnDetails ?? [];


            AllIssueRuleData allIssueRuleData = new();
            List<IssueRulesMetadataDTO> failedComplianceRule = [];
            List<IssueRulesMetadataDTO> disabledComplianceRule = [];
            List<IssueRulesMetadataDTO> complianceRule = [];

            List<IssueRulesMetadata> issueRulesMetadata = IssueMetadataRepo.Instance.GetAllIssuesMetadata();

            foreach (var issueMetadata in issueRulesMetadata)
            {

                IssueRulesMetadataDTO dto = JsonConvert.DeserializeObject<IssueRulesMetadataDTO>(JsonConvert.SerializeObject(issueMetadata)) ?? new();

                if (issueMetadata.Visible == false)
                {
                    disabledComplianceRule.Add(dto);
                }

                if (artifactType == ArtifactTypes.SEMANTIC_MODEL && (issueMetadata.ApplicableArtifactType != IssueRuleSource.SEMANTIC_MODEL && issueMetadata.ApplicableArtifactType != IssueRuleSource.SEMANTIC_MODEL_OR_REPORT)) continue;
                if (artifactType == ArtifactTypes.REPORT && (issueMetadata.ApplicableArtifactType != IssueRuleSource.REPORT && issueMetadata.ApplicableArtifactType != IssueRuleSource.SEMANTIC_MODEL_OR_REPORT)) continue;
                if ((artifactType == ArtifactTypes.REPORT && reportDocumentation == null) || (artifactType == ArtifactTypes.SEMANTIC_MODEL && modelDocumentation == null))
                {
                    dto.Error = artifactType == (artifactType == ArtifactTypes.REPORT ? "Report UI" : "Semantic Model") + " Documentation not found.";
                    failedComplianceRule.Add(dto);
                    continue;
                }
                try
                {

                    IssueContext issueContent = new()
                    {
                        ModelDocumentation = modelDocumentation ?? new(),
                        ReportDocumentation = reportDocumentation ?? new(),
                        MeasureDetails = measureDetails ?? new(),
                        CalculatedColumns = calculatedColumns ?? new()
                    };
                    SingleIssueRuleData? singleIssueRuleData = IssueDataGeneratorFactory.Instance.CreateIssueDataGenerator(issueMetadata.DataGeneratorFunction)
                                                                .GetData(issueContent);
                    if (singleIssueRuleData == null)
                    {
                        dto.Error = "No data found";
                        failedComplianceRule.Add(dto);
                        continue;
                    }

                    dto.Data = singleIssueRuleData.Issues;
                    dto.IssueCount = singleIssueRuleData.Issues.Count;
                    dto.MaxIssuable = singleIssueRuleData.MaxIssuable ?? 0;

                    complianceRule.Add(dto);
                }
                catch (Exception ex)
                {
                    GlobalHandler.WriteCrashLog(ex.ToString());
                    dto.Error = ex.ToString();
                    failedComplianceRule.Add(dto);
                }

            }

            string extractionPath = Path.Combine(AppConfig.duplicateReportZipDirectory);
            Directory.CreateDirectory(extractionPath);
            string filePath = Path.Combine(extractionPath, "insights.json");
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = System.Text.Json.JsonSerializer.Serialize(complianceRule, options);
            File.WriteAllTextAsync(filePath, json);
            Console.WriteLine($"JSON saved to: {filePath}");

            allIssueRuleData.IssueRules = complianceRule;
            allIssueRuleData.FailedIssueRules = failedComplianceRule;
            allIssueRuleData.DisabledIssueRules = disabledComplianceRule;

            return allIssueRuleData;
        }
        
        // public FixedStatusResponseModel CheckFixedStatus(CheckFixedStatusReqModel request, string userEmail)
        // {
        //     var response = new FixedStatusResponseModel();

        //     // Get issue metadata
        //     var issueMetadata = IssueMetadataRepo.Instance.GetIssuesMetadataById(request.IssueId);
        //     if (issueMetadata == null)
        //     {
        //         return response; // Return empty response if issue not found
        //     }

        //     // Get cached model documentation
        //     string cacheKey = GlobalHandler.GetArtifactCacheKey(userEmail, request.WorkspaceId, request.ArtifactId);
        //     ModelDocumentation? modelDoc = ReportModelCache.Get(cacheKey);

        //     if (modelDoc == null)
        //     {
        //         return response; // Return empty response if documentation not found
        //     }

        //     // Create lookup dictionaries for fast access
        //     var columnLookup = new Dictionary<string, ColumnSummary>(StringComparer.OrdinalIgnoreCase);
        //     var measureLookup = new Dictionary<string, MeasureSummary>(StringComparer.OrdinalIgnoreCase);
        //     var tableLookup = new Dictionary<string, TableSummary>(StringComparer.OrdinalIgnoreCase);

        //     foreach (var col in modelDoc.Columns ?? [])
        //     {
        //         string key = $"{col.TableName}|{col.ColumnName}";
        //         columnLookup[key] = col;
        //     }

        //     foreach (var measure in modelDoc.Measures ?? [])
        //     {
        //         string key = $"{measure.TableName}|{measure.MeasureName}";
        //         measureLookup[key] = measure;
        //         // Debug logging for measures
        //         if (measure.MeasureName?.Contains("Projects") == true || measure.MeasureName?.Contains("Total") == true)
        //         {
        //             Console.WriteLine($"[CheckFixedStatus] Measure in cache: {key}, Description: '{measure.Description ?? "[null]"}'");
        //         }
        //     }

        //     Console.WriteLine($"[CheckFixedStatus] Total measures in lookup: {measureLookup.Count}");

        //     foreach (var table in modelDoc.Tables ?? [])
        //     {
        //         string key = table.TableName ?? "";
        //         if (!string.IsNullOrEmpty(key))
        //         {
        //             tableLookup[key] = table;
        //         }
        //     }

        //     // Check each row based on issue type
        //     string issueType = issueMetadata.DataGeneratorFunction;

        //     Console.WriteLine($"[CheckFixedStatus] Issue type: {issueType}, Checking {request.IssueData.Count} items");

        //     foreach (var row in request.IssueData)
        //     {
        //         string? rowKey = null;
        //         bool isFixed = false;
        //         string? description = null;

        //         // Determine row key and check fixed status based on issue type
        //         switch (issueType)
        //         {
        //             case IssueRuleFunctionMapper.COLUMN_MISSING_DESCRIPTION:
        //                 {
        //                     // Handle both camelCase and PascalCase field names
        //                     string? tableName = row.ContainsKey("tableName") ? row["tableName"]?.ToString() :
        //                                      row.ContainsKey("TableName") ? row["TableName"]?.ToString() : null;
        //                     string? columnName = row.ContainsKey("columnName") ? row["columnName"]?.ToString() :
        //                                         row.ContainsKey("ColumnName") ? row["ColumnName"]?.ToString() : null;

        //                     if (!string.IsNullOrEmpty(tableName) && !string.IsNullOrEmpty(columnName))
        //                     {
        //                         rowKey = $"{tableName}|{columnName}";
        //                         if (columnLookup.TryGetValue(rowKey, out var column))
        //                         {
        //                             isFixed = !string.IsNullOrWhiteSpace(column.Description);
        //                             description = column.Description;
        //                         }
        //                     }
        //                     break;
        //                 }

        //             case IssueRuleFunctionMapper.COLUMN_HIDE:
        //                 {
        //                     // Handle both camelCase and PascalCase field names
        //                     string? tableName = row.ContainsKey("tableName") ? row["tableName"]?.ToString() :
        //                                      row.ContainsKey("TableName") ? row["TableName"]?.ToString() : null;
        //                     string? columnName = row.ContainsKey("columnName") ? row["columnName"]?.ToString() :
        //                                         row.ContainsKey("ColumnName") ? row["ColumnName"]?.ToString() : null;

        //                     if (!string.IsNullOrEmpty(tableName) && !string.IsNullOrEmpty(columnName))
        //                     {
        //                         rowKey = $"{tableName}|{columnName}";
        //                         if (columnLookup.TryGetValue(rowKey, out var column))
        //                         {
        //                             // Column is fixed if it's hidden (Visible == "No")
        //                             isFixed = column.Visible?.Equals("No", StringComparison.OrdinalIgnoreCase) == true;
        //                             Console.WriteLine($"[CheckFixedStatus] COLUMN_HIDE - {tableName}.{columnName}: Visible='{column.Visible}', isFixed={isFixed}");
        //                         }
        //                         else
        //                         {
        //                             Console.WriteLine($"[CheckFixedStatus] COLUMN_HIDE - {tableName}.{columnName}: Column not found in lookup");
        //                         }
        //                     }
        //                     break;
        //                 }

        //             case IssueRuleFunctionMapper.MEASURE_MISSING_DESCRIPTION:
        //                 {
        //                     // Handle both camelCase and PascalCase field names
        //                     string? tableName = row.ContainsKey("tableName") ? row["tableName"]?.ToString() :
        //                                      row.ContainsKey("TableName") ? row["TableName"]?.ToString() : null;
        //                     string? measureName = row.ContainsKey("measureName") ? row["measureName"]?.ToString() :
        //                                          row.ContainsKey("MeasureName") ? row["MeasureName"]?.ToString() : null;

        //                     if (!string.IsNullOrEmpty(tableName) && !string.IsNullOrEmpty(measureName))
        //                     {
        //                         rowKey = $"{tableName}|{measureName}";
        //                         Console.WriteLine($"[CheckFixedStatus] MEASURE_MISSING_DESCRIPTION - Looking for key: {rowKey}");

        //                         if (measureLookup.TryGetValue(rowKey, out var measure))
        //                         {
        //                             isFixed = !string.IsNullOrWhiteSpace(measure.Description);
        //                             description = measure.Description;
        //                             Console.WriteLine($"[CheckFixedStatus] MEASURE_MISSING_DESCRIPTION - Found measure: {rowKey}, Description: '{description ?? "[null]"}', isFixed: {isFixed}");
        //                         }
        //                         else
        //                         {
        //                             Console.WriteLine($"[CheckFixedStatus] MEASURE_MISSING_DESCRIPTION - Measure NOT found in lookup: {rowKey}");
        //                             Console.WriteLine($"[CheckFixedStatus] Available measure keys (first 10): {string.Join(", ", measureLookup.Keys.Take(10))}");
        //                         }
        //                     }
        //                     else
        //                     {
        //                         Console.WriteLine($"[CheckFixedStatus] MEASURE_MISSING_DESCRIPTION - Missing tableName or measureName. tableName: '{tableName}', measureName: '{measureName}'");
        //                     }
        //                     break;
        //                 }

        //             case IssueRuleFunctionMapper.MISSING_TABLE_DESCRIPTION:
        //                 {
        //                     // Handle both camelCase and PascalCase field names
        //                     string? tableName = row.ContainsKey("tableName") ? row["tableName"]?.ToString() :
        //                                      row.ContainsKey("TableName") ? row["TableName"]?.ToString() : null;

        //                     if (!string.IsNullOrEmpty(tableName))
        //                     {
        //                         rowKey = tableName;
        //                         if (tableLookup.TryGetValue(rowKey, out var table))
        //                         {
        //                             isFixed = !string.IsNullOrWhiteSpace(table.Description);
        //                             description = table.Description;
        //                         }
        //                     }
        //                     break;
        //                 }

        //             case IssueRuleFunctionMapper.COLUMN_SUMMARIZE_BY_KEY_ID_COL:
        //                 {
        //                     // A key/ID column is considered fixed when SummarizeBy == "none"
        //                     string? tableName = row.ContainsKey("tableName") ? row["tableName"]?.ToString() :
        //                                      row.ContainsKey("TableName") ? row["TableName"]?.ToString() : null;
        //                     string? columnName = row.ContainsKey("columnName") ? row["columnName"]?.ToString() :
        //                                         row.ContainsKey("ColumnName") ? row["ColumnName"]?.ToString() : null;

        //                     if (!string.IsNullOrEmpty(tableName) && !string.IsNullOrEmpty(columnName))
        //                     {
        //                         rowKey = $"{tableName}|{columnName}";
        //                         if (columnLookup.TryGetValue(rowKey, out var column))
        //                         {
        //                             // ColumnSummary model stores this as 'Summarization' (string)
        //                             var summarization = column.Summarization;
        //                             isFixed = summarization?.Equals("none", StringComparison.OrdinalIgnoreCase) == true;
        //                         }
        //                     }
        //                     break;
        //                 }

        //             // Add more issue types as needed
        //             default:
        //                 // For unsupported issue types, try to generate a row key anyway
        //                 string? fallbackTableName = row.ContainsKey("tableName") ? row["tableName"]?.ToString() : null;
        //                 string? fallbackColumnName = row.ContainsKey("columnName") ? row["columnName"]?.ToString() : null;
        //                 string? fallbackMeasureName = row.ContainsKey("measureName") ? row["measureName"]?.ToString() : null;

        //                 if (!string.IsNullOrEmpty(fallbackTableName) && !string.IsNullOrEmpty(fallbackColumnName))
        //                 {
        //                     rowKey = $"{fallbackTableName}|{fallbackColumnName}";
        //                 }
        //                 else if (!string.IsNullOrEmpty(fallbackTableName) && !string.IsNullOrEmpty(fallbackMeasureName))
        //                 {
        //                     rowKey = $"{fallbackTableName}|{fallbackMeasureName}";
        //                 }
        //                 else if (!string.IsNullOrEmpty(fallbackTableName))
        //                 {
        //                     rowKey = fallbackTableName;
        //                 }
        //                 break;
        //         }

        //         if (!string.IsNullOrEmpty(rowKey))
        //         {
        //             response.FixedRowKeys[rowKey] = isFixed;
        //             if (!string.IsNullOrEmpty(description))
        //             {
        //                 response.Descriptions[rowKey] = description;
        //             }
        //             Console.WriteLine($"[CheckFixedStatus] RowKey: {rowKey}, isFixed: {isFixed}");
        //         }
        //         else
        //         {
        //             Console.WriteLine($"[CheckFixedStatus] ⚠️  Could not generate rowKey for row. Available keys: {string.Join(", ", row.Keys)}");
        //         }
        //     }

        //     Console.WriteLine($"[CheckFixedStatus] Response: {response.FixedRowKeys.Count} fixed keys found");
        //     foreach (var kvp in response.FixedRowKeys)
        //     {
        //         Console.WriteLine($"[CheckFixedStatus]   {kvp.Key}: {kvp.Value}");
        //     }

        //     return response;
        // }
    }
}