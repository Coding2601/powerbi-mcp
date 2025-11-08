using PowerBI_MCP.Utils;
using Newtonsoft.Json.Linq;
using System.Globalization;
using PowerBI_MCP.DTO;
using PowerBI_MCP.Models;
using System.Text.RegularExpressions;
using System.Text;
using System.Data;
using ClosedXML.Excel;
using Microsoft.AnalysisServices.Tabular;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;

namespace PowerBI_MCP.Handlers
{

    /// <summary>
    /// Model for theme data color, used for color adjustments.
    /// </summary>
    class ThemeDataColorModel
    {
        public int ColorId { get; set; }
        public float Percent { get; set; }
    }

    /// <summary>
    /// Provides global utility methods for file operations, formatting, and logging.
    /// </summary>
    public class GlobalHandler
    {
        private static readonly ConfigurationManager configurationManager = AppConfig.configuration;
        private static readonly TelemetryConfiguration telemetryClientConfig = new() { ConnectionString = "InstrumentationKey="+ configurationManager["ApplicationInsights:InstrumentationKey"] };
        private static readonly TelemetryClient telemetryClient = new(telemetryClientConfig);

        /// <summary>
        /// Writes a crash log entry. (Implementation needed.)
        /// </summary>
        /// <param name="exceptionMessage">Exception message to log.</param>
        public static void WriteCrashLog(string exceptionMessage, string? userEmail = "Unknown User", string? exceptionCategory = null)
        {
            try
            {
                var exception = new Exception(exceptionMessage);
                var exceptionTelemetry = new ExceptionTelemetry(exception) { SeverityLevel = SeverityLevel.Error, Timestamp = DateTimeOffset.Now };
                exceptionTelemetry.Properties["Context"] = exceptionCategory ?? "Backend Server Error";
                telemetryClient.Context.Component.Version = "1.0.0";
                telemetryClient.Context.User.AuthenticatedUserId = userEmail ?? "Unknown User";
                telemetryClient.TrackException(exceptionTelemetry);
                telemetryClient.Flush();
            }
            catch (System.Exception ex)
            {
                // Handle any exceptions that occur while logging the crash log.
                // Console.WriteLine("Error writing crash log: " + ex.ToString());
            }
        }

        public static bool CheckArtifactExistsById(string artifactId)
        {
            var inReport = ArtifactModel.Reports.Any(report =>
                report.ReportId.Equals(artifactId, StringComparison.OrdinalIgnoreCase));
            var inDataset = ArtifactModel.Datasets.Any(dataset =>
                dataset.DatasetId.Equals(artifactId, StringComparison.OrdinalIgnoreCase));
            return inReport || inDataset;
        }
        
        public static bool IsArtifactAvailable(string artifactName, string artifactPath)
        {
            var inReport = ArtifactModel.Reports.Any(report =>
                report.ReportName.Equals(artifactName, StringComparison.OrdinalIgnoreCase) &&
                report.ReportPath.Equals(artifactPath, StringComparison.OrdinalIgnoreCase));
            var inDataset = ArtifactModel.Datasets.Any(dataset =>
                dataset.ServerName.Equals(artifactName, StringComparison.OrdinalIgnoreCase));
            return inReport || inDataset;
        }

        /// <summary>
        /// Reads a JSON file and returns its content as a JObject.
        /// </summary>
        /// <param name="filePath">Path to the JSON file.</param>
        /// <param name="encoding">Encoding to use for reading.</param>
        /// <returns>JObject with file content, or null if file does not exist.</returns>
        public static JObject? ReadJsonFile(string filePath, Encoding encoding)
        {
            if (File.Exists(filePath) == false)
                return null;
            string jsonString = File.ReadAllText(filePath, encoding);
            JObject jsonContent = JObject.Parse(jsonString);
            return jsonContent;
        }

        /// <summary>
        /// Converts BIM JSON to a Tabular Database object.
        /// </summary>
        /// <param name="bimJson">BIM JSON string.</param>
        /// <returns>Database object.</returns>
        public static Database ConvertBimJsonToDatabase(string bimJson)
        {
            return JsonSerializer.DeserializeDatabase(bimJson, null, Microsoft.AnalysisServices.CompatibilityMode.PowerBI);
        }

        /// <summary>
        /// Formats a list of conditional formatting summaries as a string.
        /// </summary>
        /// <param name="formattingSummaryList">List of summaries.</param>
        /// <returns>Formatted string.</returns>
        public static string FormateConditionalFormattingSummaryInStr(List<ConditionalFormattingSummary>? formattingSummaryList)
        {
            if (formattingSummaryList == null)
                return "";
            string formattedStr = "";
            int i = 1;
            foreach (ConditionalFormattingSummary formattingSummary in formattingSummaryList)
            {
                try
                {
                    if (formattingSummary.ConditionalFormattingDetails == null)
                        continue;
                    foreach (ConditionalFormattingDetail formatDetail in formattingSummary.ConditionalFormattingDetails)
                    {
                        try
                        {
                            formattedStr += i.ToString() + ") " + formattingSummary?.AppliedOn + ": " + (formatDetail.ColumnName != null ? formatDetail.ColumnName : formatDetail.FormatName) + "\n";
                            if (formatDetail.DependedColumnName != null)
                                formattedStr += "       Dependent Field: " + formatDetail.DependedColumnName + "\n";
                            if (formatDetail.ColumnName != null && formatDetail.FormatName != null)
                                formattedStr += "       Dependent Field: " + formatDetail.FormatName + "\n";
                            if (formatDetail.FormatStyle != null)
                                formattedStr += "       Formatting Field: " + formatDetail.FormatStyle + "\n";
                            if (formatDetail.Summarization != null)
                                formattedStr += "       Summarization: " + formatDetail.Summarization + "\n";
                        }
                        catch (Exception ex)
                        {
                            WriteCrashLog(ex.ToString());
                        }
                    }

                    i++;
                }
                catch (Exception ex)
                {
                    WriteCrashLog(ex.ToString());
                }
            }

            return formattedStr;
        }

        /// <summary>
        /// Converts a list of filter dictionaries to a formatted string.
        /// </summary>
        /// <param name="filterList">List of filter dictionaries.</param>
        /// <returns>Formatted string.</returns>
        public static string ConvertFilterListToStr(List<Dictionary<string, string>> filterList)
        {
            var reportFilterString = "";
            int i = 1;
            if (filterList != null && filterList.Count > 0)
                foreach (var filter in filterList)
                {
                    try
                    {
                        foreach (var kvp in filter)
                        {
                            try
                            {
                                string key = kvp.Key;
                                string value = kvp.Value;
                                reportFilterString += i.ToString() + ") " + key + "=>" + value + "  \n";
                                i++;
                            }
                            catch (Exception ex)
                            {
                                WriteCrashLog(ex.ToString());
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        WriteCrashLog(ex.ToString());
                    }
                }
            return reportFilterString;
        }

        /// <summary>
        /// Formats a visual type string to a user-friendly format.
        /// </summary>
        /// <param name="visualType">Original visual type string.</param>
        /// <returns>Formatted visual type string.</returns>
        public static string? FormatVisualType(string? visualType)
        {
            if (visualType == null)
                return null;
            if (AppConfig.visualTypeFormatter.TryGetValue(visualType, out string? value))
                return value;

            string withSpaces = Regex.Replace(visualType, "(?<!^)([A-Z])", " $1");
            string result = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(withSpaces.ToLower());

            return result;
        }

        /// <summary>
        /// Converts theme data color JSON and color list to a HEX color string.
        /// </summary>
        /// <param name="themeDataColorObj">Theme data color JSON object.</param>
        /// <param name="dataColors">List of color HEX strings.</param>
        /// <returns>HEX color string or "null" if conversion fails.</returns>
        public static string? GetThemeDataColorToHex(JObject themeDataColorObj, List<string> dataColors)
        {
            try
            {
                // Convert the JSON into our model.
                ThemeDataColorModel? themeDataColorModel = themeDataColorObj.ToObject<ThemeDataColorModel>();
                if (themeDataColorModel == null)
                    return "null";
                if (dataColors == null)
                    return "null";

                string colorHex = themeDataColorModel.ColorId == 0 ? "#FFFFFF" : "#000000";
                if (themeDataColorModel.ColorId > 1)
                {
                    int index = themeDataColorModel.ColorId - 2;
                    if (dataColors.Count > index)
                        colorHex = dataColors[index];
                    else
                        return "null";
                }

                // Get the adjustment percentage (if 0, just return the base color).
                // Here the Percent is already a fraction (e.g. 0.25 or -0.25)
                float percent = themeDataColorModel.Percent;
                if (percent == 0)
                    return colorHex;

                var colorHsv = HexToHsv(colorHex);
                double finalH = colorHsv.H;
                double finalS = colorHsv.S;
                double finalV = colorHsv.V;

                // The adjustment fraction is the absolute value of percent.
                double fraction = Math.Abs(percent);

                if (percent > 0)
                {
                    // Lighter color adjustment:
                    // S(lighter) = S_specified * (1 - fraction)
                    // V(lighter) = V_specified * (1 - fraction) + 100 * fraction
                    finalS = colorHsv.S * (1 - fraction);
                    finalV = (colorHsv.V * (1 - fraction)) + (100 * fraction);
                }
                else // percent < 0, darker color
                {
                    // Darker color adjustment:
                    // V(darker) = V_specified * (1 - fraction)
                    finalV = colorHsv.V * (1 - fraction);
                    // Hue and Saturation remain unchanged.
                }

                // Convert the adjusted HSV back to a HEX color string.
                var finalHex = HsvToHex(finalH, finalS, finalV);

                return finalHex;
            }
            catch (Exception ex)
            {
                // For production use, replace with your logging mechanism.
                // Console.WriteLine("Error: " + ex.ToString());
                return "null";
            }
        }

        /// <summary>
        /// Converts a HEX color string to HSV.
        /// </summary>
        /// <param name="hex">HEX color string.</param>
        /// <returns>Tuple of H, S, V values.</returns>
        public static (double H, double S, double V) HexToHsv(string hex)
        {
            if (hex.StartsWith("#"))
                hex = hex.Substring(1);
            if (hex.Length != 6)
                throw new ArgumentException("Invalid HEX color");

            int r = int.Parse(hex.Substring(0, 2), NumberStyles.HexNumber);
            int g = int.Parse(hex.Substring(2, 2), NumberStyles.HexNumber);
            int b = int.Parse(hex.Substring(4, 2), NumberStyles.HexNumber);

            double rNorm = r / 255.0;
            double gNorm = g / 255.0;
            double bNorm = b / 255.0;

            double max = Math.Max(rNorm, Math.Max(gNorm, bNorm));
            double min = Math.Min(rNorm, Math.Min(gNorm, bNorm));
            double delta = max - min;

            double h = 0;
            if (delta > 0)
            {
                if (max == rNorm)
                {
                    h = 60 * (((gNorm - bNorm) / delta) % 6);
                }
                else if (max == gNorm)
                {
                    h = 60 * (((bNorm - rNorm) / delta) + 2);
                }
                else
                {
                    h = 60 * (((rNorm - gNorm) / delta) + 4);
                }
            }
            if (h < 0)
                h += 360;

            double s = (max == 0) ? 0 : (delta / max) * 100;
            double v = max * 100;

            return (h, s, v);
        }

        /// <summary>
        /// Converts HSV values to a HEX color string.
        /// </summary>
        /// <param name="h">Hue.</param>
        /// <param name="s">Saturation.</param>
        /// <param name="v">Value.</param>
        /// <returns>HEX color string.</returns>
        public static string HsvToHex(double h, double s, double v)
        {
            double sFraction = s / 100.0;
            double vFraction = v / 100.0;

            double c = vFraction * sFraction;
            double x = c * (1 - Math.Abs((h / 60.0) % 2 - 1));
            double m = vFraction - c;

            double rPrime = 0,
                gPrime = 0,
                bPrime = 0;
            if (h < 60)
            {
                rPrime = c;
                gPrime = x;
                bPrime = 0;
            }
            else if (h < 120)
            {
                rPrime = x;
                gPrime = c;
                bPrime = 0;
            }
            else if (h < 180)
            {
                rPrime = 0;
                gPrime = c;
                bPrime = x;
            }
            else if (h < 240)
            {
                rPrime = 0;
                gPrime = x;
                bPrime = c;
            }
            else if (h < 300)
            {
                rPrime = x;
                gPrime = 0;
                bPrime = c;
            }
            else
            {
                rPrime = c;
                gPrime = 0;
                bPrime = x;
            }

            int r = (int)Math.Round((rPrime + m) * 255);
            int g = (int)Math.Round((gPrime + m) * 255);
            int b = (int)Math.Round((bPrime + m) * 255);

            return $"#{r:X2}{g:X2}{b:X2}";
        }

        /// <summary>
        /// Extracts partition source query details such as applied steps, SQL databases, and tables used.
        /// </summary>
        /// <param name="sourceQuery">Source query string.</param>
        /// <param name="databaseModel">Tabular model.</param>
        /// <param name="partition">Partition object.</param>
        /// <returns>List containing applied steps, SQL databases, and tables used.</returns>
        public static List<object> getPartitionSourceQueryDetails(string sourceQuery, Model databaseModel, Partition partition)
        {
            List<Dictionary<string, string>> appliedMQuerySteps = [];
            HashSet<string> sqlDatabase = [];
            HashSet<string> tableUsedInQuery = [];

            string[] regexForSource1 = { @"\bSQL\.Database\b", @"\bSharePoint\.Files\b", @"\bBinary\.FromText\b", @"\bCSV\.Document\b", @"\bExcel\.Workbook\b", @"\bJSON\.Document\b" };
            // string regexForSource = @"\b(Excel\.Workbook|CSV\.Document|Binary\.FromText|SharePoint\.Files|SQL\.Database|JSON\.Document)\b";

            foreach (string regexForSource in regexForSource1)
            {
                try
                {
                    Match matchForSource = Regex.Match(sourceQuery, regexForSource, RegexOptions.IgnoreCase);

                    string patternToGetSourceLocation = @"""(.*?)"""; // Get Path of source For eg. Get Link of sharepoint, excel, csv

                    Regex regexToGetSourceLocation = new(patternToGetSourceLocation);
                    Match getSourceLocation = regexToGetSourceLocation.Match(sourceQuery);

                    if (getSourceLocation.Success && matchForSource.Success)
                    {
                        if (matchForSource.Value != "Sql.Database")
                        {
                            sqlDatabase.Add("(Source: " + matchForSource.Value + " : " + getSourceLocation.Groups[1].Value + ")");
                            break;
                        }
                        else if (matchForSource.Value == "Sql.Database")
                        {
                            string patternToGetSQLSourceName = @"Sql\.Database\(([^,]+),\s*([^,]+)\)";
                            MatchCollection SQLSourceNameCollection = Regex.Matches(sourceQuery, patternToGetSQLSourceName, RegexOptions.IgnoreCase);

                            foreach (Match SQLSourceName in SQLSourceNameCollection)
                            {
                                try
                                {
                                    string serverName = SQLSourceName.Groups[1].Value;
                                    string dbName = SQLSourceName.Groups[2].Value;

                                    if (serverName.StartsWith('\"') && serverName.EndsWith('\"') || serverName.StartsWith('\'') && serverName.EndsWith('\''))
                                    {
                                        // If it is, prefix the console output with "Comma : "
                                        sqlDatabase.Add($"Server: {serverName}, DB: {dbName}");
                                    }
                                    else
                                    {
                                        string currentServer = "";
                                        string currentDatabase = "";
                                        foreach (NamedExpression exp in databaseModel.Expressions)
                                        {
                                            try
                                            {
                                                string regexExpressionToGetDatabaseName = @"^""([^""]+)""";
                                                Regex regex = new(regexExpressionToGetDatabaseName);

                                                Match match = regex.Match(exp.Expression);
                                                if (exp.Name == serverName)
                                                {
                                                    currentServer = match.Groups[1].Value;
                                                }

                                                if (exp.Name == dbName)
                                                {
                                                    currentDatabase = match.Groups[1].Value;
                                                }

                                                if (currentServer != "" && currentDatabase != "")
                                                {
                                                    sqlDatabase.Add("Server: " + currentServer + " Database: " + currentDatabase);
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                WriteCrashLog(ex.ToString());
                                            }
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    WriteCrashLog(ex.ToString());
                                }
                            }
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    WriteCrashLog(ex.ToString());
                }
            }

            string regexNavigationTableName = @"Schema\s*=\s*""([^""]+)""\s*,\s*Item\s*=\s*""([^""]+)""";
            Match patternMatch = Regex.Match(sourceQuery, regexNavigationTableName);
            if (patternMatch.Success)
            {
                string schema = patternMatch.Groups[1].Value;
                string item = patternMatch.Groups[2].Value;
                string result = $"{schema}.{item}";
                tableUsedInQuery.Add(result);
            }

            if (partition.SourceType == PartitionSourceType.M)
            {
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
                        WriteCrashLog(ex.ToString());
                    }
                }
            }

            string regexExpressionToGetAppliedResult = @"(""[^""]+"")\s*=\s*(Table\.[^\n]+)";
            MatchCollection appliedStepsMatch = Regex.Matches(sourceQuery, regexExpressionToGetAppliedResult, RegexOptions.IgnoreCase);
            foreach (Match match in appliedStepsMatch)
            {
                try
                {
                    string stepName = match.Groups[1].Value.Trim('\"').Trim();
                    string stepExpression = match.Groups[2].Value;
                    if (stepExpression.EndsWith(','))
                        stepExpression = stepExpression[..^1];
                    appliedMQuerySteps.Add(new Dictionary<string, string>() { { "stepName", stepName }, { "method", stepExpression } });
                }
                catch (Exception ex)
                {
                    WriteCrashLog(ex.ToString());
                }
            }

            return [appliedMQuerySteps.Cast<object>(), sqlDatabase.Cast<object>(), tableUsedInQuery.Cast<object>()];
        }

        public static string GetArtifactCacheKey(string userEmail, string workspaceId, string artifactId)
        {
            return $@"{userEmail}.{workspaceId}.{artifactId}";
        }

        public static string GetTraceName(string userEmail, string reportId)
        {
            return $@"{userEmail.Split(".")[0]}_{reportId}";
        }

        public static string ExcelCellValueFormatting(string cellValue)
        {
            if (cellValue.Length > 32766)
                cellValue = cellValue[..32766];
            return cellValue;
        }
        
        public static string SplitCamelCase(string str)
        {
            return Regex.Replace(Regex.Replace(str, @"(\P{Ll})(\P{Ll}\p{Ll})", "$1 $2"), @"(\p{Ll})(\P{Ll})", "$1 $2");
        }

        public static void CreateExcelExportFiles(string exportFile, List<DataSet> dbs, List<string> exportAreas)
        {
            using XLWorkbook wb = new();
            foreach (DataSet db in dbs)
            {
                try
                {
                    if (db != null)
                        foreach (DataTable tbl in db.Tables)
                        {
                            try
                            {
                                foreach (DataRow row in tbl.Rows)
                                {
                                    try
                                    {
                                        foreach (System.Data.DataColumn col in tbl.Columns)
                                        {
                                            try
                                            {
                                                if (row[col] is string cellValue && cellValue.Length > 32767)
                                                {
                                                    row[col] = cellValue[..32766]; // Truncate to 32,767 characters
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                WriteCrashLog(ex.ToString());
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        WriteCrashLog(ex.ToString());
                                    }
                                }
                                XLColor tabColor = XLColor.FromHtml("#00b33c");
                                if (tbl.TableName == "Insights Summary" || tbl.TableName.StartsWith("I_"))
                                {
                                    tabColor = XLColor.FromHtml("#ff471a");
                                }
                                else if (tbl.TableName == "Unused Fields")
                                {
                                    tabColor = XLColor.FromHtml("#0099ff");
                                }
                                else if (tbl.TableName == "Alignment Summary" || tbl.TableName.StartsWith("A_"))
                                {
                                    tabColor = XLColor.FromHtml("#00004d");
                                }
                                else if (tbl.TableName == "CPUStatistics")
                                {
                                    tabColor = XLColor.FromHtml("#b994ff");
                                }
                                else if (tbl.TableName == "Documentation Summary" || tbl.TableName.StartsWith("D_"))
                                {
                                    tabColor = XLColor.FromHtml("#F8913C");
                                }
                                string sheetName = tbl.TableName;
                                if (sheetName.Length > 30)
                                    sheetName = sheetName[..30];
                                if (wb.Worksheets.Any(ws => ws.Name == sheetName))
                                {
                                    sheetName = sheetName[..25] + "_" + Guid.NewGuid().ToString().Split('-')[0];
                                    sheetName = sheetName[..30];
                                }
                                var ws = wb.Worksheets.Add(tbl, sheetName, tbl.TableName).SetTabColor(tabColor);
                                if (ws != null)
                                {
                                    if (tbl.TableName == "Insights Summary" || tbl.TableName == "Documentation Summary" || tbl.TableName == "Alignment Summary")
                                    {
                                        int lastRow = ws.LastRowUsed()?.RowNumber() ?? 0;
                                        for (int iRow = 1; iRow <= lastRow; iRow++)
                                        {
                                            try
                                            {
                                                if (exportAreas.Contains("insights") && tbl.TableName == "Insights Summary" && ws.Cell(iRow, 6).Value.ToString().ToLower().StartsWith("i_"))
                                                {
                                                    string cellValue = ws.Cell(iRow, 6).Value.ToString();
                                                    if (cellValue.Length > 30)
                                                        cellValue = cellValue[..30];
                                                    ws.Cell(iRow, 6).SetHyperlink(new XLHyperlink("'" + cellValue + "'!A1"));
                                                    ws.Cell(iRow, 6).SetValue("Click to view data");
                                                }
                                                if (tbl.TableName == "Alignment Summary" && ws.Cell(iRow, 4).Value.ToString().ToLower().StartsWith("a_"))
                                                {
                                                    ws.Cell(iRow, 4).SetHyperlink(new XLHyperlink("'" + ws.Cell(iRow, 4).Value.ToString() + "'!A1"));
                                                }
                                                if (tbl.TableName == "Documentation Summary" && ws.Cell(iRow, 3).Value.ToString().ToLower().StartsWith("d_"))
                                                {
                                                    ws.Cell(iRow, 3).SetHyperlink(new XLHyperlink("'" + ws.Cell(iRow, 3).Value.ToString() + "'!A1"));
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                WriteCrashLog(ex.ToString());
                                            }
                                        }
                                    }
                                    else if (tbl.TableName.StartsWith("I_") || tbl.TableName.StartsWith("D_") || tbl.TableName.StartsWith("A_"))
                                    {
                                        ws.Range("A1:AA1").InsertRowsAbove(2);
                                        ws.Columns("A").Width = 40;
                                        ws.Columns("A").Style.Font.FontSize = 12;
                                        ws.Cell(1, 1).SetValue("Go Back To " + (tbl.TableName.StartsWith("I_") ? "Insights Summary" :
                                                                                tbl.TableName.StartsWith("D_") ? "Documentation Summary" : "Alignment Summary"));
                                        ws.Cell(1, 1).SetHyperlink(new XLHyperlink("'" + (tbl.TableName.StartsWith("I_") ? "Insights Summary" :
                                                                                            tbl.TableName.StartsWith("D_") ? "Documentation Summary" : "Alignment Summary") + "'!A1"));
                                        ws.Cell(1, 1).Style.Font.SetFontName("Segoe UI");
                                        ws.Cell(1, 1).Style.Font.SetFontSize(11);
                                    }

                                    IXLTable wsTable = ws.Table(tbl.TableName);
                                    wsTable.Theme = XLTableTheme.TableStyleLight8;
                                    wsTable.Style.Border.SetRightBorder(XLBorderStyleValues.Thin);
                                    wsTable.Style.Border.SetLeftBorder(XLBorderStyleValues.Thin);
                                    wsTable.Style.Border.SetTopBorder(XLBorderStyleValues.Thin);
                                    wsTable.Style.Border.SetBottomBorder(XLBorderStyleValues.Thin);
                                    wsTable.Style.Font.SetFontName("Segoe UI");
                                    wsTable.Style.Font.SetFontSize(11);
                                    wsTable.Style.Alignment.WrapText = true;

                                    ws.ColumnsUsed().AdjustToContents();
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.ToString());
                                WriteCrashLog(ex.ToString());
                            }
                        }
                }
                catch (Exception ex)
                {
                    WriteCrashLog(ex.ToString());
                }
            }

            wb.SaveAs(exportFile);
        }
    }
}