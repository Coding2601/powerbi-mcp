using PowerBI_MCP.DTO;
using PowerBI_MCP.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PowerBI_MCP.Service.IssueDataGenerator.Generators.MeasuresIssuesGenerator
{
	public class MeasuresWithoutCommaSeparator : IIssueDataGenerator
	{
		public SingleIssueRuleData GetData(IssueContext issueContext)
		{
			var measures = issueContext.ModelDocumentation?.Measures ?? [];
			try
			{
				var numericMeasuresWithoutComma = measures
					.Where(meas => (string.Equals(meas.DataType, "Decimal", StringComparison.OrdinalIgnoreCase)
						|| string.Equals(meas.DataType, "Double", StringComparison.OrdinalIgnoreCase)
						|| string.Equals(meas.DataType, "Int64", StringComparison.OrdinalIgnoreCase))
						&& (string.IsNullOrWhiteSpace(meas.Format) || !meas.Format!.Contains(",")))
					.Cast<object>()
					.ToList();

				return new SingleIssueRuleData
				{
					Issues = numericMeasuresWithoutComma,
					MaxIssuable = measures.Count
				};
			}
			catch (Exception ex)
			{
				GlobalHandler.WriteCrashLog(ex.ToString());
				return new SingleIssueRuleData { Issues = [], MaxIssuable = measures.Count };
			}
		}
	}
}
