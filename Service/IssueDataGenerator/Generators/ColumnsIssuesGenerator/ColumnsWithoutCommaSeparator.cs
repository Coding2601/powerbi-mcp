using PowerBI_MCP.DTO;
using PowerBI_MCP.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PowerBI_MCP.Service.IssueDataGenerator.Generators.ColumnsIssuesGenerator
{
	public class ColumnsWithoutCommaSeparator : IIssueDataGenerator
	{
		public SingleIssueRuleData GetData(IssueContext issueContext)
		{
			var columns = issueContext.ModelDocumentation?.Columns ?? [];
			try
			{
				var numericColumnsWithoutComma = columns
					.Where(col => (string.Equals(col.DataType, "Decimal", StringComparison.OrdinalIgnoreCase)
						|| string.Equals(col.DataType, "Double", StringComparison.OrdinalIgnoreCase)
						|| string.Equals(col.DataType, "Int64", StringComparison.OrdinalIgnoreCase))
						&& (string.IsNullOrWhiteSpace(col.Format) || !col.Format!.Contains(",")))
					.Cast<object>()
					.ToList();

				return new SingleIssueRuleData
				{
					Issues = numericColumnsWithoutComma,
					MaxIssuable = columns.Count
				};
			}
			catch (Exception ex)
			{
				GlobalHandler.WriteCrashLog(ex.ToString());
				return new SingleIssueRuleData { Issues = [], MaxIssuable = columns.Count };
			}
		}
	}
}


