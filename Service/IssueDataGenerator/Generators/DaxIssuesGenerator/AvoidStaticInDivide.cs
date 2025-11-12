using PowerBI_MCP.DTO;
using System.Text.RegularExpressions;

namespace PowerBI_MCP.Service.IssueDataGenerator.Generators.DaxIssuesGenerator
{
    public class AvoidStaticInDivide : IIssueDataGenerator
    {
        public SingleIssueRuleData GetData(IssueContext issueContext)
        {
            var measures = issueContext.ModelDocumentation.Measures;
            var measureUseStaticValAsDivideFallback = measures.Where(meas =>
            {
                if (meas.Expression != null && meas.Expression.Trim().Contains("DIVIDE("))
                {
                    string input = meas.Expression.Trim().ToUpper();
                    string pattern = @"DIVIDE\([^,]+,[^,]+,([^)]+)\)";
                    Match match = Regex.Match(input, pattern);
                    if (match.Success)
                    {
                        string parameter = match.Groups[1].Value.Trim();
                        parameter = parameter.Trim('\'', '\"');
                        if (parameter.ToUpper() != "BLANK(")
                            return true;
                    }
                }
                return false;
            }).Cast<object>().ToList();

            return new SingleIssueRuleData()
            {
                Issues = measureUseStaticValAsDivideFallback ?? [],
                MaxIssuable = measures.Count
            };
        }
    }
}