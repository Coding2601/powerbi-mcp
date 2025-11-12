namespace PowerBI_MCP.Utils
{
    public static class AppConfig
    {
        public static ConfigurationManager configuration { get; set; } = new ConfigurationManager();
        private static readonly string RoamingDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        public static string LocalDir { get; } = RoamingDir + "\\certy-fast";
        public static string duplicateReportZipDirectory { get; } = RoamingDir + "\\certy-fast\\reportZIP";
        public static string SuppressedInsightsFilesDirectory { get; } = RoamingDir + "\\certy-fast\\suppressedInsights";
        public static string SuppressedKeyJoinStr = "_SUPPRESSED_KEY_JOIN_";
        public static string IssueRowIgnoreKeyJoinSeparator = "__|__Issue_Row_Ignore_Join_Separator__|__";
        public static string ModelBimDirectory { get; } = RoamingDir + "\\certy-fast\\modelBim";
        public static string duplicateReportModelZipDirectory { get; } = RoamingDir + "\\certy-fast\\modelZIP";
        public static string crashLog { get; } = RoamingDir + "\\certy-fast\\CrashLog.txt";
        public static string ConnectionString { get; } = $"{RoamingDir}\\certy-fast\\CertyfastDB.sqlite;Mode=ReadWriteCreate;";
        public static string whizUpdaterDirectory { get; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "certy-fast-updater");
        public static string localCacheDir1 { get; } = RoamingDir + "\\certy-fast\\Cache";
        public static string localCacheDir2 { get; } = RoamingDir + "\\certy-fast\\Code Cache";
        public static string ignoreList { get; } = RoamingDir + "\\certy-fast\\SpellCheckIgnore.txt";

        public static string licenseKey { get; } = RoamingDir + "\\certy-fast\\License.txt";
        public static string WhizUserDataJsonFile { get; } = RoamingDir + "\\certy-fast\\userData.json";
        public static string InsightsMetadataJsonFile { get; } = RoamingDir + "\\certy-fast\\insightsMetadata.json";
        public static string UserRulesKeysForSuppressionJsonFile { get; } = RoamingDir + "\\certy-fast\\keysForSuppression.json";

        public static string CertyFastDBFilePath { get; } = LocalDir + "\\databases\\CertyfastDB.sqlite";
        public static string CertyFASTApplicationPath = @"C:\Program Files\CertyFAST\CertyFAST.exe";

        public static readonly Dictionary<string, HashSet<string>> visualFormatType = new()
        {
            {
                "Table",
                new HashSet<string>() { "tableEx" }
            },
            {
                "Grid",
                new HashSet<string>() { "pivotTable" }
            },
            {
                "Matrix",
                new HashSet<string>() { "pivotTable" }
            },
            {
                "Slicer",
                new HashSet<string>() { "slicer", "advancedSlicerVisual", "listSlicer" }
            },
            {
                "Axis Chart",
                new HashSet<string>()
                {
                    "clusteredBarChart",
                    "columnChart",
                    "lineStackedColumnComboChart",
                    "barChart",
                    "clusteredColumnChart",
                    "hundredPercentStackedBarChart",
                    "hundredPercentStackedColumnChart",
                    "lineClusteredColumnComboChart",
                    "stackedAreaChart",
                    "lineChart",
                    "areaChart",
                    "ribbonChart",
                    "waterfallChart",
                    "scatterChart",
                }
            },
            {
                "Card",
                new HashSet<string>() { "card" }
            },
            {
                "New Card",
                new HashSet<string>() { "cardVisual" }
            },
            {
                "Static Component",
                new HashSet<string>() { "basicShape", "textbox", "actionButton", "image", "shape" }
            },
            {
                "Other",
                new HashSet<string>()
                {
                    "qnaVisual",
                    "pieChart",
                    "multiRowCard",
                    "decompositionTreeVisual",
                    "keyDriversVisual",
                    "esriVisual",
                    "funnel",
                    "donutChart",
                    "map",
                    "filledMap",
                    "gauge",
                    "kpi",
                }
            },
        };

        public static readonly Dictionary<string, string> visualTypeFormatter = new() { { "tableEx", "Table" }, { "pivotTable", "Matrix" } };

        public static class OpenAIUsableTypes
        {
            public static string PUBLIC = "PUBLIC";
            public static string AZURE = "AZURE";
        };

        public static readonly string[] PowerBIViz =
        [
            "basicShape",
            "textbox",
            "actionButton",
            "qnaVisual",
            "image",
            "shape",
            "clusteredBarChart",
            "columnChart",
            "tableEx",
            "card",
            "pieChart",
            "multiRowCard",
            "decompositionTreeVisual",
            "cardVisual",
            "lineStackedColumnComboChart",
            "barChart",
            "clusteredColumnChart",
            "hundredPercentStackedBarChart",
            "hundredPercentStackedColumnChart",
            "lineClusteredColumnComboChart",
            "stackedAreaChart",
            "lineChart",
            "areaChart",
            "ribbonChart",
            "waterfallChart",
            "funnel",
            "scatterChart",
            "donutChart",
            "map",
            "filledMap",
            "gauge",
            "kpi",
            "slicer",
            "pivotTable",
            "keyDriversVisual",
            "esriVisual",
            "listSlicer",
        ];

        public static readonly string InitializeDatabaseScript =
            @"
            CREATE TABLE Source (
                id INTEGER PRIMARY KEY,
                modelName NVARCHAR(255),
                modelType NVARCHAR(10),
                reportName NVARCHAR(255),
                reportType NVARCHAR(10),
                createdAt DATETIME,
                updatedAt DATETIME,
                UNIQUE(ModelName, ModelType, ReportName, ReportType)
                );

            CREATE TABLE AnalysisResult (
                id INTEGER PRIMARY KEY,
                executionId INTEGER,
                sourceId INTEGER,
                insightId NVARCHAR(30),
                impactAreaId INTEGER,
                score FLOAT,
                maxPossibleScore INTEGER,
                passed INTEGER,
                warning INTEGER,
                error INTEGER,
                createdAt DATETIME
                );

            CREATE TABLE ExecutionLog (
                id INTEGER PRIMARY KEY,
                sourceId INTEGER,
                createdAt DATETIME,
                comment NVARCHAR(255)
                );

            CREATE TABLE ScorecardMetadata (
                id NVARCHAR(30) PRIMARY KEY,
                name NVARCHAR(200),
                description TEXT,
                resolutionSteps TEXT,
                referenceLink TEXT,
                tags TEXT,
                impactArea NVARCHAR(100),
                section NVARCHAR(100),
                sectionOrder INTEGER,
                subSectionOrder INTEGER,
                severity INTEGER,
                severityTolerance INTEGER,
                severityToleranceType INTEGER,
                maxScore INTEGER,
                scoringMechanism INTEGER,
                type NVARCHAR(20),
                systemRuleFunctionToGenerateData INTEGER,
                objectCollection NVARCHAR(50),
                objectField NVARCHAR(50),
                regex TEXT,
                columnsToDisplay TEXT,
                visible BOOLEAN,
                editable BOOLEAN,
                dataTableName NVARCHAR(31),
                insightsSource INTEGER,
                noDataMessage TEXT,
                insightAssociation INTEGER,
                createdAt DATETIME,
                updatedAt DATETIME
                );

            CREATE TABLE UserSettings (
                id INTEGER PRIMARY KEY,
                variable NVARCHAR(100) UNIQUE,
                value TEXT,
                createdAt DATETIME,
                updatedAt DATETIME
                );

            CREATE TABLE RulesSuppressionKeys (
                id INTEGER PRIMARY KEY,
                insightId NVARCHAR(30),
                columnName NVARCHAR(100),
                createdAt DATETIME,
                updatedAt DATETIME,
                UNIQUE(InsightId, ColumnName)
                );

            CREATE TABLE IgnoreList (
                id INTEGER PRIMARY KEY,
                sourceId INTEGER,
                insightId NVARCHAR(30),
                insightRowKey TEXT,
                comment TEXT,
                createdAt DATETIME,
                updatedAt DATETIME
                );

            CREATE TABLE ImpactAreaMaster (
                id INTEGER PRIMARY KEY,
                name NVARCHAR(50) UNIQUE,
                highImpact INTEGER,
                lowImpact INTEGER,
                impactColor NVARCHAR(20),
                createdAt DATETIME,
                updatedAt DATETIME
                );";
    }
}
