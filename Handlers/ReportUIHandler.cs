using System.Data;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using PowerBI_MCP.Utils;
using Newtonsoft.Json.Linq;
using PowerBI_MCP.DTO;
using System.Text;

namespace PowerBI_MCP.Handlers
{
    public static class ReportUIHandler
    {
        public static Dictionary<string, List<string>> measureOccurrences = [];
        public static Dictionary<string, List<string>> columnOccurrences = [];
        public static JObject? GetLayoutFileFromPBIR(string extractionPath)
        {
            string reportJsonFilePath = extractionPath + @"\definition\report.json";
            JObject? layoutJson = File.Exists(reportJsonFilePath) ? GlobalHandler.ReadJsonFile(reportJsonFilePath, Encoding.UTF8) : null;
            if (layoutJson == null)
                return null;

            // pages details
            JToken pages = GetPagesDataFromPBIR(extractionPath + @"\definition\pages");
            layoutJson["sections"] = pages;

            // report extensions details
            string reportExtensionDetailsFilePath = extractionPath + @"\definition\reportExtensions.json";
            JObject? layoutExtensionDetailsJson = File.Exists(reportExtensionDetailsFilePath) ? GlobalHandler.ReadJsonFile(reportExtensionDetailsFilePath, Encoding.UTF8) : null;

            if (layoutExtensionDetailsJson != null)
                layoutJson["modelExtensions"] = new JArray() { layoutExtensionDetailsJson };

            // bookmarks
            string bookmarksFolder = extractionPath + @"\definition\bookmarks";
            if (Directory.Exists(bookmarksFolder))
            {
                JArray bookmarksData = [];
                var bookmarkFiles = Directory.GetFiles(bookmarksFolder, "*.json");
                foreach (var bookmarkFile in bookmarkFiles)
                {
                    try
                    {
                        if (GlobalHandler.ReadJsonFile(bookmarkFile, Encoding.UTF8) is not JObject bookmarkDetails)
                            continue;
                        bookmarksData.Add(bookmarkDetails);
                    }
                    catch (Exception ex)
                    {
                        GlobalHandler.WriteCrashLog(ex.ToString());
                    }
                }
                layoutJson["bookmarks"] = bookmarksData;
            }

            // create layout file
            string layoutFilePath = extractionPath + @"\Layout";
            File.WriteAllText(layoutFilePath, layoutJson.ToString(), Encoding.Unicode);

            return layoutJson;
        }

        public static JArray GetPagesDataFromPBIR(string pagesPath)
        {
            JArray pages = [];
            if (Directory.Exists(pagesPath) == false)
                return pages;
            string pagesDetailsFilePath = pagesPath + @"\pages.json";
            if (File.Exists(pagesDetailsFilePath) == false)
                return [];
            JObject? pagesDetailsJson = GlobalHandler.ReadJsonFile(pagesDetailsFilePath, Encoding.UTF8);
            JArray? pageOrder = pagesDetailsJson?["pageOrder"] as JArray ?? [];
            if (pageOrder is null)
                return pages;

            foreach (JToken pageIdToken in pageOrder ?? [])
            {
                try
                {
                    string pageId = pageIdToken?.ToString() ?? "pageId";
                    string pageDirectory = Path.Combine(pagesPath, pageId);
                    string pageDetailFile = Path.Combine(pageDirectory, "page.json");
                    if (File.Exists(pageDetailFile))
                    {
                        if (GlobalHandler.ReadJsonFile(pageDetailFile, Encoding.UTF8) is not JObject pageDetails)
                            continue;
                        JArray allVisDetails = [];

                        if (Directory.Exists(pageDirectory + @"\visuals"))
                        {
                            string[] visualDirectories = Directory.GetDirectories(pageDirectory + @"\visuals");
                            foreach (string visualDirectory in visualDirectories)
                            {
                                try
                                {
                                    string visualDetailFile = visualDirectory + @"\visual.json";
                                    if (GlobalHandler.ReadJsonFile(visualDetailFile, Encoding.UTF8) is not JObject visualDetails)
                                        continue;
                                    allVisDetails.Add(visualDetails);
                                }
                                catch (Exception ex)
                                {
                                    GlobalHandler.WriteCrashLog(ex.ToString());
                                }
                            }
                            pageDetails["visualContainers"] = allVisDetails;
                            pages.Add(pageDetails);
                        }
                    }
                }
                catch (Exception ex)
                {
                    GlobalHandler.WriteCrashLog(ex.ToString());
                }
            }

            return pages;
        }

        public static Dictionary<string, string> GetCurrentTheme(string extractedFolder, JObject layoutFileJson, bool isPBIR = false)
        {
            Dictionary<string, string> returnData = [];

            //string layoutFilePath = extractedFolder + @"\Layout";
            string themeResourceFolder = extractedFolder + @"\StaticResources";

            //string fileContent = File.ReadAllText(layoutFilePath, Encoding.UTF8);    // changed Encoding.Unicode to Encoding.UTF8
            //var layoutFileJson = JObject.Parse(fileContent);
            bool isCustomTheme = false;
            string? themeName = isPBIR ? layoutFileJson["themeCollection"]?["customTheme"]?["name"]?.ToString() : layoutFileJson["theme"]?.ToString(); // changed "customTheme" to "baseTheme"
            if (themeName != null)
                isCustomTheme = true;
            JArray resourcePackagesArray = (JArray)(layoutFileJson["resourcePackages"] ?? "[]");

            foreach (var resourcePackage in resourcePackagesArray)
            {
                if (resourcePackage == null)
                    continue;
                string? themeFolder = isPBIR ? resourcePackage["name"]?.ToString() : resourcePackage["resourcePackage"]?["name"]?.ToString();

                if (themeFolder == null)
                    continue;

                if (isCustomTheme == false && themeFolder == "SharedResources" && resourcePackage?["resourcePackage"]?["type"]?.ToString() == (isPBIR ? "SharedResources" : "2"))
                {
                    foreach (var item in resourcePackage?["resourcePackage"]?["items"] ?? new JArray())
                    {
                        string? themeFileName = item["path"]?.ToString();
                        if (themeFileName == null)
                            continue;

                        var itemFileExtension = Path.GetExtension(themeFileName);

                        if (itemFileExtension == ".json" && item["type"]?.ToString() == (isPBIR ? "BaseTheme" : "202"))
                        {
                            var themeJsonFilePath = Path.Combine(Path.Combine(themeResourceFolder, themeFolder), themeFileName);
                            themeJsonFilePath = themeJsonFilePath.Replace('/', Path.DirectorySeparatorChar);

                            string themeString = File.ReadAllText(themeJsonFilePath);
                            returnData["themeJson"] = themeString;
                            returnData["themeJsonFileName"] = themeJsonFilePath;
                        }
                    }
                }
                else
                {
                    foreach (var item in (isPBIR ? resourcePackage?["items"] : resourcePackage?["resourcePackage"]?["items"]) ?? new JArray())
                    {
                        string? themeFileName = item["path"]?.ToString();
                        if (themeFileName == null)
                            continue;
                        var itemFileExtension = Path.GetExtension(themeFileName);

                        if (itemFileExtension == ".json" && themeName != null && themeFileName.Contains(themeName))
                        {
                            var themeJsonFilePath = Path.Combine(Path.Combine(themeResourceFolder, themeFolder), themeFileName);
                            themeJsonFilePath = themeJsonFilePath.Replace('/', Path.DirectorySeparatorChar);
                            string themeString = File.ReadAllText(themeJsonFilePath);
                            returnData["themeJson"] = themeString;
                            returnData["themeJsonFileName"] = themeJsonFilePath;
                        }
                    }
                }
            }

            if (!returnData.ContainsKey("themeJson"))
            {
                returnData.Add("error", "No theme file is present in this report.");
            }

            return returnData;
        }

        public static List<Dictionary<string, List<string>>> GetPageWiseDrillThroughAndTooltipFilters(JToken layoutJson)
        {
            Dictionary<string, List<string>> drillThroughFilters = [];
            Dictionary<string, List<string>> tooltipFilters = [];
            if (layoutJson["pods"] == null)
            {
                return [drillThroughFilters, tooltipFilters];
            }
            JArray pods = (JArray)(layoutJson["pods"] ?? new JArray() { });

            foreach (var pod in pods)
            {
                if (pod["parameters"] != null)
                {
                    JArray paras = JArray.Parse((pod["parameters"] ?? new JArray() { }).ToString());
                    foreach (var par in paras)
                    {
                        string? pageId = pod["boundSection"]?.ToString();
                        string? type = pod["type"]?.ToString();
                        if (pageId == null)
                            continue;
                        if (type == "1")
                        {
                            if (drillThroughFilters.ContainsKey(pageId) == true)
                                drillThroughFilters[pageId].Add(par["boundFilter"]?.ToString()?.Trim('\'') ?? "");
                            else
                                drillThroughFilters.Add(pageId, [par["boundFilter"]?.ToString()?.Trim('\'') ?? ""]);
                        }
                        else if (type == "2")
                        {
                            if (tooltipFilters.ContainsKey(pageId) == true)
                                tooltipFilters[pageId].Add(par["boundFilter"]?.ToString()?.Trim('\'') ?? "");
                            else
                                tooltipFilters.Add(pageId, [par["boundFilter"]?.ToString()?.Trim('\'') ?? ""]);
                        }
                    }
                }
            }
            return [drillThroughFilters, tooltipFilters];
        }

        public static ReportUISettingsModel ReportUIDoc(JToken json, string? reportName, string? reportId, bool isPBIR = false)
        {
            ReportUISettingsModel reportUISettingsModel = new ReportUISettingsModel()
            {
                Theme = isPBIR ? json["themeCollection"]?["customTheme"]?["name"]?.ToString() : json["theme"]?.ToString(),
                ReportName = reportName,
                ReportId = reportId
            };

            try
            {
                JToken reportConfig = isPBIR ? json : JToken.Parse((json["config"] ?? new JObject() { }).ToString());
                if (reportConfig != null)
                {
                    bool isFilterPaneExpanded = (reportConfig?["objects"]?["outspacePane"]?[0]?["properties"]?["expanded"]?["expr"]?["Literal"]?["Value"]?.ToString().ToLower()) != "false";
                    bool isReportPersonalizationEnabled = reportConfig?["settings"]?["allowInlineExploration"]?.ToString().ToLower() == "true";
                    reportUISettingsModel.IsPersonalizationEnabled = isReportPersonalizationEnabled;
                    reportUISettingsModel.IsFilterPaneExpanded = isFilterPaneExpanded;
                }

                FilterHandler.clearDict();

                var filters = isPBIR ? JArray.Parse((json["filterConfig"]?["filters"] ?? "[]").ToString()) : JArray.Parse((json["filters"] ?? "[]").ToString());

                if (filters != null && filters.Count > 0)
                {
                    List<Dictionary<string, string>> reportFilterList = FilterHandler.simplify(filters, isPBIR, FilterFieldLocation.REPORT).filterList;
                    string reportFilterString = GlobalHandler.ConvertFilterListToStr(reportFilterList);
                    reportUISettingsModel.ReportFilters = reportFilterList;
                    reportUISettingsModel.ReportFiltersString = reportFilterString;
                }
            }
            catch (Exception ex)
            {
                GlobalHandler.WriteCrashLog(ex.ToString());
            }

            return reportUISettingsModel;
        }

        public static List<MeasureSummary> ReportUIExtendedMeasures(JToken json, string reportLayoutId, bool isPBIR = false)
        {
            List<MeasureSummary> measureDataList = [];
            JToken reportConfig = isPBIR ? json : JToken.Parse((json["config"] ?? new JObject() { }).ToString());

            try
            {
                if (reportConfig != null && reportConfig["modelExtensions"] != null)
                {
                    JArray modelExtensions = (JArray)(reportConfig["modelExtensions"] ?? new JArray() { });

                    string[] PowerBIReportDataTypeArray = ["", "Text", "Fixed decimal number", "Decimal number", "Whole number", "True/false", "Date", "Date/time", "", "Time", "", "Binary"];
                    foreach (var modelExtension in modelExtensions)
                    {
                        try
                        {
                            JArray modelExtensionEntities = (JArray)(modelExtension["entities"] ?? new JArray() { });

                            foreach (var modelExtensionEntity in modelExtensionEntities)
                            {
                                try
                                {
                                    string? modelExtensionEntityName = modelExtensionEntity["name"]?.ToString();
                                    JArray modelExtensionEntityMeasures = (JArray)(modelExtensionEntity["measures"] ?? new JArray() { });

                                    foreach (var modelExtensionEntityMeasure in modelExtensionEntityMeasures)
                                    {
                                        try
                                        {
                                            string? modelExtensionEntityMeasureName = modelExtensionEntityMeasure["name"]?.ToString();
                                            string? modelExtensionEntityMeasureDataType = modelExtensionEntityMeasure["dataType"]?.ToString();
                                            string? modelExtensionEntityMeasureExpression = modelExtensionEntityMeasure["expression"]?.ToString();
                                            string? modelExtensionEntityMeasureErrorMessage = modelExtensionEntityMeasure["errorMessage"]?.ToString();
                                            string? modelExtensionEntityMeasureIsHidden = modelExtensionEntityMeasure["hidden"]?.ToString();
                                            string? modelExtensionEntityMeasureFormulaOverride = modelExtensionEntityMeasure["formulaOverride"]?.ToString();
                                            string? modelExtensionEntityMeasureDisplayFolder = modelExtensionEntityMeasure["displayFolder"]?.ToString();
                                            string? modelExtensionEntityMeasureDescription = modelExtensionEntityMeasure["description"]?.ToString();

                                            JObject modelExtensionEntityMeasureFormatInformation = (JObject)(modelExtensionEntityMeasure["formatInformation"] ?? new JObject() { });

                                            string? modelExtensionEntityMeasureFormatString = isPBIR
                                                ? modelExtensionEntityMeasure["formatString"]?.ToString()
                                                : modelExtensionEntityMeasureFormatInformation["formatString"]?.ToString();

                                            modelExtensionEntityMeasureDataType = isPBIR
                                                ? modelExtensionEntityMeasureDataType
                                                : PowerBIReportDataTypeArray[Int32.Parse(modelExtensionEntityMeasureDataType ?? "0")];

                                            string[] fullPathParts = reportLayoutId.Split(['\\', '/'], StringSplitOptions.RemoveEmptyEntries);
                                            string reportLayoutFileName = fullPathParts[fullPathParts.Length - 1];
                                            string report_Name = reportLayoutFileName.Split('.')[0];

                                            measureDataList.Add(
                                                new MeasureSummary()
                                                {
                                                    TableName = modelExtensionEntityName,
                                                    MeasureName = modelExtensionEntityMeasureName,
                                                    DataType = modelExtensionEntityMeasureDataType,
                                                    Visible = modelExtensionEntityMeasureIsHidden == "False" ? "Yes" : "No",
                                                    Expression = modelExtensionEntityMeasureExpression,
                                                    Folder = modelExtensionEntityMeasureDisplayFolder,
                                                    Format = modelExtensionEntityMeasureFormatString,
                                                    Description = modelExtensionEntityMeasureDescription,
                                                    // BaseFields = "",
                                                    HasError =
                                                        modelExtensionEntityMeasure["errorMessage"] == null || modelExtensionEntityMeasureErrorMessage == ""
                                                            ? "No"
                                                            : "Yes (" + modelExtensionEntityMeasureErrorMessage + ")",
                                                    Origin = "Report UI",
                                                    ReportId = reportLayoutId,
                                                    ReportName = report_Name,
                                                }
                                            );
                                        }
                                        catch (Exception ex)
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
                        }
                        catch (Exception ex)
                        {
                            GlobalHandler.WriteCrashLog(ex.ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                GlobalHandler.WriteCrashLog(ex.ToString());
            }
            return measureDataList;
        }

        public static GetPageDetailReturnType? GetPageDetails(
            JToken page,
            Dictionary<string, List<string>> drillThroughPages,
            Dictionary<string, List<string>> tooltipFilterPages,
            bool isReportPersonalizationEnabled,
            string reportName,
            string reportId,
            bool isPBIR = false
        )
        {
            Dictionary<string, List<string>> disabledEditInteractions = [];

            int pageWidth = int.Parse((page["width"] ?? "0").ToString()),
                pageHeight = int.Parse((page["height"] ?? "0").ToString());
            string pageName = (page["displayName"] ?? "page").ToString();
            string pageId = page["name"]?.ToString() ?? "pageId";

            JToken pageConfig = isPBIR ? page : JToken.Parse(page["config"]?.ToString() ?? "{}");
            if (pageConfig == null)
                return null;

            bool isHiddenPage = isPBIR ? pageConfig["visibility"]?.ToString() == "HiddenInViewMode" : pageConfig["visibility"]?.ToString() == "1";

            // is page isPagePersonalizationEnabled
            bool isPagePersonalizationEnabled =
                isReportPersonalizationEnabled != true || (pageConfig["objects"]?["personalizeVisual"]?[0]?["properties"]?["show"]?["expr"]?["Literal"]?["Value"]?.ToString().ToLower()) != "false";

            string pbirPageType = pageConfig["PageBinding"]?["properties"]?["type"]?.ToString() ?? "";
            // tooltip page
            bool isToolTip = isPBIR ? pbirPageType == "Tooltip" : pageConfig["type"] != null && pageConfig["type"]?.ToString() == "1" && page["displayOption"]?.ToString() == "3";

            // page size inconsistency
            var pageSizeKey = pageWidth.ToString() + "X" + pageHeight.ToString();

            // disabled editIntegrations
            var pageRelationships = isPBIR ? pageConfig["visualInteractions"] : pageConfig["relationships"];
            if (pageRelationships != null)
            {
                foreach (var relationship in pageRelationships)
                {
                    try
                    {
                        var relSource = relationship["source"];
                        var relTarget = relationship["target"];
                        if (relationship == null || relSource == null || relTarget == null)
                            continue;
                        if ((isPBIR == false && relationship["type"]?.ToString() != "3") || (isPBIR && relationship["type"]?.ToString() == "NoFilter"))
                            continue;

                        if (disabledEditInteractions.ContainsKey(relSource.ToString()) == false)
                            disabledEditInteractions.Add(relSource.ToString(), []);

                        disabledEditInteractions[relSource.ToString()].Add(relTarget.ToString());
                    }
                    catch (Exception ex)
                    {
                        GlobalHandler.WriteCrashLog(ex.ToString());
                    }
                }
            }

            ///////////// page filter /////////////////////////////////
            List<Dictionary<string, string>> pageFilterList = [];
            List<Dictionary<string, string>> drillThroughFilterList = [];
            List<Dictionary<string, string>> toolTipFilterList = [];
            if (page["filters"] != null || page["filterConfig"]?["filters"] != null)
            {
                try
                {
                    JArray pageAllFilters = JArray.Parse(((isPBIR ? page["filterConfig"]?["filters"] : page["filters"]) ?? new JArray() { }).ToString());
                    List<string> drillThroughFilterNameList = [];
                    List<string> toolTipFiltersNameList = [];
                    if (isPBIR)
                    {
                        var pageBinding = page["pageBinding"] ?? new JObject();
                        string? pageBindingType = pageBinding["type"]?.ToString();
                        if (pageBindingType != null)
                        {
                            if (pageBindingType == "Drillthrough")
                            {
                                foreach (var bindingParam in pageBinding["parameters"] ?? new JArray())
                                {
                                    try
                                    {
                                        if (bindingParam["boundFilter"]?.ToString() != null)
                                            drillThroughFilterNameList.Add(bindingParam["boundFilter"]?.ToString() ?? "");
                                    }
                                    catch (Exception ex)
                                    {
                                        GlobalHandler.WriteCrashLog(ex.ToString());
                                    }
                                }
                            }
                            else if (pageBindingType == "Tooltip")
                            {
                                foreach (var bindingParam in pageBinding["parameters"] ?? new JArray())
                                {
                                    try
                                    {
                                        if (bindingParam["boundFilter"]?.ToString() != null)
                                            toolTipFiltersNameList.Add(bindingParam["boundFilter"]?.ToString() ?? "");
                                    }
                                    catch (Exception ex)
                                    {
                                        GlobalHandler.WriteCrashLog(ex.ToString());
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        if (drillThroughPages.ContainsKey(pageId) == true)
                            drillThroughFilterNameList = drillThroughPages[pageId];
                        if (tooltipFilterPages.ContainsKey(pageId) == true)
                            toolTipFiltersNameList = tooltipFilterPages[pageId];
                    }

                    JArray pageFilters = [];
                    JArray drillThroughFilters = [];
                    JArray toolTipFilters = [];
                    foreach (var filter in pageAllFilters)
                    {
                        try
                        {
                            var filterName = filter["name"];
                            if (filterName != null)
                            {
                                if (drillThroughFilterNameList.Contains(filterName.ToString()))
                                    drillThroughFilters.Add(filter);
                                else if (toolTipFiltersNameList.Contains(filterName.ToString()))
                                    toolTipFilters.Add(filter);
                                else
                                    pageFilters.Add(filter);
                            }
                            else
                                pageFilters.Add(filter);
                        }
                        catch (Exception ex)
                        {
                            GlobalHandler.WriteCrashLog(ex.ToString());
                        }
                    }
                    pageFilterList = FilterHandler.simplify(pageFilters, isPBIR, FilterFieldLocation.PAGE).filterList;
                    drillThroughFilterList = FilterHandler.simplify(drillThroughFilters, isPBIR, FilterFieldLocation.PAGE).filterList;
                    toolTipFilterList = FilterHandler.simplify(toolTipFilters, isPBIR, FilterFieldLocation.PAGE).filterList;
                }
                catch (Exception ex)
                {
                    GlobalHandler.WriteCrashLog(ex.ToString());
                }
            }

            PageSummary PageSummary = new()
            {
                ReportId = reportId,
                ReportName = reportName,
                PageId = pageId,
                PageName = pageName,
                Width = pageWidth,
                Height = pageHeight,
                Size = pageSizeKey,
                IsHidden = isHiddenPage,
                IsPersonalizationEnabled = isPagePersonalizationEnabled,
                PageType =
                    isToolTip ? "Tooltip Page"
                    : (isPBIR && pbirPageType == "Drillthrough") || drillThroughPages.ContainsKey(pageId) ? "Drillthrough Page"
                    : "Normal Page",
                PageField =
                    tooltipFilterPages.ContainsKey(pageId) ? "tooltip fields"
                    : drillThroughPages.ContainsKey(pageId) ? "Drill through fields"
                    : "",
                PageFilters = pageFilterList,
                PageFiltersString = GlobalHandler.ConvertFilterListToStr(pageFilterList),
                PageTypeFilter =
                    (toolTipFilterList.Count > 0 ? "Tooltip Filters: \n" + GlobalHandler.ConvertFilterListToStr(toolTipFilterList) : "")
                    + (drillThroughFilterList.Count > 0 ? "Drillthrough Filter: \n" + GlobalHandler.ConvertFilterListToStr(drillThroughFilterList) : ""),
            };
            GetPageDetailReturnType getPageDetailReturnType = new() { PageSummary = PageSummary, DisabledEditInteractions = disabledEditInteractions };
            return getPageDetailReturnType;
        }

        public static GetVisualDetailsReturnType? GetVisualDetails(
            JToken vis,
            Dictionary<string, List<string>> disabledEditInteractions,
            bool isReportPersonalized,
            string pageId,
            string pageName,
            string reportName,
            string reportId,
            JObject themeJson,
            bool isPBIR = false
        )
        {
            Dictionary<string, List<Dictionary<string, string>>> allUsedFontFamilies = [];

            JToken config = isPBIR ? vis : JToken.Parse(vis["config"]?.ToString() ?? "{}");

            if (config == null)
                return null;

            string visualId = config["name"]?.ToString() ?? "visualId";

            var visPos = isPBIR ? config["position"] : config["layouts"]?[0]?["position"];
            // if this vis is a visual group
            if (isPBIR ? config["visualGroup"] != null : config["singleVisualGroup"] != null)
            {
                return new GetVisualDetailsReturnType()
                {
                    IsGroup = true,
                    VisualGroup = new VisualGroup()
                    {
                        GroupId = visualId,
                        GroupName = isPBIR ? config["visualGroup"]?["displayName"]?.ToString() : config["singleVisualGroup"]?["displayName"]?.ToString(),
                        ParentGroupId = config["parentGroupName"]?.ToString(),
                        X = visPos?["x"]?.ToString() ?? "0",
                        Y = visPos?["y"]?.ToString() ?? "0",
                        Z = visPos?["z"]?.ToString() ?? "0",
                    },
                };
            }
            var configSingleVisual = isPBIR ? config["visual"] : config["singleVisual"];
            string vcObjectsKey = isPBIR ? "visualContainerObjects" : "vcObjects";
            string[] powerbiViz = AppConfig.PowerBIViz;
            string vizTitle = "";
            string x = visPos?["x"]?.ToString() ?? "0";
            string y = visPos?["y"]?.ToString() ?? "0";
            string z = visPos?["z"]?.ToString() ?? "0";
            string width = visPos?["width"]?.ToString() ?? "0";
            string height = visPos?["height"]?.ToString() ?? "0";
            string tabOrder = visPos?["tabOrder"]?.ToString() ?? "0";
            //Visual Calculation
            bool hasVisualCalculation = false;
            List<string> calculationExpression = [];
            List<Dictionary<string, string>> ValueAxisdata = new List<Dictionary<string, string>>();
            HashSet<Dictionary<string, string>> Measure_Column_Mapping = new HashSet<Dictionary<string, string>>();
            HashSet<VisualUsedFields> usedFields = [];

            string Fields = "";

            HashSet<string> measuresSet = new HashSet<string>();
            HashSet<string> columnsSet = new HashSet<string>();
            List<Tuple<string, string, string>> themeDataColors = new List<Tuple<string, string, string>>();
            List<Tuple<string, string>> hexColors = new List<Tuple<string, string>>();
            var DataColors = themeJson["dataColors"]?.ToObject<List<string>>() ?? new List<string>();

            string altText = configSingleVisual?[vcObjectsKey]?["general"]?[0]?["properties"]?["altText"]?["expr"]?["Literal"]?["Value"]?.ToString() ?? "";
            string? buttonText = "";
            List<VisualTextModel> visualTexts = [];

            string visType = GlobalHandler.FormatVisualType(configSingleVisual?["visualType"]?.ToString()) ?? "Custom Visual";
            string ogVisType = configSingleVisual?["visualType"]?.ToString() ?? "Custom Visual";
            bool isVisualPersonalizationEnabled =
                isReportPersonalized != true
                || (configSingleVisual?[vcObjectsKey]?["visualHeader"]?[0]?["properties"]?["showPersonalizeVisualButton"]?["expr"]?["Literal"]?["Value"]?.ToString()?.ToLower()) != "false";
            ;
            //configuring measure_column_mappings

            ///////////// visual filter /////////////////////////////////
            List<Dictionary<string, string>> visualFilterList = [];
            if (vis["filters"] != null)
            {
                JArray visualFilters = JArray.Parse((vis["filters"] ?? new JArray() { }).ToString());
                var visualFilterData = FilterHandler.simplify(visualFilters, isPBIR, FilterFieldLocation.VISUAL);
                visualFilterList = visualFilterData.filterList;
                var filtersFieldDataList = visualFilterData.FilterFieldList;

                foreach (var filter in filtersFieldDataList)
                {
                    try
                    {
                        usedFields.Add(new()
                        {
                            Type = (VisualUsedFieldType)filter.FieldType,
                            TableName = filter.TableName,
                            Hierarchy = filter.Hierarchy,
                            FieldName = filter.FieldName,
                            UsedIn = "filters",
                            FilterType = (FilterType)filter.FieldType,
                            FilterFieldLocation = (FilterFieldLocation)filter.FieldLocation
                        });
                        var filterDict = new Dictionary<string, string>
                                            {
                                                { "FieldName",filter.TableName +"."+ filter.FieldName ?? "NULL" },
                                                { "FieldType", filter.FieldType?.ToString() ?? "NULL" },
                                                { "FieldLocation", filter.FieldLocation?.ToString() ?? "NULL" },
                                                { "FilterTypeName", filter.FilterTypeName?.ToString() ?? "NULL" },
                                                { "usedin", "filters" },
                                            };

                        Measure_Column_Mapping.Add(filterDict);
                    }
                    catch (Exception ex)
                    {
                        GlobalHandler.WriteCrashLog(ex.ToString());
                    }
                }

                foreach (string meas in visualFilterData.measureDict.Keys)
                {
                    try
                    {
                        measuresSet.Add(meas);
                    }
                    catch (Exception ex)
                    {
                        GlobalHandler.WriteCrashLog(ex.ToString());
                    }
                }
                foreach (string cols in visualFilterData.columnDict.Keys)
                {
                    try
                    {
                        columnsSet.Add(cols);
                    }
                    catch (Exception ex)
                    {
                        GlobalHandler.WriteCrashLog(ex.ToString());
                    }
                }
            }

            //////////////////////////////////////////////////////////////////////////////////////////////////////
            ///         visual category format
            //////////////////////////////////////////////////////////////////////////////////////////////////////
            ///
            Dictionary<string, string> visualCatFormat = new Dictionary<string, string>
            {
                { "pageId", pageId },
                { "pageName", pageName },
                { "visualId", visualId },
                { "visualType", visType },
            };

            VisualFormatter visualFormatter = new VisualFormatter();
            Dictionary<string, HashSet<string>> visualFormatType = AppConfig.visualFormatType;

            bool isHidden = isPBIR ? config["isHidden"]?.ToString() == true.ToString() : configSingleVisual?["display"]?["mode"]?.ToString() == "hidden";

            //Title Config
            try
            {
                if (configSingleVisual?[vcObjectsKey]?["title"]?[0]?["properties"]?["show"]?["expr"]?["Literal"]?["Value"]?.ToString()?.ToLower() == "false")
                    vizTitle = "";
                else
                    vizTitle = configSingleVisual?[vcObjectsKey]?["title"]?[0]?["properties"]?["text"]?["expr"]?["Literal"]?["Value"]?.ToString()?.Trim('\'') ?? "";

                if (vizTitle == null || vizTitle?.Length == 0)
                {
                    if (configSingleVisual?[vcObjectsKey]?["title"]?[0]?["properties"]?["show"]?["expr"]?["Literal"]?["Value"]?.ToString() != "false")
                    {
                        if (
                            ogVisType != null
                            && (
                                new List<string>()
                                {
                                    "tableEx",
                                    "pivottable",
                                    "card",
                                    "cardvisual",
                                    "decompositiontreevisual",
                                    "qnavisual",
                                    "multirowcard",
                                    "slicer",
                                    "image",
                                    "shape",
                                    "actionbutton",
                                    "textbox",
                                    "basicshape",
                                }.Contains(ogVisType.ToLower()) || ogVisType.ToLower().Contains("button")
                            )
                        )
                            vizTitle = "";
                        else
                            vizTitle = "Autogenerated Visual Title";
                    }
                    else
                        vizTitle = "";
                }

                if (ogVisType?.ToLower() == "slicer" && configSingleVisual?["objects"]?["header"]?[0]?["properties"]?["show"]?["expr"]?["Literal"]?["Value"]?.ToString()?.ToLower() != "false")
                {
                    vizTitle = configSingleVisual?["objects"]?["header"]?[0]?["properties"]?["text"]?["expr"]?["Literal"]?["Value"]?.ToString() ?? "Autogenerated Slicer Header";
                }
            }
            catch (Exception ex)
            {
                GlobalHandler.WriteCrashLog(ex.ToString());
            }

            try
            {
                //checking Axis titles
                if (configSingleVisual?["objects"]?["categoryAxis"]?[0]?["properties"]?["titleText"] != null)
                {
                    string categoryAxisTitle = configSingleVisual?["objects"]?["categoryAxis"]?[0]?["properties"]?["titleText"]?["expr"]?["Literal"]?["Value"]?.ToString() ?? "";
                    visualTexts.Add(new VisualTextModel() { Text = categoryAxisTitle, Type = "Category Axis Title" });
                }

                if (configSingleVisual?["objects"]?["valueAxis"]?[0]?["properties"]?["titleText"] != null)
                {
                    string valueAxisTitle = configSingleVisual?["objects"]?["valueAxis"]?[0]?["properties"]?["titleText"]?["expr"]?["Literal"]?["Value"]?.ToString() ?? "";
                    visualTexts.Add(new VisualTextModel() { Text = valueAxisTitle, Type = "Value Axis Title" });
                }
            }
            catch (Exception ex)
            {
                GlobalHandler.WriteCrashLog(ex.ToString());
            }

            //checking textbox textRuns
            try
            {
                foreach (var textRun in configSingleVisual?["objects"]?["general"]?[0]?["properties"]?["paragraphs"]?[0]?["textRuns"] ?? new JArray())
                {
                    try
                    {
                        string? text = textRun["value"]?.ToString();
                        if (text != null)
                            visualTexts.Add(new VisualTextModel() { Text = text, Type = "Text" });
                    }
                    catch (Exception ex)
                    {
                        GlobalHandler.WriteCrashLog(ex.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                GlobalHandler.WriteCrashLog(ex.ToString());
            }

            // check for measures or column that this textbox is using
            try
            {
                if (configSingleVisual?["objects"]?["values"] != null && configSingleVisual?["objects"]?["values"]?[0] != null)
                {
                    JArray? dynamicTexts = (JArray)(configSingleVisual?["objects"]?["values"] ?? new JArray() { });
                    foreach (var dynamicText in dynamicTexts ?? [])
                    {
                        try
                        {
                            var expression = (dynamicText["properties"]?["expr"]?["expr"]) ?? (dynamicText["properties"]?["expr"]);
                            if (expression != null)
                            {
                                expression = expression["Min"]?["Expression"] ?? expression["Aggregation"]?["Expression"] ?? expression["Max"]?["Expression"];
                                if (expression != null)
                                {
                                    // Check for Column Property
                                    var columnProperty = expression["Column"]?["Property"]?.ToString();
                                    if (!string.IsNullOrEmpty(columnProperty)) // Ensure it's not null or empty
                                    {
                                        if (columnsSet.Count > 0)
                                            columnsSet.Add(", ");
                                        columnsSet.Add(columnProperty);
                                    }

                                    // Check for Measure Property
                                    var measureProperty = expression["Measure"]?["Property"]?.ToString();
                                    if (!string.IsNullOrEmpty(measureProperty)) // Ensure it's not null or empty
                                    {
                                        if (measuresSet.Count > 0)
                                            measuresSet.Add(", ");
                                        measuresSet.Add(measureProperty);
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            GlobalHandler.WriteCrashLog(ex.ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                GlobalHandler.WriteCrashLog(ex.ToString());
            }

            try
            {
                //check actionButton text
                foreach (var propertyChild in configSingleVisual?["objects"]?["text"] ?? new JArray() { })
                {
                    try
                    {
                        string? text = propertyChild?["properties"]?["text"]?["expr"]?["Literal"]?["Value"]?.ToString();
                        if (text != null)
                        {
                            visualTexts.Add(new VisualTextModel() { Text = text, Type = "Action Button" });
                            if (ogVisType == "actionButton")
                                buttonText = text;
                        }
                    }
                    catch (Exception ex)
                    {
                        GlobalHandler.WriteCrashLog(ex.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                GlobalHandler.WriteCrashLog(ex.ToString());
            }

            bool isUsingFieldParameter = false;
            HashSet<string> fieldParameters = [];
            HashSet<string> columnsInfieldParameters = [];
            HashSet<string> measuresInfieldParameters = [];

            if (vis.SelectToken("dataTransforms") != null)
            {
                JToken dataTransforms = JToken.Parse((vis["dataTransforms"] ?? new JObject()).ToString());

                if (dataTransforms.SelectToken("selects") != null)
                {
                    foreach (var field in dataTransforms["selects"] ?? new JArray() { })
                    {
                        try
                        {
                            if (field.SelectToken("displayName") != null)
                            {
                                string fieldType = "";
                                string? text = field["displayName"]?.ToString() ?? "";
                                if (field["expr"]?.SelectToken("Column") != null)
                                    fieldType = "Column Field";
                                if (field["expr"]?.SelectToken("Row") != null)
                                    fieldType = "Row Field";
                                if (field["expr"]?.SelectToken("Measure") != null)
                                    fieldType = "Measure Field";

                                if (fieldType != "" && text.Length > 0)
                                {
                                    visualTexts.Add(new VisualTextModel() { Text = text, Type = fieldType });
                                }
                            }
                            if (field.SelectToken("queryName") != null)
                            {
                                if (field.SelectToken("sourceFieldParameters") != null)
                                {
                                    isUsingFieldParameter = true;
                                    string tableName = "";
                                    string columnName = "";
                                    foreach (var fp in field.SelectToken("sourceFieldParameters") ?? new JArray() { })
                                    {
                                        try
                                        {
                                            columnName = (fp?.SelectToken("expr")?.SelectToken("ref") ?? string.Empty).ToString().Trim();
                                            tableName = (fp?.SelectToken("expr")?.SelectToken("source")?.SelectToken("entity") ?? string.Empty).ToString().Trim();
                                            fieldParameters.Add(tableName + "." + columnName);
                                        }
                                        catch (Exception ex)
                                        {
                                            GlobalHandler.WriteCrashLog(ex.ToString());
                                        }
                                    }
                                    if (field.SelectToken("expr")?.SelectToken("Column") != null)
                                    {
                                        columnsInfieldParameters.Add((field.SelectToken("queryName") ?? string.Empty).ToString().Trim());
                                    }
                                    else if (field.SelectToken("expr")?.SelectToken("Measure") != null)
                                    {
                                        measuresInfieldParameters.Add((field.SelectToken("expr")?.SelectToken("Measure")?.SelectToken("Property") ?? string.Empty).ToString().Trim());
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            GlobalHandler.WriteCrashLog(ex.ToString());
                        }
                    }
                }
            }

            var visualUsedColors = new Dictionary<string, List<Dictionary<string, string>>>();

            void AddExtractedColor(string hexCode, string parentCategory, string location)
            {
                hexCode = hexCode.Trim('\'');
                if (!visualUsedColors.ContainsKey(hexCode))
                {
                    visualUsedColors[hexCode] = [];
                }

                visualUsedColors[hexCode].Add(new() { { "parentCategory", parentCategory }, { "location", location } });
            }

            VisualUsedFields ExtractUsedFieldInPBIR(JToken fieldExp, string usedIn)
            {
                var type = fieldExp["Column"] != null || fieldExp["Aggregation"]?["Expression"]?["Column"] != null ? VisualUsedFieldType.COLUMN :
                            fieldExp["Measure"] != null || fieldExp["Aggregation"]?["Expression"]?["Measure"] != null ? VisualUsedFieldType.MEASURE :
                            fieldExp["HierarchyLevel"] != null ? VisualUsedFieldType.HIERARCHY :
                             VisualUsedFieldType.UNKNOWN;
                var typeExp = type == VisualUsedFieldType.COLUMN ? fieldExp["Aggregation"]?["Expression"]?["Column"] ?? fieldExp["Column"] :
                              type == VisualUsedFieldType.MEASURE ? fieldExp["Aggregation"]?["Expression"]?["Measure"] ?? fieldExp["Measure"] :
                                type == VisualUsedFieldType.HIERARCHY ? fieldExp["HierarchyLevel"] : new JObject();

                string? tableName = type == VisualUsedFieldType.HIERARCHY ? typeExp?["Expression"]?["Hierarchy"]?["Expression"]?["SourceRef"]?["Entity"]?.ToString() : typeExp?["Expression"]?["SourceRef"]?["Entity"]?.ToString();
                string? fieldName = type == VisualUsedFieldType.HIERARCHY ? typeExp?["Expression"]?["Hierarchy"]?["Hierarchy"]?.ToString() : typeExp?["Property"]?.ToString();
                string? hierarchyLevel = type == VisualUsedFieldType.HIERARCHY ? typeExp?["Level"]?.ToString() : null;

                return new()
                {
                    UsedIn = usedIn,
                    Type = type,
                    TableName = tableName ?? "",
                    Hierarchy = hierarchyLevel ?? "",
                    FieldName = fieldName ?? "",
                };
            }

            if (ogVisType != null)
            {
                // fetch used fields 
                if (isPBIR)
                {
                    JObject queryStates = JObject.Parse(configSingleVisual?["query"]?["queryState"]?.ToString() ?? "{}");

                    foreach (var qState in queryStates)
                    {
                        var usedIn = qState.Key;
                        var expressions = qState.Value?["projections"] ?? new JArray();
                        foreach (var expression in expressions)
                        {
                            try
                            {
                                var fieldExp = expression["field"];
                                usedFields.Add(ExtractUsedFieldInPBIR(fieldExp ?? new JObject(), usedIn));
                            }
                            catch (Exception ex)
                            {
                                GlobalHandler.WriteCrashLog(ex.ToString());
                            }
                        }
                        var fieldParaExpressions = qState.Value?["fieldParameters"] ?? new JArray();
                        foreach (var expression in fieldParaExpressions)
                        {
                            try
                            {
                                var fieldExp = expression["parameterExpr"];
                                var vf = ExtractUsedFieldInPBIR(fieldExp ?? new JObject(), usedIn);
                                vf.IsFieldParameterField = true;
                                usedFields.Add(vf);
                            }
                            catch (Exception ex)
                            {
                                GlobalHandler.WriteCrashLog(ex.ToString());
                            }
                        }
                    }
                }
                else
                {
                    var projections = configSingleVisual?["projections"] as JObject;
                    var prototypeQuery = configSingleVisual?["prototypeQuery"] as JObject;
                    var selectItems = prototypeQuery?["Select"] as JArray;
                    var fromItems = prototypeQuery?["From"] as JArray;

                    if (projections != null && selectItems != null && fromItems != null)
                    {
                        // Build lookup for select items and alias-to-entity mapping
                        var selectDict = selectItems
                                                    .OfType<JObject>()
                                                    .Where(x => x["Name"] != null) // Ensure "Name" is not null
                                                    .ToDictionary(
                                                        x => x["Name"]!.ToString(), // Use null-forgiving operator (!) since "Name" is checked for null
                                                        x => x
                                                    );

                        var aliasToEntity = fromItems
                                                     .OfType<JObject>()
                                                    .Where(x => x["Name"] != null && x["Entity"] != null) // Ensure "Name" and "Entity" is not null
                                                    .ToDictionary(
                                                        x => x["Name"]!.ToString(), // Use null-forgiving operator (!) since "Name" is checked for null
                                                        x => x["Entity"]!.ToString()
                                                    );
                        //.OfType<JObject>()
                        //.ToDictionary(x => (string)x["Name"], x => (string)x["Entity"]);

                        // Iterate over Y, Tooltips, Category, etc.
                        foreach (var projectionProp in projections.Properties())
                        {
                            var usedIn = projectionProp.Name;
                            var projectionArray = projectionProp.Value as JArray;
                            if (projectionArray == null) continue;

                            foreach (var item in projectionArray.OfType<JObject>())
                            {
                                var queryRef = item["queryRef"]?.ToString() ?? "";
                                if (string.IsNullOrEmpty(queryRef)) continue;

                                if (!selectDict.TryGetValue(queryRef, out var fieldExp))
                                    continue;

                                var type = fieldExp["Column"] != null || fieldExp["Aggregation"]?["Expression"]?["Column"] != null ? VisualUsedFieldType.COLUMN :
                                            fieldExp["Measure"] != null || fieldExp["Aggregation"]?["Expression"]?["Measure"] != null ? VisualUsedFieldType.MEASURE :
                                            fieldExp["HierarchyLevel"] != null ? VisualUsedFieldType.HIERARCHY :
                                             VisualUsedFieldType.UNKNOWN;
                                var typeExp = type == VisualUsedFieldType.COLUMN ? fieldExp["Aggregation"]?["Expression"]?["Column"] ?? fieldExp["Column"] :
                                              type == VisualUsedFieldType.MEASURE ? fieldExp["Aggregation"]?["Expression"]?["Measure"] ?? fieldExp["Measure"] :
                                                type == VisualUsedFieldType.HIERARCHY ? fieldExp["HierarchyLevel"] : new JObject();

                                string? tableNameSource = type == VisualUsedFieldType.HIERARCHY ? typeExp?["Expression"]?["Hierarchy"]?["Expression"]?["SourceRef"]?["Source"]?.ToString() : typeExp?["Expression"]?["SourceRef"]?["Source"]?.ToString();
                                string? fieldName = type == VisualUsedFieldType.HIERARCHY ? typeExp?["Expression"]?["Hierarchy"]?["Hierarchy"]?.ToString() : typeExp?["Property"]?.ToString();
                                string? hierarchyLevel = type == VisualUsedFieldType.HIERARCHY ? typeExp?["Level"]?.ToString() : null;

                                var field = new VisualUsedFields()
                                {
                                    UsedIn = usedIn,
                                    Type = type,
                                    TableName = aliasToEntity != null && aliasToEntity.ContainsKey(tableNameSource ?? "") == true ?
                                                aliasToEntity[tableNameSource ?? ""] : tableNameSource ?? "",
                                    Hierarchy = hierarchyLevel ?? "",
                                    FieldName = fieldName ?? "",
                                    IsFieldParameterField = false
                                };
                                usedFields.Add(field);
                            }
                        }

                    }


                }



                try
                {
                    JObject jsonObject = JObject.Parse((isPBIR ? configSingleVisual?["query"]?["queryState"] : configSingleVisual?["prototypeQuery"])?.ToString() ?? "{}");

                    // Extract entity mapping from "From"
                    Dictionary<string, string> entityMap = new Dictionary<string, string>();
                    foreach (var fromItem in jsonObject["From"] ?? new JArray())
                    {
                        try
                        {
                            string? name = fromItem["Name"]?.ToString();
                            string? entity = fromItem["Entity"]?.ToString();
                            if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(entity))
                            {
                                entityMap[name] = entity;
                            }
                        }
                        catch (Exception ex)
                        {
                            GlobalHandler.WriteCrashLog(ex.ToString());
                        }
                    }

                    // Process "Select" fields
                    foreach (var selectItem in jsonObject["Select"] ?? new JArray())
                    {
                        try
                        {
                            string? name = string.Empty;
                            string? type = string.Empty;

                            var measureExpr = selectItem["Measure"];
                            if (measureExpr != null)
                            {
                                name = measureExpr["Property"]?.ToString();
                                type = "measure";
                            }
                            else
                            {
                                var aggregationExpr = selectItem["Aggregation"]?["Expression"]?["Column"];
                                if (aggregationExpr != null)
                                {
                                    string? source = aggregationExpr["Expression"]?["SourceRef"]?["Source"]?.ToString();
                                    string? property = aggregationExpr["Property"]?.ToString();

                                    if (!string.IsNullOrEmpty(source) && !string.IsNullOrEmpty(property) && entityMap.ContainsKey(source))
                                    {
                                        name = entityMap[source] + "." + property;
                                        type = "column";
                                    }
                                }
                            }

                            if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(type))
                            {
                                ValueAxisdata.Add(new Dictionary<string, string> { { "name", name }, { "type", type } });
                            }
                        }
                        catch (Exception ex)
                        {
                            GlobalHandler.WriteCrashLog(ex.ToString());
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex.Message);
                }

                try
                {
                    JObject jsonObject = JObject.Parse((isPBIR ? configSingleVisual?["query"]?["queryState"] : configSingleVisual?["prototypeQuery"])?.ToString() ?? "{}");

                    // Extract entity mapping from "From"
                    Dictionary<string, string> entityMap = new Dictionary<string, string>();
                    foreach (var fromItem in jsonObject["From"] ?? new JArray())
                    {
                        try
                        {
                            string? name = fromItem["Name"]?.ToString();
                            string? entity = fromItem["Entity"]?.ToString();
                            if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(entity))
                            {
                                entityMap[name] = entity;
                            }
                        }
                        catch (Exception ex)
                        {
                            GlobalHandler.WriteCrashLog(ex.ToString());
                        }
                    }

                    // Extract column/measure mapping from "Select"
                    Dictionary<string, string> selectTypeMap = new Dictionary<string, string>();
                    Dictionary<string, string> nativeReferenceMap = new Dictionary<string, string>();
                    // foreach (var selectItem in jsonObject["Select"] ?? new JArray())
                    // {
                    //     string ?name = selectItem["Name"]?.ToString();
                    //     string ?type = "";
                    //     string ?nativeReference = selectItem["NativeReferenceName"]?.ToString() ?? name;

                    //     if (selectItem["Measure"] != null)
                    //     {
                    //         type = "measure";
                    //     }
                    //     else if (selectItem["Column"] != null || selectItem["Aggregation"] != null )
                    //     {
                    //         type = "column";
                    //     }
                    //     else if (selectItem["HierarchyLevel"] != null)
                    //             {
                    //                 type = "hierarchy";
                    //             }

                    //     if (!string.IsNullOrEmpty(name))
                    //     {
                    //         selectTypeMap[name] = type;
                    //         nativeReferenceMap[name] = nativeReference;
                    //     }
                    // }
                    foreach (var selectItem in jsonObject["Select"] ?? new JArray())
                    {
                        try
                        {
                            string? name = selectItem["Name"]?.ToString();
                            string? type = "";
                            string? nativeReference = selectItem["NativeReferenceName"]?.ToString() ?? name;

                            // Use a stack for non-recursive traversal
                            Stack<JToken> stack = new Stack<JToken>();
                            stack.Push(selectItem);
                            while (stack.Count > 0)
                            {
                                var token = stack.Pop();
                                if (token.Type == JTokenType.Object)
                                {
                                    var obj = (JObject)token;
                                    // Check if any relevant key is found
                                    if (obj.ContainsKey("Measure"))
                                    {
                                        type = "measure";
                                        break;
                                    }
                                    if (obj.ContainsKey("HierarchyLevel"))
                                    {
                                        type = "hierarchy";
                                        break;
                                    }
                                    if (obj.ContainsKey("Column"))
                                    {
                                        type = "column";
                                        break;
                                    }

                                    // Push all properties onto the stack for further processing
                                    foreach (var property in obj.Properties())
                                    {
                                        try
                                        {
                                            stack.Push(property.Value);
                                        }
                                        catch (Exception ex)
                                        {
                                            GlobalHandler.WriteCrashLog(ex.ToString());
                                        }
                                    }
                                }
                                else if (token.Type == JTokenType.Array)
                                {
                                    // Push all elements of the array onto the stack
                                    foreach (var item in token.Children())
                                    {
                                        try
                                        {
                                            stack.Push(item);
                                        }
                                        catch (Exception ex)
                                        {
                                            GlobalHandler.WriteCrashLog(ex.ToString());
                                        }
                                    }
                                }
                            }

                            // Store result
                            if (name != null) selectTypeMap[name] = type;
                            if (name != null) nativeReferenceMap[name] = nativeReference ?? "";
                        }
                        catch (Exception ex)
                        {
                            GlobalHandler.WriteCrashLog(ex.ToString());
                        }
                    }

                    // Process "Projections"
                    JObject projections = (JObject)(configSingleVisual?["projections"] ?? new JObject());
                    Dictionary<string, string> projectionMap = new Dictionary<string, string>
                    {
                        { "Category", "category axis" },
                        { "Y", "value axis" },
                        { "Y2", "value axis" },
                        { "X", "value axis" },
                        { "Tooltips", "tooltip" },
                        { "Series", "series" },
                        { "Rows", "row" },
                        { "Breakdown", "breakdown" },
                        { "Size", "size" },
                        { "Play", "play axis" },
                        { "MinValue", "MinValue" },
                        { "TargetValue", "TargetValue" },
                        { "MaxValue", "MaxValue" },
                        { "Values", "values" },
                        { "TrendLine", "TrendLine" },
                        { "Goal", "Goal" },
                        { "Indicator", "Indicator" },
                        { "Columns", "Columns" },
                        { "Details", "Details" },
                        { "ExplainBy", "ExplainBy" },
                        { "Target", "Target" },
                        { "Analyze", "Analyze" },
                        { "Data", "Data" },
                    };

                    foreach (var projection in projections)
                    {
                        try
                        {
                            if (projectionMap.ContainsKey(projection.Key)) // Process relevant keys
                            {
                                string usedIn = projectionMap[projection.Key];
                                foreach (var item in projection.Value ?? new JArray())
                                {
                                    try
                                    {
                                        string? queryRef = item["queryRef"]?.ToString();
                                        if (!string.IsNullOrEmpty(queryRef))
                                        {
                                            string type = selectTypeMap.ContainsKey(queryRef) ? selectTypeMap[queryRef] : "";
                                            string name = (type == "measure" && nativeReferenceMap.ContainsKey(queryRef)) ? nativeReferenceMap[queryRef] : queryRef;

                                            // Constraint: If name follows "Min(Orders.Customer Name)", keep only "Orders.Customer Name"
                                            // Constraint: If name follows "SomeFunction(Orders.Customer Name)", keep only "Orders.Customer Name"
                                            if (
                                                name.StartsWith("Sum", StringComparison.OrdinalIgnoreCase)
                                                || name.StartsWith("CountNonNull", StringComparison.OrdinalIgnoreCase)
                                                || name.StartsWith("Count", StringComparison.OrdinalIgnoreCase)
                                                || name.StartsWith("Median", StringComparison.OrdinalIgnoreCase)
                                                || name.StartsWith("Variance", StringComparison.OrdinalIgnoreCase)
                                                || name.StartsWith("StandardDeviation", StringComparison.OrdinalIgnoreCase)
                                                || name.StartsWith("Max", StringComparison.OrdinalIgnoreCase)
                                                || name.StartsWith("Min", StringComparison.OrdinalIgnoreCase)
                                            )
                                            {
                                                int openParenIndex = name.IndexOf("(");
                                                int closeParenIndex = name.LastIndexOf(")");
                                                if (openParenIndex != -1 && closeParenIndex != -1 && openParenIndex < closeParenIndex)
                                                {
                                                    name = name.Substring(openParenIndex + 1, closeParenIndex - openParenIndex - 1);
                                                }
                                            }

                                            Measure_Column_Mapping.Add(
                                                new Dictionary<string, string>
                                                {
                                                    { "name", name },
                                                    { "type", type },
                                                    { "usedin", usedIn },
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
                        catch (Exception ex)
                        {
                            GlobalHandler.WriteCrashLog(ex.ToString());
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex.Message);
                }



                //TODO:  code to extract color code form Layout.json is removed from here 


                //extracting value axis data
            }

            //if (Measure_Column_Mapping != null && Measure_Column_Mapping.Count > 0)
            //{


            //    var mappings = Measure_Column_Mapping;

            //    // Separate filters and non-filters
            //    var nonFilterUsedIns = mappings
            //        .Where(m => m.ContainsKey("usedin") && !m["usedin"].Equals("filters", StringComparison.OrdinalIgnoreCase))
            //        .Select(m => m["usedin"].ToLower())
            //        .Distinct();

            //    var fieldSummaries = new List<string>();

            //    // Process non-filter categories first
            //    foreach (var category in nonFilterUsedIns)
            //    {
            //        var fieldsInCategory = mappings
            //            .Where(m => m.ContainsKey("usedin") && m["usedin"].Equals(category, StringComparison.OrdinalIgnoreCase))
            //            .Select(m =>
            //            {
            //                string type = m.ContainsKey("type") ? m["type"].ToLower() : "unknown";
            //                string name = m.ContainsKey("name") ? m["name"] : "Unnamed";
            //                return $"{type} - {name}";
            //            });

            //        string summary = string.Join(", ", fieldsInCategory);
            //        string titleCaseCategory = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(category);
            //        fieldSummaries.Add($"{titleCaseCategory}: {summary}");
            //    }

            //    // Now handle "filters" separately at the end
            //    var filterFields = mappings
            //        .Where(m => m.ContainsKey("usedin") && m["usedin"].Equals("filters", StringComparison.OrdinalIgnoreCase))
            //        .Select(m =>
            //        {
            //            string type = m.ContainsKey("FieldType") ? m["FieldType"].ToLower() : "unknown";
            //            string name = m.ContainsKey("FieldName") ? m["FieldName"] : "Unnamed";
            //            return $"{type} - {name}";
            //        });

            //    if (filterFields.Any())
            //    {
            //        string filterSummary = string.Join(", ", filterFields);
            //        fieldSummaries.Add($"Filters: {filterSummary}");
            //    }

            //    Fields = string.Join(Environment.NewLine, fieldSummaries);
            //}


            if (usedFields != null && usedFields.Count > 0)
            {


                var mappings = usedFields;

                // Separate filters and non-filters
                var nonFilterUsedIns = mappings
                    .Where(m => m.UsedIn.Equals("filters", StringComparison.OrdinalIgnoreCase))
                    .Select(m => m.UsedIn.ToLower())
                    .Distinct();

                var fieldSummaries = new List<string>();

                // Process non-filter categories first
                foreach (var category in nonFilterUsedIns)
                {
                    var fieldsInCategory = mappings
                        .Where(m => m.UsedIn.Equals(category, StringComparison.OrdinalIgnoreCase))
                        .Select(m =>
                        {
                            string type = m.Type.ToString();
                            string name = m.Type == VisualUsedFieldType.HIERARCHY ? $@"{m.TableName}.{m.Hierarchy}.{m.FieldName}" : $@"{m.TableName}.{m.FieldName}";
                            return $"{type} - {name}";
                        });

                    string summary = string.Join(", ", fieldsInCategory);
                    string titleCaseCategory = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(category);
                    fieldSummaries.Add($"{titleCaseCategory}: {summary}");
                }

                // Now handle "filters" separately at the end
                var filterFields = mappings
                    .Where(m => m.UsedIn.Equals("filters", StringComparison.OrdinalIgnoreCase))
                    .Select(m =>
                    {
                        string type = m.Type.ToString();
                        string name = m.Type == VisualUsedFieldType.HIERARCHY ? $@"{m.TableName}.{m.Hierarchy}.{m.FieldName}" : $@"{m.TableName}.{m.FieldName}";
                        return $"{type} - {name}";
                    });

                if (filterFields.Any())
                {
                    string filterSummary = string.Join(", ", filterFields);
                    fieldSummaries.Add($"Filters: {filterSummary}");
                }

                Fields = string.Join(Environment.NewLine, fieldSummaries);
            }



            // find all columns and measures used in visual query
            JToken visualQueries = JObject.Parse((isPBIR ? configSingleVisual?["query"]?["queryState"] : configSingleVisual?["prototypeQuery"])?.ToString() ?? "{}");

            // Initialize the collections for projection and field parameters
            JArray powerQueryProjectionSelects = isPBIR == false ? JArray.Parse((visualQueries?["Select"] ?? new JArray()).ToString()) : new JArray();
            JArray powerQueryFieldParameters = new JArray();

            // If we are processing PBIR and visualQueries is not null
            if (visualQueries != null)
            {
                foreach (JProperty visualQuery in visualQueries)
                {
                    try
                    {
                        // Process Projections
                        JArray visualQueryProjection = (JArray)(visualQuery.Value["projections"] ?? new JArray());
                        if (visualQueryProjection != null)
                        {
                            foreach (var vqp in visualQueryProjection)
                            {
                                try
                                {
                                    powerQueryProjectionSelects.Add(vqp);

                                    // Get Visual Calculation for PBIR
                                    if (vqp["field"]?["NativeVisualCalculation"] != null)
                                    {
                                        hasVisualCalculation = true;
                                        string calExpr = vqp["field"]?["NativeVisualCalculation"]?["Name"]?.ToString() + " = " + vqp["field"]?["NativeVisualCalculation"]?["Expression"]?.ToString();
                                        calculationExpression.Add(calExpr);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    GlobalHandler.WriteCrashLog(ex.ToString());
                                }
                            }
                        }

                        // Process Field Parameters
                        if (visualQuery.Value["fieldParameters"] != null)
                        {
                            JArray visualQueryFieldParameters = (JArray)(visualQuery.Value["fieldParameters"] ?? new JArray());
                            foreach (var visualQueryFieldParameter in visualQueryFieldParameters)
                            {
                                try
                                {
                                    powerQueryFieldParameters.Add(visualQueryFieldParameter);
                                }
                                catch (Exception ex)
                                {
                                    GlobalHandler.WriteCrashLog(ex.ToString());
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        GlobalHandler.WriteCrashLog($"Error processing visualQuery {visualQuery.Name}: {ex.Message}");
                        GlobalHandler.WriteCrashLog(ex.ToString());
                    }
                }
            }

            // Iterate through the projections
            for (int k = 0; k < powerQueryProjectionSelects.Count; k++)
            {
                try
                {
                    // Depending on PBIR flag, select the appropriate powerQuery object
                    var powerQuery = isPBIR ? powerQueryProjectionSelects[k]["field"] : powerQueryProjectionSelects[k];
                    if (powerQuery != null)
                    {
                        // Handle HierarchyLevel if it exists
                        if (powerQuery["HierarchyLevel"]?["Level"] != null)
                        {
                            var tName = "";
                            var cName = "";
                            // Process Hierarchy Source
                            var hierarchySource = powerQuery["HierarchyLevel"]?["Expression"]?["Hierarchy"]?["Expression"]?["SourceRef"]?["Source"]?.ToString();
                            if (hierarchySource != null)
                            {
                                for (int i = 0; i < visualQueries?["From"]?.Count(); i++)
                                {
                                    try
                                    {
                                        if (visualQueries?["From"]?[i]?["Name"]?.ToString() == hierarchySource)
                                        {
                                            tName = visualQueries?["From"]?[i]?["Entity"]?.ToString() + ".";
                                            // columnsSet.Add(visualQueries?["From"]?[i]?["Entity"]?.ToString() + "." );
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        GlobalHandler.WriteCrashLog(ex.ToString());
                                    }
                                }
                            }
                            else
                            {
                                // If no hierarchySource, process Entity and Hierarchy
                                var entity = powerQuery["HierarchyLevel"]?["Expression"]?["Hierarchy"]?["Expression"]?["SourceRef"]?["Entity"]?.ToString();
                                if (!string.IsNullOrEmpty(entity))
                                {
                                    tName = entity + ".";
                                    // columnsSet.Add(entity + ".");
                                }

                                var hierarchy = powerQuery["HierarchyLevel"]?["Expression"]?["Hierarchy"]?["Hierarchy"]?.ToString();
                                if (!string.IsNullOrEmpty(hierarchy))
                                {
                                    tName = hierarchy + ".";
                                    // columnsSet.Add(hierarchy + ".");
                                }
                            }

                            cName = powerQuery["HierarchyLevel"]?["Level"]?.ToString();
                            if (!string.IsNullOrEmpty(cName) && !string.IsNullOrEmpty(tName))
                            {
                                columnsSet.Add(tName + cName);
                            }
                        }

                        // Process Measure Property
                        var measureProperty = powerQuery["Measure"]?["Property"]?.ToString();
                        if (!string.IsNullOrEmpty(measureProperty))
                        {
                            measuresSet.Add(measureProperty);
                        }

                        // Process Column Source
                        var columnSource = powerQuery["Column"]?["Expression"]?["SourceRef"]?["Source"]?.ToString();
                        string? columnEntity = string.Empty;
                        if (!string.IsNullOrEmpty(columnSource))
                        {
                            for (int i = 0; i < visualQueries?["From"]?.Count(); i++)
                            {
                                try
                                {
                                    if (visualQueries?["From"]?[i]?["Name"]?.ToString() == columnSource)
                                    {
                                        columnEntity = visualQueries?["From"]?[i]?["Entity"]?.ToString();
                                    }
                                }
                                catch (Exception ex)
                                {
                                    GlobalHandler.WriteCrashLog(ex.ToString());
                                }
                            }
                        }
                        else
                        {
                            columnEntity = powerQuery?["Column"]?["Expression"]?["SourceRef"]?["Entity"]?.ToString();
                        }

                        var columnProperty = powerQuery?["Column"]?["Property"]?.ToString();
                        if (!string.IsNullOrEmpty(columnProperty))
                        {
                            if (columnEntity != null)
                            {
                                columnsSet.Add(columnEntity + "." + columnProperty);
                            }
                        }

                        // Process Aggregation Source
                        var aggregationSource = powerQuery?["Aggregation"]?["Expression"]?["Column"]?["Expression"]?["SourceRef"]?["Source"]?.ToString();
                        string? aggregationEntity = string.Empty;
                        if (!string.IsNullOrEmpty(aggregationSource))
                        {
                            for (int i = 0; i < visualQueries?["From"]?.Count(); i++)
                            {
                                try
                                {
                                    if (visualQueries?["From"]?[i]?["Name"]?.ToString() == aggregationSource)
                                    {
                                        aggregationEntity = visualQueries?["From"]?[i]?["Entity"]?.ToString();
                                    }
                                }
                                catch (Exception ex)
                                {
                                    GlobalHandler.WriteCrashLog(ex.ToString());
                                }
                            }
                        }
                        else
                        {
                            aggregationEntity = powerQuery?["Column"]?["Expression"]?["SourceRef"]?["Entity"]?.ToString();
                        }

                        var aggregationProperty = powerQuery?["Aggregation"]?["Expression"]?["Column"]?["Property"]?.ToString();
                        if (!string.IsNullOrEmpty(aggregationProperty))
                        {
                            if (aggregationEntity != null)
                            {
                                columnsSet.Add(aggregationEntity + "." + aggregationProperty);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    GlobalHandler.WriteCrashLog(ex.ToString());
                }
            }

            /// in pbir check for field parameter
            for (int i = 0; i < powerQueryFieldParameters?.Count; i++)
            {
                try
                {
                    var powerQuery = powerQueryFieldParameters[i];
                    if (powerQuery != null)
                    {
                        isUsingFieldParameter = true;
                        if (powerQuery["parameterExpr"]?["Column"]?["Property"] != null)
                            columnsInfieldParameters.Add(
                                powerQuery["parameterExpr"]?["Column"]?["Expression"]?["SourceRef"]?["Entity"]?.ToString() + "." + powerQuery["parameterExpr"]?["Column"]?["Property"]?.ToString()
                            );
                        else if (powerQuery["parameterExpr"]?["Measure"]?["Property"] != null)
                            measuresInfieldParameters.Add(powerQuery["parameterExpr"]?["Measure"]?["Property"]?.ToString() ?? "");
                    }
                }
                catch (Exception ex)
                {
                    GlobalHandler.WriteCrashLog(ex.ToString());
                }
            }

            if (isUsingFieldParameter)
            {
                var filteredColumns = columnsSet.Where(item => !columnsInfieldParameters.Contains(item)).ToArray();
                var filteredMeasures = measuresSet.Where(item => !measuresInfieldParameters.Contains(item)).ToArray();
                columnsSet = new HashSet<string>(filteredColumns);
                measuresSet = new HashSet<string>(filteredMeasures);
            }

            var visualToolTip = configSingleVisual?[vcObjectsKey]?["visualTooltip"]?[0];
            var visualActionVisualLink = configSingleVisual?[vcObjectsKey]?["visualLink"]?[0];

            string toolTipSection = visualToolTip?["properties"]?["section"]?["expr"]?["Literal"]?["Value"]?.ToString() ?? "";

            List<ConditionalFormattingSummary> conditionalFormattingSummaries = new ConditionalFormattingHandler().GetConditionalFormattingSummary(config);
            string? conditionalFormattingSummaryString = GlobalHandler.FormateConditionalFormattingSummaryInStr(conditionalFormattingSummaries);

            bool toolTipShow =
                (visualToolTip?["properties"]?["show"]?["expr"]?["Literal"]?["Value"]?.ToString()?.ToLower()) != "false"
                && (visualActionVisualLink?["properties"]?["showDefaultTooltip"]?["expr"]?["Literal"]?["Value"]?.ToString()?.ToLower()) != "false";

            List<string> dataColors = themeJson["dataColors"]?.ToObject<List<string>>() ?? [];
            var visualFormatting = VisualFormatter.GetVisualFormatting(JObject.Parse(config.ToString()), dataColors, isPBIR);
            var visualSubTitle = configSingleVisual?[vcObjectsKey]?["subTitle"]?[0];
            string subTitleText = visualSubTitle?["properties"]?["text"]?["expr"]?
                      ["Literal"]?["Value"]?.ToString()?.Trim('\'') ?? string.Empty;
            VisualSummary VisualSummary = new()
            {
                ReportId = reportId,
                ReportName = reportName,
                PageId = pageId,
                PageName = pageName,
                IsHidden = isHidden,
                VisualId = visualId,
                VisualType = visType,
                OgVisualType = ogVisType,
                VisualTitle = vizTitle,
                VisualSubTitle = subTitleText,
                Fields = Fields,
                Measures = string.Join(", ", measuresSet),
                Columns = string.Join(", ", columnsSet),
                Measure_Column_Mapping = Measure_Column_Mapping ?? [],
                AltText = altText,
                TabOrder = tabOrder?.ToString(),
                ParentGroupId = config?["parentGroupName"]?.ToString(),
                TooltipShow = toolTipShow,
                TooltipType = visualToolTip?["properties"]?["type"]?["expr"]?["Literal"]?["Value"]?.ToString(),
                TooltipSection = toolTipSection,
                InteractionDisabledWith = disabledEditInteractions.ContainsKey(visualId) == true ? disabledEditInteractions[visualId] : [],
                IsCustom = powerbiViz.Contains(ogVisType) == false,
                IsConditionalFormatted = conditionalFormattingSummaries.Count > 0,
                ConditionalFormattingSummary = conditionalFormattingSummaries,
                ConditionalFormattingSummaryString = conditionalFormattingSummaryString,
                IsStaticTextBox = ogVisType == "textbox" && configSingleVisual?["objects"]?["general"]?[0]?["properties"]?["paragraphs"]?.ToString().Contains("propertyIdentifier") != true,
                IsPersonalizationEnabled = isVisualPersonalizationEnabled,
                VisualFilters = visualFilterList,
                VisualFiltersString = GlobalHandler.ConvertFilterListToStr(visualFilterList),
                ButtonText = buttonText,
                X = x,
                Y = y,
                Z = z,
                Width = width,
                Height = height,
                VisualTexts = visualTexts,
                FieldParameter = fieldParameters,
                WithoutTitleFormatting = visualFormatting["WithoutTitleFormatting"],
                DataLabelFormatting = visualFormatting["DataLabelFormatting"],
                BackgroundFormatting = visualFormatting["BackgroundFormatting"],
                BorderFormatting = visualFormatting["BorderFormatting"],
                LegendFormatting = visualFormatting["LegendFormatting"],
                TooltipFormatting = visualFormatting["TooltipFormatting"],
                HasVisualCalculation = hasVisualCalculation,
                CalculationExpression = string.Join("\n", calculationExpression),
                ExtractedColors = visualUsedColors,
                UsedFields = usedFields ?? []
            };

            ReportUIFormattingModel reportUIFormattingModel = new();
            try
            {
                // slicer related quick insight issue
                if (ogVisType == "slicer")
                {
                    SlicersModel slicersModel = new() { VisualId = visualId };

                    var slicerHeaderShow = configSingleVisual?["objects"]?["header"]?[0]?["properties"]?["show"]?["expr"]?["Literal"]?["Value"];
                    var slicerTitleShow = configSingleVisual?[vcObjectsKey]?["title"]?[0]?["properties"]?["show"]?["expr"]?["Literal"]?["Value"];
                    if (slicerHeaderShow?.ToString()?.ToLower() == "false" && (slicerTitleShow == null || slicerTitleShow?.ToString()?.ToLower() == "false"))
                    {
                        slicersModel.IsHeaderDisabled = true;
                    }
                    else
                        slicersModel.IsHeaderDisabled = false;

                    /// slicer not have search enable  ///
                    var isSelfFilterEnable = configSingleVisual?["objects"]?["general"]?[0]?["properties"]?["selfFilterEnabled"]?["expr"]?["Literal"]?["Value"];
                    if (isSelfFilterEnable != null)
                    {
                        if (isSelfFilterEnable.ToString().ToLower() == "false")
                            slicersModel.IsSearchDisabled = true;
                        else
                            slicersModel.IsSearchDisabled = false;
                    }
                    else
                        slicersModel.IsSearchDisabled = true;

                    var isSlicerIsSingleSelectOne = configSingleVisual?["objects"]?["selection"]?[0]?["properties"]?["strictSingleSelect"]?["expr"]?["Literal"]?["Value"];
                    if (isSlicerIsSingleSelectOne != null && isSlicerIsSingleSelectOne.ToString().ToLower() == "true")
                    {
                        // slicer is single select only //
                        slicersModel.IsSearchAllInMultiSelectDisabled = false;
                    }
                    else
                    {
                        /// slicer is multi select and select all is not implemented///
                        var isSlicerSelectAllEnabled = configSingleVisual?["objects"]?["selection"]?[0]?["properties"]?["selectAllCheckboxEnabled"]?["expr"]?["Literal"]?["Value"];
                        if (isSlicerSelectAllEnabled != null)
                        {
                            if (isSlicerSelectAllEnabled.ToString().ToLower() == "false")
                                slicersModel.IsSearchAllInMultiSelectDisabled = true;
                            else
                                slicersModel.IsSearchAllInMultiSelectDisabled = false;
                        }
                        else
                            slicersModel.IsSearchAllInMultiSelectDisabled = true;
                    }
                    slicersModel.Columns = string.Join(", ", columnsSet);
                    slicersModel.Measures = string.Join(", ", measuresSet);

                    reportUIFormattingModel.SlicersList.Add(slicersModel);
                }

                // table or grid quick insights issue
                if (ogVisType == "tableEx" || ogVisType == "pivotTable")
                {
                    TableOrMatrixModel tableOrMatrixModel = new() { VisualId = visualId, VisualType = visType };
                    // column count > 20
                    JArray? tableCols = (JArray)(configSingleVisual?["projections"]?["Values"] ?? new JArray() { });
                    if (tableCols != null)
                        tableOrMatrixModel.ColumnCount = tableCols.Count;
                    else
                        tableOrMatrixModel.ColumnCount = 0;
                    if (ogVisType == "tableEx")
                    {
                        if (configSingleVisual?["objects"]?["total"]?[0]?["properties"]?["totals"]?["expr"]?["Literal"]?["Value"]?.ToString()?.ToLower() == "false")
                            tableOrMatrixModel.IsGrandTotalDisabled = true;
                        else
                            tableOrMatrixModel.IsGrandTotalDisabled = false;
                    }
                    // fully expanded
                    if (ogVisType == "pivotTable")
                    {
                        JArray? expansionStates = (JArray)(configSingleVisual?["expansionStates"]?[0]?["levels"] ?? new JArray() { });
                        if (expansionStates != null)
                        {
                            int len = expansionStates.Count;

                            int expanded = 0;
                            foreach (var state in expansionStates)
                            {
                                try
                                {
                                    var isCollapsed = state?["isCollapsed"];
                                    if (isCollapsed != null && isCollapsed.ToString()?.ToLower() == "true") { }
                                    else
                                        expanded++;
                                }
                                catch (Exception ex)
                                {
                                    GlobalHandler.WriteCrashLog(ex.ToString());
                                }
                            }
                            if (expanded + 1 == len)
                                tableOrMatrixModel.IsFullyExpanded = true;
                            else
                                tableOrMatrixModel.IsFullyExpanded = false;
                        }

                        if (configSingleVisual?["objects"]?["subTotals"]?[0]?["properties"]?["rowSubtotals"]?["expr"]?["Literal"]?["Value"]?.ToString()?.ToLower() == "false")
                        {
                            tableOrMatrixModel.IsRowGrandTotalDisabled = true;
                        }
                        else
                            tableOrMatrixModel.IsRowGrandTotalDisabled = false;

                        if (configSingleVisual?["objects"]?["subTotals"]?[0]?["properties"]?["columnSubtotals"]?["expr"]?["Literal"]?["Value"]?.ToString()?.ToLower() == "false")
                        {
                            tableOrMatrixModel.IsColumnGrandTotalDisabled = true;
                        }
                        else
                            tableOrMatrixModel.IsColumnGrandTotalDisabled = false;
                    }

                    reportUIFormattingModel.TableOrMatrixList.Add(tableOrMatrixModel);
                }

                if ((new[] { "image", "shape", "actionButton", "textbox", "basicShape" }).Contains(ogVisType))
                {
                    StaticComponentsModel staticComponentsModel = new StaticComponentsModel { VisualId = visualId, VisualType = visType };

                    if (ogVisType == "actionButton")
                    {
                        staticComponentsModel.ButtonText = buttonText;
                        if (configSingleVisual?[vcObjectsKey]?["visualLink"]?[0]?["properties"]?["show"]?["expr"]?["Literal"]?["Value"]?.ToString().ToLower() == "false")
                            staticComponentsModel.IsButtonActionDisabled = true;
                        else
                            staticComponentsModel.IsButtonActionDisabled = false;

                        if (toolTipShow == false)
                            staticComponentsModel.IsButtonToolTipDisabled = true;
                        else
                            staticComponentsModel.IsButtonToolTipDisabled = false;
                    }

                    if (configSingleVisual?[vcObjectsKey]?["visualHeader"]?[0]?["properties"]?["show"]?["expr"]?["Literal"]?["Value"]?.ToString().ToLower() == "false")
                        staticComponentsModel.IsHeaderIconEnabled = false;
                    else
                        staticComponentsModel.IsHeaderIconEnabled = true;

                    reportUIFormattingModel.StaticComponentsList.Add(staticComponentsModel);
                }
            }
            catch (Exception ex)
            {
                GlobalHandler.WriteCrashLog(ex.ToString());
            }

            return new GetVisualDetailsReturnType()
            {
                VisualSummary = VisualSummary,
                IsGroup = false,
                AllUsedFontFamilies = allUsedFontFamilies,
                ReportUIFormattingModel = reportUIFormattingModel,
            };
        }

        public static void FixVisualsPosition(List<VisualSummary> visualList, Dictionary<string, VisualGroup> visualGroups)
        {
            foreach (var visual in visualList)
            {
                try
                {
                    if (visual.ParentGroupId != null && visualGroups.ContainsKey(visual.ParentGroupId))
                    {
                        VisualGroup parentGroup = visualGroups[visual.ParentGroupId];
                        FixSingleVisualPosition(visual, parentGroup, visualGroups);
                    }
                }
                catch (Exception ex)
                {
                    GlobalHandler.WriteCrashLog(ex.ToString());
                }
            }
        }

        private static void FixSingleVisualPosition(VisualSummary visual, VisualGroup parentGroup, Dictionary<string, VisualGroup> visualGroups)
        {
            try
            {
                // for each visual adjust the visual group x and y positions
                if (parentGroup.X != null)
                    visual.X = (decimal.Parse(visual.X ?? "0") + decimal.Parse(parentGroup.X)).ToString();

                if (parentGroup.Y != null)
                    visual.Y = (decimal.Parse(visual.Y ?? "0") + decimal.Parse(parentGroup.Y)).ToString();

                if (parentGroup.ParentGroupId != null && visualGroups.ContainsKey(parentGroup.ParentGroupId))
                {
                    FixSingleVisualPosition(visual, visualGroups[parentGroup.ParentGroupId], visualGroups);
                }
            }
            catch (Exception ex)
            {
                GlobalHandler.WriteCrashLog("Error in FixSingleVisualPosition: " + ex.ToString());
            }
        }

        public static void FixDisabledEditInteractionsFormat(List<VisualSummary> visualList, Dictionary<string, VisualSummary> visualDict)
        {
            foreach (var visual in visualList)
            {
                try
                {
                    if (visual.InteractionDisabledWith == null)
                        continue;
                    List<string> disabledVisualIds = visual.InteractionDisabledWith;
                    string disabledVisualIdsStr = "";
                    int index = 1;
                    foreach (var disabledVisualId in disabledVisualIds)
                    {
                        try
                        {
                            if (visualDict.ContainsKey(disabledVisualId))
                            {
                                VisualSummary disabledVisual = visualDict[disabledVisualId];
                                if (disabledVisual == null)
                                    continue;

                                disabledVisualIdsStr +=
                                    index.ToString()
                                    + ") VisualId: "
                                    + disabledVisual.VisualId
                                    + ", VisualType: "
                                    + disabledVisual.VisualType
                                    + (disabledVisual.VisualTitle != null && disabledVisual.VisualTitle != "" ? ", VisualTitle: " + disabledVisual.VisualTitle : "")
                                    + "\n";
                                index++;
                            }
                        }
                        catch (Exception ex)
                        {
                            GlobalHandler.WriteCrashLog(ex.ToString());
                        }
                    }

                    visual.InteractionDisabledWithString = disabledVisualIdsStr;
                }
                catch (Exception ex)
                {
                    GlobalHandler.WriteCrashLog(ex.ToString());
                }
            }
        }

        public static List<BookmarkModel> CreateBookMarkList(
            JToken json,
            Dictionary<string, VisualSummary> visualDict,
            Dictionary<string, PageSummary> pageDictionary,
            string reportLayoutFileName,
            string reportLayoutId,
            bool isPBIR = false
        )
        {
            List<BookmarkModel> bookmarkList = new();
            JToken reportConfig = isPBIR ? json : JToken.Parse((json["config"] ?? new JObject() { }).ToString());

            int count = 0;
            JArray bookmarks = (JArray)(reportConfig?["bookmarks"] ?? new JArray() { });
            foreach (var bookmark in bookmarks)
            {
                try
                {
                    count++;
                    if (bookmark == null)
                        continue;
                    string pageId = bookmark["explorationState"]?["activeSection"]?.ToString() ?? String.Empty;
                    PageSummary? PageSummary = pageId != null && pageDictionary.ContainsKey(pageId) ? pageDictionary[pageId] : null;
                    if (pageId == null || PageSummary == null)
                        continue;

                    var visuals = bookmark["explorationState"]?["sections"]?[pageId]?["visualContainers"];
                    int visualIndex = 1;
                    string visualListString = "";
                    if (visuals != null)
                    {
                        foreach (JProperty property in visuals.Children<JProperty>())
                        {
                            try
                            {
                                string visualId = property.Name;
                                VisualSummary? visual = visualId != null && visualDict.ContainsKey(visualId) ? visualDict[visualId] : null;
                                if (visual == null)
                                    continue;
                                visualListString +=
                                    visualIndex.ToString()
                                    + ") VisualId: "
                                    + visual?.VisualId
                                    + ", VisualType: "
                                    + visual?.VisualType
                                    + (visual?.VisualTitle != null && visual?.VisualTitle != "" ? ", VisualTitle: " + visual?.VisualTitle : "")
                                    + "\n";
                                visualIndex++;
                            }
                            catch (Exception ex)
                            {
                                GlobalHandler.WriteCrashLog(ex.ToString());
                            }
                        }
                    }

                    bookmarkList.Add(
                        new BookmarkModel()
                        {
                            ReportId = reportLayoutId,
                            ReportName = reportLayoutFileName ?? "",
                            Index = count,
                            Id = bookmark["name"]?.ToString() ?? "",
                            Name = bookmark["displayName"]?.ToString() ?? "",
                            PageId = pageId,
                            PageName = PageSummary.PageName ?? "Page Name",
                            AffectedVisuals = visualListString,
                        }
                    );
                }
                catch (Exception ex)
                {
                    GlobalHandler.WriteCrashLog(ex.ToString());
                }
            }
            return bookmarkList;
        }

        public static void CreateAlignmentDataSet(List<object> alignmentIssues, DataSet ds, string alignmentType)
        {
            DataTable alignmentData = ds.Tables.Add("A_" + alignmentType);
            alignmentData.Columns.AddRange([
                new DataColumn("Page Name", typeof(string)),
                new DataColumn("Visual 1", typeof(string)),
                new DataColumn("Visual 2", typeof(string)),
                new DataColumn("Border", typeof(string)),
                new DataColumn("Alignment Gap", typeof(string)),
            ]);

            foreach (var issue in alignmentIssues)
            {
                try
                {
                    if (issue is Dictionary<string, object> dict)
                    {
                        string pageName = GetDictValue(dict, "PageName", "pageName");
                        string visual1 = GetDictValue(dict, "Visual1", "visual1");
                        string visual2 = GetDictValue(dict, "Visual2", "visual2");
                        string spacingViolation = GetDictValue(dict, "SpacingViolation", "spacingViolation");
                        string[] parts = spacingViolation.Split(':');
                        string border = parts[0].Trim();
                        string alignmentGap = parts[1].Trim();

                        alignmentData.Rows.Add(
                            pageName ?? "",
                            visual1 ?? "",
                            visual2 ?? "",
                            border ?? "",
                            alignmentGap ?? ""
                        );
                    }
                    else
                    {
                        Console.WriteLine($"Type mismatch: Expected Dictionary, got {issue?.GetType().Name ?? "null"}");
                    }
                }
                catch (Exception ex)
                {
                    GlobalHandler.WriteCrashLog(ex.ToString());
                }
            }
        }
        
        public static void CreateSpacingDataSet(List<object> alignmentIssues, DataSet ds, string alignmentType)
        {
            DataTable alignmentData = ds.Tables.Add("A_" + alignmentType);
            alignmentData.Columns.AddRange([
                new DataColumn("Page Name", typeof(string)),
                new DataColumn("Visual 1", typeof(string)),
                new DataColumn("Visual 2", typeof(string)),
                new DataColumn("Spacing Violation", typeof(string)),
            ]);
            
            foreach (var issue in alignmentIssues)
            {
                try
                {
                    if (issue is Dictionary<string, object> dict)
                    {
                        string pageName = GetDictValue(dict, "PageName", "pageName");
                        string visual1 = GetDictValue(dict, "Visual1", "visual1");
                        string visual2 = GetDictValue(dict, "Visual2", "visual2");
                        string spacingViolation = GetDictValue(dict, "SpacingViolation", "spacingViolation");
                                                
                        alignmentData.Rows.Add(
                            pageName ?? "",
                            visual1 ?? "",
                            visual2 ?? "",
                            spacingViolation ?? ""
                        );
                    }
                    else
                    {
                        Console.WriteLine($"Type mismatch: Expected Dictionary, got {issue?.GetType().Name ?? "null"}");
                    }
                }
                catch (Exception ex)
                {
                    GlobalHandler.WriteCrashLog(ex.ToString());
                }
            }
        }

        public static void CreateMarginDataSet(List<object> marginIssues, DataSet ds, string marginType)
        {
            DataTable marginData = ds.Tables.Add("A_" + marginType);
            marginData.Columns.AddRange([
                new DataColumn("Page Name", typeof(string)),
                new DataColumn("Visual ID", typeof(string)),
                new DataColumn("Visual Information", typeof(string)),
                new DataColumn("Border", typeof(string)),
                new DataColumn("Alignment Gap", typeof(string)),
            ]);
            
            foreach (var issue in marginIssues)
            {
                try
                {
                    if (issue is Dictionary<string, object> dict)
                    {
                        string pageName = GetDictValue(dict, "PageName", "pageName");
                        string visualId = GetDictValue(dict, "VisualId", "visualId");
                        string visualInfo = GetDictValue(dict, "VisualInformation", "visualInformation");
                        string spacingViolation = GetDictValue(dict, "SpacingViolation", "spacingViolation");
                        string[] parts = spacingViolation.Split(':');
                        string border = parts[0].Trim();
                        string alignmentGap = parts[1].Trim();
                                                
                        marginData.Rows.Add(
                            pageName ?? "",
                            visualId ?? "",
                            visualInfo ?? "",
                            border ?? "",
                            alignmentGap ?? ""
                        );
                    }
                    else
                    {
                        Console.WriteLine($"Type mismatch: Expected Dictionary, got {issue?.GetType().Name ?? "null"}");
                    }
                }
                catch (Exception ex)
                {
                    GlobalHandler.WriteCrashLog(ex.ToString());
                }
            }
        }

        private static string GetDictValue(Dictionary<string, object> dict, params string[] possibleKeys)
        {
            foreach (var key in possibleKeys)
            {
                if (dict.TryGetValue(key, out object value))
                {
                    return value?.ToString() ?? "";
                }
            }
            return "";
        }

        public static void CreateReportSummaryDataSet(List<ReportSummary> reportSummary, DataSet ds)
        {
            DataTable reportSummaryData = ds.Tables.Add("D_Report Summary");
            reportSummaryData.Columns.AddRange([new("Report Name", typeof(string)), new("Field", typeof(string)), new("Value", typeof(string))]);
            foreach (var reportSummaryPoint in reportSummary)
            {
                try
                {
                    reportSummaryData.Rows.Add(reportSummaryPoint.ReportName, reportSummaryPoint.Field, reportSummaryPoint.Value);
                }
                catch (Exception ex)
                {
                    GlobalHandler.WriteCrashLog(ex.ToString());
                }
            }
        }

        public static void CreatePageDataSet(List<PageSummary> pageList, DataSet ds)
        {
            DataTable pageSummaryData = ds.Tables.Add("D_Page Summary");
            pageSummaryData.Columns.AddRange(
                [
                    new DataColumn("Report Name", typeof(string)),
                    new DataColumn("Page Index", typeof(string)),
                    new DataColumn("Page Id", typeof(string)),
                    new DataColumn("Page Name", typeof(string)),
                    new DataColumn("Visible", typeof(string)),
                    new DataColumn("Page size", typeof(string)),
                    new DataColumn("Page Type", typeof(string)),
                    new DataColumn("Page Filters", typeof(string)),
                    new DataColumn("Page Type Filters", typeof(string)),
                    new DataColumn("Total Visuals", typeof(string)),
                    new DataColumn("Total Static Components", typeof(string)),
                    new DataColumn("Total Slicers", typeof(string)),
                    new DataColumn("Total Grids", typeof(string)),
                    new DataColumn("Total Bookmarks", typeof(string)),
                ]
            );
            int i = 1;
            foreach (var page in pageList)
            {
                try
                {
                    pageSummaryData.Rows.Add(
                        page.ReportName,
                        i.ToString(),
                        page.PageId,
                        page.PageName,
                        page.IsHidden ? "No" : "Yes",
                        page.Size,
                        page.PageType,
                        page.PageFiltersString,
                        page.PageTypeFilter,
                        page.TotalVisuals,
                        page.TotalStatic,
                        page.TotalSlicers,
                        page.TotalGrids,
                        page.TotalBookmarks
                    );
                    i++;
                }
                catch (Exception ex)
                {
                    GlobalHandler.WriteCrashLog(ex.ToString());
                }
            }
        }

        public static void CreateVisualDataSet(List<VisualSummary> visualList, DataSet ds)
        {
            DataTable visualsData = ds.Tables.Add("D_Visual Summary");
            visualsData.Columns.AddRange(
                [
                    new DataColumn("Report Name", typeof(string)),
                    new DataColumn("Page Name", typeof(string)),
                    new DataColumn("Visual Type", typeof(string)),
                    new DataColumn("Visual Title", typeof(string)),
                    new DataColumn("Visual Id", typeof(string)),
                    new DataColumn("Visible", typeof(string)),
                    new DataColumn("Tab Order", typeof(string)),
                    new DataColumn("Visual Group", typeof(string)),
                    new DataColumn("Visual Fields -Columns", typeof(string)),
                    new DataColumn("Visual Fields - Measures", typeof(string)),
                    new DataColumn("Alt Text", typeof(string)),
                    new DataColumn("Visual Filters", typeof(string)),
                    new DataColumn("Tooltip", typeof(string)),
                    new DataColumn("Is Conditional Formatted", typeof(string)),
                    new DataColumn("Conditional Formatting Summary", typeof(string)),
                    new DataColumn("Is Personalization Enabled", typeof(string)),
                    new DataColumn("Interaction Disabled With", typeof(string)),
                ]
            );

            foreach (var visual in visualList)
            {
                try
                {
                    visualsData.Rows.Add(
                        visual.ReportName,
                        visual.PageName,
                        visual.VisualType,
                        visual.VisualTitle,
                        visual.VisualId,
                        visual.IsHidden == true ? "No" : "Yes",
                        visual.TabOrder,
                        visual.ParentGroupId,
                        visual.Columns,
                        visual.Measures,
                        visual.AltText,
                        visual.VisualFiltersString,
                        visual.TooltipShow == true ? "Yes" : "No",
                        visual.ConditionalFormattingSummary?.Count > 0 ? "Yes" : "No",
                        GlobalHandler.FormateConditionalFormattingSummaryInStr(visual.ConditionalFormattingSummary),
                        visual.IsPersonalizationEnabled,
                        visual.InteractionDisabledWithString
                    );
                }
                catch (Exception ex)
                {
                    GlobalHandler.WriteCrashLog(ex.ToString());
                }
            }
        }

        public static void CreateBookmarkDataSet(List<BookmarkModel> bookmarks, DataSet ds)
        {
            DataTable bookmarkData = ds.Tables.Add("D_Bookmarks present in Report");
            bookmarkData.Columns.AddRange(
                [
                    new DataColumn("Report Name", typeof(string)),
                    new DataColumn("index", typeof(string)),
                    new DataColumn("id", typeof(string)),
                    new DataColumn("name", typeof(string)),
                    new DataColumn("Page Name", typeof(string)),
                    new DataColumn("Affected visual Ids", typeof(string)),
                ]
            );

            foreach (var bookmark in bookmarks)
            {
                try
                {
                    bookmarkData.Rows.Add(bookmark.ReportName, bookmark.Index, bookmark.Id, bookmark.Name, bookmark.PageName, bookmark.AffectedVisuals);
                }
                catch (Exception ex)
                {
                    GlobalHandler.WriteCrashLog(ex.ToString());
                }
            }
        }
    }
}