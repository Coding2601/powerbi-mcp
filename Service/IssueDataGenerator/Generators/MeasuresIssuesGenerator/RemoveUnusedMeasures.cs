using PowerBI_MCP.DTO;

namespace PowerBI_MCP.Service.IssueDataGenerator.Generators.MeasuresIssuesGenerator
{
    public class RemoveUnusedMeasures : IArtifactGroupIssueGenerator
    {
        private List<ReportDocumentation> reportUIDocs = [];
        void IArtifactGroupIssueGenerator.SetAssociatedReportsUIDoc(List<ReportDocumentation> reportUIDocs)
        {
            this.reportUIDocs = reportUIDocs;
        }

        SingleIssueRuleData IIssueDataGenerator.GetData(IssueContext issueContext)
        {
            ModelDocumentation modelDoc = issueContext.ModelDocumentation;

            //Dictionary<string, bool> usedMeasures = new();
            //foreach (var table in modelDoc.Tables)
            //{
            //    // calc or field parameter expression
            //    foreach (var dependency in table.FormattedExpressionAndDependency?.Dependency ?? [])
            //    {
            //        if (dependency.Type == DependencyType.Measure && !usedMeasures.ContainsKey(dependency.FieldName))
            //            usedMeasures.Add(dependency.FieldName, true);
            //    }
            //    // refresh policy expression check
            //    foreach (var dependency in table.FormattedRefreshPolicyExpressionAndDependency?.Dependency ?? [])
            //    {
            //        if (dependency.Type == DependencyType.Measure && !usedMeasures.ContainsKey(dependency.FieldName))
            //            usedMeasures.Add(dependency.FieldName, true);
            //    }

            //    // calc group items expression
            //    foreach (var calcGroup in table.CalculationGroups ?? [])
            //    {
            //        foreach (var dependency in calcGroup.Dependency ?? [])
            //        {
            //            if (dependency.Type == DependencyType.Measure && !usedMeasures.ContainsKey(dependency.FieldName))
            //                usedMeasures.Add(dependency.FieldName, true);
            //        }
            //    }
            //}


            //foreach (var meas in modelDoc.Measures)
            //{
            //    // measures in measures
            //    foreach (var dependency in meas.FormattedExpressionAndDependency?.Dependency ?? [])
            //    {
            //        if (dependency.Type == DependencyType.Measure && !usedMeasures.ContainsKey(dependency.FieldName))
            //            usedMeasures.Add(dependency.FieldName, true);
            //    }
            //}

            //foreach (var role in modelDoc.SecurityRoles)
            //{
            //    // measures in security roles
            //    foreach (var dependency in role.FormattedExpressionAndDependency?.Dependency ?? [])
            //    {
            //        if (dependency.Type == DependencyType.Measure && !usedMeasures.ContainsKey(dependency.FieldName))
            //            usedMeasures.Add(dependency.FieldName, true);
            //    }
            //}

            //foreach (var uiDoc in reportUIDocs)
            //{
            //    // measures in reports
            //    foreach (var measureName in uiDoc.MeasureDictionary.Keys)
            //    {
            //        if (!usedMeasures.ContainsKey(measureName))
            //            usedMeasures.Add(measureName, true);
            //    }
            //}


            //List<object> data = modelDoc.Measures.Where(x =>
            //{
            //    if (usedMeasures.ContainsKey(x.TableName + "." + x.MeasureName) == false)  // columns not present in used columns
            //    {
            //        return true;
            //    }
            //    return false;
            //}).Cast<object>().ToList();
            List<object> data = modelDoc.Measures.Where(x => x.IsUsed == false).Cast<object>().ToList();

            return new() { Issues = data, MaxIssuable = modelDoc.Measures.Count };
        }

    }
}