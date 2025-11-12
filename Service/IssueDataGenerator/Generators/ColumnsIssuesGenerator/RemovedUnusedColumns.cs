using PowerBI_MCP.DTO;

namespace PowerBI_MCP.Service.IssueDataGenerator.Generators.ColumnsIssuesGenerator
{
    public class RemovedUnusedColumns : IArtifactGroupIssueGenerator
    {
        private List<ReportDocumentation> reportUIDocs = [];
        
        void IArtifactGroupIssueGenerator.SetAssociatedReportsUIDoc(List<ReportDocumentation> reportUIDocs)
        {
            this.reportUIDocs = reportUIDocs;
        }


        SingleIssueRuleData IIssueDataGenerator.GetData(IssueContext issueContext)
        {
            ModelDocumentation modelDoc = issueContext.ModelDocumentation; 

            //Dictionary<string, bool> usedColumns = new();
            //foreach(var table in modelDoc.Tables)
            //{
            //    // calc or field parameter expression
            //    foreach(var dependency in table.FormattedExpressionAndDependency?.Dependency ?? [])
            //    {
            //        if (dependency.Type == DependencyType.Column && !usedColumns.ContainsKey(dependency.TableName +"."+dependency.FieldName))
            //            usedColumns.Add(dependency.TableName + "." + dependency.FieldName, true);
            //    }
            //    // refresh policy expression check
            //    foreach (var dependency in table.FormattedRefreshPolicyExpressionAndDependency?.Dependency ?? [])
            //    {
            //        if (dependency.Type == DependencyType.Column && !usedColumns.ContainsKey(dependency.TableName + "." + dependency.FieldName))
            //            usedColumns.Add(dependency.TableName + "." + dependency.FieldName, true);
            //    }

            //    // calc group items expression
            //    foreach (var calcGroup in table.CalculationGroups ?? [])
            //    {
            //        foreach (var dependency in calcGroup.Dependency ?? [])
            //        {
            //            if (dependency.Type == DependencyType.Column && !usedColumns.ContainsKey(dependency.TableName + "." + dependency.FieldName))
            //                usedColumns.Add(dependency.TableName + "." + dependency.FieldName, true);
            //        } 
            //    }
            //}

            //foreach (var col in modelDoc.Columns)
            //{
            //    // calc column expression
            //    foreach (var dependency in col.FormattedExpressionAndDependency?.Dependency ?? [])
            //    {
            //        if (dependency.Type == DependencyType.Column && !usedColumns.ContainsKey(dependency.TableName + "." + dependency.FieldName))
            //            usedColumns.Add(dependency.TableName + "." + dependency.FieldName, true);
            //    }
            //    // columns in relationships 
            //    if (modelDoc.ColumnsInRelationship.ContainsKey(col.TableName +"."+col.ColumnName) == true &&
            //        usedColumns.ContainsKey(col.TableName + "." + col.ColumnName) ==false)
            //    {
            //        usedColumns.Add(col.TableName + "." + col.ColumnName, true);
            //    }

            //    // sort by column 
            //    if (col.SortByColumn != null && col.SortByColumn.TableName != null && col.SortByColumn.ColumnName != null &&
            //        !usedColumns.ContainsKey(col.SortByColumn.TableName + "." + col.SortByColumn.ColumnName))
            //    {
            //        usedColumns.Add(col.SortByColumn.TableName + "." + col.SortByColumn.ColumnName, true);
            //    }

            //}
            //foreach(var h in modelDoc.Hierarchies)
            //{
            //    // columns in hierarchies
            //    foreach (var level in h.Levels ?? [])
            //    {
            //        if (level.ColumnName != null && h.TableName!= null  &&
            //            !usedColumns.ContainsKey(h.TableName + "." + level.ColumnName))
            //        {
            //            usedColumns.Add(h.TableName + "." + level.ColumnName, true);
            //        }
            //    }
            //}


            //foreach (var meas in modelDoc.Measures)
            //{
            //    // columns in meausres
            //    foreach (var dependency in meas.FormattedExpressionAndDependency?.Dependency ?? [])
            //    {
            //        if (dependency.Type == DependencyType.Column && !usedColumns.ContainsKey(dependency.TableName + "." + dependency.FieldName))
            //            usedColumns.Add(dependency.TableName + "." + dependency.FieldName, true);
            //    }
            //}

            //foreach (var role in modelDoc.SecurityRoles)
            //{
            //    // columns in security roles
            //    foreach (var dependency in role.FormattedExpressionAndDependency?.Dependency ?? [])
            //    {
            //        if (dependency.Type == DependencyType.Column && !usedColumns.ContainsKey(dependency.TableName + "." + dependency.FieldName))
            //            usedColumns.Add(dependency.TableName + "." + dependency.FieldName, true);
            //    }
            //}

            //foreach(var uiDoc in reportUIDocs)
            //{
            //    // columns in reports
            //    foreach (var colName in uiDoc.ColumnDictionary.Keys)
            //    {
            //        if (!usedColumns.ContainsKey(colName))
            //            usedColumns.Add(colName, true);
            //    }
            //}

            
            //List<object> data = modelDoc.Columns.Where(x =>
            //{
            //    if (usedColumns.ContainsKey(x.TableName + "." + x.ColumnName) == false)  // columns not present in used columns
            //    {
            //        return true;
            //    }
            //    return false;
            //}).Cast<object>().ToList();

            List<object> data = modelDoc.Columns.Where(x => x.IsUsed == false).Cast<object>().ToList();

            return new() { Issues = data, MaxIssuable = modelDoc.Columns.Count };
        }
    }
}