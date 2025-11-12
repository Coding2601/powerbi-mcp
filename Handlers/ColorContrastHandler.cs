using PowerBI_MCP.Handlers;
using PowerBI_MCP.DTO;

namespace PowerBI_MCP.Handlers
{
    public static class ColorContrastHandler
    {
        private const double DEFAULT_DISTINCTION_THRESHOLD = 1.0;

        public static (int r, int g, int b) HexToRgb(string hex)
        {
            // Remove '#' if present
            hex = hex.Replace("#", "").Trim().Trim('\'');

            // Handle both 3-digit and 6-digit hex codes
            if (hex.Length == 3)
            {
                hex = $"{hex[0]}{hex[0]}{hex[1]}{hex[1]}{hex[2]}{hex[2]}";
            }

            if (hex.Length != 6)
            {
                throw new ArgumentException($"Invalid hex color format: {hex}");
            }

            // Parse the hex values to RGB components
            int bigint = int.Parse(hex, System.Globalization.NumberStyles.HexNumber);
            int r = (bigint >> 16) & 255;
            int g = (bigint >> 8) & 255;
            int b = bigint & 255;

            return (r, g, b);
        }

        public static bool AreColorsDistinctToColorBlindOfXType(string color1, string color2, ColorBlindnessType colorBlindnessType, double threshold = DEFAULT_DISTINCTION_THRESHOLD)
        {
            // Convert hex to RGB
            (int r1, int g1, int b1) = HexToRgb(color1);
            (int r2, int g2, int b2) = HexToRgb(color2);

            // Simulate color blindness perception
            double[] lms1 = SimulateColorBlindness(r1, g1, b1, colorBlindnessType);
            double[] lms2 = SimulateColorBlindness(r2, g2, b2, colorBlindnessType);

            // Calculate distance between colors in LMS space
            double distance = CalculateDistance(lms1, lms2);

            // Return whether the distance meets threshold for distinction 
            return distance > threshold;
        }

        public static Dictionary<ColorBlindnessType, bool> AreColorsDistinctForAllTypesOfColorBlind(string color1, string color2, double threshold = DEFAULT_DISTINCTION_THRESHOLD)
        {
            var results = new Dictionary<ColorBlindnessType, bool>();

            foreach (ColorBlindnessType type in Enum.GetValues(typeof(ColorBlindnessType)))
            {
                try
                {
                    results[type] = AreColorsDistinctToColorBlindOfXType(color1, color2, type, threshold);
                }
                catch (Exception ex)
                {
                    GlobalHandler.WriteCrashLog(ex.ToString());
                }
            }

            return results;
        }

        private static double[] SimulateColorBlindness(int r, int g, int b, ColorBlindnessType colorBlindnessType)
        {
            // Normalize RGB values to 0-1 range
            double normR = r / 255.0;
            double normG = g / 255.0;
            double normB = b / 255.0;

            // First convert RGB to simulated color blindness perception
            switch (colorBlindnessType)
            {
                case ColorBlindnessType.Deuteranopia:
                    // Green-blind
                    (normR, normG) = (0.625 * normR + 0.375 * normG, 0.7 * normG + 0.3 * normB);
                    break;
                case ColorBlindnessType.Protanopia:
                    // Red-blind
                    normR = 0.56667 * normR + 0.43333 * normG;
                    break;
                case ColorBlindnessType.Tritanopia:
                    // Blue-blind
                    (normG, normB) = (0.95 * normG + 0.05 * normB, 0.43333 * normG + 0.56667 * normB);
                    break;
            }

            // Then convert to LMS color space
            double L = 0.31399022 * normR + 0.63951294 * normG + 0.04649755 * normB;
            double M = 0.15537241 * normR + 0.75789446 * normG + 0.08670142 * normB;
            double S = 0.01775239 * normR + 0.10944209 * normG + 0.87256922 * normB;

            return [L, M, S];
        }

        private static double CalculateDistance(double[] lms1, double[] lms2)
        {
            return Math.Sqrt(Math.Pow(lms1[0] - lms2[0], 2) + Math.Pow(lms1[1] - lms2[1], 2) + Math.Pow(lms1[2] - lms2[2], 2));
        }

        public static bool IsForegroundColorAccessibleFromBackground(string foregroundColor, string backgroundColor, double requiredRatio = 3)
        {
            // Convert hex to RGB
            (int r1, int g1, int b1) = HexToRgb(foregroundColor);
            (int r2, int g2, int b2) = HexToRgb(backgroundColor);

            // Calculate luminance
            double luminance1 = CalculateLuminance(r1, g1, b1);
            double luminance2 = CalculateLuminance(r2, g2, b2);

            // Calculate contrast ratio
            double contrastRatio = CalculateContrastRatio(luminance1, luminance2);

            // Return whether the contrast meets accessibility standards
            return contrastRatio >= requiredRatio;
        }

        private static double CalculateLuminance(int r, int g, int b)
        {
            // Convert RGB to sRGB
            double sR = r / 255.0;
            double sG = g / 255.0;
            double sB = b / 255.0;

            // Apply the sRGB gamma transformation
            sR = sR <= 0.03928 ? sR / 12.92 : Math.Pow((sR + 0.055) / 1.055, 2.4);
            sG = sG <= 0.03928 ? sG / 12.92 : Math.Pow((sG + 0.055) / 1.055, 2.4);
            sB = sB <= 0.03928 ? sB / 12.92 : Math.Pow((sB + 0.055) / 1.055, 2.4);

            // Calculate luminance
            return 0.2126 * sR + 0.7152 * sG + 0.0722 * sB;
        }

        private static double CalculateContrastRatio(double luminance1, double luminance2)
        {
            // Determine which luminance is lighter (higher value)
            double lighter = Math.Max(luminance1, luminance2);
            double darker = Math.Min(luminance1, luminance2);

            // Calculate contrast ratio
            return (lighter + 0.05) / (darker + 0.05);
        }

        /// <summary>
        /// Checks if a color combination is safe for color-blind users by avoiding problematic red/green combinations
        /// and ensuring colors are distinguishable for all types of color blindness.
        /// </summary>
        /// <param name="color1">First color in hex format</param>
        /// <param name="color2">Second color in hex format</param>
        /// <returns>True if the color combination is safe for color-blind users</returns>
        public static bool IsColorCombinationSafeForColorBlind(string color1, string color2)
        {
            try
            {
                // Convert hex to RGB
                (int r1, int g1, int b1) = HexToRgb(color1);
                (int r2, int g2, int b2) = HexToRgb(color2);

                // Check if this is a problematic red/green combination
                if (IsRedGreenCombination(r1, g1, b1, r2, g2, b2))
                {
                    return false;
                }

                // Check if colors are distinguishable for all types of color blindness
                var distinctionResults = AreColorsDistinctForAllTypesOfColorBlind(color1, color2);

                // All color blindness types should be able to distinguish the colors
                return distinctionResults.Values.All(isDistinct => isDistinct);
            }
            catch (Exception ex)
            {
                GlobalHandler.WriteCrashLog(ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// Checks if two colors form a problematic red/green combination that's difficult for color-blind users
        /// </summary>
        /// <param name="r1">Red component of first color</param>
        /// <param name="g1">Green component of first color</param>
        /// <param name="b1">Blue component of first color</param>
        /// <param name="r2">Red component of second color</param>
        /// <param name="g2">Green component of second color</param>
        /// <param name="b2">Blue component of second color</param>
        /// <returns>True if this is a problematic red/green combination</returns>
        private static bool IsRedGreenCombination(int r1, int g1, int b1, int r2, int g2, int b2)
        {
            // Define what constitutes "red" and "green" colors
            // Red: high red, low green and blue
            // Green: high green, low red and blue
            bool isRed1 = r1 > 150 && g1 < 100 && b1 < 100;
            bool isGreen1 = g1 > 150 && r1 < 100 && b1 < 100;
            bool isRed2 = r2 > 150 && g2 < 100 && b2 < 100;
            bool isGreen2 = g2 > 150 && r2 < 100 && b2 < 100;

            // Check for red/green combinations
            return (isRed1 && isGreen2) || (isGreen1 && isRed2);
        }

        /// <summary>
        /// Gets a list of color-blind-safe color palettes that can be used as alternatives
        /// </summary>
        /// <returns>List of color-blind-safe color combinations</returns>
        public static List<string[]> GetColorBlindSafePalettes()
        {
            return new List<string[]>
            {
                // Blue-Orange palette (very distinguishable)
                new string[] { "#1f77b4", "#ff7f0e" },
                // Blue-Yellow palette
                new string[] { "#1f77b4", "#ffd700" },
                // Purple-Orange palette
                new string[] { "#9467bd", "#ff7f0e" },
                // Purple-Yellow palette
                new string[] { "#9467bd", "#ffd700" },
                // Blue-Green palette (careful with this one)
                new string[] { "#1f77b4", "#2ca02c" },
                // Orange-Green palette
                new string[] { "#ff7f0e", "#2ca02c" },
                // Blue-Purple palette
                new string[] { "#1f77b4", "#9467bd" },
                // Orange-Purple palette
                new string[] { "#ff7f0e", "#9467bd" }
            };
        }
    }
}
