using PowerBI_MCP.DTO;
using PowerBI_MCP.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PowerBI_MCP.Service.IssueDataGenerator.Generators.ReportIssueGenerator
{
	public class FormattingInconsistentDecimal : IIssueDataGenerator
	{
		public SingleIssueRuleData GetData(IssueContext issueContext)
		{
			Dictionary<string, string> formatToFields = new();

			try
			{
				foreach (var m in issueContext.ModelDocumentation?.Measures ?? [])
				{
					if ((string.Equals(m.DataType, "Double", StringComparison.OrdinalIgnoreCase)
						|| string.Equals(m.DataType, "Decimal", StringComparison.OrdinalIgnoreCase))
						&& !string.IsNullOrWhiteSpace(m.Format))
					{
						if (formatToFields.ContainsKey(m.Format!))
							formatToFields[m.Format!] += ", Measure - " + m.MeasureName;
						else
							formatToFields[m.Format!] = "Measure - " + m.MeasureName;
					}
				}

				foreach (var c in issueContext.ModelDocumentation?.Columns ?? [])
				{
					if ((string.Equals(c.DataType, "Double", StringComparison.OrdinalIgnoreCase)
						|| string.Equals(c.DataType, "Decimal", StringComparison.OrdinalIgnoreCase))
						&& !string.IsNullOrWhiteSpace(c.Format))
					{
						if (formatToFields.ContainsKey(c.Format!))
							formatToFields[c.Format!] += ", Column - " + c.TableName + "." + c.ColumnName;
						else
							formatToFields[c.Format!] = "Column - " + c.TableName + "." + c.ColumnName;
					}
				}
			}
			catch (Exception ex)
			{
				GlobalHandler.WriteCrashLog(ex.ToString());
			}

			List<object> issues = [];
			if (formatToFields.Count > 1)
			{
				foreach (var kv in formatToFields)
				{
					issues.Add(new Dictionary<string, string> { { "FormatString", kv.Key }, { "AffectedFields", kv.Value } });
				}
			}

			int max = (issueContext.ModelDocumentation?.Measures?.Count ?? 0) + (issueContext.ModelDocumentation?.Columns?.Count ?? 0);
			return new SingleIssueRuleData { Issues = issues, MaxIssuable = max };
		}
	}
}


