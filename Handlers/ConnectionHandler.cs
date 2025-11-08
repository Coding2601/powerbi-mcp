using System.Diagnostics;
using System.Management;

namespace PowerBI_MCP.Handlers
{
    public class ConnectionHandler
    {
        public bool CheckValidPBIR(string extractionPath)
        {
            if (extractionPath == null || extractionPath == "" || Directory.Exists(extractionPath) == false)
                return false;
            string definitionDirPath = Path.Combine(extractionPath, "definition");
            string StaticResourcesDirPath = Path.Combine(extractionPath, "StaticResources");
            string ReportDirPath = Path.Combine(extractionPath, "definition", "report.json");
            if (Directory.Exists(definitionDirPath) == false || Directory.Exists(StaticResourcesDirPath) == false || File.Exists(ReportDirPath) == false)
                return false;
            return true;
        }

        public string GetServer(string modelName)
        {
            var processes = Process.GetProcessesByName("PBIDesktop").ToList();
            int processID = 0;
            bool isFound = false;
            foreach (Process p1 in processes)
            {
                try
                {
                    if (p1.MainWindowTitle == modelName)
                    {
                        processID = p1.Id;
                        isFound = true;
                    }
                }
                catch (Exception ex)
                {
                    GlobalHandler.WriteCrashLog(ex.ToString());
                    throw new Exception(ex.ToString());
                }
            }
            if (isFound == false)
            {
                GlobalHandler.WriteCrashLog("The report was not found. Please try closing and reopening the report.", "Report server crashed");
                throw new Exception("The report was not found. Please try closing and reopening the report.");
            }
            return GetServerByPID(processID);
        }

        public string GetServerByPID(int pid)
        {
            int number = 0;
            var queryString = $"SELECT ProcessId FROM Win32_Process WHERE ParentProcessId = {pid} AND Name='msmdsrv.exe'";
            if (OperatingSystem.IsWindows())
            {
                var searcher = new ManagementObjectSearcher(queryString);
                var collection = searcher.Get();
                foreach (var @object in collection)
                {
                    try
                    {
                        if (!(@object is null))
                        {
                            var processId = (int)(uint)@object.GetPropertyValue("ProcessId");
                            number = processId;
                        }
                    }
                    catch (Exception ex)
                    {
                        GlobalHandler.WriteCrashLog(ex.ToString());
                    }
                }
            }
            var ip = System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties();

            Process searcherProcess = new Process();
            searcherProcess.StartInfo.FileName = "cmd";
            searcherProcess.StartInfo.Arguments = "cmd /c \"netstat /ano | findstr " + number.ToString() + "\"";
            searcherProcess.StartInfo.CreateNoWindow = true;
            searcherProcess.StartInfo.RedirectStandardOutput = true;
            searcherProcess.StartInfo.UseShellExecute = false;
            searcherProcess.Start();

            String output = searcherProcess.StandardOutput.ReadToEnd();
            String TcpEndpoint = output.Split(' ')[6];
            String server = "localhost:" + TcpEndpoint.Substring(TcpEndpoint.Length - 5);

            return server;
        }
    }
}