using PowerBI_MCP.DTO;
using PowerBI_MCP.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PowerBI_MCP.Service.IssueDataGenerator.Generators.ReportIssueGenerator
{
	public class FormattingInconsistentDateTime : IIssueDataGenerator
	{
		public SingleIssueRuleData GetData(IssueContext issueContext)
		{
			Dictionary<string, string> formatToFields = new();
			try
			{
				foreach (var m in issueContext.ModelDocumentation?.Measures ?? [])
				{
					var dt = m.DataType ?? string.Empty;
					if (!string.IsNullOrWhiteSpace(m.Format) && dt.Contains("date", StringComparison.OrdinalIgnoreCase))
					{
						if (formatToFields.ContainsKey(m.Format!))
							formatToFields[m.Format!] += ", Measure - " + m.MeasureName;
						else
							formatToFields[m.Format!] = "Measure - " + m.MeasureName;
					}
				}

				foreach (var c in issueContext.ModelDocumentation?.Columns ?? [])
				{
					var dt = c.DataType ?? string.Empty;
					if (!string.IsNullOrWhiteSpace(c.Format) && dt.Contains("date", StringComparison.OrdinalIgnoreCase))
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


