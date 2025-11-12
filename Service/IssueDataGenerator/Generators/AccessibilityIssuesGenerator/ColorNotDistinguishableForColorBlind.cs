using PowerBI_MCP.DTO;
using PowerBI_MCP.Handlers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PowerBI_MCP.Service.IssueDataGenerator.Generators.AccessibilityIssuesGenerator
{
   
    public class ColorNotDistinguishableForColorBlind : IIssueDataGenerator
    {
        private class ColorAndLocation
        {
            public string Color { get; set; } = string.Empty;
            public List<Dictionary<string, string>> ColorValue { get; set; } = [];
        }
        private class ForeBackgroundPair
        {
            public string ForegroundColor { get; set; } = string.Empty;
            public string BackgroundColor { get; set; } = string.Empty;
            public string ItemParentCategory { get; set; } = string.Empty;
            public string ItemLocation { get; set; } = string.Empty;
            public string ItemType { get; set; } = string.Empty;
        }
        private string? ResolveVisualBackgroundColor(JObject themeJson, VisualSummary vis)
        {
            try
            {
                // 1) Explicit visual background formatting from PBIX extraction
                if (vis.BackgroundFormatting != null)
                {
                    try
                    {
                        bool isVisible = vis.BackgroundFormatting.TryGetValue("Visibility", out var visibility) ? visibility?.ToLower() != "false" : true;
                        string? bgColor = vis.BackgroundFormatting.TryGetValue("Color", out var colorStr) ? (colorStr ?? string.Empty) : null;
                        if (isVisible && !string.IsNullOrWhiteSpace(bgColor) && !string.Equals(bgColor, "Default", StringComparison.OrdinalIgnoreCase))
                        {
                            return bgColor.Trim().Replace("'", "");
                        }
                    }
                    catch { }
                }

                // 2) Theme-based visual background
                JObject theme = themeJson;
                if (theme != null)
                {
                    try
                    {
                        JObject asteriskVisualStyles = (JObject)(theme?["visualStyles"]?["*"]?["*"] ?? new JObject());
                        JObject visualStyles = (JObject)(theme?["visualStyles"]?[vis.OgVisualType ?? "*"]?["*"] ?? asteriskVisualStyles);
                        if (visualStyles != null)
                        {
                            // Look for ThemeDataColor first
                            var themeDataColor = visualStyles?["background"]?.FirstOrDefault()?["solid"]?["color"]?["expr"]?["ThemeDataColor"] as JObject;
                            if (themeDataColor != null)
                            {
                                var dataColors = theme?["dataColors"]?.ToObject<List<string>>() ?? [];
                                return GlobalHandler.GetThemeDataColorToHex(themeDataColor, dataColors);
                            }

                            // Otherwise look for a solid literal color
                            var literal = visualStyles?["background"]?.FirstOrDefault()?["solid"]?["color"]?.ToString()?.Trim()?.Replace("'", "");
                            if (!string.IsNullOrWhiteSpace(literal))
                            {
                                return literal;
                            }
                        }
                    }
                    catch { }
                }
            }
            catch { }
            return null;
        }
        private string? ResolvePageBackgroundColor(JObject themeJson)
        {
            try
            {
                JObject theme = themeJson;
                if (theme == null) return null;

                JObject? pageStyles = (JObject?)(theme?["visualStyles"]?["page"]?["*"] ?? theme?["visualStyles"]?["*"]?["*"]);
                if (pageStyles == null) return null;

                // Try outspace color
                var outspaceThemeData = pageStyles?["outspace"]?.FirstOrDefault()?["solid"]?["color"]?["expr"]?["ThemeDataColor"] as JObject
                    ?? pageStyles?["outspace"]?.FirstOrDefault()?["color"]?["expr"]?["ThemeDataColor"] as JObject;
                if (outspaceThemeData != null)
                {
                    var dataColors = theme?["dataColors"]?.ToObject<List<string>>() ?? [];
                    var hex = GlobalHandler.GetThemeDataColorToHex(outspaceThemeData, dataColors);
                    if (!string.IsNullOrWhiteSpace(hex)) return hex;
                }

                var outspaceLiteral = pageStyles?["outspace"]?.FirstOrDefault()?["solid"]?["color"]?.ToString()?.Trim()?.Replace("'", "")
                    ?? pageStyles?["outspace"]?.FirstOrDefault()?["color"]?.ToString()?.Trim()?.Replace("'", "");
                if (!string.IsNullOrWhiteSpace(outspaceLiteral)) return outspaceLiteral;

                // Then page background color
                var pageBgThemeData = pageStyles?["background"]?.FirstOrDefault()?["solid"]?["color"]?["expr"]?["ThemeDataColor"] as JObject
                    ?? pageStyles?["background"]?.FirstOrDefault()?["color"]?["expr"]?["ThemeDataColor"] as JObject;
                if (pageBgThemeData != null)
                {
                    var dataColors = theme?["dataColors"]?.ToObject<List<string>>() ?? [];
                    var hex = GlobalHandler.GetThemeDataColorToHex(pageBgThemeData, dataColors);
                    if (!string.IsNullOrWhiteSpace(hex)) return hex;
                }

                var pageBgLiteral = pageStyles?["background"]?.FirstOrDefault()?["solid"]?["color"]?.ToString()?.Trim()?.Replace("'", "")
                    ?? pageStyles?["background"]?.FirstOrDefault()?["color"]?.ToString()?.Trim()?.Replace("'", "");
                if (!string.IsNullOrWhiteSpace(pageBgLiteral)) return pageBgLiteral;
            }
            catch { }
            return null;
        }

        private string CreateColorIndicatorBlock(string color)
        {
            // Handle null or empty color
            if (string.IsNullOrWhiteSpace(color))
            {
                return "[No Color]";
            }

            // Try to parse as JSON object (e.g., {"solid":{"color":"#FFFFFF"}})
            try
            {
                var colorObj = JObject.Parse(color);
                var solidColor = colorObj?["solid"]?["color"]?.ToString();
                if (!string.IsNullOrWhiteSpace(solidColor))
                {
                    color = solidColor;
                }
            }
            catch (JsonReaderException)
            {
                // Not a JSON object, use the color as is
            }
            catch (Exception ex)
            {
                GlobalHandler.WriteCrashLog($"Error parsing color value '{color}': {ex.Message}");
            }

            // Ensure color starts with # for hex values
            if (!color.StartsWith("#") && color.Length == 6 && System.Text.RegularExpressions.Regex.IsMatch(color, "^[0-9A-Fa-f]{6}$"))
            {
                color = "#" + color;
            }

            string div =
                $@"<div style=""display:flex; align-items:center;"">
                    <div style=""background-color:{color}; border:1px solid rgb(197, 193, 193); width:15px; height: 15px; margin-right:5px;""></div>
                    <span>{color}</span>
                </div>";
            return div;
        }
        private string? InsertDataInColorBlindIssues(string color1, string color2, string color1Pos = "", string color2Pos = "")
        {
            if (color1 == color2)
                return null;

            Dictionary<ColorBlindnessType, bool> issueDict = ColorContrastHandler.AreColorsDistinctForAllTypesOfColorBlind(color1, color2);
            List<string> issueColorBlindType = [];
            foreach (var issue in issueDict)
            {
                try
                {
                    if (!issue.Value)
                        issueColorBlindType.Add(issue.Key.ToString());
                }
                catch (Exception ex)
                {
                    GlobalHandler.WriteCrashLog(ex.ToString());
                }
            }
            // {(color1Pos.Length > 0 ? color1Pos + " " : "")}
            //  {(color2Pos.Length > 0 ? color2Pos + " " : "")}
            if (issueColorBlindType.Count == 0)
                return null;
            return $@"The colors pair {CreateColorIndicatorBlock(color1)} {(color1Pos.Length > 0 ? color1Pos + " " : "")}and {CreateColorIndicatorBlock(color2)}  {(color2Pos.Length > 0 ? color2Pos + " " : "")}cannot be distinguished by individuals with {string.Join(", ", issueColorBlindType)} color blindness";

        }
        private List<ForeBackgroundPair> ForeAndBackgroundPairColors(VisualSummary vis, string visBgColor = "#FFFFFF")
        {
            List<ForeBackgroundPair> foreAndBackgroundPairColors = [];
            foreach (var visualColor in vis.ExtractedColors)
            {
                try
                {
                    string color = visualColor.Key;
                    List<Dictionary<string, string>> locations = visualColor.Value;
                    foreach (var loc in locations)
                    {
                        try
                        {
                            string location = loc["location"];
                            if (location.ToLower().Contains("back") == true) continue;
                            string parentCategory = loc["parentCategory"];
                            string? bgColorPrimaryStr = null;

                            bool foundPrimaryBackground = false;
                            foreach (var bgColorPrimary in vis.ExtractedColors)
                            {
                                try
                                {
                                    var bgLocs = bgColorPrimary.Value;
                                    foreach (var bgLoc in bgLocs)
                                    {
                                        string bgLocation = bgLoc["location"];
                                        string parentCategoryBg = bgLoc["parentCategory"];

                                        if (bgLocation.ToLower().Contains("back") == false) continue;

                                        if (parentCategoryBg == parentCategory)
                                        {
                                            foundPrimaryBackground = true;
                                            bgColorPrimaryStr = bgColorPrimary.Key;
                                            foreAndBackgroundPairColors.Add(new()
                                            {
                                                ForegroundColor = color,
                                                BackgroundColor = bgColorPrimaryStr,
                                                ItemParentCategory = parentCategory,
                                                ItemLocation = location
                                            });
                                            break;
                                        }
                                    }

                                }
                                catch (Exception ex)
                                {
                                    GlobalHandler.WriteCrashLog(ex.ToString());
                                }
                            }

                            // primary background color is not found in visual colors, so we will use the final background color
                            if (foundPrimaryBackground == false)
                            {
                                foreAndBackgroundPairColors.Add(new()
                                {
                                    ForegroundColor = color,
                                    BackgroundColor = visBgColor,
                                    ItemParentCategory = parentCategory,
                                    ItemLocation = location
                                });
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
            return foreAndBackgroundPairColors;
        }
        private List<ColorAndLocation> VisualDataPointsColors(VisualSummary vis)
        {
            List<ColorAndLocation> colorsList = [];
            foreach (var color in vis.ExtractedColors)
            {
                string colorKey = color.Key;
                List<Dictionary<string, string>> dataPoints = [];
                foreach (var loc in color.Value)
                {
                    string location = loc["location"];
                    string parentCategory = loc["parentCategory"];
                    if (parentCategory.ToLower().Contains("data") == true) dataPoints.Add(new Dictionary<string, string>() { { "location", location }, { "parentCategory", parentCategory } });
                }
                if (dataPoints.Count > 0) colorsList.Add(new() { Color = colorKey, ColorValue = dataPoints });

            }
            return colorsList;
        }

        private List<object> GetColorNotValidColorblindData(IssueContext issueContext)
        {
            List<object> issueData = [];

            // then colors used in visual
            foreach (var vis in issueContext.ReportDocumentation?.VisualList ?? [])
            {
                try
                {
                    // Dictionary<string, List<Dictionary<string, string>>>
                    List<ColorAndLocation> visualDataPointsColors = VisualDataPointsColors(vis);
                    for (int i = 0; i < visualDataPointsColors.Count; i++)
                    {
                        for (int j = i + 1; j < visualDataPointsColors.Count; j++)
                        {
                            if (i != j)
                            {
                                var loc1L = visualDataPointsColors[i].ColorValue.Select(loc => $"{loc["parentCategory"]} {loc["location"]}").ToList();

                                var loc2L = visualDataPointsColors[j].ColorValue.Select(loc => $"{loc["parentCategory"]} {loc["location"]}").ToList();

                                string? error = InsertDataInColorBlindIssues(visualDataPointsColors[i].Color, visualDataPointsColors[j].Color,
                                                                                    "(" + string.Join(", ", loc1L) + ")", "(" + string.Join(", ", loc2L) + ")");
                                if (error != null)
                                {
                                    Dictionary<string, object> errorDict = [];
                                    errorDict["reportId"] = vis.ReportId ?? "";
                                    errorDict["reportName"] = vis.ReportName ?? "";
                                    errorDict["pageId"] = vis.PageId ?? "";
                                    errorDict["pageName"] = vis.PageName ?? "";
                                    errorDict["visualId"] = vis.VisualId ?? "";
                                    errorDict["visualType"] = vis.VisualType ?? "";
                                    errorDict["visualTitle"] = vis.VisualTitle ?? "";
                                    errorDict.Add("error", error);
                                    issueData.Add(errorDict);
                                }
                            }
                        }
                    }

                    // Resolve fallback backgrounds: element background -> visual background -> page background
                    JObject theme = JObject.Parse(issueContext.ReportDocumentation?.Theme ?? "{}");
                    string fallbackBackground = ResolveVisualBackgroundColor(theme, vis)
                        ?? ResolvePageBackgroundColor(theme)
                        ?? "#FFFFFF";

                    List<ForeBackgroundPair> foreAndBackgroundPairColors = ForeAndBackgroundPairColors(vis, fallbackBackground);
                    foreach (var pair in foreAndBackgroundPairColors)
                    {
                        try
                        {
                            string? error = InsertDataInColorBlindIssues(pair.ForegroundColor, pair.BackgroundColor, pair.ItemParentCategory + " " + pair.ItemLocation, "Background Color");
                            if (error != null)
                            {
                                Dictionary<string, object> errorDict = [];
                                errorDict["reportId"] = vis.ReportId ?? "";
                                errorDict["reportName"] = vis.ReportName ?? "";
                                errorDict["pageId"] = vis.PageId ?? "";
                                errorDict["pageName"] = vis.PageName ?? "";
                                errorDict["visualId"] = vis.VisualId ?? "";
                                errorDict["visualType"] = vis.VisualType ?? "";
                                errorDict["visualTitle"] = vis.VisualTitle ?? "";
                                errorDict.Add("error", error);
                                issueData.Add(errorDict);
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

            return issueData;
        }
        public SingleIssueRuleData GetData(IssueContext issueContext)
        {
            List<object> issueData = GetColorNotValidColorblindData(issueContext) ?? [];
            return new() { Issues = issueData, MaxIssuable = issueData.Count };
        }
    }
}