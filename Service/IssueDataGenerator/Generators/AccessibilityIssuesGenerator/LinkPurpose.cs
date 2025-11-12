using PowerBI_MCP.DTO;
using PowerBI_MCP.Handlers;

namespace PowerBI_MCP.Service.IssueDataGenerator.Generators.AccessibilityIssuesGenerator
{
    public class LinkPurpose : IIssueDataGenerator
    {

        private (bool isValid, string reason) IsLinkTextDescriptive(string text, object visual)
        {
            // Normalize text for comparison
            var normalizedText = text.Trim().ToLowerInvariant();

            // Common generic phrases to flag
            var genericPhrases = new[]
            {
                "click here", "here", "click this", "go", "read more",
                "more", "link", "this", "learn more", "next", "previous"
            };

            // Check for generic phrases
            if (genericPhrases.Any(phrase => normalizedText == phrase || normalizedText.Contains(phrase)))
            {
                return (false, "uses generic link text");
            }

            // Check minimum length
            if (normalizedText.Length < 3)
            {
                return (false, "text is too short to be descriptive");
            }

            // Check if it's just a URL
            if (Uri.TryCreate(text, UriKind.Absolute, out _))
            {
                return (false, "URL used as link text");
            }

            // Check if it's just numbers/symbols
            if (text.All(c => !char.IsLetter(c)))
            {
                return (false, "text contains no letters");
            }

            // Additional context checks for Power BI
            var visualListRow = visual as VisualSummary;
            var visualType = visualListRow?.VisualType ?? string.Empty;

            // For action buttons, check if they include the action and target
            if (visualType.Equals("button", StringComparison.OrdinalIgnoreCase) &&
                !normalizedText.Split(' ').Any(word => word.Length > 0 && char.IsLetter(word[0])))
            {
                return (false, "button text should describe the action and target");
            }

            return (true, string.Empty);
        }
        public SingleIssueRuleData GetData(IssueContext issueContext)
        {


            List<Dictionary<string, object>> issues = [];
            try
            {
                // Get all visuals from the report layout
                var visuals = issueContext.ReportDocumentation?.VisualList ?? [];
                if (visuals == null) return new() { Issues = issues.Where(x => true).Cast<object>().ToList(), MaxIssuable = 1 };

                // Define visual types that typically contain interactive elements
                var interactiveVisualTypes = new HashSet<string>(
                    new[] { "button", "actionButton", "hyperlink", "action" },
                    StringComparer.OrdinalIgnoreCase);

                // Check each visual for link/button text issues
                foreach (var visual in visuals)
                {
                    try
                    {
                        // Skip hidden visuals or those without a visual type
                        if (visual.IsHidden || string.IsNullOrEmpty(visual.VisualType))
                            continue;

                        string visualType = visual.VisualType.ToLower();

                        // Robust PBIX interactive visual detection
                        if (!(visualType.Contains("button") || visualType.Contains("hyperlink") || visualType.Contains("action") || visualType.Contains("nav") || visualType.Contains("menu")))
                            continue;

                        // Collect all possible text fields for link/button
                        List<string> candidateTexts = new();
                        if (!string.IsNullOrWhiteSpace(visual.ButtonText)) candidateTexts.Add(visual.ButtonText.Trim() ?? "");
                        if (!string.IsNullOrWhiteSpace(visual.VisualTitle)) candidateTexts.Add(visual.VisualTitle.Trim() ?? "");
                        if (!string.IsNullOrWhiteSpace(visual.AltText)) candidateTexts.Add(visual.AltText.Trim() ?? "");
                        if (visual.VisualTexts != null && visual.VisualTexts.Count > 0)
                            candidateTexts.AddRange(visual.VisualTexts.Where(t => !string.IsNullOrWhiteSpace(t.Text)).Select(t => t.Text.Trim()));

                        // Concatenate all candidate texts for validation
                        string textToCheck = string.Join(" ", candidateTexts.Where(t => !string.IsNullOrWhiteSpace(t)));

                        // Skip if no text to check
                        if (string.IsNullOrWhiteSpace(textToCheck))
                        {
                            Dictionary<string, object> issue = [];
                            issue["reportId"] = visual.ReportId ?? "";
                            issue["reportName"] = visual.ReportName ?? "";
                            issue["pageId"] = visual.PageId ?? "";
                            issue["pageName"] = visual.PageName ?? "";
                            issue["visualId"] = visual.VisualId ?? "";
                            issue["visualType"] = visual.VisualType ?? "";
                            issue["visualTitle"] = visual.VisualTitle ?? "";
                            issue["Text"] = "";
                            issue["Issue"] = "Link/button has no text (empty or whitespace)";
                            issue["Recommendation"] = "Provide descriptive text for all interactive elements.";
                            issue["WCAG_REF"] = "2.4.4";
                            issue["error"] = issue["Issue"];
                            issues.Add(issue);
                            continue;
                        }

                        // Check for duplicate link/button text on the same page
                        bool isDuplicate = visuals.Any(v => v != visual && v.PageId == visual.PageId &&
                            (v.ButtonText == textToCheck || v.VisualTitle == textToCheck || (v.VisualTexts != null && v.VisualTexts.Any(t => t.Text == textToCheck))));
                        if (isDuplicate)
                        {
                            Dictionary<string, object> issue = [];
                            issue["reportId"] = visual.ReportId ?? "";
                            issue["reportName"] = visual.ReportName ?? "";
                            issue["pageId"] = visual.PageId ?? "";
                            issue["pageName"] = visual.PageName ?? "";
                            issue["visualId"] = visual.VisualId ?? "";
                            issue["visualType"] = visual.VisualType ?? "";
                            issue["visualTitle"] = visual.VisualTitle ?? "";
                            issue["Text"] = textToCheck;
                            issue["Issue"] = "Duplicate link/button text on the same page";
                            issue["Recommendation"] = "Ensure each interactive element has unique, descriptive text.";
                            issue["WCAG_REF"] = "2.4.4";
                            issue["error"] = issue["Issue"];
                            issues.Add(issue);
                            continue;
                        }

                        // Check if the text is descriptive enough
                        var (isValid, reason) = IsLinkTextDescriptive(textToCheck, visual);
                        if (!isValid)
                        {
                            // Create an issue for non-descriptive link/button text
                            Dictionary<string, object> issue = [];
                            issue["reportId"] = visual.ReportId ?? "";
                            issue["reportName"] = visual.ReportName ?? "";
                            issue["pageId"] = visual.PageId ?? "";
                            issue["pageName"] = visual.PageName ?? "";
                            issue["visualId"] = visual.VisualId ?? "";
                            issue["visualType"] = visual.VisualType ?? "";
                            issue["visualTitle"] = visual.VisualTitle ?? "";
                            issue["Text"] = textToCheck;
                            issue["Issue"] = $"Link/button text is not descriptive enough: {reason}";
                            issue["Recommendation"] = "Use descriptive text that clearly indicates the link's purpose or destination. Avoid generic text like 'click here' or 'read more'.";
                            issue["WCAG_REF"] = "2.4.4";
                            issue["error"] = issue["Issue"];
                            issues.Add(issue);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error processing visual {visual.VisualId}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetLinkPurposeIssuesData: {ex.Message}");
            }
            return new() { Issues = issues.Where(x => true).Cast<object>().ToList(), MaxIssuable = issueContext.ReportDocumentation?.VisualList?.Count };
        }
    }
}