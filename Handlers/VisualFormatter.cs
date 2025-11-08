using Newtonsoft.Json.Linq;
using PowerBI_MCP.DTO;
using PowerBI_MCP.Handlers;
// using PowerBI_MCP.Entities;

namespace PowerBI_MCP.Handlers
{
    /// <summary>
    /// Provides methods to extract and format visual properties from visual configuration JSON.
    /// </summary>
    public class VisualFormatter
    {
        /// <summary>
        /// Gets a dictionary of all visual formatting properties for a visual.
        /// </summary>
        /// <param name="config">Visual configuration JSON.</param>
        /// <param name="dataColors">List of theme data colors.</param>
        /// <param name="isPBIR">True if PBIR format, false for PBIX.</param>
        /// <returns>Dictionary of formatting sections and their properties.</returns>
        public static Dictionary<string, Dictionary<string, string>> GetVisualFormatting(JObject config, List<string> dataColors, bool isPBIR = false)
        {
            var visualProperties = new Dictionary<string, Dictionary<string, string>>
            {
                { "WithoutTitleFormatting", GetWithoutTitleFormatting(config, dataColors, isPBIR) },
                { "DataLabelFormatting", GetDataLabelFormatting(config, dataColors, isPBIR) },
                { "BackgroundFormatting", GetBackgroundFormatting(config, dataColors, isPBIR) },
                { "BorderFormatting", GetBorderFormatting(config, dataColors, isPBIR) },
                { "LegendFormatting", GetLegendFormatting(config) },
                { "TooltipFormatting", GetTooltipFormatting(config) },
            };

            return visualProperties;
        }

        /// <summary>
        /// Extracts border formatting properties from the visual configuration.
        /// </summary>
        /// <param name="config">Visual configuration JSON.</param>
        /// <param name="dataColors">List of theme data colors.</param>
        /// <param name="isPBIR">True if PBIR format, false for PBIX.</param>
        /// <returns>Dictionary of border formatting properties.</returns>
        public static Dictionary<string, string> GetBorderFormatting(JObject config, List<string> dataColors, bool isPBIR = false)
        {
            var configSingleVisual = isPBIR ? config["visual"] : config["singleVisual"];
            string vcObjectsKey = isPBIR ? "visualContainerObjects" : "vcObjects";
            var formattingProperties = new Dictionary<string, string>();
            string Visibility = "Default",
                Thickness = "Default",
                Color = "Default",
                Radius = "Default";

            // Extract border visibility
            if (configSingleVisual?[vcObjectsKey]?["border"]?[0]?["properties"]?["show"]?["expr"]?["Literal"]?["Value"] != null)
            {
                Visibility = configSingleVisual?[vcObjectsKey]?["border"]?[0]?["properties"]?["show"]?["expr"]?["Literal"]?["Value"]?.ToString() ?? "";
            }
            // Extract border thickness
            if (configSingleVisual?[vcObjectsKey]?["border"]?[0]?["properties"]?["width"]?["expr"]?["Literal"]?["Value"] != null)
            {
                Thickness = configSingleVisual?[vcObjectsKey]?["border"]?[0]?["properties"]?["width"]?["expr"]?["Literal"]?["Value"]?.ToString() ?? "Default";
            }

            if (configSingleVisual?[vcObjectsKey]?["border"]?[0]?["properties"]?["color"]?["solid"]?["color"]?["expr"]?["Literal"]?["Value"] != null)
            {
                Color = configSingleVisual?[vcObjectsKey]?["border"]?[0]?["properties"]?["color"]?["solid"]?["color"]?["expr"]?["Literal"]?["Value"]?.ToString() ?? "Default";
            }
            else if (configSingleVisual?[vcObjectsKey]?["border"]?[0]?["properties"]?["color"]?["solid"]?["color"]?["expr"]?["ThemeDataColor"] != null)
            {
                JObject themeDataColor = (JObject)(configSingleVisual?[vcObjectsKey]?["border"]?[0]?["properties"]?["color"]?["solid"]?["color"]?["expr"]?["ThemeDataColor"] ?? "{}");
                Color = GlobalHandler.GetThemeDataColorToHex(themeDataColor, dataColors) ?? "Default";
            }
            // Extract border radius
            if (configSingleVisual?[vcObjectsKey]?["border"]?[0]?["properties"]?["radius"]?["expr"]?["Literal"]?["Value"] != null)
            {
                Radius = configSingleVisual?[vcObjectsKey]?["border"]?[0]?["properties"]?["radius"]?["expr"]?["Literal"]?["Value"]?.ToString() ?? "Default";
            }

            formattingProperties.Add("Visibility", Visibility);
            formattingProperties.Add("Thickness", Thickness);
            formattingProperties.Add("Color", Color);
            formattingProperties.Add("Radius", Radius);

            return formattingProperties;
        }

        /// <summary>
        /// Extracts legend formatting properties from the visual configuration.
        /// </summary>
        /// <param name="config">Visual configuration JSON.</param>
        /// <param name="isPBIR">True if PBIR format, false for PBIX.</param>
        /// <returns>Dictionary of legend formatting properties.</returns>
        public static Dictionary<string, string> GetLegendFormatting(JObject config, bool isPBIR = false)
        {
            var formattingProperties = new Dictionary<string, string>();
            var configSingleVisual = isPBIR ? config["visual"] : config["singleVisual"];
            string vcObjectsKey = isPBIR ? "visualContainerObjects" : "vcObjects";
            var legend = configSingleVisual?[vcObjectsKey]?["legend"]?[0]?["properties"];

            // // Legend Visibility
            // formattingProperties.Add(GetPropertyValue(legend?["show"]?["expr"]?["Literal"]?["Value"]));

            // Consistent Position
            string? position = GetPropertyValue(legend?["position"]?["expr"]?["Literal"]?["Value"] ?? new JObject());
            formattingProperties.Add("position", position);

            // Without Consistent Position
            // var positionDifferent = legend?["positionDifferent"] != null ? legend["positionDifferent"].ToString() : "Default";
            var positionDifferent = legend?["positionDifferent"]?.ToString() ?? "Default";

            formattingProperties.Add("positionDifferent", positionDifferent);

            // // Text Color
            // formattingProperties.Add(GetPropertyValue(legend?["textColor"]?["solid"]?["color"]?["expr"]?["Literal"]?["Value"]));

            // Without Same Text Color
            var textColorDifferent = legend?["textColorDifferent"]?.ToString() ?? "Default";
            formattingProperties.Add("textColorDifferent", textColorDifferent);

            return formattingProperties;
        }

        /// <summary>
        /// Extracts tooltip formatting properties from the visual configuration.
        /// </summary>
        /// <param name="config">Visual configuration JSON.</param>
        /// <param name="isPBIR">True if PBIR format, false for PBIX.</param>
        /// <returns>Dictionary of tooltip formatting properties.</returns>
        public static Dictionary<string, string> GetTooltipFormatting(JObject config, bool isPBIR = false)
        {
            var formattingProperties = new Dictionary<string, string>();
            var configSingleVisual = isPBIR ? config["visual"] : config["singleVisual"];
            string vcObjectsKey = isPBIR ? "visualContainerObjects" : "vcObjects";
            // Safely navigate to the tooltip properties
            var tooltip = configSingleVisual?[vcObjectsKey]?["tooltips"]?[0]?["properties"];

            // Without Background
            var backgroundVisible = tooltip?["background"]?["show"]?["expr"]?["Literal"]?["Value"] != null ? "No Background" : "Default";
            formattingProperties.Add("background", backgroundVisible);

            // Value color
            var valueColor = GetPropertyValue(tooltip?["valueColor"]?["solid"]?["color"]?["expr"]?["Literal"]?["Value"] ?? new JObject());
            formattingProperties.Add("valueColor", valueColor);

            // Label color
            var labelColor = GetPropertyValue(tooltip?["labelColor"]?["solid"]?["color"]?["expr"]?["Literal"]?["Value"] ?? new JObject());
            formattingProperties.Add("labelColor", labelColor);

            // Font color
            var fontColor = GetPropertyValue(tooltip?["fontColor"]?["solid"]?["color"]?["expr"]?["Literal"]?["Value"] ?? new JObject());
            formattingProperties.Add("fontColor", fontColor);

            return formattingProperties;
        }

        /// <summary>
        /// Extracts title formatting properties (without the title text itself) from the visual configuration.
        /// </summary>
        /// <param name="config">Visual configuration JSON.</param>
        /// <param name="dataColors">List of theme data colors.</param>
        /// <param name="isPBIR">True if PBIR format, false for PBIX.</param>
        /// <returns>Dictionary of title formatting properties.</returns>
        public static Dictionary<string, string> GetWithoutTitleFormatting(JObject config, List<string> dataColors, bool isPBIR = false)
        {
            var formattingProperties = new Dictionary<string, string>();

            string Alignment = "Default",
                FontSize = "Default",
                FontFamily = "Default",
                FontColor = "Default",
                bgColor = "Default";
            var configSingleVisual = isPBIR ? config["visual"] : config["singleVisual"];
            string vcObjectsKey = isPBIR ? "visualContainerObjects" : "vcObjects";

            // Extract alignment
            if (configSingleVisual?[vcObjectsKey]?["title"]?[0]?["properties"]?["alignment"]?["expr"]?["Literal"]?["Value"] != null)
            {
                Alignment = configSingleVisual?[vcObjectsKey]?["title"]?[0]?["properties"]?["alignment"]?["expr"]?["Literal"]?["Value"]?.ToString() ?? "Default";
            }
            // Extract font size
            if (configSingleVisual?[vcObjectsKey]?["title"]?[0]?["properties"]?["fontSize"]?["expr"]?["Literal"]?["Value"] != null)
            {
                FontSize = configSingleVisual?[vcObjectsKey]?["title"]?[0]?["properties"]?["fontSize"]?["expr"]?["Literal"]?["Value"]?.ToString() ?? "Default";
            }
            // Extract font family
            if (configSingleVisual?[vcObjectsKey]?["title"]?[0]?["properties"]?["fontFamily"]?["expr"]?["Literal"]?["Value"] != null)
            {
                FontFamily = configSingleVisual?[vcObjectsKey]?["title"]?[0]?["properties"]?["fontFamily"]?["expr"]?["Literal"]?["Value"]?.ToString() ?? "Default";
            }
            // Extract font color (literal or theme)
            if (configSingleVisual?[vcObjectsKey]?["title"]?[0]?["properties"]?["fontColor"]?["solid"]?["color"]?["expr"]?["Literal"]?["Value"] != null)
            {
                FontColor = configSingleVisual?[vcObjectsKey]?["title"]?[0]?["properties"]?["fontColor"]?["solid"]?["color"]?["expr"]?["Literal"]?["Value"]?.ToString() ?? "Default";
            }
            else if (configSingleVisual?[vcObjectsKey]?["title"]?[0]?["properties"]?["fontColor"]?["solid"]?["color"]?["expr"]?["ThemeDataColor"] != null)
            {
                JObject themeDataColor = (JObject)(configSingleVisual?[vcObjectsKey]?["title"]?[0]?["properties"]?["fontColor"]?["solid"]?["color"]?["expr"]?["ThemeDataColor"] ?? "{}");
                FontColor = GlobalHandler.GetThemeDataColorToHex(themeDataColor, dataColors) ?? "Default";
            }

             // Extract bg color (literal or theme)
            if (configSingleVisual?[vcObjectsKey]?["title"]?[0]?["properties"]?["background"]?["solid"]?["color"]?["expr"]?["Literal"]?["Value"] != null)
            {
                bgColor = configSingleVisual?[vcObjectsKey]?["title"]?[0]?["properties"]?["background"]?["solid"]?["color"]?["expr"]?["Literal"]?["Value"]?.ToString() ?? "Default";
            }
            else if (configSingleVisual?[vcObjectsKey]?["title"]?[0]?["properties"]?["background"]?["solid"]?["color"]?["expr"]?["ThemeDataColor"] != null)
            {
                JObject themeDataColor = (JObject)(configSingleVisual?[vcObjectsKey]?["title"]?[0]?["properties"]?["background"]?["solid"]?["color"]?["expr"]?["ThemeDataColor"] ?? "{}");
                bgColor = GlobalHandler.GetThemeDataColorToHex(themeDataColor, dataColors) ?? "Default";
            }

            formattingProperties.Add("Alignment", Alignment);
            formattingProperties.Add("FontSize", FontSize);
            formattingProperties.Add("FontFamily", FontFamily);
            formattingProperties.Add("FontColor", FontColor);
            formattingProperties.Add("BackgroundColor", bgColor);

            return formattingProperties;
        }

        /// <summary>
        /// Extracts data label formatting properties from the visual configuration.
        /// </summary>
        /// <param name="config">Visual configuration JSON.</param>
        /// <param name="dataColors">List of theme data colors.</param>
        /// <param name="isPBIR">True if PBIR format, false for PBIX.</param>
        /// <returns>Dictionary of data label formatting properties.</returns>
        public static Dictionary<string, string> GetDataLabelFormatting(JObject config, List<string> dataColors, bool isPBIR = false)
        {
            var configSingleVisual = isPBIR ? config["visual"] : config["singleVisual"];
            var formattingProperties = new Dictionary<string, string>();
            string Orientation = "Default",
                FontSize = "Default",
                FontFamily = "Default",
                Visibility = "Default",
                BackgroundColor = "Default",
                BackgroundTransparency = "Default",
                FontColor = "Default";

            // Extract label orientation
            if (configSingleVisual?["objects"]?["labels"]?[0]?["properties"]?["labelOrientation"]?["expr"]?["Literal"]?["Value"] != null)
            {
                Orientation = configSingleVisual?["objects"]?["labels"]?[0]?["properties"]?["labelOrientation"]?["expr"]?["Literal"]?["Value"]?.ToString() ?? "Default";
            }
            // Extract font size
            if (configSingleVisual?["objects"]?["labels"]?[0]?["properties"]?["fontSize"]?["expr"]?["Literal"]?["Value"] != null)
            {
                FontSize = configSingleVisual?["objects"]?["labels"]?[0]?["properties"]?["fontSize"]?["expr"]?["Literal"]?["Value"]?.ToString() ?? "Default";
            }
            // Extract font family
            if (configSingleVisual?["objects"]?["labels"]?[0]?["properties"]?["fontFamily"]?["expr"]?["Literal"]?["Value"] != null)
            {
                FontFamily = configSingleVisual?["objects"]?["labels"]?[0]?["properties"]?["fontFamily"]?["expr"]?["Literal"]?["Value"]?.ToString() ?? "Default";
            }
            // Extract label visibility
            if (configSingleVisual?["objects"]?["labels"]?[0]?["properties"]?["show"]?["expr"]?["Literal"]?["Value"] != null)
            {
                Visibility = configSingleVisual?["objects"]?["labels"]?[0]?["properties"]?["show"]?["expr"]?["Literal"]?["Value"]?.ToString() ?? "Default";
            }

            if (configSingleVisual?["objects"]?["labels"]?[0]?["properties"]?["backgroundColor"]?["solid"]?["color"]?["expr"]?["Literal"]?["Value"] != null)
            {
                BackgroundColor = configSingleVisual?["objects"]?["labels"]?[0]?["properties"]?["backgroundColor"]?["solid"]?["color"]?["expr"]?["Literal"]?["Value"]?.ToString() ?? "Default";
            }
            else if (configSingleVisual?["objects"]?["labels"]?[0]?["properties"]?["backgroundColor"]?["solid"]?["color"]?["expr"]?["ThemeDataColor"] != null)
            {
                JObject themeDataColor = (JObject)(configSingleVisual?["objects"]?["labels"]?[0]?["properties"]?["backgroundColor"]?["solid"]?["color"]?["expr"]?["ThemeDataColor"] ?? "{}");
                BackgroundColor = GlobalHandler.GetThemeDataColorToHex(themeDataColor, dataColors) ?? "Default";
            }
            // Extract background transparency
            if (configSingleVisual?["objects"]?["labels"]?[0]?["properties"]?["transparency"]?["expr"]?["Literal"]?["Value"] != null)
            {
                BackgroundTransparency = configSingleVisual?["objects"]?["labels"]?[0]?["properties"]?["transparency"]?["expr"]?["Literal"]?["Value"]?.ToString() ?? "Default";
            }
            // Extract font color (literal or theme)
            if (configSingleVisual?["objects"]?["labels"]?[0]?["properties"]?["color"]?["solid"]?["color"]?["expr"]?["Literal"]?["Value"] != null)
            {
                FontColor = configSingleVisual?["objects"]?["labels"]?[0]?["properties"]?["color"]?["solid"]?["color"]?["expr"]?["Literal"]?["Value"]?.ToString() ?? "Default";
            }
            else if (configSingleVisual?["objects"]?["labels"]?[0]?["properties"]?["color"]?["solid"]?["color"]?["expr"]?["ThemeDataColor"] != null)
            {
                JObject themeDataColor = (JObject)(configSingleVisual?["objects"]?["labels"]?[0]?["properties"]?["color"]?["solid"]?["color"]?["expr"]?["ThemeDataColor"] ?? "{}");
                FontColor = GlobalHandler.GetThemeDataColorToHex(themeDataColor, dataColors) ?? "Default";
            }

            formattingProperties.Add("Orientation", Orientation);
            formattingProperties.Add("Visibility", Visibility);
            formattingProperties.Add("FontSize", FontSize);
            formattingProperties.Add("FontFamily", FontFamily);
            formattingProperties.Add("FontColor", FontColor);
            formattingProperties.Add("BackgroundColor", BackgroundColor);
            formattingProperties.Add("BackgroundTransparency", BackgroundTransparency);
            return formattingProperties;
        }

        /// <summary>
        /// Extracts background formatting properties from the visual configuration.
        /// </summary>
        /// <param name="config">Visual configuration JSON.</param>
        /// <param name="dataColors">List of theme data colors.</param>
        /// <param name="isPBIR">True if PBIR format, false for PBIX.</param>
        /// <returns>Dictionary of background formatting properties.</returns>
        public static Dictionary<string, string> GetBackgroundFormatting(JObject config, List<string> dataColors, bool isPBIR = false)
        {
            var configSingleVisual = isPBIR ? config["visual"] : config["singleVisual"];
            string vcObjectsKey = isPBIR ? "visualContainerObjects" : "vcObjects";

            var formattingProperties = new Dictionary<string, string>();
            string Color = "Default",
                Transparency = "Default",
                Visibility = "Default";

            // Extract background color (literal or theme)
            if (configSingleVisual?[vcObjectsKey]?["background"]?[0]?["properties"]?["color"]?["solid"]?["color"]?["expr"]?["Literal"]?["Value"] != null)
            {
                Color = configSingleVisual?[vcObjectsKey]?["background"]?[0]?["properties"]?["color"]?["solid"]?["color"]?["expr"]?["Literal"]?["Value"]?.ToString() ?? "";
            }
            else if (configSingleVisual?[vcObjectsKey]?["background"]?[0]?["properties"]?["color"]?["solid"]?["color"]?["expr"]?["ThemeDataColor"] != null)
            {
                JObject themeDataColor = (JObject)(configSingleVisual?[vcObjectsKey]?["background"]?[0]?["properties"]?["color"]?["solid"]?["color"]?["expr"]?["ThemeDataColor"] ?? "{}");
                Color = GlobalHandler.GetThemeDataColorToHex(themeDataColor, dataColors) ?? "Default";
            }
            // Extract background transparency
            if (configSingleVisual?[vcObjectsKey]?["background"]?[0]?["properties"]?["transparency"]?["expr"]?["Literal"]?["Value"] != null)
            {
                Transparency = configSingleVisual?[vcObjectsKey]?["background"]?[0]?["properties"]?["transparency"]?["expr"]?["Literal"]?["Value"]?.ToString() ?? "Default";
            }
            // Extract background visibility
            if (configSingleVisual?[vcObjectsKey]?["background"]?[0]?["properties"]?["show"]?["expr"]?["Literal"]?["Value"] != null)
            {
                Visibility = configSingleVisual?[vcObjectsKey]?["background"]?[0]?["properties"]?["show"]?["expr"]?["Literal"]?["Value"]?.ToString() ?? "Default";
            }
            formattingProperties.Add("Color", Color);
            formattingProperties.Add("Transparency", Transparency);
            formattingProperties.Add("Visibility", Visibility);
            return formattingProperties;
        }

        /// <summary>
        /// Helper method to safely extract a property value from a JToken.
        /// </summary>
        /// <param name="token">JToken to extract value from.</param>
        /// <returns>String value or "Default" if not found.</returns>
        private static string GetPropertyValue(JToken token)
        {
            return token?.ToString() ?? "Default";
        }
    }
}