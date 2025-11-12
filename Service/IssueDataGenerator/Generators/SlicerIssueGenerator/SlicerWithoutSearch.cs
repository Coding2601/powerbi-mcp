using PowerBI_MCP.DTO;
using PowerBI_MCP.Handlers;

namespace PowerBI_MCP.Service.IssueDataGenerator.Generators.SlicerIssueGenerator
{
    public class SlicerWithoutSearch : IIssueDataGenerator
    {
        public SingleIssueRuleData GetData(IssueContext issueContext)
        {
            var tempIssueData = issueContext.ReportDocumentation?.ReportUIFormatting.SlicersList.Where(x =>
            {
                if (x.IsSearchDisabled == false)
                    return false;
                // exclude slicers of date type
                if (x.Columns != null && x.Columns.Length > 0)
                {
                    var columns = x.Columns.Split(",");
                    foreach (var column in columns)
                    {
                        try
                        {
                            var actualColumn = column.Trim();
                            ColumnSummary? colData = issueContext.ModelDocumentation != null ? issueContext.ModelDocumentation?.Columns?.FirstOrDefault(x => x.TableName + "." + x.ColumnName == actualColumn) : null;
                            if (colData != null && colData.DataType != null && colData.DataType.ToLower().StartsWith("date") == true)
                                return false;
                            else if (column.ToLower().Contains("date"))
                                return false;
                        }
                        catch (Exception ex)
                        {
                            GlobalHandler.WriteCrashLog(ex.ToString());
                        }
                    }
                }
                if (x.Measures != null && x.Measures.Length > 0)
                {
                    var measures = x.Measures.Split(",");
                    foreach (var measure in measures)
                    {
                        try
                        {
                            var actualMeasure = measure.Trim();
                            MeasureSummary? measureData = issueContext.ModelDocumentation != null ? issueContext.ModelDocumentation?.Measures?.FirstOrDefault(x => x.MeasureName == actualMeasure) : null;
                            if (measureData != null && measureData.DataType != null && measureData.DataType.ToLower().StartsWith("date"))
                                return false;
                            else if (measure.ToLower().Contains("date"))
                                return false;
                        }
                        catch (Exception ex)
                        {
                            GlobalHandler.WriteCrashLog(ex.ToString());
                        }
                    }
                }
                return true;
            });

            List<object> issueData = [];
            foreach (var vis in tempIssueData ?? [])
            {
                try
                {
                    if (vis.VisualId != null && issueContext.ReportDocumentation?.VisualDataDictionary.ContainsKey(vis.VisualId) == true)
                        issueData.Add(issueContext.ReportDocumentation.VisualDataDictionary[vis.VisualId]);
                }
                catch (Exception ex)
                {
                    GlobalHandler.WriteCrashLog(ex.ToString());
                }
            }

            return new() { Issues = issueData, MaxIssuable = issueContext.ReportDocumentation?.ReportUIFormatting.SlicersList?.Count };
        }
    }
}