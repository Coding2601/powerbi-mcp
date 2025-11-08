using Microsoft.AnalysisServices.Tabular;
using PowerBI_MCP.DTO;
using PowerBI_MCP.Models;
using PowerBI_MCP.Handlers;

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
    }
}