using PowerBI_MCP.DTO;

namespace PowerBI_MCP.Handlers
{
    public class UnusedHandler
    {
        private enum ModelFieldType
        {
            TABLE, COLUMN, MEASURE, HIERARCHY
        }
        private class ModelField : IEquatable<ModelField>
        {
            public ModelFieldType FieldType { get; set; } = ModelFieldType.COLUMN;
            public string TableName { get; set; } = "";
            public string Name { get; set; } = "";
            public string HierarchyLevel { get; set; } = "";

            public bool Equals(ModelField? other)
            {
                if (other is null) return false;
                if (ReferenceEquals(this, other)) return true;

                return FieldType == other.FieldType &&
                       TableName == other.TableName &&
                       Name == other.Name &&
                       HierarchyLevel == other.HierarchyLevel;
            }

        }


        private static class ModelFieldFactory
        {
            public static ModelField CreateFromVisualField(VisualUsedFields field)
            {
                return new ModelField
                {
                    FieldType = field.Type switch
                    {
                        VisualUsedFieldType.TABLE or VisualUsedFieldType.FIELD_PARAMETER => ModelFieldType.TABLE,
                        VisualUsedFieldType.COLUMN => ModelFieldType.COLUMN,
                        VisualUsedFieldType.MEASURE => ModelFieldType.MEASURE,
                        VisualUsedFieldType.HIERARCHY => ModelFieldType.HIERARCHY,
                        _ => ModelFieldType.HIERARCHY
                    },
                    TableName = field.TableName,
                    Name = field.FieldName,
                    HierarchyLevel = field.Hierarchy
                };
            }

            public static ModelField CreateFromDependency(ExpressionDependency dep)
            {
                return new ModelField
                {
                    FieldType = dep.Type switch
                    {
                        DependencyType.Table => ModelFieldType.TABLE,
                        DependencyType.Column => ModelFieldType.COLUMN,
                        DependencyType.Measure => ModelFieldType.MEASURE,
                        _ => ModelFieldType.HIERARCHY
                    },
                    TableName = dep.TableName,
                    Name = dep.FieldName,
                    HierarchyLevel = ""
                };
            }

            public static ModelField CreateFromColumn(string tableName, string colName)
            {
                return new ModelField
                {
                    FieldType = ModelFieldType.COLUMN,
                    TableName = tableName,
                    Name = colName,
                    HierarchyLevel = ""
                };
            }
        }


        public static ModelDocumentation Handle(ModelDocumentation modelDoc, List<ReportDocumentation> reportDocs)
        {

            HashSet<ModelField> usedFieldsInVis_Role_Rel_IncRef_sortByCol_hier = [];
            HashSet<ModelField> usedFieldsInUsedThings = []; // used in usedFieldsInVis_Role_Rel_IncRef_sortByCol_hier

            // visual
            foreach (var reportDoc in reportDocs)
            {
                // Visuals
                foreach (var visual in reportDoc.VisualList)
                {
                    foreach (var field in visual.UsedFields)
                    {
                        
                        usedFieldsInVis_Role_Rel_IncRef_sortByCol_hier.Add(ModelFieldFactory.CreateFromVisualField(field));
                    }
                }
            }
            //role
            foreach (var role in modelDoc.SecurityRoles)
            {
                foreach (var dep in role.FormattedExpressionAndDependency?.Dependency ?? [])
                {
                    usedFieldsInVis_Role_Rel_IncRef_sortByCol_hier.Add(ModelFieldFactory.CreateFromDependency(dep));
                }
            }
            // increamental refresh expression
            foreach (var table in modelDoc.Tables)
            {
                foreach (var dep in table.FormattedRefreshPolicyExpressionAndDependency?.Dependency ?? [])
                {
                    usedFieldsInVis_Role_Rel_IncRef_sortByCol_hier.Add(ModelFieldFactory.CreateFromDependency(dep));
                }
            }
            // relationship
            foreach (var rel in modelDoc.Relationships)
            {
                usedFieldsInVis_Role_Rel_IncRef_sortByCol_hier.Add(ModelFieldFactory.CreateFromColumn(rel.FromColumn?.TableName ?? "", rel.FromColumn?.ColumnName ?? ""));
                usedFieldsInVis_Role_Rel_IncRef_sortByCol_hier.Add(ModelFieldFactory.CreateFromColumn(rel.ToColumn?.TableName ?? "", rel.ToColumn?.ColumnName ?? ""));
            }
            // Hierarchy
            foreach (var h in modelDoc.Hierarchies)
            {
                foreach (var lev in h.Levels ?? [])
                {
                    usedFieldsInVis_Role_Rel_IncRef_sortByCol_hier.Add(ModelFieldFactory.CreateFromColumn(h.TableName, lev.ColumnName));
                }
            }
            // sort by column
            foreach (var col in modelDoc.Columns)
            {
                if (col.SortByColumn?.ColumnName?.Length > 0)
                {
                    usedFieldsInVis_Role_Rel_IncRef_sortByCol_hier.Add(ModelFieldFactory.CreateFromColumn(col.SortByColumn?.TableName ?? "", col.SortByColumn?.ColumnName ?? ""));
                }
            }

            HashSet<ModelField> usedFieldsInCalc_Tbl_Col_Measures = [];

            foreach (var measure in modelDoc.Measures)
            {
                foreach (var dep in measure.FormattedExpressionAndDependency?.Dependency ?? [])
                {
                    usedFieldsInCalc_Tbl_Col_Measures.Add(ModelFieldFactory.CreateFromDependency(dep));
                }
            }

            foreach (var col in modelDoc.Columns)
            {
                foreach (var dep in col.FormattedExpressionAndDependency?.Dependency ?? [])
                {
                    usedFieldsInCalc_Tbl_Col_Measures.Add(ModelFieldFactory.CreateFromDependency(dep));
                }
            }
            foreach (var table in modelDoc.Tables)
            {
                foreach (var dep in table.FormattedExpressionAndDependency?.Dependency ?? [])
                {
                    usedFieldsInCalc_Tbl_Col_Measures.Add(ModelFieldFactory.CreateFromDependency(dep));
                }
            }

            // BFS on usedFieldsInVis_Role_Rel_IncRef_sortByCol_hier
            HashSet<ModelField> visited = [];
            foreach (var field in usedFieldsInVis_Role_Rel_IncRef_sortByCol_hier)
            {

                Queue<ModelField> fieldQ = [];
                fieldQ.Enqueue(field);

                while (fieldQ.Count > 0)
                {
                    var currentField = fieldQ.Dequeue();
                    if (visited.Contains(currentField)) continue;
                    visited.Add(currentField);
                    MeasureSummary? meas = modelDoc.Measures.Find(x => x.MeasureName == currentField.Name);
                    ColumnSummary? col = modelDoc.Columns.Find(x => x.TableName == currentField.TableName && x.ColumnName == currentField.Name);
                    TableSummary? tabl = modelDoc.Tables.Find(x => x.TableName == currentField.TableName);

                    if (meas != null)
                    {
                        foreach (var dep in meas.FormattedExpressionAndDependency?.Dependency ?? [])
                        {
                            ModelField x = ModelFieldFactory.CreateFromDependency(dep);
                            if (!visited.Contains(x)) fieldQ.Enqueue(x);
                            usedFieldsInUsedThings.Add(x);
                        }
                    }
                    if (col != null)
                    {
                        foreach (var dep in col.FormattedExpressionAndDependency?.Dependency ?? [])
                        {
                            ModelField x = ModelFieldFactory.CreateFromDependency(dep);
                            if (!visited.Contains(x)) fieldQ.Enqueue(x);
                            usedFieldsInUsedThings.Add(x);
                        }
                    }
                    if (tabl != null)
                    {
                        foreach (var dep in tabl.FormattedExpressionAndDependency?.Dependency ?? [])
                        {
                            ModelField x = ModelFieldFactory.CreateFromDependency(dep);
                            if (!visited.Contains(x)) fieldQ.Enqueue(x);
                            usedFieldsInUsedThings.Add(x);
                        }
                    }
                }

            }

            foreach (var x in usedFieldsInUsedThings)
            {
                usedFieldsInVis_Role_Rel_IncRef_sortByCol_hier.Add(x);
            }


            foreach (var meas in modelDoc.Measures)
            {
                if (usedFieldsInVis_Role_Rel_IncRef_sortByCol_hier.Any(x => x.Name == meas.MeasureName)) meas.IsUsed = true;
                else if (usedFieldsInCalc_Tbl_Col_Measures.Any(x => x.Name == meas.MeasureName) == true) meas.IsUsedInUnused = true;
            }

            foreach (var col in modelDoc.Columns)
            {
                if (usedFieldsInVis_Role_Rel_IncRef_sortByCol_hier.Any(x => x.TableName == col.TableName && x.Name == col.ColumnName)) col.IsUsed = true;
                else if (usedFieldsInCalc_Tbl_Col_Measures.Any(x => x.TableName == col.TableName && x.Name == col.ColumnName) == true) col.IsUsedInUnused = true;
            }

            foreach (var tbl in modelDoc.Tables)
            {
                if (usedFieldsInVis_Role_Rel_IncRef_sortByCol_hier.Any(x => x.TableName == tbl.TableName)) tbl.IsUsed = true;
                else if (usedFieldsInCalc_Tbl_Col_Measures.Any(x => x.TableName == tbl.TableName) == true) tbl.IsUsedInUnused = true;
            }

            return modelDoc;
        }
    }
}