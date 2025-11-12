using System.Text.RegularExpressions;
using PowerBI_MCP.DTO;
using PowerBI_MCP.Handlers;
using PlatformSpellCheck;
using Microsoft.AnalysisServices.Tabular;
using System.Text;

namespace PowerBI_MCP.Handlers
{
    public class ComplianceHandler
    {
        private static readonly HashSet<string> oParenthesesToCheck = new() { "(", ")", "+", "-", "*", "/", "^", "&", "=", "==", "<", ">", ">=", "<=", "<>", "&&", "||", "," };
        private static readonly HashSet<string> operations = new() { "+", "-", "*", "/", "^", "&", "=", "==", "<", ">", ">=", "<=", "<>", "&&", "||" };
        private static readonly List<string> charsToExclude = new() { "(", ")", ",", "", "=", "==", "<", ">", ">=", "<=", "<>", "&&", "||", "{", "}" };
        private static readonly List<char> charsToCheckOnFirstListIElem = new() { '\'', '[', '(', '+', '-', '*', '/', '^', '&' };
        private static readonly List<string> functionListToNotSuggestVariable = new() { "CALCULATE", "SELECTEDVALUE", "BLANK()" };
        private static readonly List<string> funcToIgnoreInVariable = new() { "CALCULATE", "SELECTEDVALUE" };
        private static readonly Dictionary<string, List<int>> func = new()
        {
            // { "if", new List<int> (){1} },
            {
                "calculate",
                new List<int>() { 2 }
            },
            {
                "all",
                new List<int>() { 1 }
            },
            {
                "allcrossfiltered",
                new List<int>() { 1 }
            },
            {
                "allexcept",
                new List<int>() { 1 }
            },
            {
                "allnoblankrow",
                new List<int>() { 1 }
            },
            {
                "allselected",
                new List<int>() { 1 }
            },
            {
                "calculatetable",
                new List<int>() { 2 }
            },
            {
                "filter",
                new List<int>() { 1 }
            },
            {
                "keepfilters",
                new List<int>() { 1 }
            },
            {
                "removefilters",
                new List<int>() { 1 }
            },
            {
                "averagex",
                new List<int>() { 1 }
            },
            {
                "countax",
                new List<int>() { 1 }
            },
            // { "countrows", new List<int> (){1} },
            {
                "countx",
                new List<int>() { 1 }
            },
            {
                "maxx",
                new List<int>() { 1 }
            },
            {
                "minx",
                new List<int>() { 1 }
            },
            {
                "productx",
                new List<int>() { 1 }
            },
            {
                "sumx",
                new List<int>() { 1 }
            },
        };
        private static readonly SpellChecker spellChecker = new();
        public static string SplitCamelCase(string str)
        {
            return Regex.Replace(Regex.Replace(str, @"(\P{Ll})(\P{Ll}\p{Ll})", "$1 $2"), @"(\p{Ll})(\P{Ll})", "$1 $2");
        }
        public static List<string> GetUsageCount(string expression, Dictionary<string, List<string>> measureDetails)
        {
            List<string> presentIn = [];
 
            foreach (var kvp in measureDetails)
            {
                try
                {
                    if (ToLowerSpaceRemoved(kvp.Value[1]).Contains(ToLowerSpaceRemoved(expression)))
                        presentIn.Add(kvp.Key);
                }
                catch (Exception ex)
                {
                    GlobalHandler.WriteCrashLog(ex.ToString());
                }
            } 
            return presentIn;
        }
        public static string? GetCorrectSpelledString(string? inputStr)
        {
            if (inputStr == null)
            {
                return null;
            }
            List<string> suggestions;

            string pattern = @"[\s]+[:?.]";
            inputStr = Regex.Replace(inputStr, pattern, match => match.Value.TrimEnd());
            if (inputStr.Length == 0)
                return inputStr;

            string splittedText = SplitCamelCase(inputStr);
            splittedText = Regex.Replace(splittedText, pattern, match => match.Value.TrimEnd());
            string correctText = inputStr;
            try
            {
                foreach (var mistake in spellChecker.Check(splittedText))
                {
                    try
                    {
                        if (mistake.Length > 0 && (int)mistake.StartIndex + (int)mistake.Length < splittedText.Length)
                        {
                            string wrongWord = splittedText.Substring((int)mistake.StartIndex, (int)mistake.Length);
                            suggestions = spellChecker.Suggestions(wrongWord).ToList();
                            if (suggestions.Count > 0)
                                correctText = correctText.Replace(wrongWord, suggestions[0]);
                        }
                    }
                    catch (Exception ex)
                    {
                        GlobalHandler.WriteCrashLog(ex.ToString());
                    }
                }
                return correctText;
            }
            catch (Exception ex)
            {
                GlobalHandler.WriteCrashLog(ex.ToString());
            }
            return inputStr;
        }
        public static List<object> GetColumnsToBeHidden(ModelDocumentation modelDoc)
        {
            List<Dictionary<string, string>> columnsToBeHidden = [];
             
            foreach (ColumnSummary column in modelDoc.Columns ??[])
            {
                try
                {
                    // Find sort-by columns that are already hidden - these are internal/utility columns
                    // But only add them if they're not already in the list (to avoid duplicates)
                    if (column.SortByColumn != null && column.SortByColumn.Visible == "No")
                    {
                        // Check if this column is already in the list to avoid duplicates
                        bool alreadyAdded = columnsToBeHidden.Any(c => 
                            c["TableName"] == column.SortByColumn.TableName && 
                            c["ColumnName"] == column.SortByColumn.ColumnName);
                        
                        if (!alreadyAdded)
                        {
                            columnsToBeHidden.Add(
                                new()
                                {
                                    { "TableName", column.SortByColumn.TableName },
                                    { "ColumnName", column.SortByColumn.ColumnName },
                                    { "DataType", column.SortByColumn.DataType },
                                    { "Visible", column.SortByColumn.Visible }, // Include Visible property for frontend
                                }
                            );
                        }
                    }
                }
                catch (Exception ex)
                {
                    GlobalHandler.WriteCrashLog(ex.ToString());
                }
            }
                   

            foreach (RelationshipSummary relationship in modelDoc.Relationships ??[])
            {
                try
                {
                    var fromColumn = relationship.FromColumn;
                    var toColumn = relationship.ToColumn;


                    if (fromColumn != null && fromColumn.Visible == "No")
                    {
                        // Check if this column is already in the list to avoid duplicates
                        bool alreadyAdded = columnsToBeHidden.Any(c => 
                            c["TableName"] == fromColumn.TableName && 
                            c["ColumnName"] == fromColumn.ColumnName);
                        
                        if (!alreadyAdded)
                        {
                            columnsToBeHidden.Add(
                                new()
                                {
                                    { "TableName", fromColumn.TableName  ?? ""},
                                    { "ColumnName", fromColumn.ColumnName?? "" },
                                    { "DataType", fromColumn.DataType?.ToString() ?? "" },
                                    { "Visible", fromColumn.Visible ?? "" }, // Include Visible property for frontend
                                }
                            );
                        }
                    }
                    if (toColumn != null && toColumn.Visible == "No")
                    {
                        // Check if this column is already in the list to avoid duplicates
                        bool alreadyAdded = columnsToBeHidden.Any(c => 
                            c["TableName"] == toColumn.TableName && 
                            c["ColumnName"] == toColumn.ColumnName);
                        
                        if (!alreadyAdded)
                        {
                            columnsToBeHidden.Add(
                                new()
                                {
                                    { "TableName", toColumn.TableName?? "" },
                                    { "ColumnName", toColumn.ColumnName?? "" },
                                    { "DataType", toColumn.DataType?.ToString() ?? "" },
                                    { "Visible", toColumn.Visible ?? "" }, // Include Visible property for frontend
                                }
                            );
                        }
                    }
                }
                catch (Exception ex)
                {
                    GlobalHandler.WriteCrashLog(ex.ToString());
                }
            } 
            return columnsToBeHidden.Cast<object>().ToList();
        }


        public static List<object> GetMeasureNotInMeasureTables(ModelDocumentation modelDoc)
        {
            List<Dictionary<string, string>> measuresNotInMeasureTable = []; 
            foreach (TableSummary table in modelDoc.Tables ??[])
            {
                int hiddenColumns = (modelDoc.Columns??[]).Where(col=>col.TableName==table.TableName && col.Visible == "No").Count();
                if (hiddenColumns != table.Columns && table.Measures > 0)
                {
                    foreach (MeasureSummary measure in modelDoc.Measures ?? [])
                    {
                        if(measure.TableName != table.TableName) continue;
                        try
                        {
                            measuresNotInMeasureTable.Add(
                                new()
                                {
                                    { "Origin",measure.Origin ??""},
                                    { "ReportName", "" },
                                    { "TableName", measure.TableName ??"" },
                                    { "MeasureName", measure.MeasureName  ??""},
                                    { "Expression", measure.Expression ??"" },
                                    { "Folder", measure.Folder ??"" },
                                }
                            );
                        }
                        catch (Exception ex)
                        {
                            GlobalHandler.WriteCrashLog(ex.ToString());
                        }
                    }
                }
            }
            return measuresNotInMeasureTable.Cast<object>().ToList();
        }
        public static string ToLowerSpaceRemoved(string expression)
        {
            return expression.Trim().ToLower();
        }
        public static List<object> GetDuplicateMeasures(Dictionary<string, Dictionary<string, string>> measureDetails)
        {
            List<Dictionary<string, string>> result = [];
            Dictionary<string, string> baseExpression = [];
            Dictionary<string, HashSet<string>> duplicateMeasures = [];
 
            foreach (var outerkvp in measureDetails)
            {
                try
                {
                    string outerExpression = ToLowerSpaceRemoved(outerkvp.Value["expression"]);
                    if (!baseExpression.ContainsKey(outerExpression))
                        baseExpression.Add(outerExpression, outerkvp.Value["expression"]);
                    foreach (var innerkvp in measureDetails)
                    {
                        try
                        {
                            string innerExpression = ToLowerSpaceRemoved(innerkvp.Value["expression"]);
                            if (outerkvp.Key != innerkvp.Key && (outerExpression.Equals(innerExpression) || innerExpression.Equals(ToLowerSpaceRemoved($"[{outerkvp.Key}]"))))
                            {
                                if (!duplicateMeasures.ContainsKey(outerExpression))
                                    duplicateMeasures.Add(outerExpression, [outerkvp.Key, innerkvp.Key]);
                                else
                                    duplicateMeasures[outerExpression].Add(innerkvp.Key);
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
            foreach (var kvp in duplicateMeasures)
            {
                try
                {
                    result.Add(
                        new()
                        {
                            { "Expression", baseExpression[kvp.Key] },
                            { "Count", kvp.Value.Count.ToString() },
                            { "MeasureNames", string.Join(", ", kvp.Value) },
                        }
                    );
                }
                catch (Exception ex)
                {
                    GlobalHandler.WriteCrashLog(ex.ToString());
                }
            }
            
            return result.Cast<object>().ToList();
        }
        public static List<object> GetDuplicateCalculatedColumns(ModelDocumentation modelDoc,Dictionary<string, Dictionary<string, string>> calculatedColumns)
        {
            List<Dictionary<string, string>> result = [];
            Dictionary<string, string> baseExpression = [];
            Dictionary<string, HashSet<string>> duplicateColumns = []; 

            foreach (ColumnSummary column in modelDoc.Columns ??[])
            { 
                foreach (var kvp in calculatedColumns)
                {
                    try
                    {
                        var expression = ToLowerSpaceRemoved(kvp.Value["expression"]);
                        if (!baseExpression.ContainsKey(expression))
                            baseExpression.Add(expression, kvp.Value["expression"]);
                        if (expression.Equals($"'{column.TableName?.ToLower() ??""}'[{column.ColumnName?.Trim()?.ToLower() ?? ""}]"))
                        {
                            if (!duplicateColumns.ContainsKey(expression))
                            {
                                duplicateColumns.Add(expression, [kvp.Key, column.ColumnName ??""]);
                            }
                            else
                            {
                                duplicateColumns[expression].Add(column.ColumnName ?? "");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        GlobalHandler.WriteCrashLog(ex.ToString());
                    }
                }
                
            } 
            foreach (var outerkvp in calculatedColumns)
            {
                try
                {
                    var outerExpression = ToLowerSpaceRemoved(outerkvp.Value["expression"]);
                    foreach (var innerkvp in calculatedColumns)
                    {
                        try
                        {
                            var innerExpression = ToLowerSpaceRemoved(innerkvp.Value["expression"]);
                            if (innerkvp.Key != outerkvp.Key && innerExpression.Equals(outerExpression))
                            {
                                if (!duplicateColumns.ContainsKey(outerExpression)) duplicateColumns.Add(outerExpression, [outerkvp.Key, innerkvp.Key]);
                                
                                else duplicateColumns[outerExpression].Add(innerkvp.Key); 
                            }
                        }
                        catch (Exception ex){ GlobalHandler.WriteCrashLog(ex.ToString()); }
                    }
                }
                catch (Exception ex)
                {
                    GlobalHandler.WriteCrashLog(ex.ToString());
                }
            }

            foreach (var kvp in duplicateColumns)
            {
                try
                {
                    result.Add(
                        new()
                        {
                            { "Expression", baseExpression[kvp.Key] },
                            { "Duplicate Count", kvp.Value.Count.ToString() },
                            { "Columns", string.Join(", ", kvp.Value) },
                        }
                    );
                }
                catch (Exception ex)
                {
                    GlobalHandler.WriteCrashLog(ex.ToString());
                }
            }
        
            return result.Cast<object>().ToList();
        }
        public static List<object> GetFactTables(ModelDocumentation modelDoc, Dictionary<string, Dictionary<string, string>> measureDetails)
        {
            List<Dictionary<string, string>> unhiddenFactTable = [];
            HashSet<string> tablesUsedInMeasures = [];
            HashSet<string> manySideTables = [];
 
                string pattern = @"'[^']*'";

                foreach (var measure in measureDetails.Keys)
                {
                    try
                    {
                        MatchCollection matches = Regex.Matches(measureDetails[measure]["expression"], pattern);

                        foreach (Match match in matches)
                        {
                            try
                            {
                                string tableName = match.Groups[0].Value.Split("[")[0];
                                if (tableName.Length > 1)
                                    tablesUsedInMeasures.Add(tableName[1..(tableName.Length - 1)]);
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

                foreach (var relationship in modelDoc.Relationships ?? [])
                {
                    try
                    {
                        if ((relationship.FromColumn != null && (relationship.FromColumn.TableName??"").Contains("LocalDateTable_")) ||
                        (relationship.ToColumn != null && (relationship.ToColumn.TableName??"").Contains("LocalDateTable_")))
                        {
                            continue;
                        }
                        else
                        {
                            if (relationship.FromCardinality == RelationshipEndCardinality.Many.ToString() && relationship.FromColumn != null)
                            {
                                manySideTables.Add(relationship.FromColumn.TableName??"" );
                            }
                            if (relationship.ToCardinality == RelationshipEndCardinality.Many.ToString()  && relationship.ToColumn != null)
                            {
                                manySideTables.Add(relationship.ToColumn.TableName ?? "");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        GlobalHandler.WriteCrashLog(ex.ToString());
                    }
                }

                foreach (string table in manySideTables)
                {
                    try
                    {
                        TableSummary? tableSummary = modelDoc.Tables?.FirstOrDefault(t => t.TableName == table);
                        if (tableSummary != null && tableSummary.Visible == "No" && tableSummary.TableName != null && tablesUsedInMeasures.Contains(tableSummary.TableName))
                        {
                            unhiddenFactTable.Add(new() { { "TableName", tableSummary.TableName } });
                        }
                    }
                    catch (Exception ex)
                    {
                        GlobalHandler.WriteCrashLog(ex.ToString());
                    }
                }
          
            return unhiddenFactTable.Cast<object>().ToList();
        }
        public static string StandardizeDax(string dax)
        {
            string temp = "";
            int inlineComments = 0;
            int multilineComments = 0;
            int singleQuote = 0;
            int squareBracket = 0;
            int doubleQuotes = 0;
            int doubleHyphen = 0;
            int length = dax.Length;
            for (int i = 0; i < length; i++)
            {
                try
                {
                    if (i + 1 < length && dax[i] == '/' && dax[i + 1] == '/' && singleQuote == 0 && squareBracket == 0 && doubleQuotes == 0 && inlineComments == 0)
                    {
                        inlineComments += 1;
                        i++;
                    }
                    else if (i + 1 < length && dax[i] == '/' && dax[i + 1] == '/' && inlineComments > 0)
                    {
                        i++;
                    }
                    else if (dax[i] == '\n' && inlineComments > 0)
                    {
                        inlineComments -= 1;
                    }
                    else if (inlineComments > 0)
                    {
                        continue;
                    }
                    else if (i + 1 < length && dax[i] == '-' && dax[i + 1] == '-' && singleQuote == 0 && squareBracket == 0 && doubleQuotes == 0 && inlineComments == 0 && doubleHyphen == 0)
                    {
                        doubleHyphen += 1;
                        i++;
                    }
                    else if (i + 1 < length && dax[i] == '-' && dax[i + 1] == '-' && doubleHyphen > 0)
                    {
                        i++;
                    }
                    else if (dax[i] == '\n' && doubleHyphen > 0)
                    {
                        doubleHyphen -= 1;
                    }
                    else if (doubleHyphen > 0)
                    {
                        continue;
                    }
                    else if (i + 1 < length && dax[i] == '/' && dax[i + 1] == '*' && singleQuote == 0 && squareBracket == 0 && doubleQuotes == 0)
                    {
                        multilineComments += 1;
                        i++;
                    }
                    else if (i + 1 < length && dax[i] == '*' && dax[i + 1] == '/' && multilineComments > 0)
                    {
                        multilineComments -= 1;
                        i++;
                    }
                    else if (multilineComments > 0)
                    {
                        continue;
                    }
                    else if (dax[i] == '\'' && singleQuote == 0)
                    {
                        singleQuote += 1;
                        temp += dax[i];
                    }
                    else if (dax[i] == '\'')
                    {
                        singleQuote -= 1;
                        temp += dax[i];
                    }
                    else if (singleQuote > 0)
                    {
                        temp += dax[i];
                    }
                    else if (dax[i] == '[' && squareBracket == 0)
                    {
                        squareBracket += 1;
                        temp += dax[i];
                    }
                    else if (i + 1 < length && dax[i] == ']' && dax[i + 1] == ']')
                    {
                        temp += dax[i];
                        temp += dax[++i];
                    }
                    else if (dax[i] == ']')
                    {
                        squareBracket -= 1;
                        temp += dax[i];
                    }
                    else if (squareBracket > 0)
                    {
                        temp += dax[i];
                    }
                    else if (dax[i] == '"' && doubleQuotes == 0)
                    {
                        doubleQuotes += 1;
                        temp += dax[i];
                    }
                    else if (dax[i] == '"')
                    {
                        doubleQuotes -= 1;
                        temp += dax[i];
                    }
                    else if (doubleQuotes > 0)
                    {
                        temp += dax[i];
                    }
                    else if (dax[i] == ' ')
                    {
                        continue;
                    }
                    else
                    {
                        temp += dax[i];
                    }
                }
                catch (Exception ex)
                {
                    GlobalHandler.WriteCrashLog(ex.ToString());
                }
            }
            return dax.Replace("\n", "");
        }
        public static Dictionary<String, String> SplitVariables(string expr)
        {
            string pattern = @"VAR\s+(?<varName>\w+)\s*=\s*(?<varValue>.*?)(?:;|$)";

            Regex regex = new Regex(pattern, RegexOptions.Singleline);
            Dictionary<String, String> variables = new();

            MatchCollection matches = regex.Matches(expr);

            foreach (Match match in matches)
            {
                try
                {
                    string varName = match.Groups["varName"].Value.Trim();
                    string varValue = match.Groups["varValue"].Value.Trim();
                    variables[varName] = varValue;
                }
                catch (Exception ex)
                {
                    GlobalHandler.WriteCrashLog(ex.ToString());
                }
            }
            return variables;
        }
        public static List<KeyValuePair<string, int>> SplitMeasureExpressionOnFunction(string expr, int exprStartingIndex)
        {
            List<string> list = new();
            List<KeyValuePair<string, int>> finalList = new();
            int exprLength = expr.Length;
            int bracketCount = 0;
            int outerBraceCount = 0;
            int singleQuoteCount = 0;
            int doubleQuoteCount = 0;
            int curlyBracesCount = 0;
            int pos = 0;
            int squareBracketCount = 0;
            string temp = "";
            for (int i = 0; i < exprLength; i++)
            {
                try
                {
                    char ch = expr[i];

                    if (ch == '{' && curlyBracesCount == 0 && (bracketCount != 0 || outerBraceCount != 0 || squareBracketCount != 0 || doubleQuoteCount != 0))
                    {
                        curlyBracesCount++;
                    }
                    if (ch == '}' && curlyBracesCount != 0 && (bracketCount != 0 || outerBraceCount != 0 || squareBracketCount != 0 || doubleQuoteCount != 0))
                    {
                        curlyBracesCount--;
                    }
                    if (ch == '{' && curlyBracesCount == 0 && bracketCount == 0 && outerBraceCount == 0 && squareBracketCount == 0 && doubleQuoteCount == 0)
                    {
                        curlyBracesCount++;
                        list.Insert(pos++, temp.Trim());
                        finalList.Insert(pos - 1, new KeyValuePair<string, int>(temp.Trim(), i + exprStartingIndex - temp.Trim().Length));
                        finalList.Insert(pos, new KeyValuePair<string, int>(ch.ToString(), i + exprStartingIndex));
                        list.Insert(pos++, ch.ToString());
                        temp = "";
                        continue;
                    }
                    if (ch == '}' && curlyBracesCount != 0 && bracketCount == 0 && outerBraceCount == 0 && squareBracketCount == 0 && doubleQuoteCount == 0)
                    {
                        curlyBracesCount--;
                        list.Insert(pos++, temp.Trim());
                        finalList.Insert(pos - 1, new KeyValuePair<string, int>(temp.Trim(), i + exprStartingIndex - temp.Trim().Length));
                        finalList.Insert(pos, new KeyValuePair<string, int>(ch.ToString(), i + exprStartingIndex));
                        list.Insert(pos++, ch.ToString());
                        temp = "";
                        continue;
                    }
                    if (ch == '\"' && doubleQuoteCount == 0)
                    {
                        doubleQuoteCount++;
                        temp += ch;
                        continue;
                    }
                    if (ch == '\"' && doubleQuoteCount != 0)
                    {
                        doubleQuoteCount--;
                        temp += ch;
                        continue;
                    }

                    if (ch == '\'' && singleQuoteCount == 0)
                    {
                        singleQuoteCount++;
                        temp += ch;
                        continue;
                    }
                    if (ch == '\'' && singleQuoteCount != 0)
                    {
                        singleQuoteCount--;
                        temp += ch;
                        continue;
                    }
                    if (ch == '[')
                    {
                        squareBracketCount++;
                    }
                    if (ch == ']' && squareBracketCount != 0)
                    {
                        squareBracketCount--;
                    }
                    if (ch == '(' && bracketCount == 0 && outerBraceCount == 0 && squareBracketCount == 0 && doubleQuoteCount == 0)
                    {
                        list.Insert(pos++, temp.Trim());
                        finalList.Insert(pos - 1, new KeyValuePair<string, int>(temp.Trim(), i + exprStartingIndex - temp.Trim().Length));
                        finalList.Insert(pos, new KeyValuePair<string, int>(ch.ToString(), i + exprStartingIndex));
                        list.Insert(pos++, ch.ToString());
                        temp = "";
                        outerBraceCount++;
                        continue;
                    }
                    if (ch == '(' && list.Count == 0 && bracketCount == 0 && outerBraceCount == 0 && doubleQuoteCount == 0 && squareBracketCount == 0)
                    {
                        temp += ch.ToString();
                        outerBraceCount++;
                        continue;
                    }
                    if (ch == ')' && bracketCount == 0 && outerBraceCount != 0 && list.Count > 0 && squareBracketCount == 0 && doubleQuoteCount == 0)
                    {
                        list.Insert(pos++, temp.Trim());
                        finalList.Insert(pos - 1, new KeyValuePair<string, int>(temp.Trim(), i + exprStartingIndex - temp.Trim().Length));
                        finalList.Insert(pos, new KeyValuePair<string, int>(ch.ToString(), i + exprStartingIndex));
                        list.Insert(pos++, ch.ToString());
                        temp = "";
                        outerBraceCount--;
                        continue;
                    }
                    if (ch == ')' && bracketCount == 0 && outerBraceCount != 0 && list.Count == 0 && squareBracketCount == 0 && doubleQuoteCount == 0)
                    {
                        temp += ch.ToString();
                        list.Insert(pos++, temp.Trim());
                        finalList.Insert(pos - 1, new KeyValuePair<string, int>(temp.Trim(), i + exprStartingIndex - temp.Trim().Length));
                        temp = "";
                        outerBraceCount--;
                        continue;
                    }
                    if (ch == '(' && outerBraceCount != 0)
                    {
                        temp += ch;
                        bracketCount++;
                        continue;
                    }
                    if (ch == ')' && outerBraceCount != 0 && bracketCount != 0)
                    {
                        temp += ch;
                        bracketCount--;
                        continue;
                    }
                    if (
                        (
                            (ch == ',' && bracketCount == 0 && (outerBraceCount == 1 && curlyBracesCount == 0 || outerBraceCount == 0 && curlyBracesCount == 1))
                            || (
                                (ch == '|' || ch == '&' || ch == '>' || ch == '<' || ch == '=' || ch == '^' || ch == '+' || ch == '*' || ch == '-' || ch == '/')
                                && bracketCount == 0
                                && outerBraceCount == 0
                                && doubleQuoteCount == 0
                                && singleQuoteCount == 0
                            )
                        ) && (temp.Length >= 0 && temp != expr && squareBracketCount == 0)
                    )
                    {
                        if (ch == '|' && i != exprLength - 1 && expr[i + 1] == '|' && bracketCount == 0 && outerBraceCount == 0)
                        {
                            list.Insert(pos++, temp.Trim());
                            finalList.Insert(pos - 1, new KeyValuePair<string, int>(temp.Trim(), i + exprStartingIndex - temp.Trim().Length));
                            finalList.Insert(pos, new KeyValuePair<string, int>(ch + expr[i + 1].ToString(), i + exprStartingIndex - (ch + expr[i + 1].ToString()).Length));
                            list.Insert(pos++, ch + expr[i + 1].ToString());
                            i++;
                            temp = "";
                            continue;
                        }
                        if (ch == '&' && i != exprLength - 1 && expr[i + 1] == '&' && bracketCount == 0 && outerBraceCount == 0)
                        {
                            list.Insert(pos++, temp.Trim());
                            finalList.Insert(pos - 1, new KeyValuePair<string, int>(temp.Trim(), i + exprStartingIndex - temp.Trim().Length));
                            finalList.Insert(pos, new KeyValuePair<string, int>(ch + expr[i + 1].ToString(), i + exprStartingIndex - (ch + expr[i + 1].ToString()).Length));
                            list.Insert(pos++, ch + expr[i + 1].ToString());
                            i++;
                            temp = "";
                            continue;
                        }
                        if (ch == '=' && i != exprLength - 1 && expr[i + 1] == '=' && bracketCount == 0 && outerBraceCount == 0)
                        {
                            list.Insert(pos++, temp.Trim());
                            finalList.Insert(pos - 1, new KeyValuePair<string, int>(temp.Trim(), i + exprStartingIndex - temp.Trim().Length));
                            finalList.Insert(pos, new KeyValuePair<string, int>(ch + expr[i + 1].ToString(), i + exprStartingIndex - (ch + expr[i + 1].ToString()).Length));
                            list.Insert(pos++, ch + expr[i + 1].ToString());
                            i++;
                            temp = "";
                            continue;
                        }
                        else if (ch == '>' && i != exprLength - 1 && expr[i + 1] == '=' && bracketCount == 0 && outerBraceCount == 0)
                        {
                            list.Insert(pos++, temp.Trim());
                            finalList.Insert(pos - 1, new KeyValuePair<string, int>(temp.Trim(), i + exprStartingIndex - temp.Trim().Length));
                            finalList.Insert(pos, new KeyValuePair<string, int>(ch + expr[i + 1].ToString(), i + exprStartingIndex - (ch + expr[i + 1].ToString()).Length));
                            list.Insert(pos++, ch + expr[i + 1].ToString());
                            i++;
                            temp = "";
                            continue;
                        }
                        else if (ch == '<' && i != exprLength - 1 && expr[i + 1] == '=' && bracketCount == 0 && outerBraceCount == 0)
                        {
                            list.Insert(pos++, temp.Trim());
                            finalList.Insert(pos - 1, new KeyValuePair<string, int>(temp.Trim(), i + exprStartingIndex - temp.Trim().Length));
                            finalList.Insert(pos, new KeyValuePair<string, int>(ch + expr[i + 1].ToString(), i + exprStartingIndex - (ch + expr[i + 1].ToString()).Length));
                            list.Insert(pos++, ch + expr[i + 1].ToString());
                            i++;
                            temp = "";
                            continue;
                        }
                        else if (ch == '<' && i != exprLength - 1 && expr[i + 1] == '>' && bracketCount == 0 && outerBraceCount == 0)
                        {
                            list.Insert(pos++, temp.Trim());
                            finalList.Insert(pos - 1, new KeyValuePair<string, int>(temp.Trim(), i + exprStartingIndex - temp.Trim().Length));
                            finalList.Insert(pos, new KeyValuePair<string, int>(ch + expr[i + 1].ToString(), i + exprStartingIndex - (ch + expr[i + 1].ToString()).Length));
                            list.Insert(pos++, ch + expr[i + 1].ToString());
                            i++;
                            temp = "";
                            continue;
                        }
                        else
                        {
                            list.Insert(pos++, temp.Trim());
                            finalList.Insert(pos - 1, new KeyValuePair<string, int>(temp.Trim(), i + exprStartingIndex - temp.Trim().Length));
                            finalList.Insert(pos, new KeyValuePair<string, int>(ch.ToString(), i + exprStartingIndex));
                            list.Insert(pos++, ch.ToString());
                            temp = "";
                            continue;
                        }
                    }
                    else
                    {
                        temp += ch;
                    }
                }
                catch (Exception ex)
                {
                    GlobalHandler.WriteCrashLog(ex.ToString());
                }
            }
            if (temp.Length != 0)
            {
                list.Insert(pos++, temp.Trim());
                finalList.Insert(pos - 1, new KeyValuePair<string, int>(temp.Trim(), expr.Length + exprStartingIndex - temp.Trim().Length));
            }

            return finalList;
        }
        public static List<KeyValuePair<string, int>> GetFilteredParametersOfExpr(string expr, int exprStartingIndex)
        {
            List<KeyValuePair<string, int>> exprList = SplitMeasureExpressionOnFunction(expr, exprStartingIndex);

            exprList = exprList.Where(x => !"".Contains(x.Key)).ToList();

            bool containsAny = exprList.Select(x => x.Key).Intersect(operations).Any();

            if (containsAny)
            {
                List<KeyValuePair<string, int>> tempExprList = new();
                int i = 0;
                string tempExpr = "";
                int lastItemIDX = 0;
                int cnt = exprList.Count();
                bool isFirstElem = true;
                for (i = 0; i < cnt; i++)
                {
                    try
                    {
                        if (!operations.Contains(exprList[i].Key))
                        {
                            if (isFirstElem)
                            {
                                lastItemIDX = exprList[i].Value;
                                isFirstElem = false;
                            }
                            tempExpr += exprList[i].Key;
                        }
                        else
                        {
                            tempExprList.Add(new KeyValuePair<String, int>(tempExpr, lastItemIDX));
                            //lastItemIDX = exprList[i].Value;
                            isFirstElem = true;
                            tempExpr = "";
                        }
                    }
                    catch (Exception ex)
                    {
                        GlobalHandler.WriteCrashLog(ex.ToString());
                    }
                }
                tempExprList.Add(new KeyValuePair<String, int>(tempExpr, lastItemIDX));
                exprList.AddRange(tempExprList);

                return tempExprList;
            }
            else
            {
                exprList = exprList.Where(x => !charsToExclude.Contains(x.Key)).ToList();
                string stItem = exprList.Count > 0 ? exprList[0].Key : "";
                bool isFunction = true;
                foreach (var chars in charsToCheckOnFirstListIElem)
                {
                    try
                    {
                        if (stItem.Contains(chars))
                        {
                            //Console.WriteLine("Char is: " + chars);
                            isFunction = false;
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        GlobalHandler.WriteCrashLog(ex.ToString());
                    }
                }

                if (exprList.Count > 0 && func.ContainsKey(stItem.ToLower()) && func[stItem.ToLower()][0] < exprList.Count)
                {
                    int filterPosLen = func[stItem.ToLower()].Count;
                    if (filterPosLen == 1)
                    {
                        int pos = func[stItem.ToLower()][0];
                        exprList.RemoveAt(pos);
                    }
                    else if (filterPosLen > 1)
                    {
                        foreach (var pos in func[stItem.ToLower()])
                        {
                            try
                            {
                                exprList.RemoveRange(func[stItem.ToLower()][0], exprList.Count - func[stItem.ToLower()][0]);
                            }
                            catch (Exception ex)
                            {
                                GlobalHandler.WriteCrashLog(ex.ToString());
                            }
                        }
                    }
                    if (exprList.Count > 0 && isFunction)
                    {
                        exprList.RemoveAt(0);
                        isFunction = false;
                    }
                    return exprList;
                }
                if (exprList.Count > 1 && isFunction)
                {
                    exprList.RemoveAt(0);
                }

                return exprList;
            }
        }
        public static Dictionary<string, ExpressionTree> GetMeasureSubExpressionsTreeWithoutFilterContext(Dictionary<string, List<string>> measureList)
        {
            Dictionary<string, ExpressionTree> measuresTree = new();
            foreach (var measure in measureList)
            {
                try
                {
                    Dictionary<string, string> variables = new();
                    if (measure.Value[0].Length > 0)
                    {
                        variables = SplitVariables(measure.Value[0]);
                    }

                    ExpressionTree root = new(measure.Value[1], 0, measureList);
                    Queue<ExpressionTree> queue = new();
                    queue.Enqueue(root);
                    while (queue.Count != 0)
                    {
                        ExpressionTree parent = queue.Dequeue();
                        int pos = parent.nodePosition;
                        List<KeyValuePair<string, int>> tempList = GetFilteredParametersOfExpr(parent.data, pos);
                        if (tempList.Count == 1)
                        {
                            string elem = tempList[0].Key;
                            int elemLen = elem.Length;
                            bool containsOptrs = false;
                            bool isMeasure = false;
                            foreach (var oprt in operations)
                            {
                                try
                                {
                                    if (elem.Contains(oprt) && (elem[0] != '[' || elem[0] != '\'') && elem[elemLen - 1] != ']')
                                    {
                                        containsOptrs = true;
                                        break;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    GlobalHandler.WriteCrashLog(ex.ToString());
                                }
                            }
                            if ((elem[0] == '[' && elem[elemLen - 1] == ']') || (elem[0] == '\'' && elem[elemLen - 1] == ']'))
                            {
                                if (elem[0] == '[' && elem[elemLen - 1] == ']')
                                {
                                    string splittedElem = elem.Substring(1, elemLen - 2);
                                    if (measureList.ContainsKey(splittedElem))
                                    {
                                        isMeasure = true;
                                    }
                                }
                                else if (elem[0] == '\'' && elem[elemLen - 1] == ']')
                                {
                                    int idxOfsbct = elem.IndexOf('[');
                                    string splittedElem = elem.Substring(idxOfsbct + 1, elemLen - idxOfsbct - 2);
                                    if (measureList.ContainsKey(splittedElem))
                                    {
                                        isMeasure = true;
                                    }
                                }
                            }
                            if (!isMeasure && !containsOptrs && ((elem[0] == '\'' && elem[elemLen - 1] == '\'') || (elem[0] == '\'' && elem[elemLen - 1] == ']')))
                            {
                                tempList.Clear();
                            }
                            else
                            {
                                if (!isMeasure && !containsOptrs && (!elem.Contains('\'') && !elem.Contains('[') && !elem.Contains(']') && !elem.Contains('(') && !elem.Contains(')')))
                                {
                                    tempList.Clear();
                                }
                            }
                        }
                        if (tempList.Count != 1)
                        {
                            foreach (var i in tempList)
                            {
                                try
                                {
                                    if (variables.ContainsKey(i.Key))
                                    {
                                        ExpressionTree root1 = ExpressionTree.Insert(root, parent, new ExpressionTree(variables[i.Key], i.Value, parent, measureList));
                                        queue.Enqueue(root1);
                                    }
                                    else if (variables.Keys.Any(t => i.Key.Contains(t)))
                                    {
                                        string temp = i.Key;
                                        int tempPos = i.Value;
                                        foreach (var key in variables.Keys)
                                        {
                                            try
                                            {
                                                if (i.Key.Contains(key, StringComparison.OrdinalIgnoreCase))
                                                {
                                                    temp = temp.Replace(key, variables[key], StringComparison.OrdinalIgnoreCase);
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                GlobalHandler.WriteCrashLog(ex.ToString());
                                            }
                                        }
                                        ExpressionTree root1 = ExpressionTree.Insert(root, parent, new ExpressionTree(temp, i.Value, parent, measureList));
                                        queue.Enqueue(root1);
                                    }
                                    else
                                    {
                                        ExpressionTree root1 = ExpressionTree.Insert(root, parent, new ExpressionTree(i.Key, i.Value, parent, measureList));
                                        queue.Enqueue(root1);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    GlobalHandler.WriteCrashLog(ex.ToString());
                                }
                            }
                        }
                        else if (tempList.Count == 1 && root.data != tempList[0].Key && parent.data != tempList[0].Key)
                        {
                            if (variables.ContainsKey(tempList[0].Key))
                            {
                                ExpressionTree root1 = ExpressionTree.Insert(root, parent, new ExpressionTree(variables[tempList[0].Key], tempList[0].Value, parent, measureList));
                                queue.Enqueue(root1);
                            }
                            else if (variables.Keys.Any(key => tempList[0].Key.Contains(key)))
                            {
                                string temp = tempList[0].Key;
                                int tempPos = tempList[0].Value;
                                foreach (var key in variables.Keys)
                                {
                                    try
                                    {
                                        if (tempList[0].Key.Contains(key))
                                        {
                                            // This will be used for replacing the variable in the expression
                                            // string pattern = $@"(?<!\w)([\[\(\{{\'\""\,\ ]?){Regex.Escape(key)}([\]\)\}}\'\""\,\ ]?)(?!\w)";
                                            // temp = Regex.Replace(temp, pattern, m => $"{m.Groups[1].Value}{variables[key]}{m.Groups[2].Value}");
                                            temp = temp.Replace(key, variables[key], StringComparison.OrdinalIgnoreCase);
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        GlobalHandler.WriteCrashLog(ex.ToString());
                                    }
                                }
                                ExpressionTree root1 = ExpressionTree.Insert(root, parent, new ExpressionTree(temp, tempPos, parent, measureList));
                                queue.Enqueue(root1);
                            }
                            else
                            {
                                ExpressionTree root1 = ExpressionTree.Insert(root, parent, new ExpressionTree(tempList[0].Key, tempList[0].Value, parent, measureList));
                                queue.Enqueue(root1);
                            }
                        }
                    }

                    if (variables.Count > 0 && variables.Keys.Any(key => root.data.Contains(key)))
                    {
                        string tempRootData = root.data;
                        foreach (var key in variables.Keys)
                        {
                            try
                            {
                                if (tempRootData.Contains(key))
                                {
                                    tempRootData = tempRootData.Replace(key, variables[key]);
                                }
                            }
                            catch (Exception ex)
                            {
                                GlobalHandler.WriteCrashLog(ex.ToString());
                            }
                        }
                        root.data = tempRootData;
                    }
                    measuresTree.Add(measure.Key, root);
                }
                catch (Exception ex)
                {
                    GlobalHandler.WriteCrashLog(ex.ToString());
                }
            }

            return measuresTree;
        }
        public static Dictionary<string, ExpressionTree> GenerateExpression(Dictionary<string, Dictionary<string, string>> measureDetails)
        {
            Dictionary<string, List<string>> splitMeasuresList = new();
          
            foreach (var measures in measureDetails)
            {
                try
                {
                    List<string> values = new();
                    int index = measures.Value["expression"].IndexOf("return", StringComparison.OrdinalIgnoreCase);
                    if (index >= 0)
                    {
                        continue;
                    }
                    else
                    {
                        values.Add("");
                        values.Add(StandardizeDax(measures.Value["expression"]));
                        splitMeasuresList.Add(measures.Key.Trim(), values);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("In Catch: " + ", Measure Name: " + measures.Key + ", Exception: " + ex.Message);
                }
            }

            Dictionary<string, ExpressionTree> tree = GetMeasureSubExpressionsTreeWithoutFilterContext(splitMeasuresList);

            return tree;  
        }
        public static List<object> GetSeparateExpressions(Dictionary<string, Dictionary<string, string>> measureDetails)
        {
            List<Dictionary<string, string>> expressions = [];
            Dictionary<string, List<string>> separateExpressions = new();
            Dictionary<string, ExpressionTree> tree = GenerateExpression(measureDetails);
            foreach (var i in tree)
            {
                try
                {
                    ExpressionTree.GetSeparateExpressions(i.Value, separateExpressions);
                }
                catch (Exception ex)
                {
                    GlobalHandler.WriteCrashLog(ex.ToString());
                }
            }
            foreach (var exp in separateExpressions)
            {
                try
                {
                    if (
                        (exp.Key.Contains("()") && !exp.Key.Contains(","))
                        || (exp.Key.StartsWith("\"") && exp.Key.EndsWith("\""))
                        || (exp.Key.Contains("=") && !exp.Key.Contains(","))
                        || exp.Key.StartsWith("[")
                        || (exp.Key.StartsWith("'") && exp.Key.EndsWith("'"))
                        || exp.Key.All(Char.IsLetter)
                        || double.TryParse(exp.Key, out double number)
                        || (exp.Key.StartsWith("'") && exp.Key.EndsWith("]"))
                        || (exp.Key.Contains("IN", StringComparison.OrdinalIgnoreCase) && exp.Key.Contains("{") && exp.Key.Trim().EndsWith("}"))
                        || (exp.Key.StartsWith("ALL(", StringComparison.OrdinalIgnoreCase) && exp.Key.EndsWith(")"))
                        || (exp.Key.StartsWith("ALLSELECTED(", StringComparison.OrdinalIgnoreCase) && exp.Key.EndsWith(")"))
                        || (exp.Key.StartsWith("ALLEXCEPT(", StringComparison.OrdinalIgnoreCase) && exp.Key.EndsWith(")"))
                        || (exp.Key.StartsWith("USERELATIONSHIP(", StringComparison.OrdinalIgnoreCase) && exp.Key.EndsWith(")"))
                        || (exp.Key.StartsWith("TREATAS(", StringComparison.OrdinalIgnoreCase) && exp.Key.EndsWith(")"))
                    )
                    {
                        separateExpressions.Remove(exp.Key);
                    }
                }
                catch (Exception ex)
                {
                    GlobalHandler.WriteCrashLog(ex.ToString());
                }
            }

            foreach (var outer in separateExpressions)
            {
                try
                {
                    foreach (var inner in separateExpressions)
                    {
                        try
                        {
                            if (outer.Key == inner.Key)
                            {
                                continue;
                            }
                            if (outer.Key.Contains(inner.Key, StringComparison.OrdinalIgnoreCase))
                            {
                                foreach (var e in outer.Value)
                                {
                                    try
                                    {
                                        if (inner.Value.Contains(e))
                                        {
                                            inner.Value.Remove(e);
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
                    GlobalHandler.WriteCrashLog(ex.ToString());
                }
            }

            foreach (var sep in separateExpressions)
            {
                try
                {
                    if (sep.Value.Count <= 1)
                    {
                        separateExpressions.Remove(sep.Key);
                    }
                }
                catch (Exception ex)
                {
                    GlobalHandler.WriteCrashLog(ex.ToString());
                }
            }

            bool expressionExists = false;
            string newValue = "";
            Dictionary<string, List<string>> existingExpressions = new Dictionary<string, List<string>>();
            foreach (var measure in measureDetails)
            {
                try
                {
                    expressionExists = false;
                    foreach (string exp in separateExpressions.Keys)
                    {
                        try
                        {
                            if (ToLowerSpaceRemoved(exp) == ToLowerSpaceRemoved(measure.Value["expression"]))
                            {
                                newValue = exp;
                                expressionExists = true;
                                break;
                            }
                        }
                        catch (Exception ex)
                        {
                            GlobalHandler.WriteCrashLog(ex.ToString());
                        }
                    }
                    if (expressionExists)
                    {
                        if (!existingExpressions.ContainsKey(newValue))
                        {
                            existingExpressions.Add(newValue, new() { measure.Key });
                        }
                        else
                        {
                            existingExpressions[newValue].Add(measure.Key);
                        }

                        if (separateExpressions[newValue].Contains(measure.Key))
                        {
                            separateExpressions[newValue].Remove(measure.Key);
                        }
                    }
                }
                catch (Exception ex)
                {
                    GlobalHandler.WriteCrashLog(ex.ToString());
                }
            }
            foreach (var exp in existingExpressions)
            {
                try
                {
                    if (separateExpressions.ContainsKey(exp.Key))
                    {
                        existingExpressions[exp.Key].AddRange(separateExpressions[exp.Key]);

                        separateExpressions.Remove(exp.Key);
                    }
                }
                catch (Exception ex)
                {
                    GlobalHandler.WriteCrashLog(ex.ToString());
                }
            }

            foreach (var kvp in separateExpressions)
            {
                try
                {
                    expressions.Add(
                        new()
                        {
                            { "Expression", kvp.Key.Trim() },
                            { "Count", kvp.Value.Count.ToString() },
                            { "Measures", string.Join(", ", kvp.Value) },
                            { "BaseMeasure", "No Base Measures Exists" },
                        }
                    );
                }
                catch (Exception ex)
                {
                    GlobalHandler.WriteCrashLog(ex.ToString());
                }
            }
            foreach (var kvp in existingExpressions)
            {
                try
                {
                    if (kvp.Value.Count > 1)
                    {
                        expressions.Add(
                            new()
                            {
                                { "Expression", kvp.Key.Trim() },
                                { "Count", (kvp.Value.Count).ToString() },
                                { "Measures", string.Join(", ", kvp.Value) },
                                { "BaseMeasure", kvp.Value[0] },
                            }
                        );
                    }
                }
                catch (Exception ex)
                {
                    GlobalHandler.WriteCrashLog(ex.ToString());
                }
            }

            return expressions.Cast<object>().ToList();
        }
        static string RemoveSpacesAroundOperators(string input)
        {
            // First, capture content inside [], '', and "" to skip replacing operators inside them
            List<string> preservedSections = new List<string>();

            // Regex to match content inside [], '', and "", escaping the double quotes
            input = Regex.Replace(
                input,
                @"(\[[^\]]*\]|'.*?'|\""[^\""]*\"")",
                match =>
                {
                    preservedSections.Add(match.Value); // Save the matched part
                    return $"##PRESERVED{preservedSections.Count - 1}##"; // Replace with a placeholder
                }
            );

            // Now, remove spaces around operators outside [], '', and ""
            foreach (var oprt in oParenthesesToCheck)
            {
                try
                {
                    input = Regex.Replace(input, $@"\s*{Regex.Escape(oprt)}\s*", oprt);
                }
                catch (Exception ex)
                {
                    GlobalHandler.WriteCrashLog(ex.ToString());
                }
            }

            // Restore the preserved sections (inside [], '', and "")
            for (int i = 0; i < preservedSections.Count; i++)
            {
                try
                {
                    input = input.Replace($"##PRESERVED{i}##", preservedSections[i]);
                }
                catch (Exception ex)
                {
                    GlobalHandler.WriteCrashLog(ex.ToString());
                }
            }
            return input;
        }
        static bool ValidateVariable(string input)
        {
            string pattern = @"^[0-9\.\W\+\-\*/%]+$";
            return Regex.IsMatch(input, pattern);
        }

        public static bool IsWrappedWithQuotes(string input)
        {
            string pattern = @"^(['""])(.*)\1$";
            return Regex.IsMatch(input, pattern);
        }
        public static bool IsRootContainsOperatorOutsideBracketsOrQuotes(string input)
        {
            int roundBracketCount = 0; // For ()
            int squareBracketCount = 0; // For []
            bool insideSingleQuote = false;
            bool insideDoubleQuote = false;

            for (int i = 0; i < input.Length; i++)
            {
                try
                {
                    char currentChar = input[i];

                    // Toggle flag for being inside single quotes
                    if (currentChar == '\'' && !insideDoubleQuote)
                    {
                        insideSingleQuote = !insideSingleQuote;
                    }
                    // Toggle flag for being inside double quotes
                    else if (currentChar == '"' && !insideSingleQuote)
                    {
                        insideDoubleQuote = !insideDoubleQuote;
                    }
                    // Skip if we're inside quotes
                    else if (!insideSingleQuote && !insideDoubleQuote)
                    {
                        // Check for opening/closing round brackets
                        if (currentChar == '(')
                        {
                            roundBracketCount++;
                        }
                        else if (currentChar == ')')
                        {
                            roundBracketCount--;
                        }
                        else if (currentChar == '[')
                        {
                            squareBracketCount++;
                        }
                        else if (currentChar == ']')
                        {
                            squareBracketCount--;
                        }

                        if (roundBracketCount == 0 && squareBracketCount == 0)
                        {
                            if (operations.Contains(currentChar.ToString()))
                            {
                                return true;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    GlobalHandler.WriteCrashLog(ex.ToString());
                }
            }

            return false;
        }
        public static int CountOccurrenceOfNodeInTree(ExpressionTree root, string nodeData)
        {
            int count = 0;
            Queue<ExpressionTree> queue = new();
            queue.Enqueue(root);
            while (queue.Count > 0)
            {
                int levelSize = queue.Count();
                for (int i = 0; i < levelSize; i++)
                {
                    try
                    {
                        ExpressionTree currentNode = queue.Dequeue();
                        bool currNodeFucCheck = false;

                        foreach (var fun in funcToIgnoreInVariable)
                        {
                            try
                            {
                                // Split root.data into tokens (by non-word characters)
                                var tokens = Regex.Split(currentNode.data, @"\W+");

                                // Check if any token exactly matches the function
                                if (tokens.Any(token => string.Equals(token, fun, StringComparison.OrdinalIgnoreCase)) && root.data != currentNode.data)
                                {
                                    currNodeFucCheck = true;
                                    break;
                                }
                            }
                            catch (Exception ex)
                            {
                                GlobalHandler.WriteCrashLog(ex.ToString());
                            }
                        }

                        /* foreach (var func in funcToIgnoreInVariable)
                        {
                            if (currentNode.data.Contains(func, StringComparison.OrdinalIgnoreCase) && root.data != currentNode.data)
                            {
                                currNodeFucCheck = true;
                                break;
                            }
                        } */

                        if (currNodeFucCheck)
                        {
                            continue;
                        }

                        if (currentNode.data == nodeData && !currNodeFucCheck)
                        {
                            //currentNode.nodeCount = currentNode.nodeCount + 1;
                            count++;
                        }

                        foreach (ExpressionTree child in currentNode.children)
                        {
                            try
                            {
                                queue.Enqueue(child);
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
            if (root.data == nodeData && count == 1)
            {
                return -1;
            }
            return count;
        }
        public static int GetRootChildCount(ExpressionTree root)
        {
            int count = 0;

            foreach (var child in root.children)
            {
                try
                {
                    count++;
                }
                catch (Exception ex)
                {
                    GlobalHandler.WriteCrashLog(ex.ToString());
                }
            }
            return count;
        }
        public static Dictionary<string, KeyValuePair<List<int>, List<int>>> ReturnDuplicateNodeUpd(ExpressionTree root)
        {
            //
            Dictionary<string, KeyValuePair<List<int>, List<int>>> nodeCount = new();
            try
            {
                if (root == null)
                {
                    Console.WriteLine("Empty Tree");
                    nodeCount.Add("Empty Tree", new KeyValuePair<List<int>, List<int>>(new List<int>() { -1, -1 }, new List<int>() { -1 }));
                    return nodeCount;
                }
                if (IsWrappedWithQuotes(root.data))
                {
                    nodeCount.Add(root.data.Trim(), new KeyValuePair<List<int>, List<int>>(new List<int>() { -1, -1 }, new List<int>() { -1 }));
                    return nodeCount;
                }

                Queue<ExpressionTree> queue = new();
                queue.Enqueue(root);

                nodeCount.Add(root.data.Trim(), new KeyValuePair<List<int>, List<int>>(new List<int>() { 1, 0 }, new List<int>() { root.nodePosition }));
                bool rootLevelCheck = false;
                foreach (var fun in funcToIgnoreInVariable)
                {
                    try
                    {
                        // Get the part of root.data before the first opening parenthesis
                        var firstWord = root.data.Split(new[] { '(', ' ' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();

                        // Check if the first word exactly matches the function
                        if (string.Equals(firstWord, fun, StringComparison.OrdinalIgnoreCase))
                        {
                            rootLevelCheck = true;
                            nodeCount[root.data.Trim()].Key[1] = -1;
                        }
                    }
                    catch (Exception ex)
                    {
                        GlobalHandler.WriteCrashLog(ex.ToString());
                    }
                }

                if (rootLevelCheck && !IsRootContainsOperatorOutsideBracketsOrQuotes(root.data))
                {
                    // Console.WriteLine("Root contains functions in ignorelist: " + root.data);
                    nodeCount.Add("Root contains functions in ignorelist", new KeyValuePair<List<int>, List<int>>(new List<int>() { -1, -1 }, new List<int>() { -1 }));
                    return nodeCount;
                    // continue;
                }
                int levelCnt = 1;

                while (queue.Count > 0)
                {
                    int levelSize = queue.Count();
                    for (int i = 0; i < levelSize; i++)
                    {
                        try
                        {
                            ExpressionTree currentNode = queue.Dequeue();
                            int nodeOccur = CountOccurrenceOfNodeInTree(root, currentNode.data);
                            currentNode.nodeCount = nodeOccur;
                            //if(nodeOccur )
                            //Console.WriteLine("Node: " +  currentNode.data + ", Node count: " + nodeOccur);

                            if (nodeOccur <= 0 && currentNode != root)
                            {
                                Console.WriteLine("Inside first continue: " + currentNode.data);
                                continue;
                            }

                            foreach (var fun in funcToIgnoreInVariable)
                            {
                                try
                                {
                                    // Split root.data into tokens (by non-word characters)
                                    var tokens = Regex.Split(currentNode.data, @"\W+");

                                    // Check if any token exactly matches the function
                                    if (tokens.Any(token => string.Equals(token, fun, StringComparison.OrdinalIgnoreCase)) && currentNode.data != root.data)
                                    {
                                        nodeCount[currentNode.data.Trim()].Key[1] = -1;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    GlobalHandler.WriteCrashLog(ex.ToString());
                                }
                            }

                            foreach (var child in currentNode.children)
                            {
                                try
                                {
                                    bool childCheck = false;

                                    foreach (var fun in funcToIgnoreInVariable)
                                    {
                                        try
                                        {
                                            // Split root.data into tokens (by non-word characters)
                                            var tokens = Regex.Split(child.data, @"\W+");

                                            // Check if any token exactly matches the function
                                            if (tokens.Any(token => string.Equals(token, fun, StringComparison.OrdinalIgnoreCase)) && child.data != root.data)
                                            {
                                                childCheck = true;
                                                break;
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            GlobalHandler.WriteCrashLog(ex.ToString());
                                        }
                                    }

                                    if (childCheck)
                                    {
                                        continue;
                                    }
                                    queue.Enqueue(child);

                                    if (!nodeCount.ContainsKey(currentNode.data.Trim()))
                                    {
                                        nodeCount.Add(currentNode.data.Trim(), new KeyValuePair<List<int>, List<int>>(new List<int>() { 1, 1 }, new List<int>() { currentNode.nodePosition }));
                                    }

                                    if (nodeCount.ContainsKey(child.data.Trim()))
                                    {
                                        nodeCount[child.data.Trim()].Key[0] = nodeCount[child.data.Trim()].Key[0] + 1;
                                        if (
                                            (currentNode.nodeCount <= CountOccurrenceOfNodeInTree(root, child.data) && currentNode != root)
                                            || (currentNode.nodeCount <= CountOccurrenceOfNodeInTree(root, child.data) && GetRootChildCount(root) > 1)
                                        )
                                        {
                                            if (!nodeCount[child.data.Trim()].Value.Contains(child.nodePosition))
                                            {
                                                nodeCount[child.data.Trim()].Value.Add(child.nodePosition);
                                            }
                                            nodeCount[currentNode.data.Trim()].Key[0] -= 1;
                                            nodeCount[currentNode.data.Trim()].Key[1] -= 1;
                                        }
                                        else
                                        {
                                            if (
                                                (!nodeCount[currentNode.data.Trim()].Value.Contains(currentNode.nodePosition) && currentNode != root)
                                                || (!nodeCount[currentNode.data.Trim()].Value.Contains(currentNode.nodePosition) && GetRootChildCount(root) > 1)
                                            )
                                            {
                                                nodeCount[currentNode.data.Trim()].Value.Add(currentNode.nodePosition);
                                            }
                                            nodeCount[child.data.Trim()].Key[0] -= 1;
                                            nodeCount[child.data.Trim()].Key[1] -= 1;
                                        }
                                    }
                                    else
                                    {
                                        if (
                                            (currentNode.nodeCount <= CountOccurrenceOfNodeInTree(root, child.data) && currentNode != root)
                                            || (currentNode.nodeCount <= CountOccurrenceOfNodeInTree(root, child.data) && GetRootChildCount(root) > 1)
                                        )
                                        {
                                            nodeCount.Add(child.data.Trim(), new KeyValuePair<List<int>, List<int>>(new List<int>() { 1, 1 }, new List<int>() { child.nodePosition }));
                                            nodeCount[currentNode.data.Trim()].Key[0] -= 1;
                                            nodeCount[currentNode.data.Trim()].Key[1] -= 1;
                                        }
                                        else
                                        {
                                            if (currentNode != root && !nodeCount.ContainsKey(currentNode.data.Trim()))
                                            {
                                                nodeCount.Add(currentNode.data.Trim(), new KeyValuePair<List<int>, List<int>>(new List<int>() { 1, 1 }, new List<int>() { currentNode.nodePosition }));
                                                nodeCount[child.data.Trim()].Key[0] -= 1;
                                                nodeCount[child.data.Trim()].Key[1] -= 1;
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
                            GlobalHandler.WriteCrashLog(ex.ToString());
                        }
                    }
                    levelCnt++;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e.ToString());
            }
            return nodeCount;
        }
        public static Dictionary<string, Dictionary<string, Dictionary<string, List<int>>>> SuggestVariable(Dictionary<string, List<string>> measureListTemp)
        {
            Dictionary<string, ExpressionTree> tree = GetMeasureSubExpressionsTreeWithoutFilterContext(measureListTemp);
            Dictionary<string, Dictionary<string, KeyValuePair<List<int>, List<int>>>> withNodeCount = new();
            Dictionary<string, List<KeyValuePair<string, List<int>>>> withVariables = new();
            Dictionary<string, List<string>> result = new();
            Dictionary<string, Dictionary<string, Dictionary<string, List<int>>>> output = new();

            foreach (var root in tree)
            {
                try
                {
                    Dictionary<string, KeyValuePair<List<int>, List<int>>> nodeCount = ReturnDuplicateNodeUpd(root.Value);
                    withNodeCount.Add(root.Key, nodeCount);
                }
                catch (Exception ex)
                {
                    GlobalHandler.WriteCrashLog(ex.ToString());
                }
            }

            foreach (var root in withNodeCount)
            {
                try
                {
                    List<KeyValuePair<string, List<int>>> varList = new();
                    //Console.WriteLine("Measure: " + root.Key);
                    foreach (var vars in root.Value)
                    {
                        try
                        {
                            if (
                                vars.Value.Key[0] > 0
                                && vars.Value.Key[1] >= 1
                                && !ValidateVariable(vars.Key)
                                && !IsWrappedWithQuotes(vars.Key)
                                && vars.Value.Value.Count > 1
                                && !functionListToNotSuggestVariable.Contains(vars.Key)
                                && vars.Key.Length > 0
                            )
                            {
                                varList.Add(new KeyValuePair<string, List<int>>(vars.Key, vars.Value.Value));
                            }
                        }
                        catch (Exception ex)
                        {
                            GlobalHandler.WriteCrashLog(ex.ToString());
                        }
                    }
                    if (varList.Count > 0)
                    {
                        withVariables.Add(root.Key, varList);
                    }
                }
                catch (Exception ex)
                {
                    GlobalHandler.WriteCrashLog(ex.ToString());
                }
            }
            if (withVariables.Count > 0)
            {
                foreach (var k in withVariables)
                {
                    try
                    {
                        List<string> variables = new();
                        Dictionary<string, Dictionary<string, List<int>>> originalExprVariableDetails = new();
                        Dictionary<string, List<int>> variableDetails = new();
                        foreach (var v in k.Value)
                        {
                            try
                            {
                                variables.Add(v.Key);
                                List<int> variablePositions = [.. v.Value];
                                variableDetails.Add(v.Key, variablePositions);
                            }
                            catch (Exception ex)
                            {
                                GlobalHandler.WriteCrashLog(ex.ToString());
                            }
                        }
                        originalExprVariableDetails.Add(measureListTemp[k.Key][1], variableDetails);
                        output.Add(k.Key, originalExprVariableDetails);

                        result.Add(k.Key, variables);
                    }
                    catch (Exception ex)
                    {
                        GlobalHandler.WriteCrashLog(ex.ToString());
                    }
                }
            }
            return output;
        }
        public static string SubExpressionHighlightFormatter(string text, Dictionary<string, List<int>> subexprs)
        {
            var finalStr = new StringBuilder();
            var updatedSubexprs = new Dictionary<string, List<int>>();

            foreach (var kvp in subexprs)
            {
                try
                {
                    var tempUKey = $"<span style=\"color:red;\">{kvp.Key}</span>";
                    updatedSubexprs[tempUKey] = kvp.Value;
                }
                catch (Exception ex)
                {
                    GlobalHandler.WriteCrashLog(ex.ToString());
                }
            }

            var positions = new List<(int position, string formattedExpr)>();

            foreach (var kvp in updatedSubexprs)
            {
                try
                {
                    foreach (var pos in kvp.Value)
                    {
                        try
                        {
                            positions.Add((pos, kvp.Key));
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

            positions.Sort((x, y) => x.position.CompareTo(y.position));

            int lastPosition = 0;

            foreach (var (position, formattedExpr) in positions)
            {
                try
                {
                    if (lastPosition < position)
                    {
                        finalStr.Append(text.Substring(lastPosition, position - lastPosition));
                    }
                    finalStr.Append(formattedExpr);
                    lastPosition = position + formattedExpr.Length - 32;
                }
                catch (Exception ex)
                {
                    GlobalHandler.WriteCrashLog(ex.ToString());
                }
            }

            if (lastPosition < text.Length)
            {
                finalStr.Append(text.Substring(lastPosition));
            }

            return finalStr.ToString();
        }
        public static List<object> GetVariableSuggestion(Dictionary<string, Dictionary<string, string>> measureDetails)
        {
            if (measureDetails.Count() == 0)
                return [];
            Dictionary<string, List<string>> splitMeasuresList = new();
            Dictionary<string, Dictionary<string, string>> splitMeasuresList2 = new();
        
            foreach (var measures in measureDetails)
            {
                try
                {
                    List<string> values = new();
                    string expression = RemoveSpacesAroundOperators(StandardizeDax(measures.Value["expression"]));

                    int index = expression.IndexOf("return", StringComparison.OrdinalIgnoreCase);
                    if (index >= 0)
                    {
                        continue;
                    }
                    else
                    {
                        values.Add("");
                        values.Add(expression);
                        splitMeasuresList.Add(measures.Key.Trim(), values);
                        splitMeasuresList2.Add(
                            measures.Key.Trim(),
                            new()
                            {
                                { "beforeReturnMeasureExpr", "" },
                                { "afterReturnMeasureExpr", expression },
                                { "table", measures.Value["table"] },
                                { "hidden", measures.Value["visible"] },
                                { "folder", measures.Value["folder"] },
                            }
                        );
                    }
                }
                catch (Exception ex)
                {
                    GlobalHandler.WriteCrashLog(ex.ToString());
                }
            }

            Dictionary<string, Dictionary<string, Dictionary<string, List<int>>>> variableDetails = SuggestVariable(splitMeasuresList);
            var finalResult = new Dictionary<string, Dictionary<string, List<string>>>();
            var finalResult2 = new Dictionary<string, Dictionary<string, string>>();

            foreach (var measures in variableDetails)
            {
                try
                {
                    if (!finalResult2.ContainsKey(measures.Key))
                    {
                        finalResult[measures.Key] = new Dictionary<string, List<string>>();
                        finalResult2[measures.Key] = new Dictionary<string, string>();
                    }

                    foreach (var expressions in measures.Value)
                    {
                        try
                        {
                            var updatedExpression = SubExpressionHighlightFormatter(expressions.Key, expressions.Value);

                            finalResult2[measures.Key].Add("updatedExpression", updatedExpression);
                            finalResult2[measures.Key].Add("expression", expressions.Key);
                            finalResult2[measures.Key].Add("variables", string.Join(", ", expressions.Value.Keys.ToList()));
                            finalResult2[measures.Key].Add("hidden", splitMeasuresList2[measures.Key]["hidden"]);
                            finalResult2[measures.Key].Add("tableName", splitMeasuresList2[measures.Key]["table"]);
                            finalResult2[measures.Key].Add("folder", splitMeasuresList2[measures.Key]["folder"]);

                            // Count of integers in the List<int> and join them by commas
                            var variableCounts = expressions.Value.Values.Select(list => list.Count).ToList();
                            finalResult2[measures.Key].Add("variableCount", string.Join(", ", variableCounts));
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

            return finalResult2
                .Select(x => new
                {
                    MeasureName = x.Key,
                    TableName = x.Value["tableName"],
                    VariableSuggestionExpression = x.Value["updatedExpression"],
                    VariableSuggestion = x.Value["variables"],
                    VariableCount = x.Value["variableCount"],
                    HiddenExpression = x.Value["expression"],
                    Visible = x.Value["hidden"],
                    Folder = x.Value["folder"],
                })
                .Cast<object>()
                .ToList();
         
        }
    }
}