using PowerBI_MCP.DTO;
using PowerBI_MCP.Handlers;

namespace PowerBI_MCP.Service.IssueDataGenerator.Generators.AccessibilityIssuesGenerator
{
    public class VisualTextSpacing : IIssueDataGenerator
    {
       private double lineHeightMultiplier = 1.5;
        private double paragraphSpacingMultiplier = 2.0;
        private double letterSpacingMultiplier = 0.12;
        private double wordSpacingMultiplier = 0.16;
        /// <summary>
        /// Estimates the width of the text with increased letter and word spacing.
        /// This is a rough heuristic and does not account for font family, style, or CJK scripts.
        /// </summary>
        private double EstimateTextWidth(string text, double fontSize, double letterSpacing, double wordSpacing)
        {
            // --- Why: To simulate the effect of increased spacing on text width ---
            // This is a rough estimate: average character width is ~0.6 * fontSize
            // Limitation: Does not account for proportional fonts, bold/italic, or glyph metrics.
            int wordCount = 1, charCount = 0;
            try
            {
                wordCount = text.Split(' ').Length;
                charCount = text.Length;
            }
            catch (Exception ex)
            {
                GlobalHandler.WriteCrashLog(ex.ToString());
            }
            double baseWidth = charCount * fontSize * 0.6;
            double extraLetter = (charCount - wordCount) * letterSpacing;
            double extraWord = (wordCount - 1) * wordSpacing;
            return baseWidth + extraLetter + extraWord;
        }

        /// <summary>
        /// Estimates the height of the text with increased line and paragraph spacing.
        /// This is a rough heuristic and does not simulate word-wrapping.
        /// </summary>
        private double EstimateTextHeight(string text, double fontSize, double lineHeight, double paragraphSpacing, double containerWidth)
        {
            // --- Why: To simulate the effect of increased spacing on text height ---
            // Limitation: Does not account for word-wrapping in fixed-width containers.
            int lineCount = 1, paraCount = 1;
            try
            {
                lineCount = text.Split('\n').Length;
                paraCount = text.Split(new[] { "\n\n" }, StringSplitOptions.None).Length;
            }
            catch (Exception ex)
            {
                GlobalHandler.WriteCrashLog(ex.ToString());
            }
            return (lineCount * lineHeight) + ((paraCount - 1) * paragraphSpacing);
        }

        private void CheckTextSpacingCompliance(
         VisualSummary visual,
         string text,
         string textType,
         List<object> issues, 
         string? containerWidth,
         string? containerHeight,
         Dictionary<string, string>? formatting,
         double lineHeightMultiplier,
         double paragraphSpacingMultiplier,
         double letterSpacingMultiplier,
         double wordSpacingMultiplier)
        {
            // --- Why: WCAG 1.4.12 requires text to remain readable and not overflow/clip with increased spacing ---
            double fontSize = 16; // default px
            bool usedDefaultFontSize = true;
            try
            {
                if (formatting != null && formatting.TryGetValue("FontSize", out var fontSizeStr))
                {
                    if (double.TryParse(fontSizeStr, out var parsedFontSize))
                    {
                        fontSize = parsedFontSize;
                        usedDefaultFontSize = false;
                    }
                }
            }
            catch (Exception ex)
            {
                GlobalHandler.WriteCrashLog(ex.ToString());
            }

            // Estimate container width/height
            double width = 200, height = 40; // defaults
            bool usedDefaultWidth = true, usedDefaultHeight = true;
            try
            {
                if (!string.IsNullOrEmpty(containerWidth) && double.TryParse(containerWidth, out var parsedWidth))
                {
                    width = parsedWidth;
                    usedDefaultWidth = false;
                }
                if (!string.IsNullOrEmpty(containerHeight) && double.TryParse(containerHeight, out var parsedHeight))
                {
                    height = parsedHeight;
                    usedDefaultHeight = false;
                }
            }
            catch (Exception ex)
            {
                GlobalHandler.WriteCrashLog(ex.ToString());
            }

            // Simulate increased spacing
            double lineHeight = fontSize * lineHeightMultiplier;
            double paragraphSpacing = fontSize * paragraphSpacingMultiplier;
            double letterSpacing = fontSize * letterSpacingMultiplier;
            double wordSpacing = fontSize * wordSpacingMultiplier;

            // Estimate text width/height with increased spacing
            double estimatedWidth = 0, estimatedHeight = 0;
            bool widthError = false, heightError = false;
            try { estimatedWidth = EstimateTextWidth(text, fontSize, letterSpacing, wordSpacing); } catch { widthError = true; }
            try { estimatedHeight = EstimateTextHeight(text, fontSize, lineHeight, paragraphSpacing, width); } catch { heightError = true; }

            // If estimated width/height exceeds container, flag as issue
            if ((estimatedWidth > width && !widthError) || (estimatedHeight > height && !heightError))
            {
                Dictionary<string, object> errorDict = [];
                errorDict["reportId"] = visual.ReportId ?? "";
                errorDict["reportName"] = visual.ReportName ?? "";
                errorDict["pageId"] = visual.PageId ?? "";
                errorDict["pageName"] = visual.PageName ?? "";
                errorDict["visualId"] = visual.VisualId ?? "";
                errorDict["visualType"] = visual.VisualType ?? "";
                errorDict["visualTitle"] = visual.VisualTitle ?? "";
                errorDict.Add("error", $"{textType} may overflow or be clipped with increased spacing (WCAG 1.4.12). Text: '{text}'");
                errorDict.Add("textType", textType);
                errorDict.Add("estimatedWidth", estimatedWidth);
                errorDict.Add("containerWidth", width);
                errorDict.Add("estimatedHeight", estimatedHeight);
                errorDict.Add("containerHeight", height);
                errorDict.Add("fontSize", fontSize);
                errorDict.Add("usedDefaultFontSize", usedDefaultFontSize);
                errorDict.Add("usedDefaultWidth", usedDefaultWidth);
                errorDict.Add("usedDefaultHeight", usedDefaultHeight);
                errorDict.Add("widthEstimationError", widthError);
                errorDict.Add("heightEstimationError", heightError);
                errorDict.Add("note", "Width/height estimation is approximate. Word-wrapping is not simulated. False positives/negatives possible. See documentation.");
                issues.Add(errorDict);
            }
            // If parsing failed, log as low confidence
            else if (widthError || heightError)
            {
                Dictionary<string, object> errorDict = [];
                errorDict["reportId"] = visual.ReportId ?? "";
                errorDict["reportName"] = visual.ReportName ?? "";
                errorDict["pageId"] = visual.PageId ?? "";
                errorDict["pageName"] = visual.PageName ?? "";
                errorDict["visualId"] = visual.VisualId ?? "";
                errorDict["visualType"] = visual.VisualType ?? "";
                errorDict["visualTitle"] = visual.VisualTitle ?? "";
                errorDict.Add("warning", $"Could not estimate {textType} width/height for spacing check due to parsing error. Text: '{text}'");
                errorDict.Add("textType", textType);
                errorDict.Add("fontSize", fontSize);
                errorDict.Add("usedDefaultFontSize", usedDefaultFontSize);
                errorDict.Add("usedDefaultWidth", usedDefaultWidth);
                errorDict.Add("usedDefaultHeight", usedDefaultHeight);
                errorDict.Add("widthEstimationError", widthError);
                errorDict.Add("heightEstimationError", heightError);
                issues.Add(errorDict);
            }
        }
        public SingleIssueRuleData GetData(IssueContext issueContext)
        {
            List<object> issueData = [];
            var issues = new List<object>();
            // For each visual in the report
            foreach (var visual in issueContext.ReportDocumentation?.VisualList ?? [])
            {
                // --- 1. Visual Title ---
                if (!string.IsNullOrEmpty(visual.VisualTitle))
                {
                    CheckTextSpacingCompliance(visual, visual.VisualTitle, "VisualTitle", issues, 
                        visual.Width, visual.Height, visual.DataLabelFormatting,
                        lineHeightMultiplier, paragraphSpacingMultiplier, letterSpacingMultiplier, wordSpacingMultiplier);
                }

                // --- 2. Button Text ---
                if (!string.IsNullOrEmpty(visual.ButtonText))
                {
                    CheckTextSpacingCompliance(visual, visual.ButtonText, "ButtonText", issues, 
                        visual.Width, visual.Height, visual.DataLabelFormatting,
                        lineHeightMultiplier, paragraphSpacingMultiplier, letterSpacingMultiplier, wordSpacingMultiplier);
                }

                // --- 3. Alt Text ---
                // Alt text is not visible, so we skip spacing checks but log as N/A for transparency
                if (!string.IsNullOrEmpty(visual.AltText))
                {
                    Dictionary<string, object> errorDict = [];
                    errorDict["reportId"] = visual.ReportId ?? "";
                    errorDict["reportName"] = visual.ReportName ?? "";
                    errorDict["pageId"] = visual.PageId ?? "";
                    errorDict["pageName"] = visual.PageName ?? "";
                    errorDict["visualId"] = visual.VisualId ?? "";
                    errorDict["visualType"] = visual.VisualType ?? "";
                    errorDict["visualTitle"] = visual.VisualTitle ?? "";
                    errorDict.Add("note", "AltText is not visible on screen; WCAG 1.4.12 does not apply. Marked as N/A.");
                    errorDict.Add("textType", "AltText");
                    errorDict.Add("text", visual.AltText);
                    issues.Add(errorDict);
                }

                // --- 4. Tooltip Text ---
                if (visual.TooltipShow && visual.TooltipFormatting != null && visual.TooltipFormatting.TryGetValue("Text", out var tooltipText) && !string.IsNullOrEmpty(tooltipText))
                {
                    CheckTextSpacingCompliance(visual, tooltipText, "TooltipText", issues,  
                        visual.Width, visual.Height, visual.TooltipFormatting,
                        lineHeightMultiplier, paragraphSpacingMultiplier, letterSpacingMultiplier, wordSpacingMultiplier);
                }

                // --- 5. Data Labels ---
                if (visual.DataLabelFormatting != null && visual.DataLabelFormatting.TryGetValue("Text", out var dataLabelText) && !string.IsNullOrEmpty(dataLabelText))
                {
                    CheckTextSpacingCompliance(visual, dataLabelText, "DataLabel", issues, 
                        visual.Width, visual.Height, visual.DataLabelFormatting,
                        lineHeightMultiplier, paragraphSpacingMultiplier, letterSpacingMultiplier, wordSpacingMultiplier);
                }

                // --- 6. Custom Texts (VisualTexts) ---
                if (visual.VisualTexts != null)
                {
                    foreach (var textModel in visual.VisualTexts)
                    {
                        if (!string.IsNullOrEmpty(textModel.Text))
                        {
                            CheckTextSpacingCompliance(visual, textModel.Text, $"VisualTexts:{textModel.Type}", issues,  
                                visual.Width, visual.Height, visual.DataLabelFormatting,
                                lineHeightMultiplier, paragraphSpacingMultiplier, letterSpacingMultiplier, wordSpacingMultiplier);
                        }
                    }
                }
            }
            return new() { Issues = issueData ?? [], MaxIssuable = issueContext.ReportDocumentation?.VisualList?.Count };
        }
    }
}