using Microsoft.AnalysisServices.Tabular;
using PowerBI_MCP.DTO;
using Newtonsoft.Json.Linq;
using PowerBI_MCP.Models;
using PowerBI_MCP.Handlers;
using PowerBI_MCP.Utils;
using System.Text;

namespace PowerBI_MCP.Handlers
{
    public class DocumentationHandler
    {
        public ModelDocumentation GenerateModelDocumentation(Database database)
        {
            if (database == null)
                throw new Exception("Failed to generated documentation, database is null");
            Model databaseModel = database.Model;

            RelationshipDTO relationshipData = ModelHandler.GetRelationshipData(databaseModel.Relationships);
            SecurityRoleDTO securityRoleData = ModelHandler.GetRolesData(databaseModel.Roles);
            TableColumnMeasureDTO tableColumnMeasureData = ModelHandler.GetTableColumnMeasureData(
                databaseModel.Tables,
                databaseModel,
                relationshipData.tableInRelationship,
                relationshipData.relationshipColumns,
                securityRoleData.securityColumns,
                securityRoleData.securityTables
            );
            List<SemanticModelSummary> modelDataList = ModelHandler.GetModelInfoDataModel(database, databaseModel, tableColumnMeasureData);
            List<ParameterSummary> parameterDataList = ModelHandler.GetParameterDataModel(databaseModel.Expressions);
            foreach (var table in tableColumnMeasureData.tableDataList)
            {
                var expressionDependency = ModelDependencyHandler.FormatExpressions(table.CalculatedTableExpression, table.TableName, tableColumnMeasureData.tableDataList, tableColumnMeasureData.columnDataList, tableColumnMeasureData.measureDataList);
                table.FormattedExpressionAndDependency = expressionDependency;

                table.FormattedRefreshPolicyExpressionAndDependency = ModelDependencyHandler.FormatExpressions(table.RefreshPolicyExpression, table.TableName, tableColumnMeasureData.tableDataList, tableColumnMeasureData.columnDataList, tableColumnMeasureData.measureDataList);
                List<FormattedExpressionAndDependency> newCalcItems = [];
                foreach (var calcItem in table.CalculationGroups ?? [])
                {
                    var calcExpressionDependency = ModelDependencyHandler.FormatExpressions(calcItem.FormatedExpression, table.TableName, tableColumnMeasureData.tableDataList, tableColumnMeasureData.columnDataList, tableColumnMeasureData.measureDataList);
                    newCalcItems.Add(calcExpressionDependency);
                }
                table.CalculationGroups = newCalcItems;
            }

            foreach (var column in tableColumnMeasureData.columnDataList)
            {
                var expressionDependency = ModelDependencyHandler.FormatExpressions(column.Expression, column.TableName, tableColumnMeasureData.tableDataList, tableColumnMeasureData.columnDataList, tableColumnMeasureData.measureDataList);
                column.FormattedExpressionAndDependency = expressionDependency;
            }


            foreach (var measure in tableColumnMeasureData.measureDataList)
            {
                var expressionDependency = ModelDependencyHandler.FormatExpressions(measure.Expression, measure.TableName, tableColumnMeasureData.tableDataList, tableColumnMeasureData.columnDataList, tableColumnMeasureData.measureDataList);
                measure.FormattedExpressionAndDependency = expressionDependency;
            }


            foreach (var role in securityRoleData.rolesDataList)
            {
                var expressionDependency = ModelDependencyHandler.FormatExpressions(role.RowLevelSecurityFilter, role.TableName, tableColumnMeasureData.tableDataList, tableColumnMeasureData.columnDataList, tableColumnMeasureData.measureDataList);
                role.FormattedExpressionAndDependency = expressionDependency;
            }

            ModelDocumentation modelDocumentation = new ModelDocumentation()
            {
                Summary = modelDataList,
                Parameters = parameterDataList,
                Tables = tableColumnMeasureData.tableDataList,
                Columns = tableColumnMeasureData.columnDataList,
                Hierarchies = tableColumnMeasureData.Hierarchies,
                Measures = tableColumnMeasureData.measureDataList,
                Relationships = relationshipData.relationshipList,
                SecurityRoles = securityRoleData.rolesDataList,
                TablesInRelationship = relationshipData.tableInRelationship,
                ColumnsInRelationship = relationshipData.relationshipColumns,
            };

            Console.WriteLine($"\nModel Documentation Summary:");
            Console.WriteLine($"  Tables: {modelDataList?.Count ?? 0}");
            Console.WriteLine($"  Table Details: {tableColumnMeasureData.tableDataList?.Count ?? 0}");
            Console.WriteLine($"  Columns: {tableColumnMeasureData.columnDataList?.Count ?? 0}");
            Console.WriteLine($"  Measures: {tableColumnMeasureData.measureDataList?.Count ?? 0}");
            Console.WriteLine($"  Parameters: {parameterDataList?.Count ?? 0}");
            Console.WriteLine($"  Relationships: {relationshipData.relationshipList?.Count ?? 0}");
            Console.WriteLine($"  Security Roles: {securityRoleData.rolesDataList?.Count ?? 0}");
            Console.WriteLine($"  Hierarchies: {tableColumnMeasureData.Hierarchies?.Count ?? 0}");
            Console.WriteLine("=".PadRight(80, '='));

            return modelDocumentation;
        }
    
        public ReportDocumentation? GenerateReportDocumentation(bool isPBIR, string reportFolderPath, string reportId)
        {
            FilterHandler.clearDict();
            JObject? layoutJson = new();
            if (isPBIR)
            {
                layoutJson = ReportUIHandler.GetLayoutFileFromPBIR(reportFolderPath);
            }
            else
            {
                layoutJson = GlobalHandler.ReadJsonFile(reportFolderPath + "\\report.json", Encoding.UTF8);
                if (layoutJson == null)
                    layoutJson = (JObject) GlobalHandler.ReadLayout(reportFolderPath+"\\Report\\Layout");
            }
            if (layoutJson == null)
                return new ReportDocumentation();

            int pageCount = 0;
            int drillThroughPageCount = 0;
            int tooltipPageCount = 0;
            int customViz = 0;

            string[] powerBIViz = AppConfig.PowerBIViz;

            ReportDocumentation reportDocumentation = new();
            // reportDocumentation.WorkspaceId = args.WorkspaceId;
            reportDocumentation.ReportId = reportId;

            // Fetch definition.pbir file if it exists
            string definitionPbirPath = Path.Combine(reportFolderPath, "definition.pbir");
            string definitionPbirBase64 = "";
            if (File.Exists(definitionPbirPath))
            {
                byte[] definitionPbirData = File.ReadAllBytes(definitionPbirPath);
                definitionPbirBase64 = Convert.ToBase64String(definitionPbirData);
            }

            List<Dictionary<string, List<string>>> drillThroughAndTooltipData = ReportUIHandler.GetPageWiseDrillThroughAndTooltipFilters(layoutJson);
            Dictionary<string, List<string>> drillThroughPages = drillThroughAndTooltipData[0];
            Dictionary<string, List<string>> tooltipFilterPages = drillThroughAndTooltipData[1];
            var themeData = ReportUIHandler.GetCurrentTheme(reportFolderPath+"\\Report", layoutJson, isPBIR);

            reportDocumentation.Theme = themeData.ContainsKey("themeJson") == true ? themeData["themeJson"] : "{}";
            JObject themeJson = themeData.ContainsKey("themeJson") == true ? JObject.Parse(themeData["themeJson"]) : [];
            reportDocumentation.ThemeName = themeData.ContainsKey("themeJsonFileName") == true ? themeData["themeJsonFileName"] : "";
            ReportUIFormattingModel reportUIFormattingObj = new();
            reportDocumentation.Measures = ReportUIHandler.ReportUIExtendedMeasures(layoutJson, reportId, isPBIR);
            ReportUISettingsModel reportUISettingsModel = ReportUIHandler.ReportUIDoc(layoutJson, reportId, reportId, isPBIR);
            reportDocumentation.ReportUISettings = reportUISettingsModel;
            JToken pages = layoutJson["sections"] ?? new JArray() { };

            foreach (var page in pages)
            {
                if (page == null)
                    continue;
                pageCount++;
                GetPageDetailReturnType? getPageDetailReturnType = ReportUIHandler.GetPageDetails(
                    page,
                    drillThroughPages,
                    tooltipFilterPages,
                    reportUISettingsModel.IsPersonalizationEnabled,
                    reportId,
                    reportId,
                    isPBIR
                );
                if (getPageDetailReturnType == null)
                    continue;
                Dictionary<string, List<string>> disabledEditInteractions = getPageDetailReturnType.DisabledEditInteractions ?? new();
                PageSummary? PageSummary = getPageDetailReturnType.PageSummary;
                if (PageSummary == null)
                    continue;
                PageSummary.PageIndex = pageCount;
                string pageId = PageSummary.PageId ?? "pageId";
                string pageName = PageSummary.PageName ?? "pageName";

                if (PageSummary.PageType == "Drillthrough Page")
                    drillThroughPageCount++;
                if (PageSummary.PageType == "Tooltip Page")
                    tooltipPageCount++;
                foreach (var vis in page["visualContainers"] ?? new JArray() { })
                {
                    GetVisualDetailsReturnType? getVisualDetailsReturnType = ReportUIHandler.GetVisualDetails(
                        vis,
                        disabledEditInteractions,
                        reportUISettingsModel.IsPersonalizationEnabled,
                        pageId,
                        pageName,
                        reportId,
                        reportId,
                        themeJson,
                        isPBIR
                    );
                    if (getVisualDetailsReturnType == null)
                        continue;
                    if (getVisualDetailsReturnType.IsGroup && getVisualDetailsReturnType.VisualGroup.GroupId != null)
                    {
                        if (reportDocumentation.visualGroups.ContainsKey(getVisualDetailsReturnType.VisualGroup.GroupId) == false)
                        {
                            reportDocumentation.visualGroups.Add(getVisualDetailsReturnType.VisualGroup.GroupId, getVisualDetailsReturnType.VisualGroup);
                        }
                        continue;
                    }
                    if (getVisualDetailsReturnType.AllUsedFontFamilies != null)
                    {
                        foreach (var fontFamilies in getVisualDetailsReturnType.AllUsedFontFamilies)
                        {
                            try
                            {
                                if (reportDocumentation.AllUsedFontFamilies.ContainsKey(fontFamilies.Key) == true)
                                    reportDocumentation.AllUsedFontFamilies[fontFamilies.Key].AddRange(fontFamilies.Value);
                                else
                                    reportDocumentation.AllUsedFontFamilies.Add(fontFamilies.Key, fontFamilies.Value);
                            }
                            catch (Exception ex)
                            {
                                GlobalHandler.WriteCrashLog(ex.ToString());
                            }
                        }
                    }
                    VisualSummary VisualSummary = getVisualDetailsReturnType.VisualSummary;
                    if (VisualSummary == null)
                        continue;

                    VisualSummary.ReportName = reportId;
                    string? visualId = VisualSummary.VisualId;
                    string? ogVisType = VisualSummary.OgVisualType;
                    reportDocumentation.VisualList.Add(VisualSummary);

                    if (visualId != null && !reportDocumentation.VisualDataDictionary.ContainsKey(visualId))
                        reportDocumentation.VisualDataDictionary.Add(visualId, VisualSummary);

                    foreach (var column in (VisualSummary.Columns ?? "").Split(","))
                    {
                        if (!reportDocumentation.ColumnDictionary.ContainsKey(column.Trim()))
                            reportDocumentation.ColumnDictionary.Add(column.Trim(), true);
                    }
                    foreach (var measure in (VisualSummary.Measures ?? "").Split(","))
                    {
                        if (!reportDocumentation.MeasureDictionary.ContainsKey(measure.Trim()))
                            reportDocumentation.MeasureDictionary.Add(measure.Trim(), true);
                    }

                    if (!powerBIViz.Contains(ogVisType))
                        customViz++;

                    foreach (var slicer in getVisualDetailsReturnType.ReportUIFormattingModel.SlicersList)
                    {
                        reportUIFormattingObj.SlicersList.Add(slicer);
                    }

                    foreach (var tableOrMatrix in getVisualDetailsReturnType.ReportUIFormattingModel.TableOrMatrixList)
                    {
                        reportUIFormattingObj.TableOrMatrixList.Add(tableOrMatrix);
                    }

                    foreach (var staticComp in getVisualDetailsReturnType.ReportUIFormattingModel.StaticComponentsList)
                    {
                        reportUIFormattingObj.StaticComponentsList.Add(staticComp);
                    }
                }
                List<VisualSummary> thisPageVisuals = reportDocumentation.VisualList.Where(x => x.PageId == pageId).ToList();
                PageSummary.TotalVisuals = thisPageVisuals.Count.ToString();
                PageSummary.TotalStatic = thisPageVisuals.Where(x => (new[] { "image", "shape", "actionButton", "textbox", "basicShape" }).Contains(x.OgVisualType)).Count().ToString();
                PageSummary.TotalCustom = thisPageVisuals.Where(x => x.IsCustom == true).Count().ToString();
                PageSummary.TotalHidden = thisPageVisuals.Where(x => x.IsHidden == true).Count().ToString();
                PageSummary.TotalSlicers = thisPageVisuals.Where(x => x.OgVisualType == "slicer").Count().ToString();
                PageSummary.TotalGrids = thisPageVisuals.Where(x => x.OgVisualType == "tableEx" || x.OgVisualType == "pivotTable").Count().ToString();
                PageSummary.ReportName = reportId;

                reportDocumentation.PageSummary.Add(PageSummary);
                if (reportDocumentation.PageDictionary.ContainsKey(pageId) == false)
                    reportDocumentation.PageDictionary.Add(pageId, PageSummary);
            }
            reportDocumentation.ReportUIFormatting = reportUIFormattingObj;
            ReportUIHandler.FixVisualsPosition(reportDocumentation.VisualList, reportDocumentation.visualGroups);
            ReportUIHandler.FixDisabledEditInteractionsFormat(reportDocumentation.VisualList, reportDocumentation.VisualDataDictionary);

            List<ReportSummary> reportSummary =
            [
                new ReportSummary()
                {
                    ReportName = reportUISettingsModel.ReportName,
                    Field = "Total Pages ",
                    Value =
                        pageCount.ToString()
                        + " (Normal: "
                        + (pageCount - tooltipPageCount - drillThroughPageCount).ToString()
                        + ", Tooltip: "
                        + tooltipPageCount.ToString()
                        + ", DrillThrough:"
                        + drillThroughPageCount.ToString()
                        + ")",
                },
                new ReportSummary()
                {
                    ReportName = reportUISettingsModel.ReportName,
                    Field = "Report Filters",
                    Value = reportUISettingsModel.ReportFiltersString,
                },
                new ReportSummary()
                {
                    ReportName = reportUISettingsModel.ReportName,
                    Field = "Custom Visuals",
                    Value = customViz.ToString(),
                },
                new ReportSummary()
                {
                    ReportName = reportUISettingsModel.ReportName,
                    Field = "Custom Theme",
                    Value = reportUISettingsModel.Theme ?? "No custom theme is applied to this report",
                },
                new ReportSummary()
                {
                    ReportName = reportUISettingsModel.ReportName,
                    Field = "Is Personalization Enabled",
                    Value = reportUISettingsModel.IsPersonalizationEnabled.ToString(),
                },
                new ReportSummary()
                {
                    ReportName = reportUISettingsModel.ReportName,
                    Field = "Is Filter Pane Expanded ",
                    Value = reportUISettingsModel.IsFilterPaneExpanded.ToString(),
                },
            ];
            reportDocumentation.ReportSummary = reportSummary;

            foreach (var row in FilterHandler.measureDict)
            {
                if (!reportDocumentation.MeasureDictionary.ContainsKey(row.Key.Trim()))
                    reportDocumentation.MeasureDictionary.Add(row.Key.Trim(), true);
            }

            foreach (var row in FilterHandler.columnDict)
            {
                if (!reportDocumentation.ColumnDictionary.ContainsKey(row.Key.Trim()))
                    reportDocumentation.ColumnDictionary.Add(row.Key.Trim(), true);
            }

            reportDocumentation.BookmarkList = ReportUIHandler.CreateBookMarkList(layoutJson, reportDocumentation.VisualDataDictionary, reportDocumentation.PageDictionary, reportId, reportId, isPBIR);

            foreach (var page in reportDocumentation.PageSummary)
            {
                page.TotalBookmarks = reportDocumentation.BookmarkList.Where(x => x.PageId == page.PageId).Count();
            }

            List<ReportDefinitionFile> reportDefinitionFiles = new();
            try
            {
                if (Directory.Exists(reportFolderPath))
                {
                    var jsonFiles = Directory.GetFiles(reportFolderPath, "*.json", SearchOption.AllDirectories);
                    foreach (var file in jsonFiles)
                    {
                        // Save the relative path from the ReportFolderPath root
                        string relativePath = Path.GetRelativePath(reportFolderPath, file);
                        reportDefinitionFiles.Add(new ReportDefinitionFile
                        {
                            // RelativePath = Path.GetFileName(file)
                            RelativePath = relativePath,
                            Content = System.IO.File.ReadAllText(file)
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                // return StatusCode(500, $"Error while reading JSON files: {ex.Message}");
            }
            Console.WriteLine($"Found {reportDefinitionFiles.Count} JSON files in report folder.");

            reportDocumentation.ReportDefinitionFiles = reportDefinitionFiles.ToArray();

            return reportDocumentation;
        }
    }
}