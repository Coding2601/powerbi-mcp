using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using PlatformSpellCheck;
using PowerBI_MCP.Models;
using PowerBI_MCP.Utils;

namespace PowerBI_MCP.Handlers
{
    public static class GlobalHandler
    {
        private static readonly SpellChecker spellChecker = new();
        private static readonly TelemetryConfiguration telemetryClientConfig = new() { ConnectionString = "InstrumentationKey=0eb4989b-1cae-446a-9113-6a98d65ebc67" };
        private static readonly TelemetryClient telemetryClient = new(telemetryClientConfig);
        private static string? applicationVersion;
        private static string? licenseKey;

        public static void WriteCrashLog(string exceptionMessage, string? exceptionCategory = null, bool writeInAppLog = true)
        {
            try
            {
                // Application Insights telemetry
                if (writeInAppLog)
                {
                    var exception = new Exception(exceptionMessage);
                    var exceptionTelemetry = new ExceptionTelemetry(exception) { SeverityLevel = SeverityLevel.Error, Timestamp = DateTimeOffset.Now };
                    exceptionTelemetry.Properties["Context"] = exceptionCategory ?? "Backend Server Error";
                    telemetryClient.Context.Component.Version = applicationVersion;
                    telemetryClient.Context.User.AuthenticatedUserId = licenseKey;
                    telemetryClient.TrackException(exceptionTelemetry);
                }
                // File logging with proper directory checks and creation
                var logDirectory = Path.GetDirectoryName(AppConfig.crashLog);
                if (!string.IsNullOrEmpty(logDirectory) && !Directory.Exists(logDirectory))
                {
                    Directory.CreateDirectory(logDirectory);
                }

                // Use FileStream with FileShare.ReadWrite to allow concurrent access
                using var fileStream = new FileStream(AppConfig.crashLog, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                using var writer = new StreamWriter(fileStream);
                writer.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] An error occurred: ");
                writer.WriteLine($"{exceptionMessage}\n");
            }
            catch (Exception ex)
            {
                // Log the logging error to console at minimum
                Console.WriteLine($"Error writing to crash log: {ex.Message}");
                telemetryClient.TrackException(new Exception("Error writing to crash log", ex));
            }
        }

        public static bool CheckArtifactExistsById(string artifactId)
        {
            var inReport = ArtifactModel.Reports.Any(report =>
                report.ReportId.Equals(artifactId, StringComparison.OrdinalIgnoreCase));
            var inDataset = ArtifactModel.Datasets.Any(dataset =>
                dataset.DatasetId.Equals(artifactId, StringComparison.OrdinalIgnoreCase));
            return inReport || inDataset;
        }
        
        public static bool IsArtifactAvailable(string artifactName, string artifactPath)
        {
            var inReport = ArtifactModel.Reports.Any(report =>
                report.ReportName.Equals(artifactName, StringComparison.OrdinalIgnoreCase) &&
                report.ReportPath.Equals(artifactPath, StringComparison.OrdinalIgnoreCase));
            var inDataset = ArtifactModel.Datasets.Any(dataset =>
                dataset.ServerName.Equals(artifactName, StringComparison.OrdinalIgnoreCase));
            return inReport || inDataset;
        }
    }
}